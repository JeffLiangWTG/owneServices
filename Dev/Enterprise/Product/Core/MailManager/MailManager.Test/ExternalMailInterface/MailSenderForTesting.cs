using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class MailSenderForTesting : ISmtpSender
	{
		public MailSenderForTesting(Action<MailItem> sendAction, string server = "", string username = "user")
		{
			this.sendAction = sendAction;
			this.server = server;
			this.username = username;
		}

		public void Send(MailItem mailItem)
		{
			sendAction(mailItem);
		}

		public void Dispose() { }

		readonly Action<MailItem> sendAction;
		readonly string server;
		readonly string username;

		public SmtpConfiguration Configuration
		{
			get => config = config ?? new SmtpConfiguration(server: server, port: 0, "");
			set => config = value;
		}
		SmtpConfiguration config;

		public RejectedRecipientInfo[] GetRejectedRecipients()
		{
			return rejectedRecipients?.ToArray();
		}

		public void SetRejectedRecipients(RejectedRecipientInfo[] recipients)
		{
			rejectedRecipients = recipients;
		}

		public string GetSendingInfo() => $"From: {username} Server: {Configuration.Server}";

		RejectedRecipientInfo[] rejectedRecipients;
	}
}
