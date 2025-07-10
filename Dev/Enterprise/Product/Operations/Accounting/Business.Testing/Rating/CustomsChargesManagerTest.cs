using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.Testing
{
	public class CustomsChargesManagerTest : RatingTestCase
	{
		CustomsCharge DeferredCharge2;
		CustomsCharge PaidByBrokerCharge2;
		CustomsCharge DeferredCharge1;
		CustomsCharge PaidByBrokerCharge1;
		CustomsChargesManager CustomsChargesManager;
		Mock<ICustomsCharges> CustomCharge1;
		OrgHeader TestCreditor;
		AccChargeCode ChargeCodeCusDeferred;
		AccChargeCode ChargeCodeCusDisbursement;
		TestObjectCreator TestObjectCreator;

		public void TestCustomsChargeDescriptionForSIMAAmount()
		{
			SetupCharges();
			var charge = new CustomsCharge(null, "Total SIMA Amount", 11700.06, 0, true, TestCreditor.PK);
			charge.AdditionalChargeInfos.AddRange([new CustomsCharge.ChargeInfo("Surtax", 2500),
				new CustomsCharge.ChargeInfo("Anti Dumping", 9190),
				new CustomsCharge.ChargeInfo("Countervailing", 10.06)]);
			var provider = new CustomsChargesProvider(new[] { charge });
			var results = CustomsChargesManager.RateCustomsCharges(new ICustomsCharges[] { provider });
			AssertEquals(1, results.Count);
			AssertEquals(@"  Total SIMA Amount                       11700.06
  Surtax 2500.00
  Anti Dumping 9190.00
  Countervailing 10.06", results[0].AdditionalInvoiceLineDescription);
		}

		public void TestUpdateExistingCustomsChargeForNoRate()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = "NAUTILUS";
			declaration.JE_VoyageFlightNo = "007";
			declaration.CustomsEntryHeaders.AddNew();
			var testJob = new Job.Loader(declaration).TryLoadOrCreateWithoutMutexForTestOnly();
			testJob.Parent = declaration;
			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(Helper.ChargeCodes["FRT"], GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200m);
			rate1.InvoiceLineDescription = "rate2";
			var chargeLine = testJob.Charges.AddNew();
			chargeLine.JR_AC = Helper.ChargeCodes["CUSDSB"].PK;
			chargeLine.JR_OSCostAmt = 100m;
			chargeLine.JR_OSSellAmt = 100m;
			chargeLine.JR_EstimatedCost = 100m;
			Assert(chargeLine.IsCustomsCharge);
			Factory.Save();

			var customsChargesManager = new CustomsChargesManager(declaration);
			customsChargesManager.DeleteCustomsChargesIfNecessary(testJob, rates);

			AssertEquals(0, testJob.Charges.Count);
			Assert(chargeLine.IsDeleted);
		}

		public void TestDoNotUpdateExistingCustomsChargeForNoRate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var testJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var rates = new AutoRateInfoCollection(Factory);
			var rate1 = rates.AddNew(Helper.ChargeCodes["FRT"], GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 200);
			rate1.InvoiceLineDescription = "rate2";
			var chargeLine = testJob.Charges.AddNew();
			chargeLine.JR_AC = Helper.ChargeCodes["CUSDSB"].PK;
			chargeLine.JR_OSCostAmt = 100m;
			chargeLine.JR_OSSellAmt = 100m;
			Assert(chargeLine.IsCustomsCharge);
			var customsChargesManager = new CustomsChargesManager(shipment);
			customsChargesManager.DeleteCustomsChargesIfNecessary(testJob, rates);

			AssertEquals(1, testJob.Charges.Count);
			AssertEquals("No customs job exists so should not clear out", 100m, chargeLine.JR_OSSellAmt);
			AssertEquals("No customs job exists so should not clear out", 100m, chargeLine.JR_OSCostAmt);
		}

		public void TestDeleteCustomsChargesIfNecessary_DeleteChargesFromTheCurrentJobOnly()
		{
			// This test is a big ugly as it is based on some assumptions is should not know about.
			// A proper test would be some integration test, but it would be too heavy and complex.
			// Not easy to test CMR messaging with autorating due to the current implementation.
			//
			// It is a BCN case. A BCN lead shipment contains charges from the main shipment and sub shipments.
			// When the lead shipment gets autorated, it should delete only lead customs charges if they are matched.
			//
			// Not sure if deleting charges is a good idea at all, but it was implemented 10+ years ago to workaround some customs case.
			// But, it didn't consider a case like BCN shipments which was added recently. So, I fixed that and added this test.

			var leadShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			leadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			leadShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_VesselName = "NAUTILUS";
			declaration.JE_VoyageFlightNo = "007";
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_JS = leadShipment.PK;

			var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment.JS_PackingMode = Constants.ContainerModes.BuyersConsol;

			var subJob = new Job.Loader(subShipment).TryLoadOrCreate();
			var subCharge = subJob.Charges.AddNew();
			subCharge.JR_AC = Helper.ChargeCodes["CUSDSB"].PK;
			subCharge.JR_OSCostAmt = 100m;
			subCharge.JR_OSSellAmt = 100m;

			var leadJob = new Job.Loader(leadShipment).TryLoadOrCreate();
			var leadCharge = leadJob.Charges.AddNew();
			leadCharge.JR_AC = Helper.ChargeCodes["CUSDSB"].PK;
			leadCharge.JR_OSCostAmt = 200m;
			leadCharge.JR_OSSellAmt = 200m;

			// It is hard to force charges collection to load additional jobs charges (charges from sub shipments) in tests (because of implementation), so,
			// this is an ugly workaround
			leadJob.Charges.Add(subCharge);

			AssertEquals("PRECONDITION: 2 charges must be loaded - one from the lead shipment and another one from the sub shipment", 2, leadJob.Charges.Count);

			var customsChargesManager = new CustomsChargesManager(leadShipment);

			// No charges has been found during autorating (empty AutoRateInfoCollection collection). So, none of customs charges should be matched, but only the ones
			// from the lead shipment must be deleted.
			customsChargesManager.DeleteCustomsChargesIfNecessary(leadJob, new AutoRateInfoCollection(Factory));

			AssertEquals("Should delete only lead shipment charge as lead job was autorated. Sub shipment charge should remain untouched.", 1, leadJob.Charges.Count);
			AssertEquals("Sub shipment charge left", 100m, leadJob.Charges[0].JR_OSCostAmt);
		}

		public void TestEmptyCustomsCharge()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(Array.Empty<CustomsCharge>());
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, false);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(0, results.Count);
		}

		public void TestRateCustomsChargesWithDeferredChargeDisabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { PaidByBrokerCharge1, DeferredCharge1, PaidByBrokerCharge2, DeferredCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, false);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(1, results.Count);
			ZString dSBCharge1Description = FormatForTest("Test1", 100) + System.Environment.NewLine;
			dSBCharge1Description += FormatForTest("Test3", 200); // +System.Environment.NewLine;
			AssertEquals(dSBCharge1Description, results[0].AdditionalInvoiceLineDescription);
			AssertEquals(300m, results[0].Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, results[0].ChargeCode.PK);
		}

		public void TestRateCustomsChargesWithDeferredChargeEnabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { PaidByBrokerCharge1, DeferredCharge1, PaidByBrokerCharge2, DeferredCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, true);

			var results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(2, results.Count);

			var dSBChargeDescription = @"  Test1                                     100.00
  Test3                                     200.00";
			var result1 = results[0];
			AssertEquals(dSBChargeDescription, result1.AdditionalInvoiceLineDescription);
			AssertEquals(300m, result1.Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, result1.ChargeCode.PK);
			AssertEquals("Base Rate NZD 100.00", result1.Bases[0].ToString());
			AssertEquals("Base Rate NZD 200.00", result1.Bases[1].ToString());

			var deferredChargeDescription = @"  Test2                                     150.00
  Test4                                     250.00";

			var result2 = results[1];
			AssertEquals(deferredChargeDescription, result2.AdditionalInvoiceLineDescription);
			AssertEquals(0m, result2.Amount);
			AssertEquals(ChargeCodeCusDeferred.PK, result2.ChargeCode.PK);
			AssertEquals("Base Rate NZD 0.00", result2.Bases[0].ToString());
			AssertEquals("Base Rate NZD 0.00", result2.Bases[1].ToString());
		}

		public void TestRateCustomsChargesWithHideValueBreakdownEnabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { PaidByBrokerCharge1, DeferredCharge1, PaidByBrokerCharge2, DeferredCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, true);
			RatingDataRegistry.Instance.HideValueBreakdownInDescription.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(2, results.Count);
			AssertEquals("This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.\r\n", results[0].Description);
			AssertEquals(300m, results[0].Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, results[0].ChargeCode.PK);
			AssertContains("(Total: 400.00)", results[1].AdditionalInvoiceLineDescription);
			AssertEquals(0m, results[1].Amount);
			AssertEquals(ChargeCodeCusDeferred.PK, results[1].ChargeCode.PK);
		}

		public void TestRateCustomsChargesWithMultipleDebtors()
		{
			SetupCharges();
			var debtor1 = Factory.New<OrgHeader>();
			debtor1.FillWithValidTestData();
			var debtor2 = Factory.New<OrgHeader>();
			debtor2.FillWithValidTestData();
			PaidByBrokerCharge1.DebtorPK = debtor1.PK;
			DeferredCharge1.DebtorPK = debtor1.PK;
			PaidByBrokerCharge2.DebtorPK = debtor2.PK;
			DeferredCharge2.DebtorPK = debtor2.PK;
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { PaidByBrokerCharge1, DeferredCharge1, PaidByBrokerCharge2, DeferredCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, true);
			RatingDataRegistry.Instance.HideValueBreakdownInDescription.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(4, results.Count);
			AssertEquals("This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.\r\n", results[0].Description);
			AssertEquals(100m, results[0].Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, results[0].ChargeCode.PK);
			AssertEquals(debtor1.PK, results[0].DebtorOverridePK);
			AssertContains("(Total: 150.00)", results[1].AdditionalInvoiceLineDescription);
			AssertEquals(0m, results[1].Amount);
			AssertEquals(ChargeCodeCusDeferred.PK, results[1].ChargeCode.PK);
			AssertEquals(debtor1.PK, results[1].DebtorOverridePK);
			AssertEquals("This Customs Disbursement Charge has been calculated based on financial details specified in the Customs Response message.\r\n", results[2].Description);
			AssertEquals(200m, results[2].Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, results[2].ChargeCode.PK);
			AssertEquals(debtor2.PK, results[2].DebtorOverridePK);
			AssertContains("(Total: 250.00)", results[3].AdditionalInvoiceLineDescription);
			AssertEquals(0m, results[3].Amount);
			AssertEquals(ChargeCodeCusDeferred.PK, results[3].ChargeCode.PK);
			AssertEquals(debtor2.PK, results[2].DebtorOverridePK);
		}

		public void TestCustomsChargesWithNoDeferredChargeAndRegistryDisabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { PaidByBrokerCharge1, PaidByBrokerCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, false);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(1, results.Count);
			ZString dSBChargeDescription = "  " + "Test1".PadRight(35) + " " + new ZDecimal(100).ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals).PadLeft(12) + System.Environment.NewLine + "  " + "Test3".PadRight(35) + " " + new ZDecimal(200).ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals).PadLeft(12);
			AssertEquals(dSBChargeDescription, results[0].AdditionalInvoiceLineDescription);
			AssertEquals(300m, results[0].Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, results[0].ChargeCode.PK);
		}

		public void TestCustomsChargesWithNoDeferredChargeAndRegistryEnabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { PaidByBrokerCharge1, PaidByBrokerCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, true);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(1, results.Count);
			ZString dSBChargeDescription = "  " + "Test1".PadRight(35) + " " + new ZDecimal(100).ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals).PadLeft(12) + System.Environment.NewLine + "  " + "Test3".PadRight(35) + " " + new ZDecimal(200).ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals).PadLeft(12);
			AssertEquals(dSBChargeDescription, results[0].AdditionalInvoiceLineDescription);
			AssertEquals(300m, results[0].Amount);
			AssertEquals(ChargeCodeCusDisbursement.PK, results[0].ChargeCode.PK);
		}

		public void TestCustomsChargesWithOnlyDeferredChargeAndRegistryEnabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { DeferredCharge1, DeferredCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, true);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(1, results.Count);
			ZString deferredChargeDescription = FormatForTest("Test2", 150) + System.Environment.NewLine;
			deferredChargeDescription += FormatForTest("Test4", 250);
			AssertEquals(deferredChargeDescription, results[0].AdditionalInvoiceLineDescription);
			AssertEquals(0m, results[0].Amount);
			AssertEquals(ChargeCodeCusDeferred.PK, results[0].ChargeCode.PK);
		}

		public void TestCustomsChargesWithOnlyDeferredChargeAndRegistryDisabled()
		{
			SetupCharges();
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(new[] { DeferredCharge1, DeferredCharge2 });
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, false);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new[] { CustomCharge1.Object });
			AssertEquals(0, results.Count);
		}

		public void TestPaidByBrokerWithNoNewSetups()
		{
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(true, true, null, null, null);
			AssertEquals(1, results.Count);
			string expectedAdditionalDescription = @"  Duty                                       55.65
  GST                                        23.47
  Entry Fee                                  19.56
  + GST                                       2.44
  ALAC Levy                                   8.93";
			AssertRateInfo(results[0], ChargeCodeCusDisbursement, ChargeCodeCusDisbursement.AC_Desc, expectedAdditionalDescription, 107.61m, 2.44m);
		}

		public void TestNotPaidByBrokerWithNoNewSetups()
		{
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(false, true, null, null, null);
			AssertEquals(1, results.Count);
			string expectedAdditionalDescription = @"  Duty                                       55.65
  GST                                        23.47
  Entry Fee                                  19.56
  + GST                                       2.44
  ALAC Levy                                   8.93";
			AssertRateInfo(results[0], ChargeCodeCusDeferred, ChargeCodeCusDeferred.AC_Desc, expectedAdditionalDescription, 0.00m, 0.00m);
		}

		public void TestPaidByBrokerWithNewSetups()
		{
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("ZZ~DUTY", "Customs Duty", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeGST = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeEntryFee = GetNewAccChargeCode("ZZ~ENTFEE", "Customs Entry Fee", Constants.ChargeType.Disbursement);
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(true, true, chargeCodeDuty, chargeCodeGST, chargeCodeEntryFee);
			AssertEquals(4, results.Count);
			AssertRateInfo(results[0], chargeCodeDuty, chargeCodeDuty.AC_Desc, "", 55.65m, 0.00m);
			AssertRateInfo(results[1], chargeCodeGST, chargeCodeGST.AC_Desc, "", 23.47m, 0.00m);
			AssertRateInfo(results[2], chargeCodeEntryFee, chargeCodeEntryFee.AC_Desc, "", 19.56m, 2.44m);
			string expectedAdditionalDescription = @"  ALAC Levy                                   8.93";
			AssertRateInfo(results[3], ChargeCodeCusDisbursement, ChargeCodeCusDisbursement.AC_Desc, expectedAdditionalDescription, 8.93m, 0.00m);
		}

		public void TestNotPaidByBrokerWithNewSetups()
		{
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("ZZ~DUTY", "Customs Duty", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeGST = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeEntryFee = GetNewAccChargeCode("ZZ~ENTFEE", "Customs Entry Fee", Constants.ChargeType.Disbursement);
			SetupCharges();
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(false, true, chargeCodeDuty, chargeCodeGST, chargeCodeEntryFee);
			AssertEquals("Results.Count", 1, results.Count);
			string expectedAdditionalDescription = @"  Duty                                       55.65
  GST                                        23.47
  Entry Fee                                  19.56
  + GST                                       2.44
  ALAC Levy                                   8.93";
			AssertRateInfo(results[0], ChargeCodeCusDeferred, ChargeCodeCusDeferred.AC_Desc, expectedAdditionalDescription, 0.00m, 0.00m);
		}

		public void TestPaidByBrokerWithNewSetupsWithDutyAndGSTAmalgamatedOnOneChargeCode()
		{
			AccChargeCode chargeCodeDutyAndGST = GetNewAccChargeCode("ZZ~DUTYGST", "Customs Duty and GST", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeEntryFee = GetNewAccChargeCode("ZZ~ENTFEE", "Customs Entry Fee", Constants.ChargeType.Disbursement);
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(true, true, chargeCodeDutyAndGST, chargeCodeDutyAndGST, chargeCodeEntryFee);
			AssertEquals(3, results.Count);
			string expectedAdditionalDescription = @"  Duty                                       55.65
  GST                                        23.47";
			AssertRateInfo(results[0], chargeCodeDutyAndGST, chargeCodeDutyAndGST.AC_Desc, expectedAdditionalDescription, 79.12m, 0.00m);
			AssertRateInfo(results[1], chargeCodeEntryFee, chargeCodeEntryFee.AC_Desc, "", 19.56m, 2.44m);
			expectedAdditionalDescription = @"  ALAC Levy                                   8.93";
			AssertRateInfo(results[2], ChargeCodeCusDisbursement, ChargeCodeCusDisbursement.AC_Desc, expectedAdditionalDescription, 8.93m, 0.00m);
		}

		public void TestNotPaidByBrokerWithNewSetupsAndIncludeCustomsDeferredChargeInInvoicingTurnedOff()
		{
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("ZZ~DUTY", "Customs Duty", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeGST = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeEntryFee = GetNewAccChargeCode("ZZ~ENTFEE", "Customs Entry Fee", Constants.ChargeType.Disbursement);
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(false, false, chargeCodeDuty, chargeCodeGST, chargeCodeEntryFee);
			AssertEquals("Results.Count", 0, results.Count);
		}

		public void TestPaidByBrokerWithNewSetupsAndIncludeCustomsDeferredChargeInInvoicingTurnedOff()
		{
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("ZZ~DUTY", "Customs Duty", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeGST = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeEntryFee = GetNewAccChargeCode("ZZ~ENTFEE", "Customs Entry Fee", Constants.ChargeType.Disbursement);
			AutoRateInfoCollection results = GetAutoRatingInfosUsing(true, false, chargeCodeDuty, chargeCodeGST, chargeCodeEntryFee);
			AssertEquals(4, results.Count);
			AssertRateInfo(results[0], chargeCodeDuty, chargeCodeDuty.AC_Desc, "", 55.65m, 0.00m);
			AssertRateInfo(results[1], chargeCodeGST, chargeCodeGST.AC_Desc, "", 23.47m, 0.00m);
			AssertRateInfo(results[2], chargeCodeEntryFee, chargeCodeEntryFee.AC_Desc, "", 19.56m, 2.44m);
			string expectedAdditionalDescription = @"  ALAC Levy                                   8.93";
			AssertRateInfo(results[3], ChargeCodeCusDisbursement, ChargeCodeCusDisbursement.AC_Desc, expectedAdditionalDescription, 8.93m, 0.00m);
		}

		public void TestGetNewRateInfo_InvoiceNumberUpdated()
		{
			var testCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("CUSDSB", "Customs Duty", Constants.ChargeType.Disbursement);
			CustomsCharge chargeDuty = new CustomsCharge(chargeCodeDuty, "Duty", 55.65m, 0m, true, testCreditor.PK);
			chargeDuty.EntryReference = "ATC400000400520225875";
			CustomsCharge[] customsCharges = new CustomsCharge[] { chargeDuty };
			var customCharge = new Mock<ICustomsCharges>();
			customCharge.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(customsCharges);
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var customsChargesManager = new CustomsChargesManager(consumer);
			AutoRateInfoCollection results = customsChargesManager.RateCustomsCharges(new ICustomsCharges[] { customCharge.Object });
		
			AssertEquals("info.InvoiceNumber", "ATC400000400520225875", results[0].InvoiceNumber);
		}

		public void TestCustomsCharges_UniqueAcrossEntries()
		{
			SetupCharges();
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, false);
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("ZZ~DUTY", "Customs Duty", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeGST = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			CustomsCharge chargeDuty = new CustomsCharge(chargeCodeDuty, "Duty", 55.65m, 0m, true, TestCreditor.PK);
			chargeDuty.EntryReference = "Entry1";
			CustomsCharge chargeGST = new CustomsCharge(chargeCodeGST, "GST", 23.47m, 0m, true, TestCreditor.PK);
			chargeGST.EntryReference = "Entry1";
			CustomsCharge chargeGST2 = new CustomsCharge(chargeCodeGST, "GST", 23.47m, 0m, true, TestCreditor.PK);
			chargeGST2.EntryReference = "Entry2";
			CustomsCharge[] customsCharges = new CustomsCharge[] { chargeDuty, chargeGST, chargeGST2 };
			CustomsChargesProvider provider = new CustomsChargesProvider(customsCharges);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new ICustomsCharges[] { provider });
			AssertEquals(3, results.Count);
			AssertEquals("Customs Duty - Entry1", results[0].InvoiceLineDescription);
			AssertEquals("Customs GST - Entry1", results[1].InvoiceLineDescription);
			AssertEquals("Customs GST - Entry2", results[2].InvoiceLineDescription);
		}

		public void TestCustomsCharges_NotUniqueAcrossEntries()
		{
			SetupCharges();
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, false);
			AccChargeCode chargeCodeDuty = GetNewAccChargeCode("ZZ~DUTY", "Customs Duty", Constants.ChargeType.Disbursement);
			AccChargeCode chargeCodeGST = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			CustomsCharge chargeDuty = new CustomsCharge(chargeCodeDuty, "Duty", 55.65m, 0m, true, TestCreditor.PK);
			CustomsCharge chargeGST = new CustomsCharge(chargeCodeGST, "GST", 23.47m, 0m, true, TestCreditor.PK);
			CustomsCharge chargeGST2 = new CustomsCharge(chargeCodeGST, "GST", 23.47m, 0m, true, TestCreditor.PK);
			CustomsCharge[] customsCharges = new CustomsCharge[] { chargeDuty, chargeGST, chargeGST2 };
			CustomsChargesProvider provider = new CustomsChargesProvider(customsCharges);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new ICustomsCharges[] { provider });
			AssertEquals(2, results.Count);
			AssertEquals("Customs Duty", results[0].InvoiceLineDescription);
			AssertEquals("Customs GST", results[1].InvoiceLineDescription);
		}

		public void TestRaiseInvoices_CustomsDisbursementChargeCodeIsMissing_ShouldThrowCustomsInvoiceRaiseException()
		{
			TestCreditor = TestObjectCreator.ABIGAS;
			var cusCharge = new CustomsCharge(null, "Customs disbursement charge", 100, 0, true, TestCreditor.PK);
			var customsCharges = new[] { cusCharge };
			var provider = new CustomsChargesProvider(customsCharges);

			var mockedMissingChargeCodePK = ZGuid.NewZGuid();

			using (RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, TestCreditor.PK.ToGuid()))
			using (RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, mockedMissingChargeCodePK.ToGuid()))
			{
				try
				{
					var consumer = Factory.New<ForwardingShipment>();
					CustomsChargesManager = new CustomsChargesManager(consumer);
					CustomsChargesManager.RateCustomsCharges(new ICustomsCharges[] { provider });
					Fail("Should throw CustomsInvoiceRaiseException in RateCustomsCharges");
				}
				catch (CustomsInvoiceRaiseException ex)
				{
					var message = @"You must set up the 'Customs Disbursement Charge Code' in the registry before Autorating can be run.";
					AssertContains("RateCustomsCharges should throw CustomsInvoiceRaiseException when charge code does not exist", message, ex.Message);
				}
			}
		}

		public void TestRaiseInvoices_IncludeCustomDeferredChargeInInvoicing_CustomDeferredChargeCodeIsMissing_ShouldThrowCustomsInvoiceRaiseException()
		{
			TestCreditor = TestObjectCreator.ABIGAS;
			var cusCharge = new CustomsCharge(null, "Customs deferred charge", 100, 0, false, TestCreditor.PK);
			var customsCharges = new[] { cusCharge };
			var provider = new CustomsChargesProvider(customsCharges);

			var mockedMissingChargeCodePK = ZGuid.NewZGuid();

			using (RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (RatingDataRegistry.Instance.CustomDeferredChargeCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, mockedMissingChargeCodePK.ToGuid()))
			{
				try
				{
					var consumer = Factory.New<ForwardingShipment>();
					CustomsChargesManager = new CustomsChargesManager(consumer);
					CustomsChargesManager.RateCustomsCharges(new ICustomsCharges[] { provider });
					Fail("Should throw CustomsInvoiceRaiseException in RateCustomsCharges");
				}
				catch (CustomsInvoiceRaiseException ex)
				{
					var message = @"You have enabled 'Include Custom Deferred Charge in Invoicing' in the registry, but the corresponding 'Custom Deferred Charge' is not set up in the registry.
Please set up 'Custom Deferred Charge' or disable 'Include Custom Deferred Charge in Invoicing' in the registry.";
					AssertContains("Raise Invoices should throw CustomsInvoiceRaiseException when charge code does not exist", message, ex.Message);
				}
			}
		}

		public void TestRateCustomsCharges_PopulateOrderReference()
		{
			var chargeCode = GetNewAccChargeCode("ZZ~GST", "Customs GST", Constants.ChargeType.Disbursement);
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var charge = new CustomsCharge(chargeCode, "Customs deferred charge", 100, 0, true, creditor.PK);
			var provider = new CustomsChargesProvider(new [] { charge });

			var consumer = Factory.New<ForwardingShipment>();
			consumer.JS_UniqueConsignRef = "McLaren";
			var manager = new CustomsChargesManager(consumer);

			var charges = manager.RateCustomsCharges(new ICustomsCharges[] { provider });
			AssertEquals("McLaren", charges.Single().JobRef);
		}

		void SetupCharges()
		{
			ChargeCodeCusDisbursement = GetFirstDisbursementChargeCode();
			ChargeCodeCusDeferred = Factory.NewWithValidTestData<AccChargeCode>();
			ChargeCodeCusDeferred.AC_ChargeType = Constants.ChargeType.Comment;
			ChargeCodeCusDeferred.AC_Code = "ZZ-CUSDEF";
			TestCreditor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			CustomCharge1 = new Mock<ICustomsCharges>();
			var consumer = (BusinessObject)Factory.New<Enterprise.Integration.Freight.ICommonShipment>();
			CustomsChargesManager = new CustomsChargesManager(consumer);
			PaidByBrokerCharge1 = new CustomsCharge(null, "Test1", 100, 0, true, TestCreditor.PK);
			DeferredCharge1 = new CustomsCharge(null, "Test2", 150, 0, false, TestCreditor.PK);
			PaidByBrokerCharge2 = new CustomsCharge(null, "Test3", 200, 0, true, TestCreditor.PK);
			DeferredCharge2 = new CustomsCharge(null, "Test4", 250, 0, false, TestCreditor.PK);
		}

		AccChargeCode GetFirstDisbursementChargeCode()
		{
			var chargeFilter = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_AG_CostAccount, SQLComparisonOperator.NotEqual, null);
			chargeFilter.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.Disbursement);
			return Factory.LoadTop1(typeof(AccChargeCode), chargeFilter) as AccChargeCode;
		}

		void SetCustomsDisbursementDetailsInRegistry(OrgHeader testCreditor, AccChargeCode disbursementChargeCode, AccChargeCode customsDeferred, bool includeCustomDeferredChargeInInvoicing)
		{
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, disbursementChargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsDeferred.PK.ToGuid());
			RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, includeCustomDeferredChargeInInvoicing);
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.NewZealand);
			AssertEquals("Precondition: Registry.CustomsDisbursementCreditor", testCreditor.PK, RatingDataRegistry.Instance.CustomsDisbursementCreditor.Value);
			AssertEquals("Precondition: Registry.CustomsDisbursementChargeCodeCustomsDisbursementChargeCode", disbursementChargeCode.PK, RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
			AssertEquals("Precondition: Registry.CustomDeferredChargeCode", customsDeferred.PK, RatingDataRegistry.Instance.CustomDeferredChargeCode.Value);
			AssertEquals("Precondition: Registry.IncludeCustomDeferredChargeInInvoicing", includeCustomDeferredChargeInInvoicing, RatingDataRegistry.Instance.IncludeCustomDeferredChargeInInvoicing.Value);
		}

		AccChargeCode GetNewAccChargeCode(ZString code, ZString description, ZString type)
		{
			AccChargeCode result = Factory.New<AccChargeCode>();
			result.AC_Code = code;
			result.AC_Desc = description;
			result.AC_ChargeType = type;
			return result;
		}

		void AssertRateInfo(AutoRateInfo info, AccChargeCode chargeCode, string description, string additionalDescription, decimal exGSTAmount, decimal gstAmount)
		{
			AssertEquals("info.ChargeCode", chargeCode.AC_Code, info.ChargeCode.AC_Code);
			AssertEquals("info.InvoiceLineDesc", description, info.InvoiceLineDescription);
			AssertEquals("info.AdditionalInvoiceLineDescription", additionalDescription, info.AdditionalInvoiceLineDescription);
			AssertEquals("info.Amount", exGSTAmount, info.Amount);
			AssertEquals("info.AgentAmount", exGSTAmount, info.AgentAmount);
			AssertEquals("info.OverriddenCostGSTAmount", gstAmount, info.OverriddenGSTAmount);
		}

		ZString FormatForTest(ZString description, ZDecimal amount)
		{
			return "  " + description.PadRight(35) + " " + amount.ToString(GlbCompany.CurrentCompany.LocalCurrency.Decimals).PadLeft(12);
		}

		AutoRateInfoCollection GetAutoRatingInfosUsing(bool isPaidByBroker, bool includeCustomsDeferredChargeInInvoicing, AccChargeCode dutyChargeCode, AccChargeCode gSTChargeCode, AccChargeCode entryFeeChargeCode)
		{
			SetupCharges();
			SetCustomsDisbursementDetailsInRegistry(TestCreditor, ChargeCodeCusDisbursement, ChargeCodeCusDeferred, includeCustomsDeferredChargeInInvoicing);
			CustomsCharge chargeDuty = new CustomsCharge(dutyChargeCode, "Duty", 55.65m, 0m, isPaidByBroker, TestCreditor.PK);
			CustomsCharge chargeGST = new CustomsCharge(gSTChargeCode, "GST", 23.47m, 0m, isPaidByBroker, TestCreditor.PK);
			CustomsCharge chargeEntryFee = new CustomsCharge(entryFeeChargeCode, "Entry Fee", 19.56m, 2.44m, isPaidByBroker, TestCreditor.PK);
			CustomsCharge chargeALAC = new CustomsCharge(null, "ALAC Levy", 8.93m, 0m, isPaidByBroker, TestCreditor.PK);
			CustomsCharge[] customsCharges = new CustomsCharge[] { chargeDuty, chargeGST, chargeEntryFee, chargeALAC };
			CustomCharge1.Setup(m => m.GetCustomsCharges(It.IsAny<ILogger>())).Returns(customsCharges);
			AutoRateInfoCollection results = CustomsChargesManager.RateCustomsCharges(new ICustomsCharges[] { CustomCharge1.Object });
			return results;
		}

		class CustomsChargesProvider : ICustomsCharges
		{
			public CustomsChargesProvider(CustomsCharge[] customsCharges)
			{
				this.CustomsChargesForTest = customsCharges;
			}

			CustomsCharge[] ICustomsCharges.GetCustomsCharges(ILogger logger)
			{
				return CustomsChargesForTest;
			}

			readonly CustomsCharge[] CustomsChargesForTest;

			ZBool ICustomsCharges.IsActive => true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}
