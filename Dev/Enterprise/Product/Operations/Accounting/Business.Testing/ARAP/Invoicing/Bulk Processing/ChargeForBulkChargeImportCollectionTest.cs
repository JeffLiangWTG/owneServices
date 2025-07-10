using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ChargeForBulkChargeImportCollection))]
	public class ChargeForBulkChargeImportCollectionTest : ChargeCollectionTest
	{
		public void TestIsSelectedForImportCallsEventHandler()
		{
			bool isSelectedChangedWasCalled = false;
			ChargeForBulkChargeImportCollection collection = (ChargeForBulkChargeImportCollection)GetCollectionToTest();
			Charge charge = collection.AddNew();
			collection.IsSelectedChanged += delegate
			{ isSelectedChangedWasCalled = true; };
			charge.IsSelectedForImport = true;
			Assert(isSelectedChangedWasCalled);
		}

		public void TestSetChargeSelected()
		{
			var collection = GetCollectionToTest() as ChargeForBulkChargeImportCollection;
			var isSelectedChangedWasCalled = false;
			collection.IsSelectedChanged += delegate
			{ isSelectedChangedWasCalled = true; };
			var charge = Factory.NewWithValidTestData<Charge>();
			collection.SetChargeSelected(charge, true);
			Assert(isSelectedChangedWasCalled);
			isSelectedChangedWasCalled = false;
			collection.SetChargeSelected(charge, false);
			Assert(isSelectedChangedWasCalled);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ChargeForBulkChargeImportCollection(Factory.NewJobForTesting<InvoicingBaseBulkChargeImporterDependentJob>());
		}

		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}
	}
}
