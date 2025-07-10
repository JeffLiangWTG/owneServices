using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.EPaymentDealCreator;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	public class EPaymentDealCreatorTest : TestCaseWithFactory
	{
		public void TestOrganisationDoesNotHaveABankAccountWithEPOMethodAndSameCurrency()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.NewZealand;
			accountDetail2.A1_IsDefaultAccount = true;
			Factory.Save();

			var expectedMessage = "An AP Bank Account could not be found with currency USD and payment type EPO for the payee AALSHI.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.ValidationErrors, status);
			AssertEquals(null, quoteToAccept);
			AssertEquals(expectedMessage, message);
		}

		public void TestOrganisationHasABankAccountWithEPOMethodAndSameCurrencyButNotLinkedToBeneficiary()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			Factory.Save();

			Assert(accountDetail2.A1_EPaymentBeneficiaryId.IsEmpty);

			var expectedMessage = "Payee Bank Account is not configured for E-Payment Processing. Please configure the Account Details in A/P tab of the Payee organization.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.ValidationErrors, status);
			AssertEquals(null, quoteToAccept);
			AssertEquals(expectedMessage, message);
		}

		public void TestOrganisationHasABankAccountWithEPOMethodAndSameCurrencyAndLinkedToBeneficiaryButBeneficiaryProviderReferenceIsEmpty()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			Assert(!accountDetail2.A1_EPaymentBeneficiaryId.IsEmpty);
			Assert(accountDetail2.EPaymentBeneficiary.ABF_ProviderReference.IsEmpty);

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 AUD
Processing Fee: 10 AUD
Funding Currency Total Cost: 1010 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, status);
			AssertEquals(quote1.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestOrganisationHasABankAccountWithEPOMethodAndSameCurrencyAndLinkedToBeneficiaryAndBeneficiaryProviderReferenceIsNotEmpty()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			var accountDetail2 = accountDetails.AddNew();
			accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail2.A1_IsDefaultAccount = true;
			accountDetail2.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			Assert(!accountDetail2.A1_EPaymentBeneficiaryId.IsEmpty);
			Assert(!accountDetail2.EPaymentBeneficiary.ABF_ProviderReference.IsEmpty);

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 AUD
Processing Fee: 10 AUD
Funding Currency Total Cost: 1010 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, status);
			AssertEquals(quote1.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestCheckUserIsAllowedToProcessEPayment()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			Env.Security.APPaymentProcessingProcessEPayment.IsAllowed = false;
			var expectedMessage = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payment Processing -> Process E-Payment";
			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.ValidationErrors, status);
			AssertNull(quoteToAccept);
			AssertEquals(expectedMessage, message);

			Env.Security.APPaymentProcessingProcessEPayment.IsAllowed = true;
			expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 AUD
