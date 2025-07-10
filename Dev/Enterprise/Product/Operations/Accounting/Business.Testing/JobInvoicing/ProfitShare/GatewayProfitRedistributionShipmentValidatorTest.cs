using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionShipmentValidatorTest : TestCaseWithFactory
	{
		public void TestValidation()
		{
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();

			var gatewayConsol = profitShareTestHelper.CreateConsol("CN00002");
			var shipment1 = profitShareTestHelper.CreateShipment("S000001", pickupAgent: pickupAgent, forwardingConsols: new[] { gatewayConsol }, changeAgentsAsCreditor: false);
			var shipment2 = profitShareTestHelper.CreateShipment("S000002", deliveryAgent: deliveryAgent, forwardingConsols: new[] { gatewayConsol }, changeAgentsAsCreditor: false);
			var shipment3 = profitShareTestHelper.CreateShipment("S000003", pickupAgent: pickupAgent, deliveryAgent: deliveryAgent, forwardingConsols: new[] { gatewayConsol }, changeAgentsAsCreditor: false);
			var shipment4 = profitShareTestHelper.CreateShipment("S000004", forwardingConsols: new[] { gatewayConsol });

			pickupAgent.OH_IsCreditor = false;
			deliveryAgent.OH_IsCreditor = false;

			AssertValidation(shipment1, false, "S000001: Pickup Agent is not a creditor.");
			AssertValidation(shipment2, false, "S000002: Delivery Agent is not a creditor.");
			AssertValidation(shipment3, false, "S000003: Pickup Agent is not a creditor.", "S000003: Delivery Agent is not a creditor.");
			AssertValidation(shipment4, false, "S000004: Pickup Agent and Delivery Agent both are not defined.");

			pickupAgent.OH_IsCreditor = true;
			deliveryAgent.OH_IsCreditor = true;
			AssertValidation(shipment1, true);
			AssertValidation(shipment2, true);
			AssertValidation(shipment3, true);

			var shipmentProfitShare = Factory.NewWithValidTestData<ShipmentProfitShares>();
			shipmentProfitShare.PSS_JS = shipment3.PK;
			Factory.Save();

			AssertValidation(shipment3, false, "S000003 is already processed.");
		}

		public void TestValidation_DuplicateShipment_UsingDifferentLoginCompany()
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			var branch1 = profitShareTestHelper.CreateBranch("BSG", "CSG", "SG", "SGSIN");
			var branch2 = profitShareTestHelper.CreateBranch("BIN", "CIN", "IN", "INIXE");
			var branch1Address = branch1.OrgProxy.MainAddress;
			var branch2Address = branch2.OrgProxy.MainAddress;
			var gatewayConsol1 = profitShareTestHelper.CreateGatewayConsol(branch1Address, branch2Address);
			var gatewayConsol2 = profitShareTestHelper.CreateGatewayConsol(branch2Address, branch1Address);

			var shipmentAttachedToConsol1PK = profitShareTestHelper.CreateShipment("S000001", pickupAgent: agent, forwardingConsols: new[] { gatewayConsol1 }).PK;
			var shipmentAttachedToConsol2PK = profitShareTestHelper.CreateShipment("S000002", pickupAgent: agent, forwardingConsols: new[] { gatewayConsol2 }).PK;
			var shipmentAttachedToBothConsolPK = profitShareTestHelper.CreateShipment("S000003", pickupAgent: agent, forwardingConsols: new[] { gatewayConsol1, gatewayConsol2 }).PK;
			
			Factory.Save();

			var branch1PK = branch1.PK.ToGuid();
			var branch2PK = branch2.PK.ToGuid();
			SetPrerequisite(agent.PK, branch1PK);
			SetPrerequisite(agent.PK, branch2PK);
			Factory.Save();

			AssertValidation(branch1PK, shipmentAttachedToConsol1PK, true);
			AssertValidation(branch1PK, shipmentAttachedToConsol2PK, true);
			AssertValidation(branch1PK, shipmentAttachedToBothConsolPK, true);
			AssertValidation(branch2PK, shipmentAttachedToConsol1PK, true);
			AssertValidation(branch2PK, shipmentAttachedToConsol2PK, true);
			AssertValidation(branch2PK, shipmentAttachedToBothConsolPK, true);

			Process(branch1PK, gatewayConsol1.PK, new[] { shipmentAttachedToConsol1PK, shipmentAttachedToBothConsolPK });
			
			AssertValidation(branch1PK, shipmentAttachedToConsol1PK, false);
			AssertValidation(branch1PK, shipmentAttachedToConsol2PK, true);
			AssertValidation(branch1PK, shipmentAttachedToBothConsolPK, false);
			AssertValidation(branch2PK, shipmentAttachedToConsol1PK, true);
			AssertValidation(branch2PK, shipmentAttachedToConsol2PK, true);
			AssertValidation(branch2PK, shipmentAttachedToBothConsolPK, true);

			Process(branch2PK, gatewayConsol2.PK, new[] { shipmentAttachedToConsol2PK, shipmentAttachedToBothConsolPK });

			AssertValidation(branch1PK, shipmentAttachedToConsol1PK, false);
			AssertValidation(branch1PK, shipmentAttachedToConsol2PK, true);
			AssertValidation(branch1PK, shipmentAttachedToBothConsolPK, false);
			AssertValidation(branch2PK, shipmentAttachedToConsol1PK, true);
			AssertValidation(branch2PK, shipmentAttachedToConsol2PK, false);
			AssertValidation(branch2PK, shipmentAttachedToBothConsolPK, false);

			Process(branch2PK, gatewayConsol1.PK, new[] { shipmentAttachedToConsol1PK });
			Process(branch1PK, gatewayConsol2.PK, new[] { shipmentAttachedToConsol2PK });

			AssertValidation(branch1PK, shipmentAttachedToConsol1PK, false);
			AssertValidation(branch1PK, shipmentAttachedToConsol2PK, false);
			AssertValidation(branch1PK, shipmentAttachedToBothConsolPK, false);
			AssertValidation(branch2PK, shipmentAttachedToConsol1PK, false);
			AssertValidation(branch2PK, shipmentAttachedToConsol2PK, false);
			AssertValidation(branch2PK, shipmentAttachedToBothConsolPK, false);

			void SetPrerequisite(ZGuid agentPK, Guid branchPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
				{
					var orgHeader = Factory.Load<OrgHeader>(agentPK);
					orgHeader.OH_IsCreditor = true;
				}
			}

			void Process(Guid branchPK, ZGuid consolPK, ZGuid[] shipmentPKs)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
				{
					var consol = Factory.Load<ForwardingConsol>(consolPK);
					using (var consolJob = new JobHeader.Loader(Factory, consol).TryLoadOrCreateWithoutMutexForTestOnly())
					{
						AssertNotNull("Pre-Condition: consol job", consolJob);

						var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();

						var consolidationProfitShare = Factory.New<ConsolidationProfitShare>();
						consolidationProfitShare.CPS_PSR = profitShareRedistribution.PK;
						consolidationProfitShare.CPS_RX_NKCurrency = "AUD";
						consolidationProfitShare.CPS_JK = consol.PK;
						consolidationProfitShare.CPS_JH_ConsolJob = consolJob.PK;

						for (int i = 0; i < shipmentPKs.Length; i++)
						{
							var shipment = Factory.Load<ForwardingShipment>(shipmentPKs[i]);
							using (var shipmentJob = new JobHeader.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly())
							{
								AssertNotNull("Pre-Condition: shipment job", shipmentJob);

								var shipmentProfitShare = Factory.New<ShipmentProfitShares>();
								shipmentProfitShare.PSS_CPS = consolidationProfitShare.PK;
								shipmentProfitShare.PSS_JS = shipment.PK;
								shipmentProfitShare.PSS_JH_ShipmentJob = shipmentJob.PK;
							}
						}
					}

					Factory.Save();
				}
			}
		}

		void AssertValidation(ForwardingShipment forwardingShipment, bool expected, params string[] expectedErrors)
		{
			using (var logger = new TestLogger())
			{
				var validation = new GatewayProfitRedistributionShipmentValidator(Factory, forwardingShipment, logger);
				CombineAssertions($"{forwardingShipment.JS_UniqueConsignRef} validation failed.", () =>
				{
					AssertEquals(expected, validation.IsValid());
					AssertContainsExactElementsInAnyOrder(expectedErrors, logger.GetLogs(LogType.Error).ToArray());
				});
			}
		}

		void AssertValidation(Guid branchPK, ZGuid shipmentPK, bool isValid)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
			{
				var shipment = Factory.Load<ForwardingShipment>(shipmentPK);

				if (isValid)
				{
					AssertValidation(shipment, true);
				}
				else
				{
					AssertValidation(shipment, false, $"{shipment.JS_UniqueConsignRef} is already processed.");
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);
		}

		ProfitShareTestHelper profitShareTestHelper;
		TestObjectCreator testObjectCreator;
	}
}
