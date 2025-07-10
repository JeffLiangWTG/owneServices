using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class ArrivalNctsHeaderValidation : NctsHeaderValidation
	{
		public ArrivalNctsHeaderValidation(NctsHeader parent)
			: base(parent)
		{
		}

		protected override void CheckArrivalMrnFromUserCore()
		{
			base.CheckArrivalMrnFromUserCore();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ArrivalMrnFromUserInfo);
		}

		protected override void CheckEventCancellationReason()
		{
			if (Parent.BH_ExportFlag == EventFlagList.Codes.Cancelled)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.EventCancellationReasonInfo);
			}
		}

		protected override void CheckBH_ExportFlag()
		{
			base.CheckBH_ExportFlag();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BH_ExportFlagInfo);
		}
	}
}
