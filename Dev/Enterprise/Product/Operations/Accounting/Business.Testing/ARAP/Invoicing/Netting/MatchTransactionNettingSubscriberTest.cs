using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	[TestedType(typeof(MatchTransactionNettingSubscriber))]
	class MatchTransactionNettingSubscriberTest : LogSubscriberTest<MatchTransactionNettingSubscriber>
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
				var receipt1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice1, ZDateTime.Today);
				var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoice1, ZDateTime.Today);
				Factory.Save();
			}

			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, 1100M, 110M, 1100M, 110M, ParticipantOrg1, TestObjectCreator.GLHeader2.PK, "FIN");
			Factory.Save();
			var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoice2, ZDateTime.Today);
			Factory.Save();

			foreach (var branch in company.Branches)
			{
				branch.GB_IsActive = false;
			}
			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there are no branchs active for transaction's company", () => RunLogWalkerCycleForTest());

			var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			var ediMessages = Factory.Load<EDIMessage>(query);
			AssertContainsExactElementsInAnyOrder("Should generate 1 universal events", new ZGuid[] { apInvoice2.PK }, ediMessages.Select(x => x.EM_LinkUniqueID));
		}

		[TestDate(2015, 06, 01)]
		public void TestMatchTransactionNettingSubscriber()
		{
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg1, TestObjectCreator.GLHeader1.PK, "FIN"); //should be processed successfully
			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg2, TestObjectCreator.GLHeader2.PK, "FIN"); //should not be processed as org does not have HID cus code setup.
			var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, ParticipantOrg1, TestObjectCreator.GLHeader2.PK, "FIN"); //should be processed successfully
			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, ParticipantOrg2, TestObjectCreator.GLHeader2.PK, "FIN"); //should not be processed as org does not have HID cus code setup.

			Factory.Save();

			var receipt1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice1, ZDateTime.Today);
			var receipt2 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice2, ZDateTime.Today);
			var payment1 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoice1, ZDateTime.Today);
			var payment2 = TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(apInvoice2, ZDateTime.Today);

			Factory.Save();

			using (var disposable = PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				RunLogWalkerCycleForTest();
				var factories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Where(f => f.NameForDebugging.StartsWith("Subscriber: MatchTransactionNettingSubscriber"));
				AssertEquals("NewsTransmitter factory has been created", 4, factories.Count());
				foreach (var factory in factories)
				{
					AssertTableHitCount("Expect no db hit to StmJobQueue table ", 0, StmJobQueueSchema.Constants.TableName, factory);
				}

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
				var ediMessages = Factory.Load<EDIMessage>(query);
				AssertContainsExactElementsInAnyOrder("Should generate 2 universal events", new ZGuid[] { arInvoice1.PK, apInvoice1.PK }, ediMessages.Select(x => x.EM_LinkUniqueID));
			}
		}

		[TestDate(2015, 06, 01)]
		public void TestMatchTransactionNettingSubscriberShouldNotCreateEDIMessageForInvoiceWhichHasSameOrgProxyWithBranch()
		{
			var originalBranchOrgProxyPK = GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy;
			try
			{
				GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = ParticipantOrg1.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, ParticipantOrg1, TestObjectCreator.GLHeader1.PK, "FIN"); //should be processed successfully
				Factory.Save();
				var receipt1 = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(arInvoice1, ZDateTime.Today);
				Factory.Save();

				RunLogWalkerCycleForTest();

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, arInvoice1.PK);
				var ediMessages = Factory.Load<EDIMessage>(query);
				AssertEquals(0, ediMessages.Length);
			}
			finally
			{
				GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = originalBranchOrgProxyPK;
			}
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
