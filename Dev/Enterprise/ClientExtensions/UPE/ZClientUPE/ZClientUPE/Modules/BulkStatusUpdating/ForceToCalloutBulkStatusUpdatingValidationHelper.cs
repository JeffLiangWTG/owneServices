
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.Module
{
	public class ForceToCalloutBulkStatusUpdatingValidationHelper : CalloutBulkStatusUpdatingValidationHelper
	{
		public ForceToCalloutBulkStatusUpdatingValidationHelper(UPECalloutQueue queue)
			: base(queue)
		{
		}

		public ForceToCalloutBulkStatusUpdatingValidationHelper(CalloutBulkStatusUpdatingQueue queue)
			: base(queue)
		{
		}

		protected override bool AllowEmptyQueueNameAndStatuses
		{
			get { return false; }
		}

		public override void ValidateRemarks()
		{
			base.ValidateRemarks();
			if (Queue.Reason.ToLower().IndexOf(Callout.ForcedToFinanceQueueRemarks.ToLower()) == -1)
			{
				Queue.ReasonInfo.AddError("You must have the words '" + Callout.ForcedToFinanceQueueRemarks + "' in the remarks.");
			}
		}
	}
}
