using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DirectPaymentController))]
	class DirectPaymentControllerTest : AccountingTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DirectPayment;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return DirectPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookDirectPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookDirectPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewCashBookDirectPayment; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookDirectPayment; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			DirectPayment = Factory.NewWithValidTestData<DirectPayment>();
			Factory.Save();
		}

		public void TestShowDeleteForm()
		{
			DirectPaymentController testController = new DirectPaymentController();

			DirectPayment testDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (IZForm testForm = testController.ShowDeleteForm(testDirectPayment))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
			testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;

			testBankTransfer.BuyExchangeRate = 0.5m;
			testBankTransfer.SellAmount = 100m;
			testBankTransfer.EnableFinanceCharge = true;
			testBankTransfer.FinanceChargeOSAmount = 10m;

			Factory.Save();

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionCount, (byte)3);
			DirectPayment testBankTranferDirectPayment = Factory.LoadTop1<DirectPayment>(filter);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (IZForm testForm = testController.ShowDeleteForm(testBankTranferDirectPayment))
			{
				AssertEquals("The transactions cannot be reversed for the following reasons: - It cannot be reversed individually as it relates to Bank Transfer. It will be automatically reversed through canceling of the Bank Transfer.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCopy()
		{
			//Arrange
			var testDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
			testDirectPayment.AH_InvoiceDate = ZDateTime.Now.AddDays(-2);
			testDirectPayment.AH_PostDate = ZDateTime.Now.AddDays(-1);
			testDirectPayment.AH_AB = TestObjectCreator.USDBankAccount.PK;
			testDirectPayment.AH_ReceiptType = ReceiptTypes.Cash;
			testDirectPayment.AH_ExchangeRate = 0.7854m;
			testDirectPayment.AH_Desc = "Cash book direct payment test";
			testDirectPayment.AH_ChequeDrawer = "CASH";
			
			var testLine = testDirectPayment.Lines.AddNew();
			testLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			testLine.AL_GB = TestObjectCreator.DefaultBranchPK;
			testLine.AL_GE = TestObjectCreator.FIADepartment.PK;
			testLine.AL_Desc = "Test Cash Line";
			testLine.AL_OSExTaxAmount = 1000m;
			testLine.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			testLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			testLine.AL_OSTaxAmount = 110m;
			Factory.Save();

			//set system exchange rate
			var sysExRateValue = 0.7154m;
			RefExchangeRate sellExRate = new BusinessObjectFactory().NewWithValidTestData<RefExchangeRate>();
			sellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			sellExRate.RE_StartDate = ZDateTime.Today;
			sellExRate.RE_ExpiryDate = ZDateTime.Today;
			sellExRate.RE_SellRate = sysExRateValue;
			sellExRate.RE_RX_NKExCurrency = TestObjectCreator.USDBankAccount.AccountCurrency.Code;
			sellExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			sellExRate.Factory.Save();

			//Act
			using (IZForm form = ((DirectPaymentController)Controller).ShowTemplateCopyForm(testDirectPayment))
			{
				//Assert
				AssertNotNull("Form should not be null", form);
				AssertEquals("Form's Display mode should be New", ODisplayMode.New, form.DisplayMode);

				var copy = form.BusinessEntityForPersistingForm as DirectPayment;
				AssertZDatesWithin5Minutes("Payment Date should be today", ZDateTime.Now, copy.AH_InvoiceDate);
				AssertZDatesWithin5Minutes("Post Date should be today", ZDateTime.Now, copy.AH_PostDate);
				AssertEquals(testDirectPayment.AH_AB, copy.AH_AB);
				AssertEquals(testDirectPayment.AH_ReceiptType, copy.AH_ReceiptType);
				AssertEquals(TestObjectCreator.USDBankAccount.AccountCurrency.Code, copy.ExchangeRate.Currency);
				AssertEquals(sysExRateValue, copy.ExchangeRate.Rate);
				AssertEquals(testDirectPayment.AH_Desc, copy.AH_Desc);
				AssertEquals(testDirectPayment.AH_ChequeDrawer, copy.AH_ChequeDrawer);

				//Assert lines are copied
				AssertEquals(copy.Lines.Count, testDirectPayment.Lines.Count);
				for (int i = 0; i < testDirectPayment.Lines.Count; i++)
				{
					var line = testDirectPayment.Lines[i];
					var lineCopy = copy.Lines[i];

					AssertEquals(line.AL_AG, lineCopy.AL_AG);
					AssertEquals(line.AL_GB, lineCopy.AL_GB);
					AssertEquals(line.AL_GE, lineCopy.AL_GE);
					AssertEquals(line.AL_Desc, lineCopy.AL_Desc);
					AssertEquals(line.AL_OSExTaxAmount, lineCopy.AL_OSExTaxAmount);
					AssertEquals(line.AL_AT, lineCopy.AL_AT);
					AssertEquals(line.AL_A9_VATClass, lineCopy.AL_A9_VATClass);
					AssertEquals(line.AL_OSTaxAmount, lineCopy.AL_OSTaxAmount);
					AssertEquals("Local amount should be automatically calculated based on current exchange rate", Math.Round(line.AL_OSExTaxAmount / sysExRateValue, 2), lineCopy.AL_LocalExTaxAmount);
					AssertZDatesWithin5Minutes("Line TaxDate should be today", ZDate.Today, lineCopy.AL_TaxDate);
				}
			}
		}

		public void TestShowTemplateCopyForm_ShouldNotCopyBankTransferCharge()
		{
			var testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.Reference = "Test";
			testBankTransfer.SetFromBankAccountWithAsserts(TestObjectCreator.USDBankAccount.PK);
			testBankTransfer.SetToBankAccountWithAsserts(TestObjectCreator.GBPBankAccount.PK);
			testBankTransfer.SetSellAmountWithAsserts(750m);
			testBankTransfer.SetSellExchangeRateWithAsserts(0.8333m);
			testBankTransfer.SetBuyExchangeRateWithAsserts(1m);

			testBankTransfer.EnableFinanceCharge = true;
			testBankTransfer.FinanceChargeOSAmount = 10m;
			testBankTransfer.FinanceChargeExchangeRate = 1.46m;
			testBankTransfer.FinanceChargeTaxID = TestObjectCreator.GST1.PK;
			testBankTransfer.FinanceChargeOSTaxAmount = 1m;
			Factory.Save();

			var filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, testBankTransfer.TransactionBelongsToGroup);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)3);
			var testFinanceCharge = Factory.LoadTop1<BankTransferCharge>(filter);
			var testController = new DirectPaymentController();
			using (var testForm = testController.ShowTemplateCopyForm(testFinanceCharge))
			{
				AssertEquals("The selected Direct Payment cannot be copy as it is linked to Bank Transfer 00001000.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(testBankTransfer);
			reversing.Reverse();
			Factory.Save();

			filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)6);
			var testReversalFinanceCharge = Factory.LoadTop1<BankTransferCharge>(filter);

			using (var testForm = testController.ShowTemplateCopyForm(testReversalFinanceCharge))
			{
				AssertEquals("The selected Direct Payment cannot be copy as it is linked to Bank Transfer 00001001.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		DirectPayment DirectPayment;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion

		TestObjectCreator fTestObjectCreator;
	}

	#region HelperMethods
	// Copy-pasted from Accounting.Business.Test
	static class BankTranferExtensionsForTest {
		static void DoAssert(BankTransfer transfer)
		{
			Assertion.AssertEquals("Invoice amounts balance to 0", transfer.TransferRowFrom.AH_InvoiceAmount, -transfer.TransferRowTo.AH_InvoiceAmount);
			Assertion.AssertEquals("Local amounts are matched", transfer.TransferRowFrom.AH_LocalExTaxAmount, transfer.TransferRowTo.AH_LocalExTaxAmount);
		}

		public static BankTransfer SetSellAmountWithAsserts(this BankTransfer transfer, ZDecimal amount)
		{
			transfer.SellAmount = amount;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetFromBankAccountWithAsserts(this BankTransfer transfer, ZGuid bankPK)
		{
			transfer.BankTransferFromPK = bankPK;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetToBankAccountWithAsserts(this BankTransfer transfer, ZGuid bankPK)
		{
			transfer.BankTransferToPK = bankPK;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetBuyExchangeRateWithAsserts(this BankTransfer transfer, ZDecimal rate)
		{
			transfer.BuyExchangeRate = rate;
			DoAssert(transfer);
			return transfer;
		}

		public static BankTransfer SetSellExchangeRateWithAsserts(this BankTransfer transfer, ZDecimal rate)
		{
			transfer.SellExchangeRate = rate;
			DoAssert(transfer);
			return transfer;
		}
	}
	#endregion
}
