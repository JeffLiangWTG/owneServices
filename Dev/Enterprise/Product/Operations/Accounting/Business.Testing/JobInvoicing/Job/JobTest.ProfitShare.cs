using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(Job))]
	public class JobProfitShareTest : MasterFiles.Business.Testing.JobHeaderTest
	{
		public void TestProfitShareAgreementLoaded()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			var consignee = orgFactory.NewWithValidTestData<OrgHeader>();
			var consignor = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			orgFactory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			CommonShipment shipment = consol.Shipments.AddNew();
			var testOrganisation = TestObjectCreator.CreateOrgHeader("Org", true, true, true, true, true, true);
			shipment.JS_OH_DeliveryAgent = testOrganisation.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";

			Assert("Precondition: Shipment is an export", shipment.IsExport());

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = testOrganisation.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			OrgProfitShareDetails profitShareAgreement2 = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement2.O4_FreightMode = "AIR";
			profitShareAgreement2.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement2.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement2.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement2.O4_StartDate = ZDateTime.Today.AddDays(-10);
			profitShareAgreement2.O4_OH_OrgOverride = consignee.PK;

			profitShareFactory.Save();
			AssertEquals("Profit share agreement found", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);

			profitShareAgreement2.O4_OH_OrgOverride = consignor.PK;
			profitShareFactory.Save();

			testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertEquals("Profit share agreement for specific client found", profitShareAgreement2.PK, testJob.ProfitShareAgreement.PK);
		}

		public void TestProfitShareAgreementLoaded_AgencyProfile()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader controllingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			OrgHeader receivingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_Code = "RAGT";
			OrgHeader sendingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_Code = "SAGT";
			orgFactory.Save();

			CommonConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultReceivingForwarderAddress(receivingAgent);
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			Assert("Precondition: Shipment is an export", shipment.IsExport());

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();

			OrgAgentRelationship agentRelationshipR = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationshipR.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentRelationshipR.O3_OH_SendingAgent = receivingAgent.PK;

			OrgProfitShareDetails profitShareAgreementR = agentRelationshipR.ProfitShareDetails.AddNew();
			profitShareAgreementR.O4_FreightMode = "AIR";
			profitShareAgreementR.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreementR.O4_ReceivingPortOrCountry = "US";
			profitShareAgreementR.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreementR.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();
			AssertEquals("Profit share agreement found", profitShareAgreementR.PK, testJob.ProfitShareAgreement.PK);

			OrgAgentRelationship agentRelationshipS = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationshipS.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentRelationshipS.O3_OH_SendingAgent = sendingAgent.PK;

			OrgProfitShareDetails profitShareAgreementS = agentRelationshipS.ProfitShareDetails.AddNew();
			profitShareAgreementS.O4_FreightMode = "AIR";
			profitShareAgreementS.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreementS.O4_ReceivingPortOrCountry = "US";
			profitShareAgreementS.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreementS.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertEquals("Profit share agreement found", profitShareAgreementS.PK, testJob.ProfitShareAgreement.PK);

			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentRelationship.O3_OH_SendingAgent = controllingAgent.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			profitShareFactory.Save();

			testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertEquals("Still Sending Profit share agreement found", profitShareAgreementS.PK, testJob.ProfitShareAgreement.PK);

			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertEquals("Profit share agreement found", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);
		}

		public void TestProfitShare_ReCalculcatedOnSave()
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();
			OrgHeader controllingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "ZAJSHAHU";
			orgFactory.Save();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = "AIR";
			Assert("Precondition: Shipment is an export", shipment.IsExport());

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.PlugInData = shipment;
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			BusinessObjectFactory profitShareFactory = new BusinessObjectFactory();
			OrgAgentRelationship agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentRelationship.O3_OH_SendingAgent = controllingAgent.PK;

			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_SendingPortOrCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent;
			party.PS_PartyProfitSharePercent = 50m;

			profitShareFactory.Save();
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingCustomer);
			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			AssertEquals("Profit share agreement found", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);

			Charge charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 100m;
			charge.JR_LocalCostAmt = 20m;

			AssertEquals(1, testJob.Charges.Count);

			Factory.Save();
			new ProfitShareShipmentChargeCreator(new ProfitShareCalculator(Factory, shipment).CreateProfitShares(), testJob).CreateCharges();
			AssertEquals(2, testJob.Charges.Count);

			Factory.Save();
			AssertEquals("No changes, so no new PS", 2, testJob.Charges.Count);

			charge.JR_LocalCostAmt = 30m;
			Factory.Save();
			AssertEquals("Profit changed, so new PS", 3, testJob.Charges.Count);
			AssertEquals("testJob has changes", false, testJob.HasChanges);

			AccountingConfigurationRegistry.Instance.ProfitShareUpdateOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			charge.JR_LocalCostAmt = 10m;
			Factory.Save();
			AssertEquals("Profit changed, but registry to auto-update is off, so NO new PS", 3, testJob.Charges.Count);

			AccountingConfigurationRegistry.Instance.ProfitShareUpdateOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			testJob = newFactory.Load<Job>(testJob.PK);
			testJob.InitializeParentFromGenericJobWithoutSettingDefaults();
			testJob.HasChanges = true;
			newFactory.Save();
			ZQuery chargesInLocalFactoryQuery = new ZQuery();
			chargesInLocalFactoryQuery.FetchOnlyFromLocalCache = true;
			Charge[] charges = newFactory.Load<Charge>(chargesInLocalFactoryQuery);
			AssertEquals("Should be no charges loaded in memory when job is saved and charges are not loaded", 0, charges.Length);
		}

		public void TestProfitShare_RecalculcatedOnSave_CreateProfitShareAsAR()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var orgFactory = new BusinessObjectFactory();
			var controllingAgent = orgFactory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "CNTRL";
			orgFactory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "USLAX", transportMode: Constants.TransportModes.Air);
			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var testJob = TestObjectCreator.CreateJob(shipment, false);
			AssertNull("No profit share agreement found", testJob.ProfitShareAgreement);

			var profitShareFactory = new BusinessObjectFactory();
			var agentRelationship = profitShareFactory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentRelationship.O3_OH_SendingAgent = controllingAgent.PK;

			var profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-10);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(10);

			var party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent;
			party.PS_PartyProfitSharePercent = 50m;

			profitShareFactory.Save();

			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingCustomer);
			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			AssertEquals("Profit share agreement found", profitShareAgreement.PK, testJob.ProfitShareAgreement.PK);

			var charge = testJob.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_LocalSellAmt = 100m;
			charge.JR_LocalCostAmt = 20m;

			AssertEquals(1, testJob.Charges.Count);

			Factory.Save();

			new ProfitShareShipmentChargeCreator(new ProfitShareCalculator(Factory, shipment).CreateProfitShares(), testJob)
			.CreateCharges();
			AssertEquals(2, testJob.Charges.Count);

			Factory.Save();

			AssertEquals("No changes, so no new PS", 2, testJob.Charges.Count);

			charge.JR_LocalCostAmt = 30m;
			Factory.Save();

			AssertEquals("Profit changed, so new PS", 3, testJob.Charges.Count);
			AssertEquals("testJob has changes", false, testJob.HasChanges);

			AccountingConfigurationRegistry.Instance.ProfitShareUpdateOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			charge.JR_LocalCostAmt = 10m;
			Factory.Save();

			AssertEquals("Profit changed, but registry to auto-update is off, so NO new PS", 3, testJob.Charges.Count);
		}

		public void TestProfitShareAgreementWithClientSpecificRules()
		{
			var controllingAgent = TestObjectCreator.LocalClient;
			var orgOverride1 = TestObjectCreator.ZECTRA;
			var orgOverride2 = TestObjectCreator.XLINDU;

			var shipment = TestObjectCreator.CreateShipment("S001", GlbBranch.CurrentBranch.GB_RL_NKHomePort, "USLAX");
			shipment.JS_OH_ImportBroker = orgOverride1.PK;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = orgOverride2.PK;
			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var agentRelationship = TestObjectCreator.CreateAgentRelationship(controllingAgent, null, OrgAgentRelationship.ProfitShareTypes.AgencyProfile);
			var profitShare1 = TestObjectCreator.CreateProfitShare(agentRelationship, 50, 50, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "US", testJob.PlugInData.InvoicingSupporter.ContainerMode, orgOverride1, OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);
			var profitShare2 = TestObjectCreator.CreateProfitShare(agentRelationship, 50, 50, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "US", testJob.PlugInData.InvoicingSupporter.ContainerMode, orgOverride2, OrgProfitShareDetailsLookups.OrgOverrideTypesList.PUA.Code);
			Factory.Save();

			AssertEquals("Profit share agreement should be for delivery agent as it has higher priority than import broker.", profitShare2.PK, testJob.ProfitShareAgreement.PK);
		}

		#region TestGetProfitShareAgreement when picking GRP as priority

		public void TestGetProfitShareAgreement_ShipmentWithLCL_ConsolWithGRP_FindGRP()
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var lclProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.ContainerModes.LCL);
			var grpProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.ContainerModes.Groupage);
			Factory.Save();

			using (var testJob = TestObjectCreator.CreateJob(shipment, false))
			{
				AssertEquals(grpProfitShare, testJob.ProfitShareAgreement);
			}
		}

		public void TestGetProfitShareAgreement_ShipmentWithLCL_ConsolWithGRP_GRPNotAvailable_FindLCL()
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var lclProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.ContainerModes.LCL);
			var seaProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.TransportModes.Sea);
			Factory.Save();

			using (var testJob = TestObjectCreator.CreateJob(shipment, false))
			{
				AssertEquals(lclProfitShare, testJob.ProfitShareAgreement);
			}
		}

		public void TestGetProfitShareAgreement_ShipmentWithLCL_ConsolWithNotGRP_FindLCL()
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship.O3_OH_ReceivingAgent = TestObjectCreator.TestOrganisation.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = TestObjectCreator.TestOrganisation.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var lclProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.ContainerModes.LCL);
			var bcnProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.ContainerModes.BuyersConsol);
			var grpProfitShare = TestObjectCreator.CreateProfitShare(agentRelationship, "AU", "US", Constants.ContainerModes.Groupage);
			Factory.Save();

			using (var testJob = TestObjectCreator.CreateJob(shipment, false))
			{
				AssertEquals(lclProfitShare, testJob.ProfitShareAgreement);
			}
		}

		#endregion

		#region TestGetProfitShareAgreement_GatewayConsol - Sending Agent is a gateway agent

		public void TestGetProfitShareAgreement_GatewayConsol_SendingGatewayAgentGTA_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: "",
				message: "Sending GW Agent GTA should have PS agreement with condition SGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.SGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_SendingGatewayAgentGTT_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: "",
				message: "Sending GW Agent GTT should have PS agreement with condition SGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.SGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_SendingGatewayAgentGTA_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: "",
				message: "Sending GW Agent GTA should have PS agreement with JobType GCN condition blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_SendingGatewayAgentGTT_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: "",
				message: "Sending GW Agent GTT should have PS agreement with JobType GCN condition blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_SendingGatewayAgentGTA_NoSendingGatewayAgentAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.RGW
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: "",
				message: "There should be no matched agreement including ones with blank job type",
				expectedToHaveAnAgreement: false);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_SendingGatewayAgentGTT_NoSendingGatewayAgentAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.RGW
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: "",
				message: "There should be no matched agreement including ones with blank job type",
				expectedToHaveAnAgreement: false);
		}

		#endregion

		#region TestGetProfitShareAgreement_GatewayConsol - Receiving Agent is a gateway agent

		public void TestGetProfitShareAgreement_GatewayConsol_ReceivingGatewayAgentGTA_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "Receiving GW Agent GTA should have PS agreement with condition RGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.RGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_ReceivingGatewayAgentGTT_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "Receiving GW Agent GTT should have PS agreement with condition RGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.RGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_ReceivingGatewayAgentGTA_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "Receiving GW Agent GTA should have PS agreement with condition blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_ReceivingGatewayAgentGTT_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.Blank
				},
				sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "Receiving GW Agent GTT should have PS agreement with condition blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_ReceivingGatewayAgentGTA_NoReceivingGatewayAgentAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW
				},
				sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "There should be no matched agreement including ones with blank job type",
				expectedToHaveAnAgreement: false);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_ReceivingGatewayAgentGTT_NoReceivingGatewayAgentAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW
				},
				sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "There should be no matched agreement including ones with blank job type",
				expectedToHaveAnAgreement: false);
		}

		#endregion

		#region TestGetProfitShareAgreement_GatewayConsol - Both agents are gateway agents

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTA_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "Both GW Agents GTA should have PS agreement with condition BGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.BGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTT_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "Both GW Agents GTT should have PS agreement with condition BGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.BGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTA_GTT_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "GW Agents GTA/GTT should have PS agreement with condition BGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.BGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTT_GTA_Match()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.BGW,
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "GW Agents GTT/GTA should have PS agreement with condition BGW",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.BGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTA_FallbackToGatewayAgentTypeSGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "Both GW Agents GTA should have PS agreement with condition SGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.SGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTT_FallbackToGatewayAgentTypeSGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "Both GW Agents GTT should have PS agreement with condition SGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.SGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTA_GTT_FallbackToGatewayAgentTypeSGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "GW Agents GTA/GTT should have PS agreement with condition SGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.SGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTT_GTA_FallbackToGatewayAgentTypeSGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.SGW,
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "GW Agents GTT/GTA should have PS agreement with condition SGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.SGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTA_FallbackToGatewayAgentTypeRGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "Both GW Agents GTA should have PS agreement with condition RGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.RGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTT_FallbackToGatewayAgentTypeRGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "Both GW Agents GTT should have PS agreement with condition RGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.RGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTA_GTT_FallbackToGatewayAgentTypeRGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "GW Agents GTA/GTT should have PS agreement with condition RGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.RGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTT_GTA_FallbackToGatewayAgentTypeRGW()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.RGW,
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "GW Agents GTT/GTA should have PS agreement with condition RGW (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.RGW);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTA_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "Both GW Agents GTA should have PS agreement with condition Blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTT_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "Both GW Agents GTT should have PS agreement with condition Blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTA_GTT_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "GW Agents GTA/GTT should have PS agreement with condition Blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTT_GTA_FallbackToGatewayAgentTypeBlank()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: new[]
				{
					GatewayAgentTypesList.Codes.Blank,
				},
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "GW Agents GTT/GTA should have PS agreement with condition Blank (fallback)",
				expectedGatewayAgentType: GatewayAgentTypesList.Codes.Blank);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTA_NoGCNAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: Enumerable.Empty<string>(),
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "There should be no matched agreement including blank job type ones",
				expectedToHaveAnAgreement: false);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_BothGatewayAgentsGTT_NoGCNAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: Enumerable.Empty<string>(),
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "There should be no matched agreement including blank job type ones",
				expectedToHaveAnAgreement: false);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTA_GTT_NoGCNAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: Enumerable.Empty<string>(),
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff,
				message: "There should be no matched agreement including blank job type ones",
				expectedToHaveAnAgreement: false);
		}

		public void TestGetProfitShareAgreement_GatewayConsol_MixGatewayAgentsGTT_GTA_NoGCNAgreement()
		{
			AssertGetProfitShareAgreement_GatewayConsol(
				gatewayAgentTypesForSettingUpAgreements: Enumerable.Empty<string>(),
				sendingAgentStatus: AgentStatusList.Codes.GatewayAgentWithTariff, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				message: "There should be no matched agreement including blank job type ones",
				expectedToHaveAnAgreement: false);
		}

		#endregion

		#region Implementation & Helpers

		void AssertGetProfitShareAgreement_GatewayConsol(
			IEnumerable<string> gatewayAgentTypesForSettingUpAgreements,
			string sendingAgentStatus, string receivingAgentStatus,
			string message,
			bool expectedToHaveAnAgreement = false,
			string expectedJobType = JobTypesList.Codes.GCN,
			string expectedGatewayAgentType = "")
		{
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(sendingGatewayCompany: GlbCompany.CurrentCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);
			var agentRelationship = TestObjectCreator.CreateAgentRelationship(gatewayConsol.SendingForwarder, gatewayConsol.ReceivingForwarder);

			gatewayConsol.JK_SendingForwarderHandlingType = sendingAgentStatus;
			gatewayConsol.JK_ReceivingForwarderHandlingType = receivingAgentStatus;

			foreach (var gatewayAgentType in gatewayAgentTypesForSettingUpAgreements)
			{
				CreateProfitShareAgreement(agentRelationship, JobTypesList.Codes.GCN, gatewayAgentType);
			}
			// These 2 agreements are created but should never be picked for gateway consol profit sharing
			CreateProfitShareAgreement(agentRelationship, JobTypesList.Codes.Blank, gatewayAgentType: null);
			CreateProfitShareAgreement(agentRelationship, JobTypesList.Codes.SHP, gatewayAgentType: null);

			using (var testJob = TestObjectCreator.CreateJob(gatewayConsol))
			{
				var actualAgreement = testJob.ProfitShareAgreement;
				if (expectedToHaveAnAgreement)
				{
					AssertEquals(expectedJobType, actualAgreement.O4_JobType.ToString());
					AssertEquals(message, expectedGatewayAgentType, actualAgreement.O4_GatewayAgentType.ToString());
				}
				else
				{
					AssertNull(actualAgreement);
				}
			}
		}

		OrgProfitShareDetails CreateProfitShareAgreement(OrgAgentRelationship agentRelationship, string jobType, string gatewayAgentType, string transportMode = null)
		{
			transportMode = transportMode ?? OrgProfitShareDetailsLookups.FreightModesList.ALL.Code;

			return
				TestObjectCreator.CreateProfitShare(
					agentRelationship: agentRelationship,
					sendingProfitSharePercentage: 50,
					receivingProfitSharePercentage: 50,
					origin: GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
					destination: Constants.CountryCodes.NewZealand,
					transportMode: transportMode,
					jobType: jobType,
					gatewayAgentType: gatewayAgentType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;

		#endregion
	}
}
