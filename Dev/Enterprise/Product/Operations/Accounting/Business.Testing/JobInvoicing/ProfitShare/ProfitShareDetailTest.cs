using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	[TestedType(typeof(ProfitShareDetail))]
	public class ProfitShareDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAgentToCredit()
		{
			OrgHeader agent = Factory.NewWithValidTestData<OrgHeader>();
			ProfitShareDetail detail = new ProfitShareDetail(agent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			AssertEquals("Agent on Profit Share Detail is the Receiving Agent", agent, detail.ProfitShareParty);
			AssertEquals(agent, ((ICustomLabelsConfigOrgProvider)detail).ConfigOrg);
		}

		public void TestTotalProfitToInvoiceForAgent()
		{
			#region Setup

			// Setup Delivery Agents
			OrgHeader deliveryAgent1 = Factory.NewWithValidTestData<OrgHeader>();

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			consol.Shipments.Add(shipment1);

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = deliveryAgent1.PK;
			consol.Shipments.Add(shipment2);

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = deliveryAgent1.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";
			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 66m;

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

			Factory.Save();

			#endregion

			ProfitShareCalculator calc = new ProfitShareCalculator(Factory, consol.GetShipmentsList());
			ProfitShareDetailCollection profitShares = calc.CreateProfitShares();

			AssertEquals("1 profit share created", 1, profitShares.Count);
			AssertEquals("Profit share for correct agent created", deliveryAgent1.PK, profitShares[0].ProfitShareParty.PK);
			AssertEquals("Profit share has 2 shipment details", 2, profitShares[0].ProfitShareShipmentDetails.Count);

			AssertEquals("Profit on Shipment Detail 1 is correct", 105.6m, profitShares[0].ProfitShareShipmentDetails[0].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Profit on Shipment Detail 2 is correct", 112.2m, profitShares[0].ProfitShareShipmentDetails[1].ProfitShareCharges[0].ProfitShare);
			AssertEquals("Profit share exists", true, profitShares[0].HasProfitShare);
		}

		public void TestSourceIdentifierProvider()
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			var detail = new ProfitShareDetail(agent, Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			var provider = detail as ISourceIdentifierProvider;

			AssertNotNull(provider);
			AssertEquals(agent.PK, provider.SourceIdentifier);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProfitShareDetail(OrgHeader.New(Factory), Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
		}

		#endregion
	}
}
