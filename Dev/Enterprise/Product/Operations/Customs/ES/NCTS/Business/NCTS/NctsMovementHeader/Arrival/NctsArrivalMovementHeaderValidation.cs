using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsArrivalMovementHeaderValidation : EU.NCTS.Business.NctsArrivalMovementHeaderValidation
	{
		public NctsArrivalMovementHeaderValidation(EU.NCTS.Business.NctsArrivalMovementHeader parent)
			: base(parent)
		{
		}

		protected new NctsArrivalMovementHeader Parent => (NctsArrivalMovementHeader)base.Parent;

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			ListValidation.MessageErrorIfInvalidCode(Parent.BM_InBondEntryTypeInfo);
		}

		protected override void CheckGoodsLocationDescriptionCore()
		{
		}

		protected override void CheckBM_ArrivalDate()
		{
		}
	}
}
