using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkConsolCostImporter))]
	public class InvoicingBaseBulkConsolCostImporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportPopulatesCalculatorPendingConsolCosts()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			TestObjectCreator.CreateShipment("S001", consol);
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);

			importer.LoadConsolsCollection();
			importer.Import();

			var accrualCalculator = Factory.GetCachedValue<ConsolAndCostAccrualCalculator>(ConsolAndCostAccrualCalculator.ConsolAndCostAccrualCalculatorKey, () => { return null; });
			AssertNotNull(accrualCalculator);
			AssertEquals(1, accrualCalculator.PendingConsolCosts.Count);
			AssertEquals(cost.PK, accrualCalculator.PendingConsolCosts[0].PK);
		}

		public void TestImportPopulatesPendingConsolCostsForActiveShipmentJobsOnly()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C000101");
			var shipment1 = TestObjectCreator.CreateShipment("S001010", consol);
			var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			var shipment2 = TestObjectCreator.CreateShipment("S001011", consol);
			var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			var shipment3 = TestObjectCreator.CreateShipment("S001012", consol);
			Factory.Save();
			AssertEquals("job header is active", true, shipment1.Job.JH_IsActive);
			AssertEquals("job header is active", true, shipment2.Job.JH_IsActive);
			AssertEquals("job header does not exist", null, shipment3.Job);

			job2.MarkAsInactive();
			Factory.Save();
			AssertEquals("job header is active", true, job1.JH_IsActive);
			AssertEquals("job header is inactive", false, job2.JH_IsActive);
			AssertEquals("job header does not exist", null, shipment3.Job);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
			AssertEquals(3, consolCost.ApportionmentCharges.Count);
			AssertEquals("job header is active", true, job1.JH_IsActive);
			AssertEquals("job header is active", true, job2.JH_IsActive);
			AssertEquals("job header is active", true, shipment3.Job.JH_IsActive);

			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;
			consolCost.ApportionmentCharges[2].JR_IsUsedForApportionment = false;
			Factory.Save();
			AssertEquals(1, consolCost.ApportionmentCharges.Count);

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);

			importer.LoadConsolsCollection();
			importer.Import();

			AssertEquals("Costing has 1 consol cost", 1, costing.ConsolCosts.Count);
			AssertEquals("Consol cost has 1 apportionment charge", 1, costing.ConsolCosts[0].ApportionmentCharges.Count);
			AssertEquals("Apportionment charge is for the active job", job1.JH_JobNum, costing.ConsolCosts[0].ApportionmentCharges[0].JR_JobNumber);
			AssertEquals("Shipment1 has a charge", 1, job1.Charges.Count);
			AssertEquals("Shipment2 has no charge", 0, job2.Charges.Count);
			AssertEquals("job header does not exist", null, shipment3.Job);
			AssertEquals("job header is active", true, job1.JH_IsActive);
			AssertEquals("job header is active", false, job2.JH_IsActive);
		}

		public void TestSelectedTotalUpdateForMultipleImporters()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			TestObjectCreator.CreateShipment("S001", consol);
			JobConsolCost cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost.E6_OSCostAmount = 100M;
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);
			importer.LoadConsolsCollection();
			AssertEquals("Precondition: ", 100M, importer.SelectedTotal);

			importer = new InvoicingBaseBulkConsolCostImporter(costing);
			importer.LoadConsolsCollection();
			AssertEquals("Current Importer must be set for consol collection.", importer, importer.Consols.Master);
			AssertEquals("Current Importer must be set for loaded consol.", importer, importer.Consols[0].Importer);
			AssertEquals("Current Importer must be set for consol cost collection.", importer, importer.Consols[0].ConsolCosts.Importer);
			AssertEquals("Current Importer must be set for consol cost.", importer, importer.Consols[0].ConsolCosts[0].Importer);
			AssertEquals("The second importer instance has correct total.", 100M, importer.SelectedTotal);
		}

		public void TestSelectedTotalUpdateForMultipleConsols()
		{
			ForwardingConsol consol1 = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			TestObjectCreator.CreateShipment("S001", consol1);
			JobConsolCost cost1 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost1.E6_OSCostAmount = 100M;
			JobConsolCost cost2 = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.CC2, TestObjectCreator.Creditor1);
			cost2.E6_OSCostAmount = 200M;

			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C002");
			TestObjectCreator.CreateShipment("S002", consol2);
			JobConsolCost cost3 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC1, TestObjectCreator.AALSHI);
			cost3.E6_OSCostAmount = 300M;
			JobConsolCost cost4 = TestObjectCreator.CreateConsolCost(consol2, TestObjectCreator.CC2, TestObjectCreator.Creditor1);
			cost4.E6_OSCostAmount = 400M;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);
			foreach (ModuleFilter filter in importer.Filters.AlwaysVisibleModuleFilters)
			{
				filter.IsActive = true;
			}
			importer.LoadConsolsCollection();
			AssertEquals("Precondition: ", 400M, importer.SelectedTotal);
		}

		public void TestConsolsCollectionInAnotherFactory()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			InvoicingBaseBulkConsolCostImporter importer = new InvoicingBaseBulkConsolCostImporter(costing);
			AssertNotEquals("Consols loaded in another factory.", importer.Consols.Factory._Instance, Factory._Instance);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator_cached ?? (testObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator_cached;
		protected override BusinessObject GetNewBusinessObject()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceConsolCosting costing = new APInvoiceConsolCosting(Factory, invoice);
			return new InvoicingBaseBulkConsolCostImporter(costing);
		}
	}
}