Processing Fee: 10 AUD
Funding Currency Total Cost: 1010 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			(status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, status);
			AssertEquals(quote1.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestPaymentApprovalBankAccountIsNotEPaymentAccountType()
		{
			var paymentApproval = CreatePaymentApproval(TestObjectCreator.USDBankAccount);
			Factory.Save();

			AssertNotEquals(AccountTypeCodeDescriptionPairList.Codes.EPA, paymentApproval.BankAccount.AB_AccountType);

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.ValidationErrors, status);
			AssertNull(quoteToAccept);
			AssertEquals("Payment Bank Account must be an E-Payment Account in order to submit payment for electronic processing.", message);
		}

		public void TestUserAuthorization()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			AssertEquals(0, paymentApproval.BankAccount.EPaymentStaffTokenCollection.Count);
			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals("No staff tokens available", QuoteAcceptingStatus.ValidationErrors, status);
			AssertNull("No staff tokens available", quoteToAccept);
			AssertEquals("No staff tokens available", "Your staff profile is not currently registered in the list of users who can use this Bank Account. To manage users, edit the Bank Account and select “Manage Users”.", message);

			var staffToken = TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.Empty, Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Pending);
			Factory.Save();

			paymentApproval.BankAccount.EPaymentStaffTokenCollection.Reload(true);
			AssertEquals(1, paymentApproval.BankAccount.EPaymentStaffTokenCollection.Count);

			(status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals("Staff token available but not authorized.", QuoteAcceptingStatus.UserNotAuthorized, status);
			AssertNull("Staff token available but not authorized.", quoteToAccept);
			AssertEquals("Staff token available but not authorized.", @"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", message);

			staffToken.TK_Status = AccEPaymentStaffTokenLookups.StatusCodes.Authorised;
			staffToken.TK_ExpiryUtc = ZDateTime.UtcNow.AddMinutes(45);
			Factory.Save();

			paymentApproval.BankAccount.EPaymentStaffTokenCollection.Reload(true);

			(status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals("Staff token available, authorized but expires within one hour.", QuoteAcceptingStatus.UserNotAuthorized, status);
			AssertNull("Staff token available, authorized but expires within one hour.", quoteToAccept);
			AssertEquals("Staff token available, authorized but expires within one hour.", @"You need to authorize your OFX user account before proceeding.
Please note that the authorization process can take up to several minutes.
Would you like to authorize your account now?", message);

			staffToken.TK_ExpiryUtc = ZDateTime.UtcNow.AddHours(2);
			Factory.Save();

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 AUD
Processing Fee: 10 AUD
Funding Currency Total Cost: 1010 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			(status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, status);
			AssertEquals(quote1.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestPaymentIsNotApproved()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			paymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.ValidationErrors, status);
			AssertNull(quoteToAccept);
			AssertEquals("Only payments in Approved status can be submitted for processing.", message);
		}

		public void TestCheckDealIsNotCreatedAlready()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			paymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			var quote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			var deal = Factory.New<EPaymentDeal>();
			deal.AED_QU_Quote = quote.PK;
			deal.AED_GC_Company = Env.CurrentCompanyPK;
			deal.AED_ProviderCode = EPaymentProviderCodes.Codes.OFX;
			Factory.Save();

			AssertEquals(DealStatusCodes.Queued, deal.AED_Status);

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.ValidationErrors, status);
			AssertNull(quoteToAccept);
			AssertEquals("This payment has already been submitted for processing.", message);
		}

		public void TestPaymentApprovalHasAnAcceptedQuoteAndPaymentDetailsAreMatching()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			AssertEquals(paymentApproval.AV_Calc_LocalAmount, quote1.QU_FromAmount);
			AssertEquals(paymentApproval.AV_Calc_LocalCurrency, quote1.QU_RX_NKFromCurrency);
			AssertEquals(paymentApproval.AV_Amount, quote1.QU_ToAmount);
			AssertEquals(paymentApproval.AV_RX_NKPaymentCurrency, quote1.QU_RX_NKToCurrency);

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 AUD
Processing Fee: 10 AUD
Funding Currency Total Cost: 1010 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, status);
			AssertEquals(quote1.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestPaymentApprovalHasAnAcceptedQuoteButPaymentDeatilsAreDifferent()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FromAmount = 2000m;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			AssertNotEquals(paymentApproval.AV_Calc_LocalAmount, quote1.QU_FromAmount);
			AssertEquals(paymentApproval.AV_Calc_LocalCurrency, quote1.QU_RX_NKFromCurrency);
			AssertEquals(paymentApproval.AV_Amount, quote1.QU_ToAmount);
			AssertEquals(paymentApproval.AV_RX_NKPaymentCurrency, quote1.QU_RX_NKToCurrency);

			var expectedMessage = "Payment has an accepted E-Quote, but its details don't match the payment. Please review payment details in order to continue.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAcceptedButQuoteIsInvalid, status);
			AssertEquals(null, quoteToAccept);
			AssertEquals(expectedMessage, message);
		}

		public void TestPaymentApprovalHasAcceptedQuote_WithFundingCurrencyFromPaymentBatch()
		{
			var (quote, payment) = CreateQuoteFromBatchPoster();

			var (isQuoteAccepted, userMessage) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, payment, false);

			AssertEquals(true, isQuoteAccepted);
			AssertEquals($"Quote {quote.QU_InternalReference} has been accepted.", userMessage);
			AssertEquals(QuoteStatusCodes.Accepted, quote.QU_Status);

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: 
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 CNY
Processing Fee: 10 CNY
Funding Currency Total Cost: 1010 CNY

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(payment);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, status);
			AssertEquals(quote.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestPaymentApprovalHasReceivedQuote_WithFundingCurrencyFromPaymentBatch()
		{
			var (quote, payment) = CreateQuoteFromBatchPoster();

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.
Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.

Provider: OFX
Payment To: 
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 CNY
Processing Fee: 10 CNY
Funding Currency Total Cost: 1010 CNY

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(payment);
			AssertEquals(QuoteAcceptingStatus.QuoteInReceivedStatusNeedUserAcceptance, status);
			AssertEquals(quote.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		(EPaymentQuote, PaymentApprovalBase) CreateQuoteFromBatchPoster()
		{
			var fundingBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var accBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			fundingBankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.China;
			Factory.Save();

			TestObjectCreator.CreateEPaymentStaffToken(accBankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);

			var aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			var transactions = new TransactionHeaderCollection(Factory);
			transactions.Add(aPInvoice);

			var batchPoster = Factory.NewWithValidTestData<APPaymentBatchPoster>();
			batchPoster.SetDefaultValuesByTransactions(transactions);
			var payment = batchPoster.PaymentApprovalCollection[0];
			batchPoster.SetPaymentDetails(payment);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			payment.AV_OH = orgHeader.PK;
			batchPoster.APB_AB = accBankAccount.PK;
			batchPoster.APB_AB_FundingBankAccount = fundingBankAccount.PK;
			Factory.Save();

			payment.AV_Amount = 1000m;
			payment.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = payment.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			AssertEquals(true, batchPoster.IsInDatabase);
			AssertEquals(true, payment.IsInDatabase);
			AssertEquals("There should be 1 payment in the collection", 1, batchPoster.PaymentApprovalCollection.Count);
			AssertEquals("There should be 1 approval linked to AccPaymentBatch", 1, batchPoster.PaymentApprovalCollection.Count);

			var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote.QU_Status = QuoteStatusCodes.Received;
			quote.QU_ProviderReference = "testreference";
			quote.QU_FromAmount = 1000m;
			quote.QU_FeeAmount = 10m;
			quote.QU_RX_NKFeeCurrency = batchPoster.FundingBankAccountCurrency;
			quote.QU_ExchangeRate = 1;
			quote.QU_ExchangeRateInverted = 1;

			return (quote, payment);
		}

		public void TestPaymentApprovalHasNeitherAcceptedQuotesNorReceivedQuotes()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Requested, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			var expectedMessage = "Payment must have an accepted E-Quote in order to continue. Would you like to request an E-Quote now?";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.NotAcceptedOrReceivedQuoteFound, status);
			AssertEquals(null, quoteToAccept);
			AssertEquals(expectedMessage, message);
		}

		public void TestPaymentApprovalHasAReceivedQuote()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			Factory.Save();

			AssertEquals(paymentApproval.AV_Calc_LocalAmount, quote1.QU_FromAmount);
			AssertEquals(paymentApproval.AV_Calc_LocalCurrency, quote1.QU_RX_NKFromCurrency);
			AssertEquals(paymentApproval.AV_Amount, quote1.QU_ToAmount);
			AssertEquals(paymentApproval.AV_RX_NKPaymentCurrency, quote1.QU_RX_NKToCurrency);

			var expectedMessage = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.
Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 AUD
Processing Fee: 10 AUD
Funding Currency Total Cost: 1010 AUD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";

			var (status, quoteToAccept, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteInReceivedStatusNeedUserAcceptance, status);
			AssertEquals(quote1.PK, quoteToAccept.PK);
			AssertEquals(expectedMessage, message);
		}

		public void TestTryToCreateEPaymentDeal()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = CreatePaymentApproval(bankAccount);
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var quote1 = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			quote1.QU_FeeAmount = 10m;
			quote1.QU_RX_NKFeeCurrency = paymentApproval.AV_Calc_LocalCurrency;
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Failed, paymentApproval);
			TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Error, paymentApproval);
			Factory.Save();

			AssertEquals(paymentApproval.AV_Calc_LocalAmount, quote1.QU_FromAmount);
			AssertEquals(paymentApproval.AV_Calc_LocalCurrency, quote1.QU_RX_NKFromCurrency);
			AssertEquals(paymentApproval.AV_Amount, quote1.QU_ToAmount);
			AssertEquals(paymentApproval.AV_RX_NKPaymentCurrency, quote1.QU_RX_NKToCurrency);

			var (isDealCreated, message) = TryToCreateEPaymentDeal(paymentApproval, quote1);
			Assert(isDealCreated);
			AssertEquals("E-Payment Deal request generated to service provider OFX. Response may take from a few moments up to several minutes to be received.", message);

			var deal = new BusinessObjectFactory().LoadTop1<EPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, quote1.PK));
			AssertNotNull(deal);
			AssertEquals(Env.CurrentCompanyPK, deal.AED_GC_Company);
			AssertEquals(quote1.QU_ProviderCode, deal.AED_ProviderCode);
			AssertEquals(DealStatusCodes.Queued, deal.AED_Status);
		}

		public void TestCheckPaymentDetailsAreSameAsQuoteDetails_FromSinglePayment_FromCurrencyDiffersFundingCurrency()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var paymentApproval = CreatePaymentApproval(bankAccount);
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;

			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			var newQuote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			newQuote.QU_RX_NKFromCurrency = "EUR";
			newQuote.QU_FromAmount = 500m;

			Factory.Save();

			AssertNotEquals(newQuote.QU_RX_NKFromCurrency, paymentApproval.FundingCurrency);
			AssertNotEquals(newQuote.QU_FromAmount, paymentApproval.AV_Calc_LocalAmount);
			var (quoteStatus, _, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAcceptedButQuoteIsInvalid, quoteStatus);
			AssertEquals("Payment has an accepted E-Quote, but its details don't match the payment. Please review payment details in order to continue.", message);
		}

		public void TestCheckPaymentDetailsAreSameAsQuoteDetails_FromSinglePayment_FromCurrencyEqualsFundingCurrency()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var paymentApproval = CreatePaymentApproval(bankAccount);
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;

			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			var newQuote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			newQuote.QU_RX_NKFromCurrency = "USD";

			Factory.Save();

			var (quoteStatus, _, _) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, quoteStatus);
		}

		public void TestCheckPaymentDetailsAreSameAsQuoteDetails_FromSinglePayment_AmountNotEqualForLocalCurrency()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var paymentApproval = CreatePaymentApproval(bankAccount);
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;

			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			var newQuote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			newQuote.QU_FromAmount = 900m;

			Factory.Save();

			var (quoteStatus, _, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAcceptedButQuoteIsInvalid, quoteStatus);
			AssertEquals("Payment has an accepted E-Quote, but its details don't match the payment. Please review payment details in order to continue.", message);
		}

		public void TestGetUserMessageForReceivedQuote_FromSinglePayment()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var paymentApproval = CreatePaymentApproval(bankAccount);
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;

			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			var newQuote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Received, paymentApproval);
			newQuote.QU_RX_NKFromCurrency = "USD";

			Factory.Save();

			var expectMsg = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.
