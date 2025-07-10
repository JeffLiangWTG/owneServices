using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class ForceToCalloutBulkStatusUpdatingQueue : CalloutBulkStatusUpdatingQueue
	{
		public ForceToCalloutBulkStatusUpdatingQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new ForceToCalloutBulkStatusUpdatingValidationHelper(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			QueueName = CommercialQueueCodeDescriptionPairList.Codes.Finance;
			Status = ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment;
			Reason = Callout.ForcedToFinanceQueueRemarks;
		}
	}
}
