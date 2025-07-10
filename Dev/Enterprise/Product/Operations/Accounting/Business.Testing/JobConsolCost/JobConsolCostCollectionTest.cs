using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	[TestedType(typeof(JobConsolCostCollection))]
	public class JobConsolCostCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestConsolCostDefaultApportionmentMethod()
		{
			AssertDefault();
			AssertFirstGlobalFallbackAsManual();

			void AssertDefault()
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var newCosting = TestCollection.TryAddNew();
				AssertEquals(ZString.Empty, newCosting.E6_ApportionmentMethod);

				newCosting.E6_AC_ChargeCode = testObjectCreator.FRT.PK;
				AssertEquals(AllocationMethod.ChargeableUnits, newCosting.E6_ApportionmentMethod);
			}

			void AssertFirstGlobalFallbackAsManual()
			{
				var newConfig = ConsolCostDefaultApportionmentMethodConfiguration.Create_ForTestOnly(AllocationMethod.Manual);
				AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newConfig);

				var testObjectCreator = new TestObjectCreator(Factory);
				var newCosting = TestCollection.TryAddNew();
				AssertEquals(ZString.Empty, newCosting.E6_ApportionmentMethod);

				newCosting.E6_AC_ChargeCode = testObjectCreator.FRT.PK;
				AssertEquals(AllocationMethod.Manual, newCosting.E6_ApportionmentMethod);
			}
		}

		public void TestSetDefaultsForNewChild()
		{
			Consol.JK_OA_CreditorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var cost = TestCollection.TryAddNew();

			AssertEquals(cost.IsPosting, TestCollection.IsPosting);
			AssertEquals(cost.E6_ParentID, Consol.PK);
			AssertEquals(cost.E6_PPDCLT, PrepaidCollectList.Codes.All);
			AssertEquals(cost.E6_GC, GlbCompany.CurrentCompany.PK);
			AssertEquals(cost.E6_RX_NKCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(cost.E6_ExchangeRate, 1m);
			AssertEquals(cost.E6_ApportionmentMethod, cost.Consol.GetApportionmentMethod(cost.ChargeCode));
			AssertEquals(cost.E6_GS_NKConsolCostOwner, GlbStaff.CurrentUser.GS_Code);
		}

		public void TestE6_ApportionmentMethod_ReadOnlyProperty_Maintain()
		{
			Assert("Pre-condition", !Consol.IsGateway());
			var consolCost = TestCollection.TryAddNew();
			AssertNotNull("Should be allowed to add new if Consol is not Gateway", consolCost);

			Env.Security.MaintainConsolJobInvoicingEditMethod.IsAllowed = false;
			AssertEquals("E6_ApportionmentMethod should be readonly when Edit Method is not allowed.", true, consolCost.E6_ApportionmentMethod_ReadOnlyForTest);

			Env.Security.MaintainConsolJobInvoicingEditMethod.IsAllowed = true;
			AssertEquals("E6_ApportionmentMethod should not be readonly when Edit Method is not allowed.", false, consolCost.E6_ApportionmentMethod_ReadOnlyForTest);
		}

		public void TestE6_ApportionmentMethod_ReadOnlyProperty_Gateway()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);
			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();
				AssertEquals(1, consol.GetApportionments(true).CostsCollection.Count);
				var cost = consol.GetApportionments(true).CostsCollection[0];
				Assert(cost.IsGatewayConsolCost);
				AssertEquals(1, cost.ApportionmentCharges.Count);

				Env.Security.GatewayConsolJobInvoicingEditMethod.IsAllowed = false;
				AssertEquals("E6_ApportionmentMethod should be readonly when Edit Method is not allowed.", true, cost.E6_ApportionmentMethod_ReadOnlyForTest);

				Env.Security.GatewayConsolJobInvoicingEditMethod.IsAllowed = true;
				AssertEquals("E6_ApportionmentMethod should not be readonly when Edit Method is not allowed.", false, cost.E6_ApportionmentMethod_ReadOnlyForTest);
			}
		}

		public void TestIsGatewayMakesCollectionReadOnly()
		{
			Assert("Pre-condition", !Consol.IsGateway());
			Assert("Should not be read only if Consol is not Gateway", !TestCollection.ReadOnly);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "CNSHA";
			Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Assert("Expected consol to now be a gateway consol", Consol.IsGateway());
			Assert(!TestCollection.IsUsedForGateway);
			Assert("Collection should not be read only because IsUsedForGateway is false", !TestCollection.ReadOnly);

			var gwJobConsolCostCollection = new JobConsolCostCollection(Factory, Consol, true);
			Assert(gwJobConsolCostCollection.IsUsedForGateway);
			Assert("Collection should be read only because IsUsedForGateway is true", gwJobConsolCostCollection.ReadOnly);
		}

		public void TestJobConsolCostCollectionLoadDifferentCostsBasedOnIsGatewayOrNot()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();

				var app = consol.GetApportionments(false);
				var cost = app.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				cost.E6_GC = GlbCompany.CurrentCompany.PK;
				cost.E6_OSCostAmount = 300m;
				cost.E6_LocalCostAmount = 300m;
				Factory.Save();

				var costCollection = new JobConsolCostCollection(Factory, consol, true);
				costCollection.Load();
				AssertEquals(1, costCollection.Count);
				cost = costCollection[0];
				Assert(cost.IsGatewayConsolCost);
				Assert(!cost.E6_GatewaySellChargeID.IsEmpty);
				AssertEquals(250m, cost.E6_LocalCostAmount);

				costCollection = new JobConsolCostCollection(Factory, consol, false);
				costCollection.Load();
				AssertEquals(1, costCollection.Count);
				cost = costCollection[0];
				Assert(!cost.IsGatewayConsolCost);
				Assert(cost.E6_GatewaySellChargeID.IsEmpty);
				AssertEquals(300m, cost.E6_LocalCostAmount);
			}
		}

		public void TestJobConsolCostCollectionLoadCorrectlyWhenGatewaySellChargeIsDeleted()
		{
			var gatewayAgentPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(gatewayAgentPort);
			consol.JK_UniqueConsignRef = "C00001111";
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			using (var job = new Job.Loader(Factory, consol).TryCreateWithoutMutexForTestOnly())
			{
				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
				var charge = job.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				Assert(consol.IsGateway());
				Factory.Save();

				charge.Delete();
				var costCollection = new JobConsolCostCollection(Factory, consol, true);
				costCollection.Load();
				AssertEquals(1, costCollection.Count);
				var cost = costCollection[0];
				Assert(cost.IsGatewayConsolCost);
				AssertEquals(250m, cost.E6_LocalCostAmount);
			}
		}

		public void TestConsolJobInvoicingEnterOrModifyCheckPoint()
		{
			Assert("Pre-condition", !Consol.IsGateway());
			var costCollection = Consol.GetApportionments().CostsCollection;
			AssertEquals(Env.Security.MaintainConsolJobInvoicingEnterOrModify, costCollection.ConsolJobInvoicingEnterOrModifyCheckPoint);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "CNSHA";
			Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Assert("Expected to be a Gateway Consol now", Consol.IsGateway());
			var gatewayCostCollection = Consol.GetApportionments(true).CostsCollection;
			AssertEquals(Env.Security.GatewayConsolJobInvoicingEnterOrModify, gatewayCostCollection.ConsolJobInvoicingEnterOrModifyCheckPoint);
		}

		public void TestAllowRemoveCorePerpertyEqualsWithConsolJobInvoicingDeleteSecurityCheckPointSetting()
		{
			Assert("Pre-condition", !Consol.IsGateway());
			var costCollection = Consol.GetApportionments().CostsCollection;
			Env.Security.MaintainConsolJobInvoicingDelete.IsAllowed = false;
			Assert("Delete should not be allowed.", !costCollection.AllowRemoveCoreForTest);
			Env.Security.MaintainConsolJobInvoicingDelete.IsAllowed = true;
			Assert("Delete should be allowed.", costCollection.AllowRemoveCoreForTest);
		}

		public void TestTryAddNew()
		{
			Assert("Pre-condition", !Consol.IsGateway());
			var consolCost = TestCollection.TryAddNew();
			AssertNotNull("Should be allowed to add new if Consol is not Gateway", consolCost);
			consolCost.Delete();

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "CNSHA";
			Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Assert("Expected to be a Gateway Consol now", Consol.IsGateway());
			Assert(!TestCollection.IsUsedForGateway);
			consolCost = TestCollection.TryAddNew();
			AssertNotNull("Collection should be allowed to add new on Gateway Consol as IsUsedForGateway = false", consolCost);

			var gwJobConsolCostCollection = new JobConsolCostCollection(Factory, Consol, true);
			Assert(gwJobConsolCostCollection.IsUsedForGateway);
			consolCost = gwJobConsolCostCollection.TryAddNew();
			AssertNull("Collection should not be allowed to add new on Gateway Consol as IsUsedForGateway = true", consolCost);

			Consol.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost = gwJobConsolCostCollection.TryAddNew();
				AssertNotNull("Should be allowed to add new on Gateway Consol when EnforceCreatingConsolCosts is set on it", consolCost);
			}
			finally
			{
				Consol.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public override void TestAddNew()
		{
			Factory.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				base.TestAddNew();
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		public override void TestTypedAddNew()
		{
			Factory.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				base.TestTypedAddNew();
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobConsolCostCollection(Factory, Consol);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<JobConsolCost>();
			using (result.ReportSettingParentSuspender.GetSuspender())
			{
				result.SetE6_ParentIDAndE6_ParentTableCodeTogether(Consol.PK, Consol.TablePrefix);
			}
			return result;
		}

		ForwardingConsol Consol
		{
			get { return fConsol ?? (fConsol = Factory.New<ForwardingConsol>()); }
		}

		ForwardingConsol fConsol;

		JobConsolCostCollection TestCollection
		{
			get { return Collection as JobConsolCostCollection; }
		}

		#endregion
	}
}
