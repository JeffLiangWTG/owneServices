using System;
using System.Net.Sockets;
using System.Text;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public abstract class SimpleMailServerHandler : ISimpleMailServerHandler
	{
		protected TcpClient client;

		const int ByteLength = 8192;

		public void Handle()
		{
			while (client.Connected)
			{
				try
				{
					var message = Read();

					if (IsEmpty(message))
					{
						break;
					}

					if (IsQuitCommand(message))
					{
						SayGoodBye();
						client.Close();
						break;
					}

					HandleCommand(message);
					AfterCommandHandled();
				}
				catch (Exception)
				{
					break;
				}
			}
		}

		public virtual void SayGoodBye()
		{
		}

		public virtual void AfterCommandHandled()
		{
		}

		public abstract bool IsQuitCommand(string message);

		public abstract void HandleCommand(string message);

		public string Read(int timeout = 0)
		{
			var clientStream = client.GetStream();
			if (timeout > 0)
			{
				clientStream.ReadTimeout = timeout;
			}
			var messageBytes = new byte[ByteLength];
			var encoder = new UTF8Encoding();
			var bytesRead = clientStream.Read(messageBytes, 0, ByteLength);

			var message = encoder.GetString(messageBytes, 0, bytesRead);
			return message;
		}

		public void Write(string message)
		{
			var clientStream = client.GetStream();
			var encoder = new UTF8Encoding();
			var bytesWrite = encoder.GetBytes(message + "\r\n");

			clientStream.Write(bytesWrite, 0, bytesWrite.Length);
			clientStream.Flush();
		}

		public bool IsEmpty(string message)
		{
			return string.IsNullOrWhiteSpace(message);
		}

		public string SplitAndGet(string message, int index, out bool isEmpty)
		{
			var msgSplit = message.Trim().Split(' ');
			isEmpty = msgSplit.Length == 0;

			if (index < 0  || index + 1 > msgSplit.Length)
			{
				return string.Empty;
			}
			return msgSplit[index];
		}

		public string SubFirstWhiteSpaceAndGet(string message)
		{
			var index = message.IndexOf(' ');
			return index < 0 ? message : message.Substring(index).Trim();
		}

		public void Dispose()
		{
			client?.Close();
		}
	}
}
