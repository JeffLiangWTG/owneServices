using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCargoDescFeeLookups : CusInBondFeeLookups
	{
		public NctsCargoDescFeeLookups(NctsCargoDescFee parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var isDeparture = ((NctsCargoDescFee)Parent).Parent?.Header?.IsDepartureMovement ?? false;
				return Factory.GetCachedValue("EU.NCTS.Business.NctsCargoDescFeeLookups.ChargeTypeList_" + isDeparture, () =>
				{
					if (isDeparture)
					{
						return new DepartureCargoDescChargeTypeList();
					}
					return new CommonCargoDescChargeTypeList();
				});
			}
		}
	}
}
