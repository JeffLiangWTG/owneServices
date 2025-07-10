using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkChargeImporterDependentJobCollection))]
	public class InvoicingBaseBulkChargeImporterDependentJobCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewElementToAddToTheCollection();
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			return new InvoicingBaseBulkChargeImporterDependentJobCollection(importer, Factory);
		}

		public void TestExcludeRatingsHeaderJobs()
		{
			InvoicingBaseBulkChargeImporterDependentJobCollection collection = (InvoicingBaseBulkChargeImporterDependentJobCollection)GetCollectionToTest();
			collection.Load();
			AssertEquals("Collection should be empty", 0, collection.Count);
			AddJobForSpotQuoteAndJobForShipment();
			collection.Load(new ZQuery());
			AssertEquals("Collection should contain shipment but not spot quote", 1, collection.Count);
			AssertEquals("this should be a shipment", JobShipmentSchema.Constants.Prefix, ((Job)collection.ToArray()[0]).JH_ParentTableCode);
		}

		void AddJobForSpotQuoteAndJobForShipment()
		{
			Job header = Factory.NewJobWithValidTestDataForTesting<Job>();
			header.JH_OA_LocalChargesAddr = testObjectCreator.AALSHI.Addresses[0].PK;
			header.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = testObjectCreator.CC1.PK;

			Job header2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			header2.JH_OA_LocalChargesAddr = testObjectCreator.AALSHI.Addresses[0].PK;
			header2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			JobCharge charge2 = header.Charges.AddNew();
			charge2.JR_AC = testObjectCreator.CC1.PK;
			Factory.Save();
		}

		TestObjectCreator testObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert("Doesn't apply to this collection as its parent class was made generic for a different purpose.", true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert("Doesn't apply to this collection as its parent class was made generic for a different purpose.", true);
		}
	}
}
