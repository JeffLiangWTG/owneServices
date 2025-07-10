using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(InvoiceDependentJobCollection))]
	public class InvoiceDependentJobCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceDependentJobCollection>
	{
		protected override void SetUp()
		{
			base.SetUp();
			invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
			TestObject = new InvoiceDependentJobCollection(invoice, Factory);
			TestObject.Add(this.GetNewElementToAddToTheCollection());
			TestObject.Add(this.GetNewElementToAddToTheCollection());
		}

		protected override InvoiceDependentJobCollection GetCollectionToTest()
		{
			return new InvoiceDependentJobCollection(TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI), Factory);
		}

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert("Doesn't apply to this collection as its parent class was made generic for a different purpose.", true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert("Doesn't apply to this collection as its parent class was made generic for a different purpose.", true);
		}

		public void TestInvoiceDependentJobCollectionItemsCount()
		{
			AssertEquals(TestObject.Count, 2);
		}

		[ExpectNoExceptions]
		public void TestInvoiceDependentJobCollectionPopulateJobsAndAmountsForAPInvoiceSummaryTab_ShouldCalculatesDistinct_MeansOnlyOne()
		{
			TestObject.RemoveAndPopulateJobsAndAmountsForAPInvoiceSummaryTab();
			AssertEquals(TestObject.Count, 1);
		}

		public void TestInvoiceDependentJobCollectionPopulateJobsAndAmountsForAPInvoiceSummaryTab_DoNotCreateBizObjUnnecessarily()
		{
			var inv1 = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
			inv1.Lines.RemoveAndDeleteAll();
			for (var idx = 0; idx < 100; idx++)
			{
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				var line1 = TestObjectCreator.CreateInvoiceLine(inv1, inv1.TransactionCurrency, inv1.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
				line1.AL_JH = job.PK;
			}

			var collection = new InvoiceDependentJobCollection(inv1, Factory);
			AssertEquals(0, collection.InvoiceDependentJobCreatedCount_ForTestOnly);
			collection.RemoveAndPopulateJobsAndAmountsForAPInvoiceSummaryTab();
			AssertEquals(100, collection.InvoiceDependentJobCreatedCount_ForTestOnly);
			AssertEquals(100, collection.Count);
			AssertEquals(100, inv1.Lines.Count);

			inv1.Lines.Remove(inv1.Lines[8].PK);
			inv1.Lines.Remove(inv1.Lines[32].PK);
			inv1.Lines.Remove(inv1.Lines[64].PK);
			AssertEquals(97, inv1.Lines.Count);

			collection.RemoveAndPopulateJobsAndAmountsForAPInvoiceSummaryTab();
			AssertEquals(100, collection.InvoiceDependentJobCreatedCount_ForTestOnly);
			AssertEquals(97, collection.Count);

			for (var idx = 0; idx < 10; idx++)
			{
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				var line1 = TestObjectCreator.CreateInvoiceLine(inv1, inv1.TransactionCurrency, inv1.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
				line1.AL_JH = job.PK;
			}
			AssertEquals(107, inv1.Lines.Count);

			collection.RemoveAndPopulateJobsAndAmountsForAPInvoiceSummaryTab();
			AssertEquals(110, collection.InvoiceDependentJobCreatedCount_ForTestOnly);
			AssertEquals(107, collection.Count);
		}

		[ExpectNoExceptions]
		public void TestInvoiceDependentJobCollectionIndexer()
		{
			AssertNotNull(TestObject[0].Job);
			AssertNotNull(TestObject[1].Job);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var testObject = new InvoiceDependentJob(Factory, invoice, job);
			invoice.Lines.RemoveAll();
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			line1.AL_JH = job.PK;
			line2.AL_JH = job.PK;
			return testObject;
		}
		APInvoice invoice;
		InvoiceDependentJobCollection TestObject;

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
