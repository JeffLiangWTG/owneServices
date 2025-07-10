using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Environment;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class SmtpClientImpl : SmtpClient, ISmtpClientImpl
	{
		public SmtpClientImpl()
		{
		}

		public SmtpClientImpl(IProtocolLogger protocolLogger) : base(protocolLogger)
		{
		}

		public List<RejectedRecipientInfo> RejectedRecipients { get; } = new List<RejectedRecipientInfo>();

		protected override void OnRecipientNotAccepted(MimeMessage message, MailboxAddress mailbox, SmtpResponse response)
		{
			RejectedRecipients.Add(new RejectedRecipientInfo { Address = mailbox.Address, ErrorCode = (int)response.StatusCode, ErrorMessage = response.Response });
		}

		public void Connect(SmtpConfiguration config)
		{
			if (!string.IsNullOrWhiteSpace(config.EhloDomain))
			{
				LocalDomain = config.EhloDomain;
			}

			try
			{
				Connect(config.Server, config.Port, SecureSocketOptionsLookup.FromSecureConnectionTypes(config.SecureConnectionType));
			}
			catch (Exception e) when (e.ShouldReconnect())
			{
				//IOException might be caused by wrong connection type, let's let MailKit decide which connection type is good.
				Connect(config.Server, config.Port);
			}

			Timeout = DataRegistry.Instance.SMTPServerTimeout * 1000;
		}

		public void Close()
		{
			Disconnect(true);
		}
	}
}
