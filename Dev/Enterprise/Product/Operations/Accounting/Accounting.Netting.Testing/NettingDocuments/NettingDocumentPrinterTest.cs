using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingDocumentPrinter))]
	public class NettingDocumentPrinterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPrintingTrialStatements()
		{
			SetupParticipantsStatements();

			var documentPrinter = new NettingDocumentPrinter(Factory);
			documentPrinter.PrintAllTrialParticipantStatements();

			AssertNotNull(documentPrinter.PrintTask_ForTestOnly);

			var documentPacks = documentPrinter.PrintTask_ForTestOnly.GetDocumentPacks();

			AssertEquals("2 document packs are created", 2, documentPacks.Count());

			documentPrinter.PrintTask_ForTestOnly = null;
			documentPrinter.PrintTrialParticipantStatement("CM3");

			AssertNotNull(documentPrinter.PrintTask_ForTestOnly);

			documentPacks = documentPrinter.PrintTask_ForTestOnly.GetDocumentPacks();

			AssertEquals("Only 1 document pack for the selected participant is created", 1, documentPacks.Count());
		}

		public void TestPrintingTrialDetailedStatements()
		{
			SetupParticipantsStatements();

			var documentPrinter = new NettingDocumentPrinter(Factory);
			documentPrinter.PrintAllTrialDetailedParticipantStatements();

			AssertNotNull(documentPrinter.PrintTask_ForTestOnly);

			var documentPacks = documentPrinter.PrintTask_ForTestOnly.GetDocumentPacks();

			AssertEquals("2 document packs are created", 2, documentPacks.Count());
		}

		public void TestPrintingFinalStatements()
		{
			SetupParticipantsStatements("NET");

			TestObjectCreator.GenerateCalculationRecords(period.PK.ToGuid());

			period.NSP_IsComplete = true;

			Factory.Save();

			var documentPrinter = new NettingDocumentPrinter(Factory, period.PK);
			documentPrinter.PrintAllFinalParticipantStatements();

			AssertNotNull(documentPrinter.PrintTask_ForTestOnly);

			var documentPacks = documentPrinter.PrintTask_ForTestOnly.GetDocumentPacks();

			AssertEquals("2 document packs are created", 2, documentPacks.Count());

			documentPrinter.PrintTask_ForTestOnly = null;
			documentPrinter.PrintParticipantStatement("CM4");

			AssertNotNull(documentPrinter.PrintTask_ForTestOnly);

			documentPacks = documentPrinter.PrintTask_ForTestOnly.GetDocumentPacks();

			AssertEquals("Only 1 document pack for the selected participant is created", 1, documentPacks.Count());
		}

		void SetupParticipantsStatements(string rateType = "IND")
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			nettingSystem = TestObjectCreator.CreateNettingSystem("NS", "Test Netting System", GlbCompany.CurrentCompany);
			period = CreatePeriod(nettingSystem);

			var org1 = testObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			var org2 = testObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");

			testObjectCreator.CreateNewCompany("CM3", org1);
			testObjectCreator.CreateNewCompany("CM4", org2);

			var participant1 = TestObjectCreator.CreateNettingOrganisation(nettingSystem, org1, "FUL");
			var participant2 = TestObjectCreator.CreateNettingOrganisation(nettingSystem, org2, "FUL");

			TestObjectCreator.CreateNettingExchangeRate(nettingSystem, "GBP", rateType, 0.50, period);
			TestObjectCreator.CreateNettingExchangeRate(nettingSystem, "USD", rateType, 0.75, period);

			SetupParticipant(participant1, "FUL", "GBP", "GBP", "GBP");
			SetupParticipant(participant2, "FUL", "USD", "USD", "USD");

			testObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "1to2", "GBP", 100M);

			Factory.Save();
		}

		public void TestPrintingCallFinaliseNettingWithoutExchangeRates()
		{
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			nettingSystem = TestObjectCreator.CreateNettingSystem("NS", "Test Netting System", GlbCompany.CurrentCompany);
			period = CreatePeriod(nettingSystem);
			CreateNextPeriod(nettingSystem);

			var org1 = TestObjectCreator.CreateNettingOrganisation(nettingSystem, TestObjectCreator.ABIGAS, "FUL");
			SetupParticipant(org1, "FUL", "GBP", "GBP", "GBP");
			var org2 = TestObjectCreator.CreateNettingOrganisation(nettingSystem, TestObjectCreator.AALSHI, "FUL");
			SetupParticipant(org2, "FUL", "USD", "USD", "USD");

			testObjectCreator.CreateFullMatchingNettingTransaction(period, org1, org2, "1to2", "GBP", 100M);

			Factory.Save();

			var documentPrinter = new NettingDocumentPrinter(Factory, period.PK);
			var message = documentPrinter.CallFinaliseNettingCycle();

			var expectedErrorMessage = @"Execution exchange rate is missing for the following currency(s):
GBP, USD";
			AssertEquals(expectedErrorMessage, message);
		}

		[ExpectNoExceptions("Expect no critical validation exception should be thrown")]
		public void TestCallFinaliseNettingWithoutControlAccountBeingSet()
		{
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			nettingSystem = TestObjectCreator.CreateNettingSystem("NS", "Test Netting System", GlbCompany.CurrentCompany);
			period = CreatePeriod(nettingSystem);

			CreateNextPeriod(nettingSystem);

			var org1 = TestObjectCreator.CreateNettingOrganisation(nettingSystem, TestObjectCreator.ABIGAS, "FUL");
			SetupParticipant(org1, "FUL", "AUD", "AUD", "AUD");
			var org2 = TestObjectCreator.CreateNettingOrganisation(nettingSystem, TestObjectCreator.AALSHI, "FUL");
			SetupParticipant(org2, "FUL", "AUD", "AUD", "AUD");

			testObjectCreator.CreateFullMatchingNettingTransaction(period, org1, org2, "1to2", "AUD", 100M);

			Factory.Save();

			var documentPrinter = new NettingDocumentPrinter(Factory, period.PK);
			var message = documentPrinter.CallFinaliseNettingCycle();
			var expectedMessage = @"Please set up Netting Control Account from the following Registry Item: Accounting -> Netting -> Netting Clearing Account";
			AssertEquals(expectedMessage, message);

			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			message = documentPrinter.CallFinaliseNettingCycle();
			expectedMessage = "";
			AssertEquals(expectedMessage, message);

			AssertEquals("Cycle set as complete", true, period.NSP_IsComplete);

			ReleaseFactory();

			var period_Reloaded = Factory.Load<NettingSystemPeriod>(period.PK);

			AssertEquals("Confirm Cycle set as complete", true, period_Reloaded.NSP_IsComplete);
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem)
		{
			period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = "201504";
			period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			period.NSP_ValueDate = ZDate.Today.AddDays(12);
			period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			return period;
		}

		void CreateNextPeriod(NettingSystem nettingSystem)
		{
			var nextPeriod = Factory.New<NettingSystemPeriod>();
			nextPeriod.NSP_Period = "201505";
			nextPeriod.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(23);
			nextPeriod.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(45);
			nextPeriod.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(37);

			nextPeriod.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(35);
			nextPeriod.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(38);
			nextPeriod.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(40);
			nextPeriod.NSP_ValueDate = ZDate.Today.AddDays(42);
			nextPeriod.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(42);

			nextPeriod.NSP_NS_NettingSystem = nettingSystem.PK;
		}

		void SetupParticipant(NettingOrganisation participant, ZString nettingType, ZString reportingCurrency, ZString arSettlementCurrency, ZString apSettlementCurrency)
		{
			participant.NSO_NettingType = nettingType;
			participant.NSO_RX_NKReportingCurrency = reportingCurrency;
			participant.NSO_RX_NKARSettlementCurrency = arSettlementCurrency;
			participant.NSO_RX_NKAPSettlementCurrency = apSettlementCurrency;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var participantStatementMenuItem = Factory.New<StmMenuItem>();
			participantStatementMenuItem.SU_MenuName = "Participant Statement";
			participantStatementMenuItem.SU_BusinessContext = "ParticipantStmnt";
			participantStatementMenuItem.SU_IsPublished = true;
			participantStatementMenuItem.SU_MenuPath = "";
			participantStatementMenuItem.SU_ContactType = "NPS";

			var trialParticipantStatementMenuItem = Factory.New<StmMenuItem>();
			trialParticipantStatementMenuItem.SU_MenuName = "Trial Participant Statement";
			trialParticipantStatementMenuItem.SU_BusinessContext = "ParticipantStmnt";
			trialParticipantStatementMenuItem.SU_IsPublished = true;
			trialParticipantStatementMenuItem.SU_MenuPath = "";
			trialParticipantStatementMenuItem.SU_ContactType = "NPS";

			var company1 = TestObjectCreator.CreateNewCompany("CM1", TestObjectCreator.ABIGAS);
			var company2 = TestObjectCreator.CreateNewCompany("CM2", TestObjectCreator.AALSHI);

			Factory.Save();
		}

		NettingObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator testObjectCreator;
		NettingSystem nettingSystem;
		NettingSystemPeriod period;
	}
}
