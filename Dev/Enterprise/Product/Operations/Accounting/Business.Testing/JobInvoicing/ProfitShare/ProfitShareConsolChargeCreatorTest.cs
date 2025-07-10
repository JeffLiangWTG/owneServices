using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ProfitShareConsolChargeCreatorTest : TestCaseWithFactory
	{
		#region AR

		public void TestCreateCharges_AR()
		{
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			AssertCreateChargesCorrect_AR(deliveryAgent, GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		public void TestCreateCharges_AR_ClearesDefaultedSellInvoiceCurrency()
		{
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.CompanyData.InvoiceRollupOrGroups[0].PG_RX_NKInvoicePostingCurrency = TestObjectCreator.EUR.Code;

			AssertCreateChargesCorrect_AR(deliveryAgent, GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		void AssertCreateChargesCorrect_AR(OrgHeader overseasAgent, RefCurrency postingCurrency, ZDecimal postingExchangeRate)
		{
			#region Setup

			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AutoCompleteJobOnProfitShareCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
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
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 300m;
			job1Charge1.JR_AgentDeclaredCostAmt = 140m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob1PSChargeLocalAmount = 105.6m;
			ZDecimal expectedJob2PSChargeLocalAmount = 112.2m;

			ZDecimal expectedJob1PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob1PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedJob2PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob2PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);

			Factory.Save();

			#endregion

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			AssertEquals("Shipment without job gets excluded", 2, profitShares[0].ProfitShareShipmentDetails.Count);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, postingCurrency, postingExchangeRate, new ChargePoster(Factory), new ApportionmentListing(Factory, consol).CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);
			AssertEquals("Precondition: Shipment 2 job has no PS charge", 1, job2.Charges.Count);

			try
			{
				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				AssertEquals("Shipment 1 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job1.Charges[1].JR_AC);
				AssertEquals("Shipment 1 profit share is NOT apportioned as it is AR", false, job1.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 1 profit share local amount is correct", expectedJob1PSChargeLocalAmount * -1, job1.Charges[1].JR_LocalSellAmt);
				AssertEquals("Shipment 1 profit share LocalSellAmount is correct", expectedJob1PSChargeLocalAmount * -1, ((IReceivablesPostingCharge)job1.Charges[1]).LocalSellAmount);
				AssertEquals("Shipment 1 profit share is correct account", overseasAgent.APGrouping, job1.Charges[1].SellAccount);
				AssertEquals("Shipment 2 profit share os amount is correct", expectedJob1PSChargeOSAmount * -1, job1.Charges[1].JR_OSSellAmt);
				AssertEquals("Shipment 2 profit share OSSellAmount is correct", expectedJob1PSChargeOSAmount * -1, ((IReceivablesPostingCharge)job1.Charges[1]).OSSellAmount);
				AssertEquals("Shipment 1 job should be completed", JobHeaderStatus.Complete.Code, job1.JH_Status);

				AssertEquals("Shipment 2 job has PS charge", 2, job2.Charges.Count);
				AssertEquals("Shipment 2 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job2.Charges[1].JR_AC);
				AssertEquals("Shipment 2 profit share is NOT apportioned for AR", false, job2.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 2 profit share is correct", expectedJob2PSChargeLocalAmount * -1, job2.Charges[1].JR_LocalSellAmt);
				AssertEquals("Shipment 1 profit share local amount is correct", expectedJob2PSChargeLocalAmount * -1, ((IReceivablesPostingCharge)job2.Charges[1]).LocalSellAmount);
				AssertEquals("Shipment 2 profit share os amount is correct", expectedJob2PSChargeOSAmount * -1, job2.Charges[1].JR_OSSellAmt);
				AssertEquals("Shipment 2 profit share os amount is correct", expectedJob2PSChargeOSAmount * -1, ((IReceivablesPostingCharge)job2.Charges[1]).OSSellAmount);
				AssertEquals("Shipment 2 profit share is correct account", overseasAgent.APGrouping, job2.Charges[1].SellAccount);
				AssertEquals("Shipment 2 job should be completed", JobHeaderStatus.Complete.Code, job2.JH_Status);

				//Remove PS Charge from job, so that job has one charge to be included in profit share and we can reuse job for the next set of assertions
				job1.Charges.Where(x => x.PK != job1Charge1.PK).ToArray().ForEach(x => x.Delete());
				job2.Charges.Where(x => x.PK != job2Charge1.PK).ToArray().ForEach(x => x.Delete());

				ZGuid receivingAgentChargeCode = SetupProfitShareChargeCodesPerParty();

				AssertEquals("Shipment 1 job has no PS charge", 1, job1.Charges.Count);
				AssertEquals("Shipment 2 job has no PS charge", 1, job2.Charges.Count);

				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				Assert("Shipment 1 profit share is correct charge code", job1.Charges.Cast<JobCharge>().Any(x => x.JR_AC == receivingAgentChargeCode));

				AssertEquals("Shipment 2 job has PS charge", 2, job2.Charges.Count);
				Assert("Shipment 2 profit share is correct charge code", job2.Charges.Cast<JobCharge>().Any(x => x.JR_AC == receivingAgentChargeCode));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
			}
		}

		/// <summary>
		/// This test just covers edge case. A real business case does not make sense for a gateway consol.
		/// </summary>
		public void TestCreateCharges_AR_GatewayConsol_ShouldNotPickGatewayAgentChargeCodeFromRegistry()
		{
			#region Setup

			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AutoCompleteJobOnProfitShareCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			const string origin = "AUSYD";
			const string destination = "USLAX";

			var chargeCodeToOverrideGatewayAgent = TestObjectCreator.CreateChargeCode("GWA");
			var chargeCodeToOverrideSendingAgent = TestObjectCreator.CreateChargeCode("SEN");
			var chargeCodeToOverrideReceivingAgent = TestObjectCreator.CreateChargeCode("RCV");

			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(
				origin, destination,
				sendingGatewayCompany: GlbCompany.CurrentCompany,
				receivingGatewayCompany: GlbCompany.CurrentCompany);
			gatewayConsol.JK_RL_NKLoadPort = origin;
			gatewayConsol.JK_RL_NKDischargePort = destination;

			var sendingAgent = gatewayConsol.SendingForwarder;
			var receivingAgent = gatewayConsol.ReceivingForwarder;

			var shipment = gatewayConsol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;

			var agentRelationship = TestObjectCreator.CreateAgentRelationship(sendingAgent, receivingAgent);
			TestObjectCreator.CreateProfitShare(agentRelationship, 50, 50, origin, destination,
				transportMode: "ALL", jobType: JobTypesList.Codes.SHP, gatewayAgentType: "");

			var testChargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			testChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var lookups = new OrgProfitSharePartyLookups(null);
			var profitShareChargeCodesPerPartyCollection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			var profitShareChargeCodeGatewayAgent = profitShareChargeCodesPerPartyCollection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.GatewayAgent)];
			profitShareChargeCodeGatewayAgent.UseDefaultProfitShareChargeCode = false;
			profitShareChargeCodeGatewayAgent.ChargeCode = chargeCodeToOverrideGatewayAgent.PK;
			var profitShareChargeCodeSendingAgent = profitShareChargeCodesPerPartyCollection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)];
			profitShareChargeCodeSendingAgent.UseDefaultProfitShareChargeCode = false;
			profitShareChargeCodeSendingAgent.ChargeCode = chargeCodeToOverrideSendingAgent.PK;
			var profitShareChargeCodeReceivingAgent = profitShareChargeCodesPerPartyCollection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)];
			profitShareChargeCodeReceivingAgent.UseDefaultProfitShareChargeCode = false;
			profitShareChargeCodeReceivingAgent.ChargeCode = chargeCodeToOverrideReceivingAgent.PK;

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				profitShareChargeCodesPerPartyCollection);

			var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex();
			job.PlugInData = shipment;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jobCharge = job.Charges.AddNew();
			jobCharge.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge.JR_AgentDeclaredSellAmt = 300m;
			jobCharge.JR_AgentDeclaredCostAmt = 140m;
			jobCharge.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			#endregion

			var profitShares = new ProfitShareCalculator(Factory, gatewayConsol.GetShipmentsList()).CreateProfitShares();
			var postingDetails = new AgentChargePostingDetails(gatewayConsol, profitShares,
				GlbCompany.CurrentCompany.LocalCurrency, 1, new ChargePoster(Factory),
				new ApportionmentListing(Factory, gatewayConsol).CostsCollection);

			AssertEquals("Precondition: Shipment job should contain only manually created charge", 1, job.Charges.Count);

			var chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);
			chargeCreator.CreateCharges();
			AssertEquals("Shipment job should have new PS charges for SEN and RCV parties", 3, job.Charges.Count);

			var expectedChargeCodes = new[] { jobCharge.JR_AC, chargeCodeToOverrideSendingAgent.PK, chargeCodeToOverrideReceivingAgent.PK };
			var message = "None of the charges should have charge code from GatewayAgent party type";
			AssertContainsExactElementsInAnyOrder(message, expectedChargeCodes, job.Charges.Select(x => x.JR_AC));
		}

		#endregion

		#region AP

		public void TestCreateCharges()
		{
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			AssertCreateChargesCorrect(deliveryAgent, GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		public void TestCreateCharges_NettingOrgOnOverseasAgent()
		{
			OrgHeader headOffice = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.SetRelatedParty(headOffice, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);

			AssertCreateChargesCorrect(deliveryAgent, GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		public void TestApportionCharges_PostingAgentChargesInFC()
		{
			OrgHeader headOffice = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.SetRelatedParty(headOffice, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);
			AssertCreateChargesCorrect(deliveryAgent, GlbCompany.CurrentCompany.LocalCurrency, 1m);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestAgentInvoiceFallBackWhenCreditingProfitShare()
		{
			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = sendingAgent.PK;

			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice agentARInvoice = Factory.New<ARInvoice>();
			agentARInvoice.AH_OH = overseasAgent.PK;
			agentARInvoice.AH_ExchangeRate = 0.783104m;
			agentARInvoice.AH_RX_NKTransactionCurrency = "USD";

			ChargePoster poster = new ChargePoster(Factory);
			poster.PostedInvoices.Add(agentARInvoice);

			#region Setup

			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(sendingAgent);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = sendingAgent.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
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
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 300m;
			job1Charge1.JR_AgentDeclaredCostAmt = 140m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = Job.CreateWithMutex(Factory, shipment2);
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob1PSChargeLocalAmount = 105.6m;
			ZDecimal expectedJob2PSChargeLocalAmount = 112.2m;

			ZDecimal expectedJob1PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob1PSChargeLocalAmount, 0.783104m, "USD");
			ZDecimal expectedJob2PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob2PSChargeLocalAmount, 0.783104m, "USD");

			Factory.Save();

			#endregion

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, creator.USD, 0.783104m, poster, apps.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);
			AssertEquals("Precondition: Shipment 2 job has no PS charge", 1, job2.Charges.Count);

			try
			{
				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				AssertEquals("Shipment 1 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job1.Charges[1].JR_AC);
				AssertEquals("Shipment 1 profit share charge should be correct currency", creator.USD.RX_Code, job1.Charges[1].JR_RX_NKCostCurrency);
				AssertEquals("Shipment 1 profit share charge should be correct exrate", 0.783104m, job1.Charges[1].JR_OSCostExRate);
				AssertEquals("Shipment 1 profit share is apportioned", true, job1.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 1 profit share local amount is correct", expectedJob1PSChargeLocalAmount, job1.Charges[1].JR_LocalCostAmt);

				Assert("System created Profit Share should never be marked as 'Include In Profit Share'", !job1.Charges[1].JR_IsIncludedInProfitShare);

				AssertEquals("Shipment 1 profit share is correct account", overseasAgent.APGrouping.PK, job1.Charges[1].CostAccount.PK);

				AssertEquals("Shipment 2 job has PS charge", 2, job2.Charges.Count);
				AssertEquals("Shipment 2 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job2.Charges[1].JR_AC);
				AssertEquals("Shipment 1 profit share charge should be correct currency", creator.USD.RX_Code, job2.Charges[1].JR_RX_NKCostCurrency);
				AssertEquals("Shipment 1 profit share charge should be correct exrate", 0.783104m, job2.Charges[1].JR_OSCostExRate);
				AssertEquals("Shipment 2 profit share is apportioned", true, job2.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 2 profit share is correct", expectedJob2PSChargeLocalAmount, job2.Charges[1].JR_LocalCostAmt);

				AssertEquals("Shipment 2 profit share os amount is correct", expectedJob2PSChargeOSAmount, job2.Charges[1].JR_OSCostAmt);
				Assert("System created Profit Share should never be marked as 'Include In Profit Share'", !job2.Charges[1].JR_IsIncludedInProfitShare);

				AssertEquals("Shipment 2 profit share is correct account", overseasAgent.APGrouping, job2.Charges[1].CostAccount);

				//Remove PS Consol cost and PS charges, so that job has one charge to be included in profit share and we can reuse job for the next set of assertions
				var profitShareCost = consol.GetApportionments().CostsCollection.Cast<JobConsolCost>().First(x => x.E6_AC_ChargeCode == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
				profitShareCost.DeleteCostAndCharges();

				ZGuid receivingAgentChargeCode = SetupProfitShareChargeCodesPerParty();

				AssertEquals("Shipment 1 job has no PS charge", 1, job1.Charges.Count);
				AssertEquals("Shipment 2 job has no PS charge", 1, job2.Charges.Count);

				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				AssertEquals("Shipment 2 job has PS charge", 2, job2.Charges.Count);
				Assert("Shipment 1 profit share is correct charge code", job1.Charges.Cast<JobCharge>().Any(x => x.JR_AC == receivingAgentChargeCode));
				Assert("Shipment 2 profit share is correct charge code", job2.Charges.Cast<JobCharge>().Any(x => x.JR_AC == receivingAgentChargeCode));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
			}
		}

		void AssertCreateChargesCorrect(OrgHeader overseasAgent, RefCurrency postingCurrency, ZDecimal postingExchangeRate)
		{
			#region Setup

			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AutoCompleteJobOnProfitShareCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S0003";
			shipment3.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment3.JS_RL_NKOrigin = "AUSYD";
			shipment3.JS_RL_NKDestination = "USLAX";
			shipment3.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S0001";
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0002";
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment4 = consol.Shipments.AddNew();
			shipment4.JS_UniqueConsignRef = "S0004";
			shipment4.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment4.JS_RL_NKOrigin = "AUSYD";
			shipment4.JS_RL_NKDestination = "USLAX";
			shipment4.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
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

			Job job3 = new Job.Loader(Factory, shipment3).TryLoadOrCreateWithMutex();
			job3.PlugInData = shipment3;
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job3Charge1 = job3.Charges.AddNew();
			job3Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job3Charge1.JR_AgentDeclaredSellAmt = 0m;
			job3Charge1.JR_AgentDeclaredCostAmt = 0m;

			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 300m;
			job1Charge1.JR_AgentDeclaredCostAmt = 140m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob1PSChargeLocalAmount = 105.6m;
			ZDecimal expectedJob2PSChargeLocalAmount = 112.2m;

			ZDecimal expectedJob1PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob1PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedJob2PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob2PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);

			Factory.Save();

			#endregion

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			AssertEquals("Shipment without job gets excluded", 2, profitShares[0].ProfitShareShipmentDetails.Count);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, postingCurrency, postingExchangeRate, new ChargePoster(Factory), apps.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);
			AssertEquals("Precondition: Shipment 2 job has no PS charge", 1, job2.Charges.Count);

			try
			{
				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				AssertEquals("Shipment 1 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job1.Charges[1].JR_AC);
				AssertEquals("Shipment 1 profit share is apportioned", true, job1.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 1 profit share local amount is correct", expectedJob1PSChargeLocalAmount, job1.Charges[1].JR_LocalCostAmt);

				AssertEquals("Shipment 1 profit share is correct account", overseasAgent.APGrouping, job1.Charges[1].CostAccount);
				AssertEquals("Shipment 1 job should be completed", JobHeaderStatus.Complete.Code, job1.JH_Status);
				if (expectedJob1PSChargeLocalAmount != 0m)
				{
					Assert(job1.JH_IsProfitSharePosted);
				}
				else
				{
					Assert(!job1.JH_IsProfitSharePosted);
				}

				AssertEquals("Shipment 2 job has PS charge", 2, job2.Charges.Count);
				AssertEquals("Shipment 2 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job2.Charges[1].JR_AC);
				AssertEquals("Shipment 2 profit share is apportioned", true, job2.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 2 profit share is correct", expectedJob2PSChargeLocalAmount, job2.Charges[1].JR_LocalCostAmt);
				AssertEquals("Shipment 2 profit share os amount is correct", expectedJob2PSChargeOSAmount, job2.Charges[1].JR_OSCostAmt);

				AssertEquals("Shipment 2 profit share is correct account", overseasAgent.APGrouping, job2.Charges[1].CostAccount);
				AssertEquals("Shipment 2 job should be completed", JobHeaderStatus.Complete.Code, job2.JH_Status);
				if (expectedJob2PSChargeLocalAmount != 0m)
				{
					Assert(job2.JH_IsProfitSharePosted);
				}
				else
				{
					Assert(!job2.JH_IsProfitSharePosted);
				}

				AssertEquals("Shipment 3 job has PS charge", 1, job3.Charges.Count);
				AssertEquals(job3Charge1, job3.Charges[0]);
				Assert(!job3.JH_IsProfitSharePosted);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
				apps.ReleaseMutexes();
			}
		}

		public void TestProfitShare_PostedFlagSetOnlyWhenProfitSharePostedOnShipment()
		{
			#region Setup

			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
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

			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 300m;
			job1Charge1.JR_AgentDeclaredCostAmt = 140m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 190m;
			job2Charge1.JR_IsIncludedInProfitShare = true; // NOTE - no profit share to post, 
														   //so profit share should be allowed to post later

			ZDecimal expectedJob1PSChargeLocalAmount = 105.6m;
			ZDecimal expectedJob2PSChargeLocalAmount = 0m;

			Factory.Save();

			#endregion
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, postingCurrency, postingExchangeRate, new ChargePoster(Factory), apps.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);
			AssertEquals("Precondition: Shipment 2 job has no PS charge", 1, job2.Charges.Count);

			try
			{
				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				AssertEquals("Shipment 1 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job1.Charges[1].JR_AC);
				AssertEquals("Shipment 1 profit share is apportioned", true, job1.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 1 profit share local amount is correct", expectedJob1PSChargeLocalAmount, job1.Charges[1].JR_LocalCostAmt);

				AssertEquals("Shipment 1 profit share is correct account", overseasAgent.APGrouping, job1.Charges[1].CostAccount);
				Assert(job1.JH_IsProfitSharePosted);

				AssertEquals("Shipment 2 job still has only 1 charge", 1, job2.Charges.Count);
				Assert(!job2.JH_IsProfitSharePosted);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
			}
		}

		public void TestProfitShare_CalculatedCorrectlyForLossesWhenPostingAgentCharges()
		{
			#region Setup

			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
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
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 100m;
			job1Charge1.JR_AgentDeclaredCostAmt = 400m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob1PSChargeLocalAmount = -198.0m;
			ZDecimal expectedJob2PSChargeLocalAmount = 112.2m;
			ZDecimal expectedConsolPSCostLocalAmount = -85.8m;

			ZDecimal expectedJob1PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob1PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedJob2PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob2PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedConsolPSCostOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedConsolPSCostLocalAmount, postingExchangeRate, postingCurrency.RX_Code);

			Factory.Save();

			#endregion

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, postingCurrency, postingExchangeRate, new ChargePoster(Factory), apps.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);

			try
			{
				chargeCreator.CreateCharges();

				AssertEquals("Shipment 1 job has PS charge", 2, job1.Charges.Count);
				AssertEquals("Shipment 1 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job1.Charges[1].JR_AC);
				AssertEquals("Shipment 1 profit share is apportioned", true, job1.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 1 profit share local amount is correct", expectedJob1PSChargeLocalAmount, job1.Charges[1].JR_LocalCostAmt);
				AssertEquals("Shipment 1 profit share OS amount is correct", expectedJob1PSChargeOSAmount, job1.Charges[1].JR_OSCostAmt);
				AssertEquals("Shipment 1 profit share is correct account", overseasAgent.APGrouping, job1.Charges[1].CostAccount);

				AssertEquals("Shipment 2 job has PS charge", 2, job2.Charges.Count);
				AssertEquals("Shipment 2 profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, job2.Charges[1].JR_AC);
				AssertEquals("Shipment 2 profit share is apportioned", true, job2.Charges[1].JR_IsApportioned);
				AssertEquals("Shipment 2 profit share local amount is correct", expectedJob2PSChargeLocalAmount, job2.Charges[1].JR_LocalCostAmt);
				AssertEquals("Shipment 2 profit share OS amount is correct", expectedJob2PSChargeOSAmount, job2.Charges[1].JR_OSCostAmt);
				AssertEquals("Shipment 2 profit share is correct account", overseasAgent.APGrouping, job2.Charges[1].CostAccount);

				AssertEquals("Consol has PS charge", 1, apps.CostsCollection.Count);
				AssertEquals("Consol profit share is apportioned", 0m, apps.CostsCollection[0].UnApportionedAmount);
				AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps.CostsCollection[0].E6_AC_ChargeCode);
				AssertEquals("Consol profit share local amount is correct", expectedConsolPSCostLocalAmount, apps.CostsCollection[0].E6_LocalCostAmount);
				AssertEquals("Consol profit share OS amount is correct", expectedConsolPSCostOSAmount, apps.CostsCollection[0].E6_OSCostAmount);
				AssertEquals("Consol profit share is correct account", overseasAgent.APGrouping, job1.Charges[1].CostAccount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
			}
		}

		public void TestProfitShare_ApportionPerContainer()
		{
			#region Setup

			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader overseasAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			#region Part One

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer;
			rcvParty.PS_PartyType = "RCV";
			rcvParty.PS_PartyProfitSharePercent = 66m;

			Factory.Save();

			// Jobs
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 100m;
			job1Charge1.JR_AgentDeclaredCostAmt = 400m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob1PSChargeLocalAmount = -198.0m;
			ZDecimal expectedJob2PSChargeLocalAmount = 112.2m;
			ZDecimal expectedConsolPSCostLocalAmount = -85.8m;

			ZDecimal expectedJob1PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob1PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedJob2PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob2PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedConsolPSCostOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedConsolPSCostLocalAmount, postingExchangeRate, postingCurrency.RX_Code);

			Factory.Save();
			#endregion

			#region Part Two
			// Setup consol & shipment
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment3 = consol2.Shipments.AddNew();
			shipment3.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment3.JS_RL_NKOrigin = "AUBNE";
			shipment3.JS_RL_NKDestination = "NZAKL";
			shipment3.JS_OH_DeliveryAgent = overseasAgent2.PK;

			CommonShipment shipment4 = consol2.Shipments.AddNew();
			shipment4.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment4.JS_RL_NKOrigin = "AUBNE";
			shipment4.JS_RL_NKDestination = "NZAKL";
			shipment4.JS_OH_DeliveryAgent = overseasAgent2.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship2 = Factory.New<OrgAgentRelationship>();
			agentRelationship2.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship2.O3_OH_ReceivingAgent = overseasAgent2.PK;
			OrgProfitShareDetails profitShare2 = agentRelationship2.ProfitShareDetails.AddNew();
			profitShare2.O4_FreightMode = "AIR";
			profitShare2.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare2.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare2.O4_SendingPortOrCountry = "AUBNE";
			profitShare2.O4_ReceivingPortOrCountry = "NZAKL";

			OrgProfitShareParty rcvParty2 = profitShare2.PartyDetails.AddNew();
			rcvParty2.PS_PartyType = "RCV";
			rcvParty2.PS_PartyProfitSharePercent = 66m;

			OrgProfitShareDetails profitShare3 = agentRelationship2.ProfitShareDetails.AddNew();
			profitShare3.O4_FreightMode = "AIR";
			profitShare3.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare3.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare3.O4_SendingPortOrCountry = "AUBNE";
			profitShare3.O4_ReceivingPortOrCountry = "CAVAN";

			OrgProfitShareParty rcvParty3 = profitShare3.PartyDetails.AddNew();
			rcvParty3.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer;
			rcvParty3.PS_PartyType = "RCV";
			rcvParty3.PS_PartyProfitSharePercent = 66m;

			Factory.Save();

			// Jobs
			Job job3 = new Job.Loader(Factory, shipment3).TryLoadOrCreateWithMutex();
			job3.PlugInData = shipment3;
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job3Charge1 = job3.Charges.AddNew();
			job3Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job3Charge1.JR_AgentDeclaredSellAmt = 100m;
			job3Charge1.JR_AgentDeclaredCostAmt = 400m;
			job3Charge1.JR_IsIncludedInProfitShare = true;

			Job job4 = new Job.Loader(Factory, shipment4).TryLoadOrCreateWithMutex();
			job4.PlugInData = shipment4;
			job4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job4Charge1 = job4.Charges.AddNew();
			job4Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job4Charge1.JR_AgentDeclaredSellAmt = 190m;
			job4Charge1.JR_AgentDeclaredCostAmt = 20m;
			job4Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob3PSChargeLocalAmount = -198.0m;
			ZDecimal expectedJob4PSChargeLocalAmount = 112.2m;
			ZDecimal expectedConsol2PSCostLocalAmount = -85.8m;

			ZDecimal expectedJob3PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob3PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedJob4PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob4PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedConsol2PSCostOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedConsol2PSCostLocalAmount, postingExchangeRate, postingCurrency.RX_Code);

			Factory.Save();
			#endregion

			#endregion

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, postingCurrency, postingExchangeRate, new ChargePoster(Factory), apps.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);

			ApportionmentListing apps2 = new ApportionmentListing(Factory, consol2);

			ProfitShareDetailCollection profitShares2 = new ProfitShareCalculator(Factory, consol2.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares2.Count);
			AgentChargePostingDetails postingDetails2 = new AgentChargePostingDetails(consol2, profitShares2, postingCurrency, postingExchangeRate, new ChargePoster(Factory), apps2.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator2 = new ProfitShareConsolChargeCreator(postingDetails2, Factory);

			AssertEquals("Precondition: Shipment 3 job has no PS charge", 1, job3.Charges.Count);

			try
			{
				chargeCreator.CreateCharges();
				chargeCreator2.CreateCharges();

				AssertEquals("Consol has PS charge", 1, apps.CostsCollection.Count);
				AssertEquals("Consol profit share is apportioned", 0m, apps.CostsCollection[0].UnApportionedAmount);
				AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps.CostsCollection[0].E6_AC_ChargeCode);
				AssertEquals("Consol profit share local amount is correct", expectedConsolPSCostLocalAmount, apps.CostsCollection[0].E6_LocalCostAmount);
				AssertEquals("Consol profit share OS amount is correct", expectedConsolPSCostOSAmount, apps.CostsCollection[0].E6_OSCostAmount);
				AssertEquals("Consol profit share is correct account", overseasAgent.APGrouping, job1.Charges[1].CostAccount);
				AssertEquals("Consol profit share apportion method is correct", ZArchitecture.Core.AllocationMethod.ContainerCount, apps.CostsCollection[0].E6_ApportionmentMethod);

				AssertEquals("Consol has PS charge", 1, apps2.CostsCollection.Count);
				AssertEquals("Consol profit share is apportioned", 0m, apps2.CostsCollection[0].UnApportionedAmount);
				AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps2.CostsCollection[0].E6_AC_ChargeCode);
				AssertEquals("Consol profit share local amount is correct", expectedConsol2PSCostLocalAmount, apps2.CostsCollection[0].E6_LocalCostAmount);
				AssertEquals("Consol profit share OS amount is correct", expectedConsol2PSCostOSAmount, apps2.CostsCollection[0].E6_OSCostAmount);
				AssertEquals("Consol profit share is correct account", overseasAgent2.APGrouping, job3.Charges[1].CostAccount);
				AssertNotEquals("Consol profit share apportion method is correct", ZArchitecture.Core.AllocationMethod.ContainerCount, apps2.CostsCollection[0].E6_ApportionmentMethod);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
			}
		}

		public void TestProfitShare_ApportionPerContainer_WithProfitSharePartyDetailsNotSet()
		{
			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";

			Factory.Save();

			// Jobs
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 100m;
			job1Charge1.JR_AgentDeclaredCostAmt = 400m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			ZDecimal expectedJob1PSChargeLocalAmount = -198.0m;
			ZDecimal expectedJob2PSChargeLocalAmount = 112.2m;
			ZDecimal expectedConsolPSCostLocalAmount = -85.8m;

			ZDecimal expectedJob1PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob1PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedJob2PSChargeOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedJob2PSChargeLocalAmount, postingExchangeRate, postingCurrency.RX_Code);
			ZDecimal expectedConsolPSCostOSAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(expectedConsolPSCostLocalAmount, postingExchangeRate, postingCurrency.RX_Code);

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				ApportionmentListing apps = new ApportionmentListing(Factory, consol);

				var profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
				AssertEquals("There should not be any profit share as there is no parties", 0, profitShares.Count);
			}
		}

		public void TestProfitShare_ChargesNotCreated()
		{
			#region Setup

			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			bool oldAutoPostMasterFreight = AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
			AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipments
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
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

			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge1.JR_AgentDeclaredSellAmt = 300m;
			job1Charge1.JR_AgentDeclaredCostAmt = 140m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			#endregion
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ProfitShareDetailCollection profitShares = new ProfitShareCalculator(Factory, consol.GetShipmentsList()).CreateProfitShares();
			AssertEquals("Precondition: 1 profit share created", 1, profitShares.Count);
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, postingCurrency, postingExchangeRate, new ChargePoster(Factory), apps.CostsCollection);
			ProfitShareConsolChargeCreator chargeCreator = new ProfitShareConsolChargeCreator(postingDetails, Factory);

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);

			Guid oldRegistryValue = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

				ErrorReporter.Clear();
				chargeCreator.CreateCharges();
				ErrorReporter.Clear();

				AssertEquals("Shipment 1 job has not PS charges", 1, job1.Charges.Count);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldRegistryValue);
				AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldAutoPostMasterFreight);
			}
		}

		public void TestProfitShare_CreateChargesPerCurrency()
		{
			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 60m;

			OrgProfitShareParty sndParty = profitShare1.PartyDetails.AddNew();
			sndParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sndParty.PS_PartyProfitSharePercent = 40m;

			Factory.Save();

			// Jobs
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job1, TestObjectCreator.USD, 0.5M);
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = TestObjectCreator.CC1.PK;
			job1Charge1.JR_RX_NKCostCurrency = "USD";
			job1Charge1.JR_RX_NKSellCurrency = "USD";
			job1Charge1.JR_RX_NKSellCurrency = "USD";
			job1Charge1.JR_OSCostAmt = 100;
			job1Charge1.JR_AgentDeclaredSellAmt = 400m;
			job1Charge1.JR_AgentDeclaredCostAmt = 100m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.USD, 0.7M);
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = TestObjectCreator.CC2.PK;
			job2Charge1.JR_RX_NKCostCurrency = "USD";
			job2Charge1.JR_RX_NKSellCurrency = "USD";
			job2Charge1.JR_OSCostAmt = 210;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			AssertEquals("Precondition: ", 2, jobs.Count);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			postManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(postManager_ProfitShareConfirmation);
			postManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			AssertEquals("Consol has PS charge", 1, apps.CostsCollection.Count);
			AssertEquals("Consol profit share is apportioned", 0m, apps.CostsCollection[0].UnApportionedAmount);
			AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps.CostsCollection[0].E6_AC_ChargeCode);
			AssertEquals("Consol profit share OS amount is correct", 282M, apps.CostsCollection[0].E6_OSCostAmount);
			AssertEquals("Consol profit share local amount is correct", 505.72M, apps.CostsCollection[0].E6_LocalCostAmount);
		}

		public void TestProfitShare_CreateChargesPerCurrency_SellAndCostDifferentCurrency()
		{
			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 60m;

			OrgProfitShareParty sndParty = profitShare1.PartyDetails.AddNew();
			sndParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sndParty.PS_PartyProfitSharePercent = 40m;

			Factory.Save();

			// Jobs
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job1, TestObjectCreator.USD, 0.5M);
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = TestObjectCreator.CC1.PK;
			job1Charge1.JR_RX_NKCostCurrency = "AUD";
			job1Charge1.JR_OSCostAmt = 50m;
			job1Charge1.JR_RX_NKSellCurrency = "USD";
			job1Charge1.JR_OSSellAmt = 80m;
			job1Charge1.JR_AgentDeclaredCostAmt = 50m;
			job1Charge1.JR_AgentDeclaredCostAmtLocal = 50m;
			job1Charge1.JR_AgentDeclaredSellAmt = 80m;
			job1Charge1.JR_AgentDeclaredSellAmtLocal = 160m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.USD, 0.7M);
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = TestObjectCreator.CC2.PK;
			job2Charge1.JR_RX_NKCostCurrency = "USD";
			job2Charge1.JR_OSCostAmt = 210m;
			job2Charge1.JR_RX_NKSellCurrency = "AUD";
			job2Charge1.JR_OSSellAmt = 300m;
			job2Charge1.JR_AgentDeclaredCostAmt = 210m;
			job2Charge1.JR_AgentDeclaredCostAmtLocal = 300m;
			job2Charge1.JR_AgentDeclaredSellAmtLocal = 600m;
			job2Charge1.JR_AgentDeclaredSellAmt = 600m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);
			AssertEquals("Precondition: Shipment 2 job has no PS charge", 1, job2.Charges.Count);

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			AssertEquals("Precondition: ", 2, jobs.Count);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			postManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(postManager_ProfitShareConfirmation);
			postManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			AssertEquals("Consol has PS charge", 2, apps.CostsCollection.Count);
			AssertEquals("Consol profit share is for USD", "USD", apps.CostsCollection[0].E6_RX_NKCurrency);
			AssertEquals("Consol profit share is apportioned", 0m, apps.CostsCollection[0].UnApportionedAmount);
			AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps.CostsCollection[0].E6_AC_ChargeCode);
			AssertEquals("Consol profit share OS amount is correct", -78M, apps.CostsCollection[0].E6_OSCostAmount);
			AssertEquals("Consol profit share local amount is correct", -84M, apps.CostsCollection[0].E6_LocalCostAmount);
			AssertEquals("Consol profit share is for AUD", "AUD", apps.CostsCollection[1].E6_RX_NKCurrency);
			AssertEquals("Consol profit share is apportioned", 0m, apps.CostsCollection[1].UnApportionedAmount);
			AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps.CostsCollection[1].E6_AC_ChargeCode);
			AssertEquals("Consol profit share OS amount is correct", 330M, apps.CostsCollection[1].E6_OSCostAmount);
			AssertEquals("Consol profit share local amount is correct", 330M, apps.CostsCollection[1].E6_LocalCostAmount);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestProfitShare_CreateChargesPerCurrency_ForReciprocalCompany()
		{
			GlbCompany currentCompanyInLocalFactory = new BusinessObjectFactory().Load<GlbCompany>(Env.CurrentCompany.PK);
			currentCompanyInLocalFactory.GC_IsReciprocal = true;
			currentCompanyInLocalFactory.Factory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			RefCurrency postingCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal postingExchangeRate = 1m;
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(GlbCompany.CurrentCompany.OrgProxy);

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = GlbCompany.CurrentCompany.OrgProxy.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;
			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = "AUSYD";
			profitShare1.O4_ReceivingPortOrCountry = "USLAX";

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = 60m;

			OrgProfitShareParty sndParty = profitShare1.PartyDetails.AddNew();
			sndParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sndParty.PS_PartyProfitSharePercent = 40m;

			Factory.Save();

			// Jobs
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job1, TestObjectCreator.USD, 0.5M);
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = TestObjectCreator.CC1.PK;
			job1Charge1.JR_RX_NKCostCurrency = "USD";
			job1Charge1.JR_RX_NKSellCurrency = "USD";
			job1Charge1.JR_RX_NKSellCurrency = "USD";
			job1Charge1.JR_OSCostAmt = 100;
			job1Charge1.JR_AgentDeclaredSellAmt = 400m;
			job1Charge1.JR_AgentDeclaredCostAmt = 100m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.USD, 0.7M);
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = TestObjectCreator.CC2.PK;
			job2Charge1.JR_RX_NKCostCurrency = "USD";
			job2Charge1.JR_RX_NKSellCurrency = "USD";
			job2Charge1.JR_OSCostAmt = 210;
			job2Charge1.JR_AgentDeclaredSellAmt = 190m;
			job2Charge1.JR_AgentDeclaredCostAmt = 20m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			AssertEquals("Precondition: ", 2, jobs.Count);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			postManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(postManager_ProfitShareConfirmation);
			postManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			Factory.Save();

			AssertEquals("Consol has PS charge", 1, apps.CostsCollection.Count);
			AssertEquals("Consol profit share is apportioned", 0m, apps.CostsCollection[0].UnApportionedAmount);
			AssertEquals("Consol profit share is correct charge code", AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, apps.CostsCollection[0].E6_AC_ChargeCode);
			AssertEquals("Consol profit share OS amount is correct", 282M, apps.CostsCollection[0].E6_OSCostAmount);
			AssertEquals("Consol profit share local amount is correct", 161.40M, apps.CostsCollection[0].E6_LocalCostAmount);
		}

		public void TestProfitShare_TotalApportionedAmountEqualsToTheCostAmount_Export()
		{
			TestProfitShare_TotalApportionedAmountEqualsToTheCostAmountCore(true);
		}

		public void TestProfitShare_TotalApportionedAmountEqualsToTheCostAmount_Import()
		{
			TestProfitShare_TotalApportionedAmountEqualsToTheCostAmountCore(false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void TestProfitShare_TotalApportionedAmountEqualsToTheCostAmountCore(bool export)
		{
			//Setup Orgs
			GlbCompany currentCompanyInLocalFactory = new BusinessObjectFactory().Load<GlbCompany>(Env.CurrentCompany.PK);
			currentCompanyInLocalFactory.Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

			OrgHeader overseasAgent = Factory.NewWithValidTestData<OrgHeader>();

			var loadPort = export ? "AUSYD" : "USLAX";
			var dischargePort = export ? "USLAX" : "AUSYD";
			var receivingAgentShare = export ? 60M : 40M;
			var sendingAgentShare = export ? 40M : 60M;
			var relatedPartyOrgParent = export ? overseasAgent : TestObjectCreator.Agent2;

			TestObjectCreator.Creditor1.OH_IsDebtor = true;
			if (export)
			{
				TestObjectCreator.Creditor1.CompanyData.OB_ARVATConfig = "DEF";
			}
			else
			{
				TestObjectCreator.Creditor1.CompanyData.OB_APVATConfig = "DEF";
			}

			TestObjectCreator.CreateRelatedParty(relatedPartyOrgParent, TestObjectCreator.Creditor1, RelatedPartyTypeList.Codes.APNettingGroup, RelatedPartyDirectionList.Codes.Forwarder);

			//Setup Rate and Registry
			RefCurrency postingCurrency = TestObjectCreator.USD;
			ZDecimal postingExchangeRate = 0.7248M;
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			// Setup consol & shipment
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_TransportMode = "AIR";

			var sendAddress = TestObjectCreator.CreateAddress(TestObjectCreator.Agent2, OrgAddressType.Office, true);
			consol.JK_OA_SendingForwarderAddress = sendAddress.PK;
			if (export)
			{
				var recAddress = TestObjectCreator.CreateAddress(overseasAgent, OrgAddressType.Office, true);
				consol.JK_OA_ReceivingForwarderAddress = recAddress.PK;
			}

			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment1.JS_RL_NKOrigin = loadPort;
			shipment1.JS_RL_NKDestination = dischargePort;
			shipment1.JS_OH_DeliveryAgent = overseasAgent.PK;

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment2.JS_RL_NKOrigin = loadPort;
			shipment2.JS_RL_NKDestination = dischargePort;
			shipment2.JS_OH_DeliveryAgent = overseasAgent.PK;

			// Setup Profit Share agreements
			OrgAgentRelationship agentRelationship1 = Factory.New<OrgAgentRelationship>();
			agentRelationship1.O3_OH_SendingAgent = TestObjectCreator.Agent2.PK;
			agentRelationship1.O3_OH_ReceivingAgent = overseasAgent.PK;

			OrgProfitShareDetails profitShare1 = agentRelationship1.ProfitShareDetails.AddNew();
			profitShare1.O4_FreightMode = "AIR";
			profitShare1.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShare1.O4_EndDate = ZDateTime.Today.AddDays(30);
			profitShare1.O4_SendingPortOrCountry = loadPort;
			profitShare1.O4_ReceivingPortOrCountry = dischargePort;

			OrgProfitShareParty rcvParty = profitShare1.PartyDetails.AddNew();
			rcvParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			rcvParty.PS_PartyProfitSharePercent = receivingAgentShare;

			OrgProfitShareParty sndParty = profitShare1.PartyDetails.AddNew();
			sndParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sndParty.PS_PartyProfitSharePercent = sendingAgentShare;

			Factory.Save();

			// Jobs
			Job job1 = new Job.Loader(Factory, shipment1).TryLoadOrCreateWithMutex();
			job1.PlugInData = shipment1;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job1, TestObjectCreator.USD, 0.5123M);
			Charge job1Charge1 = job1.Charges.AddNew();
			job1Charge1.JR_AC = TestObjectCreator.FRT.PK;
			job1Charge1.JR_RX_NKSellCurrency = "USD";
			job1Charge1.JR_AgentDeclaredSellAmt = 252.25m;
			job1Charge1.JR_IsIncludedInProfitShare = true;

			Job job2 = new Job.Loader(Factory, shipment2).TryLoadOrCreateWithMutex();
			job2.PlugInData = shipment2;
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			TestObjectCreator.SetExchangeRate(job2, TestObjectCreator.USD, 0.7293M);
			Charge job2Charge1 = job2.Charges.AddNew();
			job2Charge1.JR_AC = TestObjectCreator.FRT.PK;
			job2Charge1.JR_RX_NKSellCurrency = "USD";
			job2Charge1.JR_AgentDeclaredSellAmt = 1.15m;
			job2Charge1.JR_IsIncludedInProfitShare = true;

			//Consol Cost
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 249.32M, TestObjectCreator.Creditor1);
			consolCost.E6_IsForCollectInvoice = true;
			consolCost.E6_RX_NKCurrency = "USD";
			consolCost.E6_ExchangeRate = 0.7293M;
			consolCost.E6_InvoiceNum = "NTESTVAL";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			consolCost.E6_ApportionmentMethod = "MAN";

			consolCost.ApportionmentCharges[0].JR_IsIncludedInProfitShare = true;
			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 248.11M;

			consolCost.ApportionmentCharges[1].JR_IsIncludedInProfitShare = true;
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = true;
			consolCost.ApportionmentCharges[1].JR_OSCostAmt = 1.21M;

			Factory.Save();

			AssertEquals("Precondition: Shipment 1 job has no PS charge", 1, job1.Charges.Count);

			JobCollection jobs = new JobCollection(Factory);
			jobs.Load();
			AssertEquals("Precondition: ", 2, jobs.Count);

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs.Cast<Job>(), consol, apps);
			postManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(postManager_ProfitShareConfirmation);
			postManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			AssertEquals("Consol has PS charge", 3, apps.CostsCollection.Count);
			var pSCost = apps.CostsCollection.Where(x => x.E6_AC_ChargeCode == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value && x.E6_RX_NKCurrency == "USD").FirstOrDefault();
			AssertNotNull("A profit share consol cost should exist", pSCost);
			AssertEquals("Consol profit share is apportioned", 0m, pSCost.UnApportionedAmount);
			AssertEquals("Consol profit share OS amount is correct", 66.56M, pSCost.E6_OSCostAmount);
			AssertEquals("Total of Apportioned OS amount is correct", 66.56M, pSCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_OSCostAmt));
			AssertEquals("Consol profit share local amount is correct", 91.26M, pSCost.E6_LocalCostAmount);
			AssertEquals("Total of Apportioned Local amount is correct", 91.26M, pSCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_LocalCostAmt));
		}

		public void TestProfitSharePostingOverseasAgentChargesWhenCreateChargesPerCurrencyRegistryIsTrueAndOneShipmentHasNoChargesToBeInlucdedInProfitShare()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			#region Test Data Setup

			var overseasAgent = TestObjectCreator.CreateOrgHeader("MAOEWR", true, true);

			//Profit Share agreement 50 - 50 between sending and receiving agents
			var agentRelationship1 = TestObjectCreator.CreateAgentRelationship(overseasAgent, GlbCompany.CurrentCompany.OrgProxy);
			var profitShareAgreement = TestObjectCreator.CreateProfitShare(agentRelationship1, 50m, 50, "USLAX", "AUSYD", "ALL");
			profitShareAgreement.O4_StartDate = ZDateTime.Today.AddDays(-3);
			profitShareAgreement.O4_EndDate = ZDateTime.Today.AddDays(30);

			//Consol, Shipments and jobs
			var consol = TestObjectCreator.CreateConsol("USLAX", "AUSYD", "C00010");
			consol.JK_TransportMode = "AIR";
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.CreateAddress(overseasAgent, OrgAddressType.Office, true).PK;
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var shipment1 = TestObjectCreator.CreateShipment("S00011", "USLAX", "AUSYD", consol, false, "AIR");
			shipment1.JS_OuterPacks = 68;
			shipment1.JS_ActualVolume = 4.99m;
			shipment1.JS_ActualWeight = 382m;

			var shipment2 = TestObjectCreator.CreateShipment("S00012", "USLAX", "AUSYD", consol, false, "AIR");
			shipment1.JS_OuterPacks = 15;
			shipment1.JS_ActualVolume = 0.54m;
			shipment1.JS_ActualWeight = 270m;

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);

			Factory.Save();

			//Consol Cost
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, 100m, TestObjectCreator.Creditor1);
			consolCost.E6_RX_NKCurrency = "AUD";
			consolCost.E6_ExchangeRate = 1M;
			consolCost.E6_InvoiceNum = "INV001";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			consolCost.E6_ApportionmentMethod = "CHG";

			consolCost.ApportionmentCharges[0].JR_OSCostAmt = 100M;
			consolCost.ApportionmentCharges[0].JR_IsIncludedInProfitShare = true;
			consolCost.ApportionmentCharges[0].JR_IsUsedForApportionment = true;

			consolCost.ApportionmentCharges[1].JR_OSCostAmt = 0M;
			consolCost.ApportionmentCharges[1].JR_IsUsedForApportionment = false;

			Factory.Save();

			//Job Charges
			AssertEquals(1, job1.Charges.Count);
			var job1Charge = job1.Charges[0];
			job1Charge.JR_RX_NKSellCurrency = "AUD";
			job1Charge.JR_OSSellExRate = 1m;
			job1Charge.JR_OSSellAmt = 125m;

			AssertEquals(0, job2.Charges.Count);
			var job2Charge = job2.Charges.AddNew();
			job2Charge.JR_AC = TestObjectCreator.CC1.PK;
			job2Charge.JR_RX_NKSellCurrency = "AUD";
			job2Charge.JR_OSSellExRate = 1m;
			job2Charge.JR_OSSellAmt = 30m;
			job2Charge.JR_RX_NKSellCurrency = "AUD";
			job2Charge.JR_OSSellExRate = 1m;
			job2Charge.JR_OSSellAmt = 30m;
			job2Charge.JR_IsIncludedInProfitShare = false;

			Factory.Save();

			#endregion

			var apportionements = new ApportionmentListing(Factory, consol);
			var postManager = new ConsolInvoicingPostManager(Factory, new Job[] { job1, job2 }, consol, apportionements);
			postManager.ProfitShareConfirmation += new ProfitShareConfirmationEventHandler(postManager_ProfitShareConfirmation);
			postManager.CreateTransactions(JobInvoicingPostingOption.Agent);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("Profit Share Consol cost is created", 2, apportionements.CostsCollection.Count);
			var pSConsolCost = apportionements.CostsCollection.Cast<JobConsolCost>().FirstOrDefault(x => x.E6_AC_ChargeCode == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value && x.E6_RX_NKCurrency == "AUD");
			AssertNotNull(pSConsolCost);
			AssertEquals("Consol profit share is apportioned", 0m, pSConsolCost.UnApportionedAmount);
			AssertEquals("Consol profit share OS amount is correct", 12.5M, pSConsolCost.E6_OSCostAmount);

			AssertEquals("PS Charge should be created in Job1", 2, job1.Charges.Count);
			var pSCharge = job1.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_E6 == pSConsolCost.PK);
			AssertNotNull(pSCharge);
			AssertEquals("PS Charge OS amount is correct", 12.5M, pSCharge.JR_OSCostAmt);
			Assert("PS Charge is apportioned", pSCharge.JR_IsApportioned);

			AssertEquals("PS Charge is not created in Job2", 1, job2.Charges.Count);
		}

		bool postManager_ProfitShareConfirmation(object sender, ProfitShareConfirmationEventArgs e)
		{
			return true;
		}

		#endregion

		#region Implementation

		ZGuid SetupProfitShareChargeCodesPerParty()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge1 Description";
			ZGuid receivingAgentChargeCode = chargeCode.PK;
			Factory.Save();

			OrgProfitSharePartyLookups lookups = new OrgProfitSharePartyLookups(null);
			ChargeCodeWithTypeCollection collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)].ChargeCode = receivingAgentChargeCode;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			return receivingAgentChargeCode;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_innerValue ?? (TestObjectCreator_innerValue = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_innerValue;

		#endregion
	}
}
