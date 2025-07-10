using System;
using System.Collections.Generic;
using System.Threading;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class TestSmtpClientImplWithSpecifiedSmtpFrom : SmtpClientImpl
	{
		public TestSmtpClientImplWithSpecifiedSmtpFrom(bool forceThrowSmtpCommandException = false, string senderAddress = null)
		{
			ForceThrowSmtpCommandException = forceThrowSmtpCommandException;
			this.senderAddress = senderAddress;
		}

		internal bool ForceThrowSmtpCommandException { get; }
		internal int SendCalledCount { get; private set; }

		readonly string senderAddress;

		public override string Send(MimeMessage message, CancellationToken cancellationToken = new CancellationToken(), ITransferProgress progress = null)
		{
			ValidateSender(message);

			if (!string.IsNullOrEmpty(senderAddress))
			{
				throw new InvalidOperationException("Should call SmtpClient.Send(message, from, recipients) instead of SmtpClient.Send(message)");
			}

			return base.Send(message, cancellationToken, progress);
		}

		public override string Send(FormatOptions options, MimeMessage message, MailboxAddress sender, IEnumerable<MailboxAddress> recipients, CancellationToken cancellationToken = new CancellationToken(), ITransferProgress progress = null)
		{
			SendCalledCount++;
			ValidateSender(message);
			return base.Send(options, message, sender, recipients, cancellationToken, progress);
		}

		void ValidateSender(MimeMessage message)
		{
			if (message.Sender != null)
			{
				throw new InvalidOperationException("The Sender of the message should be null");
			}

			var mailboxEmailAddress = senderAddress;
			if (!string.IsNullOrEmpty(mailboxEmailAddress) && !message.From[0].Equals(InternetAddress.Parse(mailboxEmailAddress)) || ForceThrowSmtpCommandException)
			{
				base.OnRecipientNotAccepted(message, MailboxAddress.Parse(mailboxEmailAddress), new SmtpResponse(SmtpStatusCode.AuthenticationRequired, "blah blah"));
				throw new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.TransactionFailed, "SendAdDeniedException");
			}
		}
	}
}
