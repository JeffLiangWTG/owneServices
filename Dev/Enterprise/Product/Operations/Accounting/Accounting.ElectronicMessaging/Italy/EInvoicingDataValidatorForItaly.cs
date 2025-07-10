using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class EInvoicingDataValidatorForItaly : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForItaly(GlbCompany company)
			: base(company)
		{
		}
		protected override void RunCore(ILogger logger)
		{
		}
	}
}
