using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class AccEPaymentQuoteToGEPConverterTest : TestCaseWithFactory
	{
		#region Quote

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_GetRatesMessageType_WhenBankAccountTypeIsNotEPA()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var expectedUniversalTransaction = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccEPaymentQuote</Type>
          <Key>00001000</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>CAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Company</Name>
      </Company>
      <DataProvider>EDIDATCAU</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <BankAccount>EPA</BankAccount>
    <Branch>
      <Code>BAU</Code>
      <Name></Name>
    </Branch>
    <CheckNumberOrPaymentRef>00009283</CheckNumberOrPaymentRef>
    <Description>Paying FreightQuota Invoice 83942</Description>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </LocalCurrency>
    <LocalTotal>0</LocalTotal>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSTotal>1000</OSTotal>
    <PaymentOrReceiptType>EPA</PaymentOrReceiptType>
    <PostDate>2021-04-08T00:00:00</PostDate>
    <TransactionDate>2021-04-10T00:00:00</TransactionDate>
    <TransactionReference>00001000</TransactionReference>
    <TransactionType>PAY</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";

			ofxBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			AssertNotEquals("Precondition", AccountTypeCodeDescriptionPairList.Codes.EPA, quote.PaymentApproval.BankAccount.AB_AccountType);

			var converter = new AccEPaymentQuoteToGEPConverter();
			var ePayment = converter.ConvertQuoteToGEP(quote);

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.GetRates, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", false, ePayment.Header.ElectronicPaymentRequest.UserPkSpecified);
			AssertEquals("UserCode", false, ePayment.Header.ElectronicPaymentRequest.UserCodeSpecified);
			AssertEquals("UserAccountName", false, ePayment.Header.ElectronicPaymentRequest.UserAccountNameSpecified);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var universalTransactionFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("Universal Transaction", expectedUniversalTransaction, universalTransactionFromPayload);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenAuthorizedUserDoesNotExist()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.Empty, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised);
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var converter = new AccEPaymentQuoteToGEPConverter();
			AssertExceptionThrown<GEPMessageCreationException>(
				"Only authorized users can create deals",
				"No authorized staff token found for code 'YOU'.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenTokenExpiryIsLessThanOneHour()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var authExpirationUtc = ZDateTime.UtcNow.AddMinutes(20);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, authExpirationUtc, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var converter = new AccEPaymentQuoteToGEPConverter();
			AssertExceptionThrown<GEPMessageCreationException>(
				"Only authorized users can create deals",
				$"The users authorization is either expired or due to expire within an hour. Expiry Date (UTC): {authExpirationUtc.ToBestReadableDateTimeString()}.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenTokenAlreadyExpired()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var authExpirationUtc = ZDateTime.UtcNow.AddMinutes(-1);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, authExpirationUtc, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var converter = new AccEPaymentQuoteToGEPConverter();
			AssertExceptionThrown<GEPMessageCreationException>(
				"Only authorized users can create deals",
				$"The users authorization is either expired or due to expire within an hour. Expiry Date (UTC): {authExpirationUtc.ToBestReadableDateTimeString()}.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_GetAQuoteMessageType()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var expectedUniversalTransaction = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccEPaymentQuote</Type>
          <Key>00001000</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>CAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Company</Name>
      </Company>
      <DataProvider>EDIDATCAU</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <BankAccount>EPA</BankAccount>
    <Branch>
      <Code>BAU</Code>
      <Name></Name>
    </Branch>
    <CheckNumberOrPaymentRef>00009283</CheckNumberOrPaymentRef>
    <Description>Paying FreightQuota Invoice 83942</Description>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </LocalCurrency>
    <LocalTotal>0</LocalTotal>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSTotal>1000</OSTotal>
    <PaymentOrReceiptType>EPA</PaymentOrReceiptType>
    <PostDate>2021-04-08T00:00:00</PostDate>
    <TransactionDate>2021-04-10T00:00:00</TransactionDate>
    <TransactionReference>00001000</TransactionReference>
    <TransactionType>PAY</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";

			var converter = new AccEPaymentQuoteToGEPConverter();
			var ePayment = converter.ConvertQuoteToGEP(quote);

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.GetAQuote, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company1.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company1.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", quoteCreatingUser.PK.ToString(), ePayment.Header.ElectronicPaymentRequest.UserPk);
			AssertEquals("UserCode", staffToken.TK_GS_NKStaffCode, ePayment.Header.ElectronicPaymentRequest.UserCode);
			AssertEquals("UserAccountName", staffToken.TK_AccountName, ePayment.Header.ElectronicPaymentRequest.UserAccountName);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var universalTransactionFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("Universal Transaction", expectedUniversalTransaction, universalTransactionFromPayload);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenBankAccountIsNull()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			quote.PaymentApproval.AV_AB = ZGuid.Empty;
			AssertNull("Precondition : Bank Account is null", quote.PaymentApproval.BankAccount);

			var converter = new AccEPaymentQuoteToGEPConverter();
			AssertExceptionThrown<GEPMessageCreationException>(
				"Must have a valid bank account",
				"The bank account is missing.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenCreatingUserIsNull()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var converter = new AccEPaymentQuoteToGEPConverterThatReturnsNullUser();
			AssertExceptionThrown<GEPMessageCreationException>(
				"The creating user is required.",
				"The user who created the request could not be found.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenStaffCodeIsEmpty()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, ZString.Empty, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			AssertEquals(ZString.Empty, staffToken.TK_GS_NKStaffCode);

			var converter = new AccEPaymentQuoteToGEPConverter();
			AssertExceptionThrown<GEPMessageCreationException>(
				"Only authorized users can create deals",
				"No authorized staff token found for code 'YOU'.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_ThrowsGEPMessageCreationException_WhenUserAccountNameIsEmpty()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			AssertEquals(ZString.Empty, staffToken.TK_AccountName);

			var converter = new AccEPaymentQuoteToGEPConverter();
			AssertExceptionThrown<GEPMessageCreationException>(
				"Only authorized users can create deals",
				"The staff token is missing an account name.",
				() => converter.ConvertQuoteToGEP(quote));
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_GetAQuoteMessageType_EnableFundingEPaymentDealsfromForeignCurrencyBankAccounts()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company = objectCreator.CreateCompanyAndBranch("AUMEL");
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;

			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var accBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				accBankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.China;
				Factory.Save();

				var aPPaymentBatchPoster = Factory.NewWithValidTestData<APPaymentBatchPoster>();
				aPPaymentBatchPoster.APB_AB_FundingBankAccount = accBankAccount.PK;
				Factory.Save();

				var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
				paymentApproval.InitializeForPaymentBatch(() => false);
				paymentApproval.AV_AB = ofxBankAccount.PK;
				paymentApproval.AV_AK = testHelper.ObjectCreator.USDChequeBook.PK;
				paymentApproval.AV_PaymentType = ReceiptTypes.EPayment;
				paymentApproval.AV_RX_NKPaymentCurrency = "USD";
				paymentApproval.AV_PayExRate = 2.5m;
				paymentApproval.AV_Amount = 1000m;
				paymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-1);
				paymentApproval.AV_PaymentDate = ZDateTime.Today.AddDays(1);
				paymentApproval.AV_PaymentComment = "Paying FreightQuota Invoice 83942";
				paymentApproval.AV_ChequeOrReference = "00009283";
				paymentApproval.AV_GB = company.FirstActiveBranch.PK;
				paymentApproval.AV_GC = company.PK;
				paymentApproval.AV_APB_PaymentBatch = aPPaymentBatchPoster.PK;
				Factory.Save();

				quote = paymentApproval.TryToCreateQuote(ofxBankAccount.AB_PaymentProvider.ToString(), false).Quote;
				Factory.Save();
			}

			var expectedUniversalTransaction = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccEPaymentQuote</Type>
          <Key>00001000</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>CAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Company</Name>
      </Company>
      <DataProvider>EDIDATCAU</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <BankAccount>EPA</BankAccount>
    <Branch>
      <Code>BAU</Code>
      <Name></Name>
    </Branch>
    <CheckNumberOrPaymentRef>00009283</CheckNumberOrPaymentRef>
    <Description>Paying FreightQuota Invoice 83942</Description>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>CNY</Code>
      <Description>Chinese Yuan</Description>
    </LocalCurrency>
    <LocalTotal>0</LocalTotal>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSTotal>1000</OSTotal>
    <PaymentOrReceiptType>EPA</PaymentOrReceiptType>
    <PostDate>2021-04-08T00:00:00</PostDate>
    <TransactionDate>2021-04-10T00:00:00</TransactionDate>
    <TransactionReference>00001000</TransactionReference>
    <TransactionType>PAY</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";

			var converter = new AccEPaymentQuoteToGEPConverter();
			var ePayment = converter.ConvertQuoteToGEP(quote);

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.GetAQuote, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", quoteCreatingUser.PK.ToString(), ePayment.Header.ElectronicPaymentRequest.UserPk);
			AssertEquals("UserCode", staffToken.TK_GS_NKStaffCode, ePayment.Header.ElectronicPaymentRequest.UserCode);
			AssertEquals("UserAccountName", staffToken.TK_AccountName, ePayment.Header.ElectronicPaymentRequest.UserAccountName);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var universalTransactionFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("Universal Transaction", expectedUniversalTransaction, universalTransactionFromPayload);
		}

		[TestDate(2021, 4, 9)]
		public void TestConvertQuoteToGEP_GetAQuoteMessageType_EnableFundingEPaymentDealsfromForeignCurrencyBankAccounts_BatchPosterIsNull()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company = objectCreator.CreateCompanyAndBranch("AUMEL");
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;

			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company, ofxBankAccount);
				Factory.Save();
			}

			var expectedUniversalTransaction = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccEPaymentQuote</Type>
          <Key>00001000</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>CAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Company</Name>
      </Company>
      <DataProvider>EDIDATCAU</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <BankAccount>EPA</BankAccount>
    <Branch>
      <Code>BAU</Code>
      <Name></Name>
    </Branch>
    <CheckNumberOrPaymentRef>00009283</CheckNumberOrPaymentRef>
    <Description>Paying FreightQuota Invoice 83942</Description>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </LocalCurrency>
    <LocalTotal>0</LocalTotal>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSTotal>1000</OSTotal>
    <PaymentOrReceiptType>EPA</PaymentOrReceiptType>
    <PostDate>2021-04-08T00:00:00</PostDate>
    <TransactionDate>2021-04-10T00:00:00</TransactionDate>
    <TransactionReference>00001000</TransactionReference>
    <TransactionType>PAY</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";

			var converter = new AccEPaymentQuoteToGEPConverter();
			var ePayment = converter.ConvertQuoteToGEP(quote);

			AssertEquals("MessagingSystem", ProviderCodes.OFX, ePayment.Header.ElectronicPaymentRequest.MessagingSystem);
			AssertEquals("MessageType", GEPProviderAPICommandList.Codes.GetAQuote, ePayment.Header.ElectronicPaymentRequest.MessageType);
			AssertEquals("CompanyCode", company.GC_Code, ePayment.Header.ElectronicPaymentRequest.CompanyCode);
			AssertEquals("BranchCode", company.FirstActiveBranch.GB_Code, ePayment.Header.ElectronicPaymentRequest.BranchCode);
			AssertEquals("UserPk", quoteCreatingUser.PK.ToString(), ePayment.Header.ElectronicPaymentRequest.UserPk);
			AssertEquals("UserCode", staffToken.TK_GS_NKStaffCode, ePayment.Header.ElectronicPaymentRequest.UserCode);
			AssertEquals("UserAccountName", staffToken.TK_AccountName, ePayment.Header.ElectronicPaymentRequest.UserAccountName);
			AssertEquals("IsProductionSystem", false, ePayment.Header.ElectronicPaymentRequest.IsProductionSystem);

			var universalTransactionFromPayload = Encoding.UTF8.GetString(Convert.FromBase64String(ePayment.Payload));
			AssertEquals("Universal Transaction", expectedUniversalTransaction, universalTransactionFromPayload);
		}
		#endregion

		class AccEPaymentQuoteToGEPConverterThatReturnsNullUser : AccEPaymentQuoteToGEPConverter
		{
			protected override GlbStaff GetCreatingUser(AccEPaymentQuote quote)
			{
				quote.QU_SystemCreateUser = "123";
				return base.GetCreatingUser(quote);
			}
		}
	}
}
