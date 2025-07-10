using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using MailKit.Net.Smtp;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class RejectedRecipientInfo
	{
		public string Address { get; set; }
		public int ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
	}

	public interface IMailSender : IDisposable
	{
		void Send(MailItem mailItem);

		string GetSendingInfo();
	}

	public interface ISmtpSender : IMailSender
	{
		RejectedRecipientInfo[] GetRejectedRecipients();
	}

	public interface IMailSenderProvider
	{
		IMailSender GetSender(ILogger logger = null, string from = null);

		ISmtpSender GetSmtpSender(SmtpConfiguration smtpConfiguration, UserPasswordAuthConfiguration userPasswordAuthConfiguration = null, string senderAddress = null, ILogger logger = null);
	}

	public interface ISmtpClientImpl : ISmtpClient
	{
		List<RejectedRecipientInfo> RejectedRecipients { get; }
		void Connect(SmtpConfiguration config);
		void Close();
	}
}
