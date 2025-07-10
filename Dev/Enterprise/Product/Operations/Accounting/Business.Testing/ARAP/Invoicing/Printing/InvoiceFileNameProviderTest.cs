using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	public sealed class InvoiceFileNameProviderTest : TestCaseWithFactory
	{
		public void TestGetInvoiceFileName()
		{
			AssertEquals("AR INV 00010", InvoiceFileNameProvider.GetInvoiceFileName(arInvoice));
		}

		public void TestGetContainerListFileName()
		{
			AssertEquals("AR INV 00010 Container List", InvoiceFileNameProvider.GetContainerListFileName(arInvoice));
		}

		public void TestGetPeriodicInvoiceFileName()
		{
			AssertEquals("AR INV 00010 Periodic Details", InvoiceFileNameProvider.GetPeriodicInvoiceFileName(arInvoice));
		}

		protected override void SetUp()
		{
			base.SetUp();
			arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_TransactionType = "INV";
			arInvoice.AH_TransactionNum = "00010";
		}

		ARInvoice arInvoice;
	}
}
