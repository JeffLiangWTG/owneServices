using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	[TestedType(typeof(ProfitShareShipmentDetail))]
	public class ProfitShareShipmentDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProfitForJobsAndGrossRevenue()
		{
			OrgHeader someSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(someSendingForwarder);

			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail profitShareShipment = new ProfitShareShipmentDetail(shipment, deliveryAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(profitShareShipment);

			using (Job job = Job.CreateWithMutex(Factory, shipment))
			{
				job.JH_JobNum = "111";
				Charge charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_AgentDeclaredSellAmt = 2000m;
				charge.JR_AgentDeclaredCostAmt = 1000m;
				charge.JR_IsIncludedInProfitShare = true;
				profitShareShipment.SetJobForTesting(job);

				var creator = new TestObjectCreator(Factory);
				OrgAgentRelationship agentRelationship = creator.CreateAgentRelationship(someSendingForwarder, deliveryAgent);
				OrgProfitShareDetails profitShareAgreement = creator.CreateProfitShare(agentRelationship, 55m, 45m, "AUSYD", "USLAX", "AIR");
				profitShareShipment.ProfitShareAgreement = profitShareAgreement;

				Factory.Save();
			}

			AssertEquals(1000m, profitShareShipment.ProfitShareCharges[0].Profit);
			AssertEquals(2000m, profitShareShipment.ProfitShareCharges[0].GrossRevenue);
		}

		public void TestProfitToInvoice()
		{
			OrgHeader someSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(someSendingForwarder);

			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualChargeable = 200m;

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail profitShareShipment = new ProfitShareShipmentDetail(shipment, deliveryAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(profitShareShipment);

			Job job = Job.CreateWithMutex(Factory, shipment);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = "111";
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 2000m;
			charge.JR_AgentDeclaredCostAmt = 1000m;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			profitShareShipment.SetJobForTesting(job);

			AssertNull("Precondition: No profit share agreements exist", profitShareShipment.ProfitShareAgreement);
			AssertEquals("Precondition: No profit share agreements exist, so no profit to invoice", 0, profitShareShipment.ProfitShareCharges.Count);
			AssertNull("ProfitSharePartyDetails", profitShareShipment.ProfitSharePartyDetails);

			var creator = new TestObjectCreator(Factory);
			OrgAgentRelationship agentRelationship = creator.CreateAgentRelationship(someSendingForwarder, deliveryAgent);
			OrgProfitShareDetails profitShareAgreement = creator.CreateProfitShare(agentRelationship, 55m, 45m, "AUSYD", "USLAX", "AIR");
			profitShareShipment.ProfitShareAgreement = profitShareAgreement;

			Factory.Save();

			AssertEquals("Profit to invoice is 45% of total profit", 450m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
			AssertEquals("Agreement", 45m, profitShareShipment.ProfitSharePartyDetails.PS_PartyProfitSharePercent);

			var receivingParty = profitShareAgreement.PartyDetails.Cast<OrgProfitShareParty>().First(x => x.PS_PartyType == "RCV");

			receivingParty.PS_PartyRate = 130m;
			receivingParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee;
			Factory.Save();

			AssertEquals("Profit to invoice is 45% of total profit + $130", 580m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
			AssertEquals("Agreement", 45m, profitShareShipment.ProfitSharePartyDetails.PS_PartyProfitSharePercent);

			receivingParty.PS_PartyProfitSharePercent = 0m;
			receivingParty.PS_PartyRate = 200m;
			Factory.Save();

			AssertEquals("Profit to invoice is $200", 200m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
			AssertEquals("Agreement", 0m, profitShareShipment.ProfitSharePartyDetails.PS_PartyProfitSharePercent);

			receivingParty.PS_PartyProfitSharePercent = 10m;
			receivingParty.PS_PartyRate = 5m;
			receivingParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;
			Factory.Save();

			AssertEquals("Profit to invoice is 10% ($100) + $5 per chargeable unit", 1100m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
			AssertEquals("Agreement", 10m, profitShareShipment.ProfitSharePartyDetails.PS_PartyProfitSharePercent);
		}

		public void TestProfitToInvoice_ShareLoss()
		{
			OrgHeader someSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(someSendingForwarder);

			OrgHeader deliveryAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent1, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail profitShareShipment = new ProfitShareShipmentDetail(shipment1, deliveryAgent1, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(profitShareShipment);

			Job job = Job.CreateWithMutex(Factory, shipment1);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = "111";
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 1000m;
			charge.JR_AgentDeclaredCostAmt = 5000m;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			profitShareShipment.SetJobForTesting(job);

			AssertNull("Precondition: No profit share agreements exist", profitShareShipment.ProfitShareAgreement);
			AssertEquals("Precondition: No profit share agreements exist, so no profit to invoice", 0, profitShareShipment.ProfitShareCharges.Count);

			OrgAgentRelationship agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = someSendingForwarder.PK;
			agentRelationship.O3_OH_ReceivingAgent = deliveryAgent1.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShareAgreement.O4_SendingPortOrCountry = "AUSYD";
			profitShareAgreement.O4_ReceivingPortOrCountry = "USLAX";
			profitShareAgreement.O4_OH_ControllingAgent = someSendingForwarder.PK;
			profitShareAgreement.O4_ShareLosses = false;

			profitShareShipment.ProfitShareAgreement = profitShareAgreement;

			OrgProfitShareParty rcvParty = profitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 45m;

			Factory.Save();

			AssertEquals("Profit to invoice is 0 as the job made a loss", 0m, profitShareShipment.ProfitShareCharges[0].ProfitShare);

			profitShareAgreement.O4_ShareLosses = true;
			Factory.Save();

			AssertEquals("Profit to invoice is 1800 loss as the agreement shares losses", -1800m, profitShareShipment.ProfitShareCharges[0].ProfitShare);

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = Env.Registry.FreightChargeCode;
			charge2.JR_AgentDeclaredSellAmt = 15000m;
			charge2.JR_AgentDeclaredCostAmt = 1000m;
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge2.JR_IsIncludedInProfitShare = true;
			profitShareShipment.SetJobForTesting(job);

			profitShareAgreement.O4_ShareLosses = false;
			Factory.Save();

			AssertEquals("Profit to invoice is 4500 profit as the entire job made a profit but one charge made a loss", 4500m, profitShareShipment.ProfitShareCharges[0].ProfitShare);

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = Env.Registry.FreightChargeCode;
			charge3.JR_AgentDeclaredSellAmt = 0m;
			charge3.JR_AgentDeclaredCostAmt = 30000m;
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge3.JR_IsIncludedInProfitShare = true;
			profitShareShipment.SetJobForTesting(job);

			Factory.Save();

			AssertEquals("Profit to invoice is 0 as overall, the job made a loss", 0m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
		}

		public void TestOnlyIsUsedApportionChargesUsedByProfitShare()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("CNSHA", "USCHI", "C1000");
			var shipment1 = creator.CreateShipment("1", "CNSHA", "USCHI", consol, transportMode: Constants.TransportModes.Air);
			var shipment2 = creator.CreateShipment("2", "CNSHA", "USCHI", consol, transportMode: Constants.TransportModes.Air);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			var jobConsolCost = apps.CostsCollection.TryAddNew();
			jobConsolCost.E6_AC_ChargeCode = creator.CC1.PK;
			jobConsolCost.E6_OSCostAmount = 75m;

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var agentRelationship = creator.CreateAgentRelationship(sendingAgent, receivingAgent);
			creator.CreateProfitShare(agentRelationship, 50m, 50m, "CNSHA", "USCHI", Constants.TransportModes.Air);

			shipment1.JS_OH_DeliveryAgent = receivingAgent.PK;
			shipment2.JS_OH_DeliveryAgent = receivingAgent.PK;

			AssertEquals("There should be apportionment charges", 2, jobConsolCost.ApportionmentCharges.Count);
			AssertEquals("Apportionment charge on shipment 1", 37.5m, jobConsolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Apportionment charge on shipment 2", 37.5m, jobConsolCost.ApportionmentCharges[1].JR_OSCostAmt);
			jobConsolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;
			AssertEquals("Apportionment charge on shipment 1", 75m, jobConsolCost.ApportionmentCharges[0].JR_OSCostAmt);
			AssertEquals("Apportionment charge on shipment 2", 0m, jobConsolCost.ApportionmentCharges[1].JR_OSCostAmt);

			Factory.Save();

			var job1 = new Job.Loader(Factory, shipment1).Load();
			var job2 = new Job.Loader(Factory, shipment2).Load();

			AssertNotNull("Job on shipment 1 should be created", job1);
			AssertNull("Job on shipment 2 should NOT be created", job2);

			job1.Charges.Cast<Charge>().ForEach(x => x.JR_IsIncludedInProfitShare = true);
			ProfitShareDetailCollection createdProfitShares = new ProfitShareCalculator(Factory, consol, consol.CostSupporter.ShipmentsList).CreateProfitShares();

			var profitShareCharges = createdProfitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges;
			AssertEquals("Number of charges", 1, profitShareCharges.Count);
			AssertEquals("Gross revenue", 75m, profitShareCharges[0].GrossRevenue);
		}

		public void TestProfitToInvoice_GrossRevenue()
		{
			OrgHeader someSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(someSendingForwarder);

			OrgHeader deliveryAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent1, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail profitShareShipment = new ProfitShareShipmentDetail(shipment1, deliveryAgent1, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(profitShareShipment);

			Job job = Job.CreateWithMutex(Factory, shipment1);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = "111";
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 9000m;
			charge.JR_AgentDeclaredCostAmt = 5000m;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			profitShareShipment.SetJobForTesting(job);

			AssertNull("Precondition: No profit share agreements exist", profitShareShipment.ProfitShareAgreement);
			AssertEquals("Precondition: No profit share agreements exist, so no profit to invoice", 0, profitShareShipment.ProfitShareCharges.Count);

			OrgAgentRelationship agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = someSendingForwarder.PK;
			agentRelationship.O3_OH_ReceivingAgent = deliveryAgent1.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShareAgreement.O4_SendingPortOrCountry = "AUSYD";
			profitShareAgreement.O4_ReceivingPortOrCountry = "USLAX";
			profitShareAgreement.O4_OH_ControllingAgent = someSendingForwarder.PK;

			OrgProfitShareParty rcvParty = profitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 45m;
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue;
			profitShareShipment.ProfitShareAgreement = profitShareAgreement;

			Factory.Save();

			AssertEquals("Profit to invoice is 45% of gross revenue (9000)", 4050m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
		}

		public void TestAgent()
		{
			OrgHeader deliveryAgent1 = OrgHeader.New(Factory);
			CommonShipment shipment1 = CommonShipment.New(Factory);
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent1, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment1, deliveryAgent1, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(shipmentDetail);
			AssertEquals("Agent correct on profit share", deliveryAgent1, profitShare.ProfitShareParty);
		}

		public void TestChargesIncludedInProfitShare()
		{
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			CommonShipment shipment = Factory.NewWithValidTestData<CommonShipment>();

			ProfitShareShipmentDetail profitShareShipmentDetail = new ProfitShareShipmentDetail(shipment, deliveryAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);

			Job job = Job.CreateWithMutex(Factory, shipment);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = "4815162342";

			AddNewCharge(job);
			AddNewCharge(job);
			AddNewCharge(job);

			Factory.Save();

			profitShareShipmentDetail.SetJobForTesting(job);
			AssertEquals("ChargesIncludedInProfitShare", 3, profitShareShipmentDetail.ChargesIncludedInProfitShare.Length);

			Charge charge = AddNewCharge(job);
			charge.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			charge = AddNewCharge(job);
			charge.JR_AC = AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value;

			AssertEquals("ChargesIncludedInProfitShare", 3, profitShareShipmentDetail.ChargesIncludedInProfitShare.Length);
			AssertChargesDoesNotContainsProfitShareChargeCodes(profitShareShipmentDetail.ChargesIncludedInProfitShare);

			ZGuid receivingAgentChargeCode = Factory.NewWithValidTestData<AccChargeCode>().PK;
			ZGuid sendingAgentChargeCode = Factory.NewWithValidTestData<AccChargeCode>().PK;
			Factory.Save();

			OrgProfitSharePartyLookups lookups = new OrgProfitSharePartyLookups(null);
			ChargeCodeWithTypeCollection collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = sendingAgentChargeCode;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)].ChargeCode = receivingAgentChargeCode;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			charge = AddNewCharge(job);
			charge.JR_AC = receivingAgentChargeCode;
			charge = AddNewCharge(job);
			charge.JR_AC = sendingAgentChargeCode;

			AssertEquals("ChargesIncludedInProfitShare", 3, profitShareShipmentDetail.ChargesIncludedInProfitShare.Length);
			AssertChargesDoesNotContainsProfitShareChargeCodes(profitShareShipmentDetail.ChargesIncludedInProfitShare);

			job.Charges[0].JR_AC = receivingAgentChargeCode;
			job.Charges[1].JR_AC = sendingAgentChargeCode;

			AssertEquals("ChargesIncludedInProfitShare", 1, profitShareShipmentDetail.ChargesIncludedInProfitShare.Length);
			AssertChargesDoesNotContainsProfitShareChargeCodes(profitShareShipmentDetail.ChargesIncludedInProfitShare);
		}

		public void TestProfitShareIsRoundedAfterCalculation()
		{
			OrgHeader someSendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(someSendingForwarder);

			OrgHeader deliveryAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_ActualChargeable = 200m;

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent1, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail profitShareShipment = new ProfitShareShipmentDetail(shipment1, deliveryAgent1, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(profitShareShipment);

			Job job = Job.CreateWithMutex(Factory, shipment1);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = "111";
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AgentDeclaredSellAmt = 3000m;
			charge.JR_AgentDeclaredCostAmt = 2066.65m;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			profitShareShipment.SetJobForTesting(job);

			AssertNull("Precondition: No profit share agreements exist", profitShareShipment.ProfitShareAgreement);
			AssertEquals("Precondition: No profit share agreements exist, so no profit to invoice", 0, profitShareShipment.ProfitShareCharges.Count);
			AssertNull("ProfitSharePartyDetails", profitShareShipment.ProfitSharePartyDetails);

			OrgAgentRelationship agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = someSendingForwarder.PK;
			agentRelationship.O3_OH_ReceivingAgent = deliveryAgent1.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShareAgreement.O4_SendingPortOrCountry = "AUSYD";
			profitShareAgreement.O4_ReceivingPortOrCountry = "USLAX";
			profitShareAgreement.O4_OH_ControllingAgent = someSendingForwarder.PK;

			OrgProfitShareParty rcvParty = profitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 33m;

			profitShareShipment.ProfitShareAgreement = profitShareAgreement;

			Factory.Save();

			AssertEquals("Profit to invoice is 45% of total profit", 308.01m, profitShareShipment.ProfitShareCharges[0].ProfitShare);
			AssertEquals("Agreement", 33m, profitShareShipment.ProfitSharePartyDetails.PS_PartyProfitSharePercent);
		}

		public void TestProfitShareAmountNotOverstatedForBCNShipments()
		{
			OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "MAS");

			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			var shipmentLead = testObjectCreator.CreateShipment("S001001", consol);
			shipmentLead.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipmentLead.JS_PackingMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			var jobShipmentLead = testObjectCreator.CreateJob(shipmentLead, false);
			var charge1 = testObjectCreator.CreateCharge(jobShipmentLead, testObjectCreator.CC1, 100m, 120m);
			charge1.JR_IsIncludedInProfitShare = true;
			charge1.JR_AgentDeclaredSellAmt = 120M;
			charge1.JR_AgentDeclaredCostAmt = 100M;
			jobShipmentLead.LocalChargesPK = testObjectCreator.LocalClient.PK;

			var shipmentSub1 = testObjectCreator.CreateShipment("S001002", "AUSYD", "NZAKL");
			shipmentSub1.JS_JS_ColoadMasterShipment = shipmentLead.PK;
			var jobShipmentSub1 = testObjectCreator.CreateJob(shipmentSub1, false);
			var charge2 = testObjectCreator.CreateCharge(jobShipmentSub1, testObjectCreator.CC1, 100m, 220m);
			charge2.JR_IsIncludedInProfitShare = true;
			charge2.JR_AgentDeclaredSellAmt = 220M;
			charge2.JR_AgentDeclaredCostAmt = 100M;

			var shipmentSub2 = testObjectCreator.CreateShipment("S001003", "AUSYD", "NZAKL");
			shipmentSub2.JS_JS_ColoadMasterShipment = shipmentLead.PK;
			var jobShipmentSub2 = testObjectCreator.CreateJob(shipmentSub2, false);
			var charge3 = testObjectCreator.CreateCharge(jobShipmentSub2, testObjectCreator.CC1, 100m, 160m);
			charge3.JR_IsIncludedInProfitShare = true;
			charge3.JR_AgentDeclaredSellAmt = 160M;
			charge3.JR_AgentDeclaredCostAmt = 100m;

			Factory.Save();

			jobShipmentLead.SetDefaultsForJob();
			AssertEquals("Precondition: master/lead job should have charges from its sub shipments", 3, jobShipmentLead.Charges.Count);

			OrgHeader sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();

			ProfitShareDetail profitShare = new ProfitShareDetail(deliveryAgent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			ProfitShareShipmentDetail profitShareShipmentDetail = new ProfitShareShipmentDetail(shipmentLead, deliveryAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShare.ProfitShareShipmentDetails.Add(profitShareShipmentDetail);

			AssertEquals("Profit share should include the charge only from the plugin job", 1, profitShareShipmentDetail.ChargesIncludedInProfitShare.Length);

			OrgAgentRelationship agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_OH_SendingAgent = sendingForwarder.PK;
			agentRelationship.O3_OH_ReceivingAgent = deliveryAgent.PK;
			OrgProfitShareDetails profitShareAgreement = agentRelationship.ProfitShareDetails.AddNew();
			profitShareAgreement.O4_FreightMode = "AIR";
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShareAgreement.O4_SendingPortOrCountry = "AUSYD";
			profitShareAgreement.O4_ReceivingPortOrCountry = "NZAKL";
			profitShareAgreement.O4_OH_ControllingAgent = sendingForwarder.PK;

			OrgProfitShareParty rcvParty = profitShareAgreement.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 50m;

			profitShareShipmentDetail.ProfitShareAgreement = profitShareAgreement;

			Factory.Save();

			AssertEquals("Profit to invoice is 50% of total profit", 10m, profitShareShipmentDetail.ProfitShareCharges[0].ProfitShare);
		}

		public void TestProfitShareChargesWithAndWithoutChargesToBeIncludedInProfitShare()
		{
			#region Test Data Setup

			var creator = new TestObjectCreator(Factory);

			var overseasAgent = creator.CreateOrgHeader("MAOEWR", true, true);

			//Profit Share agreement 50 - 50 between sending and receiving agents
			var agentRelationship1 = creator.CreateAgentRelationship(overseasAgent, GlbCompany.CurrentCompany.OrgProxy);
			var profitShareAgreement = creator.CreateProfitShare(agentRelationship1, 50m, 50, "USLAX", "AUSYD", "ALL");
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);

			//Consol, Shipments and jobs
			var consol = creator.CreateConsol("USLAX", "AUSYD", "C00010");
			consol.JK_TransportMode = "AIR";
			consol.JK_OA_SendingForwarderAddress = creator.CreateAddress(overseasAgent, OrgAddressType.Office, true).PK;
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var shipment1 = creator.CreateShipment("S00011", "USLAX", "AUSYD", consol, false, "AIR");

			Factory.Save();

			//Consol Cost
			var consolCost = creator.CreateConsolCost(consol, creator.FRT, 100m, creator.Creditor1);
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_ExchangeRate = 1M;
			consolCost.E6_InvoiceNum = "INV001";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			consolCost.E6_ApportionmentMethod = "CHG";

			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 100M;
			consolCost.ApportionmentCharges[0].JR_OSSellAmt = 125M;
			consolCost.ApportionmentCharges[0].JR_IsIncludedInProfitShare = false;
			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;

			Factory.Save();

			#endregion

			var profitShareShipmentDetail = new ProfitShareShipmentDetail(shipment1, overseasAgent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
			profitShareShipmentDetail.ProfitShareAgreement = profitShareAgreement;

			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Profit share charge should not be created, because shipment 1 charge is not included in profit share", 0, profitShareShipmentDetail.ProfitShareCharges.Count);

			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Profit share charge should not be created, because shipment 1 charge is not included in profit share", 0, profitShareShipmentDetail.ProfitShareCharges.Count);

			consolCost.ApportionmentCharges[0].JR_IsIncludedInProfitShare = true;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Profit share charge should be created, because shipment 1 charge is included in profit share", 1, profitShareShipmentDetail.ProfitShareCharges.Count);

			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Profit share charge should be created, because shipment 1 charge is included in profit share", 1, profitShareShipmentDetail.ProfitShareCharges.Count);
		}

		Charge AddNewCharge(Job job)
		{
			Charge charge = job.Charges.AddNew();
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_IsIncludedInProfitShare = true;
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;

			return charge;
		}

		void AssertChargesDoesNotContainsProfitShareChargeCodes(JobCharge[] charges)
		{
			foreach (JobCharge charge in charges)
			{
				AssertNotEquals("Does not contain profit share charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, charge.PK);
				AssertNotEquals("Does not contain profit share charge code", AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value, charge.PK);

				foreach (FieldInfo partyTypeField in typeof(OrgProfitSharePartyLookups.PartyTypeCodes).GetFields(BindingFlags.Static | BindingFlags.Public))
				{
					string partyType = (string)partyTypeField.GetValue(null);
					AssertNotEquals("Does not contain profit share charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value.GetCode(partyType), charge.PK);
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new ProfitShareShipmentDetail(shipment, Factory.New<OrgHeader>(), OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);
		}

		#endregion
	}
}
