using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class KoreaSouthPayloadValidationTest : TestCaseWithFactory
	{
		public void TestPayloadValidation_GenerateInvoiceRequest()
		{
			ICountryEInvoicingObjectFactory setting = new KoreaSouthEInvoicingObjectFactory();
			AssertType<GenerateInvoiceRequestPayloadValidation>(setting.GetTransactionBatchToPayloadWriter().GetPayloadValidation(KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest));
		}
	}
}
