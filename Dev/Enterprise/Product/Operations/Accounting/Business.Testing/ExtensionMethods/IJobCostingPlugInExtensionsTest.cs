using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.Testing
{
	public class JobCostingPlugInHelpersTest : TestCaseWithFactory
	{
		public void TestIsUsedBy()
		{
			AssertType<JobCostingPlugInHelpers>(ObjectFactory.Get<IAccountingDependencyFactory>().GetJobCostingPlugInHelpers());
		}

		public void TestFindBranchFromConsolAgentsAndJobHeaders()
		{
			var helper = (IJobCostingPlugInHelpers)new JobCostingPlugInHelpers();
			IJobCostingPlugInExtensionsTest.AssertFindBranchFromConsolAgentsAndJobHeaders(Factory, (consol, companyPK) => helper.FindBranchFromConsolAgentsAndJobHeaders(consol, companyPK, Factory));
		}
	}

	public class IJobCostingPlugInExtensionsTest : TestCaseWithFactory
	{
		public void TestFindBranchFromConsolAgentsAndJobHeaders()
		{
			AssertFindBranchFromConsolAgentsAndJobHeaders(Factory, (consol, companyPK) => consol.FindBranchFromConsolAgentsAndJobHeaders(companyPK, Factory));
		}

		public static void AssertFindBranchFromConsolAgentsAndJobHeaders(BusinessObjectFactory factory, Func<ForwardingConsol, ZGuid, GlbBranch> findBranchFromConsolAgentsAndJobHeaders)
		{
			var creator = new TestObjectCreator(factory);
			var newCompany = factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "ABC";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "ABC";
			newBranch.GB_IsActive = true;
			var sendingForwarder = factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarder = factory.NewWithValidTestData<OrgHeader>();
			var departureCFS = factory.NewWithValidTestData<OrgHeader>();
			var arrivalCFS = factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.AddRelatedParty(departureCFS.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			receivingForwarder.AddRelatedParty(arrivalCFS.PK, RelatedPartyTypeList.Codes.ForwarderCFS, RelatedPartyDirectionList.Codes.Forwarder, Constants.TransportModes.All, ZString.Empty, GlbCompany.CurrentCompany);
			sendingForwarder.AllRelatedParties[0].PR_Location = "AUSYD";
			receivingForwarder.AllRelatedParties[0].PR_Location = "NZAKL";
			newBranch.GB_OH_OrgProxy = sendingForwarder.PK;
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			factory.Save();
			var branch = findBranchFromConsolAgentsAndJobHeaders(consol, newCompany.PK);
			AssertNotNull("Should found the newBranch", branch);
			AssertEquals("Should found the newBranch", newBranch.PK, branch.PK);
			newBranch.GB_IsActive = false;
			factory.Save();
			branch = findBranchFromConsolAgentsAndJobHeaders(consol, newCompany.PK);
			AssertNull("Should not found the newBranch as it is inactive", branch);
			var shipment = creator.CreateShipment("S001", consol);
			var job = creator.CreateJob(shipment, false);
			factory.Save();
			branch = findBranchFromConsolAgentsAndJobHeaders(consol, newCompany.PK);
			AssertNotNull("Should found the branch of job", branch);
			AssertEquals("Should found the branch of job", job.Branch.PK, branch.PK);
		}

		public void TestGetApportionmentMethodDefault()
		{
			AssertGetApportionmentMethod(
				registrySettings: null,
				("Empty when consol is null",
					() => null, TestObjectCreator.FRT, ZString.Empty),
				("Empty when charge code is null",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Air, ApportionmentMethodModules.Forwarding, ZString.Empty), null, ZString.Empty),
				("Default should be Chargeable Units (CHG)",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Air, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.ChargeableUnits)
			);
		}

		public void TestGetApportionmentMethodMethodsPrioritised()
		{
			var chargeCodeApportionmentOverirde = TestObjectCreator.RevenueNoTaxChargeCode.ApportionmentMethodOverrides.AddNew();
			chargeCodeApportionmentOverirde.AAM_Module = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ContainerMode = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_TransportMode = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ConsolType = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_Direction = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ApportionmentMethod = AllocationMethod.GrossWeight;

			var chargeCodeApportionmentOverirde2 = TestObjectCreator.RevenueNoTaxChargeCode.ApportionmentMethodOverrides.AddNew();
			chargeCodeApportionmentOverirde2.AAM_Module = ApportionmentMethodModules.Forwarding;
			chargeCodeApportionmentOverirde2.AAM_ContainerMode = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde2.AAM_TransportMode = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde2.AAM_ConsolType = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde2.AAM_Direction = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde2.AAM_ApportionmentMethod = AllocationMethod.Revenue;

			AssertGetApportionmentMethod(
				registrySettings: new[] {
					(ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, Constants.TransportModes.Air,  ApportionmentMethod.AllCode, Constants.ContainerModes.Loose, AllocationMethod.Manual),
					(ApportionmentMethodModules.Forwarding, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, AllocationMethod.CapacityPerContainer),
				},
				("Consol should match closest Apportionment Method",
					() => new MockIJobCosting(Constants.ContainerModes.Loose, ZString.Empty, Constants.TransportModes.Air, ZString.Empty, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.Manual),
				("Consol should match closest Apportionment Method",
					() => new MockIJobCosting(Constants.ContainerModes.Loose, ZString.Empty, Constants.TransportModes.Air, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.CapacityPerContainer),
				("Consol should match closest Apportionment Method, charge code first",
					() => new MockIJobCosting(Constants.ContainerModes.Loose, ZString.Empty, Constants.TransportModes.Air, ZString.Empty, ZString.Empty), TestObjectCreator.RevenueNoTaxChargeCode, AllocationMethod.GrossWeight),
				("Consol should match closest Apportionment Method, charge code first",
					() => new MockIJobCosting(Constants.ContainerModes.Loose, ZString.Empty, Constants.TransportModes.Air, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.RevenueNoTaxChargeCode, AllocationMethod.Revenue)
			);
		}

		public void TestGetApportionmentMethodCorrectDirectionChosen()
		{
			var chargeCodeApportionmentOverirde = TestObjectCreator.RevenueNoTaxChargeCode.ApportionmentMethodOverrides.AddNew();
			chargeCodeApportionmentOverirde.AAM_Module = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ContainerMode = Constants.ContainerModes.FCL;
			chargeCodeApportionmentOverirde.AAM_TransportMode = Constants.TransportModes.Sea;
			chargeCodeApportionmentOverirde.AAM_ConsolType = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_Direction = Constants.CartageDirection.Import;
			chargeCodeApportionmentOverirde.AAM_ApportionmentMethod = AllocationMethod.GrossWeight;

			AssertGetApportionmentMethod(
				registrySettings: new[] {
					(ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, Constants.TransportModes.Sea, ApportionmentMethod.AllCode, Constants.ContainerModes.FCL, AllocationMethod.ChargeableUnits),
					(ApportionmentMethod.AllCode, Constants.CartageDirection.Import, Constants.TransportModes.Sea, ApportionmentMethod.AllCode, Constants.ContainerModes.FCL, AllocationMethod.Shipment),
				},
				("Consol should match best select",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Sea, ApportionmentMethodModules.Forwarding, Constants.CartageDirection.Export), TestObjectCreator.FRT, AllocationMethod.ChargeableUnits),
				("Consol should match best select",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Sea, ApportionmentMethodModules.Forwarding, Constants.CartageDirection.Import), TestObjectCreator.FRT, AllocationMethod.Shipment),
				("Consol should match best select, charge code first",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Sea, ApportionmentMethodModules.Forwarding, Constants.CartageDirection.Import), TestObjectCreator.RevenueNoTaxChargeCode, AllocationMethod.GrossWeight)
			);
		}

		public void TestGetApportionmentMethodCorrectChosen()
		{
			var chargeCodeApportionmentOverirde = TestObjectCreator.RevenueNoTaxChargeCode.ApportionmentMethodOverrides.AddNew();
			chargeCodeApportionmentOverirde.AAM_Module = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ContainerMode = Constants.ContainerModes.FCL;
			chargeCodeApportionmentOverirde.AAM_TransportMode = Constants.TransportModes.Sea;
			chargeCodeApportionmentOverirde.AAM_ConsolType = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_Direction = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ApportionmentMethod = AllocationMethod.GrossWeight;

			AssertGetApportionmentMethod(
				registrySettings: new[] {
					(ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, Constants.TransportModes.Sea, ApportionmentMethod.AllCode, Constants.ContainerModes.FCL, AllocationMethod.Shipment),
				},
				("Consol should not match",
					() => new MockIJobCosting(Constants.ContainerModes.LCL, ZString.Empty, Constants.TransportModes.Sea, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.ChargeableUnits),
				("Consol should now match",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Sea, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.Shipment),
				("Consol should now match, charge code first",
					() => new MockIJobCosting(Constants.ContainerModes.FCL, ZString.Empty, Constants.TransportModes.Sea, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.RevenueNoTaxChargeCode, AllocationMethod.GrossWeight)
			);
		}

		public void TestGetApportionmentMethodObtainsModule()
		{
			var chargeCodeApportionmentOverirde = TestObjectCreator.RevenueNoTaxChargeCode.ApportionmentMethodOverrides.AddNew();
			chargeCodeApportionmentOverirde.AAM_Module = ApportionmentMethodModules.Forwarding;
			chargeCodeApportionmentOverirde.AAM_ContainerMode = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_TransportMode = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ConsolType = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_Direction = ApportionmentMethod.AllCode;
			chargeCodeApportionmentOverirde.AAM_ApportionmentMethod = AllocationMethod.GrossWeight;

			AssertGetApportionmentMethod(
				registrySettings: new[] {
					(ApportionmentMethodModules.Forwarding, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, AllocationMethod.Shipment),
					(ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, Constants.TransportModes.Air, ApportionmentMethod.AllCode, ApportionmentMethod.AllCode, AllocationMethod.Manual)
				},
				("Forwarding consol should match forwarding module",
					() => new MockIJobCosting(ZString.Empty, ZString.Empty, ZString.Empty, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.Shipment),
				("Forwarding consol should match forwarding module, charge code first",
					() => new MockIJobCosting(ZString.Empty, ZString.Empty, ZString.Empty, ApportionmentMethodModules.Forwarding, ZString.Empty), TestObjectCreator.RevenueNoTaxChargeCode, AllocationMethod.GrossWeight),
				("Mock consol ,match TransportModes:Air",
					() => new MockIJobCosting(ZString.Empty, ZString.Empty, Constants.TransportModes.Air, ApportionmentMethodModules.TransportBooking, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.Manual),
				("Mock consol ,not match any",
					() => new MockIJobCosting(ZString.Empty, ZString.Empty, Constants.TransportModes.Road, ApportionmentMethodModules.TransportBooking, ZString.Empty), TestObjectCreator.FRT, AllocationMethod.ChargeableUnits)
			);
		}

		void AssertGetApportionmentMethod(
			(string Module, string Direction, string TransportMode, string ConsolType, string ContainerMode, string ApportionmentMethod)[] registrySettings,
			params (string Comment, Func<IJobCostingPlugIn> JobCostingGetter, AccChargeCode ChargeCode, string ExpectedResult)[] asserts
			)
		{
			var config = new ConsolCostDefaultApportionmentMethodConfiguration();
			config.ConsolCostDefaultApportionmentMethodCollection.RemoveAll();

			using (config.SuspendValidationTesting())
			{
				if (registrySettings != null)
				{
					foreach (var setting in registrySettings)
					{
						config.ConsolCostDefaultApportionmentMethodCollection.Add(new ConsolCostDefaultApportionmentMethod
						{
							ConsolType = setting.ConsolType,
							TransportMode = setting.TransportMode,
							ContainerMode = setting.ContainerMode,
							Module = setting.Module,
							Direction = setting.Direction,
							Apportionment = setting.ApportionmentMethod,
						});
					}
				}

				AccountingConfigurationRegistry.Instance.ConsolCostDefaultApportionmentMethod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, config);

				Factory.Save();
			}

			CombineAssertions(() => {
				foreach (var assert in asserts)
				{
					var jobCosting = assert.JobCostingGetter();
					AssertEquals(assert.Comment, assert.ExpectedResult, jobCosting.GetApportionmentMethod(assert.ChargeCode));
				}
			});
		}

		public void TestAgentToInvoice()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Road;
			consol.JK_RL_NKLoadPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			Factory.Save();
			AssertEquals(true, consol.IsLoadPortLocal());
			AssertEquals(receivingForwarder.PK, consol.AgentToInvoice().PK);

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			Factory.Save();
			AssertEquals(false, consol.IsLoadPortLocal());
			AssertEquals(sendingForwarder.PK, consol.AgentToInvoice().PK);
		}

		public void TestIsAgentCharge()
		{
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C123");
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C321");
			var shipment = TestObjectCreator.CreateShipment("S123", consol1);
			consol2.Shipments.Add(shipment);

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				var blankCharge = shipmentJob.Charges.AddNew();
				Assert(!consol1.IsAgentCharge(blankCharge));
				Assert(!consol2.IsAgentCharge(blankCharge));

				var pickupAgent = TestObjectCreator.CreateOrgHeader("PKA", false, true);
				shipment.PickupAgentPK = pickupAgent.PK;
				var pickupCharge = shipmentJob.Charges.AddNew();
				pickupCharge.JR_OH_SellAccount = pickupAgent.PK;

				var deliveryAgent = TestObjectCreator.CreateOrgHeader("DVA", false, true);
				shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
				var deliveryCharge = shipmentJob.Charges.AddNew();
				deliveryCharge.JR_OH_SellAccount = deliveryAgent.PK;

				var overseasAgent = TestObjectCreator.CreateOrgHeader("OSA", false, true);
				shipmentJob.AgentCollectPK = overseasAgent.PK;
				var overseasCharge = shipmentJob.Charges.AddNew();
				overseasCharge.JR_OH_SellAccount = overseasAgent.PK;

				var consol1SendingAgent = TestObjectCreator.CreateOrgHeader("C1S", false, true);
				consol1.JK_OA_SendingForwarderAddress = consol1SendingAgent.MainAddress.PK;
				var consol1SendingCharge = shipmentJob.Charges.AddNew();
				consol1SendingCharge.JR_OH_SellAccount = consol1SendingAgent.PK;

				var consol1ReceivingAgent = TestObjectCreator.CreateOrgHeader("C1R", false, true);
				consol1.JK_OA_ReceivingForwarderAddress = consol1ReceivingAgent.MainAddress.PK;
				var consol1ReceivingCharge = shipmentJob.Charges.AddNew();
				consol1ReceivingCharge.JR_OH_SellAccount = consol1ReceivingAgent.PK;

				var consol1ReceivingRelatedParty = TestObjectCreator.CreateOrgHeader("RP1", false, true);
				consol1ReceivingAgent.SetRelatedParty(consol1ReceivingRelatedParty.PK, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				var consol1ReceivingRelatedPartyCharge = shipmentJob.Charges.AddNew();
				consol1ReceivingRelatedPartyCharge.JR_OH_SellAccount = consol1ReceivingRelatedParty.PK;
				consol1ReceivingRelatedPartyCharge.JR_LocalSellAmt = 100;

				var consol2SendingAgent = TestObjectCreator.CreateOrgHeader("C2S", false, true);
				consol2.JK_OA_SendingForwarderAddress = consol2SendingAgent.MainAddress.PK;
				var consol2SendingCharge = shipmentJob.Charges.AddNew();
				consol2SendingCharge.JR_OH_SellAccount = consol2SendingAgent.PK;

				var consol2ReceivingAgent = TestObjectCreator.CreateOrgHeader("C2R", false, true);
				consol2.JK_OA_ReceivingForwarderAddress = consol2ReceivingAgent.MainAddress.PK;
				var consol2ReceivingCharge = shipmentJob.Charges.AddNew();
				consol2ReceivingCharge.JR_OH_SellAccount = consol2ReceivingAgent.PK;

				var consol2ReceivingRelatedParty = TestObjectCreator.CreateOrgHeader("RP2", false, true);
				consol2ReceivingAgent.SetRelatedParty(consol2ReceivingRelatedParty.PK, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				var consol2ReceivingRelatedPartyCharge = shipmentJob.Charges.AddNew();
				consol2ReceivingRelatedPartyCharge.JR_OH_SellAccount = consol2ReceivingRelatedParty.PK;
				consol2ReceivingRelatedPartyCharge.JR_LocalSellAmt = 100;

				Assert("Pickup Agent is not a valid overseas agent", !consol1.IsAgentCharge(pickupCharge));
				Assert("Pickup Agent is not a valid overseas agent", !consol2.IsAgentCharge(pickupCharge));

				Assert("Delivery Agent should be used in preference of Consol Receiving Agent", consol1.IsAgentCharge(deliveryCharge));
				Assert("Delivery Agent should be used in preference of Consol Receiving Agent", consol2.IsAgentCharge(deliveryCharge));

				Assert(consol1.IsAgentCharge(overseasCharge));
				Assert(consol2.IsAgentCharge(overseasCharge));

				Assert(consol1.IsAgentCharge(consol1SendingCharge));
				Assert(consol2.IsAgentCharge(consol2SendingCharge));

				Assert("Delivery Agent should be used in preference of Consol Receiving Agent", !consol1.IsAgentCharge(consol1ReceivingCharge));
				Assert("Delivery Agent should be used in preference of Consol Receiving Agent", !consol2.IsAgentCharge(consol2ReceivingCharge));

				Assert("Different Consol's agents are not valid", !consol1.IsAgentCharge(consol2SendingCharge));
				Assert("Different Consol's agents are not valid", !consol1.IsAgentCharge(consol2ReceivingCharge));
				Assert("Different Consol's agents are not valid", !consol2.IsAgentCharge(consol1SendingCharge));
				Assert("Different Consol's agents are not valid", !consol2.IsAgentCharge(consol1ReceivingCharge));

				Assert("Related Netting party is valid Agent Debtor for posting", consol1.IsAgentCharge(consol1ReceivingRelatedPartyCharge));
				Assert("Related Netting party is valid Agent Debtor for posting", consol2.IsAgentCharge(consol2ReceivingRelatedPartyCharge));
				Assert("Related Netting party of different consol is not valid Agent Debtor for posting", !consol2.IsAgentCharge(consol1ReceivingRelatedPartyCharge));
				Assert("Related Netting party of different consol is not valid Agent Debtor for posting", !consol1.IsAgentCharge(consol2ReceivingRelatedPartyCharge));

				shipment.JS_OH_DeliveryAgent = ZGuid.Empty;

				Assert("Pickup agent is not a valid overseas agent", !consol1.IsAgentCharge(pickupCharge));
				Assert("Pickup agent is not a valid overseas agent", !consol2.IsAgentCharge(pickupCharge));

				Assert("Delivery Agent is empty", !consol1.IsAgentCharge(deliveryCharge));
				Assert("Delivery Agent is empty", !consol2.IsAgentCharge(deliveryCharge));

				Assert(consol1.IsAgentCharge(overseasCharge));
				Assert(consol2.IsAgentCharge(overseasCharge));

				Assert(consol1.IsAgentCharge(consol1SendingCharge));
				Assert(consol2.IsAgentCharge(consol2SendingCharge));

				Assert("Receiving Agent is valid agent when Delivery Agent is empty", consol1.IsAgentCharge(consol1ReceivingCharge));
				Assert("Receiving Agent is valid agent when Delivery Agent is empty", consol2.IsAgentCharge(consol2ReceivingCharge));

				Assert("Different Consol's agents are not valid", !consol1.IsAgentCharge(consol2SendingCharge));
				Assert("Different Consol's agents are not valid", !consol1.IsAgentCharge(consol2ReceivingCharge));
				Assert("Different Consol's agents are not valid", !consol2.IsAgentCharge(consol1SendingCharge));
				Assert("Different Consol's agents are not valid", !consol2.IsAgentCharge(consol1ReceivingCharge));

				Assert("Related Netting party is valid Agent Debtor for posting", consol1.IsAgentCharge(consol1ReceivingRelatedPartyCharge));
				Assert("Related Netting party is valid Agent Debtor for posting", consol2.IsAgentCharge(consol2ReceivingRelatedPartyCharge));
				Assert("Related Netting party of different consol is not valid Agent Debtor for posting", !consol2.IsAgentCharge(consol1ReceivingRelatedPartyCharge));
				Assert("Related Netting party of different consol is not valid Agent Debtor for posting", !consol1.IsAgentCharge(consol2ReceivingRelatedPartyCharge));
			}
		}

		public void TestIsGatewayCharge()
		{
			var receivingAgent1 = TestObjectCreator.CreateOrgHeader("GWRORG1", false, true);
			var receivingGatewayCompany1 = TestObjectCreator.CreateNewCompany("GR1", orgProxy: receivingAgent1);
			var branchForReceivingGatewayCompany1 = TestObjectCreator.CreateNewBranch(receivingGatewayCompany1, "BR1");
			var sendingAgent1 = TestObjectCreator.CreateOrgHeader("GWSORG1", false, true);
			var sendingGatewayCompany1 = TestObjectCreator.CreateNewCompany("GS1", orgProxy: sendingAgent1);
			var branchForSendingGatewayCompany1 = TestObjectCreator.CreateNewBranch(sendingGatewayCompany1, "BS1");

			var consol1 = TestObjectCreator.CreateGatewayConsol(consolNum: "GC001", sendingGatewayCompany: sendingGatewayCompany1, receivingGatewayCompany: receivingGatewayCompany1);
			consol1.JK_SendingForwarderHandlingType = ZString.Empty;
			consol1.JK_ReceivingForwarderHandlingType = ZString.Empty;

			var receivingAgent2 = TestObjectCreator.CreateOrgHeader("GWRORG2", false, true);
			var receivingGatewayCompany2 = TestObjectCreator.CreateNewCompany("GR2", orgProxy: receivingAgent2);
			var branchForReceivingGatewayCompany2 = TestObjectCreator.CreateNewBranch(receivingGatewayCompany2, "BR2");
			var sendingAgent2 = TestObjectCreator.CreateOrgHeader("GWSORG2", false, true);
			var sendingGatewayCompany2 = TestObjectCreator.CreateNewCompany("GS2", orgProxy: sendingAgent2);
			var branchForSendingGatewayCompany2 = TestObjectCreator.CreateNewBranch(sendingGatewayCompany2, "BS2");

			var consol2 = TestObjectCreator.CreateGatewayConsol(consolNum: "GC002", sendingGatewayCompany: sendingGatewayCompany2, receivingGatewayCompany: receivingGatewayCompany2);
			consol2.JK_SendingForwarderHandlingType = ZString.Empty;
			consol2.JK_ReceivingForwarderHandlingType = ZString.Empty;

			var nonAgent = TestObjectCreator.CreateOrgHeader("NOA", false, true);

			var shipment = TestObjectCreator.CreateShipment("S00123", consol1);
			shipment.Consols.Add(consol2);

			var chargeCode = TestObjectCreator.CreateChargeCode("COD");
			var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode(chargeCode.AC_Code);

			Factory.Save();

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				var charge1 = shipmentJob.Charges.AddNew();
				charge1.JR_LocalSellAmt = 100;
				var charge2 = shipmentJob.Charges.AddNew();
				charge2.JR_LocalSellAmt = 100;

				Assert("Pre-condition: Charge code not in GW Charge Code registry item, should post all charge codes when registry has no value", AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.GetAsGuidArray().IsNullOrEmpty());
				Assert("Pre-condition: Charge debtor is null", charge1.JR_OH_SellAccount.IsEmpty);
				Assert("Pre-condition: Charge debtor is null", charge2.JR_OH_SellAccount.IsEmpty);
				AssertIsGatewayCharge(false);

				charge1.JR_OH_SellAccount = nonAgent.PK;
				charge2.JR_OH_SellAccount = nonAgent.PK;
				AssertIsGatewayCharge(false);

				charge1.JR_OH_SellAccount = sendingAgent1.PK;
				charge2.JR_OH_SellAccount = sendingAgent2.PK;
				AssertIsGatewayCharge(false);
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertIsGatewayCharge(true);

				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertIsGatewayCharge(true);

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchForSendingGatewayCompany1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertIsGatewayCharge(true);
				}
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchForSendingGatewayCompany2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertIsGatewayCharge(true);
				}

				charge1.JR_OH_SellAccount = receivingAgent1.PK;
				charge2.JR_OH_SellAccount = receivingAgent2.PK;
				AssertIsGatewayCharge(false);
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertIsGatewayCharge(true);
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertIsGatewayCharge(true);

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchForReceivingGatewayCompany1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertIsGatewayCharge(true);
				}
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchForReceivingGatewayCompany2.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertIsGatewayCharge(true);
				}

				AssertNull("Pre-condition: Charge code is null", charge1.ChargeCode);
				AssertNull("Pre-condition: Charge code is null", charge2.ChargeCode);
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Factory.NewWithValidTestData<AccChargeCode>().PK.ToString());
				AssertIsGatewayCharge(false, "should not be gateway if a specific charge code is required and the charge does not have a charge code");

				charge1.JR_AC = chargeCode.PK;
				charge2.JR_AC = chargeCode.PK;
				AssertIsGatewayCharge(false, "When registry is overriden, only those charge codes can be posted, gateway codes with matching codes should allow posting");

				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString());
				AssertIsGatewayCharge(true, "When registry is overriden, only those charge codes can be posted, gateway codes with matching codes should allow posting");

				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Factory.NewWithValidTestData<AccChargeCode>().PK.ToString());
				AssertIsGatewayCharge(false, "When registry is overriden, only those charge codes can be posted, actual charge code should allow posting");
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToString());
				AssertIsGatewayCharge(true, "When registry is overriden, only those charge codes can be posted, actual charge code should allow posting");

				void AssertIsGatewayCharge(bool expectedResult, string message = "")
				{
					AssertEquals(message, expectedResult, consol1.IsGatewayCharge(charge1));
					AssertEquals(message, expectedResult, consol2.IsGatewayCharge(charge2));

					Assert(!consol2.IsGatewayCharge(charge1));
					Assert(!consol1.IsGatewayCharge(charge2));
				}
			}
		}

		public void TestHasCostSupporterPK()
		{
			var mockJobCostingPlugInWithoutCostSupporterPK = new Mock<IJobCostingPlugIn>();
			var mockJobCostSupporterWithoutPK = new Mock<IGenericJobCostSupporter>();
			mockJobCostSupporterWithoutPK.SetupGet(jcs => jcs.PK).Returns(ZGuid.Empty);
			mockJobCostingPlugInWithoutCostSupporterPK.SetupGet(jcp => jcp.CostSupporter).Returns(mockJobCostSupporterWithoutPK.Object);

			var mockJobCostingPlugInWithCostSupporterPK = new Mock<IJobCostingPlugIn>();
			var mockJobCostSupporterWithPK = new Mock<IGenericJobCostSupporter>();
			mockJobCostSupporterWithPK.SetupGet(jcs => jcs.PK).Returns(new ZGuid("42205adf-b4cd-474f-8e86-114eab39d12d"));
			mockJobCostingPlugInWithCostSupporterPK.SetupGet(jcp => jcp.CostSupporter).Returns(mockJobCostSupporterWithPK.Object);

			CombineAssertions(() =>
			{
				AssertEquals("mockJobCostingPlugInWithoutCostSupporterPK.CostSupporter returns empty PK, so HasCostSupporterPK() should return false ", false, IJobCostingPlugInExtensions.HasCostSupporterPK(mockJobCostingPlugInWithoutCostSupporterPK.Object));
				AssertEquals("mockJobCostingPlugInWithCostSupporterPK.CostSupporter return non-empty PK, so HasCostSupporterPK() should return true", true, IJobCostingPlugInExtensions.HasCostSupporterPK(mockJobCostingPlugInWithCostSupporterPK.Object));
			});
		}

		class MockIJobCosting : IJobCostingPlugIn
		{
			public MockIJobCosting(ZString containerMode, ZString consolType, ZString transportMode, ZString module, ZString direction)
			{
				this.containerMode = containerMode;
				this.consolType = consolType;
				this.transportMode = transportMode;
				this.module = module;
				this.direction = direction;
			}

			readonly ZString containerMode;
			readonly ZString consolType;
			readonly ZString transportMode;
			readonly ZString module;
			readonly ZString direction;

			public ZGuid PK => throw new NotImplementedException();

			public ZString JK_UniqueConsignRef => throw new NotImplementedException();

			public RefUNLOCO LoadPort => throw new NotImplementedException();

			public RefUNLOCO DischargePort => throw new NotImplementedException();

			public JobProfitLossCollection ProfitLossContainer => throw new NotImplementedException();

			public decimal ConsolExchangeRate => throw new NotImplementedException();

			public RefCurrency ConsolCurrency => throw new NotImplementedException();

			public bool IsMasterCollect => throw new NotImplementedException();

			public OrgHeader ReceivingAgent => throw new NotImplementedException();

			public OrgHeader ReceivingAgentAPInvoicingParty => throw new NotImplementedException();

			public OrgHeader ReceivingAgentARInvoicingParty => throw new NotImplementedException();

			public OrgHeader SendingAgent => throw new NotImplementedException();

			public OrgHeader SendingAgentAPInvoicingParty => throw new NotImplementedException();

			public OrgHeader SendingAgentARInvoicingParty => throw new NotImplementedException();

			public ZString TransportMode => transportMode;

			public ZString ContainerMode => containerMode;

			public ZString ConsolType => consolType;

			public ZString Direction => direction;

			public ZString Module => module;

			public CodeDescriptionPairList PrepaidCollectList => throw new NotImplementedException();

			public IGenericJobCostSupporter CostSupporter => throw new NotImplementedException();

			public BusinessObjectFactory Factory => throw new NotImplementedException();

			public void AddNewToLogs(Event @event, ZString reference)
			{
				throw new NotImplementedException();
			}

			public decimal ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK)
			{
				throw new NotImplementedException();
			}

			public ZString GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob)
			{
				throw new NotImplementedException();
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fCreator ?? (fCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator fCreator;
	}
}
