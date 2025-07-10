using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPackageValidation : NctsPackagePhase5Validation
	{
		public NctsPackageValidation(NctsPackage parent)
			: base(parent)
		{
		}

		protected override ZLong B5_UnitCountMaxValue => GoodsItem is NctsDepartureCargoDesc && Parent.IsInPhase5TransitionPeriod ? 99999 : base.B5_UnitCountMaxValue;

		protected override void CheckB5_MarksAndNumbers_Mandatory()
		{
			var goodsItem = GoodsItem;
			var result = goodsItem is NctsDepartureCargoDesc;
			if (result)
			{
				base.CheckB5_MarksAndNumbers_Mandatory();
			}
		}
	}
}
