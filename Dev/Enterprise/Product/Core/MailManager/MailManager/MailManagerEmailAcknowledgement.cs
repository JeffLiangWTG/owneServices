using System;

using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.MailManager.MailManagerEmailAcknowledgement))]

namespace Enterprise.MailManager
{
	public class MailManagerEmailAcknowledgement
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^ACK")]
		public bool ProcessAcknowledgementEmail(MailItem item, ILogger logger)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			if (ProcessAcknowledgementEmail(item, factory))
			{
				logger.Log(LogType.Debug, "Acknowledgement email processed.");
				factory.Save();
				return true;
			}

			return false;
		}

		#region Implementation

		bool ProcessAcknowledgementEmail(MailItem item, BusinessObjectFactory factory)
		{
			bool processed = false;

			ZString guidFromSubject = GetMailItemPKFromSubject(item.MI_Subject);

			ZGuid sourceMailItemPK;

			try
			{
				sourceMailItemPK = (guidFromSubject != ZString.Empty) ? new ZGuid(guidFromSubject) : ZGuid.Invalid;
			}
			catch (Exception zGuidEx) when (!zGuidEx.IsCriticalException())
			{
				sourceMailItemPK = ZGuid.Invalid;
				ErrorReporter.ReportOnce(zGuidEx.Message, zGuidEx);
			}

			if (sourceMailItemPK.IsValid)
			{
				MailItem sourceMailItem = factory.Load<MailItem>(sourceMailItemPK);

				if (sourceMailItem != null)
				{
					bool sourceMailSent = true;

					string fromEmail;
					if (item.MI_From.Contains("<"))
					{
						fromEmail = item.MI_From.Substring(item.MI_From.IndexOf("<") + 1,
							item.MI_From.LastIndexOf(">") - item.MI_From.IndexOf("<") - 1);
					}
					else
					{
						fromEmail = item.MI_From;
					}
					foreach (IMailRecipient recipient in sourceMailItem.MailRecipients)
					{
						if (recipient.EmailAddress.Contains(fromEmail))
						{
							recipient.MR_DeliveredTime = ZDateTime.UtcNow;
							processed = true;
						}
						if (recipient.MR_DeliveredTime.IsEmpty)
						{
							sourceMailSent = false;
						}
					}

					if (sourceMailSent)
					{
						sourceMailItem.MI_Status = MailStatus.Sent;
					}
				}
			}

			return processed;
		}

		ZString GetMailItemPKFromSubject(ZString subject)
		{
			int indexOfGUID = subject.IndexOf("GUID:");
			return (indexOfGUID != -1 && (subject.Length - indexOfGUID - 5) >= 36) ? subject.Substring(indexOfGUID + 5, 36) : ZString.Empty;
		}

		#endregion
	}
}
