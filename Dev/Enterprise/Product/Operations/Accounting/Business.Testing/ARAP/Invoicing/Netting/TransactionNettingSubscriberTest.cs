using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Test;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	[TestedType(typeof(TransactionNettingSubscriber))]
	class TransactionNettingSubscriberTest : LogSubscriberTest<TransactionNettingSubscriber>
	{
		[TestDate(2015, 06, 01)]
		public void TestRelativeCompanyHasNoActiveBranch()
		{
			var company = testObjectCreator.CreateCompanyAndBranch("AUDEM");
			Factory.Save();

			InvoicingBase arInvoice1, apInvoice1;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, company.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg1, TestObjectCreator.GLHeader1.PK, "FIN");
				apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, ParticipantOrg1, TestObjectCreator.GLHeader2.PK, "FIN");
				Factory.Save();
			}

			foreach (var branch in company.Branches)
			{
				branch.GB_IsActive = false;
			}

			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, 1100M, 110M, 1100M, 110M, ParticipantOrg1, TestObjectCreator.GLHeader2.PK, "FIN");

			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there are no branchs active for transaction's company", () => RunLogWalkerCycleForTest());

			var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
			var ediMessages = Factory.Load<EDIMessage>(query);
			AssertContainsExactElementsInAnyOrder("Should generate 1 universal transactions", new ZGuid[] { apInvoice2.PK }, ediMessages.Select(x => x.EM_LinkUniqueID));
		}

		[TestDate(2015, 06, 01)]
		public void TestTransactionNettingSubscriber()
		{
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg1, TestObjectCreator.GLHeader1.PK, "FIN"); //should be processed successfully
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg2, TestObjectCreator.GLHeader2.PK, "FIN"); //should not be processed as org does not have HID cus code setup.
			var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, ParticipantOrg1, TestObjectCreator.GLHeader2.PK, "FIN"); //should be processed successfully
			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, ParticipantOrg2, TestObjectCreator.GLHeader2.PK, "FIN"); //should not be processed as org does not have HID cus code setup.

			Factory.Save();

			RunLogWalkerCycleForTest();

			var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
			var ediMessages = Factory.Load<EDIMessage>(query);
			AssertContainsExactElementsInAnyOrder("Should generate 2 universal transactions", new ZGuid[] { arInvoice1.PK, apInvoice1.PK }, ediMessages.Select(x => x.EM_LinkUniqueID));
		}

		public void TestTransactionNettingSubscriberForReversedTransaction()
		{
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg1, TestObjectCreator.GLHeader1.PK, "FIN"); //should be processed successfully
			Factory.Save();
			RunLogWalkerCycleForTest();
			AssertEDIMessageExist(arInvoice1.PK);

			var reverser = new ARInvoiceReversing(arInvoice1 as ARInvoice);
			reverser.Reverse();
			Factory.Save();

			var reversedInvoice = arInvoice1.ReverseTransaction;
			RunLogWalkerCycleForTest();
			AssertEDIMessageExist(arInvoice1.PK, reversedInvoice.PK);

			void AssertEDIMessageExist(params ZGuid[] transactionPKs)
			{
				var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
				var ediMessages = Factory.Load<EDIMessage>(query);
				AssertContainsExactElementsInAnyOrder("Should generate 1 universal transactions", transactionPKs, ediMessages.Select(x => x.EM_LinkUniqueID));
			}
		}

		[TestDate(2015, 06, 01)]
		public void TestTransactionNettingSubscriber_DoNotSaveTheFactory()
		{
			var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, ParticipantOrg1, TestObjectCreator.GLHeader2.PK, "FIN"); //should be processed successfully

			Factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				var logSubscriber = new TransactionNettingSubscriber();
				var logger = new LoggerForTest();
				var mock = new MockNewsTransmitter(logSubscriber, new[] { logSubscriber }, new SubscriberParameters() { Logger = logger });
				mock.SaveProcessedLogsOverride = (a, b) => Enumerable.Empty<ProcessableLogGroup>();
				LogWalkerRunner.Master().Process(logger, CancellationToken.None); // No save means no change.

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				using (newFactory.AddDisposableService())
				{
					var loadedInvoice = newFactory.Load<APInvoice>(apInvoice1.PK);

					var logs = newFactory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, apInvoice1.PK).AddToFilter(StmJobQueueSchema.SJ_FilterName, logSubscriber.Name));
					mock.ProcessLogs(logs);

					var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, apInvoice1.PK);
					var messages = Factory.Load<EDIMessage>(query);
					var universalEvent = messages.FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent);
					AssertNull("Universal Event transmitted", universalEvent);
					var universalTransaction = messages.FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
					AssertNull("Universal Transaction transmitted", universalTransaction);
				}
			}
		}

		[TestDate(2015, 06, 01)]
		public void TestTransactionNettingSubscriberShouldNotCreateEDIMessageForInvoiceWhichHasSameOrgProxyWithBranch()
		{
			var originalBranchOrgProxyPK = GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy;
			try
			{
				GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = ParticipantOrg1.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg1, TestObjectCreator.GLHeader1.PK, "FIN"); //should be processed successfully
				Factory.Save();

				RunLogWalkerCycleForTest();

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, arInvoice1.PK);
				var ediMessages = Factory.Load<EDIMessage>(query);
				AssertEquals(0, ediMessages.Length);
			}
			finally
			{
				GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = originalBranchOrgProxyPK;
			}
		}

		public void TestTransactionNettingSubscriberForCashInvoice()
		{
			var invoice = TestObjectCreator.CreateCashInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, ParticipantOrg1, 100M, 0M, TestObjectCreator.GLHeader1.PK, TestObjectCreator.AUDBankAccount);

			Factory.Save();

			RunLogWalkerCycleForTest();

			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, invoice.PK);
			var messages = Factory.Load<EDIMessage>(query);
			AssertEquals("2 messages generated", 2, messages.Length);

			var universalTransaction = messages.FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
			AssertNotNull("Universal Transaction transmitted", universalTransaction);

			var universalEvent = messages.FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			AssertNotNull("Universal Event transmitted", universalEvent);

			Assert("The event needs to be sent after the transaction is sent", universalEvent?.EM_SystemCreateTimeUtc >= universalTransaction?.EM_SystemCreateTimeUtc);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid());

			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-1).ToDateTime());

			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.ABIGAS.PK.ToGuid());
			var destination = "WERWERWER";
			TestObjectCreator.ABIGAS.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, destination);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ParticipantOrg1 = TestObjectCreator.CreateOrgHeader("TSTPCNT1", true, true);
			TestObjectCreator.AddEdiCommunication(ParticipantOrg1, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, destination);

			ParticipantOrg1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "1123456");

			ParticipantOrg2 = TestObjectCreator.CreateOrgHeader("TSTPCNT2", true, true);
			TestObjectCreator.AddEdiCommunication(ParticipantOrg2, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, destination);

			Factory.Save();
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get
			{
				return true;
			}
		}

		OrgHeader ParticipantOrg1, ParticipantOrg2;
	}
}
