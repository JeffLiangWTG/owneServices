using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineTaxLookups : EU.Business.Declaration.JobComInvoiceLineTaxLookups
	{
		public JobComInvoiceLineTaxLookups(JobComInvoiceLineTax parent) : base(parent)
		{
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		public override ICodeDescriptionPairList TypeList => new NationalFeeTypeCodeList(Factory);

		public override CodeDescriptionPairList MethodOfCalculationList => Factory.GetCachedValue<MethodOfCalculationList>();
	}
}
