using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class GatewayProfitRedistributionChargeCreatorTest : TestCaseWithFactory
	{
		public void TestCreateCharges()
		{
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_IsDebtor = true;
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.OH_IsDebtor = true;

			var shipment1 = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: null, deliveryAgent: deliveryAgent);
			var shipment2 = profitShareTestHelper.CreateShipment("SHP2", pickupAgent: pickupAgent, deliveryAgent: null);
			var shipment3 = profitShareTestHelper.CreateShipment("SHP3", pickupAgent: pickupAgent, deliveryAgent: deliveryAgent);
			var shipment4 = profitShareTestHelper.CreateShipment("SHP4", pickupAgent: null, deliveryAgent: null);

			Factory.Save();

			AssertConsolChargesCreated("C00001", shipment1, 200m, 1, null, deliveryAgent, null, -200m);
			AssertConsolChargesCreated("C00002", shipment2, 200m, 1, pickupAgent, null, -200m, null);
			AssertConsolChargesCreated("C00003", shipment3, 200m, 2, pickupAgent, deliveryAgent, -60m, -140m);
			AssertConsolChargesCreated("C00004", shipment4, 200m, 0, null, null, null, null);
		}

		[DisableZeroExchangeRateOverriding]
		public void TestCreateCharges_WhenSellCurrencyIsNotLocalCurrency()
		{
			var sellCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "INR");
			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_IsCreditor = true;
			agent.CompanyData.OB_RX_NKARDDefltCurrency = sellCurrency.Code;

			var shipment = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: agent);
			var consol = profitShareTestHelper.CreateConsol("C00001");
			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			testObjectCreator.CreateExchangeRate(sellCurrency, 54m);
			using (var consolJob = new Job.Loader(Factory, consol).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var charge = consolJob.Charges.AddNew();
				charge.JR_AC = chargeCode.PK;
				AssertEquals("Pre-condition:Default Sell Currency should be INR", "INR", charge.JR_RX_NKSellCurrency);
				AssertEquals("Pre-condition:Local Currency should be AUD", "AUD", charge.JR_LocalCurrencyCode);
				consolJob.Charges.RemoveAndDeleteAll();
			}
			Factory.Save();

			using (var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var calculator = new GatewayProfitRedistributionCalculator(Factory, new ProfitShareForwardingConsolWrapper(consol), new ProfitShareForwardingShipmentWrapper(shipment), orgProfitShareDetails, 10m);
				var calculatedProfitShares = calculator.CreateProfitShares();

				using (var chargeCreator = new GatewayProfitRedistributionChargeCreator(Factory, consol, calculatedProfitShares, shipment.JS_UniqueConsignRef))
				{
					var result = chargeCreator.CreateCharges();
					Assert(result);
					Assert("No validation errors", chargeCreator.ValidationErrors.IsEmpty);
				}
			}

			using (var consolJob = consol.Job as Job)
			{
				AssertEquals(1, consolJob.Charges.Count);
				var charge = consolJob.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_OH_SellAccount == agent.PK);
				AssertChargeDetails_AR(charge, chargeCode.PK, -10m, -540m, sellCurrency.Code, agent.PK, "Profit Share Redistribution", false, shipment);
			}
		}

		void TestCreateCharges_PickupAgentIsTheSameGatewayCompany_ShouldRunAutoJRJ(bool withValidDebtor)
		{
			// Pickup agent on shipment is the same as the GW company (current login company).
			var pickupAgent = GlbCompany.CurrentCompany.OrgProxy;

			var consol = profitShareTestHelper.CreateConsol("C00001");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: pickupAgent, forwardingConsols: new[] { consol });

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsDebtor = withValidDebtor;

			// Prepaid Export Shipment will consider consignor as the default debtor.
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_INCO = "CFR";

			using var shipmentJob = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex();
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var calculator = new GatewayProfitRedistributionCalculator(
				Factory,
				new ProfitShareForwardingConsolWrapper(consol),
				new ProfitShareForwardingShipmentWrapper(shipment),
				orgProfitShareDetails,
				-100); // Negative profit share amount to test the debtor side of the PS charge on shipment.
			var calculatedProfitShares = calculator.CreateProfitShares();

			using var chargeCreator = new GatewayProfitRedistributionChargeCreator(Factory, consol, calculatedProfitShares, shipment.JS_UniqueConsignRef);
			chargeCreator.CreateCharges();

			using var consolJob = consol.Job as Job;
			var consolCharge = consolJob.Charges.Single() as Charge;

			AssertEquals(pickupAgent.PK, consolCharge.JR_OH_SellAccount);
			AssertNotNull("Pickup Agent is in the same GW Company: Internal Job should be set", consolCharge.InternalJob);
			AssertNotEquals("Pickup Agent is in the same GW Company: Internal Dept should be set", ZGuid.Empty, consolCharge.JR_GE_InternalDept);
			AssertNotEquals("Pickup Agent is in the same GW Company: Internal Branch should be set", ZGuid.Empty, consolCharge.JR_GB_InternalBranch);

			AssertEquals("Before saving, AutoJRJ has not run, hence there is no charge posted to shipment", 0, shipmentJob.Charges.Count);
			Factory.Save();

			var shipmentCharge = shipmentJob.Charges.Single() as Charge;
			if (withValidDebtor)
			{
				AssertEquals("Debtor is a valid AR, it should be set in the shipment charge.", consignor.PK, shipmentCharge.JR_OH_SellAccount);
			}
			else
			{
				AssertEquals("Debtor is NOT valid AR, it should NOT be set in the shipment charge.", ZGuid.Empty, shipmentCharge.JR_OH_SellAccount);
			}
		}

		public void TestCreateCharges_PickupAgentIsTheSameGatewayCompany_ShouldRunAutoJRJ_WithValidShipmentDebtor() =>
			TestCreateCharges_PickupAgentIsTheSameGatewayCompany_ShouldRunAutoJRJ(withValidDebtor: true);

		public void TestCreateCharges_PickupAgentIsTheSameGatewayCompany_ShouldRunAutoJRJ_WithInvalidShipmentDebtor() =>
			TestCreateCharges_PickupAgentIsTheSameGatewayCompany_ShouldRunAutoJRJ(withValidDebtor: false);

		public void TestCreateCharges_PickupAgentIsNotTheSameGatewayCompany_ShouldNotRunAutoJRJ()
		{
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.OH_IsDebtor = true;
			AssertNotEquals("Pre-condition: Pickup Agent is NOT in the same GW Company", GlbCompany.CurrentCompany.OrgProxy.PK, pickupAgent.PK);

			var shipment = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: pickupAgent);
			using var shipmentJob = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex();
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

			var consol = profitShareTestHelper.CreateConsol("C00001");
			var calculator = new GatewayProfitRedistributionCalculator(
				Factory,
				new ProfitShareForwardingConsolWrapper(consol),
				new ProfitShareForwardingShipmentWrapper(shipment),
				orgProfitShareDetails, 100);
			var calculatedProfitShares = calculator.CreateProfitShares();

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			using var chargeCreator = new GatewayProfitRedistributionChargeCreator(Factory, consol, calculatedProfitShares, shipment.JS_UniqueConsignRef);
			chargeCreator.CreateCharges();

			using var consolJob = consol.Job as Job;
			var charge = consolJob.Charges.Single() as Charge;

			AssertEquals(pickupAgent.PK, charge.JR_OH_SellAccount);
			AssertEquals("Pickup Agent is NOT in the same GW Company: Internal Job should not be set", ZGuid.Empty, charge.JR_JH_InternalJob);
			AssertEquals("Pickup Agent is NOT in the same GW Company: Internal Dept should not be set", ZGuid.Empty, charge.JR_GE_InternalDept);
			AssertEquals("Pickup Agent is NOT in the same GW Company: Internal Branch should not be set", ZGuid.Empty, charge.JR_GB_InternalBranch);
		}

		/// <summary>
		/// This is a very edge case where the debtor field is set by GW Redistribution should not be overridden by internal branch org proxy from running AutoJRJ.
		/// </summary>
		public void TestCreateCharges_ConsolChargeDebtorShouldNotBeOverriddenByInternalBranchOrgProxy()
		{
			var pickupAgent = GlbCompany.CurrentCompany.OrgProxy;

			var internalBranchProxy = pickupAgent.Factory.NewWithValidTestData<OrgHeader>();
			internalBranchProxy.OH_Code = "InternalBRN";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = internalBranchProxy.PK;

			pickupAgent.Factory.Save();

			var shipment = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: pickupAgent);
			using var shipmentJob = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex();
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

			var consol = profitShareTestHelper.CreateConsol("C00001");
			var calculator = new GatewayProfitRedistributionCalculator(
				Factory,
				new ProfitShareForwardingConsolWrapper(consol),
				new ProfitShareForwardingShipmentWrapper(shipment),
				orgProfitShareDetails, 100);
			var calculatedProfitShares = calculator.CreateProfitShares();

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			using var chargeCreator = new GatewayProfitRedistributionChargeCreator(Factory, consol, calculatedProfitShares, shipment.JS_UniqueConsignRef);
			chargeCreator.CreateCharges();

			using var consolJob = consol.Job as Job;
			var charge = consolJob.Charges.Single() as Charge;

			AssertEquals("Setting internal fields for AutoJRJ should not override debtor field which is set by GW PS Redistribution", pickupAgent.OH_Code, charge.SellAccount.OH_Code);

			// This is not an actual operation since the field is read-only on the UI,
			// just to ensure that manually setting the internal branch still changes the debtor field, outside of GW PS Redistribution.
			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals("Setting internal fields for AutoJRJ should override debtor field with internal branch proxy", internalBranchProxy.OH_Code, charge.SellAccount.OH_Code);
		}

		public void TestCreateCharges_WhenProfitSharePartyIsNotAnAR_ShouldNotSetItToConsolChargeDebtorField()
		{
			var pickupAgent = GlbCompany.CurrentCompany.OrgProxy;
			// Precondition: The target profit share party should not be set as an AR (Accounts Receivable) or Receivables.
			pickupAgent.OH_IsDebtor = false;
			pickupAgent.Factory.Save();

			var shipment = profitShareTestHelper.CreateShipment("SHP1", pickupAgent: pickupAgent);
			using var shipmentJob = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex();
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;

			var consol = profitShareTestHelper.CreateConsol("C00001");
			var calculator = new GatewayProfitRedistributionCalculator(
				Factory,
				new ProfitShareForwardingConsolWrapper(consol),
				new ProfitShareForwardingShipmentWrapper(shipment),
				orgProfitShareDetails, 100);
			var calculatedProfitShares = calculator.CreateProfitShares();

			using var chargeCreator = new GatewayProfitRedistributionChargeCreator(Factory, consol, calculatedProfitShares, shipment.JS_UniqueConsignRef);
			chargeCreator.CreateCharges();

			using var consolJob = consol.Job as Job;
			var charge = consolJob.Charges.Single() as Charge;

			AssertEquals("Debtor field should be empty because PS Party is not an AR.", ZGuid.Empty, charge.JR_OH_SellAccount);
		}

		public void TestLogCreated()
		{
			AssertLogCreated(Factory);
		}

		public void TestLogCreated_ConsolFactoryIsDifferent()
		{
			var newFactory = NewFactory();
			AssertNotEquals("Pre-condition: factory should not be same", newFactory, Factory);

			AssertLogCreated(newFactory);
		}

		public void TestLogCreated_WithShipmentJobNumber_1()
			=> AssertLogCreated(Factory, "SHP0001", "Gateway Profit share charges created(SHP0001): 1");

		public void TestLogCreated_WithShipmentJobNumber_2()
			=> AssertLogCreated(Factory, "SHP0002", "Gateway Profit share charges created(SHP0002): 1");

		#region Implementation

		void AssertConsolChargesCreated(string consolNum,
				ForwardingShipment shipment,
				ZDecimal totalProfitShared,
				int numberOfChargesCreated,
				OrgHeader expectedPickupAgentParty,
				OrgHeader expectedDeliveryAgentParty,
				ZDecimal? expectedPickupAgentShare,
				ZDecimal? expectedDeliveryAgentShare)
		{
			var consol = profitShareTestHelper.CreateConsol(consolNum);
			Factory.Save();

			using (var job = new Job.Loader(Factory, shipment).TryLoadOrCreateWithMutex())
			{
				var calculator = new GatewayProfitRedistributionCalculator(Factory, new ProfitShareForwardingConsolWrapper(consol), new ProfitShareForwardingShipmentWrapper(shipment), orgProfitShareDetails, totalProfitShared);
				var calculatedProfitShares = calculator.CreateProfitShares();

				using (var chargeCreator = new GatewayProfitRedistributionChargeCreator(Factory, consol, calculatedProfitShares, shipment.JS_UniqueConsignRef))
				{
					var result = chargeCreator.CreateCharges();
					AssertEquals(numberOfChargesCreated > 0, result);
					Assert("No validation errors", chargeCreator.ValidationErrors.IsEmpty);

					Factory.Save();
				}
			}

			using (var consolJob = consol.Job as Job)
			{
				AssertNotNull("job should not be null", consolJob);
				AssertEquals(numberOfChargesCreated, consolJob.Charges.Count);
				AssertCharges(consolJob, expectedPickupAgentParty, expectedPickupAgentShare);
				AssertCharges(consolJob, expectedDeliveryAgentParty, expectedDeliveryAgentShare);
			}

			void AssertCharges(Job job, OrgHeader agent, ZDecimal? expectedShare)
			{
				if (agent != null && expectedShare.HasValue)
				{
					var charge = job.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_OH_SellAccount == agent.PK);
					AssertChargeDetails_AR(charge, chargeCode.PK, expectedShare.Value, expectedShare.Value, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, agent.PK, "Profit Share Redistribution", false, shipment);
				}
			}
		}

		void AssertChargeDetails_AR(Charge charge, ZGuid chargeCode, ZDecimal localSellAmount, ZDecimal sellAmount, ZString sellCurrency, ZGuid debtor, ZString desc, ZBool sellPosted, ForwardingShipment shipment)
		{
			AssertNotNull("charge", charge);
			CombineAssertions("charge", () =>
			{
				AssertEquals("JR_AC", chargeCode, charge.JR_AC);
				AssertEquals("JR_LocalSellAmt", localSellAmount, charge.JR_LocalSellAmt);
				AssertEquals("JR_OSSellAmt", sellAmount, charge.JR_OSSellAmt);
				AssertEquals("JR_RX_NKSellCurrency", sellCurrency, charge.JR_RX_NKSellCurrency);
				AssertEquals("JR_OH_SellAccount", debtor, charge.JR_OH_SellAccount);
				AssertEquals("JR_OH_CostAccount", ZGuid.Empty, charge.JR_OH_CostAccount);
				AssertEquals("JR_Desc", desc, charge.JR_Desc);
				AssertEquals("JR_APInvoiceDate.Date", ZDateTime.Empty, charge.JR_APInvoiceDate.Date);
				AssertEquals("JR_APInvoiceNum", ZString.Empty, charge.JR_APInvoiceNum);
				AssertEquals("IsCostPosted", false, charge.IsCostPosted);
				AssertEquals("IsRevenuePosted", sellPosted, charge.IsRevenuePosted);
				AssertEquals("JR_Calc_RelatedJobNumber", shipment.JobNumber, charge.JR_Calc_RelatedJobNumber);
			});
		}

		void AssertLogCreated(BusinessObjectFactory newFactory,
			string shipmentJobNumber = "SHP0001",
			string expectedLogReference = "Gateway Profit share charges created(SHP0001): 1")
		{
			var consol = profitShareTestHelper.CreateConsol("C00001");
			Factory.Save();

			AssertConsolLog(null, true, false);
			AssertConsolLog(consol, false, false);
			AssertConsolLog(null, false, false);
			AssertConsolLog(consol, true, true);

			void AssertConsolLog(ForwardingConsol forwardingConsol, bool chargesUpdatedOrCreatedForTest, bool expectedLog)
			{
				using (var creator = new DummyGatewayProfitRedistributionChargeCreator(newFactory, forwardingConsol, new ProfitShareDetailCollection(), shipmentJobNumber))
				{
					creator.ChargesUpdatedOrCreatedForTest = chargesUpdatedOrCreatedForTest;
					AssertEquals("Pre-condition", chargesUpdatedOrCreatedForTest, creator.CreateCharges());

					newFactory.Save();
				}

				var logs = GetLogsFromDB();

				if (expectedLog)
				{
					AssertNotNull(logs);
					AssertEquals(1, logs.Length);
					AssertLog(logs[0]);
				}
				else
				{
					AssertNull(logs.FirstOrDefault());
				}
			}

			StmALog[] GetLogsFromDB()
			{
				newFactory.Save();//save whatever we have pending in newFactory

				var query = new ZDBOnlyQuery(typeof(StmALog));
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ProfitOfGWConsolsRedistributed.Code);
				return Factory.Load<StmALog>(query);
			}

			void AssertLog(StmALog log)
			{
				AssertNotNull(log);
				AssertEquals("JobHeader", log.SL_Table);
				AssertEquals(consol.Job.PK, log.SL_Parent);
				AssertEquals(Events.ProfitOfGWConsolsRedistributed.Code, log.SL_SE_NKEvent);
				AssertEquals(expectedLogReference, log.SL_Reference);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);

			chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "PSR";
			chargeCode.AC_Desc = "Profit Share Redistribution";

			Factory.Save();
			profitShareTestHelper.SetRegistry(chargeCode.PK);

			var agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = "AGY";
			orgProfitShareDetails = testObjectCreator.CreateGatewayProfitShareRedistribution(agentRelationship, 30, 70, "", "", "AIR", apportionmentMethod: "SHP");
		}

		TestObjectCreator testObjectCreator;
		ProfitShareTestHelper profitShareTestHelper;
		AccChargeCode chargeCode;
		OrgProfitShareDetails orgProfitShareDetails;

		class DummyGatewayProfitRedistributionChargeCreator : GatewayProfitRedistributionChargeCreator
		{
			public DummyGatewayProfitRedistributionChargeCreator(BusinessObjectFactory factory, ForwardingConsol consol, ProfitShareDetailCollection calculatedProfitShares, string shipmentJobNumber)
				: base(factory, consol, calculatedProfitShares, shipmentJobNumber)
			{
			}

			protected override bool CreateChargesCore()
			{
				CreatedChargesCount = 1;
				return ChargesUpdatedOrCreatedForTest;
			}

			public bool ChargesUpdatedOrCreatedForTest = true;
		}

		#endregion
	}
}
