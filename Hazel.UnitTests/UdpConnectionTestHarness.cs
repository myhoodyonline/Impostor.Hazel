using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Impostor.Hazel;
using Impostor.Hazel.Udp;
using Microsoft.Extensions.ObjectPool;

namespace Hazel.UnitTests
{
    internal class UdpConnectionTestHarness : UdpConnection
    {
        public List<MessageReader> BytesSent = new List<MessageReader>();
        public ushort ReliableReceiveLast => this.reliableReceiveLast;

        public UdpConnectionTestHarness(ConnectionListener listener, ObjectPool<MessageReader> readerPool) : base(listener, readerPool)
        {
        }

        public override ValueTask ConnectAsync(byte[] bytes = null, int timeout = 5000)
        {
            this.State = ConnectionState.Connected;
            return default;
        }

        protected override ValueTask<bool> SendDisconnect(MessageWriter writer)
        {
            lock (this)
            {
                if (this.State != ConnectionState.Connected)
                {
                    return ValueTask.FromResult(false);
                }

                this.State = ConnectionState.NotConnected;
            }

            return ValueTask.FromResult(true);
        }

        protected override ValueTask WriteBytesToConnection(byte[] bytes, int length)
        {
            var data = _readerPool.Get();
            data.Update(bytes);
            this.BytesSent.Add(data);
        }

        public void Test_Receive(MessageWriter msg)
        {
            byte[] buffer = new byte[msg.Length];
            Buffer.BlockCopy(msg.Buffer, 0, buffer, 0, msg.Length);

            var data = _readerPool.Get();
            data.Update(buffer);
            this.HandleReceive(data, data.Length);
        }
    }
}
