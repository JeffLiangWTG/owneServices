using System;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.ZArchitecture.Schema;

	class ProfitShareChargeWorkflowProcessorErrorEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		public void TestErrorMail_Shipment()
		{
			CreateProfitShareDetails("");

			var notifications = new NotificationCollection();
			notifications.AddError("Some Shipment Error\r\nOh no! Something went wrong with Profit Share!");
			var email = new ProfitShareChargeWorkflowProcessorErrorEmail(Shipment, null, notifications);
			email.Render();

			AssertEquals("Profit Share Charge Creation errors for Job S001001", email.Subject);
			AssertEquals(@"<html>
<body>
<p>Profit Share Charge creation was ran as a workflow action for Job S001001. There were errors during the operation.</p>
<p>Errors:</p>
<ul><li>Some Shipment Error</li><li>Oh no! Something went wrong with Profit Share!</li></ul>
</body>
</html>", email.Body);
		}

		public void TestErrorMail_Consol()
		{
			CreateProfitShareDetails("");

			var notifications = new NotificationCollection();
			notifications.AddError("Some Consol Error\r\nOh no! Something went wrong with Profit Share!");
			var email = new ProfitShareChargeWorkflowProcessorErrorEmail(null, Consol, notifications);
			email.Render();

			AssertEquals("Profit Share Charge Creation errors for Consol C001001", email.Subject);
			AssertEquals(@"<html>
<body>
<p>Profit Share Charge creation was ran as a workflow action for Consol C001001. There were errors during the operation.</p>
<p>Errors:</p>
<ul><li>Some Consol Error</li><li>Oh no! Something went wrong with Profit Share!</li></ul>
</body>
</html>", email.Body);
		}

		public void TestInvalidArguments()
		{
			AssertNoExceptionThrown(() => new ProfitShareChargeWorkflowProcessorErrorEmail(Shipment, Consol, new NotificationCollection()));
			AssertNoExceptionThrown(() => new ProfitShareChargeWorkflowProcessorErrorEmail(Shipment, null, new NotificationCollection()));
			AssertNoExceptionThrown(() => new ProfitShareChargeWorkflowProcessorErrorEmail(null, Consol, new NotificationCollection()));
			AssertExceptionThrown(typeof(ArgumentException), "Both PlugIn and Consol cannot be null.", () => new ProfitShareChargeWorkflowProcessorErrorEmail(null, null, new NotificationCollection()));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new ProfitShareChargeWorkflowProcessorErrorEmail(Shipment, null, null));
		}

		protected override Type EmailDefType
		{
			get
			{
				return typeof(ProfitShareChargeWorkflowProcessorErrorEmail);
			}
		}

		protected override void SetUp()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));
			Consol = TestObjectCreator.CreateConsol("AUMEL", "NZAKL", "C001001");
			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_UniqueConsignRef = "S001001";
			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		void CreateProfitShareDetails(string rateBasis)
		{
			Consol.SetDefaultSendingForwarderAddress(TestObjectCreator.Agent);
			Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Creditor1);
			ProfitShareDetails = new ProfitShareDetailCollection();
			CreateProfitShareProfile(TestObjectCreator.Agent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 30m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 18m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 4m, rateBasis, ProfitShareDetails, Shipment);
			Job = TestObjectCreator.CreateJob(Shipment, false);
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = Shipment;
			Factory.Save();
			JobCharge = Job.Charges.AddNew();
			JobCharge.JR_AC = Env.Registry.FreightChargeCode;
			JobCharge.JR_IsIncludedInProfitShare = true;
			JobCharge.JR_LocalSellAmt = 600m;
			JobCharge.JR_LocalCostAmt = 100m;
		}

		void CreateProfitShareProfile(OrgHeader agent, string role, decimal percent, string rateBasis, ProfitShareDetailCollection profitShares, ForwardingShipment shipment)
		{
			OrgAgentRelationship agentProfile = Factory.LoadTop1<OrgAgentRelationship>(new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agent.PK));
			if (agentProfile == null)
			{
				agentProfile = Factory.New<OrgAgentRelationship>();
				agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
				agentProfile.O3_OH_SendingAgent = agent.PK;
			}

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.Count == 1 ? agentProfile.GenericProfitShareDetails[0] : agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";
			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = role;
			party.PS_PartyProfitSharePercent = percent;
			party.PS_PartyRateBasis = rateBasis;
			ProfitShareDetail detail = profitShares.AddNew();
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment, agent, role, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator fTestObjectCreator;
		ForwardingConsol Consol;
		ForwardingShipment Shipment;
		JobCharge JobCharge;
		Job Job;
		ProfitShareDetailCollection ProfitShareDetails;
	}
}
