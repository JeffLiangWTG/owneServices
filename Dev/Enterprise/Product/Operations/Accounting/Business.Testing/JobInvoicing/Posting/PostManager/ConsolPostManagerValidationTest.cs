using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class ConsolPostManagerValidationTest : PostManagerValidationTest
	{
		public override void TestRunOperationalTaxDateValidation()
		{
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption = collection.AddNew();
			taxDateOption.JobType = "SHP";
			taxDateOption.DirectionCode = "ALL";
			taxDateOption.Mode = "ALL";
			taxDateOption.Ledger = "AR";
			taxDateOption.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;

			var taxDateOption2 = collection.AddNew();
			taxDateOption2.JobType = "FCN";
			taxDateOption2.DirectionCode = "ALL";
			taxDateOption2.Mode = "ALL";
			taxDateOption2.Ledger = "AP";
			taxDateOption2.TaxDateOption = TaxDateDefaultingOption.Code.InvoiceDate;

			var taxDateOption3 = collection.AddNew();
			taxDateOption3.JobType = "SHP";
			taxDateOption3.DirectionCode = "ALL";
			taxDateOption3.Mode = "ALL";
			taxDateOption3.Ledger = "AP";
			taxDateOption3.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedDepartureDate;

			AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			{
				var consol1 = TestObjectCreator.CreateConsol(consolNum: "C123");
				var shipment = TestObjectCreator.CreateShipment("S123", consol1);
				shipment.JS_E_ARV = ZDateTime.Empty;
				shipment.JS_E_DEP = ZDateTime.Empty;

				using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
				{
					var jobs = new[] { shipmentJob };
					var overseasAgent = TestObjectCreator.CreateOrgHeader("OSA", false, true);
					var creditor = TestObjectCreator.CreateOrgHeader("TESTORG", true, false);
					shipmentJob.AgentCollectPK = overseasAgent.PK;
					var overseasCharge = shipmentJob.Charges.AddNew();
					var chargeCode = TestObjectCreator.CreateChargeCode("COD");
					using (overseasCharge.GetValidationSuspender())
					{
						overseasCharge.JR_AC = chargeCode.PK;
						overseasCharge.JR_OH_SellAccount = overseasAgent.PK;
						overseasCharge.JR_LocalSellAmt = 100m;
						overseasCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
						overseasCharge.JR_AT_SellGSTRate = GST1.PK;

						overseasCharge.JR_OH_CostAccount = creditor.PK;
						overseasCharge.JR_LocalCostAmt = 80m;
						overseasCharge.JR_APInvoiceNum = "APINV13579";
						overseasCharge.JR_APInvoiceDate = ZDateTime.Today;

						var consolCost = TestObjectCreator.CreateConsolCost(consol1, TestObjectCreator.FRT, creditor);
						consolCost.E6_OSCostAmount = 100M;
						consolCost.E6_InvoiceNum = "789123";
						consolCost.E6_InvoiceDate = ZDateTime.Today;
						consolCost.E6_PaymentDate = ZDateTime.Today;

						Factory.Save();

						var agentValidator = NewPostManagerValidation(jobs, consol1, JobInvoicingPostingOption.Agent) as ConsolPostManagerValidation;
						Assert("Pre-condition: charge is eligible for posting", agentValidator.IsSellEligibleToPost_ForTestOnly(overseasCharge));
						AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Arrival Date'.", agentValidator);

						var costsValidator = NewPostManagerValidation(jobs, consol1, JobInvoicingPostingOption.Costs) as ConsolPostManagerValidation;
						AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Departure Date'.", costsValidator);

						var consolCostsValidator = NewPostManagerValidation(jobs, consol1, JobInvoicingPostingOption.ConsolCosts) as ConsolPostManagerValidation;
						AssertHasNoValidationErrors(consolCostsValidator);

						shipment.JS_E_ARV = ZDateTime.Today;
						shipment.JS_E_DEP = ZDateTime.Today;
						Factory.Save();
						AssertHasNoValidationErrors(agentValidator);
						AssertHasNoValidationErrors(consolCostsValidator);
						AssertHasNoValidationErrors(costsValidator);
					}
				}
			}
		}

		public void TestAgentSellEligibleToPost()
		{
			var consol1 = TestObjectCreator.CreateConsol(consolNum: "C123");
			var consol2 = TestObjectCreator.CreateConsol(consolNum: "C321");
			var shipment = TestObjectCreator.CreateShipment("S123", consol1);
			consol2.Shipments.Add(shipment);

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				var jobs = new[] { shipmentJob };
				var validator1 = NewPostManagerValidation(jobs, consol1, JobInvoicingPostingOption.Agent) as ConsolPostManagerValidation;
				var validator2 = NewPostManagerValidation(jobs, consol2, JobInvoicingPostingOption.Agent) as ConsolPostManagerValidation;

				var blankCharge = shipmentJob.Charges.AddNew();
				AssertEquals(consol1.IsAgentCharge(blankCharge), validator1.IsSellEligibleToPost_ForTestOnly(blankCharge));
				AssertEquals(consol2.IsAgentCharge(blankCharge), validator2.IsSellEligibleToPost_ForTestOnly(blankCharge));

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

				AssertMatches();

				shipment.JS_OH_DeliveryAgent = ZGuid.Empty;
				AssertMatches();

				Assert("Pre-condition: charge is eligible for posting", validator1.IsSellEligibleToPost_ForTestOnly(overseasCharge));
				overseasCharge.JR_LocalSellAmt = 0;
				Assert("0 sell amount makes charge ineligible for posting", !validator1.IsSellEligibleToPost_ForTestOnly(overseasCharge));

				void AssertMatches()
				{
					AssertEquals(consol1.IsAgentCharge(pickupCharge), validator1.IsSellEligibleToPost_ForTestOnly(pickupCharge));
					AssertEquals(consol2.IsAgentCharge(pickupCharge), validator2.IsSellEligibleToPost_ForTestOnly(pickupCharge));

					AssertEquals(consol1.IsAgentCharge(deliveryCharge), validator1.IsSellEligibleToPost_ForTestOnly(deliveryCharge));
					AssertEquals(consol2.IsAgentCharge(deliveryCharge), validator2.IsSellEligibleToPost_ForTestOnly(deliveryCharge));

					AssertEquals(consol1.IsAgentCharge(overseasCharge), validator1.IsSellEligibleToPost_ForTestOnly(overseasCharge));
					AssertEquals(consol2.IsAgentCharge(overseasCharge), validator2.IsSellEligibleToPost_ForTestOnly(overseasCharge));

					AssertEquals(consol1.IsAgentCharge(consol1SendingCharge), validator1.IsSellEligibleToPost_ForTestOnly(consol1SendingCharge));
					AssertEquals(consol2.IsAgentCharge(consol2SendingCharge), validator2.IsSellEligibleToPost_ForTestOnly(consol2SendingCharge));

					AssertEquals(consol1.IsAgentCharge(consol1ReceivingCharge), validator1.IsSellEligibleToPost_ForTestOnly(consol1ReceivingCharge));
					AssertEquals(consol2.IsAgentCharge(consol2ReceivingCharge), validator2.IsSellEligibleToPost_ForTestOnly(consol2ReceivingCharge));

					AssertEquals(consol1.IsAgentCharge(consol2SendingCharge), validator1.IsSellEligibleToPost_ForTestOnly(consol2SendingCharge));
					AssertEquals(consol1.IsAgentCharge(consol2ReceivingCharge), validator1.IsSellEligibleToPost_ForTestOnly(consol2ReceivingCharge));
					AssertEquals(consol2.IsAgentCharge(consol1SendingCharge), validator2.IsSellEligibleToPost_ForTestOnly(consol1SendingCharge));
					AssertEquals(consol2.IsAgentCharge(consol1ReceivingCharge), validator2.IsSellEligibleToPost_ForTestOnly(consol1ReceivingCharge));

					AssertEquals(consol1.IsAgentCharge(consol1ReceivingRelatedPartyCharge), validator1.IsSellEligibleToPost_ForTestOnly(consol1ReceivingRelatedPartyCharge));
					AssertEquals(consol2.IsAgentCharge(consol2ReceivingRelatedPartyCharge), validator2.IsSellEligibleToPost_ForTestOnly(consol2ReceivingRelatedPartyCharge));
					AssertEquals(consol2.IsAgentCharge(consol1ReceivingRelatedPartyCharge), validator2.IsSellEligibleToPost_ForTestOnly(consol1ReceivingRelatedPartyCharge));
					AssertEquals(consol1.IsAgentCharge(consol2ReceivingRelatedPartyCharge), validator1.IsSellEligibleToPost_ForTestOnly(consol2ReceivingRelatedPartyCharge));
				}
			}
		}

		public override void TestGatewaySellEligibleToPost()
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

				var jobs = new[] { shipmentJob };
				var validator1 = NewPostManagerValidation(jobs, consol1, JobInvoicingPostingOption.Gateway) as ConsolPostManagerValidation;
				var validator2 = NewPostManagerValidation(jobs, consol2, JobInvoicingPostingOption.Gateway) as ConsolPostManagerValidation;

				Assert("Pre-condition: Charge code not in GW Charge Code registry item, should post all charge codes when registry has no value", AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.GetAsGuidArray().IsNullOrEmpty());
				Assert("Pre-condition: Charge debtor is null", charge1.JR_OH_SellAccount.IsEmpty);
				Assert("Pre-condition: Charge debtor is null", charge2.JR_OH_SellAccount.IsEmpty);
				AssertGatewaySellEligibleToPost();

				charge1.JR_OH_SellAccount = nonAgent.PK;
				charge2.JR_OH_SellAccount = nonAgent.PK;
				AssertGatewaySellEligibleToPost();

				charge1.JR_OH_SellAccount = sendingAgent1.PK;
				charge2.JR_OH_SellAccount = sendingAgent2.PK;
				AssertGatewaySellEligibleToPost();
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertGatewaySellEligibleToPost();
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertGatewaySellEligibleToPost();

				charge1.JR_OH_SellAccount = receivingAgent1.PK;
				charge2.JR_OH_SellAccount = receivingAgent2.PK;
				AssertGatewaySellEligibleToPost();
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertGatewaySellEligibleToPost();
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertGatewaySellEligibleToPost();

				AssertNull("Pre-condition: Charge code is null", charge1.ChargeCode);
				AssertNull("Pre-condition: Charge code is null", charge2.ChargeCode);
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Factory.NewWithValidTestData<AccChargeCode>().PK.ToString());
				AssertGatewaySellEligibleToPost();

				var chargeCode = TestObjectCreator.CreateChargeCode("COD");
				charge1.JR_AC = chargeCode.PK;
				charge1.JR_LocalSellAmt = 100;
				charge2.JR_AC = chargeCode.PK;
				charge2.JR_LocalSellAmt = 100;
				AssertGatewaySellEligibleToPost();

				var globalChargeCode = TestObjectCreator.CreateGlobalChargeCode(chargeCode.AC_Code);
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalChargeCode.PK.ToString());
				AssertGatewaySellEligibleToPost();

				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Factory.NewWithValidTestData<AccChargeCode>().PK.ToString());
				AssertGatewaySellEligibleToPost();
				AccountingMasterFilesRegistry.Instance.GatewayBillingChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToString());
				AssertGatewaySellEligibleToPost();

				Assert("Pre-condition: charge is eligible for posting", validator1.IsSellEligibleToPost_ForTestOnly(charge1));
				charge1.JR_LocalSellAmt = 0;
				Assert("0 sell amount makes charge ineligible for posting", !validator1.IsSellEligibleToPost_ForTestOnly(charge1));

				void AssertGatewaySellEligibleToPost()
				{
					AssertEquals(consol1.IsGatewayCharge(charge1), validator1.IsSellEligibleToPost_ForTestOnly(charge1));
					AssertEquals(consol2.IsGatewayCharge(charge2), validator2.IsSellEligibleToPost_ForTestOnly(charge2));
					AssertEquals(consol2.IsGatewayCharge(charge1), validator2.IsSellEligibleToPost_ForTestOnly(charge1));
					AssertEquals(consol1.IsGatewayCharge(charge2), validator1.IsSellEligibleToPost_ForTestOnly(charge2));
				}
			}
		}

		protected override PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return new ConsolPostManagerValidation(jobs, null, postingOption, originalJobs);
		}

		protected override PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, IJobCostingPlugIn consol, JobInvoicingPostingOption postingOption)
		{
			return new ConsolPostManagerValidation(jobs, consol, postingOption, jobs);
		}

		protected override void AssertNoChargesValidation(PostManagerValidation validator)
		{
			if (validator.PostingOption_ForTestOnly == JobInvoicingPostingOption.Costs)
			{
				base.AssertNoChargesValidation(validator);
			}
			else
			{
				// Should be OK if no charges - Can post consol charges when there are no jobcharges to post
				AssertHasNoValidationErrors(validator);
			}
		}

		protected override string expectedErrorWhenRunJobChargeSupplyTypeValidation_ForApportionmentCharges => "The Cost Supply Type value of all apportioned charges must be the same. Please re-enter the Consol Cost's Cost Supply Type to update the apportioned charges.";

		#region Tax Branch

		public void TestRunConsolCostTaxBranchValidation()
		{
			var expectedError = "Error - Tax Branch: The Cost Tax Branch value of all apportioned charges must be the same. Please re-enter the Consol Cost's Tax Branch to update the apportioned charges.";
			var consol = TestObjectCreator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			using (var job1 = TestObjectCreator.CreateJob(consol.Shipments[0]))
			using (var job2 = TestObjectCreator.CreateJob(consol.Shipments[1]))
			{
				job1.JH_GE = TestObjectCreator.FISDepartment.PK;
				job2.JH_GE = TestObjectCreator.FISDepartment.PK;
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, 500M, TestObjectCreator.TestOrganisation);
				consolCost.E6_InvoiceNum = "TEST001";
				consolCost.E6_InvoiceDate = ZDateTime.Today;
				Factory.Save();

				var jobs = new[] { job1, job2 };
				var validator = NewPostManagerValidation(jobs, consol, JobInvoicingPostingOption.All);

				AssertRunConsolCostTaxBranchValidation(true);
				AssertRunConsolCostTaxBranchValidation(false);

				void AssertRunConsolCostTaxBranchValidation(bool enableTaxBranchReporting)
				{
					using (TestObjectCreator.SetUpTaxBranchRegistry(enableTaxBranchReporting))
					{
						TestObjectCreator.ResetSecurityCore();

						consolCost.E6_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
						AssertNullOrEmpty(validator.RunConsolCostTaxBranchValidation_ForTestOnly());

						using (jobs[0].Charges[0].GetValidationSuspender())
						{
							jobs[0].Charges[0].JR_GB_CostTaxBranch = GlbBranch.CurrentBranch.PK;
						}
						AssertEquals(enableTaxBranchReporting ? expectedError : string.Empty, validator.RunConsolCostTaxBranchValidation_ForTestOnly());

						jobs[0].Charges[0].JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
						AssertNullOrEmpty(validator.RunConsolCostTaxBranchValidation_ForTestOnly());
					}
				}
			}
		}

		#endregion
	}
}
