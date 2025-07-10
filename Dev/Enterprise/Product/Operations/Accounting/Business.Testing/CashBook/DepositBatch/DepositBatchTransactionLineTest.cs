using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchTransactionLine))]
	public class DepositBatchTransactionLineTest : TransactionHeaderTest
	{
		protected override BusinessObject PrepareTransactionHeaderForTest()
		{
			TransactionHeader obj = (TransactionHeader)base.PrepareTransactionHeaderForTest();
			// Cannot call Factory.NewWithValidTestData here because it does not fill user related properties, that will cause TestCreatingUserID to fail.
			// Cannot call obj.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, new PropertyDescriptor[0]) because it fills AH_TransactionNum with random string.
			if (obj.AH_TransactionType.IsEmpty)
			{
				// AccTransactionHeader is not valid with empty AH_TransactionType.
				// Default type (Journal) fails with crtical validation "AR JNL with an empty GL account field.".
				obj.AH_TransactionType = TransactionTypes.Receipt;
			}
			if (obj.AH_Ledger.IsEmpty)
			{
				obj.AH_Ledger = LedgerTypes.AccountsReceivable;
			}
			return obj;
		}

		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		public void TestZDecimalsHaveCorrectDecimalPlacesDepositTransactionLine()
		{
			var line = Factory.New<DepositBatchTransactionLine>();
			line.AH_AB = Factory.New<AccBankAccount>().PK;

			var osList = new List<string>
				{
					nameof(line.TotalDepositAmount)
				};

			var tester = new DecimalPlacesAttributeTester(line, line.Company);
			tester.CheckNonLocalCurrency(osList, nameof(line.BankCurrencyDecimalsAsInt), nameof(line.BankAccount.AB_RX_NKAccountCurrency), line.BankAccount);
		}

		public void TestIsBankCurrencyLocal()
		{
			var line = Factory.New<DepositBatchTransactionLine>();
			line.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			Assert(line.IsBankCurrencyLocal);

			line.AH_AB = TestObjectCreator.USDBankAccount.PK;

			Assert(!line.IsBankCurrencyLocal);
		}
		public void TestTotalDepositAmountWithForeignCurrency()
		{
			var line1 = Factory.New<DepositBatchTransactionLine>();
			line1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			line1.AH_OSTotal = 100M;
			line1.AH_OutstandingAmount = 700M;
			line1.AH_InvoiceAmount = 700M;

			AssertEquals(700m, line1.TotalDepositAmount);

			var line2 = Factory.New<DepositBatchTransactionLine>();
			line2.AH_AB = TestObjectCreator.USDBankAccount.PK;
			line2.AH_OSTotal = 200M;
			line2.AH_OutstandingAmount = 1400m;
			line2.AH_InvoiceAmount = 1400m;

			AssertEquals(200m, line2.TotalDepositAmount);
		}

		public void TestBranchCode()
		{
			DepositBatchTransactionLine testLine = Factory.New<DepositBatchTransactionLine>();
			testLine.AH_GB = ZGuid.Empty;
			AssertEquals(string.Empty, testLine.BranchCode);

			testLine.AH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, testLine.BranchCode);
		}

		public void TestBankCurrencyDecimals()
		{
			var testLine = Factory.New<DepositBatchTransactionLine>();
			testLine.AH_GB = ZGuid.Empty;
			var bankAccount = TestObjectCreator.AUDBankAccount;
			testLine.AH_AB = bankAccount.PK;
			AssertEquals(bankAccount.AccountCurrency.Decimals, testLine.BankCurrencyDecimals);

			bankAccount = TestObjectCreator.GBPBankAccount;
			testLine.AH_AB = bankAccount.PK;
			AssertEquals(bankAccount.AccountCurrency.Decimals, testLine.BankCurrencyDecimals);
		}

		public void TestCorrectSigns()
		{
			DepositBatchTransactionLine testLine = Factory.New<DepositBatchTransactionLine>();
			testLine.AH_TransactionType = TransactionTypes.Receipt;
			AssertEquals(-10M, testLine.CorrectSigns_ForTestOnly(10M));

			testLine.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals(10M, testLine.CorrectSigns_ForTestOnly(10M));
		}

		public void TestReadOnlyFields()
		{
			DepositBatchTransactionLine testLine = Factory.New<DepositBatchTransactionLine>();

			AssertEquals(true, testLine.AH_TransactionTypeInfo.ReadOnly);
			AssertEquals(true, testLine.AH_InvoiceDateInfo.ReadOnly);
			AssertEquals(true, testLine.AH_TransactionNumInfo.ReadOnly);
			AssertEquals(true, testLine.AH_OSTotalInfo.ReadOnly);
			AssertEquals(true, testLine.CurrencyCodeInfo.ReadOnly);
			AssertEquals(true, testLine.BankAccountNumberInfo.ReadOnly);
			AssertEquals(true, testLine.BankNameInfo.ReadOnly);
			AssertEquals(true, testLine.BankCodeInfo.ReadOnly);
			AssertEquals(true, testLine.AH_LedgerInfo.ReadOnly);
			AssertEquals(true, testLine.AH_ReceiptTypeInfo.ReadOnly);
			AssertEquals(true, testLine.AH_ChequeOrReferenceInfo.ReadOnly);
			AssertEquals(true, testLine.AH_ChequeDrawerInfo.ReadOnly);
			AssertEquals(true, testLine.AH_DrawerBankInfo.ReadOnly);
			AssertEquals(true, testLine.AH_DrawerBranchInfo.ReadOnly);
		}

		public void TestDefaultValueOfIsSelected()
		{
			DepositBatchTransactionLine testLine = Factory.New<DepositBatchTransactionLine>();
			AssertEquals(true, testLine.IsSelected);
		}

		public void TestInvertSigns()
		{
			DepositBatchTransactionLine testLine = Factory.New<DepositBatchTransactionLine>();
			testLine.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(true, testLine.InvertSigns_ForTestOnly);

			testLine.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(true, testLine.InvertSigns_ForTestOnly);

			testLine.AH_Ledger = LedgerTypes.CashBook;
			AssertEquals(false, testLine.InvertSigns_ForTestOnly);
		}

		public void TestNumberFountain()
		{
			DepositBatchTransactionLine testLine = Factory.New<DepositBatchTransactionLine>();
			AssertEquals(null, testLine.NumberFountainForTransactionNumber_ForTestOnly);
		}

		public void TestIsSelectedReadOnly()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch testDepositBatch = Factory.New<DepositBatch>();
			testDepositBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			AssertEquals(false, testDepositBatch.Transactions[0].IsSelectedInfo.ReadOnly);
			Factory.Save();
			AssertEquals(true, testDepositBatch.Transactions[0].IsSelectedInfo.ReadOnly);
		}

		public override void TestGetWritableProperties()
		{
			AssertEquals("GetWritableProperties().Count", 1, ((DepositBatchTransactionLine)Header).GetWritableProperties_ForTestOnly().Count);
			AssertEquals("GetWritableProperties()[0]", "IsSelected", ((DepositBatchTransactionLine)Header).GetWritableProperties_ForTestOnly()[0]);
		}

		public override void TestFetchHints()
		{
			Assert(true);
		}

		public new void TestPostDateValidation()
		{
			Assert(true);
		}

		public override void TestPostingToPriorNonClosedPeriodGivesWarning()
		{
			Assert(true);
		}

		public override void TestPropertiesSetToReadOnlyOnLoaded()
		{
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TransactionHeader header = newFactory.Load<DepositBatchTransactionLine>(Header.PK);
			AssertPropertiesAreReadOnly(header);
		}

		public override void TestReverseTransaction()
		{
			Assert("Reversing is not supported", true);
		}

		public override void TestSetDefaultValues()
		{
			Assert(true);
		}

		public override void TestSetTransactionTypeAsDefault()
		{
			Assert(true);
		}

		public override void TestTransactionNumberGenerator()
		{
			Assert(true);
		}

		public override void TestTransactionNumberOnSave()
		{
			Assert(true);
		}

		public override void TestDefaultPostDateReadOnly()
		{
			Assert(true);
		}

		#region Implementations

		protected override BusinessObject GetNewBusinessObject()
		{
			var obj = (DepositBatchTransactionLine)Factory.New(GetExpectedBusinessObjectType());
			if (obj.AH_TransactionType.IsEmpty)
			{
				obj.AH_TransactionType = TransactionTypes.Receipt;
			}
			if (obj.AH_Ledger.IsEmpty)
			{
				obj.AH_Ledger = LedgerTypes.AccountsReceivable;
			}
			if (obj.AH_TransactionNum.IsEmpty)
			{
				obj.AH_TransactionNum = "Test001";
			}
			return obj;
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(DepositBatchTransactionLineValidation); }
		}

		protected override Type TypeOfEmptyValidation
		{
			get { return typeof(DepositBatchTransactionLineValidation); }
		}

		protected override void SetupForSave()
		{
			base.SetupForSave();
			Header.AH_TransactionNum = "000221226";
		}

		#endregion
	}
}
