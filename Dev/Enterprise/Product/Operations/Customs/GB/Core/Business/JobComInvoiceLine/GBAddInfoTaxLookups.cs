using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business
{
	public class GBAddInfoTaxLookups : EUAddInfoTaxLookups
	{
		public GBAddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
		{
		}

		public CodeDescriptionPairList RateSuspensionList => CommonLookupsHelper.RateSuspensionList;
		public CodeDescriptionPairList RateOverrideList => CommonLookupsHelper.RateOverrideList;

		public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

		protected override TaxLookupsCommon GetNewCommonLookupsHelper()
		{
			return new GBTaxLookupsCommon(Parent);
		}

		protected new GBTaxLookupsCommon CommonLookupsHelper => (GBTaxLookupsCommon)base.CommonLookupsHelper;
	}
}
