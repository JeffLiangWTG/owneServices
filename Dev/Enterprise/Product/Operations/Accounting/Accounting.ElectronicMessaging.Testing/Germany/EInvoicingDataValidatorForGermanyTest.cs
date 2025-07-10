using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Germany.Testing
{
	public class EInvoicingDataValidatorForGermanyTest : BaseEInvoicingDataValidatorTest
	{
		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForGermany(GlbCompany.CurrentCompany);
		}
	}
}
