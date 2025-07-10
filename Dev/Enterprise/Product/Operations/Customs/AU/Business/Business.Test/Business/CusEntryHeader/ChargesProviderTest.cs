using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class ChargesProviderTest : TestCaseWithFactory
	{
		public abstract void TestInvoiceTotal();
		public abstract void TestCommission();
		public abstract void TestDiscount();
		public abstract void TestOtherCharges1();
		public abstract void TestOtherCharges2();
		public abstract void TestLandingCharges();
		public abstract void TestPackingCosts();
		public abstract void TestForeignInlandFreight();

		protected abstract ICommercialChargesProvider GetChargesProvider();

		protected JobDeclaration testDec;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine1;
		protected JobComInvoiceLine invoiceLine2;
		protected CusEntryHeader entryHeader;
		protected CusEntryLine entryLine;
		protected ICommercialChargesProvider chargesProvider;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();

			invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			chargesProvider = GetChargesProvider();
		}
	}
}
