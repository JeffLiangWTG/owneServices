using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class DirectDebitBatchHeaderValidation_InnerTest : TestCaseWithFactory
	{
		public void TestCheckAH_AB_ValidateAllowAutoDDR()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = false;

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;

			AssertNoErrors(dDRBatch.AH_ABInfo);
		}

		public void TestCheckAH_ABIsActive()
		{
			AccBankAccount testBank = TestObjectCreator.CreateBankAccount("TST", "TEST", TestObjectCreator.AUD, null);
			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;

			Assert(!dDRBatch.AH_ABInfo.HasErrors());

			testBank.AB_IsActive = false;
			dDRBatch.AH_AB = testBank.PK;
			AssertHasError(dDRBatch.AH_ABInfo, "This Bank Account is inactive - it may not be used.");
		}

		public void TestCheckAH_OSExTaxAmount()
		{
			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_IsCancelled = false;
			dDRBatch.AH_OSExTaxAmount = 0m;

			AssertHasError(dDRBatch.AH_OSExTaxAmountInfo, "DDR Batch amount cannot be 0. Please select payments in order to generate DDR Batch");

			dDRBatch.AH_OSExTaxAmount = 12m;
			AssertNoError(dDRBatch.AH_OSExTaxAmountInfo, "DDR Batch amount cannot be 0. Please select payments in order to generate DDR Batch");
		}

		public void TestValidateDepositPostDate()
		{
			TestObjectCreator.CreateTestPeriods(Env.Time.CurrentLocalDate.AddMonths(-3));
			Factory.Save();

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;
			testBank.AB_AccountNum = "123456789";
			testBank.AB_BSB = "123-456";

			var chequeBook = TestObjectCreator.CreateChequeBook("Test", 100, testBank);

			var testOrganisation = TestObjectCreator.AALSHI;
			var accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			var payment = Factory.New<APPayment>();
			payment.AH_AB = testBank.PK;
			payment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment.AH_OH = testOrganisation.PK;
			payment.AH_ChequeOrReference = "12434";
			payment.AH_OSExTaxAmount = 300m;
			payment.AH_PostDate = ZDate.Today.AddDays(-5);

			var directDebittBatch = Factory.New<DirectDebitBatchHeader>();
			directDebittBatch.AH_AB = testBank.PK;

			directDebittBatch.Lines.Add(payment);
			directDebittBatch.AH_OSExTaxAmount = 120m;
			directDebittBatch.AH_PostDate = ZDate.Today.AddDays(-3);
			directDebittBatch.RunPreSaveValidation();
			AssertHasErrorContaining(directDebittBatch.AH_PostDateInfo, "The post date cannot be in the past");

			directDebittBatch.GenerateReverseTransaction(true);
			var reversingHeader = (TransactionHeader)((IReversing)directDebittBatch).ReverseTransaction;
			reversingHeader.RunPreSaveValidation();
			AssertNoErrorContaining(reversingHeader.AH_PostDateInfo, "The post date cannot be in the past");
		}

		public void TestValidateBeforePosting()
		{
			TestObjectCreator.CreateTestPeriods(Env.Time.CurrentLocalDate.AddMonths(-3));
			Factory.Save();

			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;
			testBank.AB_AccountNum = "123456789";
			testBank.AB_BSB = "123-456";

			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook("Test", 100, testBank);

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			DirectPayment.DirectPayment directPayment = Factory.New<DirectPayment.DirectPayment>();
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.AH_ChequeOrReference = "1234";
			directPayment.AH_DrawerBank = "123456789";
			directPayment.AH_DrawerBranch = "123-456";
			directPayment.AH_ChequeDrawer = "ChequeDrawer";
			directPayment.AH_OH = testOrganisation.PK;
			directPayment.AH_OSExTaxAmount = 500m;

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aPPayment.AH_OH = testOrganisation.PK;
			aPPayment.AH_ChequeOrReference = "12434";
			aPPayment.AH_OSExTaxAmount = 300m;

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.AH_AB = testBank.PK;

			dDRBatch.Lines.Add(aPPayment);
			dDRBatch.Lines.Add(directPayment);
			dDRBatch.AH_OSExTaxAmount = 120m;
			dDRBatch.AH_PostDate = Env.Time.CurrentLocalDate;

			dDRBatch.ValidateBeforePosting();

			AssertNoErrors(dDRBatch);
		}

		public void TestValidateAH_ReceiptBatchNoForLines()
		{
			TestObjectCreator.CreateTestPeriods(Env.Time.CurrentLocalDate.AddMonths(-3));

			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;

			var chequeBook = TestObjectCreator.CreateChequeBook("Test", 100, testBank);

			var testOrganisation = TestObjectCreator.AALSHI;
			var accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			var payment1 = TestObjectCreator.CreateAPPayment(1m, 10m, ZDate.Today, ZDate.Today, testOrganisation.PK, testBank.PK);
			payment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment1.AH_ChequeOrReference = "11111";

			var payment2 = TestObjectCreator.CreateAPPayment(1m, 10m, ZDate.Today, ZDate.Today, testOrganisation.PK, testBank.PK);
			payment2.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment2.AH_ChequeOrReference = "22222";
			Factory.Save();

			var directDebitBatch = Factory.New<DirectDebitBatchHeader>();
			directDebitBatch.AH_AB = testBank.PK;
			directDebitBatch.AH_OSExTaxAmount = 10m;
			directDebitBatch.AH_PostDate = ZDate.Today;
			AssertEquals(2, directDebitBatch.Lines.Count);
			(payment2 as IDirectDebitBatchTransaction).IncludeInTheBatch = false;
			directDebitBatch.ValidateBeforePosting();
			AssertNoErrors(directDebitBatch);

			var newFactory = new BusinessObjectFactory();
			var payment1InNewFactory = newFactory.Load<APPayment>(payment1.PK);
			var payment2InNewFactory = newFactory.Load<APPayment>(payment2.PK);
			var directDebitBatchInNewFactory = newFactory.New<DirectDebitBatchHeader>();
			directDebitBatchInNewFactory.AH_AB = testBank.PK;
			directDebitBatchInNewFactory.AH_OSExTaxAmount = 20m;
			directDebitBatchInNewFactory.AH_PostDate = ZDate.Today;
			AssertEquals(2, directDebitBatchInNewFactory.Lines.Count);
			directDebitBatchInNewFactory.ValidateBeforePosting();
			AssertNoErrors(directDebitBatchInNewFactory);

			Assert("Precondition", directDebitBatch.Validation is DirectDebitBatchHeaderValidation);
			Assert("Precondition", directDebitBatchInNewFactory.Validation is DirectDebitBatchHeaderValidation);

			Factory.Save();

			//Confirm the validation only run when creating a batch
			Assert("Save successfully", directDebitBatch.IsInDatabase);
			Assert("After saved, we won't call ValidateBeforePosting again since validation changes from DirectDebitBatchHeaderValidation to TransactionHeaderEmptyValidation", directDebitBatch.Validation is TransactionHeaderEmptyValidation);
			directDebitBatch.ValidateBeforeFileGeneration();
			AssertNoRowErrors("We won't call ValidateBeforePosting in DirectDebitBatchHeader.ValidateBeforeFileGeneration", payment1);
			AssertNoRowErrors("We won't call ValidateBeforePosting in DirectDebitBatchHeader.ValidateBeforeFileGeneration", payment2);

			directDebitBatchInNewFactory.ValidateBeforePosting();
			AssertHasRowError(payment1InNewFactory, "The transaction is already included in another Direct Debit Batch.");
			AssertNoRowErrors(payment2InNewFactory);

			(payment1InNewFactory as IDirectDebitBatchTransaction).IncludeInTheBatch = false;
			directDebitBatchInNewFactory.ValidateBeforePosting();
			AssertNoRowErrors("Excluded from current batch", payment1InNewFactory);
			AssertNoRowErrors(payment2InNewFactory);
			Assert("Precondition", directDebitBatchInNewFactory.Validation is DirectDebitBatchHeaderValidation);
			newFactory.Save();

			//Confirm the validation only run when creating a batch
			Assert("Save successfully", directDebitBatchInNewFactory.IsInDatabase);
			Assert("After saved, we won't call ValidateBeforePosting again since validation changes from DirectDebitBatchHeaderValidation to TransactionHeaderEmptyValidation", directDebitBatchInNewFactory.Validation is TransactionHeaderEmptyValidation);
			directDebitBatchInNewFactory.ValidateBeforeFileGeneration();
			AssertNoRowErrors("We won't call ValidateBeforePosting in DirectDebitBatchHeader.ValidateBeforeFileGeneration", payment1InNewFactory);
			AssertNoRowErrors("We won't call ValidateBeforePosting in DirectDebitBatchHeader.ValidateBeforeFileGeneration", payment2InNewFactory);
		}

		public void TestValidateBatchLines_AccountDetailsNotFound()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;
			testBank.AB_AccountNum = "123456789";
			testBank.AB_BSB = "123-456";

			AccChequeBook chequeBook = TestObjectCreator.CreateChequeBook("Test", 100, testBank);

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aPPayment.AH_OH = testOrganisation.PK;
			aPPayment.AH_ChequeOrReference = "12434";
			aPPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.NewZealand;

			DirectDebitBatchHeader dDRBatch = Factory.New<DirectDebitBatchHeader>();
			dDRBatch.Lines.Add(aPPayment);
			dDRBatch.AH_OSExTaxAmount = 120m;
			dDRBatch.AH_AB = testBank.PK;

			aPPayment.RunPreSaveValidation();

			AssertHasErrors("No Account Details should be found, should error on the Payee", aPPayment.AH_OHInfo);
			AssertEquals("Should be 1 error", 1, aPPayment.AH_OHInfo.GetErrors().Count());
			ZString expectedMessage = string.Format("An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", "NZD", testOrganisation.OH_Code);
			AssertEquals("Incorrect Message error", expectedMessage, aPPayment.AH_OHInfo.GetErrors().GetFirstMessage());
		}

		public void TestAddBankAccountNumberError_ForPayment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;
			testBank.AB_AccountNum = "1234567890123";

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;

			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_BankAccount = "1234567890123";

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrganisation.PK;
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(aPPayment);
			string expectedError =
				$@"The Direct Debit Bank Account Number (in Organizations Master Files) for {testOrganisation.OH_Code.Trim()} must be equal to or less than 12 characters in length.
						To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.";
			AssertEquals(expectedError, errorMsg);

			accountDetails.A1_BankAccount = "123456789012";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(aPPayment);
			AssertEquals("", errorMsg);
		}

		public void TestAddBankAccountNumberError_ForDirectPayment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.WNZ;
			testBank.AB_AccountNum = "1234567890123";

			DirectPayment.DirectPayment directPayment = Factory.New<DirectPayment.DirectPayment>();
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_DrawerBank = "12345678901234";
			directPayment.AH_DrawerBranch = "123-456";

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(directPayment);
			string expectedError = String.Format("The Direct Debit Bank Account Number setup in Direct Payment Number {0} must be equal to or less than 12 characters in length.{1}Reverse the transaction and re-enter it with a Bank Account Number 12 characters long", directPayment.AH_TransactionNum, System.Environment.NewLine);
			AssertEquals(expectedError, errorMsg);

			directPayment.AH_DrawerBank = "123456789012";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(directPayment);
			AssertEquals("", errorMsg);
		}

		public void TestAddBankBSBNumberError_ForPayment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;
			testBank.AB_AccountNum = "1234567890";

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;

			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_BankBsb = "123456";

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrganisation.PK;
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(aPPayment);
			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberNonASBErrorMsg(aPPayment.Header.OH_Code.Trim());
			AssertEquals(expectedError, errorMsg);

			accountDetails.A1_BankBsb = "123-456";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(aPPayment);
			AssertEquals("", errorMsg);
		}

		public void TestAddBankBSBNumberError_ANZ_NewZeland_ForPayment()
		{
			GlbBranch newBranch = SetupNewNewZelandCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
				testBank.AB_AllowAutoDDR = true;
				testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
				testBank.AB_AccountNum = "1234567890";

				OrgHeader testOrganisation = TestObjectCreator.AALSHI;

				AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
				accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
				accountDetails.A1_IsDefaultAccount = true;
				accountDetails.A1_BankBsb = "123-456";

				APPayment aPPayment = Factory.New<APPayment>();
				aPPayment.AH_AB = testBank.PK;
				aPPayment.AH_OH = testOrganisation.PK;
				aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

				string errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(aPPayment);
				string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberASBErrorMsg(aPPayment.Header.OH_Code.Trim());
				AssertEquals(expectedError, errorMsg);

				accountDetails.A1_BankBsb = "123456";
				errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(aPPayment);
				AssertEquals("", errorMsg);
			}
		}

		public void TestAddBankBSBNumberError_ANZ_NotNewZeland_ForPayment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			testBank.AB_AccountNum = "1234567890";

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;

			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_BankBsb = "123456";

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrganisation.PK;
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(aPPayment);
			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberNonASBErrorMsg(aPPayment.Header.OH_Code.Trim());
			AssertEquals(expectedError, errorMsg);

			accountDetails.A1_BankBsb = "123-456";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(aPPayment);
			AssertEquals("", errorMsg);
		}

		public void TestAddBankBSBNumberError_ForDirectPayment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.CBA;
			testBank.AB_AccountNum = "1234567890123";

			DirectPayment.DirectPayment directPayment = Factory.New<DirectPayment.DirectPayment>();
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_DrawerBank = "12345678901234";
			directPayment.AH_DrawerBranch = "123456";

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(directPayment);
			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberNonASBDirectPaymentErrorMsg(directPayment.AH_TransactionNum);
			AssertEquals(expectedError, errorMsg);

			directPayment.AH_DrawerBranch = "123-456";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(directPayment);
			AssertEquals("", errorMsg);
		}

		public void TestAddBankBSBNumberError_ANZ_NewZeland_ForDirectPayment()
		{
			GlbBranch newBranch = SetupNewNewZelandCompanyAndBranch();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
				testBank.AB_AllowAutoDDR = true;
				testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
				testBank.AB_AccountNum = "1234567890123";

				DirectPayment.DirectPayment directPayment = Factory.New<DirectPayment.DirectPayment>();
				directPayment.AH_AB = testBank.PK;
				directPayment.AH_DrawerBank = "12345678901234";
				directPayment.AH_DrawerBranch = "123-456";

				string errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(directPayment);
				string expectedError = ZString.Format(DirectDebitBatchHeaderValidation.InvalidBSBNumberASBDirectPaymentErrorMsg, directPayment.AH_TransactionNum);
				AssertEquals(expectedError, errorMsg);

				directPayment.AH_DrawerBranch = "123456";
				errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(directPayment);
				AssertEquals("", errorMsg);
			}
		}

		public void TestAddBankBSBNumberError_ANZ_NotNewZeland_ForDirectPayment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.ANZ;
			testBank.AB_AccountNum = "1234567890123";

			DirectPayment.DirectPayment directPayment = Factory.New<DirectPayment.DirectPayment>();
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_DrawerBank = "12345678901234";
			directPayment.AH_DrawerBranch = "123456";

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(directPayment);
			string expectedError = DirectDebitBatchHeaderValidation.GetInvalidBSBNumberNonASBDirectPaymentErrorMsg(directPayment.AH_TransactionNum);
			AssertEquals(expectedError, errorMsg);

			directPayment.AH_DrawerBranch = "123-456";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankBSBNumberError(directPayment);
			AssertEquals("", errorMsg);
		}

		public void TestBNZBankAccountError_Payment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.BNZ;
			testBank.AB_AccountNum = "123456789";

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "123456789";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_AccountName = "John";
			accountDetails.A1_BankBsb = "665-987";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrganisation.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(aPPayment);
			AssertEquals("", errorMsg);

			accountDetails.A1_BankAccount = "123456789012";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(aPPayment);
			string expectedError =
				$@"The Direct Debit Bank Account Number (in Organizations Master Files) for {testOrganisation.OH_Code.Trim()} must be 9 or 10 characters in length.
						To correct the error, select the transaction, right click your mouse and select “Edit Payment Organization Detail” to correct the BSB Number.";
			AssertEquals(expectedError, errorMsg);
		}

		public void TestBCSBankAccountError_Payment()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;
			testBank.AB_AutoDDRFormat = Constants.DDRFileFormat.BCS;
			testBank.AB_AccountNum = "123456789";

			OrgHeader testOrganisation = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = testOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankAccount = "12345678";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_AccountName = "John";
			accountDetails.A1_BankBsb = "665987";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Constants.CurrencyCodes.Australia;

			APPayment apPayment = Factory.New<APPayment>();
			apPayment.AH_AB = testBank.PK;
			apPayment.AH_OH = testOrganisation.PK;
			apPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(apPayment);
			AssertEquals("", errorMsg);

			accountDetails.A1_BankAccount = "123456789012";
			errorMsg = DirectDebitBatchHeaderValidation.GetBankAccountNumberError(apPayment);
			string expectedError = String.Format("The account number format required for BACS DDR Files is 'XXXXXXXX'");
			AssertEquals(expectedError, errorMsg);
		}

		[TestDate(2015, 1, 10)]
		public void TestValidateUnsavedHeaderAndNotValidateSavedHeader()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2015, 1, 1));

			var dDRBatch = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRBatch.AH_PostDate = ZDateTime.Now;

			AssertEquals(false, dDRBatch.IsInDatabase);
			AssertNoErrors(dDRBatch.AH_PostDateInfo);

			dDRBatch.AH_PostDate = new ZDateTime(1900, 1, 1);
			AssertHasError(dDRBatch.AH_PostDateInfo, "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.");

			dDRBatch.AH_PostDate = ZDateTime.Now;
			AssertEquals(false, dDRBatch.IsInDatabase);
			AssertNoErrors(dDRBatch.AH_PostDateInfo);
			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_PostDate = '1900-1-1', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{0}';", dDRBatch.PK));

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var dDRBatchInNewFactory = newFactory.Load<DirectDebitBatchHeader>(dDRBatch.PK);

			AssertEquals(true, dDRBatchInNewFactory.IsInDatabase);
			AssertEquals(new ZDateTime(1900, 1, 1), dDRBatchInNewFactory.AH_PostDate);
			dDRBatchInNewFactory.RunPreSaveValidation();
			AssertNoErrors(dDRBatchInNewFactory.AH_PostDateInfo);
		}

		[TestDate(2015, 1, 10)]
		public void TestCheckAH_PostDateNotInFuture()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2015, 1, 1));
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var dDRBatch = Factory.NewWithValidTestData<DirectDebitBatchHeader>();

			dDRBatch.AH_PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("AH_PostDate", dDRBatch.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			dDRBatch.AH_PostDate = ZDateTime.Now;
			AssertNoErrors("AH_PostDate", dDRBatch.AH_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			dDRBatch.AH_PostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("AH_PostDate", dDRBatch.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			dDRBatch.AH_PostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("AH_PostDate", dDRBatch.AH_PostDateInfo);
		}

		public void TestCheckAH_PostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			var payment1 = TestObjectCreator.CreateAPPayment(1m, 10m, ZDateTime.Today.AddDays(-2), ZDateTime.Today, TestObjectCreator.TestOrganisation.PK, TestObjectCreator.AUDBankAccount.PK);
			payment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment1.AH_ChequeOrReference = "11111";

			var payment2 = TestObjectCreator.CreateAPPayment(1m, 10m, ZDateTime.Today.AddDays(-4), ZDateTime.Today, TestObjectCreator.TestOrganisation.PK, TestObjectCreator.AUDBankAccount.PK);
			payment2.AH_ReceiptType = ReceiptTypes.DirectDebit;
			payment2.AH_ChequeOrReference = "22222";
			Factory.Save();

			var directDebitBatch = Factory.New<DirectDebitBatchHeader>();
			directDebitBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(2, directDebitBatch.Lines.Count);

			directDebitBatch.AH_PostDate = ZDate.Today.AddDays(-3);
			AssertHasError(directDebitBatch.AH_PostDateInfo, "Post Date must be equal to or later than each transaction's Post Date.");

			directDebitBatch.AH_PostDate = ZDate.Today.AddDays(-1);
			AssertNoError(directDebitBatch.AH_PostDateInfo, "Post Date must be equal to or later than each transaction's Post Date.");
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
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

		GlbBranch SetupNewNewZelandCompanyAndBranch()
		{
			RefCountry newZeland = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand));
			GlbCompany newZelandCompany = Factory.NewWithValidTestData<GlbCompany>();
			newZelandCompany.GC_RN_NKCountryCode = newZeland.Code;
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newZelandCompany.PK;
			Factory.Save();
			return newBranch;
		}
	}
}
