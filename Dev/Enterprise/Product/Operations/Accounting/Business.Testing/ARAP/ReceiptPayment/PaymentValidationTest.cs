using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class PaymentValidationTest : ReceiptPaymentBaseValidationTest
	{
		public void TestChequeBook()
		{
			TestPayment.AH_ReceiptType = "CSH";

			TestValidation.CheckChequeBook_ForTestOnly();

			Assert(!TestPayment.ChequeBookInfo.HasErrors());

			TestPayment.AH_ReceiptType = "CHQ";
			TestValidation.ValidateChequeBook();

			AssertHasErrors(TestPayment.ChequeBookInfo);
		}

		public void TestBankAccountValidation()
		{
			AccBankAccount currentBranchAccount = TestObjectCreator.AUDBankAccount;
			currentBranchAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			AccBankAccount otherBranchAccount = TestObjectCreator.AUDBankAccount2;
			otherBranchAccount.AB_GB = TestObjectCreator.NonCurrentBranch.PK;

			TestPayment.AH_AB = currentBranchAccount.PK;
			TestPayment.Validation.ValidateAH_AB();
			AssertNoErrors("Should be no errors because bank account belongs to current branch", TestPayment.AH_ABInfo);
			TestPayment.AH_AB = otherBranchAccount.PK;
			TestPayment.Validation.ValidateAH_AB();
			AssertHasErrors("Should be errors because bank account belongs to another branch", TestPayment.AH_ABInfo);
		}

		public void TestCheckAH_ChequeOrReference_ValidateMiddleChequeNumber()
		{
			Payment testPayment = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "10000";
			Factory.Save();

			testPayment = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "10003";
			Factory.Save();

			testPayment = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "10002";

			AssertEquals("Should have NO error", false, testPayment.AH_ChequeOrReferenceInfo.HasErrors());

			testPayment = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "10003";

			AssertEquals("Should have Error", true, testPayment.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumberInBookRange()
		{
			TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			TestPayment.ChequeBook = fTestChequeBook.PK;
			TestPayment.AH_ChequeOrReference = "1234";
			AssertHasError(TestPayment.AH_ChequeOrReferenceInfo, GetExpectedErrorMessage(TestPayment.AH_ChequeOrReference));

			TestPayment.AH_ChequeOrReference = "12345";
			AssertEquals("Valid Cheque Number", false, TestPayment.AH_ChequeOrReferenceInfo.HasErrors());

			TestPayment.AH_ChequeOrReference = "123456";
			AssertHasError(TestPayment.AH_ChequeOrReferenceInfo, GetExpectedErrorMessage(TestPayment.AH_ChequeOrReference));

			TestPayment.AH_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			AssertHasError(TestPayment.AH_ChequeOrReferenceInfo, GetExpectedErrorMessage(TestPayment.AH_ChequeOrReference));

			TestPayment.AH_ChequeOrReference = "12X45";
			AssertHasError(TestPayment.AH_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");
		}

		string GetExpectedErrorMessage(string chequeNum)
		{
			return string.Format(@"Check number {0} is not contained in the selected check book.
The check number must be between 10000 and 99999.", chequeNum);
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumberAlreadyUsed_Case1()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			JobConsolCost cost = consol.GetApportionments().CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_E6 = cost.PK;
			charge.JR_JH = job.PK;
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = fTestChequeBook.BankAccount.PK;
			charge.JR_ChequeNo = "10001";
			charge.JR_OSCostAmt = 10m;
			charge.JR_LocalCostAmt = 10m;

			Payment payment1 = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			payment1.AH_ReceiptType = ReceiptTypes.Cheque;
			payment1.AH_AB = fTestChequeBook.BankAccount.PK;
			payment1.ChequeBook = fTestChequeBook.PK;
			payment1.AH_ChequeOrReference = "10002";
			Factory.Save();

			Payment payment2 = (Payment)new BusinessObjectFactory().NewWithValidTestData(GetValidationParentType());
			payment2.AH_ReceiptType = ReceiptTypes.Cheque;
			payment2.AH_AB = fTestChequeBook.BankAccount.PK;
			payment2.ChequeBook = fTestChequeBook.PK;
			payment2.AH_ChequeOrReference = "10001";
			AssertHasError(payment2.AH_ChequeOrReferenceInfo, "Check number 10001 is already used on Consol " + consol.JK_UniqueConsignRef + ".");

			charge.JR_E6 = ZGuid.Empty;
			Factory.Save();

			payment2.AH_ChequeOrReference = "";
			payment2.AH_ChequeOrReference = "10001";
			AssertHasError(payment2.AH_ChequeOrReferenceInfo, "Check number 10001 is already used on Job " + job.JH_JobNum + ".");

			payment2.AH_ChequeOrReference = "10002";
			AssertHasError(payment2.AH_ChequeOrReferenceInfo, "Check number 10002 is already in use.");

			payment2.AH_ChequeOrReference = "10003";
			AssertEquals("Valid Cheque Number", false, payment2.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestCheckAH_ChequeOrReference_ChequeNumberAlreadyUsed_Case2()
		{
			PrepareForTestChequeNumberInUse();

			TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			TestPayment.AH_AB = fTestBank.PK;
			TestPayment.ChequeBook = fTestChequeBook.PK;

			TestPayment.AH_ChequeOrReference = "10001";
			AssertHasErrors("JobCharge with this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
			TestPayment.AH_ChequeOrReference = "10002";
			AssertNoErrors("Payment Approval with this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
			TestPayment.AH_ChequeOrReference = "10003";
			AssertNoErrors("Hot Cheque with this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
			TestPayment.AH_ChequeOrReference = "10004";
			AssertHasErrors("AP Payment with this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
			TestPayment.AH_ChequeOrReference = "10005";
			AssertHasErrors("Direct Payment with this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
			TestPayment.AH_ChequeOrReference = "10006";
			AssertNoErrors("Reversed Payment with this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
			TestPayment.AH_ChequeOrReference = "10007";
			AssertNoErrors("Nothing using this cheque number", TestPayment.AH_ChequeOrReferenceInfo);
		}

		public void TestCheckAH_ChequeOrReference_CancelledChequeNumber()
		{
			Payment testPayment = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "12345";
			AssertEquals("Valid cheque number", false, testPayment.AH_ChequeOrReferenceInfo.HasErrors());

			testPayment.AH_IsCancelled = true;
			((IMatching)testPayment).CurrentMatchGroup.AddNew().AP_AH = testPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testPayment);

			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = testPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			group.Add(matchLink);

			Payment testPayment2 = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment2.AH_AB = fTestBank.PK;
			testPayment2.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment2.ChequeBook = fTestChequeBook.PK;
			testPayment2.AH_ChequeOrReference = "12345";
			AssertEquals("Cancelled Cheque number", false, testPayment2.AH_ChequeOrReferenceInfo.HasErrors());

			Factory.Save();

			Payment testPayment3 = (Payment)Factory.NewWithValidTestData(GetValidationParentType());
			testPayment3.AH_AB = fTestBank.PK;
			testPayment3.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment3.ChequeBook = fTestChequeBook.PK;
			testPayment3.AH_ChequeOrReference = "12345";
			AssertEquals("Used Cheque number", true, testPayment3.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public virtual void TestOSExTaxAmount()
		{
			TestPayment.AH_OSExTaxAmount = 1m;
			AssertEquals(false, TestPayment.AH_OSExTaxAmountInfo.HasErrors());

			TestPayment.AH_OSExTaxAmount = 0m;
			AssertHasError(TestPayment.AH_OSExTaxAmountInfo, "Please enter an Amount.");

			TestPayment.AH_OSExTaxAmount = -1m;
			AssertHasError(TestPayment.AH_OSExTaxAmountInfo, "Overseas amount must be greater than 0");
		}

		public void TestEFTDetailNoWarning()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "BANKNAME";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			TestPayment.Validation.ValidateAH_OH();

			AssertNoWarnings(TestPayment.AH_OHInfo);
		}

		public void TestEFTDetailWarningForDirectDebitFlag()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestPayment.Validation.ValidateAH_OH();

			AssertHasWarning(TestPayment.AH_OHInfo,
				$"Organization {TestObjectCreator.TestOrganisation.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - BSB Number" + System.Environment.NewLine +
					"   - Account Name" + System.Environment.NewLine +
					"   - Bank Account" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitBSBNumber()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "";
			accountDetails.A1_AccountName = "BANKNAME";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestPayment.Validation.ValidateAH_OH();

			AssertHasWarning(TestPayment.AH_OHInfo,
				$"Organization {TestObjectCreator.TestOrganisation.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - BSB Number" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitAccountName()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestPayment.Validation.ValidateAH_OH();

			AssertHasWarning(TestPayment.AH_OHInfo,
				$"Organization {TestObjectCreator.TestOrganisation.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - Account Name" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitBankAccount()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_AccountName = "AccountName";
			accountDetails.A1_BankAccount = "";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestPayment.Validation.ValidateAH_OH();

			AssertHasWarning(TestPayment.AH_OHInfo,
				$"Organization {TestObjectCreator.TestOrganisation.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - Bank Account" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForDirectDebitCombined()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			TestPayment.Validation.ValidateAH_OH();

			AssertHasWarning(TestPayment.AH_OHInfo,
				$"Organization {TestObjectCreator.TestOrganisation.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - Auto Direct Debit" + System.Environment.NewLine +
					"   - BSB Number" + System.Environment.NewLine +
					"   - Account Name" + System.Environment.NewLine +
					"   - Bank Account" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public void TestEFTDetailWarningForBSBFormat()
		{
			SetUpPaymentForTesting();
			AccAPAccountDetails accountDetails = TestObjectCreator.TestOrganisation.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "123-456";
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "654321";
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;
			TestObjectCreator.AUDBankAccount.AB_AutoDDRFormat = "ASB";

			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			TestPayment.Validation.ValidateAH_OH();
			AssertHasWarning(TestPayment.AH_OHInfo,
				$"Organization {TestObjectCreator.TestOrganisation.OH_Code} has following Banking Details incorrectly setup.{System.Environment.NewLine}{"   - BSB Number is in incorrect format. It should be in XXXXXX format" + System.Environment.NewLine}" +
				"You'll need to correct this before you can generate DDR file.");
		}

		public override void TestCheckAH_ReceiptType()
		{
			base.TestCheckAH_ReceiptType();

			SetUpPaymentForTesting();

			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			AssertHasError(TestPayment.AH_ReceiptTypeInfo, "Enter a valid " + TestPayment.AH_ReceiptTypeInfo.Description + ".");

			Factory.Save();

			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			AssertNoError(TestPayment.AH_ReceiptTypeInfo, "Enter a valid " + TestPayment.AH_ReceiptTypeInfo.Description + ".");
		}

		public void TestCheckAH_ReceiptTypeSecurity_ARPayment()
		{
			if (TestPayment.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				Env.Security.NewReceivablesPaymentCheque.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentCheque.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewReceivablesPaymentCash.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cash;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentCash.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cash;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewReceivablesPaymentCreditCard.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.CreditCard;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentCreditCard.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.CreditCard;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewReceivablesPaymentDirectDebit.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentDirectDebit.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewReceivablesPaymentEFT.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.EFT;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentEFT.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.EFT;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewReceivablesPaymentSFT.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.ScheduledEFT;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentSFT.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.ScheduledEFT;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewReceivablesPaymentCRQ.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.CollectionRequest;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewReceivablesPaymentCRQ.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.CollectionRequest;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			}
			else
			{
				Assert("The test is not applicable for AP payments", true);
			}
		}

		public void TestCheckAH_ReceiptTypeSecurity_APPayment()
		{
			if (TestPayment.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewPayablesPaymentCash.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cash;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentCash.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.Cash;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.CreditCard;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.CreditCard;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.EFT;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.EFT;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewPayablesPaymentSFT.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.ScheduledEFT;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentSFT.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.ScheduledEFT;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

				Env.Security.NewPayablesPaymentCRQ.IsAllowed = true;
				TestPayment.AH_ReceiptType = ReceiptTypes.CollectionRequest;
				Assert("Receipt Type should have no errors", !TestPayment.AH_ReceiptTypeInfo.HasErrors());
				Env.Security.NewPayablesPaymentCRQ.IsAllowed = false;
				TestPayment.AH_ReceiptType = ReceiptTypes.CollectionRequest;
				AssertHasError(TestPayment.AH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			}
			else
			{
				Assert("The test is not applicable for AR payments", true);
			}
		}

		public void TestAccountDetailsNotFound()
		{
			SetUpPaymentForTesting();
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			TestPayment.Validation.ValidateAH_OH();

			AssertNoWarnings(TestPayment.AH_OHInfo);
			AssertHasErrors("Payee/OrgHeader should have errors", TestPayment.AH_OHInfo);
			string expectedMessage = string.Format("An AP Bank Account could not be found with currency {0} and payment type DDR for the payee {1}.\r\n\r\nPlease set up an AP Account for the organization {1} under the AP Details tab, by right-clicking this grid and selecting \"Edit Payment Organization Detail\", with the currency {0} and payment type of DDR.", Core.Constants.CurrencyCodes.Australia, TestPayment.Header.OH_Code);
			AssertContains("Incorrect error message", expectedMessage, TestPayment.AH_OHInfo.GetErrors().GetFirstMessage());
			AssertNoErrors("AllowAutoDDR should not have errors", TestPayment.AllowAutoDDRInfo);
			AssertNoErrors("BankBsb should not have errors", TestPayment.PayeeBankBSBInfo);
		}

		public void TestChequeOrReferenceValidationIsDisabledForAutoAllocationMode()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook();

			SetUpPaymentForTesting();
			TestPayment.AH_ReceiptType = ReceiptTypes.Cheque;

			TestPayment.AH_AB = testChequeBook.AK_AB;
			TestPayment.ChequeBook = testChequeBook.PK;
			TestPayment.AH_ChequeOrReference = "BLAH!";
			AssertHasErrors("Cheque Number should have an error", TestPayment.AH_ChequeOrReferenceInfo);

			TestPayment.AH_AB = autoPrintChequeBook.AK_AB;
			TestPayment.ChequeBook = autoPrintChequeBook.PK;
			TestPayment.AH_ChequeOrReference = "BLAH!";
			AssertNoErrors("Should not have any errors as now in autoallocation mode", TestPayment.AH_ChequeOrReferenceInfo);
		}

		void SetUpPaymentForTesting()
		{
			var bankAccount = TestObjectCreator.AUDBankAccount;
			bankAccount.AB_AllowAutoDDR = ZBool.True;
			bankAccount.AB_AutoDDRFormat = Constants.DDRFileFormat.ASB;

			TestPayment.AH_AB = bankAccount.PK;
			TestPayment.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
		}

		void PrepareForTestChequeNumberInUse()
		{
			Job testJob = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 1m, TestObjectCreator.Agent);
			charge.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_OSCostExRate = 1m;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			Assert("Cheque number is not used yet", !charge.JR_ChequeNoInfo.HasErrors());

			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = TestObjectCreator.TestOrganisation.PK;
			paymentApproval.AV_AB = fTestBank.PK;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			paymentApproval.AV_AK = fTestChequeBook.PK;
			paymentApproval.AV_ChequeOrReference = "10002";
			Assert("Cheque number is not used yet", !paymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_AK = fTestChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "10003";
			Assert("Cheque number is not used yet", !hotCheque.AQ_ChequeNumberInfo.HasErrors());

			Payment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ReceiptTypes.Cheque;
			payment.AH_AB = fTestBank.PK;
			payment.ChequeBook = fTestChequeBook.PK;
			payment.AH_ChequeOrReference = "10004";
			Assert("Cheque number is not used yet", !payment.AH_ChequeOrReferenceInfo.HasErrors());

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			directPayment.AH_AB = fTestBank.PK;
			directPayment.ChequeBookPK = fTestChequeBook.PK;
			directPayment.AH_ChequeOrReference = "10005";
			Assert("Cheque number is not used yet", !directPayment.AH_ChequeOrReferenceInfo.HasErrors());

			Payment reversedPayment = Factory.NewWithValidTestData<APPayment>();
			reversedPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			reversedPayment.AH_AB = fTestBank.PK;
			reversedPayment.ChequeBook = fTestChequeBook.PK;
			reversedPayment.AH_ChequeOrReference = "10006";
			reversedPayment.AH_IsCancelled = true;
			((IMatching)reversedPayment).CurrentMatchGroup.AddNew().AP_AH = reversedPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(reversedPayment);
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = (ZByte)5;
			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;
		}

		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;

		protected Payment TestPayment
		{
			get { return (Payment)TestReceiptPayment; }
		}

		protected new PaymentValidation TestValidation
		{
			get { return (PaymentValidation)base.TestValidation; }
		}
	}
}
