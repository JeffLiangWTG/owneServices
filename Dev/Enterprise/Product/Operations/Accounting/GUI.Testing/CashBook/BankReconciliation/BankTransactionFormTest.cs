using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation.Testing
{
	[TestedType(typeof(BankTransactionForm))]
	public class BankTransactionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DirectTransactionsBusinessObject bizO = new DirectTransactionsBusinessObject(factory, ZGuid.Empty, ZDateTime.Empty);
			return new BankTransactionForm(bizO);
		}

		protected DirectTransactionsBusinessObject BizO;

		public void TestEditDirectPayment()
		{
			BizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			using (var form = new BankTransactionForm(BizO))
			{
				form.Show();
				Application.DoEvents();
				form.NewDirectPaymentButton_Click_ForTestOnly(null, null);
				var directPaymentsGrid = form.GetControl<ZGrid>("DirectPaymentsGrid");

				directPaymentsGrid.SelectAllElements();
				AssertEquals("Percondition", 1, directPaymentsGrid.SelectedElements.Length);

				directPaymentsGrid.ContextMenu.ShowPopupMenu();
				var editMenuItem = directPaymentsGrid.ContextMenu.MenuItems.FindByText("Edit");
				AssertNotNull("Should contain 'Edit' menu item", editMenuItem);

				editMenuItem.PerformClick();
				AssertType<BankReconDirectPaymentForm>(ZFormModaliser.LastFormShownForTest);

				var directPaymentInEditForm = (ZFormModaliser.ActiveForm as ZForm).BusinessEntity as BankReconDirectPayment;
				directPaymentInEditForm.AH_Desc = "Test Desc";
				ZFormModaliser.LastFormShownForTest.Close();
				AssertEquals("Test Desc", (form.BusinessEntity as DirectTransactionsBusinessObject).DirectPayments[0].AH_Desc);

				form.NewDirectPaymentButton_Click_ForTestOnly(null, null);
				directPaymentsGrid.SelectAllElements();
				AssertEquals("Percondition", 2, directPaymentsGrid.SelectedElements.Length);

				directPaymentsGrid.ContextMenu.ShowPopupMenu();
				editMenuItem = directPaymentsGrid.ContextMenu.MenuItems.FindByText("Edit");
				AssertNull("Should not contain 'Edit' menu item", editMenuItem);
			}
		}

		public void TestEditDirectReceipt()
		{
			BizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			using (var form = new BankTransactionForm(BizO))
			{
				form.Show();
				Application.DoEvents();
				form.NewDirectReceiptButton_Click_ForTestOnly(null, null);
				var directReceiptsGrid = form.GetControl<ZGrid>("DirectReceiptsGrid");

				directReceiptsGrid.SelectAllElements();
				AssertEquals("Percondition", 1, directReceiptsGrid.SelectedElements.Length);

				directReceiptsGrid.ContextMenu.ShowPopupMenu();
				var editMenuItem = directReceiptsGrid.ContextMenu.MenuItems.FindByText("Edit");
				AssertNotNull("Should contain 'Edit' menu item", editMenuItem);

				editMenuItem.PerformClick();
				AssertType<BankReconDirectReceiptForm>(ZFormModaliser.LastFormShownForTest);

				var directReceiptInEditForm = (ZFormModaliser.ActiveForm as ZForm).BusinessEntity as BankReconDirectReceipt;
				directReceiptInEditForm.AH_Desc = "Test Desc";
				ZFormModaliser.LastFormShownForTest.Close();
				AssertEquals("Test Desc", (form.BusinessEntity as DirectTransactionsBusinessObject).DirectReceipts[0].AH_Desc);

				form.NewDirectReceiptButton_Click_ForTestOnly(null, null);
				directReceiptsGrid.SelectAllElements();
				AssertEquals("Percondition", 2, directReceiptsGrid.SelectedElements.Length);

				directReceiptsGrid.ContextMenu.ShowPopupMenu();
				editMenuItem = directReceiptsGrid.ContextMenu.MenuItems.FindByText("Edit");
				AssertNull("Should not contain 'Edit' menu item", editMenuItem);
			}
		}

		public void TestShowBankTransactionForm_CopyTaxDate()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var bizO = new DirectTransactionsBusinessObject(new BusinessObjectFactory(), ZGuid.Empty, ZDateTime.Empty);
			var directPayment = factory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.AH_AB = factory.NewWithValidTestData<AccBankAccount>().PK;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_AT = testObjectCreator.GST1.PK;
			directPayment.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-11);
			var directReceipt = factory.NewWithValidTestData<BankReconDirectReceipt>();
			directReceipt.AH_AB = factory.NewWithValidTestData<AccBankAccount>().PK;
			directReceipt.Lines.AddNew(directReceipt.DependentTransactionLineType);
			directReceipt.Lines[0].AL_AT = testObjectCreator.GST1.PK;
			directReceipt.Lines[0].AL_TaxDate = ZDate.Today.AddDays(-31);
			bizO.Headers.Add(directPayment);
			bizO.Headers.Add(directReceipt);
			factory.Save();

			directReceipt.MakeDepositBatch();
			directReceipt.RelatedDepositBatch.IsSelected = true;
			factory.Save();

			var helper = new BankTransactionFormHelper(null, ZDateTime.Empty, bizO, false);
			helper.BankTransactionFormShown += Helper_BankTransactionFormShown;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			helper.ShowBankTransactionForm();

			AssertEquals("directPayment TaxDate", ZDate.Today.AddDays(-1), directPayment.Lines[0].AL_TaxDate);
			AssertEquals("directReceipt TaxDate", ZDate.Today.AddDays(-3), directReceipt.Lines[0].AL_TaxDate);

			void Helper_BankTransactionFormShown(object sender, EventArgs e)
			{
				var testForm = ZFormModaliser.LastFormShownDialogForTest as BankTransactionForm;
				testForm.TransactionsBizO_ForTestOnly.DirectPayments[0].Lines[0].AL_TaxDate = ZDate.Today.AddDays(-1);
				testForm.TransactionsBizO_ForTestOnly.DirectReceipts[0].Lines[0].AL_TaxDate = ZDate.Today.AddDays(-3);
				testForm.TransactionsBizO_ForTestOnly.MoveDirectReceiptsAndPaymentsToBaseCollection();
			}
		}

		public void TestShowAndCreateTransactions()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BizO = new DirectTransactionsBusinessObject(new BusinessObjectFactory(), ZGuid.Empty, ZDateTime.Empty);
			BankReconDirectPayment directPayment = factory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			BankReconDirectReceipt directReceipt = factory.NewWithValidTestData<BankReconDirectReceipt>();
			directReceipt.MakeDepositBatch();
			BizO.Headers.Add(directPayment);
			BizO.Headers.Add(directReceipt);

			BankTransactionFormHelper helper = new BankTransactionFormHelper(null, ZDateTime.Empty, BizO, false);
			helper.BankTransactionFormShown += new EventHandler(Helper_BankTransactionFormShown);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			helper.ShowBankTransactionForm();
			using (BankTransactionForm testForm = ZFormModaliser.LastFormShownDialogForTest as BankTransactionForm)
			{
				AssertNotNull(testForm);
				AssertEquals(2, testForm.TransactionsBizO_ForTestOnly.Headers.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectPayments.Count);
				Assert(testForm.TransactionsBizO_ForTestOnly.DirectPayments.Contains(directPayment));
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectReceipts.Count);
				Assert(testForm.TransactionsBizO_ForTestOnly.DirectReceipts.Contains(directReceipt));

				DirectDebitBatchHeader directDebitBatchHeader = null;
				if (BizO.Headers[0] is BankReconDirectPayment)
				{
					directDebitBatchHeader = ((BankReconDirectPayment)BizO.Headers[0]).RelatedDirectDebitBatch;
				}
				else if (BizO.Headers[1] is BankReconDirectPayment)
				{
					directDebitBatchHeader = ((BankReconDirectPayment)BizO.Headers[1]).RelatedDirectDebitBatch;
				}
				AssertNotNull("RelatedDirectDebitBatch", directDebitBatchHeader);
			}

			BizO = new DirectTransactionsBusinessObject(factory, ZGuid.Empty, ZDateTime.Empty);
			using (BankTransactionForm testForm = new BankTransactionForm(BizO))
			{
				testForm.Show();
				testForm.NewDirectReceiptButton_Click_ForTestOnly(null, null);
				testForm.NewDirectPaymentButton_Click_ForTestOnly(null, null);
				AssertEquals(0, testForm.TransactionsBizO_ForTestOnly.Headers.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectPayments.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectReceipts.Count);

				directPayment = testForm.TransactionsBizO_ForTestOnly.DirectPayments[0];
				directPayment.AH_OSExTaxAmount = 30m;
				directPayment.AH_LocalExTaxAmount = 30m;
				directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
				directPayment.Lines[0].AL_LocalExTaxAmount = 30m;
				directPayment.Lines[0].AL_OSExTaxAmount = 30m;

				directReceipt = testForm.TransactionsBizO_ForTestOnly.DirectReceipts[0];
				directReceipt.AH_OSExTaxAmount = 30m;
				directReceipt.AH_LocalExTaxAmount = 30m;
				directReceipt.Lines.AddNew(directReceipt.DependentTransactionLineType);
				directReceipt.Lines[0].AL_LocalExTaxAmount = 30m;
				directReceipt.Lines[0].AL_OSExTaxAmount = 30m;

				testForm.ApplyButton_Click_ForTestOnly(null, null);
				AssertEquals(0, testForm.TransactionsBizO_ForTestOnly.Headers.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectPayments.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectReceipts.Count);

				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(factory);
				testHelper.SetupPeriods();

				directPayment.AH_AB = factory.NewWithValidTestData<AccBankAccount>().PK;
				directPayment.AH_ChequeOrReference = "test";
				directPayment.AH_ReceiptType = "CSH";

				directReceipt.AH_AB = factory.NewWithValidTestData<AccBankAccount>().PK;
				directReceipt.AH_ChequeOrReference = "test";
				directReceipt.AH_ReceiptType = "CSH";

				testForm.ApplyButton_Click_ForTestOnly(null, null);
				AssertEquals(2, testForm.TransactionsBizO_ForTestOnly.Headers.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectPayments.Count);
				AssertEquals(1, testForm.TransactionsBizO_ForTestOnly.DirectReceipts.Count);
			}
		}

		public void TestChequeDrawerReadOnly()
		{
			BizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			using (var form = new BankTransactionForm(BizO))
			{
				form.Show();

				form.NewDirectReceiptButton_Click_ForTestOnly(null, null);
				var directReceiptsGrid = form.GetControl<ZGrid>("DirectReceiptsGrid");
				var directReceipt = directReceiptsGrid.List[0] as BankReconDirectReceipt;

				AssertNullOrEmpty("Percondition", directReceipt.AH_ReceiptType);
				AssertEquals(true, directReceipt.AH_ChequeDrawerInfo.ReadOnly);

				directReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
				AssertEquals(false, directReceipt.AH_ChequeDrawerInfo.ReadOnly);

				directReceipt.AH_ReceiptType = ReceiptTypes.Cash;
				AssertEquals(true, directReceipt.AH_ChequeDrawerInfo.ReadOnly);

				form.NewDirectPaymentButton_Click_ForTestOnly(null, null);
				var directPaymentsGrid = form.GetControl<ZGrid>("DirectPaymentsGrid");
				var directPayment = directPaymentsGrid.List[0] as BankReconDirectPayment;
				AssertEquals(false, directPayment.AH_ChequeDrawerInfo.ReadOnly);
			}
		}

		public void TestAL_TaxDateInPaymentAndReceiptGrids()
		{
			BizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			using (var form = new BankTransactionForm(BizO))
			{
				form.Show();
				var taxDateColumnForPayment = form.DirectPaymentLinesGrid_ForTestOnly.GetColumnStyle("AL_TaxDate");
				AssertNotNull(taxDateColumnForPayment);
				AssertEquals(false, taxDateColumnForPayment.IsVisible);

				var taxDateColumnForReceipt = form.DirectReceiptLinesGrid_ForTestOnly.GetColumnStyle("AL_TaxDate");
				AssertNotNull(taxDateColumnForReceipt);
				AssertEquals(false, taxDateColumnForReceipt.IsVisible);
			}
		}

		void Helper_BankTransactionFormShown(object sender, EventArgs e)
		{
			BankTransactionForm testForm = ZFormModaliser.LastFormShownDialogForTest as BankTransactionForm;
			testForm.TransactionsBizO_ForTestOnly.MoveDirectReceiptsAndPaymentsToBaseCollection();
			BizO.Headers.RemoveAll();
		}

		public void TestNotifyUserAboutNotSupportedException()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BizO = new DirectTransactionsBusinessObject(factory, ZGuid.Empty, ZDateTime.Empty);
			BankReconDirectPayment directPayment = factory.NewWithValidTestData<BankReconDirectPayment>();
			BizO.Headers.Add(directPayment);
			factory.Save();
			using (BankTransactionForm form = new BankTransactionForm(BizO))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				BizO.Headers.RemoveAndDelete(directPayment);
				AssertEquals("You cannot delete Accounting Transactions in Database.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestColumns_ForGSTRegistered()
		{
			var clolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_AT,
				DirectTransactionLineBase.Schema.AL_TaxDate,
				DirectTransactionLineBase.Schema.AL_A9_VATClass,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount
			};

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertBankTransactionFormColumns(true, clolumns);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertBankTransactionFormColumns(false, clolumns);
		}

		public void TestColumns_ForGSTRegistered_RecoverableColumns()
		{
			var clolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_AT,
				DirectTransactionLineBase.Schema.AL_TaxDate,
				DirectTransactionLineBase.Schema.AL_A9_VATClass,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount
			};

			var recoverableClolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount_Recoverable,
				DirectTransactionLineBase.Schema.AL_OSTaxAmount_NotRecoverable,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount_Recoverable,
				DirectTransactionLineBase.Schema.AL_LocalTaxAmount_NotRecoverable
			};

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AssertBankTransactionFormColumns(true, clolumns,
				new Action<ZGrid>((x) =>
				{
					foreach (var columnName in recoverableClolumns)
					{
						AssertEquals(true, x.Columns.Contains(columnName));
					}
				}),
				new Action<ZGrid>((x) =>
				{
					foreach (var columnName in recoverableClolumns)
					{
						AssertEquals(false, x.Columns.Contains(columnName));
					}
				})
			);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AssertBankTransactionFormColumns(false, recoverableClolumns);
		}

		public void TestColumns_ForGovernmentChargeCode()
		{
			var clolumns = new List<string>	{ DirectTransactionLineBase.Schema.AL_GovtChargeCode };

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertBankTransactionFormColumns(true, clolumns);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertBankTransactionFormColumns(false, clolumns);
			}
		}

		public void TestColumns_ForPlaceOfSupply()
		{
			var clolumns = new List<string>	{ DirectTransactionLineBase.Schema.AL_PlaceOfSupply };

			AssertEquals(false, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
			AssertBankTransactionFormColumns(false, clolumns);

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			{
				AssertEquals(true, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
				AssertBankTransactionFormColumns(true, clolumns);
			}
		}

		public void TestColumns_ForExtraTaxApplicable()
		{
			var clolumns = new List<string>
			{
				DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount,
				DirectTransactionLineBase.Schema.AL_LocalGSTAmount,
				DirectTransactionLineBase.Schema.AL_OSGSTAmount
			};

			string exTaxCaption = null;
			string gSTCaption = null;
			var additionalAction = new Action<ZGrid>((x) => {
				AssertEquals(exTaxCaption + " Amount", x.GetColumnCaption(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount));
				AssertEquals(exTaxCaption + " Local", x.GetColumnCaption(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount));
				AssertEquals(gSTCaption + " Local", x.GetColumnCaption(DirectTransactionLineBase.Schema.AL_LocalGSTAmount));
				AssertEquals(gSTCaption + " Amount", x.GetColumnCaption(DirectTransactionLineBase.Schema.AL_OSGSTAmount));
			});

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals(false, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertBankTransactionFormColumns(false, clolumns);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				exTaxCaption = "QST";
				gSTCaption = "GST";
				AssertEquals(true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertBankTransactionFormColumns(true, clolumns, additionalAction, additionalAction);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				exTaxCaption = "SGST";
				gSTCaption = "GST";
				AssertEquals(true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertBankTransactionFormColumns(true, clolumns, additionalAction, additionalAction);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				exTaxCaption = "RET";
				gSTCaption = "IVA";
				AssertEquals(true, GlbCompany.CurrentCompany.IsExtraTaxApplicable());
				AssertBankTransactionFormColumns(true, clolumns, additionalAction, additionalAction);
			}
		}

		public void TestSupplyTypeColumnVisibility()
		{
			var columns = new List<string> { DirectTransactionLineBase.Schema.AL_SupplyType };

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(
				Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				AssertBankTransactionFormColumns(false, columns);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(
				Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AssertBankTransactionFormColumns(true, columns);
			}
		}

		void AssertBankTransactionFormColumns(bool shouldBeVisible, List<string> columnNames,
			Action<ZGrid> additionalActionForDirectPaymentLinesGrid = null,
			Action<ZGrid> additionalActionForDirectReceiptLinesGrid = null)
		{
			AssertBankTransactionForm(new Action<BankTransactionForm>((x) =>
			{
				var directPaymentLinesGrid = x.GetControl<ZGrid>("DirectPaymentLinesGrid");
				var directReceiptLinesGrid = x.GetControl<ZGrid>("DirectReceiptLinesGrid");

				x.NewDirectPaymentButton_Click_ForTestOnly(null, null);
				foreach (var columnName in columnNames)
				{
					AssertEquals(shouldBeVisible, directPaymentLinesGrid.Columns.Contains(columnName));
				}
				additionalActionForDirectPaymentLinesGrid?.Invoke(directPaymentLinesGrid);

				x.NewDirectReceiptButton_Click_ForTestOnly(null, null);
				foreach (var columnName in columnNames)
				{
					AssertEquals(shouldBeVisible, directReceiptLinesGrid.Columns.Contains(columnName));
				}
				additionalActionForDirectReceiptLinesGrid?.Invoke(directReceiptLinesGrid);
			}));
		}

		public void TestTransactionHeadersGridColumns_TaxBranch()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertTransactionLineGridNoTaxBranch();
			AssertTransactionHeaderGridNoTaxBranch();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertTransactionLineGridNoTaxBranch();
			AssertTransactionHeaderGridNoTaxBranch();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertTransactionLineGridNoTaxBranch();
			AssertTransactionHeaderGridNoTaxBranch();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertTransactionLinesGridColumns(new[] {
				(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, true, true),
				(DirectTransactionLineBase.Schema.TaxBranchName, true, (bool?)true),
			});
			AssertTransactionHeadersGridColumns(new[] {
				(TransactionHeaderWithLines.Schema.AH_GB_TaxBranch, true, true),
				(TransactionHeaderWithLines.Schema.AH_Calc_TaxBranchName, true, (bool?)true),
			});

			void AssertTransactionLineGridNoTaxBranch()
			{
				AssertTransactionLinesGridColumns(new[] {
					(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, false, null),
					(DirectTransactionLineBase.Schema.TaxBranchName, false, (bool?)null),
				});
			}

			void AssertTransactionHeaderGridNoTaxBranch()
			{
				AssertTransactionHeadersGridColumns(new[] {
					(TransactionHeaderWithLines.Schema.AH_GB_TaxBranch, false, null),
					(TransactionHeaderWithLines.Schema.AH_Calc_TaxBranchName, false, (bool?)null),
				});
			}
		}

		void AssertTransactionHeadersGridColumns(IEnumerable<(string columnName, bool isColumnsAvailable, bool? isVisible)> testColumns)
		{
			AssertBankTransactionForm(form => {
				var directPaymentsGrid = form.GetControl<ZGrid>("DirectPaymentsGrid");
				var directReceiptsGrid = form.GetControl<ZGrid>("DirectReceiptsGrid");

				foreach (var testColumn in testColumns)
				{
					AssertColumnsAddedCorrectly(directPaymentsGrid, testColumn.columnName, testColumn.isColumnsAvailable, testColumn.isVisible);
					AssertColumnsAddedCorrectly(directReceiptsGrid, testColumn.columnName, testColumn.isColumnsAvailable, testColumn.isVisible);
				}
			});
		}

		void AssertTransactionLinesGridColumns(IEnumerable<(string columnName, bool isColumnsAvailable, bool? isVisible)> testColumns)
		{
			AssertBankTransactionForm(form => {
				var directPaymentLinesGrid = form.GetControl<ZGrid>("DirectPaymentLinesGrid");
				var directReceiptLinesGrid = form.GetControl<ZGrid>("DirectReceiptLinesGrid");

				foreach (var testColumn in testColumns)
				{
					AssertColumnsAddedCorrectly(directPaymentLinesGrid, testColumn.columnName, testColumn.isColumnsAvailable, testColumn.isVisible);
					AssertColumnsAddedCorrectly(directReceiptLinesGrid, testColumn.columnName, testColumn.isColumnsAvailable, testColumn.isVisible);
				}
			});
		}

		void AssertColumnsAddedCorrectly(ZGrid grid, ZString columnName, bool isColumnsAvailable, bool? isVisible)
		{
			var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
			var isAvailable = !(columns.FirstOrDefault(x => x.ColumnName == columnName)?.IsUnavailable ?? true);
			AssertEquals("Available", isColumnsAvailable, isAvailable);
			if (isColumnsAvailable)
			{
				AssertNotNull("asserting Visible should be not null when Available.", isVisible);
				AssertEquals("Visible", isVisible, columns.FirstOrDefault(x => x.ColumnName == columnName).IsVisible);
			}
		}

		void AssertBankTransactionForm(Action<BankTransactionForm> action)
		{
			BizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			using (var form = new BankTransactionForm(BizO))
			{
				form.Show();
				Application.DoEvents();

				action(form);
			}
		}
	}
}
