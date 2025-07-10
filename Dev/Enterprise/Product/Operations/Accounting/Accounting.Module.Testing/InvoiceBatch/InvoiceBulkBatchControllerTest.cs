using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoiceBulkBatchController))]
	class InvoiceBulkBatchControllerTest : InvoiceBatchControllerGeneralTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.InvoiceBulkBatch;
		}

		public override void TestDeleteForm()
		{
			Assert(true);
		}

		public override void TestEditForm()
		{
			Assert(true);
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true); // non Customs Controller
		}

		public new void TestDoReversingBeforeShowDeleteForm()
		{
			Assert("Bulk batch is not saved in DB.", true);
		}

		public new void TestGetTopLevelBusinessObjectTypeCaching()
		{
			Assert("Bulk batch is not saved in DB.", true);
		}

		public new void TestGetTopLevelBusinessObjectCachedWithoutFactoryLoad()
		{
			Assert("Bulk batch is not saved in DB.", true);
		}

		public new void TestShowDeleteFormDeleteMode()
		{
			Assert("Bulk batch is not saved in DB.", true);
		}

		public new void TestShowDeleteFormForMultipleReversing()
		{
			Assert("Bulk batch is not saved in DB.", true);
		}

		public new void TestShowEditFormShowsEditForm()
		{
			Assert("Bulk batch is not saved in DB.", true);
		}

		protected override bool DeleteShouldShowForm
		{
			get { return false; }
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestInvoiceBatch; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestInvoiceBatch = Factory.NewWithValidTestData<InvoiceBulkBatch>();
			Factory.Save();
		}

		InvoiceBatchHeader TestInvoiceBatch;
	}
}
