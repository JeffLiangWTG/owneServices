using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface;
using MimeKit;
using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	public class MailItemCopySender : AutoMailItemCopySender
	{
		public MailItemCopySender()
			: this(System.Array.Empty<MailItem>())
		{
		}

		public MailItemCopySender(MailItem[] items)
			: base(new BusinessObjectFactory())
		{
			ItemsToSendCopy = items;
			SentItemPKs = System.Array.Empty<ZGuid>();
		}

		public override ZString MessagesToSend
		{
			get { return ItemsToSendCopy.Length.ToString(); }
		}

		MailItem[] ItemsToSendCopy { get; set; }
		public ZGuid[] SentItemPKs { get; private set; }

		public virtual void SendCopyTo()
		{
			Errors = "";
			var newItemPKs = new List<ZGuid>();

			RunPreSaveValidation();
			if (!HasErrors)
			{
				foreach (MailItem item in ItemsToSendCopy)
				{
					var message = MimeMessageExtensions.CreateMessageFromEml(item.RawEmailBytes.ToArray());
					message.Headers.Remove(HeaderId.To.ToHeaderName());
					message.Headers.Remove(HeaderId.Cc.ToHeaderName());
					message.Headers.Remove(HeaderId.Bcc.ToHeaderName());
					foreach (var recipient in MailAddressToSendCopyTo.Split(';'))
					{
						message.To.Add(new MailboxAddress(null, recipient.Trim()));
					}
					message.Subject = Res.GetString("f1386ea2-2a76-449b-bd64-dc38a627364c", "FW: {0}", message.Subject);

					var mailItem = Factory.New<MailItem>();
					mailItem.RawMIMEString = Encoding.UTF8.GetString(message.GetData());

					if (!string.IsNullOrEmpty(Env.Registry.MailboxEmailAddress))
					{
						mailItem.MI_From = ZString.Format("{0} <{1}>", OutgoingMailCreator.QuoteDisplayName(Env.Registry.MailboxDisplayName), Env.Registry.MailboxEmailAddress);
					}
					else if (Env.CurrentUser != null)
					{
						mailItem.MI_From = ZString.Format("{0} <{1}>", OutgoingMailCreator.QuoteDisplayName(Env.CurrentUser.FullName), Env.CurrentUser.EmailAddress);
					}
					else
					{
						mailItem.MI_From = item.MI_From;
					}

					mailItem.MI_Direction = MailDirection.Transmit;
					mailItem.MI_Status = MailStatus.Queued;
					mailItem.MI_SendDateTime = ZDateTime.UtcNow;
					mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
					mailItem.MI_ContentType = item.MI_ContentType;

					foreach (MailAttachment sourceAttachment in item.MailAttachments)
					{
						var fwdAttachment = mailItem.MailAttachments.AddNew();
						fwdAttachment.MA_ContentType = sourceAttachment.MA_ContentType;
						fwdAttachment.MA_Data = sourceAttachment.MA_Data;
						fwdAttachment.MA_Encoding = sourceAttachment.MA_Encoding;
						fwdAttachment.MA_FileName = sourceAttachment.MA_FileName;
					}

					newItemPKs.Add(mailItem.PK);
					mailItem.RunPreSaveValidation();

					if (mailItem.HasErrors)
					{
						Errors += mailItem.Notifications.GetErrors().ToUniqueMessageListString();
					}
				}
			}
			else
			{
				foreach (var notification in this.GetErrors())
				{
					Errors += notification.Message + System.Environment.NewLine;
				}
			}

			if (Errors.Length == 0)
			{
				Factory.Save();
				SentItemPKs = newItemPKs.ToArray();
			}
		}
	}
}
