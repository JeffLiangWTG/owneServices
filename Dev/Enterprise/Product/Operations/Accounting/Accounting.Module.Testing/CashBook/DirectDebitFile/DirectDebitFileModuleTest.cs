using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DirectDebitFileModule))]
	public class DirectDebitFileModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		public void TestGetNewGridCollection()
		{
			using (DirectDebitFileModule testModule = new DirectDebitFileModule())
			{
				ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
				APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
				DirectPayment directPayment = Factory.NewWithValidTestData(typeof(DirectPayment)) as DirectPayment;
				DirectDebitBatchHeader directBatchHeader = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
				DirectDebitBatchHeader directBatchHeader2 = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

				Factory.Save();

				BusinessObjectCollection dDRCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				dDRCollection.Load();
				AssertEquals(2, dDRCollection.Count);
			}
		}

		public void TestMenuItems()
		{
			using (DirectDebitFileModule testModule = new DirectDebitFileModule())
			{
				MenuItem[] testMenuItems = testModule.GetNewActionMenuItems_ForTestOnly();

				AssertEquals(2, testMenuItems.Length);

				//AssertEquals(TestModule.ViewMenuItemText, TestMenuItems[0].Text);
				//AssertEquals(TestModule.NewMenuItemText, TestMenuItems[1].Text);
				//AssertEquals("Generate DDR File", TestMenuItems[2].Text);
				//AssertEquals("Cancel Batch", TestMenuItems[4].Text);
				AssertEquals("D&ata Transfer", testMenuItems[0].Text);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (DirectDebitFileModule module = new DirectDebitFileModule())
			{
				var deleteMenuItem = module.FormActionMenu.FindByText("Cancel Batch") as ZMenuItem;
				if (deleteMenuItem != null)
				{
					AssertEquals("Cancel Batch", deleteMenuItem.Text);
					AssertEquals("Cancels the selected batch after viewing its details read-only (shortcut Del)", deleteMenuItem.CaptionResourceString.FullDescription);
				}
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DirectDebitFile;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.Add(Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)));
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			return new BusinessObject[] { Factory.NewWithValidTestData<DirectDebitBatchHeader>() };
		}
	}
}
