using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.Business
{
	public static class EmailLoopDetector
	{
		static int GetReceivedEmailsCountInPeriod(MailItem mail, string application, string businessObjectNumber)
		{
			var minutesAgo = -EDIDataRegistry.Instance.EmailLoopDetectingWindowDuration.Value;
			var query = new ZDBOnlyQuery(typeof(MailItem));
			query.AddToFilter(MailDBItemsSchema.MI_ReceivedDateTime, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddMinutes(minutesAgo));
			query.AddToFilter(MailDBItemsSchema.MI_Application, application);
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive);

			query.AddToFilter(MailDBItemsSchema.MI_From, mail.MI_From);
			query.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, businessObjectNumber);

			query.MaximumRows = EDIDataRegistry.Instance.EmailLoopMaximumEmailCount.Value + 1;

			return mail.Factory.Load<MailItem>(query).Length;
		}

		public static bool IsReceivedEmailsCountOutOfLimit<T>(this EDIBusinessObjectEmailProcessor<T> processor, MailItem mail) where T : BusinessObject, IAllowAttachEmailsToEDocs
		{
			if (!EDIDataRegistry.Instance.EnableEmailLoopDetection.Value)
			{
				return false;
			}

			var businessObjectNumber = processor.GetBusinessObjectIdentifierFromSubject(mail.MI_Subject);
			var result = GetReceivedEmailsCountInPeriod(mail, processor.MailApplicationCode, businessObjectNumber) >= EDIDataRegistry.Instance.EmailLoopMaximumEmailCount.Value;

			return result;
		}
	}
}
