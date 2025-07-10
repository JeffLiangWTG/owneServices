using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.GUI.DataExport;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.DirectDebitBatch.Testing
{
	[TestedType(typeof(DirectDebitBatchForm))]
	sealed class DirectDebitBatchFormTest : AccountingZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DirectDebitBatchForm(Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader);
		}

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Delete;
				AssertEquals("Verb should be 'Cancel'", "Cancel", testForm.FormVerb);
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'new'", "New", testForm.FormVerb);
			}
		}
		public void TestTotalAmountFieldVisibility()
		{
			var testHeader = Factory.New<DirectDebitBatchHeader>();
			testHeader.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			using (var testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				var invoiceAmountCalcFind = testForm.Controls.Find("InvoiceAmountCalcFind", true).FirstOrDefault();
				AssertNotNull("totalAmountTextBox", invoiceAmountCalcFind);

				Assert("Precondition : Batch is in local currency", testHeader.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("Total Amount field should not be visible when batch is in local currency", !invoiceAmountCalcFind.Visible);

				testHeader.AH_AB = TestObjectCreator.USDBankAccount.PK;
				Assert("Precondition : Batch is in foreign currency", testHeader.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("Total Amount field should be visible when batch is in foreign currency", invoiceAmountCalcFind.Visible);
			}
		}

		public void TestMessageRaisedIfNoRowsSelected()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				testHeader.AH_AB = DDRBankAccount.PK;

				AssertEquals("No Transaction is selected for DDR", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNoMessageRaisedIfRowsSelected()
		{
			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 0, "");

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				testHeader.AH_AB = DDRBankAccount.PK;
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOrganisationFormPopUp()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_TransactionNum = "Batch001";
			testHeader.AH_AB = TestObjectCreator.AUD.PK;

			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_ReceiptBatchNo = testHeader.AH_TransactionNum;
			testPayment.AH_AB = TestObjectCreator.AUD.PK;
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;

			testHeader.Lines.Add(testPayment);

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testForm.DisplayMode = ODisplayMode.Browse;
				testForm.Show();

				using (ZOrganisationsForm testOrgForm = testForm.ShowOrganisationFormToEdit_ForTestOnly())
				{
					AssertNotNull(testOrgForm);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportWizardPerformance()
		{
			var testHeader = CreateValidDDRBatch();
			Factory.Save();

			IExportCollectionInfo collectionInfo;
			using (var adapter = new DirectDebitBatchDataExportAdapter(Factory, testHeader))
			{
				adapter.Sort(testHeader);
				collectionInfo = adapter.GetMultiTypeCollectionInfo(testHeader);
			}

			using (DataExportWizardForm form = new DataExportWizardForm(collectionInfo, "DDRBatch"))
			{
				ExportWizard exportWizard = form.BusinessEntity as ExportWizard;
				string filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\DataExport\Settings\GenericTest.xml");
				exportWizard.Setting = "GENERICTEST";
				exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
				AssertTableHitCount("Expect 4 db hit to AccTransactionHeader table", 4, AccTransactionHeaderSchema.Constants.TableName, Factory);
			}
		}

		public void TestCancelledLabelShownIfCancelled()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_IsCancelled = ZBool.True;

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testForm.DisplayMode = ODisplayMode.Browse;
				testForm.Show();

				Assert(testForm.CancelledBatchLabel_ForTestOnly.Visible);
			}
		}

		public void TestCancelledLabelNotShownIfNotCancelled()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_IsCancelled = ZBool.False;

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testForm.DisplayMode = ODisplayMode.Browse;
				testForm.Show();

				Assert(!testForm.CancelledBatchLabel_ForTestOnly.Visible);
			}
		}

		public void TestCancelledLabelNotShownIfFormIsCancelling()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_IsCancelled = ZBool.False;

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();

				Assert(!testForm.CancelledBatchLabel_ForTestOnly.Visible);
			}
		}

		public void TestPostDateIsReadOnlyIfFormIsCancelling()
		{
			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_IsCancelled = ZBool.False;

			using (var testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();

				Assert(testForm.PostDateDateEdit_ForTestOnly.ReadOnly);
			}
		}

		public void TestRightClickMenu()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				ContextMenu menuBefore = testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu;
				int menuCount = menuBefore.MenuItems.Count + 1; // 1 additional menu item for grid colors
				testForm.Show();
				AssertEquals(2, testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu.MenuItems.Count - menuCount);
				MenuItem viewTransactionMenu = testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu.MenuItems[menuCount];
				MenuItem editOrgsMenu = testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu.MenuItems[menuCount + 1];

				AssertEquals(testForm.ViewTransactionMenuItemText_ForTestOnly, viewTransactionMenu.Text);
				AssertEquals(testForm.OrgEditMenuItemText_ForTestOnly, editOrgsMenu.Text);
			}
		}

		public void TestMenuItemDisabledForDirectPayment()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_TransactionNum = "Batch001";

			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testPayment.AH_ReceiptBatchNo = testHeader.AH_TransactionNum;
			testPayment.AH_AB = TestObjectCreator.AUD.PK;

			DirectPayment testDirectPayment = Factory.NewWithValidTestData(typeof(DirectPayment)) as DirectPayment;
			testDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			testDirectPayment.AH_ReceiptBatchNo = testHeader.AH_TransactionNum;
			testDirectPayment.AH_AB = TestObjectCreator.AUD.PK;

			testHeader.AH_AB = TestObjectCreator.AUD.PK;

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				testForm.SetEditOrgMenuItemReadOnly_ForTestOnly(testDirectPayment);
				Assert(!testForm.OrgEditMenuItem_ForTestOnly.Enabled);
				Assert(!testForm.ViewTransactionMenu_ForTestOnly.Enabled);

				testForm.SetEditOrgMenuItemReadOnly_ForTestOnly(testPayment);
				Assert(testForm.OrgEditMenuItem_ForTestOnly.Enabled);
				Assert(testForm.ViewTransactionMenu_ForTestOnly.Enabled);
			}
		}

		public void TestRemoveDocumentsMenu()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu.MenuItems.Add("Documents");
				testForm.ContextMenu_Popup_ForTestOnly(this, null);
				bool iSDocumentsExist = false;
				for (int i = 0; i < testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu.MenuItems.Count; i++)
				{
					if (testForm.DepositBatchLineGrid_ForTestOnly.ContextMenu.MenuItems[i].Text == "Documents")
					{
						iSDocumentsExist = true;
					}
				}
				Assert("Menu Item 'Documents' must be deleted", !iSDocumentsExist);
			}
		}

		public override void TestPrevAndNextButton()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.OnLoad_ForTestOnly(new EventArgs());
				Assert(testForm.AutoAddPreviousNextButtons);
			}
		}

		public void TestGetSaveDialogFilter()
		{
			DirectDebitBatchHeader dDRHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm form = new DirectDebitBatchForm(dDRHeader))
			{
				AssertEquals("File format should be ABA", "ABA Files (*.ABA)|.ABA", form.GetSaveDialogFilter_ForTestOnly(Constants.DDRFileFormat.BBL));
				AssertEquals("File format should be BCS", "BCS Files (*.BCS)|.BCS", form.GetSaveDialogFilter_ForTestOnly(Constants.DDRFileFormat.BCS));
			}
		}

		public void TestGridIsSortable()
		{
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				AssertEquals(true, testForm.DepositBatchLineGrid_ForTestOnly.AllowSorting);
			}

			var directPayment = TestObjectCreator.CreateDirectPayment(DateTime.Today, 100, 0, 0, 0);
			testHeader.Lines.Add(directPayment);

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				AssertEquals(true, testForm.DepositBatchLineGrid_ForTestOnly.AllowSorting);
			}
		}

		public void TestGridContainAccDetailColumns()
		{
			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				Assert("Should have AccountCurrency column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("AccountCurrency"));
				Assert("Should have BankName column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("PayeeBankName"));
				Assert("Should have IBANNumber column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("PayeeIBANNumber"));
				Assert("Should have CountryCode column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("PayeeCountryCode"));
				Assert("Should have BankSwift column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("PayeeBankSwift"));
				Assert("Should have BankSwift column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("PayeeBankBSB"));
				Assert("Should have BankSwift column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("AccountTitle"));
				Assert("Should have BankSwift column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("PayeeBankAccountNumber"));
				Assert("Should have Bank Created By column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("BankCreateUser"));
				Assert("Should have Bank Created Time column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("BankCreateTimeLocal"));
				Assert("Should have Bank Last Edit column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("BankLastEditUser"));
				Assert("Should have Bank Last Edited Time column", testForm.DepositBatchLineGrid_ForTestOnly.Columns.Contains("BankLastEditTimeLocal"));

				Assert("AccountCurrency column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("AccountCurrency").IsVisible);
				Assert("PayeeBankName column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("PayeeBankName").IsVisible);
				Assert("PayeeIBANNumber column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("PayeeIBANNumber").IsVisible);
				Assert("PayeeCountryCode column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("PayeeCountryCode").IsVisible);
				Assert("PayeeBankSwift column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("PayeeBankSwift").IsVisible);
				Assert("PayeeBankBSB column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("PayeeBankBSB").IsVisible);
				Assert("AccountTitle column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("AccountTitle").IsVisible);
				Assert("PayeeBankAccountNumber column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("PayeeBankAccountNumber").IsVisible);
				Assert("Bank Created By column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("BankCreateUser").IsVisible);
				Assert("Bank Created Time column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("BankCreateTimeLocal").IsVisible);
				Assert("Bank Last Edit column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("BankLastEditUser").IsVisible);
				Assert("Bank Last Edited Time column should be visible.", testForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("BankLastEditTimeLocal").IsVisible);
			}
		}

		public void TestIncludeinthebatchColunmWhenNewAndView()
		{
			var testHeader = CreateValidDDRBatch();
			Assert(!testHeader.IsInDatabase);
			using (DirectDebitBatchForm testNewForm = new DirectDebitBatchForm(testHeader))
			{
				testNewForm.DisplayMode = ODisplayMode.New;
				testNewForm.Show();
				Assert("Include in the batch column should be visible.", testNewForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("IncludeInTheBatch").IsVisible);
			}

			testHeader.Factory.Save();
			Assert(testHeader.IsInDatabase);
			using (DirectDebitBatchForm testViewForm = new DirectDebitBatchForm(testHeader))
			{
				testViewForm.DisplayMode = ODisplayMode.Browse;
				testViewForm.Show();
				Assert("Include in the batch column should be invisible. ", !testViewForm.DepositBatchLineGrid_ForTestOnly.GetColumnStyle("IncludeInTheBatch").IsVisible);
			}
		}

		public void TestCustomizeExportMenuAndSecurityCheckPoint()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				Menu.MenuItemCollection actionsMenuItems = testForm.ActionsMenuItem_ForTestOnly.MenuItems;
				MenuItem customiseExportMenuItem = null;

				foreach (MenuItem item in actionsMenuItems)
				{
					if (item.Name == "CustomiseExport")
					{
						customiseExportMenuItem = item;
						break;
					}
				}

				AssertNotNull(customiseExportMenuItem);
				AssertEquals("Customize Export", customiseExportMenuItem.Text);

				Env.Security.AllowCustomizeExport.IsAllowed = false;
				customiseExportMenuItem.PerformClick();
				AssertEquals(Env.Security.AllowCustomizeExport.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.AllowCustomizeExport.IsAllowed = true;
				customiseExportMenuItem.PerformClick();
				AssertNotEquals(Env.Security.AllowCustomizeExport.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestValidationRunsOnLinesInDirectDebitBatchForm()
		{
			AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();
			Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccPeriodManagement previousOpenPeriod = periodManagementTestHelper.PreviousOpenPeriod;

			APPayment payment = Factory.New(typeof(APPayment)) as APPayment;
			SetUpDDRTransaction(payment, DDRBankAccount, 110m, 0, "");
			payment.AH_PostDate = ZDateTime.Today.AddDays(-5);
			Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.Remove(accountDetails);
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			Assert(!testHeader.IsInDatabase);
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				testForm.ValidateAndSave_ForTestOnly();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertNoErrors("There should be no errors as DDR Batch can have lines with post dates in past", testHeader.Lines[0].AH_PostDateInfo);
				AssertHasError(testHeader.Lines[0].AH_OHInfo, @"An AP Bank Account could not be found with currency AUD and payment type DDR for the payee AALSHI.

Please set up an AP Account for the organization AALSHI under the AP Details tab, by right-clicking this grid and selecting ""Edit Payment Organization Detail"", with the currency AUD and payment type of DDR.");
				TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.Add(accountDetails);
				testForm.ValidateAndSave_ForTestOnly();
				AssertNoErrors(testHeader);
			}

			TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.Remove(accountDetails);
			Assert(testHeader.IsInDatabaseIncludingChildren);
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.Show();
				testForm.SaveDDRFile_ForTestOnly(testHeader);

				AssertEquals("There are errors that need to be corrected before this DDR File can be generated.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertNoErrors("There should be no errors as DDR Batch can have lines with post dates in past", testHeader.Lines[0].AH_PostDateInfo);
				AssertHasError(testHeader.Lines[0].AH_OHInfo, @"An AP Bank Account could not be found with currency AUD and payment type DDR for the payee AALSHI.

Please set up an AP Account for the organization AALSHI under the AP Details tab, by right-clicking this grid and selecting ""Edit Payment Organization Detail"", with the currency AUD and payment type of DDR.");
				TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.Add(accountDetails);
				testForm.ValidateAndSave_ForTestOnly();
				AssertNoErrors(testHeader);

				try
				{
					testForm.SaveDDRFile_ForTestOnly(testHeader);
					Assert(File.Exists(Path.Combine(Env.TempPath, "TestDDRFile.aba")));
				}
				finally
				{
					DeleteIfExists(Path.Combine(Env.TempPath, "TestDDRFile.aba"));
				}

				AssertNoErrors(testHeader);
			}
		}

		public void TestSaveDDRFile_WhenDollarLengthIsGreaterThanTheExpectedDollarLength()
		{
			AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			OrgHeader testOrg = TestObjectCreator.AALSHI;
			APPayment aPPayment = TestObjectCreator.CreateAPPayment(2m, 500000000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);

			aPPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			aPPayment.AH_ExchangeRate = 2m;

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);

			try
			{
				using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
				{
					AssertNoExceptionThrown(() => testForm.SaveDDRFile_ForTestOnly(testHeader));
				}
			}
			finally
			{
				DeleteIfExists(Path.Combine(Env.TempPath, "TestDDRFile.aba"));
			}
		}

		public void TestSaveDDRFile_WhenFilePathIsEmpty()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			testBank.AB_AllowAutoDDR = true;

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = testBank.PK;
			setting.SD_Name = "DDRBatchExportSetting";

			var testOrg = TestObjectCreator.AALSHI;
			TestObjectCreator.AddAPBankAccountDetails(testOrg, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);
			var aPPayment = TestObjectCreator.CreateAPPayment(1m, 500000000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);

			AssertEquals("Preconditon", 1, testOrg.CompanyData.AccountDetailsCollection.Count);
			testOrg.CompanyData.AccountDetailsCollection[0].A1_BankBsb = "123-456";

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.FilePathIsEmpty_ForTestOnly = true;
				AssertNotEquals("Preconditon", Constants.DDRFileFormat.CUS, DDRBankAccount.AB_AutoDDRFormat);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => testForm.SaveDDRFile_ForTestOnly(testHeader));
				Assert(!File.Exists(Path.Combine(Env.TempPath, "TestDDRFile.aba")));
				AssertEquals("DDR File cannot be generated as a file path was not provided.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				testForm.FilePathIsEmpty_ForTestOnly = true;
				AssertEquals("Preconditon", Constants.DDRFileFormat.CUS, DDRBankAccount.AB_AutoDDRFormat);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => testForm.SaveDDRFile_ForTestOnly(testHeader));
				Assert(!File.Exists(Path.Combine(Env.TempPath, "TestDDRFile.csv")));
				AssertEquals("DDR File cannot be generated as a file path was not provided.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveDDRFile_PredefinedBankFormat_WhenIOException()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			testBank.AB_AllowAutoDDR = true;

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = testBank.PK;
			setting.SD_Name = "DDRBatchExportSetting";

			var testOrg = TestObjectCreator.AALSHI;
			TestObjectCreator.AddAPBankAccountDetails(testOrg, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);
			var aPPayment = TestObjectCreator.CreateAPPayment(1m, 500000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);
			testHeader.CreateFile_ErrorAction_ForTestOnly = (unmappedPath) =>
			{
				throw new IOException("Unexpected IO failure for testing; predefined bank format.");
			};

			AssertEquals("Preconditon", 1, testOrg.CompanyData.AccountDetailsCollection.Count);
			testOrg.CompanyData.AccountDetailsCollection[0].A1_BankBsb = "123-456";

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				var logCountBefore = testHeader.Logs.GetAllLogs().Count;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => testForm.SaveDDRFile_ForTestOnly(testHeader));

				Assert("DDR File should not be saved to disk after an IO exception.", !File.Exists(Path.Combine(Env.TempPath, "TestDDRFile.aba")));
				var logCountAfter = testHeader.Logs.GetAllLogs().Count;
				AssertEquals("No event Log should be created.", logCountBefore, logCountAfter);
				AssertEquals("eDoc of DDR file should not be attached.", 0, testHeader.DocManagerInfo.Files.Count);
				AssertEquals("An error message should be shown when IOException occurs", "Cannot write the file to the disk. Please check with your system administrator./r/n	Details: Unexpected IO failure for testing; predefined bank format.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveDDRFile_CustomBankFormatSavesCorrectly()
		{
			AssertDirectDebitBatchForCustomBankFormat(hasDDRBatchExportSetting: true);
			AssertTableHitCount("Expect 6 db hit to AccTransactionHeader table", 5, AccTransactionHeaderSchema.Constants.TableName, Factory);
		}

		public void TestSaveDDRFile_CustomBankFormatWithoutExportShowsValidationError()
		{
			AssertDirectDebitBatchForCustomBankFormat(hasDDRBatchExportSetting: false);
		}

		public void TestSaveDDRFile_ThrowsOnNull()
		{
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				AssertExceptionThrown<ArgumentNullException>(() => testForm.SaveDDRFile_ForTestOnly(null));
			}
		}

		void AssertDirectDebitBatchForCustomBankFormat(bool hasDDRBatchExportSetting)
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			testBank.AB_AllowAutoDDR = true;
			if (hasDDRBatchExportSetting)
			{
				var setting = Factory.NewWithValidTestData<StmData>();
				setting.SD_Owner = testBank.PK;
				setting.SD_Name = "DDRBatchExportSetting";
			}
			var testOrg = TestObjectCreator.AALSHI;
			TestObjectCreator.AddAPBankAccountDetails(testOrg, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);
			var aPPayment = TestObjectCreator.CreateAPPayment(1m, 500000000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);

			string testFilePath = null;
			try
			{
				using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
				{
#if WINZOR
					//Winzor FileMapper needs a form to work
					testForm.Show();
#endif
					testFilePath = Path.Combine(Env.TempPath, "TestDDRFile.csv");
					if (hasDDRBatchExportSetting)
					{
						var logCountBefore = testHeader.Logs.GetAllLogs().Count;
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testForm.SaveDDRFile_ForTestOnly(testHeader);
						Assert("Custom DDR File should be saved to disk.", File.Exists(testFilePath));
						AssertEquals("Export Adapter should show the final path, not the temp path.", "File successfully created at " + testFilePath, UnitTestUserNotification.Instance.LastMessage.Text);
						var logCountAfter = testHeader.Logs.GetAllLogs().Count;
						AssertGreaterThan("Event Log should be created.", logCountAfter, logCountBefore);
						AssertGreaterThan("eDoc of DDR file should be attached.", testHeader.DocManagerInfo.Files.Count, 0);

						var storageFile = (BusinessObject)testHeader.DocManagerInfo.Files[0];
						var storageFileAsAttachment = (Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile;
						var docType = (RefDocType)storageFile["DocType"];
						Assert("StorageFile should be saved", storageFile.IsInDatabase);
						AssertEquals("RefDocType should exist and be of the correct code", "DDR", docType.RT_DocType);
						AssertGreaterThan("StorageFile should be larger than zero bytes.", storageFileAsAttachment.FileSizeInBytes, 0);
						var expectedFileName = Path.GetFileName(testFilePath);
						AssertEquals("StorageFile Name be the test file name, without path.", expectedFileName, storageFileAsAttachment.FileName);
					}
					else
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testForm.SaveDDRFile_ForTestOnly(testHeader);
						Assert("Custom DDR File should NOT be saved to disk when there is an error.", !File.Exists(testFilePath));
						AssertEquals("Error message should be shown when no export is setup.", "You must configure an export for this bank account.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					}
				}
			}
			finally
			{
				DeleteIfExists(testFilePath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveDDRFile_CustomBankFormatWithFileNameExpression()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			testBank.AB_AllowAutoDDR = true;

			var testOrg = TestObjectCreator.AALSHI;
			TestObjectCreator.AddAPBankAccountDetails(testOrg, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);
			var aPPayment = TestObjectCreator.CreateAPPayment(1m, 500000000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);

			var adapter = new DirectDebitBatchDataExportAdapter(Factory, testHeader);
			adapter.Sort(testHeader);
			var info = adapter.GetType().GetProperty("ExportWizard", BindingFlags.NonPublic | BindingFlags.Instance);
			var exportWizard = (ExportWizard)info.GetValue(adapter, null);
			string filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\DataExport\Settings\yusen.xml");
			exportWizard.Setting = "YUSENTEST";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			exportWizard.FileNameExpression = "\"Test.txt\"";
			exportWizard.FileNameExpressionObject = testHeader;
			exportWizard.SaveSettings();
			ZBlob blob = ZBlob.FromAscii(exportWizard.Setting);

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = testBank.PK;
			setting.SD_Name = "DDRBatchExportSetting";
			setting.SD_BinaryValue = blob;

			string testFilePath = null;
			try
			{
				using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
				{
#if WINZOR
					//Winzor FileMapper needs a form to work
					testForm.Show();
#endif
					testFilePath = Path.Combine(new FileMapper().GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Test.txt");
					var logCountBefore = testHeader.Logs.GetAllLogs().Count;
					testForm.SaveDDRFile_ForTestOnly(testHeader);
					Assert("Custom DDR File should be saved to disk.", File.Exists(testFilePath));
					var logCountAfter = testHeader.Logs.GetAllLogs().Count;
					AssertGreaterThan("Event Log should be created.", logCountAfter, logCountBefore);
					AssertGreaterThan("eDoc of DDR file should be attached.", testHeader.DocManagerInfo.Files.Count, 0);

					var storageFile = (BusinessObject)testHeader.DocManagerInfo.Files[0];
					var storageFileAsAttachment = (Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile;
					var docType = (RefDocType)storageFile["DocType"];
					Assert("StorageFile should be saved", storageFile.IsInDatabase);
					AssertEquals("RefDocType should exist and be of the correct code", "DDR", docType.RT_DocType);
					AssertGreaterThan("StorageFile should be larger than zero bytes.", storageFileAsAttachment.FileSizeInBytes, 0);
					var expectedFileName = Path.GetFileName(testFilePath);
					AssertEquals("StorageFile Name be the test file name, without path.", expectedFileName, storageFileAsAttachment.FileName);
				}
			}
			finally
			{
				DeleteIfExists(testFilePath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveDDRFile_CustomBankFormat_WhenIOException()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupSinglePeriod(30, ZDateTime.Now.Date, ZDateTime.Now.Date.AddDays(30));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CUS;
			testBank.AB_AllowAutoDDR = true;

			var testOrg = TestObjectCreator.AALSHI;
			TestObjectCreator.AddAPBankAccountDetails(testOrg, ReceiptTypes.DirectDebit, TestObjectCreator.AUD);
			var aPPayment = TestObjectCreator.CreateAPPayment(1m, 500000000m, ZDateTime.Now.Date, ZDateTime.Now.Date, testOrg.PK, testBank.PK);
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = testBank.PK;
			aPPayment.AH_ChequeOrReference = "11";
			testHeader.Lines.Add(aPPayment);

			var adapter = new DirectDebitBatchDataExportAdapter(Factory, testHeader);
			adapter.Sort(testHeader);
			var info = adapter.GetType().GetProperty("ExportWizard", BindingFlags.NonPublic | BindingFlags.Instance);
			var exportWizard = (ExportWizard)info.GetValue(adapter, null);
			string filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\DataExport\Settings\yusen.xml");
			exportWizard.Setting = "YUSENTEST";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			exportWizard.FileNameExpression = "\"Test.txt\"";
			exportWizard.FileNameExpressionObject = testHeader;
			exportWizard.SaveSettings();
			ZBlob blob = ZBlob.FromAscii(exportWizard.Setting);

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = testBank.PK;
			setting.SD_Name = "DDRBatchExportSetting";
			setting.SD_BinaryValue = blob;

			string testFilePath = null;
			try
			{
				using (DirectDebitBatchForm testForm = new DirectDebitBatchForm_ForCustomIOExceptionTest(testHeader))
				{
#if WINZOR
					//Winzor FileMapper needs a form to work
					testForm.Show();
#endif
					testFilePath = Path.Combine(new FileMapper().GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Test.txt");
					var logCountBefore = testHeader.Logs.GetAllLogs().Count;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNoExceptionThrown(() => testForm.SaveDDRFile_ForTestOnly(testHeader));

					Assert("Custom DDR File should not be saved to disk after an IO exception.", !File.Exists(testFilePath));
					var logCountAfter = testHeader.Logs.GetAllLogs().Count;
					AssertEquals("No event Log should be created.", logCountBefore, logCountAfter);
					AssertEquals("eDoc of DDR file should not be attached.", 0, testHeader.DocManagerInfo.Files.Count);
					AssertEquals("An error message should be shown when IOException occurs", "Cannot write the file to the disk. Please check with your system administrator./r/n	Details: Unexpected IO failure for testing; custom bank format.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				DeleteIfExists(testFilePath);
			}
		}

		public void TestSaveDDRFileDialogPromptsCorrectly()
		{
			var testHeader = CreateValidDDRBatch();
			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Assert(!testHeader.IsInDatabase);
				testForm.ValidateAndSave_ForTestOnly();
				AssertEquals("Only First time save will prompt the 'Do you want to create DDR File?' messagebox", "Do you want to create DDR File?", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert(testHeader.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testHeader.AH_ChequeOrReference = "CHREF#1";
				testForm.ValidateAndSave_ForTestOnly();
				AssertNull("After First time save, 'Do you want to create DDR File?' messagebox shouldn't be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGenerateDDRFileButtonGetsEnabledCorrectly_WithRegenerateDirectDebitFileSecurityRight()
		{
			AssertGenerateDDRFileButtonEditability(true);
		}

		public void TestGenerateDDRFileButtonGetsEnabledCorrectly_WithoutRegenerateDirectDebitFileSecurityRight()
		{
			AssertGenerateDDRFileButtonEditability(false);
		}

		void AssertGenerateDDRFileButtonEditability(bool userHasRight)
		{
			Env.Security.RegenerateDirectDebitFile.IsAllowed = userHasRight;
			var testHeader = CreateValidDDRBatch();
			using (var testNewForm = new DirectDebitBatchForm(testHeader))
			{
				Assert(!testHeader.IsInDatabase);
				testNewForm.Show();
				Assert("GenerateDDRFileButton_ForTestOnly.Enabled for new DDR batch", !testNewForm.GenerateDDRFileButton_ForTestOnly.Enabled);

				testNewForm.ValidateAndSave_ForTestOnly();
				Assert(testHeader.IsInDatabase);
				AssertEquals("GenerateDDRFileButton_ForTestOnly.Enabled for new DDR batch, just posted", userHasRight, testNewForm.GenerateDDRFileButton_ForTestOnly.Enabled);
			}

			using (var testEditForm = new DirectDebitBatchForm(testHeader))
			{
				testEditForm.Show();
				Assert(testHeader.IsInDatabase);
				AssertEquals("GenerateDDRFileButton_ForTestOnly.Enabled when saved DDR batch is opened again for Edit", userHasRight, testEditForm.GenerateDDRFileButton_ForTestOnly.Enabled);
			}

			testHeader.AH_IsCancelled = true;
			testHeader.Factory.Save();
			using (var testCancelForm = new DirectDebitBatchForm(testHeader))
			{
				testCancelForm.Show();
				Assert(testHeader.IsInDatabase);
				Assert("GenerateDDRFileButton_ForTestOnly.Enabled for cancelled DDR batch", !testCancelForm.GenerateDDRFileButton_ForTestOnly.Enabled);
			}
		}

		public override void TestDeleteButtonText()
		{
			using (DirectDebitBatchForm testForm = (DirectDebitBatchForm)GetFormToBashCore())
			{
				AssertEquals("Delete Button Text when opening the form for cancelling DDR Batch", "&Cancel Batch", ZFormPostingButtonsStrategy.DeleteButtonText(testForm).Text);
			}
		}

		public void TestIncludeInbatchColumnBecomesReadOnlyCorrectly()
		{
			var testHeader = CreateValidDDRBatch();

			using (DirectDebitBatchForm testForm = new DirectDebitBatchForm(testHeader))
			{
				Assert(!testHeader.IsInDatabase);
				testForm.Show();
				AssertEquals("Number of TransactionLines before Saving", 3, testHeader.Lines.Count);
				Assert("'IncludeInTheBatch' column of Transaction Line 1 should not be read only before saving", !testHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);
				Assert("'IncludeInTheBatch' column of Transaction Line 2 should not be read only before saving", !testHeader.Lines[1].IncludeInTheBatchInfo.ReadOnly);
				Assert("'IncludeInTheBatch' column of Transaction Line 3 should not be read only before saving", !testHeader.Lines[2].IncludeInTheBatchInfo.ReadOnly);

				testHeader.Lines[1].IncludeInTheBatch = false;
				testHeader.Lines[2].IncludeInTheBatch = false;
				testForm.ValidateAndSave_ForTestOnly();

				AssertEquals("Number of TransactionLines After Saving", 1, testHeader.Lines.Count);
				Assert("'IncludeInTheBatch' column of Transaction Line 1 should be read only after saving", testHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);
			}

			using (DirectDebitBatchForm testEditForm = new DirectDebitBatchForm(testHeader))
			{
				testEditForm.Show();
				AssertEquals("Number of TransactionLines After Saving", 1, testHeader.Lines.Count);
				Assert("'IncludeInTheBatch' column of Transaction Line 1 should be read only after saving", testHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);
			}

			var testHeader1 = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader1.AH_AB = DDRBankAccount.PK;
			using (DirectDebitBatchForm testNewForm = new DirectDebitBatchForm(testHeader1))
			{
				testNewForm.Show();
				AssertEquals("Number of TransactionLines before Saving", 2, testHeader1.Lines.Count);
				Assert("'IncludeInTheBatch' column of Transaction Line 1 should not be read only before saving", !testHeader1.Lines[0].IncludeInTheBatchInfo.ReadOnly);
				Assert("'IncludeInTheBatch' column of Transaction Line 2 should not be read only before saving", !testHeader1.Lines[1].IncludeInTheBatchInfo.ReadOnly);
			}
		}

		#region Implementation

		protected override bool ShouldHaveAuditPlugIn => true;

		AccBankAccount fDDRBankAccount;
		AccBankAccount DDRBankAccount
		{
			get
			{
				if (fDDRBankAccount == null)
				{
					fDDRBankAccount = TestObjectCreator.AUDBankAccount;
					fDDRBankAccount.AB_AllowAutoDDR = ZBool.True;
				}
				return fDDRBankAccount;
			}
		}

		DirectDebitBatchHeader CreateValidDDRBatch()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();
			Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var previousOpenPeriod = periodManagementTestHelper.PreviousOpenPeriod;

			CreateAPPayment("121");
			CreateAPPayment("122");
			CreateAPPayment("123");
			Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.Remove(accountDetails);
			var testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_AB = DDRBankAccount.PK;
			TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.Add(accountDetails);
			return testHeader;
		}

		APPayment CreateAPPayment(ZString chequeOrReference)
		{
			var payment = Factory.New(typeof(APPayment)) as APPayment;
			SetUpDDRTransaction(payment, DDRBankAccount, 110m, 0, "", chequeOrReference);
			payment.AH_PostDate = ZDateTime.Today.AddDays(-5);
			return payment;
		}

		void SetUpDDRTransaction(TransactionHeader payment, AccBankAccount dDRBankAccount, decimal amount, decimal foreignAmount, ZString batchNo, string chequeOrReference = "123")
		{
			payment.AH_ExchangeRate = 1m;
			payment.AH_AB = dDRBankAccount.PK;

			payment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment.AH_GB = GlbBranch.CurrentBranch.PK;
			payment.AH_ReceiptBatchNo = batchNo;
			payment.AH_ChequeOrReference = chequeOrReference;
			payment.AH_OH = TestObjectCreator.AALSHI.PK;
			if (foreignAmount != 0m)
			{
				payment.AH_OSExTaxAmount = foreignAmount;
			}
			else
			{
				payment.AH_OSExTaxAmount = amount;
			}
			payment.AH_LocalExTaxAmount = amount;
		}

		class DirectDebitBatchForm_ForCustomIOExceptionTest : DirectDebitBatchForm
		{
			public DirectDebitBatchForm_ForCustomIOExceptionTest(DirectDebitBatchHeader header)
				: base(header)
			{
			}

			protected override DirectDebitBatchDataExportAdapter CreateDirectDebitBatchDataExportAdapter(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			{
				var result = base.CreateDirectDebitBatchDataExportAdapter(factory, header);
				result.CreateFile_ErrorAction_ForTestOnly = (unmappedPath) =>
				{
					throw new IOException("Unexpected IO failure for testing; custom bank format.");
				};
				return result;
			}
		}
		#endregion
	}
}
