using CargoWise.Integration;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceLineTaxLookups : EU.Business.Declaration.JobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(JobComInvoiceLineTax parent) : base(parent)
		{ }

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		public override ICodeDescriptionPairList TypeList
		{
			get
			{
				var isImport = Parent.InvoiceLine.IsImport;
				return Factory.GetCachedValue(string.Join("|", "DE.Business.Declaration.JobComInvoiceLineTax.TypeList", isImport), () => Parent.InvoiceLine.IsImport ? new SpecialCaseGroupList() : base.TypeList);
			}
		}

		public override CodeDescriptionPairList MethodOfCalculationList
		{
			get
			{
				var isImport = Parent.InvoiceLine.IsImport;
				return Factory.GetCachedValue(string.Join("|", "DE.Business.Declaration.JobComInvoiceLineTax.MethodOfCalculationList", isImport), () => isImport ? new SpecialCaseTypeList() : base.MethodOfCalculationList);
			}
		}

		protected override EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon GetNewCommonLookupsHelper() => new DETaxLookupsCommon(Parent);
	}
}
