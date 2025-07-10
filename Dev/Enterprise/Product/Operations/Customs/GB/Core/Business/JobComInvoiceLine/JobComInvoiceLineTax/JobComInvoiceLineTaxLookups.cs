using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business
{
	public class JobComInvoiceLineTaxLookups : EU.Business.Declaration.JobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(EU.Business.Declaration.JobComInvoiceLineTax parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList RateSuspensionList => CommonLookupsHelper.RateSuspensionList;

		public override CodeDescriptionPairList RateOverrideList => CommonLookupsHelper.RateOverrideList;

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected override TaxLookupsCommon GetNewCommonLookupsHelper()
		{
			return new GBTaxLookupsCommon(Parent);
		}

		protected new GBTaxLookupsCommon CommonLookupsHelper => (GBTaxLookupsCommon)base.CommonLookupsHelper;
	}
}
