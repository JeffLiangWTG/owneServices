using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.OpeningReceipt.Testing
{
	public class OpeningReceiptValidationTest : TransactionHeaderValidationTest
	{
		public void TestCheckAH_OH_IsValid()
		{
			var receipt = (OpeningReceipt)Factory.New(HeaderType);
			var validation = new OpeningReceiptValidation(receipt);
			receipt.AH_OH = TestObjectCreator.ZECTRA.PK;
			validation.ValidateAH_OH();
			AssertHasError(receipt.AH_OHInfo, "Enter a valid Account.");
			receipt.AH_OH = TestObjectCreator.Creditor1.PK;
			validation.ValidateAH_OH();
			AssertNoErrors(receipt.AH_OHInfo);
		}

		public void TestCheckAH_OH()
		{
			Header.AH_OH = ZGuid.Invalid;
			Assert("Organisation cannot be invalid", Header.AH_OHInfo.HasErrors());

			Header.AH_OH = TestObjectCreator.TestOrganisation.PK;
			Assert("Should not be any errors", !Header.AH_OHInfo.HasErrors());

			Header.AH_OH = ZGuid.Empty;
			Assert("Organisation cannot be empty", Header.AH_OHInfo.HasErrors());
		}

		public void TestCheckAH_Desc()
		{
			Header.AH_Desc = "Desc";
			Header.AH_Desc = ZString.Empty;
			Assert("Description cannot be empty", Header.AH_DescInfo.HasErrors());
		}

		public override void TestCheckAH_ReceiptType()
		{
			base.TestCheckAH_ReceiptType();

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			Assert("Direct Credit is a valid receipt type", !Header.AH_ReceiptTypeInfo.HasErrors());

			Header.AH_ReceiptType = ZString.Empty;
			Assert("Receipt Type cannot be empty", Header.AH_ReceiptTypeInfo.HasErrors());

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			Assert("Credit Card is a valid receipt Type", !Header.AH_ReceiptTypeInfo.HasErrors());

			Header.AH_ReceiptType = "FSD";
			Assert("Receipt Type is invalid", Header.AH_ReceiptTypeInfo.HasErrors());
		}

		public void TestAH_ReceiptTypeWithCashAccount()
		{
			var bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			bank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			Header.AH_AB = bank.PK;
			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertNoErrors("Setting receipt type to cash is valid for CSH accounts", Header.AH_ReceiptTypeInfo);

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasError("Cash receipt type is not valid for other than CSH accounts", Header.AH_ReceiptTypeInfo, "For Cash Account, please select CSH - Cash Receipt Type.");
		}

		public override void TestCheckAH_AB()
		{
			base.TestCheckAH_AB();

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();

			Header.AH_AB = ZGuid.Empty;
			Assert("Bank cannot be empty", Header.AH_ABInfo.HasErrors());

			Header.AH_AB = bank.PK;
			Assert("Bank is valid", !Header.AH_ABInfo.HasErrors());

			bank.AB_IsActive = false;
			Header.AH_AB = bank.PK;
			AssertHasError(Header.AH_ABInfo, "Enter a valid Bank Account.");
		}

		public void TestCheckAH_OSExTaxAmount()
		{
			Header.AH_OSExTaxAmount = 90m;
			Header.AH_OSExTaxAmount = 0m;
			Assert("OSExTax amount can't be empty", Header.AH_OSExTaxAmountInfo.HasErrors());

			Header.AH_OSExTaxAmount = 90m;
			Assert("OSExTax amount has no errors", !Header.AH_OSExTaxAmountInfo.HasErrors());
			Header.AH_OSExTaxAmount = -90m;
			Assert("OSExTax amount can't be negative", Header.AH_OSExTaxAmountInfo.HasErrors());
		}

		public void TestCheckAH_ChequeOrReference()
		{
			Header.AH_ChequeOrReference = "ASdf";
			Header.AH_ChequeOrReference = ZString.Empty;
			Assert("Cheque/Reference cannot be empty", Header.AH_ChequeOrReferenceInfo.HasErrors());

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Header.AH_ChequeOrReference = "8505";
			Assert("8505 is a valid cheque number", !Header.AH_ChequeOrReferenceInfo.HasErrors());

			Header.AH_ChequeOrReference = "Hello";
			Assert("Cheque number must contain numbers only for cheques", Header.AH_ChequeOrReferenceInfo.HasErrors());

			Header.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			Header.AH_ChequeOrReference = "Hello";
			Assert("Credit Card Reference number can contain non-numeric characters", !Header.AH_ChequeOrReferenceInfo.HasErrors());
		}

		protected override Type HeaderType
		{
			get { return typeof(OpeningReceipt); }
		}
	}
}
