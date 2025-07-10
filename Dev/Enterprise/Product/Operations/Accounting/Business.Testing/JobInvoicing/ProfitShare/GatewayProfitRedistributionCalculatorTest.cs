using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionCalculatorTest : TestCaseWithFactory
	{
		public void TestCreateProfitShare()
		{
			var consol = testObjectCreator.CreateConsol();

			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_IsCreditor = true;
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_IsCreditor = true;

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: null, deliveryAgent: deliveryAgent, forwardingConsols: new[] { consol });
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2", pickupAgent: pickupAgent, deliveryAgent: null, forwardingConsols: new[] { consol });
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3", pickupAgent: pickupAgent, deliveryAgent: deliveryAgent, forwardingConsols: new[] { consol });
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4", pickupAgent: null, deliveryAgent: null, forwardingConsols: new[] { consol });

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = "AGY";
			var ps50_50 = testObjectCreator.CreateGatewayProfitShareRedistribution(agentRelationship, 50, 50, "", "", "AIR", apportionmentMethod: "SHP");
			var ps30_70 = testObjectCreator.CreateGatewayProfitShareRedistribution(agentRelationship, 30, 70, "", "", "AIR", apportionmentMethod: "SHP");
			var ps0_100 = testObjectCreator.CreateGatewayProfitShareRedistribution(agentRelationship, 0, 100, "", "", "AIR", apportionmentMethod: "SHP");
			var ps100_0 = testObjectCreator.CreateGatewayProfitShareRedistribution(agentRelationship, 100, 0, "", "", "AIR", apportionmentMethod: "SHP");
			Factory.Save();

			//shipment1 - pickupAgent: null
			AssertCalculation(shipment1, 500m, ps50_50, 1, null, deliveryAgent, null, 500m);
			AssertCalculation(shipment1, 500m, ps30_70, 1, null, deliveryAgent, null, 500m);
			AssertCalculation(shipment1, 500m, ps0_100, 1, null, deliveryAgent, null, 500m);
			AssertCalculation(shipment1, 500m, ps100_0, 0, null, null, null, null);

			//shipment2 - deliveryAgent: null
			AssertCalculation(shipment2, 500m, ps50_50, 1, pickupAgent, null, 500m, null);
			AssertCalculation(shipment2, 500m, ps30_70, 1, pickupAgent, null, 500m, null);
			AssertCalculation(shipment2, 500m, ps0_100, 0, null, null, null, null);
			AssertCalculation(shipment2, 500m, ps100_0, 1, pickupAgent, null, 500m, null);

			//shipment3 - both pickupAgent and deliveryAgent are defined
			AssertCalculation(shipment3, 500m, ps50_50, 2, pickupAgent, deliveryAgent, 250m, 250m);
			AssertCalculation(shipment3, 500m, ps30_70, 2, pickupAgent, deliveryAgent, 150m, 350m);
			AssertCalculation(shipment3, 500m, ps0_100, 1, null, deliveryAgent, null, 500m);
			AssertCalculation(shipment3, 500m, ps100_0, 1, pickupAgent, null, 500m, null);

			//shipment4 - both pickupAgent and deliveryAgent are not defined
			AssertCalculation(shipment4, 500m, ps50_50, 0, null, null, null, null);
			AssertCalculation(shipment4, 500m, ps30_70, 0, null, null, null, null);
			AssertCalculation(shipment4, 500m, ps0_100, 0, null, null, null, null);
			AssertCalculation(shipment4, 500m, ps100_0, 0, null, null, null, null);

			void AssertCalculation(ForwardingShipment shipment,
				ZDecimal totalProfitShared,
				OrgProfitShareDetails orgProfitShareDetails,
				int expectedNumberOfShares,
				OrgHeader expectedPickupAgentParty,
				OrgHeader expectedDeliveryAgentParty,
				ZDecimal? expectedPickupAgentShare,
				ZDecimal? expectedDeliveryAgentShare)
			{
				using (var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex())
				{
					AssertNotNull("Pre-condition", job);

					var calculator = new GatewayProfitRedistributionCalculator(Factory, new ProfitShareForwardingConsolWrapper(consol), new ProfitShareForwardingShipmentWrapper(shipment), orgProfitShareDetails, totalProfitShared);

					var profitShares = calculator.CreateProfitShares();
					AssertNotNull(profitShares);

					var profitShareDetails = profitShares.Cast<ProfitShareDetail>();
					AssertEquals(expectedNumberOfShares, profitShareDetails.Count());

					AssertProfitShareDetail(profitShareDetails, expectedPickupAgentParty, expectedPickupAgentShare, OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentPickupAgent, shipment);
					AssertProfitShareDetail(profitShareDetails, expectedDeliveryAgentParty, expectedDeliveryAgentShare, OrgProfitSharePartyLookups.PartyTypeCodes.ShipmentDeliveryAgent, shipment);
				}
			}

			void AssertProfitShareDetail(IEnumerable<ProfitShareDetail> profitShareDetails, OrgHeader agent, ZDecimal? expectedShare, string partyType, IJobInvoicingPlugIn shipment)
			{
				if (agent != null)
				{
					var partyProfitShares = profitShareDetails.Where(x => x.PartyType == partyType);
					AssertEquals(1, partyProfitShares.Count());

					var partyProfitShare = partyProfitShares.First();
					AssertEquals(consol, partyProfitShare.Consol);
					AssertEquals(agent, partyProfitShare.ProfitShareParty);
					AssertEquals(expectedShare.HasValue, partyProfitShare.HasProfitShare);

					AssertEquals(1, partyProfitShare.ProfitShareShipmentDetails.Count);

					var profitShareShipmentDetail = partyProfitShare.ProfitShareShipmentDetails.Cast<ProfitShareShipmentDetail>().FirstOrDefault();
					AssertNotNull(profitShareShipmentDetail);
					AssertEquals(shipment, profitShareShipmentDetail.Parent);
					AssertEquals(partyType, profitShareShipmentDetail.PartyType);
					AssertEquals(GlbCompany.CurrentCompany.LocalCurrency, profitShareShipmentDetail.ProfitShareCharges[0].Currency);
					AssertEquals(expectedShare, profitShareShipmentDetail.ProfitShareCharges[0].ProfitShare);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);
		}

		TestObjectCreator testObjectCreator;
		ProfitShareTestHelper profitShareTestHelper;
	}
}
