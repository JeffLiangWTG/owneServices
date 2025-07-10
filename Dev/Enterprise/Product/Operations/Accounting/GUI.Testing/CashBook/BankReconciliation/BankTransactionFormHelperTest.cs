using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation.Testing
{
	public class BankTransactionFormHelperTest : TestCaseWithFactory
	{
		public void TestBankReconTransactionForDirectDebitBatchIsUpdatedAfterUpdateTaxAmountAndCommittingChanges()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;
			Factory.Save();

			var temporaryFactory = new BusinessObjectFactory();
			var tempAdditionalTransactions = new DirectTransactionsBusinessObject(temporaryFactory, ZGuid.Empty, ZDateTime.Empty);
			var directPayment = temporaryFactory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			var directPaymentLine = directPayment.Lines.AddNew();
			directPaymentLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			directPaymentLine.AL_OSExTaxAmount = 100m;
			directPaymentLine.AL_AT = TestObjectCreator.GST1.PK;

			tempAdditionalTransactions.Headers.Add(directPayment);

			var directTransactions = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			var helper = new BankTransactionFormHelper(TestObjectCreator.AUDBankAccount, ZDateTime.Empty, directTransactions, true);
			helper.CommitChanges_ForTestOnly(tempAdditionalTransactions);

			AssertEquals("one header should be added.", 1, directTransactions.Headers.Count);
			AssertEquals("directPayment should be added", true, directTransactions.Headers.Any(bo => bo.PK == directPayment.PK));

			var directDebitBatch = ((BankReconDirectPayment)directTransactions.Headers[0]).RelatedDirectDebitBatch;
			AssertEquals("Initial Direct Debit Batch OS Amount", 110m, directDebitBatch.AH_OSTotal);
			AssertEquals("Initial Direct Debit Batch Local Amount", 110m, directDebitBatch.AH_InvoiceAmount);

			directPaymentLine.AL_AT = TestObjectCreator.GST2.PK;
			helper.CommitChanges_ForTestOnly(tempAdditionalTransactions);

			AssertNoExceptionThrown("Will not show Critical Validation Error: DirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts", () => Factory.Save());

			AssertEquals("New Direct Debit Batch OS Amount", 120m, directDebitBatch.AH_OSTotal);
			AssertEquals("New Direct Debit Batch Local Amount", 120m, directDebitBatch.AH_InvoiceAmount);
		}

		public void TestBankReconTransactionForDirectDebitBatchIsUpdatedAfterCommittingChanges()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;
			Factory.Save();

			var temporaryFactory = new BusinessObjectFactory();
			var tempAdditionalTransactions = new DirectTransactionsBusinessObject(temporaryFactory, ZGuid.Empty, ZDateTime.Empty);
			var directPayment = temporaryFactory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			var directPaymentLine = directPayment.Lines.AddNew();
			directPaymentLine.AL_OSExTaxAmount = 100m;
			tempAdditionalTransactions.Headers.Add(directPayment);

			var directTransactions = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			var helper = new BankTransactionFormHelper(TestObjectCreator.AUDBankAccount, ZDateTime.Empty, directTransactions, true);
			helper.CommitChanges_ForTestOnly(tempAdditionalTransactions);

			AssertEquals("one header should be added.", 1, directTransactions.Headers.Count);
			AssertEquals("directPayment should be added", true, directTransactions.Headers.Any(bo => bo.PK == directPayment.PK));

			var directDebitBatch = ((BankReconDirectPayment)directTransactions.Headers[0]).RelatedDirectDebitBatch;
			AssertEquals("Initial Direct Debit Batch Amount", 100m, directDebitBatch.AH_OSTotal);

			var bankReconTransactionForBatch = directTransactions.BankReconTransactions[0];
			AssertEquals(bankReconTransactionForBatch.PK, directDebitBatch.PK);
			AssertEquals("Initial Direct Debit Batch Bank Recon Amount", 100m, bankReconTransactionForBatch.AH_OSTotal);
			AssertEquals("Initial Amount shown on Bank Reconciliation Form", 100m, bankReconTransactionForBatch.Credit);

			directPaymentLine.AL_OSExTaxAmount = 250m;
			helper.CommitChanges_ForTestOnly(tempAdditionalTransactions);

			AssertEquals("New Direct Debit Batch Amount", 250m, directDebitBatch.AH_OSTotal);
			AssertEquals("New Direct Debit Batch Bank Recon Amount", 250m, bankReconTransactionForBatch.AH_OSTotal);
			AssertEquals("New Amount shown on Bank Reconciliation Form", 250m, bankReconTransactionForBatch.Credit);
		}

		public void TestBankReconTransactionForDirectPaymentIsUpdatedAfterCommittingChanges()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;
			Factory.Save();

			var temporaryFactory = new BusinessObjectFactory();
			var tempAdditionalTransactions = new DirectTransactionsBusinessObject(temporaryFactory, ZGuid.Empty, ZDateTime.Empty);
			var directPayment = temporaryFactory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			var directPaymentLine = directPayment.Lines.AddNew();
			directPaymentLine.AL_OSExTaxAmount = 100m;
			tempAdditionalTransactions.Headers.Add(directPayment);

			var directTransactions = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			var helper = new BankTransactionFormHelper(TestObjectCreator.AUDBankAccount, ZDateTime.Empty, directTransactions, true);
			helper.CommitChanges_ForTestOnly(tempAdditionalTransactions);

			AssertEquals("one header should be added.", 1, directTransactions.Headers.Count);
			AssertEquals("directPayment should be added", true, directTransactions.Headers.Any(bo => bo.PK == directPayment.PK));

			var directDebitBatch = ((BankReconDirectPayment)directTransactions.Headers[0]).RelatedDirectDebitBatch;
			AssertEquals("Initial Direct Debit Batch Amount", 100m, directDebitBatch.AH_OSTotal);

			var bankReconTransactionForDirectPayment = directTransactions.BankReconTransactions[0];
			AssertEquals(bankReconTransactionForDirectPayment.PK, directPayment.PK);
			AssertEquals("Initial Direct Payment Bank Recon Amount", -100m, bankReconTransactionForDirectPayment.AH_OSTotal);
			AssertEquals("initial Amount shown on Bank Reconciliation Form", 100m, bankReconTransactionForDirectPayment.Credit);

			directPaymentLine.AL_OSExTaxAmount = 250m;
			helper.CommitChanges_ForTestOnly(tempAdditionalTransactions);

			AssertEquals("New Direct Debit Batch Amount", 250m, directDebitBatch.AH_OSTotal);
			AssertEquals("New Direct Payment Bank Recon Amount", -250m, bankReconTransactionForDirectPayment.AH_OSTotal);
			AssertEquals("New Amount shown on Bank Reconciliation Form", 250m, bankReconTransactionForDirectPayment.Credit);
		}

		public void TestCommitChanges_Edit()
		{
			var bizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);

			var directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
			directReceipt.MakeDepositBatch();
			UpdateTransactionHeader_TypeA(directReceipt);

			var directReceiptLine_Edit = directReceipt.Lines.AddNew() as DirectTransactionLineBase;
			UpdateTransactionLine_TypeA(directReceiptLine_Edit);

			var directReceiptLine_Delete = directReceipt.Lines.AddNew();

			bizO.Headers.Add(directReceipt);

			var commitChanges = new DirectTransactionsBusinessObject(new BusinessObjectFactory(), ZGuid.Empty, ZDateTime.Empty);

			var editedReceipt = commitChanges.Factory.ImportFromAnotherFactory(directReceipt) as DirectTransactionHeaderBase;

			var editedReceiptLine = commitChanges.Factory.ImportFromAnotherFactory(directReceiptLine_Edit) as DirectTransactionLineBase;
			SubAccountHelper.CopySubAccounts(editedReceiptLine, directReceiptLine_Edit, true);
			editedReceiptLine.OnLoaded();
			editedReceipt.Lines.Add(editedReceiptLine);
			commitChanges.Headers.Add(editedReceipt);

			UpdateTransactionHeader_TypeB(editedReceipt);
			UpdateTransactionLine_TypeB(editedReceiptLine);
			var newReceiptLine = editedReceipt.Lines.AddNew() as DirectTransactionLineBase;
			UpdateTransactionLine_TypeA(newReceiptLine);

			var helper = new BankTransactionFormHelper(null, ZDateTime.Empty, bizO, true);

			AssertEquals("PreCondition, directReceipt Line's Number", 2, directReceipt.Lines.Count);
			AssertTransactionHeader_TypeA("PreCondition, header before committed", directReceipt);
			AssertTransactionLine_TypeA("PreCondition, line before committed", directReceiptLine_Edit);

			helper.CommitChanges_ForTestOnly(commitChanges);

			AssertTransactionHeader_TypeB("Header should be synchronized to TypeB value setting.", directReceipt);

			AssertEquals("After Committed, one line was removed and one line was added , so it should be still 2 line.", 2, directReceipt.Lines.Count);
			AssertTransactionLine_TypeB("Line should be synchronized to TypeB value setting.", directReceiptLine_Edit);
			AssertEquals("Line should be removed", false, directReceipt.Lines.Any(bo => bo.PK == directReceiptLine_Delete.PK));
			AssertEquals("Line should be added", true, directReceipt.Lines.Any(bo => bo.PK == newReceiptLine.PK));

			var directReceiptLine_New = directReceipt.Lines.First(bo => bo.PK == newReceiptLine.PK) as DirectTransactionLineBase;
			AssertTransactionLine_TypeA("Added line's value should be same as committed one.", directReceiptLine_New);
		}

		public void TestCommitChanges_Delete()
		{
			var bizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			var commitChanges = new DirectTransactionsBusinessObject(new BusinessObjectFactory(), ZGuid.Empty, ZDateTime.Empty);

			var directPayment = Factory.NewWithValidTestData<BankReconDirectPayment>();
			bizO.Headers.Add(directPayment);

			var directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
			directReceipt.MakeDepositBatch();
			bizO.Headers.Add(directReceipt);
			commitChanges.Headers.Add(commitChanges.Factory.ImportFromAnotherFactory(directReceipt) as BankReconDirectReceipt);

			var helper = new BankTransactionFormHelper(null, ZDateTime.Empty, bizO, true);

			AssertEquals("PreCondition", 2, bizO.Headers.Count);
			helper.CommitChanges_ForTestOnly(commitChanges);
			AssertEquals("one transaction should be removed after commit changes", 1, bizO.Headers.Count);
			AssertEquals("directReceipt should be kept after commit changes", true, bizO.Headers.Any(bo => bo.PK == directReceipt.PK));
			AssertEquals("directPayment should be removed after commit changes", false, bizO.Headers.Any(bo => bo.PK == directPayment.PK));
		}

		public void TestCommitChanges_DeleteAndSetNullForRelatedDirectDebitBatch()
		{
			var bizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			var commitChanges = new DirectTransactionsBusinessObject(new BusinessObjectFactory(), ZGuid.Empty, ZDateTime.Empty);

			var directPayment = Factory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			directPayment.MakeDirectDebitBatch();
			bizO.Headers.Add(directPayment);

			AssertEquals("PreCondition", 1, bizO.Headers.Count);
			AssertNotNull("PreCondition: Has RelatedDirectDebitBatch", directPayment.RelatedDirectDebitBatch);

			var helper = new BankTransactionFormHelper(null, ZDateTime.Empty, bizO, true);
			helper.CommitChanges_ForTestOnly(commitChanges);

			AssertEquals("DirectPayment should be removed after commit", 0, bizO.Headers.Count);
			AssertNull("Should be set to null after commit", directPayment.RelatedDirectDebitBatch);
		}

		public void TestCommitChanges_New()
		{
			var bizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);
			var commitChanges = new DirectTransactionsBusinessObject(new BusinessObjectFactory(), ZGuid.Empty, ZDateTime.Empty);

			var newDirectReceipt = commitChanges.Factory.NewWithValidTestData<BankReconDirectReceipt>();
			newDirectReceipt.MakeDepositBatch();
			UpdateTransactionHeader_TypeA(newDirectReceipt);
			var newDirectReceiptLine = newDirectReceipt.Lines.AddNew() as DirectTransactionLineBase;
			UpdateTransactionLine_TypeA(newDirectReceiptLine);

			commitChanges.Headers.Add(newDirectReceipt);

			var helper = new BankTransactionFormHelper(null, ZDateTime.Empty, bizO, true);

			AssertEquals("PreCondition", 0, bizO.Headers.Count);
			helper.CommitChanges_ForTestOnly(commitChanges);
			AssertEquals("one header should be added.", 1, bizO.Headers.Count);
			AssertEquals("newDirectReceipt should be added", true, bizO.Headers.Any(bo => bo.PK == newDirectReceipt.PK));

			var addedHeader = bizO.Headers.First(bo => bo.PK == newDirectReceipt.PK) as BankReconDirectReceipt;
			AssertTransactionHeader_TypeA("value checking", addedHeader);

			AssertEquals("checking line number, should be same as committed header", 1, addedHeader.Lines.Count);
			AssertTransactionLine_TypeA("value checking", addedHeader.Lines[0]);
		}

		void UpdateTransactionHeader_TypeA(AccTransactionHeader header)
		{
			header.AH_InvoiceDate = new ZDateTime(2022, 01, 07);
			header.AH_PostDate = new ZDateTime(2022, 01, 06);
			header.AH_ReceiptType = "AAA";
			header.AH_ChequeOrReference = "BBB";
			header.AH_RX_NKTransactionCurrency = "CC";
			header.AH_ExchangeRate = new ZDecimal(0.1);
			header.AH_ChequeDrawer = "DD";
			header.AH_DrawerBank = "EE";
			header.AH_DrawerBranch = "FF";
			header.AH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			AssertTransactionHeader_TypeA("PreCondition UpdateTransactionHeader_TypeA", header);
		}

		void UpdateTransactionHeader_TypeB(AccTransactionHeader header)
		{
			header.AH_InvoiceDate = new ZDateTime(2021, 12, 07);
			header.AH_PostDate = new ZDateTime(2021, 12, 06);
			header.AH_ReceiptType = "AAQ";
			header.AH_ChequeOrReference = "BBBQ";
			header.AH_RX_NKTransactionCurrency = "CCQ";
			header.AH_ExchangeRate = new ZDecimal(0.11);
			header.AH_ChequeDrawer = "DDQ";
			header.AH_DrawerBank = "EEQ";
			header.AH_DrawerBranch = "FFQ";
			header.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;

			AssertTransactionHeader_TypeB("PreCondition UpdateTransactionHeader_TypeB", header);
		}

		void AssertTransactionHeader_TypeA(string comment, AccTransactionHeader header)
		{
			CombineAssertions(comment, () => {
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_InvoiceDate, new ZDateTime(2022, 01, 07), header.AH_InvoiceDate);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_PostDate, new ZDateTime(2022, 01, 06), header.AH_PostDate);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ReceiptType, "AAA", header.AH_ReceiptType);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ChequeOrReference, "BBB", header.AH_ChequeOrReference);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_RX_NKTransactionCurrency, "CC", header.AH_RX_NKTransactionCurrency);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ExchangeRate, new ZDecimal(0.1), header.AH_ExchangeRate);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ChequeDrawer, "DD", header.AH_ChequeDrawer);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_DrawerBank, "EE", header.AH_DrawerBank);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_DrawerBranch, "FF", header.AH_DrawerBranch);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_GB_TaxBranch, TestObjectCreator.NonCurrentBranch.PK, header.AH_GB_TaxBranch);
			});
		}

		void AssertTransactionHeader_TypeB(string comment, AccTransactionHeader header)
		{
			CombineAssertions(comment, () => {
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_InvoiceDate, new ZDateTime(2021, 12, 07), header.AH_InvoiceDate);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_PostDate, new ZDateTime(2021, 12, 06), header.AH_PostDate);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ReceiptType, "AAQ", header.AH_ReceiptType);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ChequeOrReference, "BBBQ", header.AH_ChequeOrReference);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_RX_NKTransactionCurrency, "CCQ", header.AH_RX_NKTransactionCurrency);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ExchangeRate, new ZDecimal(0.11), header.AH_ExchangeRate);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_ChequeDrawer, "DDQ", header.AH_ChequeDrawer);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_DrawerBank, "EEQ", header.AH_DrawerBank);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_DrawerBranch, "FFQ", header.AH_DrawerBranch);
				AssertEquals(DirectTransactionHeaderBase.Schema.AH_GB_TaxBranch, GlbBranch.CurrentBranch.PK, header.AH_GB_TaxBranch);
			});
		}

		void UpdateTransactionLine_TypeA(DirectTransactionLineBase line)
		{
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_Desc = "AL_DescAAA";
			line.AL_OSExTaxAmount = 123;
			line.AL_AT = TestObjectCreator.SVAT1.PK;
			line.SetTaxDateSafe(new ZDate(2022, 01, 05));
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			line.AL_OSTaxAmount = 1111;
			line.AL_GovtChargeCode = "Govt_AA";
			line.AL_PlaceOfSupply = "PAA";
			line.AL_Calc_InputGSTVATRecoverablePercentage = 0.2;

			AssertTransactionLine_TypeA("PreCondition AssertTransactionLine_TypeA", line);
		}

		void UpdateTransactionLine_TypeB(DirectTransactionLineBase line)
		{
			line.AL_AG = TestObjectCreator.GLHeader2.PK;
			line.AL_Desc = "AL_DescAAAQ";
			line.AL_OSExTaxAmount = 122;
			line.AL_AT = TestObjectCreator.SVAT2.PK;
			line.SetTaxDateSafe(new ZDate(2021, 12, 05));
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg2.PK;
			line.AL_OSTaxAmount = 1110;
			line.AL_GovtChargeCode = "Govt_AAQ";
			line.AL_PlaceOfSupply = "PAAQ";
			line.AL_Calc_InputGSTVATRecoverablePercentage = 0.1;

			AssertTransactionLine_TypeB("PreCondition AssertTransactionLine_TypeB", line);
		}

		void AssertTransactionLine_TypeA(string comment, DependentTransactionLine line)
		{
			CombineAssertions(comment, () => {
				AssertEquals(DirectTransactionLineBase.Schema.AL_AG, TestObjectCreator.GLHeader1.PK, line.AL_AG);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GB, line.TransactionHeader.AH_GB, line.AL_GB);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GE, line.TransactionHeader.AH_GE, line.AL_GE);
				AssertEquals(DirectTransactionLineBase.Schema.AL_Desc, "AL_DescAAA", line.AL_Desc);
				AssertEquals(DirectTransactionLineBase.Schema.AL_OSExTaxAmount, new ZDecimal(123), line.AL_OSExTaxAmount);
				AssertEquals(DirectTransactionLineBase.Schema.AL_TaxDate, new ZDate(2022, 01, 05), line.AL_TaxDate);
				AssertEquals(DirectTransactionLineBase.Schema.AL_A9_VATClass, TestObjectCreator.TaxMsg1.PK, line.AL_A9_VATClass);
				AssertEquals(DirectTransactionLineBase.Schema.AL_OSTaxAmount, new ZDecimal(1111), line.AL_OSTaxAmount);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GovtChargeCode, "Govt_AA", line.AL_GovtChargeCode);
				AssertEquals(DirectTransactionLineBase.Schema.AL_PlaceOfSupply, "PAA", line.AL_PlaceOfSupply);
				AssertEquals(DirectTransactionLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage, new ZDecimal(0.2), line.AL_Calc_InputGSTVATRecoverablePercentage);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GovtChargeCode, "Govt_AA", line.AL_GovtChargeCode);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, line.TransactionHeader.AH_GB_TaxBranch, line.AL_GB_TaxBranch);
			});
		}

		void AssertTransactionLine_TypeB(string comment, DependentTransactionLine line)
		{
			CombineAssertions(comment, () => {
				AssertEquals(DirectTransactionLineBase.Schema.AL_AG, TestObjectCreator.GLHeader2.PK, line.AL_AG);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GB, line.TransactionHeader.AH_GB, line.AL_GB);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GE, line.TransactionHeader.AH_GE, line.AL_GE);
				AssertEquals(DirectTransactionLineBase.Schema.AL_Desc, "AL_DescAAAQ", line.AL_Desc);
				AssertEquals(DirectTransactionLineBase.Schema.AL_OSExTaxAmount, new ZDecimal(122), line.AL_OSExTaxAmount);
				AssertEquals(DirectTransactionLineBase.Schema.AL_TaxDate, new ZDate(2021, 12, 05), line.AL_TaxDate);
				AssertEquals(DirectTransactionLineBase.Schema.AL_A9_VATClass, TestObjectCreator.TaxMsg2.PK, line.AL_A9_VATClass);
				AssertEquals(DirectTransactionLineBase.Schema.AL_OSTaxAmount, new ZDecimal(1110), line.AL_OSTaxAmount);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GovtChargeCode, "Govt_AAQ", line.AL_GovtChargeCode);
				AssertEquals(DirectTransactionLineBase.Schema.AL_PlaceOfSupply, "PAAQ", line.AL_PlaceOfSupply);
				AssertEquals(DirectTransactionLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage, new ZDecimal(0.1), line.AL_Calc_InputGSTVATRecoverablePercentage);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GovtChargeCode, "Govt_AAQ", line.AL_GovtChargeCode);
				AssertEquals(DirectTransactionLineBase.Schema.AL_GB_TaxBranch, line.TransactionHeader.AH_GB_TaxBranch, line.AL_GB_TaxBranch);
			});
		}

		public void TestShowBankTransactionForm()
		{
			var bizO = new DirectTransactionsBusinessObject(Factory, ZGuid.Empty, ZDateTime.Empty);

			var directPayment = Factory.NewWithValidTestData<BankReconDirectPayment>();
			directPayment.RelatedStatementPK = ZGuid.NewZGuid();
			bizO.Headers.Add(directPayment);

			var directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
			directReceipt.RelatedStatementPK = ZGuid.NewZGuid();
			directReceipt.MakeDepositBatch();
			bizO.Headers.Add(directReceipt);

			IEnumerable<string> columnsDirectPaymentsGrid = null;
			IEnumerable<string> columnsDirectReceiptsGrid = null;
			DirectTransactionsBusinessObject businessEntity = null;
			var helper = new BankTransactionFormHelper(null, ZDateTime.Empty, bizO, true);
			helper.BankTransactionFormLoad += (sender, e) =>
			{
				var testingForm = sender as BankTransactionForm;
				Argument.NotNull(testingForm, nameof(BankTransactionForm));

				businessEntity = testingForm.BusinessEntity as DirectTransactionsBusinessObject;
				columnsDirectPaymentsGrid = GetGridColumns(testingForm.GetControl<ZGrid>("DirectPaymentsGrid"));
				columnsDirectReceiptsGrid = GetGridColumns(testingForm.GetControl<ZGrid>("DirectReceiptsGrid"));
			};

			ZFormModaliser.ShowDialogsInTest = true;
			helper.ShowBankTransactionForm();
			AssertNotNull(businessEntity);

			AssertNotNull(columnsDirectPaymentsGrid);
			AssertGridColumnsAreReadOnly(columnsDirectPaymentsGrid, businessEntity.DirectPayments.FindByPK(directPayment.PK) as BankReconDirectPayment);

			AssertNotNull(columnsDirectReceiptsGrid);
			AssertGridColumnsAreReadOnly(columnsDirectReceiptsGrid, businessEntity.DirectReceipts.FindByPK(directReceipt.PK) as BankReconDirectReceipt);

			IEnumerable<string> GetGridColumns(ZGrid zGrid)
			{
				return zGrid.ColumnStyles.OfType<ZGridColumnInfo>()
					.Select(col => col.ColumnName)
					.ToArray();
			}

			void AssertGridColumnsAreReadOnly(IEnumerable<string> columnNames, DirectTransactionHeaderBase testHeader)
			{
				var exceptColumns = new[] { DirectTransactionHeaderBase.Schema.AH_GB_TaxBranch };
				var columnsShouldReadOnly = columnNames.Except(exceptColumns);
				CombineAssertions("properties below should be read only in default. If you miss something, please ass test to DirectTransactionHeaderBaseTest->TestMostColumnsReadOnly too.", () =>
				{
					foreach (var columnName in columnsShouldReadOnly)
					{
						AssertEquals(columnName, true, testHeader.ZPropertyInfoHash[columnName].ReadOnly);
					}
				});
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
