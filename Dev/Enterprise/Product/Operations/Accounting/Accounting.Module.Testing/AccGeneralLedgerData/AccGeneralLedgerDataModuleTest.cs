using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AccGeneralLedgerDataModule))]
	class AccGeneralLedgerDataModuleTest : ZModuleBasherTest
	{
		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.AccGeneralLedgerData;

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override bool HasDefaultController()
		{
			return false;
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			return new BusinessObject[] { Factory.NewWithValidTestData<APInvoice>(),
												Factory.NewWithValidTestData<ARInvoice>(),
												Factory.NewWithValidTestData<ARCreditNote>(),
												Factory.NewWithValidTestData<APCreditNote>(),
												Factory.NewWithValidTestData<ARAdjustmentNote>(),
												Factory.NewWithValidTestData<APAdjustmentNote>(),
												Factory.NewWithValidTestData<ARJournal>(),
												Factory.NewWithValidTestData<APJournal>(),
												Factory.NewWithValidTestData<ARTransferFromRow>(),
												Factory.NewWithValidTestData<ARTransferToRow>(),
												Factory.NewWithValidTestData<APTransferFromRow>(),
												Factory.NewWithValidTestData<APTransferToRow>(),
												Factory.NewWithValidTestData<ARContraRow>(),
												Factory.NewWithValidTestData<APContraRow>(),
												Factory.NewWithValidTestData<AROverpayment>(),
												Factory.NewWithValidTestData<APOverpayment>(),
												Factory.NewWithValidTestData<ARDiscount>(),
												Factory.NewWithValidTestData<APDiscount>(),
												Factory.NewWithValidTestData<ARExchangeDifference>(),
												Factory.NewWithValidTestData<APExchangeDifference>(),
												Factory.NewWithValidTestData<ARReceipt>(),
												Factory.NewWithValidTestData<APReceipt>(),
												Factory.NewWithValidTestData<ARPayment>(),
												Factory.NewWithValidTestData<APPayment>(),
												Factory.NewWithValidTestData<DirectReceipt>(),
												Factory.NewWithValidTestData<DirectPayment>(),
												Factory.NewWithValidTestData<BankTransferFromRow>(),
												Factory.NewWithValidTestData<BankTransferToRow>(),
												Factory.NewWithValidTestData<CashbookExchangeDiff>(),
												Factory.NewWithValidTestData<OpeningPayment>(),
												Factory.NewWithValidTestData<OpeningReceipt>(),
												Factory.NewWithValidTestData<JobRevenueJournal>(),
												Factory.NewWithValidTestData<JCJournalHeader>(),
												Factory.NewWithValidTestData<GLJournal>(),
											};
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = factory.NewWithValidTestData<AccGeneralLedgerData>();
			return result;
		}

		public void TestLicenseCheckpoints()
		{
			using (var module = new AccCollectionBatchModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewController_Line()
		{
			AccountingLine(TransactionLineTypes.WIP, typeof(WIPController));
			AccountingLine(TransactionLineTypes.Accrual, typeof(AccrualController));

			void AccountingLine(string lineType, Type type)
			{
				var line = Factory.NewWithValidTestData<AccTransactionLines>();
				line.AL_LineType = lineType;

				generalLedgerData.GLD_AL_TransactionLine = line.PK;
				Factory.Save();

				using (var module = new AccGeneralLedgerDataModuleForTest())
				using (module.ShowViewFormForTest(generalLedgerData))
				{
					AssertEquals($"when GLD_AL_TransactionLine is valid and AL_LineType is {lineType}, ", type, module.controller.GetType());
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestGetNewController_GLD_AH_TransactionHeader_Ledger_Transaction()
		{
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, typeof(APInvoiceController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, typeof(APCreditNoteController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, typeof(APAdjustmentNoteController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.Journal, typeof(ARJournalController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.Transfer, typeof(ARTransferController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.Contra, typeof(ARContraController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.Overpayment, typeof(AROverpaymentController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.Discount, typeof(ARDiscountController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.ExchangeDifference, typeof(ARExchangeDifferenceController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, typeof(ZARReceiptController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, typeof(ZAPPaymentController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, typeof(DirectReceiptController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, typeof(DirectPaymentController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.CashBook, TransactionTypes.Transfer, typeof(BankTransferController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.CashBook, TransactionTypes.ExchangeDifference, typeof(BankCurrencyAdjustmentController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal, typeof(JobRevenueJournalController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.JobCosting, TransactionTypes.Journal, typeof(JCJournalController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.General, TransactionTypes.GLAutoJournal, typeof(GLJournalController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.General, TransactionTypes.GLStandardJournal, typeof(GLJournalController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.General, TransactionTypes.GLReversingJournal, typeof(GLJournalController));
			AccountingHeader_Ledger_Transaction(LedgerTypes.General, TransactionTypes.GLNoteJournal, typeof(GLJournalController));

			void AccountingHeader_Ledger_Transaction(string ledgerType, string transactionType, Type type)
			{
				var header = Factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_Ledger = ledgerType;
				header.AH_TransactionType = transactionType;

				generalLedgerData.GLD_AH_TransactionHeader = header.PK;
				Factory.Save();

				using (var module = new AccGeneralLedgerDataModuleForTest())
				using (module.ShowViewFormForTest(generalLedgerData))
				{
					AssertEquals($"when GLD_AH_TransactionHeader is valid, AH_Ledger is {ledgerType}, TransactionType is {transactionType}, ", type, module.controller.GetType());
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestGetNewController_GLD_YC_CashBasisVAT()
		{
			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			invoice.AH_FullyPaidDate = ZDateTime.Today;
			var line = invoice.Lines[0];
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			var cashVATRecord = TestObjectCreator.CreateCashBasisVAT(invoice.Lines[0], -100, -10);
			generalLedgerData.GLD_YC_CashBasisVAT = cashVATRecord.PK;
			Factory.Save();
			using (var module = new AccGeneralLedgerDataModuleForTest())
			using (module.ShowViewFormForTest(generalLedgerData))
			{
				AssertEquals("when GLD_YC_CashBasisVAT is valid, ", typeof(APInvoiceController), module.controller.GetType());
			}
		}

		[TestDate(2023, 05, 12)]
		public void TestGetNewController_GLD_ATM_TaxGLMovement()
		{
			var osTaxAmount = 100.0m;
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var objForTest = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			var taxTransaction = Factory.New<AccTaxTransaction>();
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			taxTransaction.ATT_ETC = taxConfiguration.PK;
			taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
			taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			taxTransaction.ATT_Ledger = LedgerTypes.AccountsReceivable;
			taxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			taxTransaction.ATT_RX_NKOSTaxCurrency = "AUD";
			taxTransaction.ATT_OSTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxBaseAmount = osTaxAmount;
			taxTransaction.ATT_OSTaxAmount = osTaxAmount;
			taxTransaction.ATT_LocalTaxAmount = osTaxAmount;
			taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;
			taxTransaction.ATT_Rate = 0.1274m;

			var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_ATT = taxTransaction.PK;
			pivot.FillWithValidTestData();

			taxTransaction.ATT_PostDate = ZDate.Today;
			taxTransaction.ATT_TaxDate = ZDate.Today;
			taxTransaction.ATT_TaxSystemCode = "DNC";
			taxTransaction.ATT_AH = ((ITaxRecordParentBase)objForTest).PK;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxTransaction.ATT_AT_TaxID = taxRate.PK;

			Factory.Save();

			var taxGLMovement = Factory.New<AccTaxGLMovement>();

			taxGLMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
			taxGLMovement.ATM_Period = 202301;
			taxGLMovement.ATM_Date = ZDate.Today;
			taxGLMovement.ATM_Amount = 100m;
			taxGLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;

			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();

			taxGLMovement.ATM_AG_DebitAccount = glHeader1.PK;
			taxGLMovement.ATM_AG_CreditAccount = glHeader2.PK;

			Factory.Save();

			generalLedgerData.GLD_ATM_TaxGLMovement = taxGLMovement.PK;
			Factory.Save();

			using (var module = new AccGeneralLedgerDataModuleForTest())
			using (module.ShowViewFormForTest(generalLedgerData))
			{
				AssertEquals("when GLD_ATM_TaxGLMovement is valid, ", typeof(ARInvoiceController), module.controller.GetType());
			}
		}

		public void TestAllowedActions()
		{
			Assert(Module.HasActions);
			Assert(Module.AllowView);
			Assert(!Module.AllowNew);
			Assert(!Module.AllowEdit);
			Assert(!Module.AllowDelete);
			Assert(!Module.AllowUniversalCopy);
			Assert(!Module.CouldAllowUniversalCopy);
			Assert(!Module.AllowCopyFilterGridHyperlinkToClipboard);
			using (var module = new AccGeneralLedgerDataModule())
			{
				Assert(module.ModuleDecisionProvider.AllowExcelExport);
			}
		}

		#region TestPerformSearch

		public void TestPerformSearch()
		{
			var glbCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			glbCompany.GC_SystemCreateUser = "GLD";

			Factory.Save();

			TestConnection.ExecuteNonQuery(Sql);

			Module.PerformSearch_ForTest();
			AssertEquals("Grid Collection should contain 1 element", 1,
				((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);
		}

		#endregion

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AccountingJournals, Module.SecurityCheckpoint);
		}

		public void TestPrintAccountingJournalButtons()
		{
			var buttons = Module.ToolBarButtons;
			var printButton = buttons.FindByText("Print");
			AssertNotNull("There should be a Button named Print", printButton);
			AssertNotNull("The Print button should have a dropdown menu", printButton.DropDownMenu);
			AssertNotNull("There should be a suboption called 'Print Accounting Journal'", printButton.DropDownMenu.MenuItems.FindByText("Print Accounting Journal"));
		}

		[TestDate(2023, 5, 18)]
		public void TestPrintAccountingJournalForINV()
		{
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());

			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			var postDate = ZDateTime.Now;

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			arInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			arInvoice1.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvoice1.Lines[0].AL_PostDate = postDate;
			arInvoice1.Lines[0].AL_ReverseDate = postDate;
			arInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;

			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			arInvoice2.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			arInvoice2.Lines[0].AL_PostDate = postDate;
			arInvoice2.Lines[0].AL_ReverseDate = postDate;
			arInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;

			var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
			apInvoice1.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;
			apInvoice1.Lines[0].AL_PostDate = postDate;
			apInvoice1.Lines[0].AL_ReverseDate = postDate;
			apInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;

			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("2", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
			apInvoice2.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;
			apInvoice2.Lines[0].AL_PostDate = postDate;
			apInvoice2.Lines[0].AL_ReverseDate = postDate;
			apInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;

			Factory.Save();

			((INeedRow)arInvoice1.Lines[0]).Row.SetAdded();
			((INeedRow)arInvoice2.Lines[0]).Row.SetAdded();
			((INeedRow)apInvoice1.Lines[0]).Row.SetAdded();
			((INeedRow)apInvoice2.Lines[0]).Row.SetAdded();

			AssertPrintAccountingJournal(4, new[] { ((INeedRow)arInvoice1.Lines[0]).Row, ((INeedRow)arInvoice2.Lines[0]).Row, ((INeedRow)apInvoice1.Lines[0]).Row, ((INeedRow)apInvoice2.Lines[0]).Row });
		}

		[TestDate(2023, 5, 18)]
		public void TestPrintAccountingJournalForCB()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			var postDate = ZDateTime.Now;
			var directReceipt = TestObjectCreator.CreateDirectReceipt(postDate, 10m, 2m, 10, 2m);

			Factory.Save();

			AssertPrintAccountingJournal(1, new[] { ((INeedRow)directReceipt.Lines[0]).Row });
		}

		[TestDate(2023, 5, 18)]
		public void TestPrintAccountingJournalForGL()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			var glJournal = TestObjectCreator.CreateGLJournal("GJL", ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(glJournal, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			Factory.Save();

			AssertPrintAccountingJournal(1, new[] { ((INeedRow)line1).Row, ((INeedRow)line2).Row });
		}

		[TestDate(2023, 5, 18)]
		public void TestPrintAccountingJournalForJRJ()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			Factory.Save();

			var postDate = ZDateTime.Now;
			var journal1 = TestObjectCreator.CreateJCJournalHeader(postDate, 20m);
			var line1 = TestObjectCreator.CreateJCJournalLine(journal1, TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job1, postDate, 20m);

			TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.RevenueChargeCode, TestObjectCreator.Job2, 20m);

			Factory.Save();

			((INeedRow)line1).Row.SetAdded();

			AssertPrintAccountingJournal(1, new[] { ((INeedRow)line1).Row });
		}

		[TestDate(2023, 5, 18)]
		public void TestPrintAccountingJournalForWIPAccrual()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			Factory.Save();

			var job = TestObjectCreator.CreateJob("S00001222", TestObjectCreator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;

			var wip = TestObjectCreator.CreateWIP(charge1);
			var accrual = TestObjectCreator.CreateAccrual(charge1);

			Factory.Save();

			((INeedRow)wip).Row.SetAdded();
			((INeedRow)accrual).Row.SetAdded();

			AssertPrintAccountingJournal(2, new[] { ((INeedRow)wip).Row, ((INeedRow)accrual).Row });
		}

		void AssertPrintAccountingJournal(int printCount, DataRow[] gLDDataSources)
		{
			TestObjectCreator.MockNudgeGLDProcessData(gLDDataSources);

			using (var testModule = Module)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();

					testModule.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					if (printCount == 1)
					{
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals("There are " + printCount + " accounting journals to print. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestPrintAccountingJournal_PromptMessage()
		{
			using (var testModule = Module)
			{
				using (var form = new ZForm())
				{
					var creator = new TestObjectCreator(Factory);
					testModule.PerformSearch_ForTest();

					var testCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals(0, testCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrintAccountingJournal_ForTestOnly(null, new EventArgs());
					AssertEquals("Please select transaction(s) or Journal(s) to print.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestActions_ShouldHaveExportAllColumnsToExcel()
		{
			TestExportToExcel_WithoutShowingRecords("Export All Columns To Excel");
		}

		public void TestActions_ShouldHaveExportVisibleColumnsToExcel()
		{
			TestExportToExcel_WithoutShowingRecords("Export Visible Columns To Excel");
		}

		public void TestExportToExcel_WithoutShowingRecords(string exportActionName)
		{
			var testInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			testInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			using (ZForm zForm = new ZForm())
			{
				var gridCollection = new TransactionHeaderCollection(Factory);
				var filteredtransactionHeadercollection = new FilteredTransactionHeaderCollectionView(gridCollection);

				var filterBizObj = new APTransactionFilterStripBusinessObject();

				TransactionFilterStripControl filterControl = new TransactionFilterStripControl(filteredtransactionHeadercollection, filterBizObj);

				using (ARTransactionModuleStrip module = new ARTransactionModuleStrip())
				{
					filterControl.Grid.SetParentFilterGridModule(module);

					filterControl.FirePerformSearch();
					zForm.Controls.Add(filterControl);
					zForm.Show();

					try
					{
						AssertNoExceptionThrown(delegate
						{ filterControl.Grid.ContextMenu.MenuItems.FindByText(exportActionName).PerformClick(); });
					}
					finally
					{
						DeleteIfExists(Enterprise.ZArchitecture.Excel.ExcelExporter.LastExportedFileNameStaticForTest);
					}
				}
			}
		}

		public void TestPrintReportingBookAccountingJournalButtons()
		{
			var chart1 = TestObjectCreator.CreateAlternateChart("1", "4", false, true);
			var chart2 = TestObjectCreator.CreateAlternateChart("2", "4", false, false);
			Factory.Save();

			var reportingBook1 = TestObjectCreator.CreateReportingBook("1", "1", chart1.PK, "UUU");
			var reportingBook2 = TestObjectCreator.CreateReportingBook("2", "1", chart2.PK, "DDD");

			Factory.Save();

			var reportingBookAccountingJournalPrintOptionCollection = new ReportingBookAccountingJournalPrintOptionCollection();
			var reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook1.PK;
			reportingBookAccountingJournalPrintOption.Default = true;

			reportingBookAccountingJournalPrintOption = reportingBookAccountingJournalPrintOptionCollection.AddNew();
			reportingBookAccountingJournalPrintOption.ReportingBook = reportingBook2.PK;

			AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reportingBookAccountingJournalPrintOptionCollection);

			var buttons = Module.ToolBarButtons;
			var printButton = buttons.FindByText("Print");
			AssertNotNull("There should be a Button named Print", printButton);
			AssertNotNull("The Print button should have a dropdown menu", printButton.DropDownMenu);

			var printReportingBookAccountingJournalMenuItem = printButton.DropDownMenu.MenuItems.FindByText("Print Reporting Book Accounting Journal");
			AssertNotNull("There should be a suboption called 'Print Reporting Book Accounting Journal'", printReportingBookAccountingJournalMenuItem);
			AssertNotNull("There should be a suboption called 'Print Reporting Book Accounting Journal'", printReportingBookAccountingJournalMenuItem.MenuItems.FindByText("1"));
			AssertNotNull("There should be a suboption called 'Print Reporting Book Accounting Journal'", printReportingBookAccountingJournalMenuItem.MenuItems.FindByText("2"));
		}

		#region Implementation

		AccGeneralLedgerDataModuleForTest Module;
		AccGeneralLedgerData generalLedgerData;

		protected override void SetUp()
		{
			base.SetUp();
			Module = new AccGeneralLedgerDataModuleForTest();

			generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			generalLedgerData.GLD_Type = "PST";
			generalLedgerData.GLD_GLAccountType = "ARC";
			generalLedgerData.GLD_PostPeriod = 1;
			generalLedgerData.GLD_PostDate = ZDateTime.Today;

			TestObjectCreator.SetupCashBasisVAT();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion

		class AccGeneralLedgerDataModuleForTest : AccGeneralLedgerDataModule
		{
			public MenuItem[] GetNewActionMenuItems_ForTestOnly()
			{
				return GetNewActionMenuItems();
			}

			public void HandlePrintAccountingJournal_ForTestOnly(object sender, EventArgs e)
			{
				HandlePrintAccountingJournal(sender, e);
			}

			public IBusinessObjectCollection GetNewGridCollection_ForTestOnly()
			{
				return GetNewGridCollection();
			}

			public AccGeneralLedgerDataModuleForTest()
			{
			}
			public ZController controller;

			protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				controller = base.GetNewController(selectedBusinessObject);
				return controller;
			}

			public IZForm ShowViewFormForTest(BusinessObject selectedBusinessObject)
			{
				return base.ShowViewForm(selectedBusinessObject);
			}
		}

		const string Sql = @"
declare @bdate smalldatetime, @edate smalldatetime
set @bdate = '2010-04-07 09:28:00'
set @edate = '2023-02-07 09:28:00'
declare @companyPK UNIQUEIDENTIFIER
declare @accountPK UNIQUEIDENTIFIER
declare @branchPK UNIQUEIDENTIFIER
declare @departmentPK UNIQUEIDENTIFIER
declare @chargeCodePK UNIQUEIDENTIFIER
declare @jobPK UNIQUEIDENTIFIER

Begin 
   SELECT @companyPK = (select TOP 1 GC_PK FROM dbo.GLBCompany WHERE GC_SystemCreateUser = 'GLD' ORDER BY NEWID());
   SELECT @accountPK = (select TOP 1 AG_PK FROM dbo.AccGLHeader ORDER BY NEWID());
   SELECT @branchPK = (select TOP 1 GB_PK FROM dbo.GLBBranch ORDER BY NEWID());
   SELECT @departmentPK = (select TOP 1 GE_PK FROM dbo.GLBDepartment ORDER BY NEWID());
   SELECT @chargeCodePK = (select TOP 1 AC_PK FROM dbo.AccChargeCode ORDER BY NEWID());
   SELECT @jobPK = (select TOP 1 JH_PK FROM dbo.JobHeader ORDER BY NEWID());
   Insert Into dbo.AccGeneralLedgerData 
   (
   GLD_PK,
   GLD_GC_Company, 
   GLD_PostDate, 
   GLD_AG_GLAccount,
   GLD_OSDebitAmount,
   GLD_GB_Branch,
   GLD_GE_Department,
   GLD_SystemCreateTimeUtc,
   GLD_SystemLastEditTimeUtc,
   GLD_SystemCreateUser,
   GLD_SystemLastEditUser,
   GLD_PostPeriod,
   GLD_Type,
   GLD_GB_TaxBranch,
   GLD_GLAccountType
	)
   Values  (
			NEWID(),
			@companyPK,
			@edate,
			@accountPK,
			5000,
			@branchPK,
			@departmentPK,
			@edate,
			@edate,
			'~BP',
			'~BP',
			1,
			'PST',
            @branchPK,
			'ARC'
            )
End
";
	}
}
