using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class ProfitShareChargeWorkflowProcessorTest : TestCaseWithFactory
	{
		public void TestShipmentProfitShareChargeCreation_ValidCase()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);

			var notifications = new NotificationCollection();

			var processor = new ProfitShareChargeWorkflowProcessor(Shipment);
			AssertNoExceptionThrown(() => processor.Process(notifications));

			AssertEquals("One profit share charge is created", 2, Job.Charges.Count);
			AssertEquals("Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00", Job.Charges[1].JR_Desc);
			AssertEquals(150M, Job.Charges[1].JR_LocalCostAmt);
		}

		public void TestShipmentProfitShareChargeCreation_ProfitShareAdjustmentChargeCodeDoesNotBelongToCurrentCompany()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);

			var notifications = new NotificationCollection();

			var processor = new ProfitShareChargeWorkflowProcessor(Shipment);
			AssertNoExceptionThrown(() => processor.Process(notifications));
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.NewGuid());
			var ex = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => processor.Process(notifications));
			AccountingEmailDef email = (AccountingEmailDef)ex.Emails[0];

			AssertEquals("Email should be returned with exception and not via OutgoingMailManager", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email should be returned with exception when validation error occurs", ex.Emails);
			AssertEquals("Profit Share Charge Creation errors for Job S001001", email.Subject);
			AssertEquals(@"<html>
<body>
<p>Profit Share Charge creation was ran as a workflow action for Job S001001. There were errors during the operation.</p>
<p>Errors:</p>
<ul><li>Incorrect Registry Item value.</li><li>Please set up a correct value to the Registry Item: 'Accounting -> Job Invoicing -> Profit Share -> Profit Share Adjustment Charge Code'.</li></ul>
</body>
</html>", email.Body);
		}

		public void TestShipmentProfitShareChargeCreation_InvalidCase()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Job.JH_OA_LocalChargesAddr = Guid.Empty;
			Job.JH_OA_AgentCollectAddr = Guid.Empty;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);

			var notifications = new NotificationCollection();

			var processor = new ProfitShareChargeWorkflowProcessor(Shipment);
			var ex = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => processor.Process(notifications));
			AccountingEmailDef email = (AccountingEmailDef)ex.Emails[0];

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(Job.PK);
			AssertEquals("No profit share charge is created", 1, jobInNewFactory.Charges.Count);
			AssertEquals("Email should be returned with exception and not via OutgoingMailManager", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email should be returned with exception when validation error occurs", ex.Emails);
			AssertEquals("Profit Share Charge Creation errors for Job S001001", email.Subject);
			AssertEquals(@"<html>
<body>
<p>Profit Share Charge creation was ran as a workflow action for Job S001001. There were errors during the operation.</p>
<p>Errors:</p>
<ul><li>Profit Share charge(s) were created but not posted because there are validation error(s).</li><li>The Profit Share charge(s) should be posted manually after fixing the validation error(s).</li><li>Error - JR_CostTaxDate: No rate found for selected date.</li><li>Error - JH_OA_AgentCollectAddr: Please enter Local Client or Overseas Agent.</li><li>Error - JH_OA_LocalChargesAddr: Please enter Local Client or Overseas Agent.</li></ul>
</body>
</html>", email.Body);
		}

		public void TestConsolProfitShareChargeCreation_ValidCase()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);

			var notifications = new NotificationCollection();

			var processor = new ProfitShareChargeWorkflowProcessor((IJobCostingPlugIn)Consol);
			AssertNoExceptionThrown(() => processor.Process(notifications));

			AssertEquals("One profit share charge is created", 2, Job.Charges.Count);
			AssertEquals("Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00", Job.Charges[1].JR_Desc);
			AssertEquals(150M, Job.Charges[1].JR_LocalCostAmt);
		}

		public void TestConsolProfitShareChargeCreation_PartialValid()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var shipment1 = CreateProfitShareDetails("", "S001001", createNewConsol: true);
			var shipment2 = CreateProfitShareDetails("", "S001002", createNewConsol: false);
			var shipment3 = CreateProfitShareDetails("", "S001003", createNewConsol: false);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(false);
			TestObjectCreator.Creditor1.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Creditor1.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			var loader = new Job.Loader(Factory, shipment1);
			var job1 = loader.Load(setParent: true, setJobDefaults: false);
			job1.JH_OA_LocalChargesAddr = Guid.Empty;
			job1.JH_OA_AgentCollectAddr = Guid.Empty;
			Factory.Save();

			loader = new Job.Loader(Factory, shipment2);
			var job2 = loader.Load(setParent: true, setJobDefaults: false);

			loader = new Job.Loader(Factory, shipment3);
			var job3 = loader.Load(setParent: true, setJobDefaults: false);

			AssertEquals("Precondition: Only one charge before profit share", 1, job1.Charges.Count);
			AssertEquals("Precondition: Only one charge before profit share", 1, job2.Charges.Count);
			AssertEquals("Precondition: Only one charge before profit share", 1, job3.Charges.Count);

			var notifications = new NotificationCollection();

			var processor = new ProfitShareChargeWorkflowProcessor((IJobCostingPlugIn)Consol);
			var ex = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => processor.Process(notifications));
			AccountingEmailDef email = (AccountingEmailDef)ex.Emails[0];

			var newFactory = new BusinessObjectFactory();
			var job1InNewFactory = newFactory.Load<Job>(job1.PK);
			var job2InNewFactory = newFactory.Load<Job>(job2.PK);
			var job3InNewFactory = newFactory.Load<Job>(job3.PK);
			AssertEquals("No profit share charge is created for shipment1", 1, job1InNewFactory.Charges.Count);

			AssertEquals("No profit share charge is created for shipment2", 1, job2InNewFactory.Charges.Count);

			AssertEquals("No profit share charge is created for shipment3", 1, job3InNewFactory.Charges.Count);

			AssertEquals("Email should be returned with exception and not via OutgoingMailManager", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email should be returned with exception when validation error occurs", ex.Emails);
			AssertEquals("Profit Share Charge Creation errors for Consol C00001000", email.Subject);

			AssertContains("S001001 - Error - JH_OA_AgentCollectAddr: Please enter Local Client or Overseas Agent.", email.Body);
			AssertContains("S001001 - Error - JH_OA_LocalChargesAddr: Please enter Local Client or Overseas Agent.", email.Body);
		}

		public void TestShipmentProfitShareChargeCreation_DoesNotSaveFactory_AndDoesNotSendEmail_WhenNoProfitShareChargesCreatedOrUpdated()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);

			var notifications = new NotificationCollection();

			var mockChargeCreator = new Mock<IProfitShareChargeCreator>();
			mockChargeCreator.Setup(x => x.CreateCharges()).Returns(false);
			mockChargeCreator.Setup(x => x.ValidationErrors).Returns(ZString.Empty);
			var mockCalculator = new Mock<IProfitShareCalculator>();
			mockCalculator.Setup(x => x.CreateProfitShares()).Returns(new ProfitShareDetailCollection());

			var mockProvider = new Mock<ProfitShareShipmentChargeProcessor.IObjectProvider>();
			mockProvider.Setup(x => x.GetCalculator(It.IsAny<BusinessObjectFactory>(), It.IsAny<IJobCostingPlugIn>(), It.IsAny<IJobInvoicingPlugIn[]>())).Returns(mockCalculator.Object);
			mockProvider.Setup(x => x.GetChargeCreator(It.IsAny<ProfitShareDetailCollection>(), It.IsAny<Job>(), It.IsAny<bool>())).Returns(mockChargeCreator.Object);

			var objectProvider = new ProfitShareShipmentChargeProcessor.ObjectProviderImplementation();
			var processor = new ProfitShareChargeWorkflowProcessor((IJobCostingPlugIn)Consol, objectProvider: mockProvider.Object);
			var ex = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => processor.Process(notifications));

			CombineAssertions("No profit share charges are created", () =>
			{
				AssertEquals("Job.Charges.Count", 1, Job.Charges.Count);
				AssertEquals("International Freight", Job.Charges[0].JR_Desc);
				AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);
			});

			AssertEquals("When no charge lines are created, no email should be created", 0, ex.Emails.Length);
			AssertEquals("Email should never be sent via OutgoingMailManager", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestInvalidRegistryValue_Shipment_ProfitShareChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			AssertInvalidRegistryValue_Shipment(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Caption);
		}

		public void TestInvalidRegistryValue_Shipment_ProfitShareChargeCodesPerParty()
		{
			var lookups = new OrgProfitSharePartyLookups(null);
			var collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			var chargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			chargeCode.AC_GC = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = chargeCode.PK;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertInvalidRegistryValue_Shipment(AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Caption);
		}

		void AssertInvalidRegistryValue_Shipment(string registryName)
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);

			var notifications = new NotificationCollection();

			var processor = new ProfitShareChargeWorkflowProcessor(Shipment);
			var ex = AssertExceptionThrown<LogSubscriberToAbortLogGroupProcessingSilentlyException>(() => processor.Process(notifications));

			AccountingEmailDef email = (AccountingEmailDef)ex.Emails[0];
			AssertEquals("No profit share charge is created", 1, Job.Charges.Count);
			AssertEquals("Email should be returned with exception and not via OutgoingMailManager", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertNotNull("Email should be returned with exception when validation error occurs", ex.Emails);
			AssertEquals("Profit Share Charge Creation errors for Job S001001", email.Subject);
			AssertEquals($@"<html>
<body>
<p>Profit Share Charge creation was ran as a workflow action for Job S001001. There were errors during the operation.</p>
<p>Errors:</p>
<ul><li>Incorrect Registry Item value.</li><li>Please set up a correct value to the Registry Item: 'Accounting -> Job Invoicing -> Profit Share -> {registryName}'.</li></ul>
</body>
</html>", email.Body);
		}

		#region Implementation

		ForwardingShipment CreateProfitShareDetails(string rateBasis, string shipmentNumber = "", bool createNewConsol = true)
		{
			if (createNewConsol)
			{
				Consol = Factory.New<ForwardingConsol>();
				Consol.SetDefaultSendingForwarderAddress(TestObjectCreator.Agent);
				Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Creditor1);
			}

			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_UniqueConsignRef = string.IsNullOrEmpty(shipmentNumber) ? "S001001" : shipmentNumber;

			ProfitShareDetails = new ProfitShareDetailCollection();

			CreateProfitShareProfile(TestObjectCreator.Agent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 30m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 18m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 4m, rateBasis, ProfitShareDetails, Shipment);

			Job = TestObjectCreator.CreateJob(Shipment, false);
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = Shipment;

			JobCharge = Job.Charges.AddNew();
			JobCharge.JR_AC = Env.Registry.FreightChargeCode;
			JobCharge.JR_IsIncludedInProfitShare = true;
			JobCharge.JR_LocalSellAmt = 600m;
			JobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			JobCharge.JR_LocalCostAmt = 100m;

			Factory.Save();

			return Shipment;
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

		protected override void SetUp()
		{
			var currentStaffMember = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaffMember.GS_EmailAddress = "somebody@somedomain.com";
			var group = Factory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			AccountingConfigurationRegistry.Instance.JobPostingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fTestObjectCreator;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		JobCharge JobCharge;
		Job Job;
		ProfitShareDetailCollection ProfitShareDetails;

		#endregion
	}
}
