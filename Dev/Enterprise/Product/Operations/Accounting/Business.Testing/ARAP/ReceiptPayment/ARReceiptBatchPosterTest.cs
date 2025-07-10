using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(ARReceiptBatchPoster))]
	public class ARReceiptBatchPosterTest : NonPersistentBusinessObjectTestCase
	{
		#region Other Tests

		[ExpectNoExceptions()]
		public void TestSuccessfulPostingWithCreatingDepositBatchAndMatching()
		{
			ARReceiptBatchPosterForTest batchPoster = new ARReceiptBatchPosterForTest(Factory);
			PrepareForSuccessfulPost(batchPoster);
			Assert("Include in Deposit Batch should be defaulted to true", TestReceipt1.IncludeInDepositBatch);
			Assert("Include in Deposit Batch should be defaulted to true", TestReceipt2.IncludeInDepositBatch);
			TestReceipt2.IncludeInDepositBatch = ZBool.False;

			Assert("NeedToCreateDepositBatch_ForTestOnly flag should be set", batchPoster.NeedToCreateDepositBatch_ForTestOnly);
			AssertEquals("TestReceipt2 should not have it's own default bank set", TestBank.PK, TestReceipt2.AH_AB);
			batchPoster.RunPreSaveValidation();
			Assert("Batch Poster should have errors as amounts aren't set yet", batchPoster.HasErrors);

			TestReceipt1.AH_OSExTaxAmount = 150;
			AssertEquals("Local amount should be calculated on the TestReceipt1", 63.83M, TestReceipt1.AH_LocalExTaxAmount);
			AssertEquals("Total Foreign amount should be updated", 150M, batchPoster.ForeignCurrencyTotal);
			AssertEquals("Total Local amount should be updated", 63.83M, batchPoster.LocalCurrencyTotal);
			TestReceipt2.AH_OSExTaxAmount = 100;
			AssertEquals("Total Foreign amount should be updated", 250M, batchPoster.ForeignCurrencyTotal);
			AssertEquals("Total Local amount should be updated", 106.38M, batchPoster.LocalCurrencyTotal);
			TestReceipt3.AH_OSExTaxAmount = 0.05;
			AssertEquals("Total Foreign amount should be updated", 250.05M, batchPoster.ForeignCurrencyTotal);
			AssertEquals("Total Local amount should be updated", 106.40M, batchPoster.LocalCurrencyTotal);
			batchPoster.RunPreSaveValidation();
			Assert("Batch Poster should not have errors any more", !batchPoster.HasErrors);
			Assert("There should not be any messages shown yet", !batchPoster.DepositSlipPrintedForTest);

			Factory.Save();

			Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
			Assert("TestReceipt3 should have been saved", TestReceipt3.IsInDatabase);
			Assert("TestReceipt2 should have been saved", TestReceipt2.IsInDatabase);
			Assert("Flag should be set", batchPoster.IsDepositBatchCreated_ForTestOnly);
			Assert("NeedToCreateDepositBatch_ForTestOnly flag should not be set any more", !batchPoster.NeedToCreateDepositBatch_ForTestOnly);
			AssertNotNull("Deposit Batch should be created", batchPoster.RelatedDepositBatch_ForTestOnly);
			Assert("Deposit batch should be saved", batchPoster.RelatedDepositBatch_ForTestOnly.IsInDatabase);
			AssertEquals("There should be only 2 transactions in deposit batch", 2, batchPoster.RelatedDepositBatch_ForTestOnly.Transactions.Count);
			Assert("Deposit batch transactions should contain TestReceipt1", batchPoster.RelatedDepositBatch_ForTestOnly.Transactions.Contains(TestReceipt1));
			Assert("Deposit batch transactions should contain TestReceipt3", batchPoster.RelatedDepositBatch_ForTestOnly.Transactions.Contains(TestReceipt3));

			Assert("Deposit Slip should be printed", batchPoster.IsDepositBatchPrinted_ForTestOnly);
			Assert("Deposit Slip should be printed", batchPoster.DepositSlipPrintedForTest);
		}

		public void TestReceiptBatchIsEditableChildObject()
		{
			BatchPoster.BankAccountPK = TestBank.PK;
			BatchPoster.ExchangeRate.Rate = 2.35M;
			TestReceipt1 = BatchPoster.ReceiptBatch.AddNew();
			TestReceipt1.AH_OH = TestOrg.PK;
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_ChequeOrReference = "1";
			BatchPoster.RunPreSaveValidation();
			Assert("Batch Poster should not have errors any more", !BatchPoster.HasErrors);
			TestReceipt1.AH_OH = ZGuid.Empty;
			BatchPoster.RunPreSaveValidation();
			Assert("Batch Poster should have an error as TestReceipt1 does and it's registered", BatchPoster.HasErrors);
		}

		[ExpectNoExceptions()]
		public void TestRemoveReceiptFromBatch()
		{
			ARReceipt newReceipt = Factory.NewWithValidTestData<ARReceipt>();
			TestReceipt1 = BatchPoster.ReceiptBatch.AddNew();
			TestReceipt2 = BatchPoster.ReceiptBatch.AddNew();
			AssertEquals("There should be 2 receipts in the batch", 2, BatchPoster.ReceiptBatch.Count);
			BatchPoster.RemoveReceiptFromBatch(null);
			BatchPoster.RemoveReceiptFromBatch(newReceipt);
			BatchPoster.RemoveReceiptFromBatch(TestReceipt2);
			AssertEquals("There should be 1 receipt in the batch", 1, BatchPoster.ReceiptBatch.Count);
			Assert("Receipt Batch should contain TestReceipt1", BatchPoster.ReceiptBatch.Contains(TestReceipt1));
		}

		public void TestPrintingMessageIsShownOnlyOnceIfFactorySavedTwice()
		{
			ARReceiptBatchPosterForTest batchPoster = new ARReceiptBatchPosterForTest(Factory);
			PrepareForSuccessfulPost(batchPoster);
			batchPoster.CreateDepositSlip = ZBool.True;
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 100;
			TestReceipt2.AH_LocalExTaxAmount = 100;
			TestReceipt3.AH_OSExTaxAmount = 0.05;
			TestReceipt3.AH_LocalExTaxAmount = 0.05;
			batchPoster.RunPreSaveValidation();
			Assert("Batch Poster should have no errors", !batchPoster.HasErrors);
			Assert("Deposit slip should not be printed yet", !batchPoster.DepositSlipPrintedForTest);
			Factory.Save();
			Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
			Assert("Deposit slip should be printed", batchPoster.DepositSlipPrintedForTest);
			batchPoster.ResetEventResults();
			Factory.Save();
			Assert("Should not fire printing event again", !batchPoster.DepositSlipPrintedForTest);
		}

		public void TestReceiptBatchIsCreatedOnlyOnceIfFactorySavedTwice()
		{
			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_LocalExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_LocalExTaxAmount = 150;
			BatchPoster.CreateDepositSlip = ZBool.True;
			Factory.Save();
			Assert("Receipt Batch should be saved", TestReceipt1.IsInDatabase);
			AssertNotNull("Deposit Batch should be created", BatchPoster.RelatedDepositBatch_ForTestOnly);
			Assert("Deposit batch should be saved", BatchPoster.RelatedDepositBatch_ForTestOnly.IsInDatabase);

			BatchPoster.RelatedDepositBatch_ForTestOnly = null;
			Factory.Save();
			AssertNull("Deposit Batch should stay null", BatchPoster.RelatedDepositBatch_ForTestOnly);
		}

		#endregion

		#region Public Members Tests

		public void TestCashAccountReceiptTypeValidation()
		{
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			BatchPoster.BankAccountPK = cashAccount.PK;

			BatchPoster.ReceiptType = ReceiptTypes.Cash;
			AssertNoErrors("Setting receipt type to cash is valid for cash accounts", BatchPoster.ReceiptTypeInfo);

			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			AssertHasError("Only cash receipt type is valid for cash accounts", BatchPoster.ReceiptTypeInfo, "For Cash Account, please select CSH - Cash Receipt Type.");
		}

		public void TestSettingCashAccountSetsReceiptType()
		{
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			BatchPoster.ReceiptType = ReceiptTypes.Cheque; // a receipt type other than 'Cash'
			BatchPoster.BankAccountPK = cashAccount.PK;
			Assert("Setting a cash account should set receipt type to cash", BatchPoster.ReceiptType == ReceiptTypes.Cash);
		}

		public void TestSettingCashReceiptTypeMakesReadOnly()
		{
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			SetUpBatchPosterWith2Receipts();

			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			foreach (ARReceipt aRReceipt in BatchPoster.ReceiptBatch)
			{
				Assert("Setting receipt type to other than cash should make readonly false", !aRReceipt.AH_ReceiptTypeInfo.ReadOnly);
			}

			BatchPoster.ReceiptType = ReceiptTypes.Cash;
			BatchPoster.BankAccountPK = cashAccount.PK;
			foreach (ARReceipt aRReceipt in BatchPoster.ReceiptBatch)
			{
				Assert("Setting receipt type to cash should make readonly true", aRReceipt.AH_ReceiptTypeInfo.ReadOnly);
			}
		}

		public void TestChangingPostDateUpdatesAH_PostDateOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			ZDateTime testDate = ZDateTime.Now.AddDays(2).Date;

			BatchPoster.PostDate = testDate;
			AssertEquals("AH_PostDate should be updated on TestReceipt1", testDate, TestReceipt1.AH_PostDate);
			AssertEquals("AH_PostDate should be updated on TestReceipt2", testDate, TestReceipt2.AH_PostDate);
		}

		public void TestChangingInvoiceDateUpdatesAH_InvoiceDateOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			ZDateTime testDate = ZDateTime.Now.AddDays(2).Date;

			BatchPoster.InvoiceDate = testDate;
			AssertEquals("AH_InvoiceDate should be updated on TestReceipt1", testDate, TestReceipt1.AH_InvoiceDate);
			AssertEquals("AH_InvoiceDate should be updated on TestReceipt2", testDate, TestReceipt2.AH_InvoiceDate);
		}

		public void TestChangingDescriptionUpdatesAH_DescOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			ZString testDescription = "BLAH! :)";

			BatchPoster.Description = testDescription;
			AssertEquals("AH_Desc should be updated on TestReceipt1", testDescription, TestReceipt1.AH_Desc);
			AssertEquals("AH_Desc should be updated on TestReceipt2", testDescription, TestReceipt2.AH_Desc);
		}

		public void TestAskUserIfNeededBeforeDescriptionUpdate()
		{
			ARReceiptBatchPosterForTest batchPoster = new ARReceiptBatchPosterForTest(Factory);
			Assert("Message should not be shown when the Description changed during setting defaults", !batchPoster.AskBeforeUpdateDescriptionFired);
			SetUpBatchPosterWith2Receipts(batchPoster);
			batchPoster.Description = "1";
			Assert("Message should not be shown as all receipts have the same AH_Desc", !batchPoster.AskBeforeUpdateDescriptionFired);

			TestReceipt1.AH_Desc = "Something Different";
			batchPoster.Result_AskBeforeUpdateDescription = ZBool.False;
			batchPoster.Description = "2";
			Assert("Message should be shown as there is more than one receipt in collection and one of them has different value", batchPoster.AskBeforeUpdateDescriptionFired);
			AssertEquals("Description should not be updated as the answer was NO", "1", batchPoster.Description);
			AssertEquals("Description should not be updated as the answer was NO", "Something Different", TestReceipt1.AH_Desc);
			AssertEquals("Description should not be updated as the answer was NO", "1", TestReceipt2.AH_Desc);

			batchPoster.ResetEventResults();
			batchPoster.Result_AskBeforeUpdateDescription = ZBool.True;
			batchPoster.Description = "2";
			AssertEquals("Description should be updated as the answer was YES", "2", batchPoster.Description);
			AssertEquals("Description should be updated as the answer was YES", "2", TestReceipt1.AH_Desc);
			AssertEquals("Description should be updated as the answer was YES", "2", TestReceipt2.AH_Desc);
		}

		public void TestChangingReceiptTypeUpdatesAH_ReceiptTypeOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			ZString testReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;

			BatchPoster.ReceiptType = testReceiptType;
			AssertEquals("AH_ReceiptType should be updated on TestReceipt1", testReceiptType, TestReceipt1.AH_ReceiptType);
			AssertEquals("AH_ReceiptType should be updated on TestReceipt2", testReceiptType, TestReceipt2.AH_ReceiptType);
		}

		public void TestChangingReceiptTypeWillUpdateChequeOrReference()
		{
			SetUpBatchPosterWith2Receipts();
			BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("AH_ChequeOrReference should be updated on TestReceipt1", ZArchitecture.Core.ReceiptTypes.Cash, TestReceipt1.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be updated on TestReceipt2", ZArchitecture.Core.ReceiptTypes.Cash, TestReceipt2.AH_ChequeOrReference);

			BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("AH_ChequeOrReference should be updated on TestReceipt1", ZString.Empty, TestReceipt1.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be updated on TestReceipt2", ZString.Empty, TestReceipt2.AH_ChequeOrReference);

			TestReceipt1.AH_ChequeOrReference = "BLAH!";
			TestReceipt2.AH_ChequeOrReference = "BLAH!";
			BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("AH_ChequeOrReference should be updated on TestReceipt1", ZString.Empty, TestReceipt1.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be updated on TestReceipt2", ZString.Empty, TestReceipt2.AH_ChequeOrReference);
		}

		public void TestAskUserIfNeededBeforeReceiptTypeUpdate()
		{
			ARReceiptBatchPosterForTest batchPoster = new ARReceiptBatchPosterForTest(Factory);
			Assert("Message should not be shown when the Receipt Type changed during setting defaults", !batchPoster.AskBeforeUpdateReceiptTypeFired);
			SetUpBatchPosterWith2Receipts(batchPoster);
			batchPoster.ReceiptType = ReceiptTypes.Cash;
			Assert("Message should not be shown as all receipts have the same AH_ReceiptType", !batchPoster.AskBeforeUpdateReceiptTypeFired);
			TestReceipt1.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			batchPoster.Result_AskBeforeUpdateReceiptType = ZBool.False;
			batchPoster.ReceiptType = ReceiptTypes.Cheque;
			Assert("Message should be shown as there is more than one receipt in collection and one of them has different value", batchPoster.AskBeforeUpdateReceiptTypeFired);
			AssertEquals("ReceiptType should not be updated as the answer was NO", ReceiptTypes.Cash, batchPoster.ReceiptType);
			AssertEquals("ReceiptType should not be updated as the answer was NO", ReceiptTypes.DirectDebitLine, TestReceipt1.AH_ReceiptType);
			AssertEquals("ReceiptType should not be updated as the answer was NO", ReceiptTypes.Cash, TestReceipt2.AH_ReceiptType);

			batchPoster.ResetEventResults();
			batchPoster.Result_AskBeforeUpdateReceiptType = ZBool.True;
			batchPoster.ReceiptType = ReceiptTypes.Cheque;
			AssertEquals("ReceiptType should be updated as the answer was YES", ReceiptTypes.Cheque, batchPoster.ReceiptType);
			AssertEquals("ReceiptType should be updated as the answer was YES", ReceiptTypes.Cheque, TestReceipt1.AH_ReceiptType);
			AssertEquals("ReceiptType should be updated as the answer was YES", ReceiptTypes.Cheque, TestReceipt2.AH_ReceiptType);
		}

		public void TestChangingBankAccountUpdatesAH_ABOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			BatchPoster.BankAccountPK = TestBank.PK;
			AssertEquals("AH_AB should be updated on TestReceipt1", TestBank.PK, TestReceipt1.AH_AB);
			AssertEquals("AH_AB should be updated on TestReceipt2", TestBank.PK, TestReceipt2.AH_AB);
		}

		public void TestChangingBankAccountResetsChequeANDCurrency()
		{
			SetUpBatchPosterWith2Receipts();
			RefCurrency newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount newBank = Factory.NewWithValidTestData<AccBankAccount>();
			newBank.AB_RX_NKAccountCurrency = newCurrency.RX_Code;
			TestBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			BatchPoster.BankAccountPK = TestBank.PK;
			TestReceipt1.AH_ChequeOrReference = "1";
			TestReceipt2.AH_ChequeOrReference = "2";
			AssertEquals("AH_ChequeOrReference should be set on TestReceipt1", "1", TestReceipt1.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be set on TestReceipt2", "2", TestReceipt2.AH_ChequeOrReference);
			BatchPoster.BankAccountPK = newBank.PK;
			AssertEquals("Currency should be changed", newCurrency.RX_Code, BatchPoster.ExchangeRate.Currency);
			AssertEquals("AH_ChequeOrReference should be reset on TestReceipt1", ZString.Empty, TestReceipt1.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be reset on TestReceipt2", ZString.Empty, TestReceipt2.AH_ChequeOrReference);
		}

		public void TestChangingSellExRateUpdatesAH_ExchangeRateOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			BatchPoster.BankAccountPK = TestBank.PK;
			BatchPoster.ExchangeRate.Rate = 2.5M;
			AssertEquals("AH_ExchangeRate should be updated on TestReceipt1", 2.5M, TestReceipt1.AH_ExchangeRate);
			AssertEquals("AH_ExchangeRate should be updated on TestReceipt2", 2.5M, TestReceipt2.AH_ExchangeRate);
		}

		public void TestChangingCreateDepositSlipUpdatesIncludeInDepositBatchOnReceipts()
		{
			SetUpBatchPosterWith2Receipts();
			ARReceipt testReceipt3 = BatchPoster.ReceiptBatch.AddNew();

			testReceipt3.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			TestReceipt1.IncludeInDepositBatch = ZBool.True;
			BatchPoster.CreateDepositSlip = ZBool.False;
			Assert("IncludeInDepositBatch should be updated for TestReceipt1", !TestReceipt1.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be updated for TestReceipt2", !TestReceipt2.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be updated for TestReceipt3", !testReceipt3.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be read only", TestReceipt1.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should be read only", TestReceipt2.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should be read only", testReceipt3.IncludeInDepositBatchInfo.ReadOnly);
			ARReceipt testReceipt4 = BatchPoster.ReceiptBatch.AddNew();
			Assert("IncludeInDepositBatch should be set correctly for new receipt", !testReceipt4.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be set read only for new receipt", testReceipt4.IncludeInDepositBatchInfo.ReadOnly);

			BatchPoster.CreateDepositSlip = ZBool.True;
			Assert("IncludeInDepositBatch should be updated for TestReceipt1", TestReceipt1.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be updated for TestReceipt2", TestReceipt2.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be updated for TestReceipt3", !testReceipt3.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be read only", !TestReceipt1.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should not be read only", !TestReceipt2.IncludeInDepositBatchInfo.ReadOnly);
			Assert("IncludeInDepositBatch should be read only", testReceipt3.IncludeInDepositBatchInfo.ReadOnly);
			testReceipt4 = BatchPoster.ReceiptBatch.AddNew();
			Assert("IncludeInDepositBatch should be set correctly for new receipt", testReceipt4.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be set to read only for new receipt", !testReceipt4.IncludeInDepositBatchInfo.ReadOnly);

			BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			testReceipt4 = BatchPoster.ReceiptBatch.AddNew();
			Assert("IncludeInDepositBatch should be set correctly for new receipt", !testReceipt4.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be set read only for new receipt", testReceipt4.IncludeInDepositBatchInfo.ReadOnly);
			testReceipt4.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("IncludeInDepositBatch should be set correctly for new receipt", testReceipt4.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be set to read only for new receipt", !testReceipt4.IncludeInDepositBatchInfo.ReadOnly);
		}

		public void TestCalc_LocalRX()
		{
			AssertEquals("Should return local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, BatchPoster.Calc_LocalRX_NK);
		}

		public void TestCalc_LocalRXDecimals()
		{
			RefCurrency newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			newCurrency.RX_SubUnitRatio = 100000;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = newCurrency.RX_Code;
			Factory.Save();
			AssertEquals("Should return local currency's decimals", 5, BatchPoster.Calc_LocalRXDecimals);
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = ZString.Empty;
			AssertEquals("Should return default value", 2, BatchPoster.Calc_LocalRXDecimals);
		}

		public void TestCalc_RXDecimals()
		{
			RefCurrency newCurrency = Factory.NewWithValidTestData<RefCurrency>();
			newCurrency.RX_SubUnitRatio = 100000;
			Factory.Save();
			BatchPoster.RX_NK = ZString.Empty;
			AssertEquals("Should return default value", 2, BatchPoster.Calc_RXDecimals);
			BatchPoster.RX_NK = newCurrency.RX_Code;
			AssertEquals("Should return local currency's decimals", 5, BatchPoster.Calc_RXDecimals);
		}

		#endregion

		#region Implementation Memebers Tests

		public void TestRemoveNewlyCreatedReceiptsFromUnmatchedTransactions()
		{
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_IsCreditor = true;
			TestOrg2.OH_IsDebtor = true;

			TestOrg.ARSettlementGroupPK = TestOrg2.PK;
			TestOrg.OH_IsCreditor = true;
			TestOrg.OH_IsDebtor = true;

			TestReceipt1 = Factory.NewWithValidTestData<ARReceipt>();
			TestReceipt1.AH_OH = TestOrg.PK;
			TestReceipt1.AH_OSExTaxAmount = 10M;

			TestReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			TestReceipt2.AH_OH = TestOrg.PK;
			TestReceipt2.AH_OSExTaxAmount = 10M;

			TestReceipt3 = Factory.NewWithValidTestData<ARReceipt>();
			TestReceipt3.AH_OH = TestOrg2.PK;
			TestReceipt3.AH_OSExTaxAmount = 10M;

			TestReceipt4 = Factory.NewWithValidTestData<ARReceipt>();
			TestReceipt4.AH_OH = TestOrg2.PK;
			TestReceipt4.AH_OSExTaxAmount = 10M;

			Factory.Save();

			AssertEquals("Matching Object for TestReceipt2 will have only 1 unmatched transaction", 1, TestReceipt2.MatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Normally, TestReceipt2 will have TestReceipt1 in the Unmatched transactions collection", TestReceipt2.MatchingBaseObject.UnmatchedTransactions.Contains(TestReceipt1));
			AssertEquals("Matching Object for TestReceipt1 will have only 1 unmatched transaction", 1, TestReceipt1.MatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Normally, TestReceipt1 will have TestReceipt2 in the Unmatched transactions collection", TestReceipt1.MatchingBaseObject.UnmatchedTransactions.Contains(TestReceipt2));
			AssertEquals("Matching Object for TestReceipt3 will have all 3 transactions as Unmatched Transactions", 3, TestReceipt3.MatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Normally, TestReceipt3 will have TestReceipt1 in the Unmatched transactions collection", TestReceipt3.MatchingBaseObject.UnmatchedTransactions.Contains(TestReceipt1));
			Assert("Normally, TestReceipt3 will have TestReceipt2 in the Unmatched transactions collection", TestReceipt3.MatchingBaseObject.UnmatchedTransactions.Contains(TestReceipt2));
			Assert("TestReceipt3 should have TestReceipt4 in the Unmatched transactions collection", TestReceipt3.MatchingBaseObject.UnmatchedTransactions.Contains(TestReceipt4));

			BatchPoster.ReceiptBatch.Add(TestReceipt1);
			BatchPoster.ReceiptBatch.Add(TestReceipt2);
			BatchPoster.ReceiptBatch.Add(TestReceipt3);
			BatchPoster.MatchAfterPosting = ZBool.True;
			BatchPoster.ExchangeRate.Rate = 1M;
			BatchPoster.ReceiptType = ReceiptTypes.Cash;
			Assert("MatchingObjectsPrepared_ForTestOnly should be false yet", !BatchPoster.MatchingObjectsPrepared_ForTestOnly);
			Assert("MatchingObjectsPreparedForTest_ForTestOnly should be false yet", !BatchPoster.MatchingObjectsPreparedForTest_ForTestOnly);
			BatchPoster.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Unmatching collection of TestReceipt1 should be empty", 0, TestReceipt1.MatchingBaseObject.UnmatchedTransactions.Count);
			AssertEquals("Unmatching collection of TestReceipt2 should be empty", 0, TestReceipt2.MatchingBaseObject.UnmatchedTransactions.Count);
			AssertEquals("Unmatching collection of TestReceipt3 should contain only 1 trnasction", 1, TestReceipt3.MatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Unmatching colleciton of TestReceipt3 should contain TestReceipt4, as it wasn't posted by poster", TestReceipt3.MatchingBaseObject.UnmatchedTransactions.Contains(TestReceipt4));
			Assert("MatchingObjectsPrepared_ForTestOnly should be set to true", BatchPoster.MatchingObjectsPrepared_ForTestOnly);
			Assert("MatchingObjectsPreparedForTest_ForTestOnly should be set to true", BatchPoster.MatchingObjectsPreparedForTest_ForTestOnly);
		}

		public void TestRemoveNewlyCreatedReceiptsFromUnmatchedTransactions_WontHitIfMatchingIsSwitchedOff()
		{
			Assert("Should be set to false by default", !BatchPoster.MatchingObjectsPrepared_ForTestOnly);
			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_LocalExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_LocalExTaxAmount = 150;
			BatchPoster.MatchAfterPosting = ZBool.False;
			BatchPoster.RunPreSaveValidation();
			Assert("Batch Poster should not have any errors", !BatchPoster.HasErrors);

			Factory.Save();
			Assert("Should not prepare matching bases as MatchingAfterPosting was set to false", !BatchPoster.MatchingObjectsPrepared_ForTestOnly);
			Assert("MatchingObjectsPreparedForTest_ForTestOnly should be false yet", !BatchPoster.MatchingObjectsPreparedForTest_ForTestOnly);
		}

		public void TestRemoveNewlyCreatedReceiptsFromUnmatchedTransactions_WontHitTwice()
		{
			Assert("Should be set to false by default", !BatchPoster.MatchingObjectsPrepared_ForTestOnly);
			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_LocalExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_LocalExTaxAmount = 150;
			BatchPoster.MatchAfterPosting = ZBool.True;
			BatchPoster.RunPreSaveValidation();
			Assert("Batch Poster should not have any errors", !BatchPoster.HasErrors);

			Factory.Save();
			Assert("Should prepare matching bases", BatchPoster.MatchingObjectsPrepared_ForTestOnly);
			Assert("MatchingObjectsPreparedForTest_ForTestOnly should be set to true", BatchPoster.MatchingObjectsPreparedForTest_ForTestOnly);
			BatchPoster.MatchingObjectsPreparedForTest_ForTestOnly = ZBool.False;
			Factory.Save();
			Assert("Should not prepare matching bases for the second time", !BatchPoster.MatchingObjectsPreparedForTest_ForTestOnly);
		}

		public void TestSetDefaultValues()
		{
			Assert("Batch poster should not have any changes so far", !BatchPoster.HasChanges);
			AssertEquals(ZDateTime.Now.Date, BatchPoster.InvoiceDate.Date);
			AssertEquals(ZDateTime.Now.Date, BatchPoster.PostDate.Date);
			AssertEquals(ZArchitecture.Core.ReceiptTypes.Cheque, BatchPoster.ReceiptType);
			AssertEquals(ZGuid.Empty, BatchPoster.BankAccountPK);
			AssertEquals("AR RECEIPT", BatchPoster.Description);
			Assert(!BatchPoster.CreateDepositSlip);
			Assert(!BatchPoster.MatchAfterPosting);
			Assert("Batch poster should not have any changes", !BatchPoster.HasChanges);

			AccountingConfigurationRegistry.Instance.DefaultReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			BatchPoster = new ARReceiptBatchPoster(Factory);
			AssertEquals(ReceiptTypes.Cash, BatchPoster.ReceiptType);
		}

		public void TestDefaultAH_ChequeOrReference()
		{
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var batchPoster = new ARReceiptBatchPoster(Factory);
			batchPoster.ReceiptBatch.AddNew();
			batchPoster.ReceiptBatch.AddNew();
			AssertEquals(2, batchPoster.ReceiptBatch.Count);

			batchPoster.ReceiptType = ReceiptTypes.CreditCard;
			AssertEquals(ReceiptTypes.CreditCard, batchPoster.ReceiptType);
			AssertEquals("Test1", batchPoster.ReceiptBatch[0].AH_ChequeOrReference);
			AssertEquals("Test1", batchPoster.ReceiptBatch[1].AH_ChequeOrReference);

			batchPoster.ReceiptType = ReceiptTypes.Cash;
			AssertEquals(ReceiptTypes.Cash, batchPoster.ReceiptType);
			AssertEquals("CSH", batchPoster.ReceiptBatch[0].AH_ChequeOrReference);
			AssertEquals("CSH", batchPoster.ReceiptBatch[1].AH_ChequeOrReference);
		}

		public void TestDefaultPostDateReadOnly()
		{
			bool receivablesAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				BatchPoster = new ARReceiptBatchPoster(Factory);
				Assert("PostDate should not be readonly", !BatchPoster.PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				BatchPoster = new ARReceiptBatchPoster(Factory);
				Assert("PostDate should not be readonly", !BatchPoster.PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				BatchPoster = new ARReceiptBatchPoster(Factory);
				Assert("PostDate should not be readonly", !BatchPoster.PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				BatchPoster = new ARReceiptBatchPoster(Factory);
				Assert("PostDate should not be readonly", !BatchPoster.PostDateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = receivablesAllowed;
			}
		}

		public void TestNeedToCreateDepositBatch()
		{
			SetUpBatchPosterWith2Receipts();
			BatchPoster.CreateDepositSlip = ZBool.True;
			Assert(BatchPoster.NeedToCreateDepositBatch_ForTestOnly);
			BatchPoster.CreateDepositSlip = ZBool.False;
			Assert(!BatchPoster.NeedToCreateDepositBatch_ForTestOnly);
			BatchPoster.CreateDepositSlip = ZBool.True;
			BatchPoster.IsDepositBatchCreated_ForTestOnly = ZBool.True;
			Assert(!BatchPoster.NeedToCreateDepositBatch_ForTestOnly);

			BatchPoster = new ARReceiptBatchPoster(Factory);
			BatchPoster.CreateDepositSlip = ZBool.True;
			Assert(!BatchPoster.NeedToCreateDepositBatch_ForTestOnly);
		}

		public void TestNeedToPrintDepositBatch()
		{
			PrepareForSuccessfulPost();
			BatchPoster.CreateDepositSlip = ZBool.True;
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 100;
			TestReceipt2.AH_LocalExTaxAmount = 100;
			TestReceipt3.AH_OSExTaxAmount = 0.05;
			TestReceipt3.AH_LocalExTaxAmount = 0.05;
			BatchPoster.RunPreSaveValidation();
			Assert("Batch Poster should have no errors", !BatchPoster.HasErrors);
			Assert("Property should be set to false, as the Deposit Batch is null", !BatchPoster.NeedToPrintDepositBatch_ForTestOnly);
			BatchPoster.RelatedDepositBatch_ForTestOnly = Factory.NewWithValidTestData<DepositBatch>();
			Assert("Property should be set to false, as the Deposit Batch is not in database", !BatchPoster.NeedToPrintDepositBatch_ForTestOnly);
			BatchPoster.RelatedDepositBatch_ForTestOnly.Delete();
			Assert(!BatchPoster.IsDepositBatchPrinted_ForTestOnly);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Factory.Save();
			Assert(BatchPoster.IsDepositBatchPrinted_ForTestOnly);
			Assert(!BatchPoster.NeedToPrintDepositBatch_ForTestOnly);
			BatchPoster.IsDepositBatchPrinted_ForTestOnly = ZBool.False;
			Assert(BatchPoster.NeedToPrintDepositBatch_ForTestOnly);
		}

		public void TestPrintDepositSlip_DontPrintIfFlagNotSet()
		{
			ARReceiptBatchPosterForTest batchPoster = new ARReceiptBatchPosterForTest(Factory);
			PrepareForSuccessfulPost(batchPoster);
			batchPoster.CreateDepositSlip = ZBool.False;
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 100;
			TestReceipt3.AH_OSExTaxAmount = 0.05;
			batchPoster.RunPreSaveValidation();
			Assert("Batch Poster should have no errors", !batchPoster.HasErrors);
			Factory.Save();
			Assert("TestReceipt1 should have been saved", TestReceipt1.IsInDatabase);
			Assert("Deposit Slip should not be printed as flag was set to False", !batchPoster.DepositSlipPrintedForTest);
		}

		public void TestCreateDepositBatch_OptionNotSelected()
		{
			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			BatchPoster.CreateDepositSlip = ZBool.False;
			Factory.Save();
			Assert("Receipt Batch should be saved", TestReceipt1.IsInDatabase);
			AssertNull("Deposit Batch should not be created", BatchPoster.RelatedDepositBatch_ForTestOnly);
		}

		public void TestCreateDepositBatch_OptionSelectedButNoneOfReceiptsTicked()
		{
			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			BatchPoster.CreateDepositSlip = ZBool.True;
			TestReceipt1.IncludeInDepositBatch = ZBool.False;
			TestReceipt2.IncludeInDepositBatch = ZBool.False;
			TestReceipt3.IncludeInDepositBatch = ZBool.False;
			Factory.Save();
			Assert("Receipt Batch should be saved", TestReceipt1.IsInDatabase);
			AssertNull("Deposit Batch should not be created", BatchPoster.RelatedDepositBatch_ForTestOnly);
		}

		public void TestCreateDepositBatch_OptionSelectedReceiptsPartlyTicked()
		{
			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_LocalExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_LocalExTaxAmount = 150;
			BatchPoster.CreateDepositSlip = ZBool.True;
			TestReceipt2.IncludeInDepositBatch = ZBool.False;
			Factory.Save();
			Assert("Receipt Batch should be saved", TestReceipt1.IsInDatabase);
			AssertNotNull("Deposit Batch should be created", BatchPoster.RelatedDepositBatch_ForTestOnly);
			Assert("Deposit batch should be saved", BatchPoster.RelatedDepositBatch_ForTestOnly.IsInDatabase);
			AssertEquals("There should be only 2 transactions in deposit batch", 2, BatchPoster.RelatedDepositBatch_ForTestOnly.Transactions.Count);
			Assert("Deposit batch transactions should contain TestReceipt1", BatchPoster.RelatedDepositBatch_ForTestOnly.Transactions.Contains(TestReceipt1));
			Assert("Deposit batch transactions should contain TestReceipt3", BatchPoster.RelatedDepositBatch_ForTestOnly.Transactions.Contains(TestReceipt3));
			AssertEquals("Branch should be set correctly on DepositBatch", GlbBranch.CurrentBranch.PK, BatchPoster.RelatedDepositBatch_ForTestOnly.AH_GB);
			AssertEquals("Bank should be set correctly on DepositBatch", TestBank.PK, BatchPoster.RelatedDepositBatch_ForTestOnly.AH_AB);
		}

		public void TestDepositBatchPostDateIsSameAsBatchPosterPostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			PrepareForSuccessfulPost();
			TestReceipt1.AH_OSExTaxAmount = 150;
			TestReceipt1.AH_LocalExTaxAmount = 150;
			TestReceipt2.AH_OSExTaxAmount = 150;
			TestReceipt2.AH_LocalExTaxAmount = 150;
			TestReceipt3.AH_OSExTaxAmount = 150;
			TestReceipt3.AH_LocalExTaxAmount = 150;
			BatchPoster.CreateDepositSlip = ZBool.True;
			var testDate = ZDateTime.Today.AddDays(-2);
			BatchPoster.PostDate = testDate;
			Factory.Save();
			AssertNotNull("Deposit Batch should be created", BatchPoster.RelatedDepositBatch_ForTestOnly);
			AssertEquals("Deposit Batch Post Date is same as Batch Poster Post Date", testDate, BatchPoster.RelatedDepositBatch_ForTestOnly.AH_PostDate.Date);
			AssertEquals("Deposit Batch Deposit Date is same as Batch Poster Post Date", testDate, BatchPoster.RelatedDepositBatch_ForTestOnly.AH_InvoiceDate.Date);
		}

		#endregion

		#region LookUps Tests

		public void TestBankAccounts()
		{
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			TestBank.Delete();

			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			bank1.AB_GB = GlbBranch.CurrentBranch.PK;

			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();
			bank2.AB_GB = ZGuid.Empty;

			AccBankAccount bank3 = Factory.NewWithValidTestData<AccBankAccount>();
			bank3.AB_GB = newBranch.PK;

			BatchPoster = new ARReceiptBatchPoster(Factory);

			AssertNotNull("BankAccounts should not be null", BatchPoster.BankAccounts);
			AssertEquals("BankAccounts Type", typeof(AccBankAccountCollection), BatchPoster.BankAccounts.GetType());
			BatchPoster.BankAccounts.Load();
			Assert("BankAccounts contains TestBank", BatchPoster.BankAccounts.Contains(bank1.PK));
			Assert("BankAccounts contains Bank 2", BatchPoster.BankAccounts.Contains(bank2.PK));
			Assert("BankAccounts contains Bank 3", !BatchPoster.BankAccounts.Contains(bank3.PK));
		}

		public void TestBankAccounts_ContainsOnlyActiveBanks()
		{
			AccBankAccount inactiveBank = Factory.NewWithValidTestData<AccBankAccount>();
			inactiveBank.AB_IsActive = false;

			BatchPoster = new ARReceiptBatchPoster(Factory);
			BatchPoster.BankAccounts.Load();
			Assert("Should contain active bank", BatchPoster.BankAccounts.Contains(TestBank));
			Assert("Should not contain inactive bank", !BatchPoster.BankAccounts.Contains(inactiveBank));
		}

		public void TestReceiptMethods()
		{
			AssertEquals("ReceiptMethods Type", OLookUpEditType.ReceiptMethod, BatchPoster.ReceiptMethods.LookupEditType);
		}

		#endregion

		#region Validation Tests

		public void TestChequeValidationForReceiptType()
		{
			SetUpBatchPosterWith2Receipts();
			Assert("Precondition: Receipt Type should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			BatchPoster.ReceiptType = ZString.Empty;
			Assert("Receipt Type cannot be empty", BatchPoster.ReceiptTypeInfo.HasErrors());
			BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			BatchPoster.ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			Assert("Receipt Type cannot be invalid", BatchPoster.ReceiptTypeInfo.HasErrors());
		}

		public void TestValidateReceiptType_Security()
		{
			SetUpBatchPosterWith2Receipts();
			BatchPoster.ReceiptType = ZString.Empty;
			Env.Security.NewReceivablesReceiptCheque.IsAllowed = true;
			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			Assert("Receipt Type should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			Env.Security.NewReceivablesReceiptCheque.IsAllowed = false;
			BatchPoster.ReceiptType = ZString.Empty;
			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			AssertHasError(BatchPoster.ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

			Env.Security.NewReceivablesReceiptCash.IsAllowed = true;
			BatchPoster.ReceiptType = ReceiptTypes.Cash;
			Assert("Receipt Type should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			Env.Security.NewReceivablesReceiptCash.IsAllowed = false;
			BatchPoster.ReceiptType = ZString.Empty;
			BatchPoster.ReceiptType = ReceiptTypes.Cash;
			AssertHasError(BatchPoster.ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

			Env.Security.NewReceivablesReceiptCreditCard.IsAllowed = true;
			BatchPoster.ReceiptType = ReceiptTypes.CreditCard;
			Assert("Receipt Type should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			Env.Security.NewReceivablesReceiptCreditCard.IsAllowed = false;
			BatchPoster.ReceiptType = ZString.Empty;
			BatchPoster.ReceiptType = ReceiptTypes.CreditCard;
			AssertHasError(BatchPoster.ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");

			Env.Security.NewReceivablesReceiptDirectCredit.IsAllowed = true;
			BatchPoster.ReceiptType = ReceiptTypes.DirectCredit;
			Assert("Receipt Type should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			Env.Security.NewReceivablesReceiptDirectCredit.IsAllowed = false;
			BatchPoster.ReceiptType = ZString.Empty;
			BatchPoster.ReceiptType = ReceiptTypes.DirectCredit;
			AssertHasError(BatchPoster.ReceiptTypeInfo, "You do not have appropriate security rights to select this receipt type.");
		}

		public void TestValidateBankAccountPK_IfEmpty()
		{
			Assert("Precondition: Bank Account should have no errors", !BatchPoster.BankAccountPKInfo.HasErrors());
			BatchPoster.ValidateBankAccountPK_ForTestOnly();
			AssertHasErrors(BatchPoster.BankAccountPKInfo);
		}

		public void TesValidateBankAccountPK_IsValid()
		{
			BatchPoster.BankAccountPK = ZGuid.Invalid;
			BatchPoster.ValidateBankAccountPK_ForTestOnly();
			AssertHasError(BatchPoster.BankAccountPKInfo, "Please enter a valid bank account.");
		}

		public void TestValidateBankAccountPK_ProperCurrency()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccBankAccount testBank = testObjectCreator.CreateBankAccount("TST", "TEST", testObjectCreator.USD, null);

			BatchPoster.RX_NK = "USD";
			BatchPoster.BankAccountPK = testBank.PK;
			BatchPoster.ValidateBankAccountPK_ForTestOnly();

			Assert(!BatchPoster.BankAccountPKInfo.HasErrors());
		}

		public void TestValidateBankAccountPK_CurrencyDoesNotMatch()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccBankAccount testBank = testObjectCreator.CreateBankAccount("TST", "TEST", testObjectCreator.USD, null);

			BatchPoster.BankAccountPK = testBank.PK;
			BatchPoster.RX_NK = testObjectCreator.GBP.RX_Code;
			BatchPoster.ValidateBankAccountPK_ForTestOnly();

			AssertHasError(BatchPoster.BankAccountPKInfo, "Bank account currency does not match the invoice currency.");
		}

		public void TestValidatePostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			BatchPoster.BankAccountPK = TestBank.PK;
			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			BatchPoster.PostDate = ZDateTime.Empty;
			Assert("Should have an error", BatchPoster.PostDateInfo.HasErrors());

			BatchPoster.PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError(BatchPoster.PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			BatchPoster.PostDate = ZDateTime.Now;
			AssertNoErrors("Should Be no errors", BatchPoster.PostDateInfo);

			BatchPoster.PostDate = ZDateTime.Now.AddDays(-1);
			AssertHasWarning(BatchPoster.PostDateInfo, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems " +
				 System.Environment.NewLine + " - Financial Reports" +
				 System.Environment.NewLine + " - Sub-Ledger Reports" +
				 System.Environment.NewLine + " - Bank Reconciliation" +
				 System.Environment.NewLine + " - Reversing");
			AssertHasWarning(BatchPoster.PostDateInfo, "The receipt batch post date will be same as this post date.");
		}

		public void TestValidateInvoiceDate()
		{
			BatchPoster.BankAccountPK = TestBank.PK;
			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			BatchPoster.InvoiceDate = ZDateTime.Empty;
			Assert("Should have an error", BatchPoster.InvoiceDateInfo.HasErrors());

			BatchPoster.InvoiceDate = ZDateTime.Now;
			AssertNoErrors("Should Be no errors", BatchPoster.InvoiceDateInfo);
		}

		public void TestValidateSellExRate()
		{
			BatchPoster.BankAccountPK = TestBank.PK;
			BatchPoster.ReceiptType = ReceiptTypes.Cheque;
			BatchPoster.SellExRate = -1;
			Assert("Should have an error as PayExRate can not be negative", BatchPoster.SellExRateInfo.HasErrors());
			BatchPoster.SellExRate = 0;
			Assert("Should have an error as PayExRate can not equal zero", BatchPoster.SellExRateInfo.HasErrors());
			BatchPoster.SellExRate = 1;
			AssertNoErrors("Should be no errors", BatchPoster.SellExRateInfo);
		}

		public void TestValidateDescription()
		{
			Assert("Precondition: Description should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
			BatchPoster.Description = ZString.Empty;
			AssertHasError(BatchPoster.DescriptionInfo, "You must enter a description");
			BatchPoster.Description = "BLAH!";
			Assert("Description should have no errors", !BatchPoster.ReceiptTypeInfo.HasErrors());
		}

		public void TestValidateCreateDepositSlip()
		{
			Assert("CreateDepositSlip should not have errors by default", !BatchPoster.CreateDepositSlipInfo.HasErrors());
			SetUpBatchPosterWith2Receipts();
			BatchPoster.ReceiptType = ReceiptTypes.EFT;
			TestReceipt1.AH_ReceiptType = ReceiptTypes.Cheque;
			TestReceipt2.AH_ReceiptType = ReceiptTypes.EFT;
			BatchPoster.CreateDepositSlip = ZBool.True;
			Assert("CreateDepositSlip should not have errors as TestReceipt1 can be added to the Slip", !BatchPoster.CreateDepositSlipInfo.HasErrors());
			TestReceipt1.AH_ReceiptType = ReceiptTypes.Cash;
			Assert("CreateDepositSlip should not have errors as TestReceipt1 can be added to the Slip", !BatchPoster.CreateDepositSlipInfo.HasErrors());
			TestReceipt1.AH_ReceiptType = ReceiptTypes.CreditCard;
			Assert("CreateDepositSlip should not have errors as TestReceipt1 can be added to the Slip", !BatchPoster.CreateDepositSlipInfo.HasErrors());
			TestReceipt1.AH_ReceiptType = ReceiptTypes.InterestPaid;
			Assert("CreateDepositSlip should have error as there are no Receipts that can be added to the Slip", BatchPoster.CreateDepositSlipInfo.HasErrors());
			Assert("CreateDepositSlip should be still set to True", BatchPoster.CreateDepositSlip);
			BatchPoster.CreateDepositSlip = ZBool.False;
			Assert("CreateDepositSlip should not have any errors", !BatchPoster.CreateDepositSlipInfo.HasErrors());
			BatchPoster.CreateDepositSlip = ZBool.True;
			Assert("CreateDepositSlip should have error as there are no Receipts that can be added to the Slip", BatchPoster.CreateDepositSlipInfo.HasErrors());

			ARReceipt newReceipt = BatchPoster.ReceiptBatch.AddNew();
			Assert("IncludeInDepositBatch should be defaulted by False", !newReceipt.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should be read only", newReceipt.IncludeInDepositBatchInfo.ReadOnly);
			newReceipt.AH_ReceiptType = ReceiptTypes.Cash;
			Assert("CreateDepositSlip should not have errors as NewReceipt can be added to the Slip", !BatchPoster.CreateDepositSlipInfo.HasErrors());
			Assert("IncludeInDepositBatch should be changed to True", newReceipt.IncludeInDepositBatch);
			Assert("IncludeInDepositBatch should not be read only", !newReceipt.IncludeInDepositBatchInfo.ReadOnly);
		}

		public void TestPostDate_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("PostDate_ReadOnly", BatchPoster));

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("PostDate_ReadOnly", BatchPoster));

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals("Allowed to post past so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("PostDate_ReadOnly", BatchPoster));
		}

		public void TestCheckPostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			BatchPoster.PostDate = ZDateTime.Empty;
			Assert("Should have error about missing post date", BatchPoster.PostDateInfo.HasError("Please enter a value."));

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "The post date cannot be in the past", true);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);
			AssertPostDateMessage(-2, BatchPoster, "The receipt batch post date will be same as this post date.", false);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);
			AssertPostDateMessage(-2, BatchPoster, "The receipt batch post date will be same as this post date.", false);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			AssertPostDateMessage(0, BatchPoster, "", false);
			AssertPostDateMessage(-2, BatchPoster, "The post date cannot be in the past", true);
		}

		public void TestCheckAH_PostDateNotInFuture()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			BatchPoster.PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("PostDate", BatchPoster.PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			BatchPoster.PostDate = ZDateTime.Now;
			AssertNoErrors("PostDate", BatchPoster.PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			BatchPoster.PostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("PostDate", BatchPoster.PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			BatchPoster.PostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("PostDate", BatchPoster.PostDateInfo);
		}

		void AssertPostDateMessage(int daysFromToday, ARReceiptBatchPoster batchPoster, string message, bool isError)
		{
			batchPoster.PostDate = ZDateTime.Now.AddDays(daysFromToday);
			string assertionMsg = string.Format("Should have {0} about post date is in {1}",
				string.IsNullOrEmpty(message) ? "no problem" : (isError ? "error" : "warning"),
				daysFromToday < 0 ? "past" : (daysFromToday == 0 ? "present" : "future"));
			bool result;
			if (!string.IsNullOrEmpty(message))
			{
				result = isError ? batchPoster.PostDateInfo.HasError(message) : batchPoster.PostDateInfo.HasWarning(message);
			}
			else
			{
				result = !batchPoster.PostDateInfo.HasErrors() && !batchPoster.PostDateInfo.HasWarnings();
			}
			Assert(assertionMsg, result);
		}

		#endregion

		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		ARReceiptBatchPoster BatchPoster;
		OrgHeader TestOrg;
		OrgHeader TestOrg2;
		OrgHeader TestOrg3;
		ARReceipt TestReceipt1;
		ARReceipt TestReceipt2;
		ARReceipt TestReceipt3;
		ARReceipt TestReceipt4;
		AccBankAccount TestBank;
		AccountingPeriodTestHelper PeriodHelper;

		protected override void SetUp()
		{
			base.SetUp();
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();

			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg.OH_IsDebtor = true;
			TestOrg.CompanyData.OB_ARPreviousChequeDrawer = "Check Drawer";
			TestOrg.CompanyData.OB_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestOrg.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Drawer Branch";
			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();

			BatchPoster = (ARReceiptBatchPoster)GetNewBusinessObject();
		}

		void PrepareForSuccessfulPost()
		{
			PrepareForSuccessfulPost(BatchPoster);
		}

		void PrepareForSuccessfulPost(ARReceiptBatchPoster batchPoster)
		{
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_IsDebtor = true;
			TestOrg2.ARSettlementGroupPK = TestOrg.PK;
			TestOrg2.CompanyData.OB_ARPreviousChequeDrawer = "Check Drawer";
			TestOrg2.CompanyData.OB_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestOrg2.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Drawer Branch";
			TestOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg3.OH_IsDebtor = true;
			TestOrg3.CompanyData.OB_ARPreviousChequeDrawer = "Check Drawer";
			TestOrg3.CompanyData.OB_ARPreviousChequeDrawerBank = "Drawer Bank";
			TestOrg3.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Drawer Branch";

			AccBankAccount testBank2 = Factory.NewWithValidTestData<AccBankAccount>();
			testBank2.AB_IsDefaultReceiptBankAccount = ZBool.True;
			testBank2.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestOrg2.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			Factory.Save();

			TestReceipt1 = batchPoster.ReceiptBatch.AddNew();
			TestReceipt1.AH_OH = TestOrg.PK;

			TestReceipt2 = batchPoster.ReceiptBatch.AddNew();
			TestReceipt2.AH_OH = TestOrg2.PK;

			TestReceipt3 = batchPoster.ReceiptBatch.AddNew();
			TestReceipt3.AH_OH = TestOrg3.PK;

			TestBank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			batchPoster.BankAccountPK = TestBank.PK;
			batchPoster.RX_NK = Core.Constants.CurrencyCodes.EuropeanUnion;
			batchPoster.ExchangeRate.Rate = 2.35M;
			batchPoster.CreateDepositSlip = ZBool.True;
			TestReceipt1.AH_ChequeOrReference = "1";
			TestReceipt2.AH_ChequeOrReference = "2";
			TestReceipt3.AH_ChequeOrReference = "3";
		}

		void SetUpBatchPosterWith2Receipts()
		{
			SetUpBatchPosterWith2Receipts(BatchPoster);
		}

		void SetUpBatchPosterWith2Receipts(ARReceiptBatchPoster batchPoster)
		{
			TestReceipt1 = batchPoster.ReceiptBatch.AddNew();
			TestReceipt1.AH_OH = TestOrg.PK;

			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestReceipt2 = batchPoster.ReceiptBatch.AddNew();
			TestReceipt2.AH_OH = TestOrg2.PK;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARReceiptBatchPoster(Factory);
		}

		class ARReceiptBatchPosterForTest : ARReceiptBatchPoster
		{
			public ARReceiptBatchPosterForTest(BusinessObjectFactory factory)
				: base(factory)
			{
				this.OnAskBeforeUpdateDescription += new OnEnquireUserHandler(BatchPoster_OnAskBeforeUpdateDescription);
				this.OnAskBeforeUpdateReceiptType += new OnEnquireUserHandler(BatchPoster_OnAskBeforeUpdateReceiptType);
				this.OnPrintDepositSlip += new DefaultBankSelectionEventHandler(BatchPoster_OnPrintDepositSlip);
			}

			ZBool BatchPoster_OnAskBeforeUpdateDescription()
			{
				AskBeforeUpdateDescriptionFired = ZBool.True;
				return Result_AskBeforeUpdateDescription;
			}
			public ZBool Result_AskBeforeUpdateDescription;
			public ZBool AskBeforeUpdateDescriptionFired;

			ZBool BatchPoster_OnAskBeforeUpdateReceiptType()
			{
				AskBeforeUpdateReceiptTypeFired = ZBool.True;
				return Result_AskBeforeUpdateReceiptType;
			}
			public ZBool AskBeforeUpdateReceiptTypeFired;
			public ZBool Result_AskBeforeUpdateReceiptType;

			void BatchPoster_OnPrintDepositSlip(DepositBatch depositSlipToPrint)
			{
				AssertEquals("DepositSlipToPrint should be the RelatedDepositBatch from BatchPoster", RelatedDepositBatch_ForTestOnly, depositSlipToPrint);
				DepositSlipPrintedForTest = ZBool.True;
			}
			public ZBool DepositSlipPrintedForTest;

			public void ResetEventResults()
			{
				AskBeforeUpdateReceiptTypeFired = ZBool.False;
				AskBeforeUpdateDescriptionFired = ZBool.False;
				DepositSlipPrintedForTest = ZBool.False;
			}
		}

		#endregion

		public void TestIsAllowedReturnsFalseWhenCashBookAllowFuturePostingOfCashBookTransactionsIsNull()
		{
			TestObjectCreator.ResetSecurityCore();
			AssertEquals(false, Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint());
			var receiptBatchPoster = new ARReceiptBatchPoster(Factory);
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Assert(AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value);
			Assert("CashBookAllowFuturePosting_ForTestOnlyOfTransactions.IsAllowed should be false", !receiptBatchPoster.AllowFuturePosting_ForTestOnly);
		}
	}
}
