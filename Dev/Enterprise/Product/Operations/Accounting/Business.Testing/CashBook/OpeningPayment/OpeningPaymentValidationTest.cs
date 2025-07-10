using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment.Testing
{
	public class OpeningPaymentValidationTest : TransactionHeaderValidationTest
	{
		public void TestCheckAH_OH_IsValid()
		{
			var payment = (OpeningPayment)Factory.New(HeaderType);
			var validation = new OpeningPaymentValidation(payment);
			payment.AH_OH = TestObjectCreator.ZECTRA.PK;
			validation.ValidateAH_OH();
			AssertHasError(payment.AH_OHInfo, "Enter a valid Account.");
			payment.AH_OH = TestObjectCreator.Creditor1.PK;
			validation.ValidateAH_OH();
			AssertNoErrors(payment.AH_OHInfo);
		}

		public void TestCheckAH_OH()
		{
			Header.AH_OH = Org1.PK;
			Header.AH_OH = ZGuid.Empty;
			Assert("Organisation can't be empty", Header.AH_OHInfo.HasErrors());

			Header.AH_OH = Org1.PK;
			Header.AH_OH = ZGuid.Invalid;
			Assert("Organisation must be valid", Header.AH_OHInfo.HasErrors());
		}

		public override void TestCheckAH_ReceiptType()
		{
			base.TestCheckAH_ReceiptType();
			Header.AH_ReceiptType = ZString.Empty;
			Assert("Payment Type can't be empty", Header.AH_ReceiptTypeInfo.HasErrors());

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Cheque is a valid payment type", !Header.AH_ReceiptTypeInfo.HasErrors());

			Header.AH_ReceiptType = "SDF";
			Assert("Payment type is invalid, should give errors", Header.AH_ReceiptTypeInfo.HasErrors());
		}

		public void TestAH_ReceiptTypeWithCashAccount()
		{
			var bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			bank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			Header.AH_AB = bank.PK;
			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertNoErrors("Setting receipt type to cash is valid for CSH accounts", Header.AH_ReceiptTypeInfo);

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("This shows the source of the wording", "Receipt Type", Header.AH_ReceiptTypeInfo.HumanReadableName);
			AssertHasError("Cash receipt type is not valid for other than CSH accounts", Header.AH_ReceiptTypeInfo, $"For Cash Account, please select CSH - Cash Receipt Type.");
		}

		public override void TestCheckAH_AB()
		{
			base.TestCheckAH_AB();

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();

			Header.AH_AB = ZGuid.Empty;
			Assert("AH_AB can't be empty", Header.AH_ABInfo.HasErrors());

			Header.AH_AB = bank.PK;
			Assert("Bank is valid", !Header.AH_ABInfo.HasErrors());

			bank.AB_IsActive = false;
			Header.AH_AB = bank.PK;
			AssertHasError(Header.AH_ABInfo, "Enter a valid Bank Account.");
		}

		public void TestCheckAH_OSExTaxAmount()
		{
			Header.AH_OSExTaxAmount = 0m;
			Assert("OSAmount can't be 0", Header.AH_OSExTaxAmountInfo.HasErrors());

			Header.AH_OSExTaxAmount = 10m;
			Assert("10 is valid", !Header.AH_OSExTaxAmountInfo.HasErrors());

			Header.AH_OSExTaxAmount = -10m;
			Assert("OSAmount can't be negative", Header.AH_OSExTaxAmountInfo.HasErrors());
		}

		public void TestCheckAH_Desc()
		{
			Header.AH_Desc = "Desc";
			Header.AH_Desc = ZString.Empty;
			Assert("Description can't be empty", Header.AH_DescInfo.HasErrors());
		}

		public void TestCheckAH_ChequeOrReference()
		{
			Header.AH_ChequeOrReference = "545";
			Assert("545 is valid cheque number", !Header.AH_ChequeOrReferenceInfo.HasErrors());

			Header.AH_ChequeOrReference = ZString.Empty;
			Assert("Cheque number can't be empty", Header.AH_ChequeOrReferenceInfo.HasErrors());

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Header.AH_ChequeOrReference = "ASDF";
			Assert("Cheque number must contain numbers only", Header.AH_ChequeOrReferenceInfo.HasErrors());
		}

		protected override Type HeaderType
		{
			get { return typeof(OpeningPayment); }
		}
	}
}
