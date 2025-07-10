using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class GlobalElectronicPaymentEDIInterchangeCreatorTest : TestCaseWithFactory
	{
		[TestDate(2021, 4, 9)]
		public void TestCreateInterchangeAndDeliver_BeneficiaryRequest()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";

			Factory.Save();

			AccEPaymentBeneficiaryRequest request;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				request = objectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(EPaymentStatusCodes.BeneficiaryRequest.Queued);
				Factory.Save();
			}

			var ediMessageFactory = new BusinessObjectFactory();
			using (ediMessageFactory.AddDisposableService())
			{
				var ePayment = new AccEPaymentBeneficiaryRequestToGEPConverter().ConvertBeneficiaryRequestToGEP(request);
				var notification = new NotificationsForTest();

				var interchangeCreator = new GlobalElectronicPaymentEDIInterchangeCreator();
				interchangeCreator.CreateInterchangeAndDeliver(ediMessageFactory, request, ePayment, notification);
				ediMessageFactory.Save();
			}

			var expectedPayload = @"{""requestReference"":""" + request.ABR_InternalReference + @""",""bankAccountCode"":""EPA"",""updatedDateFrom"":"""",""startPageNumber"":1,""maxNumberOfRecordInHttpResponse"":100,""maxNumberOfRecordInXUE"":500}";

			var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges Created", 1, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				TestHelper.AssertEDIInterchangeForBeneficiaryRequest(interchange,
					new List<EPaymentTestHelper.GEPMessageWithJSONPayload>()
					{
						new EPaymentTestHelper.GEPMessageWithJSONPayload(company1.GC_Code, company1.FirstActiveBranch.GB_Code, expectedPayload, Env.CurrentBranchPK)
					});
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestCreateInterchangeAndDeliver_Deal()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = objectCreator.CreateStaff("YOU");
			var ofxBankAccount = testHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			var staffToken = testHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			EPaymentQuote quote;
			EPaymentDeal deal;
			GlobalElectronicPayment ePayment;
			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1, ofxBankAccount);
				quote.QU_Status = QuoteStatusCodes.Accepted;
				quote.QU_ProviderReference = "testreference";
				quote.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quote.QU_FromAmount = 400m;
				quote.QU_ExchangeRate = 2.5m;
				quote.QU_ExchangeRateInverted = 0.4m;
				quote.QU_FeeAmount = 10m;
				quote.QU_RX_NKFeeCurrency = quote.QU_RX_NKToCurrency;

				deal = testHelper.CreateDeal(quote);

				var paymentApproval = Factory.Load<PaymentApprovalBase>(quote.QU_AV);
				paymentApproval.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				var beneficiary = objectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
				var accountDetails = paymentApproval.PayeeOrganisation.CompanyData.AccountDetailsCollection;
				accountDetails.RemoveAndDeleteAll();
				var accountDetail1 = accountDetails.AddNew();
				accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				accountDetail1.A1_IsDefaultAccount = true;
				accountDetail1.A1_EPaymentBeneficiaryId = beneficiary.PK;

				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					ePayment = new AccEPaymentDealToGEPConverter().ConvertDealToGEP(deal);
				}
			}

			var ediMessageFactory = new BusinessObjectFactory();
			using (ediMessageFactory.AddDisposableService())
			{
				var notification = new NotificationsForTest();
				var interchangeCreator = new GlobalElectronicPaymentEDIInterchangeCreator();
				interchangeCreator.CreateInterchangeAndDeliver(ediMessageFactory, deal, ePayment, notification);
				ediMessageFactory.Save();
			}

			var expectedPayload = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""EPA"",""minFundAmount"":400,""maxFundAmount"":400,""fundCurrency"":""AUD"",""payAmount"":1000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges Created", 1, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				TestHelper.AssertEDIInterchangeForDeal(interchange,
					new List<EPaymentTestHelper.GEPMessageWithJSONPayload>()
					{
						new EPaymentTestHelper.GEPMessageWithJSONPayload(company1.GC_Code, company1.FirstActiveBranch.GB_Code, expectedPayload, Env.CurrentBranchPK)
					});
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestCreateInterchangeAndDeliver_Quote()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1);
				Factory.Save();
			}

			var ediMessageFactory = new BusinessObjectFactory();
			using (ediMessageFactory.AddDisposableService())
			{
				var ePayment = new AccEPaymentQuoteToGEPConverter().ConvertQuoteToGEP(quote);
				var notification = new NotificationsForTest();

				var interchangeCreator = new GlobalElectronicPaymentEDIInterchangeCreator();
				interchangeCreator.CreateInterchangeAndDeliver(ediMessageFactory, quote, ePayment, notification);
				ediMessageFactory.Save();
			}

			var universalTransactionForQuote = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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
    <BankAccount>ZHSBCUSD</BankAccount>
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

			var interchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges Created", 1, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				TestHelper.AssertEDIInterchangeForQuote(interchange,
					new List<EPaymentTestHelper.GEPMessageWithUniversalTransactionPayload>()
					{
						new EPaymentTestHelper.GEPMessageWithUniversalTransactionPayload(company1.GC_Code, company1.FirstActiveBranch.GB_Code, universalTransactionForQuote, Env.CurrentBranchPK)
					});
			}
		}

		#region Implementation

		EPaymentTestHelper TestHelper => testHelper ?? (testHelper = new EPaymentTestHelper(TestObjectCreator));
		EPaymentTestHelper testHelper;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion

		class NotificationsForTest : INotifications
		{
			public IList<INotification> Notifications { get; } = new List<INotification>();

			public void Add(INotification notification)
			{
				Notifications.Add(notification);
			}
		}
	}
}
