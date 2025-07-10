using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	abstract class OnlineDeliveryBase : QueuedForBatchProcessor, ISupportCreateDeliveryInfoStrategyFactory
	{
		protected OnlineDeliveryBase(DocDeliveryContact contact)
		{
			Contact = contact;
		}

		protected DocDeliveryContact Contact { get; private set; }

		public string AttachmentType
		{
			get
			{
				var attachementType = Contact.AttachmentTypeWithFormatSwitching;
				if (string.IsNullOrEmpty(attachementType))
				{
					attachementType = AttachmentTypeList.Codes.Xls;
				}
				return attachementType;
			}
		}

		protected override bool ConsolidateReports => !Contact.SendIndividually;

		protected override void SetAdditionalProperties(StmPrintJob printJob, DeliveryInfo info)
		{
			base.SetAdditionalProperties(printJob, info);
			printJob.SP_EmailAttachmentFormat = GetEmailAttachmentFormat(info, AttachmentType);
		}
	}
}
