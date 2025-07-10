using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Universal.Netting;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal.Netting
{
	[TestedType(typeof(NettingTransactionSubscriber))]
	class NettingTransactionLogWalkerSubscriberTest : LogSubscriberTest<NettingTransactionSubscriber>
	{
		[TestDate(2020, 11, 01)]
		public void TestRelativeCompanyHasNoActiveBranch()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AddNettingCommunicationMode();

			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception", () => RunLogWalkerCycleForTest());

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
			Assert("Didn't report error after finished process logs.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var trans = testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 2000M, NettingTransactionApprovalStatus.Approved);
			Factory.Save();

			foreach (var branch in glbCompany.Branches)
			{
				branch.GB_IsActive = false;
			}
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there are no branchs active for transaction's company", () => RunLogWalkerCycleForTest());

			var logDetail = (from log in NotifiedEventList
							 where log.Contains($@"[NettingTransactionSubscriber] Netting Participant {recipientOrgHeader.OH_Code}’s Company {glbCompany.GC_Code} does not have any active branch. Please check and correct the Netting Participant in the Netting System Portal(NSP), and ensure there is atleast one active branch in Company {glbCompany.GC_Code}.")
							 select log).ToList();
			AssertEquals("Warning message was added to the log", 1, logDetail.Count);
			Assert("Should not report anything on the Error Reporter when the Company has no active branch.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals(0, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		[TestDate(2020, 11, 01)]
		public void TestNoActiveNettingCompanyFound()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AddNettingCommunicationMode();

			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception", () => RunLogWalkerCycleForTest());

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
			Assert("Didn't report error after finished process logs.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			var trans = testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 2000M, NettingTransactionApprovalStatus.Approved);
			Factory.Save();

			glbCompany.GC_IsActive = false;

			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there are no branchs active for transaction's company", () => RunLogWalkerCycleForTest());

			var logDetail = (from log in NotifiedEventList
							 where log.Contains($@"[NettingTransactionSubscriber] No active Netting Company Found for the participant {recipientOrgHeader.OH_Code}. Please check and correct the Netting Participant in the Netting System Portal(NSP).")
							 select log).ToList();
			AssertEquals("Warning message was added to the log", 1, logDetail.Count);
			Assert("Should not report anything on the Error Reporter when the Company has no active branch.", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			AssertEquals(0, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		public override void TestIsRequired()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, new NettingTransactionSubscriber().IsRequired);
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, new NettingTransactionSubscriber().IsRequired);
		}

		[TestDate(2015, 09, 01)]
		public void TestBatchLoad()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AddNettingCommunicationMode();
			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");
			Factory.Save();

			using (var disposable = PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				RunLogWalkerCycleForTest();
				var nettingFactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Where(n => n.Load<EDIMessage>(new ZQuery { FetchOnlyFromLocalCache = true }).Length > 0).ToList();
				AssertEquals(1, nettingFactories.Count);
			}
		}

		[TestDate(2015, 09, 01)]
		public void TestNettingReceivableTransactionStatus()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var transaction1 = testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Added);
			var transaction2 = testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AddNettingCommunicationMode();

			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);

			transaction1.ApprovalStatus = NettingTransactionApprovalStatus.Approved;

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		[TestDate(2015, 09, 01)]
		public void TestRunningOnANonNettingSystem()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			AddNettingCommunicationMode();

			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(0, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		[TestDate(2015, 09, 01)]
		public void TestRecipientDoesNotHaveEHubCustomsCodeSet()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AddNettingCommunicationMode();

			Factory.Save();

			RunLogWalkerCycleForTest();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			AssertEquals(0, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);

			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		[TestDate(2015, 09, 01)]
		public void TestNettingOrgHeaderDoesNotHaveEdiCommunicationSet()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(0, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);

			AddNettingCommunicationMode();

			testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 1000M, NettingTransactionApprovalStatus.Approved);

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		[TestDate(2015, 09, 01)]
		public void TestNettingMessageIsNotQueuedWhenNettingTransactionIsCreatedBySystemUser()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NettingReceivableTransaction transaction1 = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 1000M, NettingTransactionApprovalStatus.Approved);
			transaction1.NRT_SystemCreateUser = User.ServiceUserCode;

			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(glbCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AddNettingCommunicationMode();
			recipientOrgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "NS12235");

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(0, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);

			NettingReceivableTransaction transaction2 = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV102", "USD", 1000M, NettingTransactionApprovalStatus.Approved);
			Assert("Precondition: transaction not created by system user", transaction2.NRT_SystemCreateUser != User.ServiceUserCode);

			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(1, LogSubscriber.NumberOfSuccessfulProcessedLogForTest);
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get { return true; }
		}

		void AddNettingCommunicationMode()
		{
			EDICommunicationsModeDependentCollection mode_NC = new EDICommunicationsModeDependentCollection(nettingSystemOrgHeader);
			var mode1 = mode_NC.AddNew();
			mode1.EK_Module = EDICommunicationsMode.Modules.Netting;
			mode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
			mode1.EK_Destination = "NS12235";
			mode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
		}

		NettingObjectCreator testObjectCreator;
		NettingSystem nettingSystem;
		NettingSystemPeriod period;
		NettingOrganisation issuer;
		NettingOrganisation recipient;
		OrgHeader recipientOrgHeader;
		OrgHeader nettingSystemOrgHeader;
		GlbCompany glbCompany;

		protected override void SetUp()
		{
			base.SetUp();

			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testObjectCreator = new NettingObjectCreator(Factory);

			var companyProxy = testObjectCreator.CreateOrgHeader("CMYPROXYA", true, true);
			glbCompany = testObjectCreator.CreateNewCompany("DDM", orgProxy: companyProxy);
			testObjectCreator.CreateNewBranch(glbCompany, "ADE");
			nettingSystem = testObjectCreator.CreateNettingSystem("NS1", "Test Netting System", glbCompany);
			period = testObjectCreator.CreateNettingPeriod(nettingSystem, "202011", ZDateTime.Today, ZDateTime.Today.AddDays(30), ZDateTime.Today.AddDays(25)
				, ZDateTime.Today.AddDays(37), ZDateTime.Today.AddDays(40), ZDateTime.Today.AddDays(45), ZDate.Today.AddDays(30));
			issuer = testObjectCreator.CreateNettingOrganisation(nettingSystem, testObjectCreator.ABIGAS, "FUL");
			recipientOrgHeader = testObjectCreator.AALSHI;
			recipient = testObjectCreator.CreateNettingOrganisation(nettingSystem, recipientOrgHeader, "FUL");
			nettingSystemOrgHeader = Factory.Load<OrgHeader>(glbCompany.GC_OH_OrgProxy);
		}
	}
}
