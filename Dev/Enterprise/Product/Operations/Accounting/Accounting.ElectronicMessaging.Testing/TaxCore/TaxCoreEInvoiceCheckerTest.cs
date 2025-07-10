using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public class TaxCoreEInvoiceCheckerTest : TestCaseWithFactory
	{
		public void TestCheckAuthorised()
		{
			var invoiceBatch = Factory.New<AccEInvoicingBatch>();
			var response = new TaxCoreEInvoiceResponse();
			var checker = new TaxCoreEInvoiceChecker() as ITaxCoreEInvoiceChecker;

			Assert("Pre-condition: AIB_GovernmentAllocatedNumber is empty", invoiceBatch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("Pre-condition: InvoiceNumber is null or empty", string.IsNullOrEmpty(response.InvoiceNumber));
			Assert("CheckAuthorised is false as InvoiceNumber is not provided", !checker.CheckAndApplyAuthorisation(invoiceBatch, response));

			response.InvoiceNumber = "123";
			Assert("CheckAuthorised is true", checker.CheckAndApplyAuthorisation(invoiceBatch, response));

			invoiceBatch.AIB_GovernmentAllocatedNumber = "XYZ";
			Assert("CheckAuthorised is false as AIB_GovernmentAllocatedNumber is already assigned", !checker.CheckAndApplyAuthorisation(invoiceBatch, response));

			response.InvoiceNumber = "";
			Assert("CheckAuthorised is false as AIB_GovernmentAllocatedNumber is already assigned", !checker.CheckAndApplyAuthorisation(invoiceBatch, response));
		}

		public void TestApplyAuthorisation()
		{
			var invoiceBatch = Factory.New<AccEInvoicingBatch>();
			var response = new TaxCoreEInvoiceResponse() { InvoiceNumber = "123" };
			var checker = new TaxCoreEInvoiceChecker() as ITaxCoreEInvoiceChecker;

			Assert("Pre-condition: AIB_GovernmentAllocatedNumber is empty", invoiceBatch.AIB_GovernmentAllocatedNumber.IsEmpty);
			Assert("CheckAuthorised is true", checker.CheckAndApplyAuthorisation(invoiceBatch, response));
			AssertEquals("AIB_GovernmentAllocatedNumber must be set with InvoiceNumber", "123", invoiceBatch.AIB_GovernmentAllocatedNumber);
		}
	}
}
