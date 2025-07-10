using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	#region ProfitShareActionMethodApplicatorTest

	[TestedType(typeof(ProfitShareActionMethodApplicator))]
	class ProfitShareActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestNoRegistryValue_ProfitShareChargeCode()
		{
			const string jobErrorLog = @"ERROR: Incorrect Registry Item value.
Please set up a correct value to the Registry Item: 'Accounting -> Job Invoicing -> Profit Share -> Profit Share Charge Code'.
";
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			ApplyApplicator("Invalid registry value", new BusinessObject[] { Factory.New<ForwardingConsol>() }, jobErrorLog);
		}

		public void TestNoRegistryValue_ProfitShareChargeCodesPerParty()
		{
			const string jobErrorLog = @"ERROR: Incorrect Registry Item value.
Please set up a correct value to the Registry Item: 'Accounting -> Job Invoicing -> Profit Share -> Profit Share Charge Codes Per Party'.
";
			var lookups = new OrgProfitSharePartyLookups(null);
			var collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			var chargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			chargeCode.AC_GC = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = chargeCode.PK;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			ApplyApplicator("Invalid registry value", new BusinessObject[] { Factory.New<ForwardingConsol>() }, jobErrorLog);
		}

		public void TestValidCase()
		{
			const string jobErrorLog = @"INFO: [HL Consol C00001000]
INFO: [HL Shipment S001001]
INFO: Profit Share Charge found for creating or updating.
";

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateProfitShareDetails("");
			TestObjectCreator.Creditor1.CompanyData.OB_IsDebtor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			Assert("Precondition: the job charge is persistent.", Job.Charges[0].IsInDatabase);

			ApplyApplicator("Profit share charges are created for consol", new BusinessObject[] { Consol }, jobErrorLog);

			AssertEquals("One profit share charge is created", 2, Job.Charges.Count);
		}

		public void TestPartialValid()
		{
			const string jobErrorLog = @"INFO: [HL Consol C00001000]
INFO: [HL Shipment S001001]
ERROR: Error - JH_OA_AgentCollectAddr: Please enter Local Client or Overseas Agent.
ERROR: Error - JH_OA_LocalChargesAddr: Please enter Local Client or Overseas Agent.
INFO: [HL Shipment S001002]
INFO: Profit Share Charge found for creating or updating.
INFO: [HL Shipment S001003]
INFO: Profit Share Charge found for creating or updating.
";

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var shipment1 = CreateProfitShareDetails("", "S001001", createNewConsol: true);
			var shipment2 = CreateProfitShareDetails("", "S001002", createNewConsol: false);
			var shipment3 = CreateProfitShareDetails("", "S001003", createNewConsol: false);

			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Agent.CompanyData.SetAPTaxApplicable(false);
			TestObjectCreator.Creditor1.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.Creditor1.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			var loader = new Job.Loader(Factory, shipment1);
			var job1 = loader.Load(setParent: true, setJobDefaults: false);
			job1.JH_OA_LocalChargesAddr = Guid.Empty;
			job1.JH_OA_AgentCollectAddr = Guid.Empty;

			loader = new Job.Loader(Factory, shipment2);
			var job2 = loader.Load(setParent: true, setJobDefaults: false);

			loader = new Job.Loader(Factory, shipment3);
			var job3 = loader.Load(setParent: true, setJobDefaults: false);

			AssertEquals("Precondition: Only one charge before profit share", 1, job1.Charges.Count);
			AssertEquals("Precondition: Only one charge before profit share", 1, job2.Charges.Count);
			AssertEquals("Precondition: Only one charge before profit share", 1, job3.Charges.Count);

			ApplyApplicator("Profit share charges are created for consol", new BusinessObject[] { Consol }, jobErrorLog);

			var newFactory = new BusinessObjectFactory();
			var job1InNewFactory = newFactory.Load<Job>(job1.PK);
			var job2InNewFactory = newFactory.Load<Job>(job2.PK);
			var job3InNewFactory = newFactory.Load<Job>(job3.PK);
			AssertEquals("No profit share charge is created for shipment1", 1, job1InNewFactory.Charges.Count);

			AssertEquals("Profit share charge is created for shipment2", 2, job2InNewFactory.Charges.Count);

			AssertEquals("Profit share charge is created for shipment3", 2, job3InNewFactory.Charges.Count);
		}

		public void TestInvalidDebtor()
		{
			const string jobErrorLog = @"INFO: [HL Consol C00001000]
INFO: [HL Shipment S001001]
ERROR: In order to create profit share charges for RCV party, ZCreditor1 has to be marked as Receivables
";

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateProfitShareDetails("");
			Factory.Save();

			ApplyApplicator("No Profit share charge created as invalid debtor detected", new BusinessObject[] { Consol }, jobErrorLog);
		}

		public void TestInvalidJobFound()
		{
			const string jobErrorLog = @"INFO: [HL Consol C00001000]
INFO: [HL Shipment S001001]
INFO: Profit Share Charge found for creating or updating.
INFO: [HL Shipment S001002]
WARNING: No job found.
";

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			CreateProfitShareDetails("");
			TestObjectCreator.Creditor1.CompanyData.OB_IsDebtor = true;

			var shipment1 = Consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S001002";
			Factory.Save();

			ApplyApplicator("Profit share charges are created for consol", new BusinessObject[] { Consol }, jobErrorLog);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProfitShareActionMethodApplicator(ObjectFactory.GetType("ForwardingConsolActionSupporter"));
		}

		ForwardingShipment CreateProfitShareDetails(string rateBasis, string shipmentNumber = "", bool createNewConsol = true)
		{
			if (createNewConsol)
			{
				Consol = Factory.New<ForwardingConsol>();
				Consol.SetDefaultSendingForwarderAddress(TestObjectCreator.Agent);
				Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Creditor1);
			}

			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_UniqueConsignRef = string.IsNullOrEmpty(shipmentNumber) ? "S001001" : shipmentNumber;

			ProfitShareDetails = new ProfitShareDetailCollection();

			CreateProfitShareProfile(TestObjectCreator.Agent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 30m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 18m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 4m, rateBasis, ProfitShareDetails, Shipment);

			Job = TestObjectCreator.CreateJob(Shipment, false);
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = Shipment;

			JobCharge = Job.Charges.AddNew();
			JobCharge.JR_AC = Env.Registry.FreightChargeCode;
			JobCharge.JR_IsIncludedInProfitShare = true;
			JobCharge.JR_LocalSellAmt = 600m;
			JobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			JobCharge.JR_LocalCostAmt = 100m;

			Factory.Save();

			return Shipment;
		}

		void CreateProfitShareProfile(OrgHeader agent, string role, decimal percent, string rateBasis, ProfitShareDetailCollection profitShares, ForwardingShipment shipment)
		{
			OrgAgentRelationship agentProfile = Factory.LoadTop1<OrgAgentRelationship>(new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agent.PK));
			if (agentProfile == null)
			{
				agentProfile = Factory.New<OrgAgentRelationship>();
				agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
				agentProfile.O3_OH_SendingAgent = agent.PK;
			}

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.Count == 1 ? agentProfile.GenericProfitShareDetails[0] : agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = role;
			party.PS_PartyProfitSharePercent = percent;
			party.PS_PartyRateBasis = rateBasis;

			ProfitShareDetail detail = profitShares.AddNew();
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment, agent, role, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fTestObjectCreator;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		JobCharge JobCharge;
		Job Job;
		ProfitShareDetailCollection ProfitShareDetails;
	}

	#endregion

	#region ProfitShareActionMethodApplicatorForQuotedBookingTest

	[TestedType(typeof(ProfitShareActionMethodApplicator))]
	class ProfitShareActionMethodApplicatorForQuotedBookingTest : OperationalActionMethodApplicatorTest
	{
		public void TestInvalidJobFound()
		{
			const string jobErrorLog = @"INFO: Quick Booking - Booking (S001001)
WARNING: No job found.
";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001001";
			shipment.JS_HouseBill = "S001001";

			var booking = CreateQuotedBooking(shipment);

			Factory.Save();

			ApplyApplicator("Profit share applicator for quoted booking", new BusinessObject[] { booking }, jobErrorLog);
		}

		#region Implementation

		BusinessObject CreateQuotedBooking(ForwardingShipment shipment)
		{
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsBooking = true;

			var builder = ObjectFactory.Get<IQuotedBookingBuilder>();
			return (BusinessObject)builder.InitializeFrom(shipment.PK, Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProfitShareActionMethodApplicator(ObjectFactory.GetType("QuotedBookingSupporter"));
		}

		#endregion
	}

	#endregion
}
