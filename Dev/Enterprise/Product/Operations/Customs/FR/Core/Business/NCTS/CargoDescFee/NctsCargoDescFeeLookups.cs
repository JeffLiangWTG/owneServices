using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NctsCargoDescFeeLookups : EU.NCTS.Business.NctsCargoDescFeeLookups
	{
		public NctsCargoDescFeeLookups(NctsCargoDescFee parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var isDeparture = ((NctsCargoDescFee)Parent).Parent?.Header?.IsDepartureMovement ?? false;
				return Factory.GetCachedValue("FR.NctsCargoDescFeeLookups.ChargeTypeList_" + isDeparture, delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddRange(base.ChargeTypeList);
					list.AddRange(new HarbourFeeCodes());
					list.RemoveCode(HarbourFeeCodes.Codes.P635);
					return list;
				});
			}
		}

		public CodeDescriptionPairList RateOverrideReasonCodeList => Factory.GetCachedValue<EU.Business.RateOverrideReasonList>();
	}
}