Payment must have an accepted E-Quote in order to continue. The following E-Quote is available for this Payment and will be automatically accepted if you proceed to book the deal.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 USD
Processing Fee: 0 
Funding Currency Total Cost: 1000 USD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";
			var (quoteStatus, _, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(expectMsg, message);
			AssertEquals(QuoteAcceptingStatus.QuoteInReceivedStatusNeedUserAcceptance, quoteStatus);

			expectMsg = @"Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 USD
Processing Fee: 0 
Funding Currency Total Cost: 1000 USD";

			(quoteStatus, _, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteInReceivedStatusNeedUserAcceptance, quoteStatus);
			AssertContains(expectMsg, message);
		}

		public void TestGetUserMessageForAcceptedQuote_FromSinglePayment()
		{
			var bankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			TestObjectCreator.CreateEPaymentStaffToken(bankAccount.PK, ZDateTime.UtcNow.AddHours(2), Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			var paymentApproval = CreatePaymentApproval(bankAccount);
			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;

			var beneficiary = TestObjectCreator.CreateEPaymentBeneficiary("apple");
			var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
			accountDetails.RemoveAndDeleteAll();
			var accountDetail1 = accountDetails.AddNew();
			accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetail1.A1_IsDefaultAccount = true;
			accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;
			var newQuote = TestObjectCreator.CreateValidEPaymentQuoteForStatus(QuoteStatusCodes.Accepted, paymentApproval);
			newQuote.QU_RX_NKFromCurrency = "USD";

			Factory.Save();

			var expectMsg = @"This FX transaction will be executed by a third party provider, OFX. CargoWise provides the messaging and information exchange only.
Review the details of the transaction to ensure they are correct.

Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 USD
Processing Fee: 0 
Funding Currency Total Cost: 1000 USD

The OS Amount is the amount your recipient will receive* and the Total Cost is the amount you will be required to pay to OFX.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and OFX receives no portion of it.

By clicking “Yes”, this FX transaction with OFX becomes legally binding if accepted by OFX. Would you like to continue?

Please check the status of the transaction after clicking ""Yes"".
If the OFX quote has expired, OFX cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transaction, please contact OFX directly.";
			var (quoteStatus, _, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(expectMsg, message);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, quoteStatus);

			expectMsg = @"Provider: OFX
Payment To: A.A.L. SHIPPING AGENCIES P/L
OS Amount: 1000 USD
Exchange Rate: 1 (1)
Funding Currency Amount: 1000 USD
Processing Fee: 0 
Funding Currency Total Cost: 1000 USD";

			(quoteStatus, _, message) = FindAcceptedQuote(paymentApproval);
			AssertEquals(QuoteAcceptingStatus.QuoteAlreadyAccepted, quoteStatus);
			AssertContains(expectMsg, message);
		}

		PaymentApprovalBase CreatePaymentApproval(AccBankAccount bankAccount)
		{
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, bankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval.AV_OH = TestObjectCreator.AALSHI.PK;
			paymentApproval.AV_RX_NKPaymentCurrency = "USD";
			paymentApproval.AV_PayExRate = 1m;
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-1);
			paymentApproval.AV_PaymentDate = ZDateTime.Today.AddDays(1);
			paymentApproval.AV_PaymentComment = "Paying FreightQuota Invoice 83942";
			paymentApproval.AV_ChequeOrReference = "00009283";
			paymentApproval.AV_GB = Env.CurrentBranchPK;
			paymentApproval.AV_GC = Env.CurrentCompanyPK;
			return paymentApproval;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
