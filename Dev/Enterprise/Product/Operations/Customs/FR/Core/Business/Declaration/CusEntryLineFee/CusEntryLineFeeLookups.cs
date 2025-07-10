using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent) : base(parent)
		{
		}

		public new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new FRTaxLookupsCommon(EntryLineFee);

		public override CodeDescriptionPairList RateOverrideReasonList => Factory.GetCachedValue<RateOverrideReasonList>();
	}
}
