using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class EInvoicingDataValidatorForItalyTest : BaseEInvoicingDataValidatorTest
	{
		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForItaly(GlbCompany.CurrentCompany);
		}
	}
}
