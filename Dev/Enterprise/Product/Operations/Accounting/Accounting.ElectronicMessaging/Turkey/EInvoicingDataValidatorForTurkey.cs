using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class EInvoicingDataValidatorForTurkey : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForTurkey(GlbCompany company)
			: base(company)
		{
		}

		protected override void RunCore(ILogger logger)
		{
		}
	}
}
