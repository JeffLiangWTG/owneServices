using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.ExternalMailInterface
{
	static class MailItemExtensions
	{
		const string allowedPunctuations = @"!@#%&*()-_[{]}\;:'"",./?";

		public static ZString RemoveUnsupportedPunctuations(this ZString value)
		{
			var chars = value.ToString().Where(c => !char.IsPunctuation(c) || (char.IsPunctuation(c) && allowedPunctuations.Contains(c)));
			return new string(chars.ToArray());
		}

		public static bool HasActiveRecipients(this MailItem mailItem)
		{
			return (from recipient in mailItem.MailRecipients.Cast<IMailRecipient>() where !recipient.IsSuspended select recipient).Any();
		}

		public static bool AttachmentIsReferredToInHTMLBody(this MailItem mailItem, MailAttachment attachment, out string matchingFileName)
		{
			if (mailItem.MI_ContentType == EmailContentTypes.HTML.ContentTypeCode)
			{
				Regex attachmentIsReferred1 = new Regex("<(a|img).*?(src|href)=[\"'](cid:)?" + Regex.Escape(attachment.MA_FileName) + "[\"'].*?>");
				if (attachmentIsReferred1.IsMatch(mailItem.MI_Body))
				{
					matchingFileName = attachment.MA_FileName;
					return true;
				}
				var encodedFileName = System.Web.HttpUtility.UrlPathEncode(attachment.MA_FileName);
				Regex attachmentIsReferred2 = new Regex("<(a|img).*?(src|href)=[\"'](cid:)?" + Regex.Escape(encodedFileName) + "[\"'].*?>", RegexOptions.IgnoreCase);
				if (attachmentIsReferred2.IsMatch(mailItem.MI_Body))
				{
					matchingFileName = encodedFileName;
					return true;
				}
			}

			matchingFileName = null;
			return false;
		}

		public static void UpdateMailItemStatus(this MailItem mailItem)
		{
			UpdateRecipientsStatus(mailItem);
			if (mailItem.MI_Status != MailStatus.QueuedWithAck)
			{
				UpdateMailItemWithoutACK(mailItem);
			}
			else
			{
				UpdateMailItemWithACK(mailItem);
			}
		}

		static void UpdateMailItemWithoutACK(MailItem mailItem)
		{
			if (mailItem.MailRecipients.Count > 0)
			{
				mailItem.MI_Status = MailStatus.Sent;
				foreach (IMailRecipient recipient in mailItem.MailRecipients)
				{
					recipient.MR_DeliveredTime = ZDateTime.UtcNow;
				}
			}
			else
			{
				mailItem.MI_Status = MailStatus.Failed;
			}
		}

		/// <summary>
		/// Logic is:
		/// if at least one recipient is waiting for ACK, leave status as waiting for ACK
		/// if no recipients is waiting for ACK, then if all successfully delivered, then status is SENT
		///		if at least one is failed, then status is FAILED
		/// </summary>
		/// <param name="mailItem">Mail item to set status for</param>
		static void UpdateMailItemWithACK(MailItem mailItem)
		{
			if (!mailItem.MailRecipients.ContainsWaitingForACKRecipients)
			{
				if (mailItem.MailRecipients.ContainsFailedRecipients)
				{
					mailItem.MI_Status = MailStatus.Failed;
				}
				else if (mailItem.MailRecipients.AllRecipientsSuccessfullyDelivered)
				{
					mailItem.MI_Status = MailStatus.Sent;
				}
			}
		}

		static void UpdateRecipientsStatus(MailItem mailItem)
		{
			if (mailItem.MI_Status == MailManager.MailStatus.QueuedWithAck)
			{
				foreach (IMailRecipient recipient in mailItem.MailRecipients)
				{
					if (!recipient.IsSuspended)
					{
						recipient.MR_AckAttempt++;
						recipient.MR_LastAttempt = ZDateTime.UtcNow;
					}
				}
			}
			else
			{
				foreach (IMailRecipient recipient in mailItem.MailRecipients)
				{
					recipient.MR_AckAttempt++;
					recipient.MR_LastAttempt = ZDateTime.UtcNow;
				}
			}
		}
	}
}
