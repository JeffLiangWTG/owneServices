using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconDirectPayment))]
	class BankReconDirectPaymentTest : DirectPaymentTest
	{
		public void TestUpdateRelatedDirectDebitBatchAmount()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			AssertNull("Precondition", bankReconDirectPayment.RelatedDirectDebitBatch);
			AssertNoExceptionThrown(bankReconDirectPayment.UpdateRelatedDirectDebitBatchAmount);

			bankReconDirectPayment.MakeDirectDebitBatch();
			AssertEquals("Precondition", false, bankReconDirectPayment.RelatedDirectDebitBatch.Lines.Any());
			AssertNoExceptionThrown(bankReconDirectPayment.UpdateRelatedDirectDebitBatchAmount);

			bankReconDirectPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			bankReconDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			var directPaymentLine = bankReconDirectPayment.Lines.AddNew();
			directPaymentLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			directPaymentLine.AL_OSExTaxAmount = 100m;
			bankReconDirectPayment.DeleteDirectDebitBatch();
			bankReconDirectPayment.MakeDirectDebitBatch();
			AssertEquals("Precondition", true, bankReconDirectPayment.RelatedDirectDebitBatch.Lines.Any());
			AssertEquals("Precondition", typeof(DirectPayment.DirectPayment), bankReconDirectPayment.RelatedDirectDebitBatch.Lines[0].GetType());

			directPaymentLine.AL_AT = TestObjectCreator.GST1.PK;
			bankReconDirectPayment.UpdateRelatedDirectDebitBatchAmount();
			AssertEquals(110m, bankReconDirectPayment.RelatedDirectDebitBatch.AH_InvoiceAmount);
			AssertEquals(110m, bankReconDirectPayment.RelatedDirectDebitBatch.AH_OSTotal);
			AssertEquals(110m, bankReconDirectPayment.RelatedDirectDebitBatch.Lines.FSelectedTotal_ForTestOnly);
			AssertEquals(110m, bankReconDirectPayment.RelatedDirectDebitBatch.Lines.FLocalSelectedTotal_ForTestOnly);

			directPaymentLine.AL_AT = TestObjectCreator.GST2.PK;
			bankReconDirectPayment.UpdateRelatedDirectDebitBatchAmount();
			AssertEquals(120m, bankReconDirectPayment.RelatedDirectDebitBatch.AH_InvoiceAmount);
			AssertEquals(120m, bankReconDirectPayment.RelatedDirectDebitBatch.AH_OSTotal);
			AssertEquals(120m, bankReconDirectPayment.RelatedDirectDebitBatch.Lines.FSelectedTotal_ForTestOnly);
			AssertEquals(120m, bankReconDirectPayment.RelatedDirectDebitBatch.Lines.FLocalSelectedTotal_ForTestOnly);
		}

		public void TestSettingAH_OSTotalUpdateRelatedDirectDebitBatchAH_OSTotal()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			bankReconDirectPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			bankReconDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			bankReconDirectPayment.AH_OSTotal = -100m;

			bankReconDirectPayment.MakeDirectDebitBatch();
			var directDebitBatch = bankReconDirectPayment.RelatedDirectDebitBatch;
			AssertEquals("AH_OSTotal", 100m, directDebitBatch.AH_OSTotal);

			bankReconDirectPayment.AH_OSTotal = -200m;
			AssertEquals("AH_OSTotal", 200m, directDebitBatch.AH_OSTotal);
		}

		[TestDate(2022, 6, 24)]
		public void TestSettingAH_PostDateUpdateRelatedDirectDebitBatchAH_PostDate()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			bankReconDirectPayment.AH_PostDate = ZDateTime.Today;

			bankReconDirectPayment.MakeDirectDebitBatch();
			var directDebitBatch = bankReconDirectPayment.RelatedDirectDebitBatch;
			AssertEquals("AH_PostDate", ZDateTime.Today, directDebitBatch.AH_PostDate);

			bankReconDirectPayment.AH_PostDate = ZDateTime.Today.AddDays(-2);
			AssertEquals("AH_PostDate", ZDateTime.Today.AddDays(-2), directDebitBatch.AH_PostDate);
		}

		[TestDate(2022, 6, 24)]
		public void TestSettingAH_InvoiceDateUpdateRelatedDirectDebitBatchAH_InvoiceDate()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			bankReconDirectPayment.AH_InvoiceDate = ZDateTime.Today;

			bankReconDirectPayment.MakeDirectDebitBatch();
			var directDebitBatch = bankReconDirectPayment.RelatedDirectDebitBatch;
			AssertEquals("AH_InvoiceDate", ZDateTime.Today, directDebitBatch.AH_InvoiceDate);

			bankReconDirectPayment.AH_InvoiceDate = ZDateTime.Today.AddDays(-2);
			AssertEquals("AH_InvoiceDate", ZDateTime.Today.AddDays(-2), directDebitBatch.AH_InvoiceDate);
		}

		[TestDate(2022, 6, 24)]
		public void TestSettingAH_DueDateUpdateRelatedDirectDebitBatchAH_DueDate()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			bankReconDirectPayment.AH_DueDate = ZDateTime.Today;

			bankReconDirectPayment.MakeDirectDebitBatch();
			var directDebitBatch = bankReconDirectPayment.RelatedDirectDebitBatch;
			AssertEquals("AH_DueDate", ZDateTime.Today, directDebitBatch.AH_DueDate);

			bankReconDirectPayment.AH_DueDate = ZDateTime.Today.AddDays(-2);
			AssertEquals("AH_DueDate", ZDateTime.Today.AddDays(-2), directDebitBatch.AH_DueDate);
		}

		public void TestValidationType()
		{
			AssertEquals("Validation", typeof(BankReconDirectPaymentValidation), TestBizO.Validation.GetType());
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(BankReconDirectPaymentValidation); }
		}

		//Copy of TransactionHeaderWithLinesTest.TestIsChequeNumberAutoAllocated
		public override void TestIsChequeNumberAutoAllocated()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 2);
			BankReconDirectPayment testHeader = Factory.New<BankReconDirectPayment>();
			testHeader.AH_AB = testBank.PK;
			testHeader.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testHeader.ChequeBookPK = autoPrintChequeBook.PK;
			Assert("Auto allocation mode should not be enabled by default", !testHeader.IsChequeNumberAutoAllocated_ForTestOnly);
			AssertEquals("current cheque number should be set", "2", testHeader.AH_ChequeOrReference);
			Assert("ChequeOrReference field should not be read only", !testHeader.AH_ChequeOrReferenceInfo.ReadOnly);
		}

		public override void TestPaymentMethods()
		{
			BankReconDirectPayment testHeader = Factory.New<BankReconDirectPayment>();
			AssertEquals("Payment Methods list should be of valid type", OLookUpEditType.PaymentMethod, testHeader.PaymentMethods.LookupEditType);
			CodeDescriptionPairList testPaymentMethodsList = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
			testPaymentMethodsList.RemoveCode(ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertEquals("Both test and real lists should contain same members", testPaymentMethodsList.ElementsAsString, Header.PaymentMethods.ElementsAsString);
		}

		public void TestMakeDirectDebitBatch()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			AssertNull("RelatedDirectDebitBatch", bankReconDirectPayment.RelatedDirectDebitBatch);

			bankReconDirectPayment.MakeDirectDebitBatch();
			AssertNotNull("RelatedDirectDebitBatch", bankReconDirectPayment.RelatedDirectDebitBatch);
			AssertEquals("Type", typeof(DirectDebitBatch.DirectDebitBatchHeader), bankReconDirectPayment.RelatedDirectDebitBatch.GetType());
		}

		public void TestSetDirectDebitBatchDetails()
		{
			var bankReconDirectPayment = Factory.New<BankReconDirectPayment>();
			bankReconDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			bankReconDirectPayment.AH_ReceiptBatchNo = ZString.Empty;
			bankReconDirectPayment.AH_TransactionType = TransactionTypes.DirectPayment;
			bankReconDirectPayment.AH_OSTotal = -100m;
			bankReconDirectPayment.AH_InvoiceAmount = -100m;
			bankReconDirectPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bankReconDirectPayment.AH_PostDate = new ZDateTime(2012, 03, 22);
			bankReconDirectPayment.AH_InvoiceDate = new ZDateTime(2012, 03, 22);
			bankReconDirectPayment.AH_DueDate = new ZDateTime(2012, 03, 22);
			bankReconDirectPayment.AH_AB = TestObjectCreator.USDBankAccount.PK;
			bankReconDirectPayment.MakeDirectDebitBatch();

			var header = bankReconDirectPayment.RelatedDirectDebitBatch;

			AssertEquals("RelatedTransactionPK", bankReconDirectPayment.PK, header.RelatedTransactionPK);
			AssertEquals("AH_OSTotal", 100m, header.AH_OSTotal);
			AssertEquals("AH_InvoiceAmount", 100m, header.AH_InvoiceAmount);
			AssertEquals("AH_RX_NKTransactionCurrency", Core.Constants.CurrencyCodes.UnitedStates, header.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_PostDate", new ZDateTime(2012, 03, 22), header.AH_PostDate);
			AssertEquals("AH_InvoiceDate", new ZDateTime(2012, 03, 22), header.AH_InvoiceDate);
			AssertEquals("AH_DueDate", new ZDateTime(2012, 03, 22), header.AH_DueDate);
			AssertEquals("AH_AB", TestObjectCreator.USDBankAccount.PK, header.AH_AB);
		}
	}
}
