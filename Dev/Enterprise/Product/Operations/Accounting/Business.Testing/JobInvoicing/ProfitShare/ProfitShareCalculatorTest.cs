using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ProfitShareCalculatorTest : TestCaseWithFactory
	{
		public void TestCreateProfitShares_ShipmentHasNoSendingAgent_ShouldLoadProfitShareAgreementsWithoutSendingAgent()
		{
			var sendingAgent = TestObjectCreator.ABIGAS;
			var receivingAgent = TestObjectCreator.AALSHI;

			var relationshipSendReceive = TestObjectCreator.CreateAgentRelationship(sendingAgent, receivingAgent);
			var agreementSendReceive = TestObjectCreator.CreateProfitShare(relationshipSendReceive, 50, 50, "AU", "US", "ALL");

			var relationshipReceive = TestObjectCreator.CreateAgentRelationship(null, receivingAgent);
			var agreementReceive = TestObjectCreator.CreateProfitShare(relationshipReceive, 60, 40, "AU", "US", "ALL");

			var relationshipBlank = TestObjectCreator.CreateAgentRelationship(null, null);
			var agreementBlank = TestObjectCreator.CreateProfitShare(relationshipBlank, 30, 70, "AU", "US", "ALL");

			Factory.Save();

			// Shipment without SendingAgent, with Delivery Agent
			var shipment1 = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			shipment1.JS_OH_DeliveryAgent = receivingAgent.PK;
			Assert("Precondition: Shipment has no sending agent", shipment1.JS_JK_SendingAgent.IsEmpty);

			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = job1.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_IsIncludedInProfitShare = true;
			charge1.JR_LocalSellAmt = 500m;
			charge1.JR_LocalCostAmt = 150m;

			// Shipment without SendingAgent, without Delivery Agent
			var shipment2 = TestObjectCreator.CreateShipment("S002", "AUSYD", "USLAX");
			shipment2.JS_OH_DeliveryAgent = ZGuid.Empty;

			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var charge2 = job2.Charges.AddNew();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_IsIncludedInProfitShare = true;
			charge2.JR_LocalSellAmt = 500m;
			charge2.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment1);
			AssertProfitShares(
				"Agreement matching receiving agent and blank sending agent should be applied.",
				["RCV|AALSHI|40.00% of profit of 350.00"],
				calculator.CreateProfitShares());

			calculator = new ProfitShareCalculator(Factory, shipment2);
			AssertProfitShares(
				"Profit share should not be created because there is no agent from the shipment to match.",
				[],
				calculator.CreateProfitShares());
		}

		public void TestCreateProfitShares_ConsolReceivingAgentFallback()
		{
			var receivingAgent = TestObjectCreator.AALSHI;
			var anotherReceivingAgent = TestObjectCreator.ZECTRA;

			var relationship = TestObjectCreator.CreateAgentRelationship(null, receivingAgent);
			TestObjectCreator.CreateProfitShare(relationship, 50, 50, "AU", "US", "ALL");

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 500m;
			charge.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"Agreement should be found with matched Shipment Delivery Agent and profit shares should be created.",
				["RCV|AALSHI|50.00% of profit of 350.00"],
				calculator.CreateProfitShares());

			shipment.JS_OH_DeliveryAgent = anotherReceivingAgent.PK;
			calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"Agreement should NOT be found with unmatched Shipment Delivery Agent",
				[],
				calculator.CreateProfitShares());

			consol.JK_OA_ReceivingForwarderAddress = anotherReceivingAgent.MainAddress.PK;
			shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
			calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"Fallback: Agreement should NOT be found with unmatched Consol Receiving Forwarder (blank Shipment Delivery Agent)",
				[],
				calculator.CreateProfitShares());

			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"Fallback: Agreement should be found with matched Consol Receiving Forwarder (blank Shipment Delivery Agent)",
				["RCV|AALSHI|50.00% of profit of 350.00"],
				calculator.CreateProfitShares());

			shipment.JS_OH_DeliveryAgent = anotherReceivingAgent.PK;
			calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"Fallback: Agreement should be found with matched Consol Receiving Forwarder (unmatched Shipment Delivery Agent) but the PS should be created for the shipment Delivery Agent",
				["RCV|ZECTRA|50.00% of profit of 350.00"],
				calculator.CreateProfitShares());
		}

		static void AddShareForParty(OrgProfitShareDetails profitShare, string partyType, decimal percent)
		{
			var party = profitShare.PartyDetails.AddNew();
			party.PS_PartyType = partyType;
			party.PS_PartyProfitSharePercent = percent;
		}

		public void TestCreateProfitShares_ShipmentHasPickupAgent_ShouldMatchProfitShareSetupOrgOverride()
		{
			var receivingAgent = TestObjectCreator.AALSHI;
			var anotherReceivingAgent = TestObjectCreator.ZECTRA;

			var relationship = TestObjectCreator.CreateAgentRelationship(null, null);
			// unexpected profit share setups
			TestObjectCreator.CreateProfitShare(relationship, 10, 90, "AU", "US", "ALL", orgOverride: null, orgType: null);
			TestObjectCreator.CreateProfitShare(relationship, 20, 80, "AU", "US", "ALL", orgOverride: receivingAgent, orgType: "ALL");
			TestObjectCreator.CreateProfitShare(relationship, 30, 70, "AU", "US", "ALL", orgOverride: anotherReceivingAgent, orgType: "ALL");
			TestObjectCreator.CreateProfitShare(relationship, 40, 60, "AU", "US", "ALL", orgOverride: anotherReceivingAgent, orgType: "PUA");

			// OrgOverride == Shipment Pickup Agent
			var profitShare = TestObjectCreator.CreateProfitShare(relationship, 60, 25, "AU", "US", "ALL", orgOverride: receivingAgent, orgType: "PUA");
			AddShareForParty(profitShare, "PIC", 15);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX");
			shipment.PickupAgentPK = receivingAgent.PK;

			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 500m;
			charge.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"Shipment Pickup Agent should match Profit Share Setup PUA OrgOverride AALSHI",
				["PIC|AALSHI|15.00% of profit of 350.00"],
				calculator.CreateProfitShares());
		}

		public void TestCreateProfitShares_Priority_SendingAgentVsPickupAgent()
		{
			// Sending Agent < Pickup Agent
			// PSA/PSS #1 with matching sending agent, matching receiving agent, blank Pickup Agent
			// PSA/PSS #2 with blank Sending agent, matching Receiving Agent, matching Pickup Agent
			// PSA/PSS #2 wins

			var sendingAgent = TestObjectCreator.ABIGAS;
			var receivingAgent = TestObjectCreator.AALSHI;
			var pickupAgent = TestObjectCreator.ZECTRA;

			var relationship1 = TestObjectCreator.CreateAgentRelationship(sendingAgent, receivingAgent);
			TestObjectCreator.CreateProfitShare(relationship1, 10, 90, "AU", "US", "ALL", orgOverride: null, orgType: null);

			var relationship2 = TestObjectCreator.CreateAgentRelationship(null, receivingAgent);
			var profitShare = TestObjectCreator.CreateProfitShare(relationship2, 20, 50, "AU", "US", "ALL", orgOverride: pickupAgent, orgType: "PUA");
			AddShareForParty(profitShare, "PIC", 30);

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
			shipment.PickupAgentPK = pickupAgent.PK;

			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 500m;
			charge.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"The Profit Share Setup matching Receiving Agent and Pickup Agent should have higher priority than the one matching Sending Agent and Receiving Agent.",
				[
					"SEN|ABIGAS|20.00% of profit of 350.00",
					"RCV|AALSHI|50.00% of profit of 350.00",
					"PIC|ZECTRA|30.00% of profit of 350.00",
				],
				calculator.CreateProfitShares());
		}

		public void TestCreateProfitShares_Priority_ReceivingAgentVsPickupAgent()
		{
			// Receiving Agent < Pickup Agent
			// PSA/PSS #1 with matching sending agent, matching receiving agent, blank Pickup Agent
			// PSA/PSS #2 with matching Sending agent, blank Receiving Agent, matching Pickup Agent
			// PSA/PSS #2 wins

			var sendingAgent = TestObjectCreator.ABIGAS;
			var receivingAgent = TestObjectCreator.AALSHI;
			var pickupAgent = TestObjectCreator.ZECTRA;

			var relationship1 = TestObjectCreator.CreateAgentRelationship(sendingAgent, receivingAgent);
			TestObjectCreator.CreateProfitShare(relationship1, 10, 90, "AU", "US", "ALL", orgOverride: null, orgType: null);

			var relationship2 = TestObjectCreator.CreateAgentRelationship(sendingAgent, null);
			TestObjectCreator.CreateProfitShare(relationship2, 20, 80, "AU", "US", "ALL", orgOverride: pickupAgent, orgType: "PUA");
			var profitShare = TestObjectCreator.CreateProfitShare(relationship2, 20, 50, "AU", "US", "ALL", orgOverride: pickupAgent, orgType: "PUA");
			AddShareForParty(profitShare, "PIC", 30);

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
			shipment.PickupAgentPK = pickupAgent.PK;

			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 500m;
			charge.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"The Profit Share Setup matching Sending Agent and Pickup Agent should have higher priority than the one matching Sending Agent and Receiving Agent.",
				[
					"SEN|ABIGAS|20.00% of profit of 350.00",
					"RCV|AALSHI|50.00% of profit of 350.00",
					"PIC|ZECTRA|30.00% of profit of 350.00",
				],
				calculator.CreateProfitShares());
		}

		public void TestGetProfitShareAgreementForShipment_Priority_SendingAgentVsReceivingAgent()
		{
			// Receiving agent < Sending agent
			// PSA/PSS #1 with matching sending agent, blank receiving agent, blank Pickup Agent
			// PSA/PSS #2 with blank Sending agent, matching Receiving Agent, blank pickup Agent
			// PSA/PSS #1 wins

			var sendingAgent = TestObjectCreator.ABIGAS;
			var receivingAgent = TestObjectCreator.AALSHI;
			var pickupAgent = TestObjectCreator.ZECTRA;

			var relationship1 = TestObjectCreator.CreateAgentRelationship(sendingAgent, null);
			TestObjectCreator.CreateProfitShare(relationship1, 10, 90, "AU", "US", "ALL", orgOverride: null, orgType: null);

			var relationship2 = TestObjectCreator.CreateAgentRelationship(null, receivingAgent);
			TestObjectCreator.CreateProfitShare(relationship2, 20, 80, "AU", "US", "ALL", orgOverride: null, orgType: null);

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
			shipment.PickupAgentPK = pickupAgent.PK;

			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 500m;
			charge.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares(
				"The Profit Share Setup matching Sending Agent and Pickup Agent should have higher priority than the one matching Sending Agent and Receiving Agent.",
				[
					"SEN|ABIGAS|10.00% of profit of 350.00",
					"RCV|AALSHI|90.00% of profit of 350.00",
				],
				calculator.CreateProfitShares());
		}

		public void TestCreateProfitShares_OrganisationHasMoreRoles_ShouldCreateProfitShareForEachRole()
		{
			var receivingAgent = TestObjectCreator.AALSHI;

			// Sending Agent and Pickup Agent are the same organisation
			var sendingAgent = TestObjectCreator.ABIGAS;
			var pickupAgent = sendingAgent;

			var relationship = TestObjectCreator.CreateAgentRelationship(sendingAgent, receivingAgent);
			var profitShare = TestObjectCreator.CreateProfitShare(relationship, "AU", "US", "ALL", orgOverride: pickupAgent, "PUA");

			var sendParty = profitShare.PartyDetails.AddNew();
			sendParty.PS_PartyType = "SEN";
			sendParty.PS_PartyProfitSharePercent = 10;

			var rcvParty = profitShare.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 20;

			var picParty = profitShare.PartyDetails.AddNew();
			picParty.PS_PartyType = "PIC";
			picParty.PS_PartyProfitSharePercent = 30;

			Factory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
			shipment.PickupAgentPK = pickupAgent.PK;

			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_LocalSellAmt = 500m;
			charge.JR_LocalCostAmt = 150m;

			var calculator = new ProfitShareCalculator(Factory, shipment);
			AssertProfitShares("Profit shares should be calculated for all parties even if they are the same organisation.",
				[
					"SEN|ABIGAS|10.00% of profit of 350.00",
					"PIC|ABIGAS|30.00% of profit of 350.00",
					"RCV|AALSHI|20.00% of profit of 350.00"
				],
				calculator.CreateProfitShares());
		}

		static void AssertProfitShares(string message, IEnumerable<string> expectedResult, ProfitShareDetailCollection actualResult)
		{
			var profitSharesToStrings = actualResult.Select(x =>
			{
				var partyType = x.ProfitShareShipmentDetails[0].PartyType;
				var partyName = x.ProfitShareShipmentDetails[0].ProfitShareParty.OH_Code;
				var profitShareDescription = x.ProfitShareShipmentDetails[0].ProfitShareCharges[0].Description;

				return $"{partyType}|{partyName}|{profitShareDescription}";
			});

			AssertContainsExactElementsInAnyOrder(message, expectedResult, profitSharesToStrings);
		}

		public void TestCreateAgencyProfitShareForOrganisationWithNulls()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			IJobInvoicingPlugIn shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			ProfitShareCalculator calculator = new ProfitShareCalculator(Factory, shipment);
			OrgHeader agreementOrganisation = null;
			ZString partyType = ZString.Empty;
			ProfitShareDetailCollection profitShares = calculator.CreateProfitShares();
			OrgHeader creditorOrganization = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			AssertNoExceptionThrown(() => calculator.CreateAgencyProfitShareForOrganisation(shipment, agreementOrganisation, partyType, profitShares, creditorOrganization, controllingCustomer));
		}

		public void TestProfitShareAgreementWithClientSpecificRules_GetProfitShareAgreementForShipment()
		{
			OrgHeader agent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader agent2 = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = agent2.PK;

			var orgOverride1 = TestObjectCreator.ZECTRA;
			var orgOverride2 = TestObjectCreator.XLINDU;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.SetDefaultSendingForwarderAddress(agent1);
			consol.SetDefaultReceivingForwarderAddress(agent2);
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_ExportBroker = orgOverride1.PK;
			shipment.JS_OH_ImportBroker = orgOverride2.PK;
			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var agentRelationship = TestObjectCreator.CreateAgentRelationship(agent1, agent2);
			var profitShare1 = TestObjectCreator.CreateProfitShare(agentRelationship, 50, 50, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "US", testJob.PlugInData.InvoicingSupporter.ContainerMode, orgOverride1, OrgProfitShareDetailsLookups.OrgOverrideTypesList.EBR.Code);
			var profitShare2 = TestObjectCreator.CreateProfitShare(agentRelationship, 20, 80, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "US", testJob.PlugInData.InvoicingSupporter.ContainerMode, orgOverride2, OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);

			Charge existingCharge = testJob.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_LocalCostAmt = 150m;

			Factory.Save();

			ProfitShareCalculator calc = new ProfitShareCalculator(Factory, shipment);
			ProfitShareDetailCollection profitShares = calc.CreateProfitShares();
			AssertEquals("profitShares.Count", 1, profitShares.Count);
			AssertEquals("ProfitShareShipmentDetails.Count", 1, profitShares[0].ProfitShareShipmentDetails.Count);
			AssertEquals("Profit share agreement should be for import broker as it has higher priority than export broker.", profitShare2, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareAgreement);
			AssertEquals(0.2m * 350m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
		}

		public void TestProfitShareAgreementWithClientSpecificRules_CreateAgencyProfitShareForOrganisation()
		{
			OrgHeader agent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader agent2 = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = agent2.PK;

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			var orgOverride1 = TestObjectCreator.ZECTRA;
			var orgOverride2 = TestObjectCreator.XLINDU;

			var consol = TestObjectCreator.CreateConsol("AUSYD", "USLAX", "C001");
			consol.SetDefaultSendingForwarderAddress(agent1);
			consol.SetDefaultReceivingForwarderAddress(agent2);
			var shipment = TestObjectCreator.CreateShipment("S001", "AUSYD", "USLAX", consol);
			shipment.JS_OH_ExportBroker = orgOverride1.PK;
			shipment.JS_OH_ImportBroker = orgOverride2.PK;
			((IDocAddresses)shipment).DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);
			((IDocAddresses)shipment).DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			Assert("Precondition: Shipment is an export", shipment.IsExport());

			var testJob = TestObjectCreator.CreateJob(shipment, false);

			var agentRelationship = TestObjectCreator.CreateAgentRelationship(controllingAgent, null, OrgAgentRelationship.ProfitShareTypes.AgencyProfile);
			var profitShare1 = TestObjectCreator.CreateProfitShare(agentRelationship, 50, 50, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "US", testJob.PlugInData.InvoicingSupporter.ContainerMode, orgOverride1, OrgProfitShareDetailsLookups.OrgOverrideTypesList.EBR.Code);
			var profitShare2 = TestObjectCreator.CreateProfitShare(agentRelationship, 20, 80, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "US", testJob.PlugInData.InvoicingSupporter.ContainerMode, orgOverride2, OrgProfitShareDetailsLookups.OrgOverrideTypesList.IBR.Code);

			Charge existingCharge = testJob.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_LocalCostAmt = 150m;

			Factory.Save();

			ProfitShareCalculator calc = new ProfitShareCalculator(Factory, shipment);
			ProfitShareDetailCollection profitShares = calc.CreateProfitShares();
			AssertEquals("profitShares.Count", 1, profitShares.Count);
			AssertEquals("ProfitShareShipmentDetails.Count", 1, profitShares[0].ProfitShareShipmentDetails.Count);
			AssertEquals("Profit share agreement should be for import broker as it has higher priority than export broker.", profitShare2, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareAgreement);
		}

		public void TestGivenProfitShareProfileForControllingAgent_WhenCreateProfitShares_ThenControllingAgentAndHeadOfficeShouldBeCalculated()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch1.GB_OH_OrgProxy = agent1.PK;

			var branch2 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch2.GB_OH_OrgProxy = agent2.PK;

			var branch3 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch3.GB_OH_OrgProxy = agent3.PK;

			var branch4 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch4.GB_OH_OrgProxy = controllingAgent.PK;

			var branch5 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch5.GB_OH_OrgProxy = controllingCustomer.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(agent1);
			consol.SetDefaultReceivingForwarderAddress(agent2);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);
			shipment.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var existingCharge = job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_LocalCostAmt = 150m;

			CreateProfitShareProfile(agent1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 30m);
			CreateProfitShareProfile(agent2, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 18m);
			var controllingPartyRelationship = CreateProfitShareProfile(controllingAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 10m);
			controllingPartyRelationship.O3_OH_GroupNetworkOrFranchise = agent3.PK;
			var headOfficeDetail = controllingPartyRelationship.GenericProfitShareDetails[0].PartyDetails.AddNew();
			headOfficeDetail.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;
			headOfficeDetail.PS_PartyProfitSharePercent = 5m;

			Factory.Save();

			var calc = new ProfitShareCalculator(Factory, shipment);
			var profitShares = calc.CreateProfitShares();
			AssertEquals(4, profitShares.Count);
			AssertEquals(0.1m * 350m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals(0.05m * 350m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals(0.3m * 350m, profitShares[2].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals(0.18m * 350m, profitShares[3].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
		}

		public void TestGivenProfitShareProfileForControllingCustomer_WhenCreateProfitShares_ThenControllingAgentAndHeadOfficeShouldNotBeCalculated()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch1.GB_OH_OrgProxy = agent1.PK;

			var branch2 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch2.GB_OH_OrgProxy = agent2.PK;

			var branch3 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch3.GB_OH_OrgProxy = agent3.PK;

			var branch4 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch4.GB_OH_OrgProxy = controllingAgent.PK;

			var branch5 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch5.GB_OH_OrgProxy = controllingCustomer.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(agent1);
			consol.SetDefaultReceivingForwarderAddress(agent2);
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);
			shipment.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var existingCharge = job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_LocalCostAmt = 150m;

			CreateProfitShareProfile(agent1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 30m);
			CreateProfitShareProfile(agent2, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 18m);
			var controllingPartyRelationship = CreateProfitShareProfile(controllingCustomer, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 10m);
			controllingPartyRelationship.O3_OH_GroupNetworkOrFranchise = agent3.PK;
			var headOfficeDetail = controllingPartyRelationship.GenericProfitShareDetails[0].PartyDetails.AddNew();
			headOfficeDetail.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;
			headOfficeDetail.PS_PartyProfitSharePercent = 5m;

			Factory.Save();

			var calc = new ProfitShareCalculator(Factory, shipment);
			var profitShares = calc.CreateProfitShares();
			AssertEquals(2, profitShares.Count);
			AssertEquals(0.3m * 350m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals(0.18m * 350m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
		}

		public void TestProfitShareCalculation_Agency_HeadOfficeRedirection()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareRedirectHeadOfficeIfExternal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader agent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader agent2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader agent4 = Factory.NewWithValidTestData<OrgHeader>();

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(agent1);
			consol.SetDefaultReceivingForwarderAddress(agent2);
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);
			shipment.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			Charge existingCharge = job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_LocalCostAmt = 150m;

			CreateProfitShareProfile(agent1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 30m);
			CreateProfitShareProfile(agent2, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 18m);
			OrgAgentRelationship controllingPartyRelationship = CreateProfitShareProfile(controllingAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 10m);
			controllingPartyRelationship.O3_OH_GroupNetworkOrFranchise = agent4.PK;
			OrgProfitShareParty headOfficeDetail = controllingPartyRelationship.GenericProfitShareDetails[0].PartyDetails.AddNew();
			headOfficeDetail.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;
			headOfficeDetail.PS_PartyProfitSharePercent = 5m;

			Factory.Save();

			ProfitShareCalculator calc = new ProfitShareCalculator(Factory, shipment);
			ProfitShareDetailCollection profitShares = calc.CreateProfitShares();
			AssertEquals(4, profitShares.Count);
			AssertEquals("Controlling Agent", 0.1m * 350m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Controlling Agent - Total Profit Printed", 0.1m * 350m, ProfitShareInLocalCurrency(profitShares[0]));

			AssertEquals("Controlling Agent - Head Office Redirection", 0.05m * 350m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Controlling Agent - Head Office Redirection - Total Profit Printed", 0.05m * 350m, ProfitShareInLocalCurrency(profitShares[1]));

			AssertEquals("Sending Agent", 0.3m * 350m, profitShares[2].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Sending Agent - Total Profit Printed", 0.3m * 350m, ProfitShareInLocalCurrency(profitShares[2]));

			AssertEquals("Receiving Agent", 0.18m * 350m, profitShares[3].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Receiving Agent - Total Profit Printed", 0.18m * 350m, ProfitShareInLocalCurrency(profitShares[3]));
		}

		public void TestProfitShareCalculation_ProxyOrg()
		{
			OrgHeader agent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader agent2 = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = agent2.PK;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(agent1);
			consol.SetDefaultReceivingForwarderAddress(agent2);
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			Charge existingCharge = job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 500m;
			existingCharge.JR_LocalCostAmt = 150m;

			CreateProfitShareProfile(agent1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 30m);
			CreateProfitShareProfile(agent2, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 18m);

			Factory.Save();

			ProfitShareCalculator calc = new ProfitShareCalculator(Factory, shipment);
			ProfitShareDetailCollection profitShares = calc.CreateProfitShares();
			AssertEquals(1, profitShares.Count);
			AssertEquals(0.3m * 350m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
		}

		OrgAgentRelationship CreateProfitShareProfile(OrgHeader agent, string role, decimal percent)
		{
			OrgAgentRelationship agentProfile = Factory.LoadTop1<OrgAgentRelationship>(new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agent.PK));
			if (agentProfile == null)
			{
				agentProfile = Factory.New<OrgAgentRelationship>();
				agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
				agentProfile.O3_OH_SendingAgent = agent.PK;
			}

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.Count == 1 ? agentProfile.GenericProfitShareDetails[0] : agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";
			profitShareAgreement.O4_SendingPortOrCountry = "AU";

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = role;
			party.PS_PartyProfitSharePercent = percent;
			return agentProfile;
		}

		public void TestProfitShareCalculation_Standard()
		{
			#region Setup

			// Setup Orgs
			OrgHeader deliveryAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader deliveryAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader headOffice = Factory.NewWithValidTestData<OrgHeader>();

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment1.DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer).E2_OA_Address = controllingCustomer.MainAddress.PK;
			shipment1.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			consol.Shipments.Add(shipment1);

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = deliveryAgent2.PK;
			shipment2.DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer).E2_OA_Address = controllingCustomer.MainAddress.PK;
			shipment2.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			consol.Shipments.Add(shipment2);

			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment3.DocAddresses.CreateWithAddressType(DocAddressType.ControllingCustomer).E2_OA_Address = controllingCustomer.MainAddress.PK;
			shipment3.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);
			consol.Shipments.Add(shipment3);

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship1.O3_OH_ReceivingAgent = deliveryAgent1.PK;
			agentRelationship1.O3_OH_GroupNetworkOrFranchise = headOffice.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";
			profitShare1.O4_OH_ControllingAgent = controllingCustomer.PK;

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 66m;

			OrgProfitShareParty sndParty = profitShare1.PartyDetails.AddNew();
			sndParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sndParty.PS_PartyProfitSharePercent = 15m;

			OrgProfitShareParty headOfficeParty = profitShare1.PartyDetails.AddNew();
			headOfficeParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;
			headOfficeParty.PS_PartyProfitSharePercent = 10m;

			OrgProfitShareParty ctrlAgentParty = profitShare1.PartyDetails.AddNew();
			ctrlAgentParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent;
			ctrlAgentParty.PS_PartyProfitSharePercent = 9m;

			OrgAgentRelationship agentRelationship2 = Factory.New<OrgAgentRelationship>();
			agentRelationship2.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship2.O3_OH_ReceivingAgent = deliveryAgent2.PK;
			OrgProfitShareDetails profitShare2 = agentRelationship2.ProfitShareDetails.AddNew();
			profitShare2.O4_FreightMode = "ALL";
			profitShare2.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare2.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare2.O4_SendingPortOrCountry = "AUSYD";
			profitShare2.O4_ReceivingPortOrCountry = "USLAX";
			profitShare2.O4_OH_ControllingAgent = controllingCustomer.PK;

			rcvParty = profitShare2.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 75m;

			Factory.Save();

			// Jobs
			Job job1 = Job.CreateWithMutex(Factory, shipment1);
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_JobNum = shipment1.JS_UniqueConsignRef;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 300m;
			job1Charge1.JR_AgentDeclaredCostAmt = 140m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = Job.CreateWithMutex(Factory, shipment2);
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_JobNum = shipment2.JS_UniqueConsignRef;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			Job job3 = Job.CreateWithMutex(Factory, shipment3);
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job3.JH_GB = GlbBranch.CurrentBranch.PK;
			job3.JH_JobNum = shipment3.JS_UniqueConsignRef;
			Charge job3Charge1 = job3.Charges.AddNew();
			job3Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job3Charge1.JR_AgentDeclaredSellAmt = 250m;
			job3Charge1.JR_AgentDeclaredCostAmt = 100m;
			job3Charge1.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			#endregion

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("5 profit shares calculated", 5, profitShares.Count);

			AssertEquals("Dlv Agent 1", deliveryAgent1.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Dlv Agent 1", 204.6m, TotalAssumingSingleCurrency(profitShares[0]));

			AssertEquals("Sending Agent", sendingAgent.PK, profitShares[1].ProfitShareParty.PK);
			AssertEquals("Sending Agent", 46.5m, TotalAssumingSingleCurrency(profitShares[1]));

			AssertEquals("Head Office", headOffice.PK, profitShares[2].ProfitShareParty.PK);
			AssertEquals("Head Office", 31m, TotalAssumingSingleCurrency(profitShares[2]));

			AssertEquals("Controlling Agent", controllingAgent.PK, profitShares[3].ProfitShareParty.PK);
			AssertEquals("Controlling Agent", 27.9m, TotalAssumingSingleCurrency(profitShares[3]));

			AssertEquals("Dlv Agent 2", deliveryAgent2.PK, profitShares[4].ProfitShareParty.PK);
			AssertEquals("Dlv Agent 2", 127.5m, TotalAssumingSingleCurrency(profitShares[4]));

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = sendingAgent.PK;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = headOffice.PK;
			profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("3 profit shares calculated as Sending agent and Head Office are org proxies", 3, profitShares.Count);

			AssertEquals("Dlv Agent 1", deliveryAgent1.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Dlv Agent 1", 204.6m, TotalAssumingSingleCurrency(profitShares[0]));

			AssertEquals("Controlling Agent", controllingAgent.PK, profitShares[1].ProfitShareParty.PK);
			AssertEquals("Controlling Agent", 27.9m, TotalAssumingSingleCurrency(profitShares[1]));

			AssertEquals("Dlv Agent 2", deliveryAgent2.PK, profitShares[2].ProfitShareParty.PK);
			AssertEquals("Dlv Agent 2", 127.5m, TotalAssumingSingleCurrency(profitShares[2]));
		}

		public void TestProfitShareCalculation_Standard_SendingAgentIsSameAsControllingAgent()
		{
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var headOffice = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = sendingAgent;

			AssertProfitShareCalculation_Standard(receivingAgent, sendingAgent, headOffice, controllingAgent, (profitShares) =>
			{
				AssertEquals("4 profit shares calculated", 4, profitShares.Count);

				AssertEquals("Party: Receiving Agent", receivingAgent.PK, profitShares[0].ProfitShareParty.PK);
				AssertEquals("Total Profit", 217.8m, TotalAssumingSingleCurrency(profitShares[0]));
				AssertEquals("Total Profit Printed", 217.8m, ProfitShareInLocalCurrency(profitShares[0]));

				AssertEquals("Party: Sending Agent", sendingAgent.PK, profitShares[1].ProfitShareParty.PK);
				AssertEquals("Total Profit", 49.5m, TotalAssumingSingleCurrency(profitShares[1]));
				AssertEquals("Total Profit Printed", 49.5m, ProfitShareInLocalCurrency(profitShares[1]));

				AssertEquals("Party: Head Office", headOffice.PK, profitShares[2].ProfitShareParty.PK);
				AssertEquals("Total Profit", 33m, TotalAssumingSingleCurrency(profitShares[2]));
				AssertEquals("Total Profit Printed", 33m, ProfitShareInLocalCurrency(profitShares[2]));

				AssertEquals("Party: Controlling Agent", controllingAgent.PK, profitShares[3].ProfitShareParty.PK);
				AssertEquals("Total Profit", 29.7m, TotalAssumingSingleCurrency(profitShares[3]));
				AssertEquals("Total Profit Printed", 29.7m, ProfitShareInLocalCurrency(profitShares[3]));
			});
		}

		public void TestProfitShareCalculation_Standard_ReceivingAgentIsSameAsControllingAgent()
		{
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var headOffice = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = receivingAgent;

			AssertProfitShareCalculation_Standard(receivingAgent, sendingAgent, headOffice, controllingAgent, (profitShares) =>
			{
				AssertEquals("4 profit shares calculated", 4, profitShares.Count);

				AssertEquals("Party: ReceivingAgent", receivingAgent.PK, profitShares[0].ProfitShareParty.PK);
				AssertEquals("Total Profit", 217.8m, TotalAssumingSingleCurrency(profitShares[0]));
				AssertEquals("Total Profit Printed", 217.8m, ProfitShareInLocalCurrency(profitShares[0]));

				AssertEquals("Party: Sending Agent", sendingAgent.PK, profitShares[1].ProfitShareParty.PK);
				AssertEquals("Total Profit", 49.5m, TotalAssumingSingleCurrency(profitShares[1]));
				AssertEquals("Total Profit Printed", 49.5m, ProfitShareInLocalCurrency(profitShares[1]));

				AssertEquals("Party: Head Office", headOffice.PK, profitShares[2].ProfitShareParty.PK);
				AssertEquals("Total Profit", 33m, TotalAssumingSingleCurrency(profitShares[2]));
				AssertEquals("Total Profit Printed", 33m, ProfitShareInLocalCurrency(profitShares[2]));

				AssertEquals("Party: Controlling Agent", controllingAgent.PK, profitShares[3].ProfitShareParty.PK);
				AssertEquals("Total Profit", 29.7m, TotalAssumingSingleCurrency(profitShares[3]));
				AssertEquals("Total Profit Printed", 29.7m, ProfitShareInLocalCurrency(profitShares[3]));
			});
		}

		public void TestProfitShareCalculation_Standard_WhenOnePartyPlayingAllRole()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			AssertProfitShareCalculation_Standard(org, org, org, org, (profitShares) =>
			{
				AssertEquals("4 profit share calculated", 4, profitShares.Count);

				AssertEquals("Party", org.PK, profitShares[0].ProfitShareParty.PK);
				AssertEquals("Total Profit Printed", 217.8m, ProfitShareInLocalCurrency(profitShares[0]));

				AssertEquals("Party", org.PK, profitShares[1].ProfitShareParty.PK);
				AssertEquals("Total Profit Printed", 49.5m, ProfitShareInLocalCurrency(profitShares[1]));

				AssertEquals("Party", org.PK, profitShares[2].ProfitShareParty.PK);
				AssertEquals("Total Profit Printed", 33m, ProfitShareInLocalCurrency(profitShares[2]));

				AssertEquals("Party", org.PK, profitShares[3].ProfitShareParty.PK);
				AssertEquals("Total Profit Printed", 29.7m, ProfitShareInLocalCurrency(profitShares[3]));
			});
		}

		void AssertProfitShareCalculation_Standard(OrgHeader receivingAgent, OrgHeader sendingAgent, OrgHeader headOffice, OrgHeader controllingAgent, Action<ProfitShareDetailCollection> assert)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);

			CreateShipmentAndCharge(300m, 140m);// 160m as profit
			CreateShipmentAndCharge(190m, 20m);// 170m as profit

			var profitShare = CreatePartyDetails();
			CreateProfitShareByPartyType(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 66m);//66% of 330m = 217.8m
			CreateProfitShareByPartyType(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 15m);//15% of 330m = 49.5m
			CreateProfitShareByPartyType(OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor, 10m);//10% of 330m = 33m
			CreateProfitShareByPartyType(OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 9m);//9% of 330m = 29.7m

			Factory.Save();

			assert(new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares());

			void CreateShipmentAndCharge(ZDecimal sellAmount, ZDecimal costAmount)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
				shipment.DocAddresses.AddNew(controllingAgent.MainAddress, DocAddressType.ControllingAgent);

				using (var job = Job.CreateWithMutex(Factory, shipment))
				{
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					job.JH_GB = GlbBranch.CurrentBranch.PK;
					job.JH_JobNum = shipment.JS_UniqueConsignRef;

					var job1Charge = job.Charges.AddNew();
					job1Charge.JR_AC = Env.Registry.FreightChargeCode;
					job1Charge.JR_AgentDeclaredSellAmt = sellAmount;
					job1Charge.JR_AgentDeclaredCostAmt = costAmount;
					job1Charge.JR_IsIncludedInProfitShare = true;
				}
			}

			OrgProfitShareDetails CreatePartyDetails()
			{
				var agentRelationship = Factory.New<OrgAgentRelationship>();
				agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
				agentRelationship.O3_OH_ReceivingAgent = receivingAgent.PK;
				agentRelationship.O3_OH_GroupNetworkOrFranchise = headOffice.PK;

				var profitShareDetails = agentRelationship.ProfitShareDetails.AddNew();
				profitShareDetails.O4_FreightMode = "AIR";
				profitShareDetails.O4_StartDate = ZDateTime.Today.AddDays(-3);
				profitShareDetails.O4_EndDate = ZDateTime.Today.AddDays(30);
				profitShareDetails.O4_SendingPortOrCountry = "AUSYD";
				profitShareDetails.O4_ReceivingPortOrCountry = "USLAX";

				return profitShareDetails;
			}

			OrgProfitShareParty CreateProfitShareByPartyType(string partyType, ZDecimal partyProfitSharePercent)
			{
				var party = profitShare.PartyDetails.AddNew();
				party.PS_PartyType = partyType;
				party.PS_PartyProfitSharePercent = partyProfitSharePercent;

				return party;
			}
		}

		public void TestProfitShareCalculationWhenShipmentHasNoJob()
		{
			#region Setup

			TestObjectCreator creator = new TestObjectCreator(Factory);

			// Setup Orgs
			OrgHeader deliveryAgent = creator.CreateOrgHeader("DA", true, true);

			// Setup consol & shipments
			var consol = creator.CreateConsol("AUSYD", "USLAX", "C1");
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			var shipment1 = creator.CreateShipment("S0001", "AUSYD", "USLAX", consol);
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var shipment2 = creator.CreateShipment("S0002", "AUSYD", "USLAX", consol);
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_OH_DeliveryAgent = deliveryAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = deliveryAgent.PK;
			OrgProfitShareDetails profitShare = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare.O4_FreightMode = "AIR";
			profitShare.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare.O4_SendingPortOrCountry = "AUSYD";
			profitShare.O4_ReceivingPortOrCountry = "USLAX";

			OrgProfitShareParty rcvParty = profitShare.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 66m;

			Factory.Save();

			var job1 = creator.CreateJob(shipment1);
			var job1Charge1 = creator.CreateCharge(job1, creator.CC1, "desc", creator.AUD, 140m, creator.AALSHI, creator.AUD, 300m, creator.ABIGAS);
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			#endregion

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Profit Shares Created", 1, profitShares.Count);
			AssertEquals("When calculating profit shares, shipment with no job doesn't get taken into account", 1, profitShares[0].ProfitShareShipmentDetails.Count);
			AssertEquals("Profit share created belong to Job1", job1.PK, profitShares[0].ProfitShareShipmentDetails[0].Job.PK);
		}

		public void TestProfitShareCalculation_UnrelevantPartiesShouldBeIgnored()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			consol.SetDefaultReceivingForwarderAddress(receivingAgent);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
			controllingAgentAddress.OrganisationPK = controllingAgent.PK;

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = receivingAgent.PK;

			var profitShare = agentRelationship.ProfitShareDetails.AddNew();
			profitShare.O4_FreightMode = "ALL";
			profitShare.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare.O4_SendingPortOrCountry = "AUSYD";
			profitShare.O4_ReceivingPortOrCountry = "USLAX";

			var party1 = profitShare.PartyDetails.AddNew();
			party1.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			party1.PS_PartyProfitSharePercent = 50m;

			var party2 = profitShare.PartyDetails.AddNew();
			party2.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			party2.PS_PartyProfitSharePercent = 50m;

			Factory.Save();
			using (var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				var existingCharge = job.Charges.AddNew();
				existingCharge.JR_AC = Env.Registry.FreightChargeCode;
				existingCharge.JR_IsIncludedInProfitShare = true;
				existingCharge.JR_LocalSellAmt = 1000m;
				existingCharge.JR_LocalCostAmt = 200m;

				var calc = new ProfitShareCalculator(Factory, shipment);
				var profitShares = calc.CreateProfitShares();
				AssertEquals("Controlling Agent should be ignored as it's not defined in profit share agreement", 2, profitShares.Count);
				AssertEquals(0.5m * 800m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
				AssertEquals(0.5m * 800m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			}
		}

		public void TestProfitShareCalculation_AllPartiesWithinSameOrgProxy()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var branch1 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch1.GB_OH_OrgProxy = sendingAgent.PK;

			var branch2 = GlbCompany.CurrentCompany.Branches.AddNew();
			branch2.GB_OH_OrgProxy = receivingAgent.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			consol.SetDefaultReceivingForwarderAddress(receivingAgent);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship.O3_OH_ReceivingAgent = receivingAgent.PK;

			var profitShare = agentRelationship.ProfitShareDetails.AddNew();
			profitShare.O4_FreightMode = "ALL";
			profitShare.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare.O4_SendingPortOrCountry = "AUSYD";
			profitShare.O4_ReceivingPortOrCountry = "USLAX";

			var party1 = profitShare.PartyDetails.AddNew();
			party1.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			party1.PS_PartyProfitSharePercent = 50m;

			var party2 = profitShare.PartyDetails.AddNew();
			party2.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			party2.PS_PartyProfitSharePercent = 50m;

			Factory.Save();
			using (var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly())
			{
				var existingCharge = job.Charges.AddNew();
				existingCharge.JR_AC = Env.Registry.FreightChargeCode;
				existingCharge.JR_IsIncludedInProfitShare = true;
				existingCharge.JR_LocalSellAmt = 500m;
				existingCharge.JR_LocalCostAmt = 100m;

				var calc = new ProfitShareCalculator(Factory, shipment);
				var profitShares = calc.CreateProfitShares();
				AssertEquals("Both profit shares should be created since both parties are within same proxy as current company", 2, profitShares.Count);
				AssertEquals("Expected to be half of 400 as it's 50 percent of profit", 200m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
				AssertEquals("Expected to be half of 400 as it's 50 percent of profit", 200m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			}
		}

		ZDecimal TotalAssumingSingleCurrency(ProfitShareDetail detail)
		{
			ZDecimal result = 0m;

			foreach (ProfitShareShipmentDetail shipDetail in detail.ProfitShareShipmentDetails)
			{
				if (shipDetail.ProfitShareCharges.Count > 1)
				{
					throw new InvalidOperationException("This method only works when charges are not created PER currency.");
				}

				result += shipDetail.ProfitShareCharges[0].ProfitShare ?? ZDecimal.Zero;
			}

			return result;
		}

		public void TestProfitShareCalculation_StandardAgreementProfile_AgentShouldBeRetrievedFromSourceConsol()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			var agent4 = Factory.NewWithValidTestData<OrgHeader>();
			var agent5 = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "NZAKL";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			consol1.SetDefaultSendingForwarderAddress(agent1);
			consol1.SetDefaultReceivingForwarderAddress(agent2);
			consol2.SetDefaultSendingForwarderAddress(agent3);
			consol2.SetDefaultReceivingForwarderAddress(agent4);

			var job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var existingCharge = job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = 200m;
			existingCharge.JR_LocalCostAmt = 100m;

			TestObjectCreator.CreateProfitShare(TestObjectCreator.CreateAgentRelationship(agent1, agent2),
				sendingProfitSharePercentage: 60, receivingProfitSharePercentage: 40, "", "", transportMode: "ALL",
				jobType: JobTypesList.Codes.Blank, gatewayAgentType: GatewayAgentTypesList.Codes.Blank);

			TestObjectCreator.CreateProfitShare(TestObjectCreator.CreateAgentRelationship(agent3, agent4),
				sendingProfitSharePercentage: 70, receivingProfitSharePercentage: 30, "", "", transportMode: "ALL",
				jobType: JobTypesList.Codes.Blank, gatewayAgentType: GatewayAgentTypesList.Codes.Blank);

			Factory.Save();

			var profitShares = new ProfitShareCalculator(Factory, consol1, consol1.CostSupporter.ShipmentsList).CreateProfitShares();
			AssertEquals(2, profitShares.Count);
			AssertEquals("Sending Agent is retrieved from consol1", 0.6m * 100m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Sending Agent is retrieved from consol1", agent1.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Receiving Agent is retrieved from consol1 when there is no delivery agent on the shipment", 0.4m * 100m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Receiving Agent is retrieved from consol1 when there is no delivery agent on the shipment", agent2.PK, profitShares[1].ProfitShareParty.PK);

			profitShares = new ProfitShareCalculator(Factory, consol2, consol2.CostSupporter.ShipmentsList).CreateProfitShares();
			AssertEquals(2, profitShares.Count);
			AssertEquals("Sending Agent is retrieved from consol2", 0.7m * 100m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Sending Agent is retrieved from consol2", agent3.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Receiving Agent is retrieved from consol2 when there is no delivery agent on the shipment", 0.3m * 100m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Receiving Agent is retrieved from consol2 when there is no delivery agent on the shipment", agent4.PK, profitShares[1].ProfitShareParty.PK);

			TestObjectCreator.CreateProfitShare(TestObjectCreator.CreateAgentRelationship(agent1, agent5),
				sendingProfitSharePercentage: 80, receivingProfitSharePercentage: 20, "", "", transportMode: "ALL",
				jobType: JobTypesList.Codes.Blank, gatewayAgentType: GatewayAgentTypesList.Codes.Blank);

			TestObjectCreator.CreateProfitShare(TestObjectCreator.CreateAgentRelationship(agent3, agent5),
				sendingProfitSharePercentage: 90, receivingProfitSharePercentage: 10, "", "", transportMode: "ALL",
				jobType: JobTypesList.Codes.Blank, gatewayAgentType: GatewayAgentTypesList.Codes.Blank);

			shipment.JS_OH_DeliveryAgent = agent5.PK;

			Factory.Save();

			profitShares = new ProfitShareCalculator(Factory, consol1, consol1.CostSupporter.ShipmentsList).CreateProfitShares();
			AssertEquals(2, profitShares.Count);
			AssertEquals("Sending Agent is retrieved from consol1", 0.8m * 100m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Sending Agent is retrieved from consol1", agent1.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Receiving Agent is retrieved from delivery agent of the shipment", 0.2m * 100m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Receiving Agent is retrieved from delivery agent of the shipment", agent5.PK, profitShares[1].ProfitShareParty.PK);

			profitShares = new ProfitShareCalculator(Factory, consol2, consol2.CostSupporter.ShipmentsList).CreateProfitShares();
			AssertEquals(2, profitShares.Count);
			AssertEquals("Sending Agent is retrieved from consol2", 0.9m * 100m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Sending Agent is retrieved from consol2", agent3.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Receiving Agent is retrieved from delivery agent of the shipment", 0.1m * 100m, profitShares[1].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Receiving Agent is retrieved from delivery agent of the shipment", agent5.PK, profitShares[1].ProfitShareParty.PK);
		}

		#region Gateway Consol

		public void TestCreateStandardAgreementProfitShares_GatewayConsol_LoginToReceivingAgentCompany_SendingAgentIsGTA()
		{
			AssertCreateStandardAgreementProfitShares_GatewayConsol(
				loginCompany: "RCV", sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: "",
				expectedProfitShareShipmentDetailPartyTypes: new[]
				{
					OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent,
					OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor
				},
				expectedSellAmounts: new[] { 500m, -280m, -48m },
				"Original charge 500, and new profit share charges calculated from SGW agreement: 70%*400 = 280 / 18% / 12%*400 = 48"
			);
		}

		public void TestCreateStandardAgreementProfitShares_GatewayConsol_LoginToReceivingAgentCompany_ReceivingAgentIsGTA()
		{
			AssertCreateStandardAgreementProfitShares_GatewayConsol(
				loginCompany: "RCV", sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				expectedProfitShareShipmentDetailPartyTypes: new[]
				{
					OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent,
					OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor
				},
				expectedSellAmounts: new[] { 500m, -240m, -60m },
				"Original charge 500, and new profit share charges calculated from RGW agreement: 60%*400 = 240 / 25% / 15%*400 = 60"
			);
		}

		public void TestCreateStandardAgreementProfitShares_GatewayConsol_LoginToReceivingAgentCompany_BothAgentsAreGTA()
		{
			AssertCreateStandardAgreementProfitShares_GatewayConsol(
				loginCompany: "RCV", sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				expectedProfitShareShipmentDetailPartyTypes: new[]
				{
					OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent,
					OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor
				},
				expectedSellAmounts: new[] { 500m, -320m, -32m },
				"Original charge 500, and new profit share charges calculated from BGW agreement: 80%*400 = 320 / 12% / 8%*400 = 32"
			);
		}

		public void TestCreateStandardAgreementProfitShares_GatewayConsol_LoginToSendingAgentCompany_SendingAgentIsGTA()
		{
			AssertCreateStandardAgreementProfitShares_GatewayConsol(
				loginCompany: "SEN", sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: "",
				expectedProfitShareShipmentDetailPartyTypes: new[]
				{
					OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent,
					OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor
				},
				expectedSellAmounts: new[] { 500m, -72m, -48m },
				"Original charge 500, and new profit share charges calculated from SGW agreement: 70% / 18%*400 = 72 / 12%*400 = 48"
			);
		}

		public void TestCreateStandardAgreementProfitShares_GatewayConsol_LoginToSendingAgentCompany_ReceivingAgentIsGTA()
		{
			AssertCreateStandardAgreementProfitShares_GatewayConsol(
				loginCompany: "SEN", sendingAgentStatus: "", receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				expectedProfitShareShipmentDetailPartyTypes: new[]
				{
					OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent,
					OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor
				},
				expectedSellAmounts: new[] { 500m, -100m, -60m },
				"Original charge 500, and new profit share charges calculated from RGW agreement: 60% / 25%*400 = 100 / 15%*400 = 60"
			);
		}

		public void TestCreateStandardAgreementProfitShares_GatewayConsol_LoginToSendingAgentCompany_BothAgentsAreGTA()
		{
			AssertCreateStandardAgreementProfitShares_GatewayConsol(
				loginCompany: "SEN", sendingAgentStatus: AgentStatusList.Codes.GatewayAgent, receivingAgentStatus: AgentStatusList.Codes.GatewayAgent,
				expectedProfitShareShipmentDetailPartyTypes: new[]
				{
					OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent,
					OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor
				},
				expectedSellAmounts: new[] { 500m, -48m, -32m },
				"Original charge 500, and new profit share charges calculated from BGW agreement: 80% / 12%*400 = 48 / 8%*400 = 32"
			);
		}

		void AssertCreateStandardAgreementProfitShares_GatewayConsol(
			string loginCompany, string sendingAgentStatus, string receivingAgentStatus,
			IEnumerable<string> expectedProfitShareShipmentDetailPartyTypes,
			IEnumerable<decimal> expectedSellAmounts,
			string reason)
		{
			// score profit shares to revenue amounts
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			const string origin = "AUSYD";
			const string destination = "NZAKL";

			GlbCompany setupGatewayEntities(string code, string portOrCountry, bool useLoginCompany = false)
			{
				var newAgent = TestObjectCreator.CreateOrgHeader(code + "ORG", true, false);
				var newCompany = useLoginCompany ? GlbCompany.CurrentCompany : TestObjectCreator.CreateNewCompany(code, orgProxy: newAgent);
				if (useLoginCompany)
				{
					newCompany.GC_OH_OrgProxy = newAgent.PK;
				}
				var newBranch = TestObjectCreator.CreateNewBranch(newCompany, code);

				var appPort = newAgent.AppointedGatewayAgentPorts.AddNew();
				appPort.O5_OA_AgentOfficeAddress = newAgent.MainAddress.PK;
				appPort.O5_PortOrCountry = portOrCountry;
				appPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				appPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				return newCompany;
			}

			var sendingCompany = setupGatewayEntities("SEN", origin, loginCompany == "SEN");
			var sendingAgent = sendingCompany.OrgProxy;
			var receivingCompany = setupGatewayEntities("RCV", destination, loginCompany == "RCV");
			var receivingAgent = receivingCompany.OrgProxy;
			// Login to HDF company is not a valid case for gateway consol. We won't have Gateway Billing.
			var headOfficeCompany = setupGatewayEntities("HDF", origin);
			var headOffice = headOfficeCompany.OrgProxy;

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(
				sendingGatewayCompany: sendingCompany, sendingGatewayAgent: sendingAgent,
				receivingGatewayCompany: receivingCompany, receivingGatewayAgent: receivingAgent);
			// 3-way relationship: SEN/RCV/HDF
			var agentRelationship = TestObjectCreator.CreateAgentRelationship(sendingAgent, receivingAgent);
			agentRelationship.O3_OH_GroupNetworkOrFranchise = headOffice.PK;

			void AddHeadOfficeFranchisorShare(decimal sharedPercentage, OrgProfitShareDetails agreement)
			{
				var hdfParty = agreement.PartyDetails.AddNew();
				hdfParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.HeadOfficeFranchisor;
				hdfParty.PS_PartyProfitSharePercent = sharedPercentage;
			}

			// 90/6/4 split
			var agreementBlank = TestObjectCreator.CreateProfitShare(agentRelationship,
				sendingProfitSharePercentage: 90, receivingProfitSharePercentage: 6, origin, destination, transportMode: "ALL",
				jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.Blank);
			AddHeadOfficeFranchisorShare(sharedPercentage: 4, agreementBlank);
			// 80/12/8 split
			var agreementBGW = TestObjectCreator.CreateProfitShare(agentRelationship,
				sendingProfitSharePercentage: 80, receivingProfitSharePercentage: 12, origin, destination, transportMode: "ALL",
				jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.BGW);
			AddHeadOfficeFranchisorShare(sharedPercentage: 8, agreementBGW);
			// 70/18/12 split
			var agreementSGW = TestObjectCreator.CreateProfitShare(agentRelationship,
				sendingProfitSharePercentage: 70, receivingProfitSharePercentage: 18, origin, destination, transportMode: "ALL",
				jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.SGW);
			AddHeadOfficeFranchisorShare(sharedPercentage: 12, agreementSGW);
			// 60/25/15 split
			var agreementRGW = TestObjectCreator.CreateProfitShare(agentRelationship,
				sendingProfitSharePercentage: 60, receivingProfitSharePercentage: 25, origin, destination, transportMode: "ALL",
				jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.RGW);
			AddHeadOfficeFranchisorShare(sharedPercentage: 15, agreementRGW);

			Factory.Save();

			var profitShareCalculator = new ProfitShareCalculator(Factory, new[] { gatewayConsol });
			using (var consolJob = new Job.Loader(gatewayConsol).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				consolJob.Charges.RemoveAndDeleteAll();
				var existingCharge = consolJob.Charges.AddNew();
				existingCharge.JR_AC = Env.Registry.FreightChargeCode;
				existingCharge.JR_IsIncludedInProfitShare = true;
				// profit = sell - cost = 400
				existingCharge.JR_LocalSellAmt = 500;
				existingCharge.JR_LocalCostAmt = 100;

				gatewayConsol.JK_SendingForwarderHandlingType = sendingAgentStatus;
				gatewayConsol.JK_ReceivingForwarderHandlingType = receivingAgentStatus;
				var profitShares = profitShareCalculator.CreateProfitShares();
				var details = profitShares.OfType<ProfitShareDetail>().SelectMany(x => x.ProfitShareShipmentDetails);

				AssertEquals(2, details.Count());
				AssertContainsExactElementsInAnyOrder("Should be party types of other companies (not the current one) in the relationship.",
					expectedProfitShareShipmentDetailPartyTypes, details.OfType<ProfitShareShipmentDetail>().Select(x => x.PartyType));

				var profitShareChargeCreator = new ProfitShareShipmentChargeCreator(profitShares, consolJob, false);
				profitShareChargeCreator.CreateCharges();
				AssertContainsExactElementsInAnyOrder(reason, expectedSellAmounts, consolJob.Charges.OfType<Charge>().Select(x => (decimal)x.JR_LocalSellAmt));
			}
		}
		#endregion

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		ZDecimal ProfitShareInLocalCurrency(ProfitShareDetail detail)
		{
			ZDecimal result = 0m;

			foreach (ProfitShareShipmentDetail shipDetail in detail.ProfitShareShipmentDetails)
			{
				if (shipDetail.ProfitShareCharges.Count > 1)
				{
					throw new InvalidOperationException("This method only works when charges are not created PER currency.");
				}

				result += shipDetail?.ProfitShareInLocalCurrencyForPrinting ?? ZDecimal.Zero;
			}

			return result;
		}
	}
}
