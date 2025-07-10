using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class IncludeChargeInProfitShareProcessorTest : TestCaseWithFactory
	{
		public void TestProcessJobInvoiceHeaderWhenJobHeaderDoesNotExist()
		{
			var orgFactory = new BusinessObjectFactory();
			var consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			var consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();

			orgFactory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var includeChargeInProfitShareProcessor = new IncludeChargeInProfitShareProcessor(shipment);
			var notifications = new NotificationBuffer();

			var testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;

			Factory.Save();

			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			Assert("Precondition:", !charge.JR_IsIncludedInProfitShare);
			includeChargeInProfitShareProcessor.Process(notifications);
			Assert(!notifications.HasErrors);
			Assert("As no profit share agreement found, so the flag will remain false", !charge.JR_IsIncludedInProfitShare);

			var profitShareFactory = new BusinessObjectFactory();
			var agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			AssertNotNull("Profit share agreement now exists", testJob.ProfitShareAgreement);

			Assert("Precondition:", !charge.JR_IsIncludedInProfitShare);
			includeChargeInProfitShareProcessor.Process(notifications);
			Assert(!notifications.HasErrors);
			Assert("Charge should have JR_IsIncludedInProfitShare flag set to true", charge.JR_IsIncludedInProfitShare);
			AssertNotNull(testJob.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_Reference == "Charge(s) included in Profit Share by 'IPS' trigger action."));

			var newFactory = Factory.CreateNewFactory();
			var chargeInDatabase = newFactory.Load<JobCharge>(charge.PK);

			AssertEquals("For charge in database, JR_IsIncludedInProfitShare flag should still be false.", false, chargeInDatabase.JR_IsIncludedInProfitShare);

			Factory.Save();

			AssertEquals("For charge in database, JR_IsIncludedInProfitShare flag should be true.", true, chargeInDatabase.JR_IsIncludedInProfitShare);
		}

		public void TestIncludeChargeInProfitShareProcessor_ThrowExceptionWhenProcessHasError()
		{
			var orgFactory = new BusinessObjectFactory();
			var consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			var consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();

			orgFactory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var includeChargeInProfitShareProcessor = new IncludeChargeInProfitShareProcessor(shipment);
			var notifications = new NotificationBuffer();

			var testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;

			Factory.Save();

			var profitShareFactory = new BusinessObjectFactory();
			var agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			AssertNotNull("Profit share agreement now exists", testJob.ProfitShareAgreement);

			testJob.JH_GE = ZGuid.Empty;

			Assert("Precondition:", !charge.JR_IsIncludedInProfitShare);
			var exceptionThrown = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>("Expected exception", "Include charge in profit share processing unsuccessful.", () => includeChargeInProfitShareProcessor.Process(notifications));
			AssertNotNull("Email in exception", exceptionThrown.Emails);
			AssertEquals("EmailsCreated.Count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Assert(testJob.HasErrors);
		}

		public void TestIncludeChargeInProfitShareProcessor_DisposeAfterProcessAndSave()
		{
			var orgFactory = new BusinessObjectFactory();
			var consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			var consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();

			orgFactory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var includeChargeInProfitShareProcessor = new IncludeChargeInProfitShareProcessor(shipment);
			var notifications = new NotificationBuffer();

			var testJob = new Job.Loader(shipment).TryCreateWithMutex();
			testJob.PlugInData = shipment;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;

			Factory.Save();

			includeChargeInProfitShareProcessor.Process(notifications);

			var profitShareFactory = new BusinessObjectFactory();
			var agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			includeChargeInProfitShareProcessor.Process(notifications);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loader = new Job.Loader(newFactory, shipment);

			using (var job = loader.TryCreateWithMutex())
			{
				AssertNotNull("New Job can be created because job has been dispose after Factory Save", job);
			}
		}

		public void TestIncludeChargeInProfitShareProcessor_DisposeAfterProcessFailed()
		{
			var orgFactory = new BusinessObjectFactory();
			var consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			var consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();

			orgFactory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			var includeChargeInProfitShareProcessor = new IncludeChargeInProfitShareProcessor(shipment);
			var notifications = new NotificationBuffer();

			var testJob = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			testJob.PlugInData = shipment;

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.FRT.PK;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GSTFREE1.PK;

			Factory.Save();

			var profitShareFactory = new BusinessObjectFactory();
			var agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			testJob.JH_GE = ZGuid.Empty;

			AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>("Expected exception", "Include charge in profit share processing unsuccessful.", () => includeChargeInProfitShareProcessor.Process(notifications));
			Assert(testJob.HasErrors);

			var newFactory = Factory.CreateNewFactory();
			var loader = new Job.Loader(newFactory, shipment);

			using (var job = loader.TryCreateWithMutex())
			{
				AssertNotNull("New Job can be created because job has been dispose after Factory Save", job);
			}
		}

		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
