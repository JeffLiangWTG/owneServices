using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class WorkQueueMembershipLinkValidation : TagLinkValidation
	{
		public WorkQueueMembershipLinkValidation(WorkQueueMembershipLink parent)
			: base(parent)
		{
		}

		protected new WorkQueueMembershipLink Parent => (WorkQueueMembershipLink)base.Parent;

		protected override void CheckTGL_Sequence()
		{
			base.CheckTGL_Sequence();

			if (Parent.ShouldValidateSequenceUniqueness)
			{
				var queue = Parent.Magnitude as WorkQueue;

				if (queue != null)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.TGL_SequenceInfo, queue.Members, checkEmptyValues: true, errorMessage: UniqueSequenceErrorMessage);
				}
			}
		}

		static string UniqueSequenceErrorMessage => Res.GetString("b1886e40-e4b2-4c84-969d-48a5015d60d3", "The Sequence has been duplicated and must be unique for each item in a Work Queue.");
	}
}
