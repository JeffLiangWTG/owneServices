using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Accounting.ElectronicPayment.Common.GlobalElectronicPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using BeneficiaryRequestStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class GlobalElectronicPaymentProcessorTest : TestCaseWithFactory
	{
		public void TestEPaymentsProcessingDependsOnEnableEPaymentFunctionalityRegistry()
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
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company2.PK.ToGuid(), false))
			{
				logger = new TestServiceLogger();
				var paymentProcessor = new GlobalElectronicPaymentProcessor();
				paymentProcessor.ProcessEPayments(logger, CancellationToken.None);
			}
			var log = logger.ToString();

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
			AssertNotContains($"Debug|Started processing quotes for Company [{company2.GC_Code}].", log);
			AssertNotContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertNotContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("One Interchange should be created", 1, interchanges.Length);

			var quoteInCompany1InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany1.PK);
			AssertEquals("quoteInCompany1 should be processed because EnableEPaymentFunctionality registry is enabled", QuoteStatusCodes.Requested, quoteInCompany1InNewFactory.QU_Status);

			var quoteInCompany2InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany2.PK);
			AssertEquals("quoteInCompany2 should not be processed because EnableEPaymentFunctionality registry is disabled", QuoteStatusCodes.Queued, quoteInCompany2InNewFactory.QU_Status);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNotNull("Data Export Event linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));

			var paymentApprovalInCompany2InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany2.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany2InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));
		}

		[TestDate(2021, 02, 01)]
		public void TestNotActiveBranchForCompany()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				TestHelper.CreateQuote(company);
				Factory.Save();
			}

			company.Branches.Cast<GlbBranch>().ToList().ForEach(x => x.GB_IsActive = false);
			Factory.Save();
			AssertNull(company.FirstActiveBranch);
			AssertEquals("CAU", company.GC_Code);

			TestServiceLogger logger;
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company.PK.ToGuid(), true))
			{
				logger = new TestServiceLogger();
				var paymentProcessor = new GlobalElectronicPaymentProcessor();
				paymentProcessor.ProcessEPayments(logger, CancellationToken.None);
			}
			var log = logger.ToString();

			AssertNotContains("because the CAU company doesn't have any active branch, there should be no log for this company", "[CAU]", log);
			AssertNotContains("We expect no warning", "Warning|", log);
			AssertNotContains("We expect no error", "Error|", log);

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company.PK.ToGuid(), true))
			{
				logger = new TestServiceLogger();
				var paymentProcessor = new GlobalElectronicPaymentProcessor(Array.Empty<IGEPRequestQueue>(), new[] { company });
				paymentProcessor.ProcessEPayments(logger, CancellationToken.None);
			}
			log = logger.ToString();

			AssertContains("Debug|Cannot find an active branch for company [CAU].", log);
			AssertNotContains("We expect no error", "Error|", log);
		}

		public void TestCompanyBranchBecomeInactiveDuringProcessing()
		{
			var company = TestObjectCreator.CreateNewCompany("CAU");
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BAU";
			branch.GB_RL_NKHomePort = "AUADL";
			branch.GB_IsActive = true;
			Factory.Save();

			var logger = new TestServiceLogger();
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(company.PK.ToGuid(), true))
			{
				var paymentProcessor = new GlobalElectronicPaymentProcessor();

				AssertNoExceptionThrown(
					"When new processor with no cached company data, "
					+ "the first run should not throw due to no active branches for a company.",
					() => paymentProcessor.ProcessEPayments(logger, CancellationToken.None)
				);

				//Simulate data changed outside of this app domain during the processing
				// while company data is already loaded in memory
				Db.Connection.ExecuteNonQuery(
					@"UPDATE dbo.GlbBranch SET GB_IsActive = 0, GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = @GB_PK",
					(cmd) => cmd.AddParameter(
						"@GB_PK",
						System.Data.SqlDbType.UniqueIdentifier,
						branch.PK.ToGuid())
				);

				AssertNoExceptionThrown(
					"When the processor has cached company data and a branch is deactivated in database, "
						+ "it should not throw due to no active branches for a company.",
					() => paymentProcessor.ProcessEPayments(logger, CancellationToken.None)
				);

				AssertNoExceptionThrown(
					"When subsequent new processor is running with no cached company data, "
					+ "it should not throw due to no active branches for a company.",
					() => new GlobalElectronicPaymentProcessor()
						.ProcessEPayments(logger, CancellationToken.None)
				);
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestUnsuccessfulEDIInterchangeForBeneficiaryRequest()
		{
			AccEPaymentBeneficiaryRequest requestInCompany1;
			AccEPaymentBeneficiaryRequest requestInCompany2;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			var company2 = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
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

			var logger = new TestServiceLogger();
			var paymentProcessor = new GlobalElectronicPaymentProcessorWithException();
			paymentProcessor.ProcessEPayments(logger, CancellationToken.None);

			var log = logger.ToString();
			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary request {requestInCompany1.ABR_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process beneficiary request {requestInCompany1.ABR_InternalReference} of company [{company1.GC_Code}].
 Error : [OFX User account information is missing.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.Common.AccEPaymentBeneficiaryRequestToGEPConverter.ConvertBeneficiaryRequestToGEP", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			AssertContains($"Debug|Started processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary request {requestInCompany2.ABR_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process beneficiary request {requestInCompany2.ABR_InternalReference} of company [{company2.GC_Code}].
 Error : [OFX User account information is missing.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.Common.AccEPaymentBeneficiaryRequestToGEPConverter.ConvertBeneficiaryRequestToGEP", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges not created", 0, interchanges.Length);

			var requestInCompany1InNewFactory = newFactory.Load<AccEPaymentBeneficiaryRequest>(requestInCompany1.PK);
			AssertEquals(BeneficiaryRequestStatusCodes.Error, requestInCompany1InNewFactory.ABR_Status);
			AssertEquals(ZDateTime.UtcNow, requestInCompany1InNewFactory.ABR_LastResponseReceivedUtc);
			AssertEquals("OFX User account information is missing.", requestInCompany1InNewFactory.ABR_ErrorDescription);

			var requestInCompany2InNewFactory = newFactory.Load<AccEPaymentBeneficiaryRequest>(requestInCompany2.PK);
			AssertEquals(BeneficiaryRequestStatusCodes.Error, requestInCompany2InNewFactory.ABR_Status);
			AssertEquals(ZDateTime.UtcNow, requestInCompany2InNewFactory.ABR_LastResponseReceivedUtc);
			AssertEquals("OFX User account information is missing.", requestInCompany2InNewFactory.ABR_ErrorDescription);
		}

		[TestDate(2021, 4, 9)]
		public void TestZSaveExceptionIsHandled()
		{
			AccEPaymentBeneficiaryRequest requestInCompany1;
			AccEPaymentBeneficiaryRequest requestInCompany2;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			var company2 = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
				var beneficiaryRequestCreatingUser = TestObjectCreator.CreateStaff("YOU");
				var ofxBankAccount = TestHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
				var staffToken = TestHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
				staffToken.TK_AccountName = "OFXUSR1";

				requestInCompany1 = TestObjectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(BeneficiaryRequestStatusCodes.Queued);
				requestInCompany1.ABR_SystemCreateUser = beneficiaryRequestCreatingUser.GS_Code;
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				company2.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
				var beneficiaryRequestCreatingUser = TestObjectCreator.CreateStaff("WE");
				var ofxBankAccount = TestHelper.CreateOFXPaymentProviderBankAccount(company2.PK);
				ofxBankAccount.AB_Code = "BK2";
				var staffToken = TestHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company2.PK, beneficiaryRequestCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);
				staffToken.TK_AccountName = "OFXUSR2";

				requestInCompany2 = TestObjectCreator.CreateValidEPaymentBeneficiaryRequestForStatus(BeneficiaryRequestStatusCodes.Queued);
				requestInCompany2.ABR_SystemCreateUser = beneficiaryRequestCreatingUser.GS_Code;
				Factory.Save();
			}

			var logger = new TestServiceLogger();
			var paymentProcessor = new GlobalElectronicPaymentProcessorWithZSaveException();
			paymentProcessor.ProcessEPayments(logger, CancellationToken.None);

			var log = logger.ToString();
			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary request {requestInCompany1.ABR_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process beneficiary request {requestInCompany1.ABR_InternalReference} of company [{company1.GC_Code}].
 Error : [
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = Could not save

]", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			AssertContains($"Debug|Started processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary request {requestInCompany2.ABR_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process beneficiary request {requestInCompany2.ABR_InternalReference} of company [{company2.GC_Code}].
 Error : [
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = Could not save

]", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges not created", 0, interchanges.Length);

			var requestInCompany1InNewFactory = newFactory.Load<AccEPaymentBeneficiaryRequest>(requestInCompany1.PK);
			AssertEquals(BeneficiaryRequestStatusCodes.Error, requestInCompany1InNewFactory.ABR_Status);
			AssertEquals(ZDateTime.UtcNow, requestInCompany1InNewFactory.ABR_LastResponseReceivedUtc);
			AssertEquals("CargoWise.EntityFramework.ZSaveException occurred while saving changes.", requestInCompany1InNewFactory.ABR_ErrorDescription);

			var requestInCompany2InNewFactory = newFactory.Load<AccEPaymentBeneficiaryRequest>(requestInCompany2.PK);
			AssertEquals(BeneficiaryRequestStatusCodes.Error, requestInCompany2InNewFactory.ABR_Status);
			AssertEquals(ZDateTime.UtcNow, requestInCompany2InNewFactory.ABR_LastResponseReceivedUtc);
			AssertEquals("CargoWise.EntityFramework.ZSaveException occurred while saving changes.", requestInCompany2InNewFactory.ABR_ErrorDescription);
		}

		public void TestUnsuccessfulEDIInterchangeForCreateADeal()
		{
			EPaymentQuote quoteInCompany1;
			EPaymentQuote quoteInCompany2;

			EPaymentDeal dealInCompany1;
			EPaymentDeal dealInCompany2;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = TestObjectCreator.CreateStaff("YOU");
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
				Factory.Save();

				quoteInCompany1.QU_Status = DealStatusCodes.Accepted;
				quoteInCompany1.QU_ProviderReference = "testreference";
				quoteInCompany1.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quoteInCompany1.QU_FromAmount = 400m;
				quoteInCompany1.QU_ExchangeRate = 2.5m;
				quoteInCompany1.QU_ExchangeRateInverted = 0.4m;
				quoteInCompany1.QU_FeeAmount = 10m;
				quoteInCompany1.QU_RX_NKFeeCurrency = quoteInCompany1.QU_RX_NKToCurrency;
				Factory.Save();

				dealInCompany1 = TestHelper.CreateDeal(quoteInCompany1);
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company2.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany2 = TestHelper.CreateQuote(company2, ofxBankAccount2);
				Factory.Save();

				quoteInCompany2.QU_Status = DealStatusCodes.Accepted;
				quoteInCompany2.QU_ProviderReference = "testreference";
				quoteInCompany2.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quoteInCompany2.QU_FromAmount = 400m;
				quoteInCompany2.QU_ExchangeRate = 2.5m;
				quoteInCompany2.QU_ExchangeRateInverted = 0.4m;
				quoteInCompany2.QU_FeeAmount = 10m;
				quoteInCompany2.QU_RX_NKFeeCurrency = quoteInCompany2.QU_RX_NKToCurrency;
				Factory.Save();

				dealInCompany2 = TestHelper.CreateDeal(quoteInCompany2);
				Factory.Save();
			}

			var logger = new TestServiceLogger();
			var paymentProcessor = new GlobalElectronicPaymentProcessorWithException();
			paymentProcessor.ProcessEPayments(logger, CancellationToken.None);

			var log = logger.ToString();

			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deal {dealInCompany1.AED_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process deal {dealInCompany1.AED_InternalReference} of company [{company1.GC_Code}].
 Error : [Something went wrong during EDI Message creation.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.GlobalElectronicPaymentProcessor.CreateEDIMessageAndInterchange", log);
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
			AssertContains($@"Error|Failed to process deal {dealInCompany2.AED_InternalReference} of company [{company2.GC_Code}].
 Error : [Something went wrong during EDI Message creation.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.GlobalElectronicPaymentProcessor.CreateEDIMessageAndInterchange", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges not created", 0, interchanges.Length);

			var dealInCompany1InNewFactory = newFactory.Load<AccEPaymentDeal>(dealInCompany1.PK);
			AssertEquals(DealStatusCodes.SubmissionFailed, dealInCompany1InNewFactory.AED_Status);
			AssertEquals(ErrorDescriptionForTest, dealInCompany1InNewFactory.AED_ErrorDescription);

			var dealInCompany2InNewFactory = newFactory.Load<AccEPaymentDeal>(dealInCompany2.PK);
			AssertEquals(DealStatusCodes.SubmissionFailed, dealInCompany2InNewFactory.AED_Status);
			AssertEquals(ErrorDescriptionForTest, dealInCompany2InNewFactory.AED_ErrorDescription);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == $"Purpose: E-Payment Deal {dealInCompany1.AED_InternalReference} submitted for processing to OFX"));

			var paymentApprovalInCompany2InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany2.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany2InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == $"Purpose: E-Payment Deal {dealInCompany2.AED_InternalReference} submitted for processing to OFX"));
		}

		public void TestUnsuccessfulEDIInterchangeForCreateADealOnUserValidationError()
		{
			EPaymentQuote quoteInCompany1;
			EPaymentDeal dealInCompany1;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = TestObjectCreator.CreateStaff("YOU");
			var ofxBankAccount = TestHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			ofxBankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			var staffToken = TestHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.Empty, company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.NotAuthorised);
			staffToken.TK_AccountName = "This name is provided by OFX.";
			Factory.Save();

			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany1 = TestHelper.CreateQuote(company1, ofxBankAccount);
				quoteInCompany1.QU_Status = DealStatusCodes.Accepted;
				quoteInCompany1.QU_ProviderReference = "testreference";
				quoteInCompany1.QU_LastResponseReceivedUtc = ZDateTime.UtcNow;
				quoteInCompany1.QU_FromAmount = 400m;
				quoteInCompany1.QU_ExchangeRate = 2.5m;
				quoteInCompany1.QU_ExchangeRateInverted = 0.4m;
				quoteInCompany1.QU_FeeAmount = 10m;
				quoteInCompany1.QU_RX_NKFeeCurrency = quoteInCompany1.QU_RX_NKToCurrency;
				Factory.Save();

				dealInCompany1 = TestHelper.CreateDeal(quoteInCompany1);
				Factory.Save();
			}

			var logger = new TestServiceLogger();
			var paymentProcessor = new GlobalElectronicPaymentProcessorWithException();
			paymentProcessor.ProcessEPayments(logger, CancellationToken.None);

			var log = logger.ToString();

			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No quotes were available for processing.", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deal {dealInCompany1.AED_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process deal {dealInCompany1.AED_InternalReference} of company [{company1.GC_Code}].
 Error : [The bank account type is expected to be 'EPA' but was 'BNK'.
No authorized staff token found for code 'YOU'.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.Common.AccEPaymentDealToGEPConverter.CheckUseAuthorisation", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges not created", 0, interchanges.Length);

			var dealInCompany1InNewFactory = newFactory.Load<AccEPaymentDeal>(dealInCompany1.PK);
			AssertEquals(DealStatusCodes.SubmissionFailed, dealInCompany1InNewFactory.AED_Status);
			AssertEquals("Error generating FX Deal Request. Please try again.", dealInCompany1InNewFactory.AED_ErrorDescription);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == $"Purpose: E-Payment Deal {dealInCompany1.AED_InternalReference} submitted for processing to OFX"));
		}

		public void TestUnsuccessfulEDIInterchangeForGetRate()
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

			var logger = new TestServiceLogger();
			var paymentProcessor = new GlobalElectronicPaymentProcessorWithException();
			paymentProcessor.ProcessEPayments(logger, CancellationToken.None);

			var log = logger.ToString();
			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing quote {quoteInCompany1.QU_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process quote 00001000 of company [{company1.GC_Code}].
 Error : [Something went wrong during EDI Message creation.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.GlobalElectronicPaymentProcessor.CreateEDIMessageAndInterchange", log);
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
			AssertContains($@"Error|Failed to process quote 00001000 of company [{company2.GC_Code}].
 Error : [Something went wrong during EDI Message creation.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.GlobalElectronicPaymentProcessor.CreateEDIMessageAndInterchange", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company2.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company2.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company2.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges not created", 0, interchanges.Length);

			var quoteInCompany1InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany1.PK);
			AssertEquals(QuoteStatusCodes.Failed, quoteInCompany1InNewFactory.QU_Status);
			AssertEquals(ErrorDescriptionForTest, quoteInCompany1InNewFactory.QU_ErrorDescription);

			var quoteInCompany2InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany2.PK);
			AssertEquals(QuoteStatusCodes.Failed, quoteInCompany2InNewFactory.QU_Status);
			AssertEquals(ErrorDescriptionForTest, quoteInCompany2InNewFactory.QU_ErrorDescription);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));

			var paymentApprovalInCompany2InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany2.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany2InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));
		}

		public void TestUnsuccessfulEDIInterchangeForGetRateOnUserValidationError()
		{
			EPaymentQuote quoteInCompany1;

			var company1 = TestObjectCreator.CreateCompanyAndBranch("AUMEL");

			company1.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.Australia;
			var quoteCreatingUser = TestObjectCreator.CreateStaff("YOU");
			var ofxBankAccount = TestHelper.CreateOFXPaymentProviderBankAccount(company1.PK);
			TestHelper.CreateStaffToken(ofxBankAccount.PK, ZDateTime.UtcNow.AddHours(2), company1.PK, quoteCreatingUser.GS_Code, AccEPaymentStaffTokenLookups.StatusCodes.Authorised);

			Factory.Save();

			using (Env.SetTemporaryUserContext(quoteCreatingUser.PK.ToGuid(), company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quoteInCompany1 = TestHelper.CreateQuote(company1, ofxBankAccount);
				Factory.Save();
			}

			var logger = new TestServiceLogger();
			var paymentProcessor = new GlobalElectronicPaymentProcessorWithException();
			paymentProcessor.ProcessEPayments(logger, CancellationToken.None);

			var log = logger.ToString();
			AssertContains($"Debug|Started processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing quote {quoteInCompany1.QU_InternalReference}.", log);
			AssertContains("Debug|Starting EDI message creation process.", log);
			AssertContains($@"Error|Failed to process quote {quoteInCompany1.QU_InternalReference} of company [{company1.GC_Code}].
 Error : [The staff token is missing an account name.]
 StackTrace:
    at Enterprise.Accounting.ElectronicPayment.Common.AccEPaymentQuoteToGEPConverter.CheckUserAuthorisation", log);
			AssertContains($"Debug|Finished processing quotes for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing deals for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No deals were available for processing.", log);
			AssertContains($"Debug|Finished processing deals for Company [{company1.GC_Code}].", log);
			AssertContains($"Debug|Started processing beneficiary requests for Company [{company1.GC_Code}].", log);
			AssertContains("Debug|No beneficiary requests were available for processing.", log);
			AssertContains($"Debug|Finished processing beneficiary requests for Company [{company1.GC_Code}].", log);

			var newFactory = new BusinessObjectFactory();
			var interchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("Interchanges not created", 0, interchanges.Length);

			var quoteInCompany1InNewFactory = newFactory.Load<EPaymentQuote>(quoteInCompany1.PK);
			AssertEquals(QuoteStatusCodes.Failed, quoteInCompany1InNewFactory.QU_Status);
			AssertEquals("Error generating FX Quote Request. Please try again.", quoteInCompany1InNewFactory.QU_ErrorDescription);

			var paymentApprovalInCompany1InNewFactory = newFactory.Load<AccPaymentApproval>(quoteInCompany1.QU_AV);
			AssertNull("Data Export Event not linked to payment approval", paymentApprovalInCompany1InNewFactory.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == "DEX" && l.SL_Reference == "Purpose: FX Quote Request 00001000 Sent to OFX"));
		}

		#region Implementation

		EPaymentTestHelper TestHelper => testHelper ?? (testHelper = new EPaymentTestHelper(TestObjectCreator));
		EPaymentTestHelper testHelper;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		static string ErrorDescriptionForTest => "Something went wrong during EDI Message creation.";

		class GlobalElectronicPaymentProcessorWithException : GlobalElectronicPaymentProcessor
		{
			protected override void CreateEDIMessageAndInterchangeCore(BusinessObjectFactory ediInterchangeFactory, IEPaymentDeliveryContextValueProvider deliveryContextValueProvider, GlobalElectronicPayment electronicPayment, Logger notifications)
			{
				notifications.AddMessageError(ErrorDescriptionForTest);
			}
		}

		class GlobalElectronicPaymentProcessorWithZSaveException : GlobalElectronicPaymentProcessor
		{
			protected override void CreateEDIMessageAndInterchangeCore(BusinessObjectFactory ediInterchangeFactory, IEPaymentDeliveryContextValueProvider deliveryContextValueProvider, GlobalElectronicPayment electronicPayment, Logger notifications)
			{
				throw new ZSaveException(new ZDataException(new Exception("Could not save"), null, Db.Connection), ediInterchangeFactory);
			}
		}

		#endregion
	}
}
