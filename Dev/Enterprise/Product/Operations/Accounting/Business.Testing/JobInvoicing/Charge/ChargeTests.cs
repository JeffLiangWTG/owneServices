namespace Enterprise.Accounting.Business.Testing
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Accounting.Registry.Business;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Business.Base.Transaction;
	using Enterprise.Accounting.Business.ConsolRevenue;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Accounting.Integration;
	using Enterprise.Core;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.Rating.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	public class ChargeTests : TestCaseWithFactory
	{
		public void TestIsUsedForApportionment()
		{
			IJobCostingPlugIn consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S1";
			ForwardingShipment shipment2 = ((ForwardingConsol)consol).Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S2";

			var creator = new TestObjectCreator(Factory);
			var master = new ConsolRevenueMaster(consol, Factory);
			var rev = new ConsolRevenue(master);
			rev.ChargeCode = creator.CC1.PK;
			rev.SellAmount = 100m;
			rev.ApportionmentMethod = AllocationMethod.Shipment;

			Assert(rev.SplitCharges[0].IsUsedForApportionment);
			Assert(rev.SplitCharges[1].IsUsedForApportionment);

			rev.SplitCharges[0].JR_OSSellAmt = 0m;
			Assert(!rev.SplitCharges[0].IsUsedForApportionment);
			Assert(rev.SplitCharges[1].IsUsedForApportionment);

			master.ReleaseMutexes();
		}

		public void TestChargeDebtorDefaulting_ExportCollect()
		{
			var orgChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Origin);
			var dstChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			var freightChargeCode = Env.Registry.FreightChargeCode;

			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var ag = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = cnr.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = cnr.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, cnr, "For the FOB IncoTerm: ORG charges should default from Local Client");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, cne, "For the FOB IncoTerm: FRT charges should default from the Agent then from CNE");
			AssertDebtorOnCharge(job.Charges, dstChargeCode, cne, "For the FOB IncoTerm: DST charges should default debtor like FRT");

			job.Charges.RemoveAndDeleteAll();
			job.AgentCollectPK = ag.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, cnr);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, ag, "Overriden agent is prefered to CNE for DST Rates");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, ag, "Overriden agent is prefered to CNE for FRT Rates");

			job.Charges.RemoveAndDeleteAll();
			var lc = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = lc.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, lc, "Overriden LC is prefered");
			AssertDebtorOnCharge(job.Charges, dstChargeCode, ag);
			AssertDebtorOnCharge(job.Charges, freightChargeCode, ag);
		}

		public void TestChargeDebtorDefaulting_ExportPrepaid()
		{
			var orgChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Origin);
			var dstChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			var freightChargeCode = Env.Registry.FreightChargeCode;

			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var ag = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_INCO = Constants.IncoTerms.CostInsuranceAndFreight;
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = cnr.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = cnr.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, cnr, "CIF: ORG charges should default from CNR");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, cnr, "CIF: FRT charges should default from the Agent then from CNR");
			AssertDebtorOnCharge(job.Charges, dstChargeCode, cne, "CIF: DST charges should default debtor CNE");

			job.Charges.RemoveAndDeleteAll();
			job.AgentCollectPK = ag.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, cnr);
			AssertDebtorOnCharge(job.Charges, freightChargeCode, cnr);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, ag, "Agent is prefered to CNE");

			job.Charges.RemoveAndDeleteAll();
			var lc = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = lc.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, lc, "Overriden LC is prefered to CNR");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, lc, "Overriden LC is prefered to CNR");
			AssertDebtorOnCharge(job.Charges, dstChargeCode, ag);
		}

		public void TestChargeDebtorDefaulting_Domestic()
		{
			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var local = Factory.NewWithValidTestData<OrgHeader>();
			var ag = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = cnr.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";

			shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectThirdParty;

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = local.PK;
			job.AgentCollectPK = ag.PK;

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals("Collect 3rd Party means no default debtor is applicable", ZGuid.Empty, charge.JR_OH_SellAccount);

			job.Charges.RemoveAndDeleteAll();
			using (AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				AssertEquals("Collect 3rd Party means no default debtor is applicable, but it should fall back to job local client ", local.PK, charge.JR_OH_SellAccount);
			}

			job.Charges.RemoveAndDeleteAll();
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;

			charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals("We defaulting to local client always even though this should technically be cne ",
				local.PK, charge.JR_OH_SellAccount);

			job.Charges.RemoveAndDeleteAll();
			shipment.ConsigneePK = ZGuid.Empty;

			charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;

			AssertEquals("Agent is not applicable or Domestic job", local.PK, charge.JR_OH_SellAccount);
		}

		public void TestChargeCalculationDescriptionIsLong_ShouldBeTrimmed()
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.Business.Testing.JobInvoicing.Charge.TestFiles.RevenueCalculationDescription.txt"))
			using (var reader = new StreamReader(stream))
			{
				var longNote = reader.ReadToEnd();

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();

				var charge = job.Charges.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.RevenueCalculationDescription = ZBlob.FromUTF8(longNote);

				AssertNoErrors(charge.Notes.FindByDescription("AUTORATE_SELL").FirstOrDefault().ST_NoteDataInfo);

				var actualMessage = System.Text.Encoding.UTF8.GetString(charge.RevenueCalculationDescription);
				var expectedMessageToStart = @"This charge is calculated from multiple rates
Calculation failed due to no UNT to PLT unit conversion present";

				AssertStartsWith("Calculation Message to be Started With:", expectedMessageToStart, actualMessage);
				AssertEndsWith("Calculation Message to End With:", "...trimmed to fit", actualMessage);
			}
		}

		#region IFT Party

		public void TestIFTPartyDefaults_Domestic()
		{
			var orgChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Origin);
			var dstChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Destination);
			var transportChargeCode = GetChargeCode(ChargeCodeGroupList.Codes.Transport);
			var cneIFT = Factory.NewWithValidTestData<OrgHeader>();
			var cnrIFT = Factory.NewWithValidTestData<OrgHeader>();
			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var cnr = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var freightChargeCode = Env.Registry.FreightChargeCode;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = cnr.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.AgentCollectPK = Factory.NewWithValidTestData<OrgHeader>().PK; //Adding an overseas agent but it should never be used for a domestic job
			job.LocalChargesPK = cnrIFT.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, cnrIFT, "For now at least domestic LC should always be debtor");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, cnrIFT);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, cnrIFT);
			AssertDebtorOnCharge(job.Charges, transportChargeCode, cnrIFT);

			job.Charges.RemoveAndDeleteAll();
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Collect;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, cnrIFT);
			AssertDebtorOnCharge(job.Charges, freightChargeCode, cnrIFT);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, cnrIFT);
			AssertDebtorOnCharge(job.Charges, transportChargeCode, cnrIFT);//, "Transport is not a freight job so should not default debtor to IFT party");

			job.Charges.RemoveAndDeleteAll();
			shipment.JS_INCO = Constants.DomesticPaymentTerms.CollectThirdParty;
			job.LocalChargesPK = cneIFT.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, null, "Cannot be cnr/cnr with this payment term so nothing should default");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, null);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, null);
			AssertDebtorOnCharge(job.Charges, transportChargeCode, null);

			job.Charges.RemoveAndDeleteAll();
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertDebtorOnCharge(job.Charges, orgChargeCode, cneIFT);
			AssertDebtorOnCharge(job.Charges, freightChargeCode, cneIFT);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, cneIFT);
			AssertDebtorOnCharge(job.Charges, transportChargeCode, cneIFT);

			job.Charges.RemoveAndDeleteAll();
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = localClient.PK;

			AssertDebtorOnCharge(job.Charges, orgChargeCode, localClient);
			AssertDebtorOnCharge(job.Charges, freightChargeCode, localClient);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, localClient);
			AssertDebtorOnCharge(job.Charges, transportChargeCode, localClient);

			job.Charges.RemoveAndDeleteAll();
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertDebtorOnCharge(job.Charges, orgChargeCode, null, "Works the same");
			AssertDebtorOnCharge(job.Charges, freightChargeCode, null);
			AssertDebtorOnCharge(job.Charges, dstChargeCode, null);
			AssertDebtorOnCharge(job.Charges, transportChargeCode, null);
		}

		#endregion

		#region Charge Always Registry

		public void TestRegistryChargeDebtorDefaulting_ChargeAgentAlwaysCodes()
		{
			var chargeCode1 = GetChargeCode(ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = GetChargeCode(ChargeCodeGroupList.Codes.Destination);
			var chargeCode3 = GetChargeCode(ChargeCodeGroupList.Codes.Insurance);
			Factory.Save();

			var freightChargeCode = Env.Registry.FreightChargeCode;

			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var ag = Factory.NewWithValidTestData<OrgHeader>();
			var lc = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_INCO = Core.Constants.IncoTerms.DeliveredDutyPaid;
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = cnr.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var jobCharges = job.Charges;

			AssertDebtorOnCharge(jobCharges, freightChargeCode, cnr, "For the DDP charges should default from Local Client (consignor)");
			AssertDebtorOnCharge(jobCharges, chargeCode1, cnr);
			AssertDebtorOnCharge(jobCharges, chargeCode2, cnr);
			AssertDebtorOnCharge(jobCharges, chargeCode3, cnr);

			job.Charges.RemoveAndDeleteAll();
			job.LocalChargesPK = lc.PK;
			job.AgentCollectPK = ag.PK;

			AssertDebtorOnCharge(jobCharges, freightChargeCode, lc, "For the DDP: charges should default from Local Client");
			AssertDebtorOnCharge(jobCharges, chargeCode1, lc);
			AssertDebtorOnCharge(jobCharges, chargeCode2, lc);
			AssertDebtorOnCharge(jobCharges, chargeCode3, lc);

			job.Charges.RemoveAndDeleteAll();
			var chargeCodes = new[] { freightChargeCode.ToString(), chargeCode1.ToString(), chargeCode2.ToString(), chargeCode3.ToString() };
			var registryValues = new ZStringBuilder(chargeCodes).ToStringWithDelimiterBetweenAppends(",");
			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValues);
			Factory.Save();

			AssertDebtorOnCharge(jobCharges, chargeCode1, ag);
			AssertDebtorOnCharge(jobCharges, chargeCode2, ag);
			AssertDebtorOnCharge(jobCharges, chargeCode3, ag);
			AssertDebtorOnCharge(jobCharges, freightChargeCode, ag);

			job.Charges.RemoveAndDeleteAll();
			job.AgentCollectPK = cne.PK;

			AssertDebtorOnCharge(jobCharges, freightChargeCode, cne, "Should always default to overseas agent on the job due to registry");
			AssertDebtorOnCharge(jobCharges, chargeCode1, cne);
			AssertDebtorOnCharge(jobCharges, chargeCode2, cne);
			AssertDebtorOnCharge(jobCharges, chargeCode3, cne);
		}

		public void TestRegistryChargeDebtorDefaulting_ChargeLocalClientAlwaysCodes()
		{
			var chargeCode1 = GetChargeCode(ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = GetChargeCode(ChargeCodeGroupList.Codes.Destination);
			var chargeCode3 = GetChargeCode(ChargeCodeGroupList.Codes.Insurance);
			Factory.Save();

			var freightChargeCode = Env.Registry.FreightChargeCode;

			var cne = Factory.NewWithValidTestData<OrgHeader>();
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var ag = Factory.NewWithValidTestData<OrgHeader>();
			var lc = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_INCO = Core.Constants.IncoTerms.ExWorks;
			shipment.ConsigneePK = cne.PK;
			shipment.ConsignorPK = cnr.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var jobCharges = job.Charges;

			AssertDebtorOnCharge(jobCharges, freightChargeCode, cne, "For the EXW charges should default from Consignee");
			AssertDebtorOnCharge(jobCharges, chargeCode1, cne);
			AssertDebtorOnCharge(jobCharges, chargeCode2, cne);
			AssertDebtorOnCharge(jobCharges, chargeCode3, cne);

			job.Charges.RemoveAndDeleteAll();
			job.LocalChargesPK = lc.PK;
			job.AgentCollectPK = ag.PK;

			AssertDebtorOnCharge(jobCharges, freightChargeCode, ag, "For the EXW: charges should default from Agent");
			AssertDebtorOnCharge(jobCharges, chargeCode1, ag);
			AssertDebtorOnCharge(jobCharges, chargeCode2, ag);
			AssertDebtorOnCharge(jobCharges, chargeCode3, ag);

			job.Charges.RemoveAndDeleteAll();
			var chargeCodes = new[] { freightChargeCode.ToString(), chargeCode1.ToString(), chargeCode2.ToString(), chargeCode3.ToString() };
			var registryValues = new ZStringBuilder(chargeCodes).ToStringWithDelimiterBetweenAppends(",");
			IncoTermRegistry.Instance.ChargeLocalClientAlwaysCodes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValues);
			Factory.Save();

			AssertDebtorOnCharge(jobCharges, freightChargeCode, lc, "As per the registry setting, should be overriden");
			AssertDebtorOnCharge(jobCharges, chargeCode1, lc);
			AssertDebtorOnCharge(jobCharges, chargeCode2, lc);
			AssertDebtorOnCharge(jobCharges, chargeCode3, lc);

			job.Charges.RemoveAndDeleteAll();
			job.LocalChargesPK = cnr.PK;

			AssertDebtorOnCharge(jobCharges, freightChargeCode, cnr, "Should always default to local client on the job due to registry");
			AssertDebtorOnCharge(jobCharges, chargeCode1, cnr);
			AssertDebtorOnCharge(jobCharges, chargeCode2, cnr);
			AssertDebtorOnCharge(jobCharges, chargeCode3, cnr);
		}

		#endregion

		#region CanClearAutoratedCostOrSell

		public void TestCanClearAutoratedCostOrSell()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			var operationalJobCode = shipment.RatingAdapter.OperationalJobCode;

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var manuallyAddedCharge = job.Charges.AddNew();
			manuallyAddedCharge.JR_AC = TestObjectCreator.CC1.PK;

			var chargeWithPaymentBasis = job.Charges.AddNew();
			chargeWithPaymentBasis.JR_AC = TestObjectCreator.CC2.PK;
			AddPaymentBasis(chargeWithPaymentBasis, true, operationalJobCode, Constants.CurrencyCodes.Australia);
			AddPaymentBasis(chargeWithPaymentBasis, false, operationalJobCode, Constants.CurrencyCodes.Australia);

			Assert("Prerequisites: rating not overriden", !manuallyAddedCharge.JR_CostRatingOverride && !manuallyAddedCharge.JR_SellRatingOverride);
			Assert("Prerequisites: not posted or apportioned", !manuallyAddedCharge.JR_IsApportioned && !manuallyAddedCharge.JR_IsPosted);
			Assert("Prerequisites: has no payment basis", !manuallyAddedCharge.CostPaymentBases.Any() && !manuallyAddedCharge.SellPaymentBases.Any());

			Assert("Prerequisites: rating not overriden", !chargeWithPaymentBasis.JR_CostRatingOverride && !chargeWithPaymentBasis.JR_SellRatingOverride);
			Assert("Prerequisites: not posted or apportioned", !chargeWithPaymentBasis.JR_IsApportioned && !chargeWithPaymentBasis.JR_IsPosted);
			Assert("Prerequisites: has payment basis", chargeWithPaymentBasis.CostPaymentBases.Any() && chargeWithPaymentBasis.SellPaymentBases.Any());

			var operationalJobCodes = new ZString[] { operationalJobCode };

			Assert("No matching payment basis with REA - should be able to clear cost", manuallyAddedCharge.CanReautorate(CostSell.Cost, operationalJobCodes));
			Assert("No matching payment basis with REA - should be able to clear sell", manuallyAddedCharge.CanReautorate(CostSell.Revenue, operationalJobCodes));

			Assert("Should be possible to clear autorated cost", chargeWithPaymentBasis.CanReautorate(CostSell.Cost, operationalJobCodes));
			Assert("Should be possible to clear autorated sell", chargeWithPaymentBasis.CanReautorate(CostSell.Revenue, operationalJobCodes));

			operationalJobCodes = new ZString[] { "SOMEGARBAGE" };

			Assert("No matching payment basis adapter ID", !chargeWithPaymentBasis.CanReautorate(CostSell.Cost, operationalJobCodes));
			Assert("No matching payment basis adapter ID", !chargeWithPaymentBasis.CanReautorate(CostSell.Revenue, operationalJobCodes));

			operationalJobCodes = new ZString[] { operationalJobCode };

			var commentCharge = job.Charges.AddNew();
			commentCharge.JR_AC = TestObjectCreator.CommentChargeCode.PK;
			AddPaymentBasis(commentCharge, true, operationalJobCode, Constants.CurrencyCodes.Australia);
			AddPaymentBasis(commentCharge, false, operationalJobCode, Constants.CurrencyCodes.Australia);

			Assert("Should still be possible to clear", commentCharge.CanReautorate(CostSell.Cost, operationalJobCodes));
			Assert("Should still be possible to clear", commentCharge.CanReautorate(CostSell.Revenue, operationalJobCodes));
		}

		#endregion

		#region Payment Basis

		public void TestDeletingACharge_DeletesPaymentBases()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var jobCharges = job.Charges;
			var newCharge = jobCharges.AddNew();
			newCharge.JR_AC = Env.Registry.FreightChargeCode;
			newCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			newCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var paymentBasesToAdd = new[]
			{
				new PaymentBasis(new Quantity(1, "KG"), RateInfo.CreateFLT(10m, "AUD"), AdapterType.Shipment, "SHP"),
				new PaymentBasis(new Quantity(2, "KG"), RateInfo.CreateFLT(10m, "AUD"), AdapterType.Shipment, "SHP"),
			};

			newCharge.AddPaymentBases(paymentBasesToAdd, true);

			Factory.Save();

			var chargePK = newCharge.PK;
			AssertEquals("Charge should have payment bases", 2, newCharge.CostPaymentBases.Count);
			job.Charges.RemoveAndDeleteAll();

			Factory.Save();
			var paymentBases = Factory.Load<JobPaymentBasis>(new ZQuery(JobPaymentBasisSchema.PBS_E6, chargePK));
			AssertEquals("Should not have any payment bases", 0, paymentBases.Length);
		}

		public void TestPaymentBasis_IsDeletedByOSAmountChanges_Cost()
		{
			AssertPaymentBasisIsDeletedByOSAmountChanges(true);
		}

		public void TestPaymentBasis_IsDeletedByOSAmountChanges_Sell()
		{
			AssertPaymentBasisIsDeletedByOSAmountChanges(false);
		}

		void AssertPaymentBasisIsDeletedByOSAmountChanges(bool isCost)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var jobCharges = job.Charges;

			var charge = jobCharges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_RX_NKCostCurrency = Constants.CurrencyCodes.Bolivia;
			charge.JR_RX_NKSellInvoiceCurrency = Constants.CurrencyCodes.Bolivia;
			charge.JR_OSCostAmt = 1000;
			charge.JR_OSSellAmt = 1000;
			charge.JR_CostRatingOverride = false;
			charge.JR_SellRatingOverride = false;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			AddPaymentBasis(charge, isCost, shipment.JS_UniqueConsignRef, Constants.CurrencyCodes.Bolivia);

			var collection = isCost ? charge.CostPaymentBases : charge.SellPaymentBases;
			var rateType = isCost ? charge.JR_CostRatingOverride : charge.JR_SellRatingOverride;

			AssertEquals("Charge should have payment bases added", 2, collection.Count);
			AssertEquals("Should remain at false", false, rateType);

			var exchangeRate = job.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = Constants.CurrencyCodes.Bolivia;
			exchangeRate.JF_BaseRate = 5;
			exchangeRate.JF_OrgType = isCost ? "CRD" : "DEB";
			exchangeRate.JF_IsTransformed = true;

			collection = isCost ? charge.CostPaymentBases : charge.SellPaymentBases;
			rateType = isCost ? charge.JR_CostRatingOverride : charge.JR_SellRatingOverride;
			var localAmount = isCost ? charge.JR_LocalCostAmt : charge.JR_LocalSellAmt;

			AssertEquals("Pre-condition: local cost to be changed by currency conversion", 200m, localAmount);
			AssertEquals("Adding an exchange rate should not clear collection", 2, collection.Count);
			AssertEquals("Should remain at false", false, rateType);

			charge.JR_OSCostAmt = 777;
			charge.JR_OSSellAmt = 888;

			Factory.Save();

			var paymentBases = Factory.Load<JobPaymentBasis>(new ZQuery(JobPaymentBasisSchema.PBS_E6, charge.PK));

			rateType = isCost ? charge.JR_CostRatingOverride : charge.JR_SellRatingOverride;
			AssertEquals("Rate type should be overriden", true, rateType);
			AssertEquals("Should be cleared", 0, paymentBases.Length);
		}

		#endregion

		#region Implementation

		ZGuid GetChargeCode(string chargeGroup)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = chargeGroup;

			return chargeCode.PK;
		}

		static void AssertDebtorOnCharge(ChargeCollection charges, ZGuid code, OrgHeader expectedDebtor, string message = "")
		{
			var charge = charges.AddNew();
			charge.JR_AC = code;

			if (expectedDebtor != null)
			{
				AssertEquals(message, expectedDebtor.PK, charge.JR_OH_SellAccount);
			}
			else
			{
				Assert(message, charge.JR_OH_SellAccount.IsEmpty);
			}
		}

		static void AddPaymentBasis(Charge charge, bool isCost, ZString operationalJobCode, ZString currencyCode)
		{
			var quantity = new Quantity(1, QuantityUnit.HB);
			var rateInfo = RateInfo.CreateFLT(500m, currencyCode);
			var paymentBasesToAdd = new[]
			{
				new PaymentBasis(quantity, rateInfo, AdapterType.Shipment, operationalJobCode),
				new PaymentBasis(quantity, rateInfo, AdapterType.Shipment, operationalJobCode),
			};

			charge.AddPaymentBases(paymentBasesToAdd, isCost);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}

	public sealed class JobChargeCriticalValidationTest : CriticalValidationTest<JobCharge>
	{
		public void TestAmendmentOfPostedRevenueIsPermitted()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var forbiddenCompany = GlbCompany.CurrentCompany;

			using (AccountingMasterFilesRegistry.Instance.AllowNegativeRevenueChargesOnJob.SetTemporaryValue(forbiddenCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var job = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 1m, testObjectCreator.Agent, 1m);
				var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("10001", testObjectCreator.AUD, 1M, testObjectCreator.ABIGAS);
				var invoiceLine = testObjectCreator.CreateARInvoiceLine(invoice, job, testObjectCreator.CC1, testObjectCreator.AUD, 1m, "Description", 999m);
				var invoiceCharge = testObjectCreator.CreateJobCharge(invoiceLine, job, testObjectCreator.CC1);
				invoice.Factory.Save();

				AssertEquals(true, invoiceCharge.IsInDatabase);
				AssertEquals(true, invoiceCharge.IsRevenuePosted);

				var amendment = (invoice as IAmending)?.GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;
				if (!amendment.IsInDatabase && amendment != null && amendment.IsAmendingTransaction)
				{
					Assert(amendment.Lines.Count > 0);
					AssertEquals(invoice.Lines.Count, amendment.Lines.Count);

					foreach (var originalAndAmendedLine in invoice.Lines.OfType<InvoicingLineBase>().Zip(amendment.Lines.OfType<InvoicingLineBase>(), (originalLine, amendmentLine) => new { originalLine, amendmentLine }))
					{
						var amendedCharge = job.Charges.AddNew();
						amendedCharge.JR_AL_ARLine = originalAndAmendedLine.amendmentLine.PK;
						amendedCharge.SetAmountsFromLinkedLinesForTests();

						AssertOnSavingCheck(amendedCharge, new TestCaseDefinition_ForSeparateTestsMethods(
							"TestNegativeRevenueChargesOnJobWhenNotPermitted",
							false,
							CriticalValidationErrorType.JobChargeNegativeRevenueIsNotPermitted,
							CriticalValidationMessageTemplate.JobChargeNegativeRevenueIsNotPermittedErrorMessage));
					}
				}
			}
		}

		[TestDate(2021, 5, 10)]
		public void TestChargeTaxDateBeEmptyBeforeLinkWithConsolCost()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var shipment1 = TestObjectCreator.CreateShipment("S001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			consolCost.E6_AT_TaxRate = TestObjectCreator.FREEVAT.PK;
			consolCost.E6_LocalCostAmount = 100m;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.Code;
			consolCost.E6_TaxDate = ZDate.Today;
			Factory.Save();

			var shipment2 = TestObjectCreator.CreateShipment("S002");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var charge = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, "Desc", null, 100m, TestObjectCreator.AALSHI, "123456", null, 100m, TestObjectCreator.Debtor);
			charge.JR_AT_CostGSTRate = TestObjectCreator.FREEVAT.PK;
			charge.JR_CostTaxDate = ZDate.Empty;
			job2.Charges.Add(charge);
			Factory.Save();

			charge.JR_E6 = consolCost.PK;
			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods(
							"JR_CostTaxDate doesn't match with E6_TaxDate will fail critical validation",
							true,
							CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
							CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_JobChargeErrorMessage(string.Empty)));
		}

		[TestDate(2021, 5, 10)]
		public void TestChargeTaxDateChangedToEmptyAfterPost()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			consolCost.E6_AT_TaxRate = TestObjectCreator.FREEVAT.PK;
			consolCost.E6_LocalCostAmount = 100m;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.Code;
			Factory.Save();

			var charge = consolCost.ApportionmentCharges[0];

			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("I001", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			transaction.Lines[0].AL_AT = TestObjectCreator.FREEVAT.PK;
			charge.ReverseAccrual(ZDate.Today);
			charge.JR_AL_APLine = transaction.Lines[0].PK;
			consolCost.E6_AH_APInvoice = transaction.PK;
			consolCost.E6_TaxDate = ZDate.Today;
			Factory.Save();

			AssertEquals("Pre-condition", consolCost.E6_AT_TaxRate, charge.JR_AT_CostGSTRate);
			AssertEquals(ZDate.Today, consolCost.E6_TaxDate);
			AssertNotNull(charge.JR_AT_CostGSTRate);
			AssertEquals(ZDate.Today, charge.JR_CostTaxDate);
			AssertEquals(true, charge.IsCostPosted);

			var newFactory = Factory.CreateNewFactory();
			var reloadedCharge = newFactory.Load<Charge>(charge.PK);
			reloadedCharge.JR_CostTaxDate = ZDate.Empty;

			AssertOnSavingCheck(reloadedCharge, new TestCaseDefinition_ForSeparateTestsMethods(
							"JR_CostTaxDate doesn't match with E6_TaxDate will fail critical validation",
							true,
							CriticalValidationErrorType.JobChargeInvoiceDetailsNotEqualConsolCostOnes_8,
							CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_JobChargeErrorMessage(string.Empty)));
		}

		[TestDate(2021, 5, 10)]
		public void TestChargeTaxDateBeEmptyWhenNotPosted()
		{
			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT);
			consolCost.E6_AT_TaxRate = TestObjectCreator.FREEVAT.PK;
			consolCost.E6_LocalCostAmount = 100m;
			consolCost.E6_RX_NKCurrency = TestObjectCreator.AUD.Code;
			consolCost.E6_TaxDate = ZDate.Empty;
			Factory.Save();

			var charge = consolCost.ApportionmentCharges[0];

			AssertEquals("Pre-condition", consolCost.E6_AT_TaxRate, charge.JR_AT_CostGSTRate);
			AssertEquals(ZDate.Empty, consolCost.E6_TaxDate);
			AssertNotNull(charge.JR_AT_CostGSTRate);
			AssertEquals(ZDate.Empty, charge.JR_CostTaxDate);
			AssertEquals(false, charge.IsCostPosted);

			consolCost.E6_TaxDate = ZDate.Today;
			charge.JR_CostTaxDate = ZDate.Empty;

			AssertEquals("There is ErrorReportOnce to moniter such changes", "ApportionedChargeWithMismatched_TaxDate_1", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			AssertOnSavingCheck(charge, new TestCaseDefinition_ForSeparateTestsMethods("Charge TaxDate be empty will NOT fail critical validation when not posted"));
		}

		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			Assert(true);

			return new List<TestCaseDefinitionWithDelegate_Obsolete>();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
