using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using BeneficiaryRequestStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Testing
{
	[TestedType(typeof(GlobalElectronicPaymentProcessingServiceTask))]
	public class GlobalElectronicPaymentProcessingServiceTaskTest : ServiceTaskTestCase<GlobalElectronicPaymentProcessingServiceTask>
	{
		#region Beneficiary Request

		public void TestEDIInterchangeIsNotCreatedWhenNoQueuedBeneficiaryRequestsExist()
		{
			var requestsInDb = Factory.Load<AccEPaymentBeneficiaryRequest>(new ZQuery());
			AssertEquals("No Beneficiary Requests in database", 0, requestsInDb.Length);

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var serviceTask = new GlobalElectronicPaymentProcessingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
			}

			var log = logger.ToString();
			AssertContains("Debug|Global Electronic Payment Processing service task started.", log);
			AssertContains($"Debug|Started processing quotes for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|Global Electronic Payment Processing service task completed.", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("No Interchange Created", 0, interchanges.Length);
		}

		[TestDate(2021, 4, 9)]
		public void TestSuccessfulEDIInterchangeForBeneficiaryRequest()
		{
			AccEPaymentBeneficiaryRequest requestInCompany1;
			AccEPaymentBeneficiaryRequest requestInCompany2;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var ofxBankAccountInCompany1 = TestHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			ofxBankAccountInCompany1.AB_Code = "EP1";
			var staffTokenInCompany1 = TestHelper.CreateStaffToken(ofxBankAccountInCompany1.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffTokenInCompany1.TK_AccountName = "This name is provided by OFX.";
			var company2 = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			company2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			var ofxBankAccountInCompany2 = TestHelper.CreateOFXPaymentProviderBankAccount(company2.PK);
			ofxBankAccountInCompany2.AB_Code = "EP2";
			var staffTokenInCompany2 = TestHelper.CreateStaffToken(ofxBankAccountInCompany2.PK, ZDateTime.UtcNow.AddHours(2), company2.PK, Env.CurrentUser.Initials, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffTokenInCompany2.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				requestInCompany1 = TestObjectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(BeneficiaryRequestStatusCodes.Queued);
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				requestInCompany2 = TestObjectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(BeneficiaryRequestStatusCodes.Queued);
				Factory.Save();
			}

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company1.PK.ToGuid(), true))
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company2.PK.ToGuid(), true))
			{
				var serviceTask = new GlobalElectronicPaymentProcessingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);

				using (Env.Instance.TemporaryServiceTaskContext(GlobalElectronicPaymentProcessingServiceTask.Code, canRunInAnyBranch: true))
				{
					serviceTask.RunTask();
				}
			}

			var log = logger.ToString();

			AssertContains("Debug|Global Electronic Payment Processing service task started.", log);

			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary request {requestInCompany1.ABR_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains("Debug|Finished EDI message creation.", log);
			AssertContains("Debug|Attempting to save all changes in database.", log);
			AssertContains("Debug|Successfully saved. Processing is complete.", log);
			AssertContains($"Information|Finished processing beneficiary request {requestInCompany1.ABR_InternalReference}.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			AssertContains($"Debug|Started processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary request {requestInCompany2.ABR_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains("Debug|Finished EDI message creation.", log);
			AssertContains("Debug|Attempting to save all changes in database.", log);
			AssertContains("Debug|Successfully saved. Processing is complete.", log);
			AssertContains($"Information|Finished processing beneficiary request {requestInCompany2.ABR_InternalReference}.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			AssertContains("Debug|Global Electronic Payment Processing service task completed.", log);

			var newFactory = new BusinessObjectFactory();
			using (newFactory.AddDisposableService())
			{
				var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("Interchanges Created", 2, interchanges.Length);

				var expectedPayload1 = @"{""requestReference"":""" + requestInCompany1.ABR_InternalReference + @""",""bankAccountCode"":""EP1"",""updatedDateFrom"":"""",""startPageNumber"":1,""maxNumberOfRecordInHttpResponse"":100,""maxNumberOfRecordInXUE"":500}";

				var expectedPayload2 = @"{""requestReference"":""" + requestInCompany2.ABR_InternalReference + @""",""bankAccountCode"":""EP2"",""updatedDateFrom"":"""",""startPageNumber"":1,""maxNumberOfRecordInHttpResponse"":100,""maxNumberOfRecordInXUE"":500}";

				foreach (var interchange in interchanges)
				{
					TestHelper.AssertEDIInterchangeForBeneficiaryRequest(interchange,
						new List<EPaymentTestHelper.GEPMessageWithJSONPayload>()
						{
							new EPaymentTestHelper.GEPMessageWithJSONPayload(company1.GC_Code, company1.FirstActiveBranch.GB_Code, expectedPayload1, company1.Branches[0].PK),
							new EPaymentTestHelper.GEPMessageWithJSONPayload(company2.GC_Code, company2.FirstActiveBranch.GB_Code, expectedPayload2, company2.Branches[0].PK),
						});
				}
			}
		}

		#endregion

		#region Deal

		public void TestEDIInterchangeIsNotCreatedWhenNoQueuedDealsExist()
		{
			var dealsInDb = Factory.Load<EPaymentDeal>(new ZQuery());
			AssertEquals("No Deals in database", 0, dealsInDb.Length);

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var serviceTask = new GlobalElectronicPaymentProcessingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
			}

			var log = logger.ToString();
			AssertContains("Debug|Global Electronic Payment Processing service task started.", log);
			AssertContains($"Debug|Started processing quotes for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|Global Electronic Payment Processing service task completed.", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("No Interchange Created", 0, interchanges.Length);
		}

		[TestDate(2021, 4, 9)]
		public void TestSuccessfulEDIInterchangeForCreateADeal()
		{
			EPaymentQuote quoteInCompany1;
			EPaymentQuote quoteInCompany2;

			EPaymentDeal dealInCompany1;
			EPaymentDeal dealInCompany2;

			var quoteCreatingUser = TestObjectCreator.CreateStaff("YOU");
			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var ofxBankAccount1 = TestHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			ofxBankAccount1.AB_Code = "OFX1";
			var staffToken1 = TestHelper.CreateStaffToken(ofxBankAccount1.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken1.TK_AccountName = "This name is provided by OFX.";
			var company2 = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			company2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			var ofxBankAccount2 = TestHelper.CreateOFXPaymentProviderBankAccount(company2.PK);
			ofxBankAccount2.AB_Code = "OFX2";
			var staffToken2 = TestHelper.CreateStaffToken(ofxBankAccount2.PK, ZDateTime.UtcNow.AddHours(2), company2.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
			staffToken2.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany1 = TestHelper.CreateQuote(company1, ofxBankAccount1);
				quoteInCompany1.QU_Status = QuoteStatusCodes.Accepted;
				quoteInCompany1.QU_ProviderReference = "testreference";
				quoteInCompany1.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quoteInCompany1.QU_FromAmount = 400m;
				quoteInCompany1.QU_ExchangeRate = 2.5m;
				quoteInCompany1.QU_ExchangeRateInverted = 0.4m;
				quoteInCompany1.QU_FeeAmount = 10m;
				quoteInCompany1.QU_RX_NKFeeCurrency = quoteInCompany1.QU_RX_NKToCurrency;

				dealInCompany1 = testHelper.CreateDeal(quoteInCompany1);

				var paymentApproval1 = Factory.Load<PaymentApprovalBase>(quoteInCompany1.QU_AV);
				paymentApproval1.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				var beneficiary1 = TestObjectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
				var accountDetails1 = paymentApproval1.PayeeOrganisation.CompanyData.AccountDetailsCollection;
				accountDetails1.RemoveAndDeleteAll();
				var accountDetail1 = accountDetails1.AddNew();
				accountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				accountDetail1.A1_IsDefaultAccount = true;
				accountDetail1.A1_EPaymentBeneficiaryId = beneficiary1.PK;

				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany2 = TestHelper.CreateQuote(company2, ofxBankAccount2);
				quoteInCompany2.QU_Status = QuoteStatusCodes.Accepted;
				quoteInCompany2.QU_ProviderReference = "testreference";
				quoteInCompany2.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quoteInCompany2.QU_FromAmount = 400m;
				quoteInCompany2.QU_ExchangeRate = 2.5m;
				quoteInCompany2.QU_ExchangeRateInverted = 0.4m;
				quoteInCompany2.QU_FeeAmount = 10m;
				quoteInCompany2.QU_RX_NKFeeCurrency = quoteInCompany2.QU_RX_NKToCurrency;

				dealInCompany2 = testHelper.CreateDeal(quoteInCompany2);

				var paymentApproval2 = Factory.Load<PaymentApprovalBase>(quoteInCompany2.QU_AV);
				paymentApproval2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.ServicesTrade;
				var beneficiary2 = TestObjectCreator.CreateEPaymentBeneficiary("f94f9527-8160-4ea4-8a4f-9be892c81dc4");
				var accountDetails2 = paymentApproval2.PayeeOrganisation.CompanyData.AccountDetailsCollection;
				accountDetails2.RemoveAndDeleteAll();
				var accountDetail2 = accountDetails2.AddNew();
				accountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				accountDetail2.A1_IsDefaultAccount = true;
				accountDetail2.A1_EPaymentBeneficiaryId = beneficiary2.PK;

				Factory.Save();
			}

			var configForCompany1 = new ExchangeRateToleranceConfiguration();
			configForCompany1.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForUSDCompany1 = new ExchangeRateTolerance
			{
				Currency = "USD",
				ExchangeRateTolerancePercentage = 5
			};
			configForCompany1.ExchangeRateToleranceCollection.Add(toleranceForUSDCompany1);

			var configForCompany2 = new ExchangeRateToleranceConfiguration();
			configForCompany2.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			var toleranceForUSDCompany2 = new ExchangeRateTolerance
			{
				Currency = "USD",
				ExchangeRateTolerancePercentage = 10
			};
			configForCompany2.ExchangeRateToleranceCollection.Add(toleranceForUSDCompany2);

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company1.PK.ToGuid(), true))
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company2.PK.ToGuid(), true))
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCompany1))
			using (AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, configForCompany2))
			using (AccountingMasterFilesRegistry.Instance.RebookExpiredQuotesBasedOnExRateTolerance.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var serviceTask = new GlobalElectronicPaymentProcessingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
			}

			var log = logger.ToString();

			AssertContains("Debug|Global Electronic Payment Processing service task started.", log);

			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deal {dealInCompany1.AED_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains("Debug|Finished EDI message creation.", log);
			AssertContains("Debug|Attempting to save all changes in database.", log);
			AssertContains("Debug|Successfully saved. Processing is complete.", log);
			AssertContains($"Information|Finished processing deal {dealInCompany1.AED_InternalReference}.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			AssertContains($"Debug|Started processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deal {dealInCompany2.AED_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains("Debug|Finished EDI message creation.", log);
			AssertContains("Debug|Attempting to save all changes in database.", log);
			AssertContains("Debug|Successfully saved. Processing is complete.", log);
			AssertContains($"Information|Finished processing deal {dealInCompany2.AED_InternalReference}.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			AssertContains("Debug|Global Electronic Payment Processing service task completed.", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges Created", 2, interchanges.Length);

			var expectedPayload1 = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""OFX1"",""minFundAmount"":380,""maxFundAmount"":420,""fundCurrency"":""AUD"",""payAmount"":1000.0000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000.0000,""payReason"":""Services trade"",""payReference"":""""}]}";

			var expectedPayload2 = @"{""dealInternalReference"":""00001000"",""quoteProviderReference"":""testreference"",""bankAccountCode"":""OFX2"",""minFundAmount"":360,""maxFundAmount"":440,""fundCurrency"":""NZD"",""payAmount"":1000.0000,""payCurrency"":""USD"",""paymentItems"":[{""payeeId"":""f94f9527-8160-4ea4-8a4f-9be892c81dc4"",""amount"":1000.0000,""payReason"":""Services trade"",""payReference"":""""}]}";

			foreach (var interchange in interchanges)
			{
				TestHelper.AssertEDIInterchangeForDeal(interchange,
					new List<EPaymentTestHelper.GEPMessageWithJSONPayload>()
					{
						new EPaymentTestHelper.GEPMessageWithJSONPayload(company1.GC_Code, company1.FirstActiveBranch.GB_Code, expectedPayload1, company1.Branches[0].PK),
						new EPaymentTestHelper.GEPMessageWithJSONPayload(company2.GC_Code, company2.FirstActiveBranch.GB_Code, expectedPayload2, company2.Branches[0].PK),
					});
			}

			var dealInCompany1InNewFactory = newFactory.Load<EPaymentDeal>(dealInCompany1.PK);
			AssertEquals(QuoteStatusCodes.Requested, dealInCompany1InNewFactory.AED_Status);

			var dealInCompany2InNewFactory = newFactory.Load<EPaymentDeal>(dealInCompany2.PK);
			AssertEquals(QuoteStatusCodes.Requested, dealInCompany2InNewFactory.AED_Status);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNotNull("Data Export Event linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == $"Purpose: E-Payment Deal {dealInCompany1.AED_InternalReference} submitted for processing to OFX"));

			var paymentApprovalInCompany2InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany2.QU_AV);
			AssertNotNull("Data Export Event linked to payment approval", paymentApprovalInCompany2InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == $"Purpose: E-Payment Deal {dealInCompany2.AED_InternalReference} submitted for processing to OFX"));
		}

		#endregion

		#region Quote

		public void TestEDIInterchangeIsNotCreatedWhenNoQueuedQuotesExist()
		{
			var quotesInDb = Factory.Load<EPaymentQuote>(new ZQuery());
			AssertEquals("No Quotes in database", 0, quotesInDb.Length);

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var serviceTask = new GlobalElectronicPaymentProcessingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
			}

			var log = logger.ToString();
			AssertContains("Debug|Global Electronic Payment Processing service task started.", log);
			AssertContains($"Debug|Started processing quotes for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{Env.CurrentCompany.Code}].", log);
			AssertContains("Debug|Global Electronic Payment Processing service task completed.", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("No Interchange Created", 0, interchanges.Length);
		}

		[TestDate(2021, 4, 9)]
		public void TestSuccessfulEDIInterchangeForGetRate()
		{
			EPaymentQuote quoteInCompany1;
			EPaymentQuote quoteInCompany2;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			var company2 = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany1 = TestHelper.CreateQuote(company1);
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany2 = TestHelper.CreateQuote(company2);
				Factory.Save();
			}

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company1.PK.ToGuid(), true))
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company2.PK.ToGuid(), true))
			{
				var serviceTask = new GlobalElectronicPaymentProcessingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
			}
			var log = logger.ToString();

			AssertContains("Debug|Global Electronic Payment Processing service task started.", log);

			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing quote {quoteInCompany1.QU_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains("Debug|Finished EDI message creation.", log);
			AssertContains("Debug|Attempting to save all changes in database.", log);
			AssertContains("Debug|Successfully saved. Processing is complete.", log);
			AssertContains($"Information|Finished processing quote {quoteInCompany1.QU_InternalReference}.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			AssertContains($"Debug|Started processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing quote {quoteInCompany2.QU_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains("Debug|Finished EDI message creation.", log);
			AssertContains("Debug|Attempting to save all changes in database.", log);
			AssertContains("Debug|Successfully saved. Processing is complete.", log);
			AssertContains($"Information|Finished processing quote {quoteInCompany2.QU_InternalReference}.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			AssertContains("Debug|Global Electronic Payment Processing service task completed.", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges Created", 2, interchanges.Length);

			var universalTransactionForQuoteInCompany1 = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
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
    <LocalTotal>0.0000</LocalTotal>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSTotal>1000.0000</OSTotal>
    <PaymentOrReceiptType>EPA</PaymentOrReceiptType>
    <PostDate>2021-04-08T00:00:00</PostDate>
    <TransactionDate>2021-04-10T00:00:00</TransactionDate>
    <TransactionReference>00001000</TransactionReference>
    <TransactionType>PAY</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";

			var universalTransactionForQuoteInCompany2 = @"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccEPaymentQuote</Type>
          <Key>00001000</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>CNZ</Code>
        <Country>
          <Code>NZ</Code>
          <Name>New Zealand</Name>
        </Country>
        <Name>Company</Name>
      </Company>
      <DataProvider>EDIDATCNZ</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <BankAccount>ZHSBCUSD</BankAccount>
    <Branch>
      <Code>BNZ</Code>
      <Name></Name>
    </Branch>
    <CheckNumberOrPaymentRef>00009283</CheckNumberOrPaymentRef>
    <Description>Paying FreightQuota Invoice 83942</Description>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </LocalCurrency>
    <LocalTotal>0.0000</LocalTotal>
    <OSCurrency>
      <Code>USD</Code>
      <Description>United States Dollar</Description>
    </OSCurrency>
    <OSTotal>1000.0000</OSTotal>
    <PaymentOrReceiptType>EPA</PaymentOrReceiptType>
    <PostDate>2021-04-08T00:00:00</PostDate>
    <TransactionDate>2021-04-10T00:00:00</TransactionDate>
    <TransactionReference>00001000</TransactionReference>
    <TransactionType>PAY</TransactionType>
  </TransactionInfo>
</UniversalTransaction>";

			foreach (var interchange in interchanges)
			{
				TestHelper.AssertEDIInterchangeForQuote(interchange,
					new List<EPaymentTestHelper.GEPMessageWithUniversalTransactionPayload>()
					{
						new EPaymentTestHelper.GEPMessageWithUniversalTransactionPayload(company1.GC_Code, company1.FirstActiveBranch.GB_Code, universalTransactionForQuoteInCompany1, company1.Branches[0].PK),
						new EPaymentTestHelper.GEPMessageWithUniversalTransactionPayload(company2.GC_Code, company2.FirstActiveBranch.GB_Code, universalTransactionForQuoteInCompany2, company2.Branches[0].PK),
					});
			}

			var quoteInCompany1InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany1.PK);
			AssertEquals(QuoteStatusCodes.Requested, quoteInCompany1InNewFactory.QU_Status);

			var quoteInCompany2InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany2.PK);
			AssertEquals(QuoteStatusCodes.Requested, quoteInCompany2InNewFactory.QU_Status);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNotNull("Data Export Event linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));

			var paymentApprovalInCompany2InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany2.QU_AV);
			AssertNotNull("Data Export Event linked to payment approval", paymentApprovalInCompany2InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));
		}

		#endregion

		public void TestInitialiseTask()
		{
			AssertEquals("15minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestSerivceTaskIsMandatoryAndScheduleReadOnly()
		{
			var attribute = GetHostedServiceAttributes().SingleOrDefault();
			AssertNotNull(attribute);
			Assert($"{attribute.Code} service task must be mandatory", attribute.IsMandatory);
			Assert($"{attribute.Code} service task schedule must be readonly", attribute.IsScheduleReadOnly);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEPaymentQuoteSchema.Constants.TableName,
						"Global Electronic Payment Quotes Queuing",
						AccEPaymentQuoteSchema.Constants.QU_Status + "=" + EPaymentStatusCodes.Quote.Queued),

					new TaskNudgeInformationForTest(
						AccEPaymentDealSchema.Constants.TableName,
						"Global Electronic Payment Deals Queuing",
						AccEPaymentDealSchema.Constants.AED_Status + "=" + EPaymentStatusCodes.Deal.Queued),

					new TaskNudgeInformationForTest(
						AccEPaymentBeneficiaryRequestSchema.Constants.TableName,
						"Global Electronic Payment Beneficiary Request Queuing",
						AccEPaymentBeneficiaryRequestSchema.Constants.ABR_Status + "=" + EPaymentStatusCodes.BeneficiaryRequest.Queued),
				};
			}
		}

		#region Implementation

		EPaymentTestHelper TestHelper => testHelper ?? (testHelper = new EPaymentTestHelper(TestObjectCreator));
		EPaymentTestHelper testHelper;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
