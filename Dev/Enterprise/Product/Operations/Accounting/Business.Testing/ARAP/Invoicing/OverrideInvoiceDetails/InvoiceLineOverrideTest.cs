using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceLineOverrideTest : NonPersistentBusinessObjectTestCase
	{
		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		public void TestConstruction()
		{
			var line = Factory.New<APInvoiceLine>();
			var invoiceOverride = GetInvoiceOverride(line);
			AssertEquals("Line is set to construction arg", line, invoiceOverride.LineForTest);
			AssertNotNull(invoiceOverride.Lookups);
			AssertEquals("Correct Factory", line.Factory, invoiceOverride.Factory);
		}

		public void TestPassThroughProperties()
		{
			var job = TestObjectCreator.CreateJob("Job1", TestObjectCreator.LocalClient, 1, TestObjectCreator.Agent, 1);
			job.JH_JobLocalReference = "123";
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_JH = job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var charge = TestObjectCreator.CreateCharge(line);
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			line.AL_GB = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany).PK;
			line.AL_GE = TestObjectCreator.FIADepartment.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_RX_NKTransactionCurrency = testObjectCreator.USD.RX_Code;
			line.AL_ExchangeRate = 1m;
			line.AL_Desc = "123";
			Factory.Save();

			var invoiceOverride = GetInvoiceOverride(line);

			Assert("Precondition: There is a generic charge", !line.GenericCharge.IsEmpty);
			AssertEquals(line.GenericCharge, invoiceOverride.GenericCharge);
			AssertEquals(line.AL_GB, invoiceOverride.AL_GB);
			AssertEquals(line.AL_GE, invoiceOverride.AL_GE);
			AssertEquals(line.AL_AT, invoiceOverride.AL_AT);
			AssertEquals(line.AL_RX_NKTransactionCurrency, invoiceOverride.AL_RX_NKTransactionCurrency);
			AssertEquals(line.AL_OSExTaxAmount, invoiceOverride.AL_OSExTaxAmount);
			AssertEquals(line.AL_OverseasTotal, invoiceOverride.AL_OverseasTotal);
			AssertEquals(line.AL_LocalExTaxAmount, invoiceOverride.AL_LocalExTaxAmount);
			AssertEquals(line.AL_LocalTaxAmount, invoiceOverride.AL_LocalTaxAmount);
			AssertEquals(line.AL_LocalTotalAmount, invoiceOverride.AL_LocalTotalAmount);
			AssertEquals(line.ChargeTypeWithOverride, invoiceOverride.ChargeTypeWithOverride);
			AssertEquals(line.AL_LocalGSTAmount, invoiceOverride.AL_LocalGSTAmount);
			AssertEquals(line.AL_OSGSTAmount, invoiceOverride.AL_OSGSTAmount);
			AssertEquals(line.AL_JH, invoiceOverride.AL_JH);
			AssertEquals(job.JH_JobLocalReference, invoiceOverride.JobLocalReference);
		}

		protected abstract InvoiceLineOverride GetInvoiceOverride(DependentTransactionLine line);
	}
}
