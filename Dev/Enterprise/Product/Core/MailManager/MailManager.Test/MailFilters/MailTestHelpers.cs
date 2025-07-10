using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.IMAP;
using Enterprise.ZArchitecture.Core.Lists;
using MailKit;
using MailManager;
using SecureConnectionTypes = Enterprise.ZArchitecture.Core.SecureConnectionTypes;

namespace Enterprise.MailManager.MailFilters.Testing
{
	public static class MailTestHelpers
	{
		public static void SetServerCertificateValidationCallback(IMailService mailService)
		{
			mailService.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
		}

		public static IEnumerable<string> EnsureMailCountAboveZeroAndReturnMailIDs(MailKitImap imap)
		{
			return EnsureMailCountAboveZeroAndReturnImap(imap).GetAllMessageIds();
		}

		public static MailKitImap EnsureMailCountAboveZeroAndReturnImap(MailKitImap imap)
		{
			SendEmailWithTLS();
			var messageIds = imap.GetAllMessageIds();
			var sleepCount = 0;
			while (!messageIds.Any() && sleepCount < 10)
			{
				Thread.Sleep(50);
				sleepCount++;
				messageIds = imap.GetAllMessageIds();
			}
			return imap;
		}

		public static void SendEmailWithTLS(string recipient = "")
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var mailItem = factory.New<MailItem>();
			mailItem.MI_From = UserEmailAddress1;
			mailItem.AddRecipientForUserCommunication(string.IsNullOrEmpty(recipient) ? UserEmailAddress1 : recipient);
			mailItem.MI_Subject = "subject - email With TLS";
			mailItem.MI_Body = "This email was sent via email using TLS";

			using (var sender = new MailKitMailSender(new SmtpConfiguration(LocalServer, SmtpPort, ZArchitecture.Core.SecureConnectionTypes.SSL), new UserPasswordAuthConfiguration(Username1, Password1)))
			{
				SetServerCertificateValidationCallback(sender.SmtpClientImpl);
				sender.Send(mailItem);
			}
		}

		public static MailItem CreateMail(BusinessObjectFactory factory, string subject, string from, string status = MailStatus.Queued, string application = MailApplication.Standard, DateTime? recieved = null)
		{
			var mail = factory.NewWithValidTestData<MailItem>();

			mail.MI_ReceivedDateTime = recieved ?? DateTime.Now;
			mail.MI_Subject = subject;
			mail.MI_From = from;
			mail.MI_Status = status;
			mail.MI_Application = application;
			mail.MI_Direction = "RCV";

			return mail;
		}

		public static MailItem GetMailItemWithoutAttachments(BusinessObjectFactory factory)
		{
			var result = factory.New<MailItem>();
			result.MI_Direction = DirectionList.Codes.Transmit;
			result.AddRecipientForUserCommunication("To@host", MailRecipient.RecipientTypes.TO);
			result.AddRecipientForUserCommunication("Cc@host", MailRecipient.RecipientTypes.CC);
			result.AddRecipientForUserCommunication("Bcc@host", MailRecipient.RecipientTypes.BCC);

			return result;
		}

		public static MailServerConfiguration GetConfiguration(string protocol)
		{
			return protocol switch
			{
				MailRetrievalProtocols.POP3 => new MailServerConfiguration(LocalServer, Pop3Port, Username1, Password1, protocol, SecureConnectionTypes.None),
				MailRetrievalProtocols.IMAP => new MailServerConfiguration(LocalServer, ImapPort, Username1, Password1, protocol, SecureConnectionTypes.None),
				_ => throw new NotImplementedException()
			};
		}

		internal static readonly string LocalServer = IPAddress.Loopback.ToString();
		internal static readonly string Username1 = "cargowiseuatcentraltestaccount";
		internal static readonly string Password1 = "C3ntr4lT35t";
		internal static readonly string Username2 = "alpha";
		internal static readonly string Password2 = "6Jo%q8dz";
		internal static readonly string UserEmailAddress1 = "cargowiseuatcentraltestaccount@test.wisecloud.zone";
		internal static readonly string UserEmailAddress2 = "alpha@test.wisecloud.zone";
		internal static readonly ushort SmtpPort = 25;
		internal static readonly ushort Pop3Port = 110;
		internal static readonly ushort ProlongedBlockingPop3Port = 120;
		internal static readonly ushort ImapPort = 143;
	}
}
