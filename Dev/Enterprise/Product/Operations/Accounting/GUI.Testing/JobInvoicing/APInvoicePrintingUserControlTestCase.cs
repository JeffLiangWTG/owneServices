using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class APInvoicePrintingUserControlTestCase : TestCaseWithFactory
	{
		public void TestAmendingFormCreatedWhenUserAmendAPInvoice()
		{
			using (var form = new ZForm())
			using (var control = new APInvoicePrintingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 100.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
				var line = TestObjectCreator.CreateAPInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 100.00m);
				charge.JR_AL_APLine = line.PK;
				Factory.Save();

				var filter = new JobAPInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.APInvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.APInvoicesGrid.VisibleRowCount);

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.APInvoicesGrid.VisibleRowCount);

				control.APInvoicesGrid.Select(0);
				control.AmendWithCreditNote_ForTestOnly(form, new EventArgs());
				Assert("expect to display credit note form", control.AmendingForm_ForTestOnly is CreditNoteForm);
				control.AmendingForm_ForTestOnly.Dispose();
			}
		}

		public void TestAmendingFormNotCreatedWhenUserAmendReversedTransaction()
		{
			using (var form = new ZForm())
			using (var control = new APInvoicePrintingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = TestObjectCreator.CreateShipment("S0001");
				var job = TestObjectCreator.CreateJob(shipment);
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Description", TestObjectCreator.AUD, 100.00m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100.00m, TestObjectCreator.ABIGAS);

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestObjectCreator.AALSHI);
				var line = TestObjectCreator.CreateAPInvoiceLine(invoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Description", 100.00m);
				charge.JR_AL_APLine = line.PK;
				Factory.Save();

				var filter = new JobAPInvoicePrintingFilter(shipment, job.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.APInvoicesGrid.SelectAllElements();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 1, control.APInvoicesGrid.VisibleRowCount);

				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(invoice);
				reversing.Reverse();
				reversing.ReverseTransaction.TransactionNumber = "XYZ123";
				Factory.Save();

				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				Application.DoEvents();
				AssertEquals("SelectedRowCount", 2, control.APInvoicesGrid.VisibleRowCount);

				for (int i = 0; i < 2; i++)
				{
					control.APInvoicesGrid.Select(i);
					control.AmendWithCreditNote_ForTestOnly(form, new EventArgs());
					AssertNull(control.AmendingForm_ForTestOnly);
					if (control.SelectedTransactionInInvoicesGrid_ForTestOnly is APInvoice)
					{
						AssertEquals("LastMessage", "Cannot amend selected transaction as this has been reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals("LastMessage", "Only AP Invoice can be amended.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestAddressColumnPresent()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			using (ZForm form = new ZForm())
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var shipment = testObjectCreator.CreateShipment("S0001");
				var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("00004000", testObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, testObjectCreator.ABIGAS);
				var line = testObjectCreator.CreateAPInvoiceLine(invoice, testObjectCreator.Job1, testObjectCreator.RevenueNoTaxChargeCode, testObjectCreator.AUD, 1M, "Description", 100M);
				var charge = testObjectCreator.CreateCharge(testObjectCreator.Job1, testObjectCreator.RevenueNoTaxChargeCode, "Description", testObjectCreator.AUD, 100M, testObjectCreator.AALSHI, testObjectCreator.AUD, 0M, testObjectCreator.ABIGAS);

				charge.JR_AL_APLine = line.PK;
				Factory.Save();

				JobAPInvoicePrintingFilter filter = new JobAPInvoicePrintingFilter(shipment, testObjectCreator.Job1.PK);
				control.Bind(filter);

				Assert(control.APInvoicesGrid.Columns.Contains("DisplayInvoiceAddressOverride"));
				Assert(control.APInvoicesGrid.Columns.Contains("DisplayInvoiceContactOverride"));
			}
		}

		public void TestNoRecordSelected()
		{
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				Menu.MenuItemCollection menuItems = control.APInvoicesGrid.ContextMenu.MenuItems;
				var printMenuItem = menuItems.FindByText("Edit Requisition Date and Status");
				AssertNoExceptionThrown("No record selected", printMenuItem.PerformClick);
			}
		}

		public void TestInvoiceFilterObjectIsNotInitialised()
		{
			using (APInvoicePrintingUserControl aPControl = new APInvoicePrintingUserControl())
			{
				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in FindButton_Click_ForTestOnly()", () => { aPControl.FindButton_Click_ForTestOnly(aPControl, new EventArgs()); });
				AssertNoExceptionThrown("A null InvoiceFilterObject should be handled in ClearButton_Click_ForTestOnly()", () => { aPControl.ClearButton_Click_ForTestOnly(aPControl, new EventArgs()); });
			}
		}

		public void TestContextMenu()
		{
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				Menu.MenuItemCollection menuItems = control.APInvoicesGrid.ContextMenu.MenuItems;
				AssertEquals("&View", menuItems[0].Text);
				var printMenuItem = menuItems.FindByText(APInvoicePrintingUserControl.PrintMenuText);
				AssertNotNull(APInvoicePrintingUserControl.PrintMenuText + " is exist.", printMenuItem);
				AssertEquals(APInvoicePrintingUserControl.PrintTransactionMenuText_ForTestOnly, printMenuItem.MenuItems[0].Text);
				AssertEquals(APInvoicePrintingUserControl.PrintMatchDocMenuText_ForTestOnly, printMenuItem.MenuItems[1].Text);
				AssertEquals(APInvoicePrintingUserControl.PrintSelfBillingInvoiceMenuText_ForTestOnly, printMenuItem.MenuItems[2].Text);
			}
		}

		public void TestAllocateComplianceNumberMenuItemsShownForEnableComplianceDocumentModule()
		{
			var countrySupportComplianceSubtype = TestObjectCreator.CountriesSupportComplianceSubtype[0];

			AssertContainMenuItem(countrySupportComplianceSubtype, false, true);
			AssertContainMenuItem(countrySupportComplianceSubtype, true, false);
		}

		void AssertContainMenuItem(string country, bool enableComplianceDocumentModule, bool shouldContainAllocateComplianceNumber)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableComplianceDocumentModule))
			using (var filterControl = new APInvoicePrintingUserControl())
			{
				AssertEquals(shouldContainAllocateComplianceNumber, filterControl.APInvoicesGrid.ContextMenu.MenuItems.FindByText("Allocate Compliance Number") != null);
			}
		}

		public void TestAPAllocateComplianceNumberMenuItemsNotShownForChina()
		{
			var country = CountryCodes.China;

			AssertContainMenuItem(country, false, false);
			AssertContainMenuItem(country, true, false);
		}

		public void TestComplianceMenuItemsShownForSupportedCountries()
		{
			foreach (var country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (var filterControl = new APInvoicePrintingUserControl())
				{
					AssertNotNull(filterControl.APInvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number"));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (var filterControl = new APInvoicePrintingUserControl())
			{
				AssertNull(filterControl.APInvoicesGrid.ContextMenu.MenuItems.FindByText("Update Compliance Sub Type and/or Number"));
			}
		}

		public void TestWarningsForAllocateComplianceNumber()
		{
			APInvoicePrintingUserControl control = null;
			ZForm form = null;
			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
				{
					form = new ZForm();
					control = new APInvoicePrintingUserControl();
					form.Controls.Add(control);
					form.Show();

					var shipment = TestObjectCreator.CreateShipment("S0001");
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("00004000", TestObjectCreator.EUR, 1.0m, 1000m, 100m, 0m, 1000m, 100m, 0m, TestObjectCreator.ABIGAS);
					var line = TestObjectCreator.CreateAPInvoiceLine(invoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.EUR, 1.0m, "Description", 1000.00m);
					var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, "Description", TestObjectCreator.EUR, 1000m, TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1000m, TestObjectCreator.ABIGAS);
					charge.JR_AL_APLine = line.PK;
					Factory.Save();

					var filter = new JobAPInvoicePrintingFilter(shipment, TestObjectCreator.Job1.PK);
					control.Bind(filter);
					control.InvoiceFilterObject_ForTestOnly = filter;
					control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
					control.APInvoicesGrid.SelectAllElements();
					Application.DoEvents();
					AssertEquals("SelectedRowCount", 1, control.APInvoicesGrid.SelectedRowCount);

					control.UpdateComplianceNumber_ForTestOnly(control, EventArgs.Empty);

					AssertEquals("LastMessage", @"This function will only update transactions that have a Compliance Sub Type and do not already have a Compliance Number populated.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				form?.Dispose();
				control?.Dispose();
			}
		}

		public void TestComplianceSubTypeGridColumnOnlyShowForSupportedCountries()
		{
			foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (APInvoicePrintingUserControl filterControl = new APInvoicePrintingUserControl())
				{
					Assert("The Compliance SubType column should exists", ColumnExistsInTheGrid(filterControl.APInvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (APInvoicePrintingUserControl filterControl = new APInvoicePrintingUserControl())
			{
				Assert("The Compliance SubType column should be deleted for non-supported company", !ColumnExistsInTheGrid(filterControl.APInvoicesGrid, "AH_ComplianceSubType", ResourceStringData.Empty));
			}
		}

		public void TestTaxBranchColumnVisibilityDependsOnPresentationProvider()
		{
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			var presentationProviderMock = GetMockAPInvoicePrintingUserControlPresentationProvider();
			AssertTaxBranchColumnAppearance(true);
			AssertTaxBranchColumnAppearance(false);

			void AssertTaxBranchColumnAppearance(bool isTaxBranchColumnVisible)
			{
				presentationProviderMock.Setup(x => x.IsTaxBranchColumnAvailable()).Returns(isTaxBranchColumnVisible);
				using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
				{
					if (isTaxBranchColumnVisible)
					{
						Assert($"{taxBranchColumn} column should be available in the grid", ColumnExistsInTheGrid(control.APInvoicesGrid, taxBranchColumn, ResourceStringData.Empty));
					}
					else
					{
						Assert($"{taxBranchColumn} column should not be visible", !ColumnExistsInTheGrid(control.APInvoicesGrid, taxBranchColumn, ResourceStringData.Empty));
					}
				}
			}
		}

		public void TestTaxBranchColumnIsVisibilityDependsTaxBranchColumnAvailability()
		{
			var taxBranchColumn = TransactionHeader.Schema.AH_GB_TaxBranch;
			GetMockAPInvoicePrintingUserControlPresentationProvider().Setup(x => x.IsTaxBranchColumnAvailable()).Returns(true);
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				Assert($"{taxBranchColumn} column should be visible as default", control.APInvoicesGrid.GetColumnStyle(taxBranchColumn).IsVisible);
			}
		}

		public void TestBranchColumnExistForGrid()
		{
			var branchColumn = TransactionHeader.Schema.AH_GB;
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				Assert($"{branchColumn} column should be availbale in grid", ColumnExistsInTheGrid(control.APInvoicesGrid, branchColumn, ResourceStringData.Empty));
			}
		}

		public void TestBranchColumnVisibleAsDefaultForGrid()
		{
			var branchColumn = TransactionHeader.Schema.AH_GB;
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				Assert($"{branchColumn} column should be visible as default", control.APInvoicesGrid.GetColumnStyle(branchColumn).IsVisible);
			}
		}

		public void TestEditMenuItem()
		{
			using (ZForm form = new ZForm())
			using (APInvoicePrintingUserControl control = new APInvoicePrintingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

				var shipment = testObjectCreator.CreateShipment("S0001");
				var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("00004000", testObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, testObjectCreator.ABIGAS);
				var line = testObjectCreator.CreateAPInvoiceLine(invoice, testObjectCreator.Job1, testObjectCreator.RevenueNoTaxChargeCode, testObjectCreator.AUD, 1M, "Description", 100M);
				var charge = testObjectCreator.CreateCharge(testObjectCreator.Job1, testObjectCreator.RevenueNoTaxChargeCode, "Description", testObjectCreator.AUD, 100M, testObjectCreator.AALSHI, testObjectCreator.AUD, 0M, testObjectCreator.ABIGAS);

				charge.JR_AL_APLine = line.PK;
				Factory.Save();

				JobAPInvoicePrintingFilter filter = new JobAPInvoicePrintingFilter(shipment, testObjectCreator.Job1.PK);
				control.Bind(filter);
				control.InvoiceFilterObject_ForTestOnly = filter;
				control.InvoiceFilterObject_ForTestOnly.RefreshInvoiceList();
				control.APInvoicesGrid.SelectAllElements();
				Application.DoEvents();
				Factory.Save();

				control.APInvoicesGrid.Select(0);

				Menu.MenuItemCollection menuItems = control.APInvoicesGrid.ContextMenu.MenuItems;
				AssertEquals("&Edit", menuItems[1].Text);

				menuItems[1].PerformClick();

				AssertEquals(typeof(InvoiceForm), control.EditForm_ForTestOnly.GetType());

				AssertEquals(typeof(APInvoice), control.EditForm_ForTestOnly.BusinessEntity.GetType());

				AssertEquals(ODisplayMode.Browse, control.EditForm_ForTestOnly.DisplayMode);

				control.EditForm_ForTestOnly.Close();

				var oldSecurityValue = Env.Security.ViewPayablesTransaction.IsAllowed;

				Env.Security.ViewPayablesTransaction.IsAllowed = false;

				menuItems[1].PerformClick();

				AssertNull(control.EditForm_ForTestOnly);

				Env.Security.ViewPayablesTransaction.IsAllowed = oldSecurityValue;
			}
		}

		public void TestShowEInvoicingColumns()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.KoreaSouth);
			AssertShowEInvoicingColumns(false);
			AssertShowEInvoicingColumns(true);
		}

		void AssertShowEInvoicingColumns(bool enableEInvoicingFunctionalityForPayables)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableEInvoicingFunctionalityForPayables))
			using (var control = new APInvoicePrintingUserControl())
			{
				CombineAssertions("Column should be availbale in grid.", () =>
				{
					AssertEquals("EInvoicingBatchNumber", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingBatchNumber, ResourceStringData.Empty));
					AssertEquals("EInvoicingAuthorisationNumber", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingAuthorisationNumber, ResourceStringData.Empty));
					AssertEquals("EInvoicingeHubAllocatedNumber", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingeHubAllocatedNumber, ResourceStringData.Empty));
					AssertEquals("EInvoicingError", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingError, ResourceStringData.Empty));
					AssertEquals("EInvoicingGovernmentAllocatedNumber", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingGovernmentAllocatedNumber, ResourceStringData.Empty));
					AssertEquals("EInvoicingLastResponseReceivedUtcd", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingLastResponseReceivedUtc, ResourceStringData.Empty));
					AssertEquals("EInvoicingLastSentTimeUtc", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingLastSentTimeUtc, ResourceStringData.Empty));
					AssertEquals("EInvoicingStatusd", enableEInvoicingFunctionalityForPayables, ColumnExistsInTheGrid(control.APInvoicesGrid, TransactionHeader.Schema.EInvoicingStatus, ResourceStringData.Empty));
				});
			}
		}

		protected MenuItem[] Menu;

		protected APInvoicePrintingUserControl fTestInvoicePrintingModule;
		protected APInvoicePrintingUserControl TestInvoicePrintingModule
		{
			get { return fTestInvoicePrintingModule; }
		}
		public void TestOverrideTransactionDescription()
		{
			using (APInvoicePrintingUserControl filterControl = new APInvoicePrintingUserControl())
			{
				AssertNotNull("Menu item should exist", filterControl.APInvoicesGrid.ContextMenu.MenuItems.FindByText("Override Transaction Description"));
			}
		}
		ZBool ColumnExistsInTheGrid(ZGrid grid, ZString columnName, ResourceStringData groupName)
		{
			IEnumerator columnEnum = grid.ColumnStyles.GetEnumerator();
			while (columnEnum.MoveNext())
			{
				ZGridColumnInfo column = (ZGridColumnInfo)columnEnum.Current;
				if (column.ColumnName == columnName && (groupName.IsEmpty() || column.GroupName.Equals(groupName.Caption)))
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}

		Mock<IAPInvoicePrintingUserControlPresentationProvider> GetMockAPInvoicePrintingUserControlPresentationProvider()
		{
			var presentationProviderMock = new Mock<IAPInvoicePrintingUserControlPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();
			accountingPresentationProviderFactoryMock.Setup(x => x.GetAPJobInvoicePrintingUserControlPresentationProvider()).Returns(presentationProviderMock.Object);
			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);

			return presentationProviderMock;
		}
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
