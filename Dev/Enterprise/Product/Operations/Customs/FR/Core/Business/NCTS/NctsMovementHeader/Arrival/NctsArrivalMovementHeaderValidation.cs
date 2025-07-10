using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
	{
		public NctsArrivalMovementHeaderValidation(NctsArrivalMovementHeader parent) : base(parent)
		{
		}

		protected override void CheckBM_LocationOfGoodsCode()
		{
			base.CheckBM_LocationOfGoodsCode();

			var parent = (NctsArrivalMovementHeader)Parent;
			if (parent.IsSimplifiedNctsProcedure && parent.BM_LocationOfGoodsCode.IsEmpty)
			{
				parent.BM_LocationOfGoodsCodeInfo.AddMessageError(Res.GetString("5d26409d-db99-482f-a0f0-fb1856261c4e", "In a Simplified Procedure, this field can’t be empty."));
			}
		}

		protected override void CheckBM_MessageStatus()
		{
			base.CheckBM_MessageStatus();

			var parent = (NctsArrivalMovementHeader)Parent;
			if (parent.BM_MessageStatus == EDIMessageStatusList.Codes.Rejected)
			{
				var nctsHeader = (NctsHeader)parent.Header;
				var rejectionDetails = nctsHeader.GetLastFRMEventErrorDescription();

				if (!string.IsNullOrEmpty(rejectionDetails))
				{
					parent.BM_MessageStatusInfo.AddWarning(rejectionDetails);
				}
			}
		}
	}
}
