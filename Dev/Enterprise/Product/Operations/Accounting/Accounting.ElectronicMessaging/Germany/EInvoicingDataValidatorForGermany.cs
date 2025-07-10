using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	public class EInvoicingDataValidatorForGermany : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForGermany(GlbCompany company) : base(company)
		{
		}

		protected override void RunCore(ILogger logger)
		{
			//Here all codes for business validation will be written.
		}
	}
}
