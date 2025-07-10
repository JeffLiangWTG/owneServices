using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoiceBatchModule))]
	public class InvoiceBatchModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		public void TestGetNewGridCollection()
		{
			using (InvoiceBatchModule testModule = new InvoiceBatchModule())
			{
				ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
				APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
				DirectPayment directPayment = Factory.NewWithValidTestData(typeof(DirectPayment)) as DirectPayment;
				InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
				InvoiceBatchHeader batchHeader2 = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

				Factory.Save();

				BusinessObjectCollection dDRCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				dDRCollection.Load();
				AssertEquals(2, dDRCollection.Count);
			}
		}

		public void TestGetNewStandardMenuItems()
		{
			using (InvoiceBatchModule module = new InvoiceBatchModule())
			{
				MenuItem[] standardMenuItems = module.GetNewStandardMenuItems_ForTestOnly();

				AssertEquals(0, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("View")));
				AssertEquals(1, Array.IndexOf(standardMenuItems, standardMenuItems.FindByText("New")));

				AssertNull("There should be no 'edit' menu item", standardMenuItems.FindByText("&Edit"));

				var newMenuItem = standardMenuItems.FindByText("New");

				AssertNotNull("New menu item must exist", newMenuItem);
				AssertNotNull("NewBatchText_ForTestOnly menu item must exist", newMenuItem.MenuItems.FindByText(module.NewBatchText_ForTestOnly));
				AssertNotNull("NewBulkBatchText_ForTestOnly menu item must exist", newMenuItem.MenuItems.FindByText(module.NewBulkBatchText_ForTestOnly));
			}
		}
		public void TestGetNewAdditionalMenuItems()
		{
			using (InvoiceBatchModule module = new InvoiceBatchModule())
			{
				MenuItem[] additionalMenuItems = module.GetNewAdditionalMenuItems_ForTestOnly();
				MenuItem printMenuItem = additionalMenuItems.FindByText(module.PrintMenuText_ForTestOnly);

				AssertNotNull("Print menu item must exist", printMenuItem);
				AssertEquals(module.PrintMenuText_ForTestOnly, printMenuItem.Text);
			}
		}

		public void TestToolBarButtons()
		{
			using (InvoiceBatchModule module = new InvoiceBatchModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("View"));
				AssertNotNull(module.ToolBarButtons.FindByText("Cancel Batch"));
				AssertNotNull(module.ToolBarButtons.FindByText("&Actions"));
				AssertNotNull(module.ToolBarButtons.FindByText("&New"));
				AssertNotNull(module.ToolBarButtons.FindByText("Print"));
			}
		}

		public void TestCancelInvoiceBatchCannotPrint()
		{
			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
			batchHeader.AH_IsCancelled = ZBool.True;

			Factory.Save();
			using (InvoiceBatchModule testModule = new InvoiceBatchModule())
			{
				testModule.Print_ForTestOnly(batchHeader);
				AssertEquals("You cannot print canceled Invoice Batch Statement.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCancelMatchedInvoice()
		{
			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoiceLine invoiceLine = (ARInvoiceLine)aRInvoice.Lines.AddNew();
			invoiceLine.FillWithValidTestData();
			invoiceLine.AL_OSExTaxAmount = 120m;
			invoiceLine.AL_LocalExTaxAmount = 120m;
			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			batchHeader.Line.Add(aRInvoice);
			aRInvoice.AH_OutstandingAmount = 0m;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = matchLinks.AddNew();
			matchLink.AP_AH = aRInvoice.PK;
			matchLink.AP_Amount = aRInvoice.AH_InvoiceAmount;

			APJournal journalForMatching = Factory.NewWithValidTestData<APJournal>();
			journalForMatching.AH_OSExTaxAmount = aRInvoice.AH_OSExTaxAmount;
			journalForMatching.AH_LocalExTaxAmount = journalForMatching.AH_OSExTaxAmount;
			journalForMatching.AH_OutstandingAmount = 0M;
			matchLink = matchLinks.AddNew();
			matchLink.AP_AH = journalForMatching.PK;
			matchLink.AP_Amount = journalForMatching.AH_InvoiceAmount;
			Business.TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			Factory.Save();
			using (InvoiceBatchModule testModule = new InvoiceBatchModule())
			{
				testModule.ShowDeleteForm_ForTestOnly(batchHeader);
				AssertEquals("You cannot cancel this Invoice Batch since some of the invoices included in this batch are already matched.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCancelNormalInvoice()
		{
			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;

			Factory.Save();
			using (InvoiceBatchModule testModule = new InvoiceBatchModule())
			{
				using (IZForm resultForm = testModule.ShowDeleteForm_ForTestOnly(batchHeader))
				{
					AssertNotNull(resultForm);
				}
			}
		}

		public void TestPrintForIncorrectOrgInvoiceType()
		{
			InvoiceBatchHeader batchHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			batchHeader.AH_OH = organisation.PK;

			Factory.Save();
			using (InvoiceBatchModule testModule = new InvoiceBatchModule())
			{
				testModule.Print_ForTestOnly(batchHeader);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (InvoiceBatchModule module = new InvoiceBatchModule())
			{
				AssertEquals(module.GetDeleteMenuItemText_ForTestOnly().Caption, "Cancel Batch");
				AssertEquals("Cancels the selected item after viewing its details read-only (shortcut Del)", module.GetDeleteMenuItemText_ForTestOnly().FullDescription);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.InvoiceBatch;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.Add(Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)));
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			return new BusinessObject[] { Factory.NewWithValidTestData<InvoiceBatchHeader>() };
		}
	}
}
