using CargoWise.Integration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineTaxLookups : Customs.Business.JobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(JobComInvoiceLineTax parent) : base(parent)
		{ }

		protected TaxLookupsCommon CommonLookupsHelper
		{
			get
			{
				return Factory.GetCachedValue(this.GetType().FullName + ".CommonLookupsHelper_" + Parent.PK, () =>
				{
					return GetNewCommonLookupsHelper();
				});
			}
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected virtual TaxLookupsCommon GetNewCommonLookupsHelper()
		{
			return new TaxLookupsCommon(Parent);
		}

		public virtual CodeDescriptionPairList RateSuspensionList => new CodeDescriptionPairList();
		public virtual CodeDescriptionPairList RateDutyList => CommonLookupsHelper.RateDutyList;
		public override CodeDescriptionPairList MOPList => CommonLookupsHelper.MOPList;
		public override ICodeDescriptionPairList TypeList => CommonLookupsHelper.TypeList;
		public override CodeDescriptionPairList BaseQuantityUQList => CommonLookupsHelper.BaseQuantityUQList;
	}
}
