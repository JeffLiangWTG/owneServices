using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionProcessorTest : TestCaseWithFactory
	{
		[TestDate(2022, 07, 01)]
		public void TestProfitShareRedistribution()
		{
			AssertProfitShareRedistribution();
		}

		public void TestProfitShareRedistribution_WhenADifferentFactoryUsedForRedistribution()
		{
			AssertProfitShareRedistribution(assertLog: false, doNotCreateShipmentJobsOnFactorySave: true, useDifferentFactoryForProcessing: true);
		}

		public void TestProfitShareRedistribution_WhenCustomBranchDefaultingRulesEngineConfigurationIsSet()
		{
			AccountingConfigurationRegistry.Instance.CustomBranchDefaultingRulesEngineConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertProfitShareRedistribution(assertLog: false, doNotCreateShipmentJobsOnFactorySave: true, useDifferentFactoryForProcessing: true);
		}

		void AssertProfitShareRedistribution(bool assertLog = true, bool doNotCreateShipmentJobsOnFactorySave = false, bool useDifferentFactoryForProcessing = false)
		{
			#region Setup

			var disposableManager = new DisposableManager();
			Factory.AddDisposableService(disposableManager);
			if (doNotCreateShipmentJobsOnFactorySave)
			{
				AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}

			var chargeCode = CreateProfitShareCharge(Factory);
			profitShareTestHelper.SetRegistry(chargeCode.PK);

			//International Zone Setup
			//need to check with Product whether it's only OriginGateway or any gateway
			var zone = CreateZone("SG", RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway);
			zone.Countries.Add(LocationHelper.GetLocationFromString(CountryCodes.Australia, Factory) as RefCountry);
			zone.Countries.Add(LocationHelper.GetLocationFromString(CountryCodes.Malaysia, Factory) as RefCountry);
			zone.Countries.Add(LocationHelper.GetLocationFromString(CountryCodes.Singapore, Factory) as RefCountry);

			//Pickup & Delivery Agents
			var pickupAgentMY = CreateAgent(CountryCodes.Malaysia);
			var pickupAgentSG = CreateAgent(CountryCodes.Singapore);
			var pickupAgentTH = CreateAgent(CountryCodes.Thailand);

			var deliveryAgentSG = CreateAgent(CountryCodes.Singapore);
			var deliveryAgentAU = CreateAgent(CountryCodes.Australia);
			var deliveryAgentNZ = CreateAgent(CountryCodes.NewZealand);

			// Setup consol & shipments
			var consol1 = CreateConsol("MYTWU", "SGSIN", "C000001", 100m);
			var consol2 = CreateConsol("SGSIN", "AUSYD", "C000002", 200m);
			var consol3 = CreateConsol("SGSIN", "NZAKL", "C000003", -10m);
			var consol4 = CreateConsol("THBKK", "SGSIN", "C000004", 50m);

			var shipment1 = CreateShipment("S000001", "MYTWU", "AUSYD", 10, pickupAgentMY, deliveryAgentAU, new[] { consol1, consol2 });
			var shipment2 = CreateShipment("S000002", "MYTWU", "NZAKL", 15, pickupAgentMY, deliveryAgentNZ, new[] { consol1, consol3 });
			var shipment3 = CreateShipment("S000003", "SGSIN", "AUSYD", 20, pickupAgentSG, deliveryAgentAU, new[] { consol2 });
			var shipment4 = CreateShipment("S000004", "SGSIN", "NZAKL", 30, pickupAgentSG, deliveryAgentNZ, new[] { consol3 });
			var shipment5 = CreateShipment("S000005", "THBKK", "SGSIN", 35, pickupAgentTH, deliveryAgentSG, new[] { consol4 });

			// Setup Profit Share Redistribution
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			var ps1 = CreateGatewayProfitShareRedistribution(agentRelationship, 50, 50, zone.Code, zone.Code);
			var ps2 = CreateGatewayProfitShareRedistribution(agentRelationship, 100, 0, zone.Code, "");
			var ps3 = CreateGatewayProfitShareRedistribution(agentRelationship, 0, 100, "", zone.Code);

			Factory.Save();

			if (doNotCreateShipmentJobsOnFactorySave)
			{
				AssertNull("Pre-condition: shipment1", shipment1.ShipmentJobHeader);
				AssertNull("Pre-condition: shipment2", shipment2.ShipmentJobHeader);
				AssertNull("Pre-condition: shipment3", shipment3.ShipmentJobHeader);
				AssertNull("Pre-condition: shipment4", shipment4.ShipmentJobHeader);
				AssertNull("Pre-condition: shipment5", shipment5.ShipmentJobHeader);
			}

			#endregion Setup

			var factoryForProcessing = useDifferentFactoryForProcessing ? new BusinessObjectFactory() : Factory;
			var profitShareRedistribution = factoryForProcessing.New<ForwardingProfitShareRedistribution>();
			profitShareRedistribution.AddConsols(new[] { consol1, consol2, consol3, consol4 });
			agentRelationship.SelectedProfitShareDetailsList.AddRange(new[] { ps1, ps2, ps3 });
			profitShareRedistribution.AddProfitShareRules(new[] { agentRelationship });

			var processor = new GatewayProfitRedistributionProcessor(profitShareRedistribution, logger);
			var result = processor.Process();
			Assert("Processing should be successful", result);

			if (assertLog)
			{
				var expectedLogs = @"Starting Consols' Validations ...
C000001 is validated successfully.
C000002 is validated successfully.
C000003 is validated successfully.
C000004 is validated successfully.
Consols' Validations is completed.
Starting ProfitShare rules' Validations ...
ProfitShare rules' Validations is completed.
Starting Shipments' Validations ...
S000001 is validated successfully.
S000002 is validated successfully.
S000003 is validated successfully.
S000004 is validated successfully.
S000005 is validated successfully.
Shipments' Validations is completed.

Starting Consols' profit Calculation...
C000001 Total Profit (AUD): 100.
C000001 Total Profit for redistribution (AUD): 100.
C000002 Total Profit (AUD): 200.
C000002 Total Profit for redistribution (AUD): 200.
C000003 Total Profit (AUD): -10.
C000003 Total Profit for redistribution (AUD): -10.
C000004 Total Profit (AUD): 50.
C000004 Total Profit for redistribution (AUD): 50.
Total Consols Profit (AUD): 340
Total Consols Profit for redistribution (AUD): 340
Calculating Total Shipments' chargeables...
Apportionment Method: CHG
S000001 chargeable by Profit Apportionment Method: 10
S000002 chargeable by Profit Apportionment Method: 15
S000003 chargeable by Profit Apportionment Method: 20
S000004 chargeable by Profit Apportionment Method: 30
S000005 chargeable by Profit Apportionment Method: 35
Total Shipments' chargeables: 110
S000001 Shares: 30.909090909090909090909090909
S000002 Shares: 46.363636363636363636363636364
S000003 Shares: 61.818181818181818181818181818
S000004 Shares: 92.72727272727272727272727273
S000005 Shares: 108.18181818181818181818181818

C000001 is selected as the first consol of S000001
The Best matching profit share rule for S000001 is:
Start Date: 28-Jun-22
End Date: 31-Jul-22
Sending Location: SG
Receiving Location: SG
Mode: AIR
Agreement Type: PCF
Share Loses: Y
Controlling Customer: Empty
Job Type: GCN
Apportionment Method: CHG
Party Type: Shipment Pickup Agent
Party Profit Share: 50%
Party Type: Shipment Delivery Agent
Party Profit Share: 50%
Profit Charges are successfully created for S000001

C000001 is selected as the first consol of S000002
The Best matching profit share rule for S000002 is:
Start Date: 28-Jun-22
End Date: 31-Jul-22
Sending Location: SG
Receiving Location: 
Mode: AIR
Agreement Type: PCF
Share Loses: Y
Controlling Customer: Empty
Job Type: GCN
Apportionment Method: CHG
Party Type: Shipment Pickup Agent
Party Profit Share: 100%
Party Type: Shipment Delivery Agent
Party Profit Share: 0%
Profit Charges are successfully created for S000002

C000002 is selected as the first consol of S000003
The Best matching profit share rule for S000003 is:
Start Date: 28-Jun-22
End Date: 31-Jul-22
Sending Location: SG
Receiving Location: SG
Mode: AIR
Agreement Type: PCF
Share Loses: Y
Controlling Customer: Empty
Job Type: GCN
Apportionment Method: CHG
Party Type: Shipment Pickup Agent
Party Profit Share: 50%
Party Type: Shipment Delivery Agent
Party Profit Share: 50%
Profit Charges are successfully created for S000003

C000003 is selected as the first consol of S000004
The Best matching profit share rule for S000004 is:
Start Date: 28-Jun-22
End Date: 31-Jul-22
Sending Location: SG
Receiving Location: 
Mode: AIR
Agreement Type: PCF
Share Loses: Y
Controlling Customer: Empty
Job Type: GCN
Apportionment Method: CHG
Party Type: Shipment Pickup Agent
Party Profit Share: 100%
Party Type: Shipment Delivery Agent
Party Profit Share: 0%
Profit Charges are successfully created for S000004

C000004 is selected as the first consol of S000005
The Best matching profit share rule for S000005 is:
Start Date: 28-Jun-22
End Date: 31-Jul-22
Sending Location: 
Receiving Location: SG
Mode: AIR
Agreement Type: PCF
Share Loses: Y
Controlling Customer: Empty
Job Type: GCN
Apportionment Method: CHG
Party Type: Shipment Pickup Agent
Party Profit Share: 0%
Party Type: Shipment Delivery Agent
Party Profit Share: 100%
Profit Charges are successfully created for S000005

Available Profit for redistribution was (AUD): 340
Distributed Profit for redistribution is (AUD): 339.99";
				AssertEquals(expectedLogs, string.Join("\r\n", logger.GetLogs(LogType.Information)));
			}

			var expectedCalculatedCharges = @"S000001 Pickup: 15.45 Delivery: 15.45
S000002 Pickup: 46.36 Delivery: 0
S000003 Pickup: 30.91 Delivery: 30.91
S000004 Pickup: 92.73 Delivery: 0
S000005 Pickup: 0 Delivery: 108.18";

			var shipmentWrappers = profitShareRedistribution.Shipments.Cast<ProfitShareForwardingShipmentWrapper>().DistinctBy(x => x.Shipment.PK).ToDictionary(x => x.Shipment.PK, x => x);
			AssertEquals(expectedCalculatedCharges, string.Join("\r\n", shipmentWrappers.Select(x => $"{x.Value.JS_UniqueConsignRef} Pickup: {x.Value.JS_Calc_PickupAgentProfitShare} Delivery: {x.Value.JS_Calc_DeliveryAgentProfitShare}")));

			var expectedPerConsolTotal = @"C000001 TotalProfitAmount: 100 RedistributedProfitAmount: 100
C000002 TotalProfitAmount: 200 RedistributedProfitAmount: 200
C000003 TotalProfitAmount: -10 RedistributedProfitAmount: -10
C000004 TotalProfitAmount: 50 RedistributedProfitAmount: 50";
			var consolWrappers = profitShareRedistribution.Consols.Cast<ProfitShareForwardingConsolWrapper>().DistinctBy(x => x.Consol.PK).ToDictionary(x => x.Consol.PK, x => x);
			AssertEquals(expectedPerConsolTotal, string.Join("\r\n", consolWrappers.Select(x => $"{x.Value.JK_UniqueConsignRef} TotalProfitAmount: {x.Value.JK_Calc_TotalProfitAmount.ToStringTrimZeros()} RedistributedProfitAmount: {x.Value.JK_Calc_RedistributedProfitAmount.ToStringTrimZeros()}")));

			if (assertLog)
			{
				profitShareRedistribution.Factory.Save();

				AssertConsolLog(consol1, new[] { "Gateway Profit share charges created(S000001): 2", "Gateway Profit share charges created(S000002): 1" });
				AssertConsolLog(consol2, new[] { "Gateway Profit share charges created(S000003): 2" });
				AssertConsolLog(consol3, new[] { "Gateway Profit share charges created(S000004): 1" });
				AssertConsolLog(consol4, new[] { "Gateway Profit share charges created(S000005): 1" });
			}

			disposableManager.Dispose();
		}

		public void TestProfitShareRedistribution_ShouldAllowConsolProfitRedistribution_UsingDifferentCompanies()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var pickupAgent = CreateAgent("AU");
			var deliveryAgent = CreateAgent("SG");
			Factory.Save();

			(var company1, var branch1) = testObjectCreator.CreateCompanyAndBranch("AUSYD", pickupAgent);
			(var company2, var branch2) = testObjectCreator.CreateCompanyAndBranch("SGSIN", deliveryAgent);

			var gatewayConsol = testObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", "C000001", receivingGatewayCompany: company1, sendingGatewayCompany: company2);

			CreateShipment("S000001", "AUSYD", "SGSIN", 20, pickupAgent, deliveryAgent, new[] { gatewayConsol });
			CreateShipment("S000002", "AUSYD", "SGSIN", 30, pickupAgent, deliveryAgent, new[] { gatewayConsol });

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			var profitShare = CreateGatewayProfitShareRedistribution(agentRelationship, 50, 50, "AUSYD", "SGSIN");
			agentRelationship.SelectedProfitShareDetailsList.Add(profitShare);

			Factory.Save();

			CreateProfit(branch1.PK, gatewayConsol.PK, 100);
			CreateProfit(branch2.PK, gatewayConsol.PK, 200);

			CreateCharge(branch1.PK);
			CreateCharge(branch2.PK);

			AssertProcess(branch1.PK, true);
			AssertProcess(branch1.PK, false);

			AssertProcess(branch2.PK, true);
			AssertProcess(branch2.PK, false);

			void CreateProfit(ZGuid branchPK, ZGuid consolPK, decimal profit)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var factory = new BusinessObjectFactory();
					var consol = factory.Load<ForwardingConsol>(consolPK);
					AssertNull(consol.Job);

					profitShareTestHelper.CreateProfit(factory, consol, profit, Env.Registry.FreightChargeCode);
					var job = consol.Job as Job;
					AssertEquals("Pre-condition", profit, job.JH_ProfitLoss);
					factory.Save();
				}
			}

			void CreateCharge(ZGuid branchPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var chargeCode = CreateProfitShareCharge(new BusinessObjectFactory());
					profitShareTestHelper.SetRegistry(chargeCode.PK, Env.CurrentCompanyPK);
				}
			}

			void AssertProcess(ZGuid branchPK, bool expectedResult)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var factory = new BusinessObjectFactory();
					var consol = factory.Load<ForwardingConsol>(gatewayConsol.PK);
					consol.Shipments[0].PickupAgent.OH_IsCreditor = true;
					consol.Shipments[0].DeliveryAgent.OH_IsCreditor = true;
					
					var profitShareRedistribution = factory.New<ForwardingProfitShareRedistribution>();
					profitShareRedistribution.AddConsols(new[] { consol });
					profitShareRedistribution.AddProfitShareRules(new[] { agentRelationship });

					var processor = new GatewayProfitRedistributionProcessor(profitShareRedistribution, logger);
					var result = processor.Process();
					AssertEquals($"Process couldn't complete for Company :- {Env.CurrentCompany.Code}", expectedResult, result);

					if (expectedResult)
					{
						AssertNoExceptionThrown(() => factory.Save());
					}
				}
			}
		}

		public void TestProfitShareRedistribution_ShouldCopyRules()
		{
			#region Setup

			var disposableManager = new DisposableManager();
			Factory.AddDisposableService(disposableManager);

			var chargeCode = CreateProfitShareCharge(Factory);
			profitShareTestHelper.SetRegistry(chargeCode.PK);

			//International Zone Setup
			//need to check with Product whether it's only OriginGateway or any gateway
			var zone = CreateZone("SG", RefZoneHeaderLookups.ZoneTypeCodes.OriginGateway);
			zone.Countries.Add(LocationHelper.GetLocationFromString(CountryCodes.Australia, Factory) as RefCountry);
			zone.Countries.Add(LocationHelper.GetLocationFromString(CountryCodes.Malaysia, Factory) as RefCountry);
			zone.Countries.Add(LocationHelper.GetLocationFromString(CountryCodes.Singapore, Factory) as RefCountry);

			//Pickup & Delivery Agents
			var pickupAgentMY = CreateAgent(CountryCodes.Malaysia);
			var pickupAgentSG = CreateAgent(CountryCodes.Singapore);
			var pickupAgentTH = CreateAgent(CountryCodes.Thailand);

			var deliveryAgentSG = CreateAgent(CountryCodes.Singapore);
			var deliveryAgentAU = CreateAgent(CountryCodes.Australia);
			var deliveryAgentNZ = CreateAgent(CountryCodes.NewZealand);

			// Setup consol & shipments
			var consol1 = CreateConsol("MYTWU", "SGSIN", "C000001", 100m);
			var consol2 = CreateConsol("SGSIN", "AUSYD", "C000002", 200m);
			var consol3 = CreateConsol("SGSIN", "NZAKL", "C000003", -10m);
			var consol4 = CreateConsol("THBKK", "SGSIN", "C000004", 50m);

			var shipment1 = CreateShipment("S000001", "MYTWU", "AUSYD", 10, pickupAgentMY, deliveryAgentAU, new[] { consol1, consol2 });
			var shipment2 = CreateShipment("S000002", "MYTWU", "NZAKL", 15, pickupAgentMY, deliveryAgentNZ, new[] { consol1, consol3 });
			var shipment3 = CreateShipment("S000003", "SGSIN", "AUSYD", 20, pickupAgentSG, deliveryAgentAU, new[] { consol2 });
			var shipment4 = CreateShipment("S000004", "SGSIN", "NZAKL", 30, pickupAgentSG, deliveryAgentNZ, new[] { consol3 });
			var shipment5 = CreateShipment("S000005", "THBKK", "SGSIN", 35, pickupAgentTH, deliveryAgentSG, new[] { consol4 });

			// Setup Profit Share Redistribution
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			var ps1 = CreateGatewayProfitShareRedistribution(agentRelationship, 50, 50, zone.Code, zone.Code);
			var ps2 = CreateGatewayProfitShareRedistribution(agentRelationship, 100, 0, zone.Code, "");
			var ps3 = CreateGatewayProfitShareRedistribution(agentRelationship, 0, 100, "", zone.Code);

			Factory.Save();

			#endregion Setup

			var factoryForProcessing = Factory;
			var profitShareRedistribution = factoryForProcessing.New<ForwardingProfitShareRedistribution>();
			profitShareRedistribution.AddConsols(new[] { consol1, consol2, consol3, consol4 });
			agentRelationship.SelectedProfitShareDetailsList.AddRange(new[] { ps1, ps2, ps3 });
			profitShareRedistribution.AddProfitShareRules(new[] { agentRelationship });

			var processor = new GatewayProfitRedistributionProcessor(profitShareRedistribution, logger);
			var result = processor.Process();
			Assert("Processing should be successful", result);

			AssertEquals(3, profitShareRedistribution.ProfitShareRules.Count);

			void AssertCopyMatchesOriginal(ProfitShareRedistributionRule rule, OrgProfitShareDetails psd)
			{
				AssertEquals(psd.O4_StartDate, rule.PRR_StartDate);
				AssertEquals(psd.O4_EndDate, rule.PRR_EndDate);
				AssertEquals(psd.O4_SendingPortOrCountry, rule.PRR_SendingPortOrCountry);
				AssertEquals(psd.O4_ReceivingPortOrCountry, rule.PRR_ReceivingPortOrCountry);
				AssertEquals(psd.O4_JobType, rule.PRR_JobType);
				AssertEquals(psd.O4_GatewayAgentType, rule.PRR_GatewayAgentType);
				AssertEquals(psd.O4_GatewayProfitApportionmentMethod, rule.PRR_GatewayProfitApportionmentMethod);
				AssertEquals(psd.O4_FreightMode, rule.PRR_FreightMode);
				AssertEquals(psd.O4_AgreementType, rule.PRR_AgreementType);
				AssertEquals(psd.O4_ShareLosses, rule.PRR_ShareLosses);
				AssertEquals(psd.O4_OH_ControllingAgent, rule.PRR_OH_ControllingAgent);
				AssertEquals(psd.O4_OrgOverrideType, rule.PRR_OrgOverrideType);
				AssertEquals(psd.O4_OH_OrgOverride, rule.PRR_OH_OrgOverride);
				AssertEquals(psd.ShipmentPickupAgentProfitShare, rule.PRR_ShipmentPickupAgentProfitSharePercent);
				AssertEquals(psd.ShipmentDeliveryAgentProfitShare, rule.PRR_ShipmentDeliveryAgentProfitSharePercent);
			}

			AssertCopyMatchesOriginal(profitShareRedistribution.ProfitShareRules[0], ps1);
			AssertCopyMatchesOriginal(profitShareRedistribution.ProfitShareRules[1], ps2);
			AssertCopyMatchesOriginal(profitShareRedistribution.ProfitShareRules[2], ps3);

			disposableManager.Dispose();
		}

		public void TestProfitShareRedistribution_ShouldLogException()
		{
			var disposableManager = new DisposableManager();
			Factory.AddDisposableService(disposableManager);

			var consol = CreateConsol("AUSYD", "SGSIN", "C000001", 100m);
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			CreateGatewayProfitShareRedistribution(agentRelationship, 50, 50, "AUSYD", "SGSIN");

			var profitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();
			profitShareRedistribution.AddConsols(new[] { consol });
			profitShareRedistribution.AddProfitShareRules(new[] { agentRelationship });

			var errors = new StringBuilder();
			var mockLogger = new Mock<IDisposableProfitShareRedistributionLogger>(MockBehavior.Strict);
			mockLogger.Setup(x => x.Log(LogType.Error, It.IsAny<string>())).Callback<LogType, string>((logType, error) => { errors.AppendLine(error); });
			mockLogger.Setup(x => x.Log(LogType.Information, "Starting Consols' Validations ...")).Throws(new Exception("general exception"));

			var onAllCompletedCalled = false;
			mockLogger.Setup(x => x.OnAllCompleted(It.IsAny<EventArgs>())).Callback(() => { onAllCompletedCalled = true; });

			var processor = new GatewayProfitRedistributionProcessor(profitShareRedistribution, mockLogger.Object);
			var result = processor.Process();
			Assert(onAllCompletedCalled);
			AssertEquals("Exception:general exception", errors.ToString().Trim());

			disposableManager.Dispose();
		}

		public void TestProfitShareRedistribution_ShouldNotCreateCharges_AgentDefinedOnShipmentDoNotHaveShareDefinedOnRules_PickupAgent()
		{
			AssertShouldNotCreateCharges_AgentDefinedOnShipmentDoNotHaveShareDefinedOnRules(CreateAgent("AU"), null);
		}

		public void TestProfitShareRedistribution_ShouldNotCreateCharges_AgentDefinedOnShipmentDoNotHaveShareDefinedOnRules_DeliveryAgent()
		{
			AssertShouldNotCreateCharges_AgentDefinedOnShipmentDoNotHaveShareDefinedOnRules(null, CreateAgent("SG"));
		}

		void AssertShouldNotCreateCharges_AgentDefinedOnShipmentDoNotHaveShareDefinedOnRules(OrgHeader pickupAgent, OrgHeader deliveryAgent)
		{
			if (!(pickupAgent != null ^ deliveryAgent != null))
			{
				Assert("Precondition: This test requires at least one party should be null and other not null", false);
			}

			var origin = "AUSYD";
			var destination = "SGSIN";

			// Setup consol & shipments
			var consol = CreateConsol(origin, destination, "C000001", 100m);
			var shipment = CreateShipment("S000001", origin, destination, 10, pickupAgent, deliveryAgent, new[] { consol });

			// Setup Profit Share Redistribution
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = "AGY";
			var profitShare = testObjectCreator.CreateProfitShare(agentRelationship, origin, destination, "AIR", jobType: "GCN", apportionmentMethod: "SHP");
			var party = profitShare.PartyDetailsForGatewayProfitShareRedistribution.AddNew();
			party.PS_PartyType = pickupAgent == null ? "SPA" : "SDA";
			party.PS_PartyProfitSharePercent = 100m;
			Factory.Save();

			var profitShareRedistribution = Factory.New<ForwardingProfitShareRedistribution>();
			profitShareRedistribution.AddConsols(new[] { consol });
			agentRelationship.SelectedProfitShareDetailsList.Add(profitShare);
			profitShareRedistribution.AddProfitShareRules(new[] { agentRelationship });

			var processor = new GatewayProfitRedistributionProcessor(profitShareRedistribution, logger);
			Assert("Processing should not be successful", !processor.Process());
			AssertContainsExactElementsInAnyOrder(new[] { "Unable to share profit for S000001" }, logger.GetLogs(LogType.Warning).ToArray());

			shipment.Job?.Dispose();
		}

		ForwardingConsol CreateConsol(string origin, string destination, string consolNum, decimal profit)
			=> profitShareTestHelper.CreateConsol(origin, destination, consolNum, profit);

		ForwardingShipment CreateShipment(string shipmentNum, string pickup, string delivery, ZDecimal chargeable, OrgHeader pickupAgent, OrgHeader deliveryAgent, ForwardingConsol[] forwardingConsols)
		{
			var shipment = profitShareTestHelper.CreateShipment(shipmentNum, pickup, delivery, chargeable: chargeable, pickupAgent: pickupAgent, deliveryAgent: deliveryAgent, forwardingConsols: forwardingConsols);
			(var pickupAddress, var deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress(pickup, delivery);
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			return shipment;
		}

		OrgHeader CreateAgent(string countryCode)
		{
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.MainAddress.OA_RN_NKCountryCode = countryCode;

			return agent;
		}

		RefZoneHeader CreateZone(string zoneCode, string zoneType, OrgHeader relatedParty = null)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = zoneCode;
			zone.FZ_ZoneType = zoneType;
			if (relatedParty != null)
			{
				zone.FZ_OH_RelatedParty = relatedParty.PK;
			}

			zones.Add(zone);

			return zone;
		}

		OrgProfitShareDetails CreateGatewayProfitShareRedistribution(OrgAgentRelationship agentRelationship, decimal pickupProfitSharePercentage, decimal deliveryProfitSharePercentage, ZString origin, ZString destination)
		{
			return testObjectCreator.CreateGatewayProfitShareRedistribution(agentRelationship, pickupProfitSharePercentage, deliveryProfitSharePercentage, origin, destination, TransportModes.Air, apportionmentMethod: GatewayProfitApportionmentMethodList.Codes.CHG);
		}

		void AssertConsolLog(ForwardingConsol consol, string[] expectedLogs)
		{
			var logs = GetLogsFromDB();

			AssertNotNull(logs);
			AssertEquals(expectedLogs.Length, logs.Length);
			AssertContainsExactElementsInAnyOrder(expectedLogs, logs.Select(x => x.SL_Reference));

			StmALog[] GetLogsFromDB()
			{
				var query = new ZDBOnlyQuery(typeof(StmALog));
				query.AddToFilter(StmALogSchema.SL_Table, "JobHeader");
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ProfitOfGWConsolsRedistributed.Code);
				query.AddToFilter(StmALogSchema.SL_Parent, consol.Job.PK);
				return Factory.Load<StmALog>(query);
			}
		}

		AccChargeCode CreateProfitShareCharge(BusinessObjectFactory factory)
		{
			var chargeCode = factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PSR";
			chargeCode.AC_Desc = "Profit Share Redistribution";

			factory.Save();
			return chargeCode;
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);
			zones = new List<RefZoneHeader>();
			logger = new TestLogger();
		}

		protected override void TearDown()
		{
			base.TearDown();

			testObjectCreator = null;
			profitShareTestHelper = null;
			zones = null;
			logger.Dispose();
			logger = null;
		}

		ProfitShareTestHelper profitShareTestHelper;
		TestObjectCreator testObjectCreator;
		List<RefZoneHeader> zones;
		TestLogger logger;
	}
}
