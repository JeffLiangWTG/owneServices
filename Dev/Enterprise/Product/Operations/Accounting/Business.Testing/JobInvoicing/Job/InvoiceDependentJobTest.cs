using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(InvoiceDependentJob))]
	public class InvoiceDependentJobTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return TestObject;
		}
		protected override void SetUp()
		{
			base.SetUp();
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestObject = new InvoiceDependentJob(Factory, invoice, job);
			line1 = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			line2 = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			line1.AL_JH = job.PK;
			line2.AL_JH = job.PK;
			TestObject.RelatedInvoiceLines = new[] { line1, line2 };
		}
		InvoiceDependentJob TestObject;
		InvoicingLineBase line1;
		InvoicingLineBase line2;

		public void TestJH_RelatedInvoiceLinesTotalCostInInvoiceCurrency()
		{
			line1.AL_OSAmount = -500m;
			line2.AL_OSAmount = -100m;
			AssertEquals(600m, TestObject.JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency);
		}

		public void TestJH_RelatedInvoiceLinesCostAmountInInvoiceCurrency()
		{
			line1.AL_OSExTaxAmount = 500m;
			line2.AL_OSExTaxAmount = 100m;
			AssertEquals(600m, TestObject.JH_RelatedInvoiceLinesCostAmountInInvoiceCurrency);
		}

		public void TestJH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency()
		{
			line1.AL_OSTaxAmount = 500m;
			line2.AL_OSTaxAmount = 100m;
			AssertEquals(600m, TestObject.JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency);
		}

		public void TestJH_RelatedInvoiceLinesTotalCost()
		{
			line1.AL_LocalExTaxAmount = 500m;
			line2.AL_LocalExTaxAmount = 100m;
			line1.AL_LocalTaxAmount = 50m;
			line2.AL_LocalTaxAmount = 10m;
			AssertEquals(660m, TestObject.JH_RelatedInvoiceLinesTotalCost);
		}

		public void TestJH_RelatedInvoiceLinesCostAmount()
		{
			line1.AL_LocalExTaxAmount = 500m;
			line2.AL_LocalExTaxAmount = 100m;
			AssertEquals(600m, TestObject.JH_RelatedInvoiceLinesCostAmount);
		}

		public void TestJH_RelatedInvoiceLinesCostTaxAmount()
		{
			line1.AL_LocalTaxAmount = 500m;
			line2.AL_LocalTaxAmount = 100m;
			AssertEquals(600m, TestObject.JH_RelatedInvoiceLinesCostTaxAmount);
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
	}
}
