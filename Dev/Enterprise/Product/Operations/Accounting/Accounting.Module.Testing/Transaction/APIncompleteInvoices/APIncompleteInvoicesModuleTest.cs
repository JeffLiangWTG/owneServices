using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	[TestedType(typeof(APIncompleteInvoicesModule))]
	public class APIncompleteInvoicesModuleTest : ZModuleBasherTest
	{
		public void TestApprovalRequestStatusColumn()
		{
			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var column = module.DisplayGrid.GetColumnStyle(InvoicingBase.Schema.ApprovalRequestStatus);
				AssertNotNull("ApprovalRequestStatus column", column);
				Assert("Hidden by default", !column.IsVisible);
			}
		}

		public void TestController()
		{
			using (APIncompleteInvoicesModule moduleToTest = new APIncompleteInvoicesModule())
			{
				AssertEquals(ControllerIDs.APIncompleteInvoice, moduleToTest.GetNewController_ForTestOnly(GetBusinessObjectsToGetControllersFor()[0]).ID);
			}
		}

		public void TestAllowNew()
		{
			using (APIncompleteInvoicesModule moduleToTest = new APIncompleteInvoicesModule())
			{
				Assert(!moduleToTest.AllowNew);
			}
		}

		public void TestCanBeCopied()
		{
			using (APIncompleteInvoicesModule moduleToTest = new APIncompleteInvoicesModule())
			{
				Assert(!moduleToTest.CanBeCopied_ForTestOnly());
			}
		}

		public void TestGetNewStandardMenuItems()
		{
			using (APIncompleteInvoicesModule module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem[] standardMenuItems = module.GetNewStandardMenuItems_ForTestOnly();
				AssertNotNull("There should be a 'Cancel' menu item", standardMenuItems.FindByText(module.CancelIncompleteInvoiceMenuText_ForTestOnly.Caption));
			}
		}

		public void TestHandleCancel()
		{
			BusinessObject[] transactions = GetBusinessObjectsToGetControllersFor();
			Assert("Should be at least one", transactions.Length > 0);
			((TransactionHeader)transactions[0]).AH_IsCancelled = true;
			Factory.Save();

			using (APIncompleteInvoicesModule module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					APIncompleteInvoicesFilterBusinessObject filterBO = (APIncompleteInvoicesFilterBusinessObject)module.FilterBusinessObject;

					module.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandleCancel_ForTestOnly(this, new EventArgs());
					AssertEquals("This transaction is already canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleCancelPostedIncompleteInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var incompleteInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001", testObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m);
			incompleteInvoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var apInvoice = newFactory.Load<APInvoice>(incompleteInvoice.PK);
			apInvoice.RestoreSavedData();
			apInvoice.MoveFromIncompleteToPayableLedger();

			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var aPIncompleteInvoicesForm = new ZForm())
				{
					aPIncompleteInvoicesForm.Controls.Add(module.EmbeddedControl);
					aPIncompleteInvoicesForm.Show();

					var filterBO = (APIncompleteInvoicesFilterBusinessObject)module.FilterBusinessObject;
					module.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(1, testCollection.Count);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					newFactory.Save(); //post incomplete invoice

					var mockAccountingControllerIdDecider = new Mock<IAccountingControllerIdDecider>();
					ObjectFactory.Substitute(mockAccountingControllerIdDecider.Object);
					mockAccountingControllerIdDecider.Setup(x => x.GetControllerID(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ModuleIdentifier>())).Returns<ControllerID>(null);

					module.HandleCancel_ForTestOnly(this, new EventArgs());
					mockAccountingControllerIdDecider.VerifyNoOtherCalls();
					AssertEquals("The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleCancelEmptyTransactions()
		{
			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var aPIncompleteInvoicesForm = new ZForm())
				{
					aPIncompleteInvoicesForm.Controls.Add(module.EmbeddedControl);
					aPIncompleteInvoicesForm.Show();

					AssertNoExceptionThrown(() => { module.HandleCancel_ForTestOnly(this, new EventArgs()); });
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCancelIncompleteInvoice_WithDeletedChargeCode_NoExceptionOutput()
		{
			var creator = new TestObjectCreator(Factory);
			var gst = creator.CreateTaxRate("GST1", "GST Rate 1", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1);
			var whtfree = creator.CreateOrLoadWithholdingTax("WHTFREE1", "WHT Free Rate 1", 0);
			var cc = creator.CreateChargeCode("TestCC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, gst, whtfree);

			var incompleteInvoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "00001", creator.AUD, 1m, 10m, 10m, 10m, 10m, creator.ABIGAS, cc.PK);
			incompleteInvoice.AH_IsCancelled = false;
			incompleteInvoice.SaveAsIncomplete();

			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var aPIncompleteInvoicesForm = new ZForm())
				{
					module.GridCollection.Add(incompleteInvoice);
					aPIncompleteInvoicesForm.Controls.Add(module.EmbeddedControl);
					aPIncompleteInvoicesForm.Show();

					var newFactory = new BusinessObjectFactory();
					var newCC = newFactory.Load<AccChargeCode>(cc.PK);

					newCC.Delete();
					Assert(newCC.IsDeleted);
					newCC.Factory.Save();

					AssertNoExceptionThrown(() => { module.HandleCancel_ForTestOnly(this, new EventArgs()); });
					AssertContains("Could not find generic charge ZZTestCC1", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOSAmountNotZeroAfterSavingAsIncomplete()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			APInvoice invoice = creator.CreateAPInvoice<APInvoice>("1111", creator.AUD, 1M, 0, 0, 0, 200, 0, 0, creator.ABIGAS);
			APInvoiceLine line = creator.CreateAPInvoiceLine(invoice, creator.Job1, creator.CC1, creator.AUD, 1M, "Hello", 0);
			invoice.SaveAsIncomplete();
			Factory.Save();

			AssertEquals("Invoice Should Not have AH_OutStandingAmount Equal to Zero", (Decimal)(-200), (Decimal)invoice.AH_OutstandingAmount);
		}

		public void TestExportQuery()
		{
			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				var expectedSQL = string.Format(System.Globalization.CultureInfo.InvariantCulture,
@"(
	AH_GC = '{0}'
)
AND
AH_Ledger = 'IN'", GlbCompany.CurrentCompany.PK);
				AssertContains(expectedSQL, module.ExportQuery_ForTestOnly.LiteralTextSqlFormatted);
			}
		}

		public void TestHasTypeErrorForSelectedBusinessObjects()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var incompleteInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001", testObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m);
			incompleteInvoice.SaveAsIncomplete();
			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var form = new ZForm())
				{
					AssertEquals("Ledger Type", "IN", incompleteInvoice.AH_Ledger);
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var newFactory = new BusinessObjectFactory();
					newFactory.RefreshEnabled = false;
					var apInvoice = newFactory.Load<APInvoice>(incompleteInvoice.PK);
					apInvoice.RestoreSavedData();
					apInvoice.MoveFromIncompleteToPayableLedger();
					apInvoice.Factory.Save();

					AssertEquals("Ledger Type is still IN", "IN", incompleteInvoice.AH_Ledger);
					AssertEquals("Ledger Type chaged", "AP", apInvoice.AH_Ledger);

					module.GridCollection.Add(incompleteInvoice);
					module.DisplayGrid.SelectAllElements();
					var editMenuItem = module.FormActionMenu.FindByText("Edit");
					editMenuItem.PerformClick();
					AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

					var deleteMenuItem = module.FormActionMenu.FindByText("Delete");
					deleteMenuItem.PerformClick();
					AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHasTypeErrorForSelectedBusinessObjects_DeleteIncompleteInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var incompleteInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001", testObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m);
			incompleteInvoice.SaveAsIncomplete();
			using (var module = (APIncompleteInvoicesModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				using (var form = new ZForm())
				{
					AssertEquals("Ledger Type", "IN", incompleteInvoice.AH_Ledger);
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var newFactory = new BusinessObjectFactory();
					newFactory.RefreshEnabled = false;
					var apInvoice = newFactory.Load<APInvoice>(incompleteInvoice.PK);
					apInvoice.Delete();
					apInvoice.Factory.Save();

					module.GridCollection.Add(incompleteInvoice);
					module.DisplayGrid.SelectAllElements();
					var editMenuItem = module.FormActionMenu.FindByText("Edit");
					editMenuItem.PerformClick();
					AssertEquals("Error should be shown", "The selected transaction is no longer valid. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOtherCompanyTransactionAreNotVisibleInSearchResults()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var incompleteInvoiceInCurrentCompany = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001", testObjectCreator.AUD, 1m, 1m, 0m, 1m, 0m);
			incompleteInvoiceInCurrentCompany.SaveAsIncomplete();

			InvoicingBase incompleteInvoiceInOtherCompany;
			using (testObjectCreator.SwitchEnvToCompany(testObjectCreator.NonCurrentCompany))
			{
				incompleteInvoiceInOtherCompany = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00002", testObjectCreator.AUD, 1m, 2m, 0m, 2m, 0m);
				incompleteInvoiceInOtherCompany.SaveAsIncomplete();
			}

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertEquals("Precondition: no active filters", 0, module.FilterBusinessObject.ActiveModuleFilters.Count);

				module.PerformSearch_ForTest();

				var actualPks = module.GridCollection.Cast<IBusiness>().Select(x => x.Identifier);
				var expectedPks = new[] { incompleteInvoiceInCurrentCompany.PK };
				AssertContainsExactElementsInAnyOrder("Search must only show invoices from current login company", expectedPks, actualPks);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APIncompleteInvoices;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);
			collection.AddRange(GetBusinessObjectsToGetControllersFor());
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.SaveAsIncomplete();
			return new BusinessObject[] { invoice };
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return GetBusinessObjectsToGetControllersFor().First();
		}
	}
}
