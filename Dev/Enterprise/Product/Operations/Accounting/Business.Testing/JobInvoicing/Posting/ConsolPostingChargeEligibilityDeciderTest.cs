using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class ConsolPostingChargeEligibilityDeciderTest : PostingChargeEligibilityDeciderTest
	{
		public void TestIsAgentCharge()
		{
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C123");
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C321");
			var shipment = TestObjectCreator.CreateShipment("S123", consol1);
			consol2.Shipments.Add(shipment);

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				var blankCharge = shipmentJob.Charges.AddNew();
				var decider1 = new ConsolPostingChargeEligibilityDecider(consol1, new[] { blankCharge });
				var decider2 = new ConsolPostingChargeEligibilityDecider(consol2, new[] { blankCharge });

				AssertEquals(consol1.IsAgentCharge(blankCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(blankCharge));
				AssertEquals(consol2.IsAgentCharge(blankCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(blankCharge));

				var pickupAgent = TestObjectCreator.CreateOrgHeader("PKA", false, true);
				shipment.PickupAgentPK = pickupAgent.PK;
				var pickupCharge = shipmentJob.Charges.AddNew();
				pickupCharge.JR_OH_SellAccount = pickupAgent.PK;
				pickupCharge.JR_LocalSellAmt = 100;

				var deliveryAgent = TestObjectCreator.CreateOrgHeader("DVA", false, true);
				shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
				var deliveryCharge = shipmentJob.Charges.AddNew();
				deliveryCharge.JR_OH_SellAccount = deliveryAgent.PK;
				deliveryCharge.JR_LocalSellAmt = 100;

				var overseasAgent = TestObjectCreator.CreateOrgHeader("OSA", false, true);
				shipmentJob.AgentCollectPK = overseasAgent.PK;
				var overseasCharge = shipmentJob.Charges.AddNew();
				overseasCharge.JR_OH_SellAccount = overseasAgent.PK;
				overseasCharge.JR_LocalSellAmt = 100;

				var consol1SendingAgent = TestObjectCreator.CreateOrgHeader("C1S", false, true);
				consol1.JK_OA_SendingForwarderAddress = consol1SendingAgent.MainAddress.PK;
				var consol1SendingCharge = shipmentJob.Charges.AddNew();
				consol1SendingCharge.JR_OH_SellAccount = consol1SendingAgent.PK;
				consol1SendingCharge.JR_LocalSellAmt = 100;

				var consol1ReceivingAgent = TestObjectCreator.CreateOrgHeader("C1R", false, true);
				consol1.JK_OA_ReceivingForwarderAddress = consol1ReceivingAgent.MainAddress.PK;
				var consol1ReceivingCharge = shipmentJob.Charges.AddNew();
				consol1ReceivingCharge.JR_OH_SellAccount = consol1ReceivingAgent.PK;
				consol1ReceivingCharge.JR_LocalSellAmt = 100;

				var consol1ReceivingRelatedParty = TestObjectCreator.CreateOrgHeader("RP1", false, true);
				consol1ReceivingAgent.SetRelatedParty(consol1ReceivingRelatedParty.PK, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				var consol1ReceivingRelatedPartyCharge = shipmentJob.Charges.AddNew();
				consol1ReceivingRelatedPartyCharge.JR_OH_SellAccount = consol1ReceivingRelatedParty.PK;
				consol1ReceivingRelatedPartyCharge.JR_LocalSellAmt = 100;

				var consol2SendingAgent = TestObjectCreator.CreateOrgHeader("C2S", false, true);
				consol2.JK_OA_SendingForwarderAddress = consol2SendingAgent.MainAddress.PK;
				var consol2SendingCharge = shipmentJob.Charges.AddNew();
				consol2SendingCharge.JR_OH_SellAccount = consol2SendingAgent.PK;
				consol2SendingCharge.JR_LocalSellAmt = 100;

				var consol2ReceivingAgent = TestObjectCreator.CreateOrgHeader("C2R", false, true);
				consol2.JK_OA_ReceivingForwarderAddress = consol2ReceivingAgent.MainAddress.PK;
				var consol2ReceivingCharge = shipmentJob.Charges.AddNew();
				consol2ReceivingCharge.JR_OH_SellAccount = consol2ReceivingAgent.PK;
				consol2ReceivingCharge.JR_LocalSellAmt = 100;

				var consol2ReceivingRelatedParty = TestObjectCreator.CreateOrgHeader("RP2", false, true);
				consol2ReceivingAgent.SetRelatedParty(consol2ReceivingRelatedParty.PK, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
				var consol2ReceivingRelatedPartyCharge = shipmentJob.Charges.AddNew();
				consol2ReceivingRelatedPartyCharge.JR_OH_SellAccount = consol2ReceivingRelatedParty.PK;
				consol2ReceivingRelatedPartyCharge.JR_LocalSellAmt = 100;

				var charges = new[] { pickupCharge, deliveryCharge, overseasCharge, consol1SendingCharge, consol1ReceivingCharge, consol1ReceivingRelatedPartyCharge, consol2SendingCharge, consol2ReceivingCharge, consol2ReceivingRelatedPartyCharge };
				decider1 = new ConsolPostingChargeEligibilityDecider(consol1, charges);
				decider2 = new ConsolPostingChargeEligibilityDecider(consol2, charges);

				AssertMatches();

				shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
				AssertMatches();

				void AssertMatches()
				{
					AssertEquals(consol1.IsAgentCharge(pickupCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(pickupCharge));
					AssertEquals(consol2.IsAgentCharge(pickupCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(pickupCharge));

					AssertEquals(consol1.IsAgentCharge(deliveryCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(deliveryCharge));
					AssertEquals(consol2.IsAgentCharge(deliveryCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(deliveryCharge));

					AssertEquals(consol1.IsAgentCharge(overseasCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(overseasCharge));
					AssertEquals(consol2.IsAgentCharge(overseasCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(overseasCharge));

					AssertEquals(consol1.IsAgentCharge(consol1SendingCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol1SendingCharge));
					AssertEquals(consol2.IsAgentCharge(consol2SendingCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol2SendingCharge));

					AssertEquals(consol1.IsAgentCharge(consol1ReceivingCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol1ReceivingCharge));
					AssertEquals(consol2.IsAgentCharge(consol2ReceivingCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol2ReceivingCharge));

					AssertEquals(consol1.IsAgentCharge(consol2SendingCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol2SendingCharge));
					AssertEquals(consol1.IsAgentCharge(consol2ReceivingCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol2ReceivingCharge));
					AssertEquals(consol2.IsAgentCharge(consol1SendingCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol1SendingCharge));
					AssertEquals(consol2.IsAgentCharge(consol1ReceivingCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol1ReceivingCharge));

					AssertEquals(consol1.IsAgentCharge(consol1ReceivingRelatedPartyCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol1ReceivingRelatedPartyCharge));
					AssertEquals(consol2.IsAgentCharge(consol2ReceivingRelatedPartyCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol2ReceivingRelatedPartyCharge));
					AssertEquals(consol2.IsAgentCharge(consol1ReceivingRelatedPartyCharge), decider2.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol1ReceivingRelatedPartyCharge));
					AssertEquals(consol1.IsAgentCharge(consol2ReceivingRelatedPartyCharge), decider1.GetEligibleCharges(JobInvoicingPostingOption.Agent).Contains(consol2ReceivingRelatedPartyCharge));
				}
			}
		}

		public override void TestEligibilityForPostingGatewayCharges()
		{
			var receivingAgent1 = TestObjectCreator.CreateOrgHeader("GWRORG1", false, true);
			var receivingGatewayCompany1 = TestObjectCreator.CreateNewCompany("GR1", orgProxy: receivingAgent1);
			var sendingAgent1 = TestObjectCreator.CreateOrgHeader("GWSORG1", false, true);
			var sendingGatewayCompany1 = TestObjectCreator.CreateNewCompany("GS1", orgProxy: sendingAgent1);

			var consol1 = TestObjectCreator.CreateGatewayConsol(consolNum: "GC001", sendingGatewayCompany: sendingGatewayCompany1, receivingGatewayCompany: receivingGatewayCompany1);
			consol1.JK_SendingForwarderHandlingType = ZString.Empty;
			consol1.JK_ReceivingForwarderHandlingType = ZString.Empty;

			var receivingAgent2 = TestObjectCreator.CreateOrgHeader("GWRORG2", false, true);
			var receivingGatewayCompany2 = TestObjectCreator.CreateNewCompany("GR2", orgProxy: receivingAgent2);
			var sendingAgent2 = TestObjectCreator.CreateOrgHeader("GWSORG2", false, true);
			var sendingGatewayCompany2 = TestObjectCreator.CreateNewCompany("GS2", orgProxy: sendingAgent2);

			var consol2 = TestObjectCreator.CreateGatewayConsol(consolNum: "GC002", sendingGatewayCompany: sendingGatewayCompany2, receivingGatewayCompany: receivingGatewayCompany2);
			consol2.JK_SendingForwarderHandlingType = ZString.Empty;
			consol2.JK_ReceivingForwarderHandlingType = ZString.Empty;

			var nonAgent = TestObjectCreator.CreateOrgHeader("NOA", false, true);

			var shipment = TestObjectCreator.CreateShipment("S00123", consol1);
			shipment.Consols.Add(consol2);
			Factory.Save();

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				var charge1 = shipmentJob.Charges.AddNew();
				charge1.JR_LocalSellAmt = 100;
				var charge2 = shipmentJob.Charges.AddNew();
				charge2.JR_LocalSellAmt = 100;

				consol1.JK_OA_SendingForwarderAddress = sendingAgent1.MainAddress.PK;
				consol1.JK_SendingForwarderHandlingType = ZString.Empty;
				consol1.JK_OA_ReceivingForwarderAddress = receivingAgent1.MainAddress.PK;
				consol1.JK_ReceivingForwarderHandlingType = ZString.Empty;

				consol2.JK_OA_SendingForwarderAddress = sendingAgent2.MainAddress.PK;
				consol2.JK_SendingForwarderHandlingType = ZString.Empty;
				consol2.JK_OA_ReceivingForwarderAddress = receivingAgent2.MainAddress.PK;
				consol2.JK_ReceivingForwarderHandlingType = ZString.Empty;

				var decider1 = new ConsolPostingChargeEligibilityDecider(consol1, new[] { charge1, charge2 });
				var decider2 = new ConsolPostingChargeEligibilityDecider(consol2, new[] { charge1, charge2 });

				Assert("Pre-condition: Charge code not in GW Charge Code registry item, should post all charge codes when registry has no value", AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.GetAsGuidArray().IsNullOrEmpty());
				Assert("Pre-condition: Charge debtor is null", charge1.JR_OH_SellAccount.IsEmpty);
				Assert("Pre-condition: Charge debtor is null", charge2.JR_OH_SellAccount.IsEmpty);
				AssertGetEligibleGatewayCharges();

				charge1.JR_OH_SellAccount = nonAgent.PK;
				charge2.JR_OH_SellAccount = nonAgent.PK;
				AssertGetEligibleGatewayCharges();

				charge1.JR_OH_SellAccount = sendingAgent1.PK;
				charge2.JR_OH_SellAccount = sendingAgent2.PK;
				AssertGetEligibleGatewayCharges();
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertGetEligibleGatewayCharges();
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertGetEligibleGatewayCharges();

				charge1.JR_OH_SellAccount = receivingAgent1.PK;
				charge2.JR_OH_SellAccount = receivingAgent2.PK;
				AssertGetEligibleGatewayCharges();
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertGetEligibleGatewayCharges();
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertGetEligibleGatewayCharges();

				AssertNull("Pre-condition: Charge code is null", charge1.ChargeCode);
				AssertNull("Pre-condition: Charge code is null", charge2.ChargeCode);
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Factory.NewWithValidTestData<AccChargeCode>().PK.ToString());
				AssertGetEligibleGatewayCharges();

				var chargeCode = TestObjectCreator.CreateChargeCode("COD");
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_LocalSellAmt = 100;
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_LocalSellAmt = 100;
				AssertGetEligibleGatewayCharges();

				var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode(chargeCode.AC_Code);
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString());
				AssertGetEligibleGatewayCharges();

				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Factory.NewWithValidTestData<AccChargeCode>().PK.ToString());
				AssertGetEligibleGatewayCharges();
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToString());
				AssertGetEligibleGatewayCharges();

				void AssertGetEligibleGatewayCharges()
				{
					AssertEquals(consol1.IsGatewayCharge(charge1), decider1.GetEligibleCharges(JobInvoicingPostingOption.Gateway).Contains(charge1));
					AssertEquals(consol2.IsGatewayCharge(charge2), decider2.GetEligibleCharges(JobInvoicingPostingOption.Gateway).Contains(charge2));
					AssertEquals(consol2.IsGatewayCharge(charge1), decider2.GetEligibleCharges(JobInvoicingPostingOption.Gateway).Contains(charge1));
					AssertEquals(consol1.IsGatewayCharge(charge2), decider1.GetEligibleCharges(JobInvoicingPostingOption.Gateway).Contains(charge2));
				}
			}
		}

		protected override PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			return new ConsolPostingChargeEligibilityDecider(Consol, charges);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
		}

		IJobCostingPlugIn Consol;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
