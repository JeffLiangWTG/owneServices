using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Enterprise.MailManager.MailFilters.Testing;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimpleSmtpServer : SimpleMailServer
	{
		public override ushort Port => MailTestHelpers.SmtpPort;

		public struct SimpleEmail
		{
			public string MessageId;
			public string Data;
		}

		static readonly EmailContainer EmailContainer = new EmailContainer();

		public override SimpleMailServerHandler GetMailHandler(TcpClient client)
		{
			return new SimpleSmtpServerHandler(client);
		}

		public override void Greeting(SimpleMailServerHandler handler)
		{
			handler.Write("220 Fake SMTP Server Ready");
		}

		public static void AddOneEmail(string messageId, string data = "Test Data")
		{
			var email = new SimpleEmail();
			email.MessageId = messageId;
			email.Data = data;
			EmailContainer.Add(email);
		}

		public static void AddEmail(string data)
		{
			var email = new SimpleEmail();
			email.MessageId = GetMailMessageId(data);
			if (data.TrimEnd().EndsWith("."))
			{
				data = data.Substring(0, data.TrimEnd().Length - 1);
			}
			email.Data = data;
			EmailContainer.Add(email);
		}

		public static string GetMailMessageId(string data)
		{
			using var memoryStream = new MemoryStream();
			using var writer = new StreamWriter(memoryStream);
			writer.Write(data);
			writer.Flush();
			memoryStream.Position = 0;
			return MimeMessage.Load(memoryStream).MessageId;
		}

		public static (int count, int total) GetEmailCount()
		{
			return EmailContainer.GetEmailCount();
		}

		public static SimpleEmail GetOneEmail(int uid, out bool isError)
		{
			return EmailContainer.GetOneEmail(uid, out isError);
		}

		public static void DeleteEmail(int uid, out bool isError)
		{
			EmailContainer.DeleteEmail(uid, out isError);
		}

		public static Dictionary<int, SimpleEmail> GetEmailDictionary()
		{
			return EmailContainer.GetEmailDictionary();
		}

		public static void ClearAll()
		{
			EmailContainer.ClearAll();
		}
	}
}
