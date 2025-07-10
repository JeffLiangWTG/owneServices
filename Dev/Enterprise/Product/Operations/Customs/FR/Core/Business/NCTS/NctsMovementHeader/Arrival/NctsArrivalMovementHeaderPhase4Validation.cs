using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsArrivalMovementHeaderPhase4Validation : NctsArrivalMovementHeaderValidation
	{
		public NctsArrivalMovementHeaderPhase4Validation(NctsArrivalMovementHeader parent) : base(parent)
		{
		}

		protected override void CheckBM_PlaceOfUnloading()
		{
			base.CheckBM_PlaceOfUnloading();
			var parent = (NctsArrivalMovementHeader)Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.BM_PlaceOfUnloadingInfo);
		}
	}
}
