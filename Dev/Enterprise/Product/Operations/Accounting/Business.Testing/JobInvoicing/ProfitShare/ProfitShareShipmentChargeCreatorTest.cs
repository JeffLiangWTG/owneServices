using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ProfitShareShipmentChargeCreatorTest : TestCaseWithFactory
	{
		public void TestDefaultDependencyTypes()
		{
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			AssertType<AdjustPostedInvoiceHelper>(creator.AdjustPostedInvoiceHelper_ExposedForTestOnly);
			AssertType<ProfitShareShipmentChargePoster>(creator.ChargePoster);
			AssertType<ProfitShareShipmentValidator>(creator.Validator);
		}

		public void TestCreateCharges_SavedInDatabase()
		{
			CreateProfitShareDetails("");
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			AssertEquals(1, Job.Charges.Count);
			Assert(Job.Charges[0].IsInDatabase);
			creator.CreateCharges();
			AssertEquals("Two additional charges are created", 3, Job.Charges.Count);
			AssertEquals("Two additionally created charges are saved", 3, Job.Charges.Where(c => c.IsInDatabase).Count());
		}

		public void TestCreateCharges_NotSavedInDatabase()
		{
			CreateProfitShareDetails("");
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job, false);
			Factory.Save();
			AssertEquals(1, Job.Charges.Count);
			Assert(Job.Charges[0].IsInDatabase);
			creator.CreateCharges();
			AssertEquals("Two additional charges are created", 3, Job.Charges.Count);
			AssertEquals("Two additionally created charges are not saved", 2, Job.Charges.Where(c => !c.IsInDatabase).Count());
		}

		public void TestDuplicateExRatesCreatedByProfitShareAreHandled()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			GlbCompany.CurrentCompany.Factory.Save();
			CreateProfitShareDetails("");
			Agent1.CompanyData.OB_RX_NKARDDefltCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			Agent1.OH_IsDebtor = true;
			Agent2.OH_IsDebtor = true;
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Job.Charges[0].JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			Job.Charges[0].JR_OSSellExRate = 5;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var jobInOtherFactory = otherFactory.Load<Job>(Job.PK);
			jobInOtherFactory.ExchangeRates.AddRate(TestObjectCreator.USD, 3, Agent1.PK, ExchangeRateOrgTypeEnum.Debtor);
			otherFactory.Save();

			var createChargesResult = false;
			AssertNoExceptionThrown("Duplicate Exchange rate created in other factory does not cause unhandled exception", () => createChargesResult = creator.CreateCharges());
			Assert("Should not return true for this if error is encountered and saving cancelled", !createChargesResult);
			Assert("Job and children have no errors, so saving is attempted", !Job.HasErrors);
			AssertEquals("Profit share charges are not saved as there is a handled Exchange rate conflict", 4, Job.Charges.Count(x => !x.IsInDatabase));
		}

		public void TestValidateOrganisationDetails_AP_OrgNotCreditor()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			Agent1.CompanyData.OB_IsCreditor = false;
			Agent2.CompanyData.OB_IsCreditor = false;
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			creator.RunPreCreateValidation();
			AssertEquals("Organization XVBQP68SIYXQ is not a valid creditor", creator.ValidationErrors);
		}

		public void TestValidateOrganisationDetails_AP_OrgIsCreditor()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			Agent1.CompanyData.OB_IsCreditor = true;
			Agent2.CompanyData.OB_IsCreditor = true;
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			creator.RunPreCreateValidation();
			AssertEquals(ZString.Empty, creator.ValidationErrors);
		}

		public void TestValidateOrganisationDetails_AR_OrgNotDebtor()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateProfitShareDetails("");
			Agent1.CompanyData.OB_IsDebtor = false;
			Agent2.CompanyData.OB_IsDebtor = false;
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			creator.RunPreCreateValidation();
			AssertEquals("In order to create profit share charges for RCV party, XVBQP68SIYXQ has to be marked as Receivables", creator.ValidationErrors);
		}

		public void TestValidateOrganisationDetails_AR_OrgIsDebtor()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateProfitShareDetails("");
			Agent1.CompanyData.OB_IsDebtor = true;
			Agent2.CompanyData.OB_IsDebtor = true;
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			creator.RunPreCreateValidation();
			AssertEquals(ZString.Empty, creator.ValidationErrors);
		}

		public void TestCreateCharges_AP()
		{
			CreateProfitShareDetails("");
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			AssertChargeDetails_AP(Job.Charges[1], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 150m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent1.PK, "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00", ZDateTime.Today, "PS JobNumber", true);
			AssertChargeDetails_AP(Job.Charges[2], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 110m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent2.PK, "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00, Controlling Agent - 4.00% of profit of 500.00", ZDateTime.Today, "PS JobNumber", true);
		}

		public void TestCreateCharges_AR()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			GlbCompany.CurrentCompany.Factory.Save();
			CreateProfitShareDetails("");
			Agent1.OH_IsDebtor = true;
			Agent2.OH_IsDebtor = true;
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			AssertChargeDetails_AR(Job.Charges[1], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, -150m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent1.PK, "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00", true);
			AssertChargeDetails_AR(Job.Charges[2], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, -110m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent2.PK, "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00, Controlling Agent - 4.00% of profit of 500.00", true);
		}

		public void TestCreateCharges_AR_DoesNotPostRevenueWithInvalidDebtor()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CreateProfitShareDetails("");

			Agent1.CompanyData.OB_IsDebtor = false;

			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			AssertEquals(1, Job.Charges.Count);

			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			Assert(Job.Charges[1].Notifications.ContainsNotificationContaining("JR_OH_SellAccount: Enter a valid Debtor."));

			Assert(Job.Charges[2].Notifications.ContainsNotificationContaining("JR_OH_SellAccount: Enter a valid Debtor."));
		}

		public void TestCreateCharges_AR_DoesNotShowCriticalValidationError()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CreateProfitShareDetails("");

			Agent1.CompanyData.OB_IsDebtor = false;

			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			AssertEquals(1, Job.Charges.Count);

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			AssertNoExceptionThrown("Should not attempt to save as there is valiation error on business object.", () => creator.CreateCharges());
		}

		public void TestCreateChargesUsingPerPartyChargeCodes()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge1 Description";
			ZGuid receivingAgentChargeCode = chargeCode.PK;

			chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge2 Description";
			ZGuid sendingAgentChargeCode = chargeCode.PK;
			Factory.Save();

			OrgProfitSharePartyLookups lookups = new OrgProfitSharePartyLookups(null);
			ChargeCodeWithTypeCollection collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = sendingAgentChargeCode;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent)].ChargeCode = receivingAgentChargeCode;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			CreateProfitShareDetails("");
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			AssertChargeDetails_AP(Job.Charges[1], receivingAgentChargeCode.ToGuid(), 150m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent1.PK, "Charge1 Description - Receiving Agent - 30.00% of profit of 500.00", ZDateTime.Today, "PS JobNumber", true);
			AssertChargeDetails_AP(Job.Charges[2], sendingAgentChargeCode.ToGuid(), 110m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent2.PK, "Charge2 Description - Sending Agent - 18.00% of profit of 500.00, Controlling Agent - 4.00% of profit of 500.00", ZDateTime.Today, "PS JobNumber", true);
		}

		public void TestCreateChargesWithInvalidProfitShareChargeCodesPerParty()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge1 Description";
			var receivingAgentChargeCode = chargeCode.PK;

			chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Desc = "Charge2 Description";
			var sendingAgentChargeCode = chargeCode.PK;
			Factory.Save();

			chargeCode = TestObjectCreator.CreateChargeCode("TestCode");
			chargeCode.AC_GC = TestObjectCreator.NonCurrentCompany.PK;
			Factory.Save();

			var lookups = new OrgProfitSharePartyLookups(null);
			var registryItem = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty;
			var collection = registryItem.Value;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].UseDefaultProfitShareChargeCode = false;
			collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent)].ChargeCode = chargeCode.PK;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			CreateProfitShareDetails("");
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			AssertExceptionThrown<ZCannotSaveException>("should throw exception", AccountingConstants.ProfitShareErrorMessages.InvalidRegistry(registryItem), () => creator.CreateCharges());
		}

		public void TestCreateCharges_NoPost()
		{
			AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CreateProfitShareDetails("");
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			AssertChargeDetails_AP(Job.Charges[1], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 150m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent1.PK, "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00", ZDateTime.Empty, "", false);
			AssertChargeDetails_AP(Job.Charges[2], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 110m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent2.PK, "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00, Controlling Agent - 4.00% of profit of 500.00", ZDateTime.Empty, "", false);
			Assert("Precondition", !Job.HasErrors);

			creator.CreateCharges();
			AssertEquals("No further charges created", 3, Job.Charges.Count);
		}

		public void TestCreateCharges_PerCurrency()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			objectCreator.CreateExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "INR"), 10M);
			objectCreator.CreateExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "USD"), 5M);

			CreateProfitShareDetails("");
			Job.Charges.RemoveAndDeleteAll();

			Charge existingCharge1 = Job.Charges.AddNew();
			existingCharge1.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge1.JR_IsIncludedInProfitShare = true;
			existingCharge1.JR_RX_NKSellCurrency = "INR";
			existingCharge1.JR_OSSellAmt = 200m;
			existingCharge1.JR_RX_NKCostCurrency = "USD";
			existingCharge1.JR_OSCostAmt = 11m;

			Charge existingCharge2 = Job.Charges.AddNew();
			existingCharge2.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge2.JR_IsIncludedInProfitShare = true;
			existingCharge2.JR_RX_NKSellCurrency = "USD";
			existingCharge2.JR_OSSellAmt = 20m;
			existingCharge2.JR_RX_NKCostCurrency = "INR";
			existingCharge2.JR_OSCostAmt = 500m;

			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();

			creator.CreateCharges();
			AssertEquals(6, Job.Charges.Count);

			var chargesSorted = (from c in Job.Charges.ToArray<Charge>() orderby c.JR_OSCostAmt select c).ToArray();

			CombineAssertions(
				delegate
				{
					AssertChargeDetails_AP(chargesSorted[0], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, -90m, "INR", Agent1.PK, "Profit Share / Rebate - Receiving Agent - 30.00% of profit of -300.00", ZDateTime.Today, "PS JobNumber-INR", false, "charge 1");
					AssertChargeDetails_AP(chargesSorted[1], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, -66m, "INR", Agent2.PK, "Profit Share / Rebate - Sending Agent - 18.00% of profit of -300.00, Controlling Agent - 4.00% of profit of -300.00", ZDateTime.Today, "PS JobNumber-INR", false, "charge 2");
					AssertChargeDetails_AP(chargesSorted[2], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 1.98m, "USD", Agent2.PK, "Profit Share / Rebate - Sending Agent - 18.00% of profit of 9.00, Controlling Agent - 4.00% of profit of 9.00", ZDateTime.Today, "PS JobNumber-USD", false, "charge 3");
					AssertChargeDetails_AP(chargesSorted[3], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 2.7m, "USD", Agent1.PK, "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 9.00", ZDateTime.Today, "PS JobNumber-USD", false, "charge 4");
				});

			Assert("Precondition", !Job.HasErrors);
			creator.CreateCharges();
			AssertEquals("No new charges created if re-run since profit has not changed", 6, Job.Charges.Count);

			creator.CreateCharges();
			AssertEquals("Again, No new charges created if re-run since profit has not changed", 6, Job.Charges.Count);

			//Need to fix this!!!
			//ExistingCharge2.JR_OSCostAmt = 400m;
			//Creator.CreateCharges();
			//int newCount = Job.Charges.Count;
			//Assert("New charges created since profit has changed", newCount > 6);

			//Creator.CreateCharges();
			//AssertEquals("No NEW charges created again", newCount, Job.Charges.Count);
		}

		public void TestCreateCharges_GrossRevenue()
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			AssertChargeDetails_AP(Job.Charges[1], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 180m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent1.PK, "Profit Share / Rebate - Receiving Agent - 30.00% of gross revenue of 600.00", ZDateTime.Today, "PS JobNumber", true);
			AssertChargeDetails_AP(Job.Charges[2], AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 132m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, Agent2.PK, "Profit Share / Rebate - Sending Agent - 18.00% of gross revenue of 600.00, Controlling Agent - 4.00% of gross revenue of 600.00", ZDateTime.Today, "PS JobNumber", true);
		}

		public void TestCreateChargesDoesNotAlterJobHasChanges()
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();
			AssertEquals(3, Job.Charges.Count);
			Job.JH_Status = "WHL";
			Factory.Save();

			Job.JH_Status = "WRK";
			AssertEquals("Precondition - Job HasChanges is true before create charges", true, Job.HasChanges);
			creator.CreateCharges();
			AssertEquals(3, Job.Charges.Count);
			AssertEquals("Postcondition - Job HasChanges should still be true after create charges", true, Job.HasChanges);
		}

		public void TestCreateCharges_NoAdjustment()
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue);

			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			AssertNoErrors("Precondition", Job);
			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);
			AssertEquals(false, Job.HasChanges);
			AssertEquals("Shipment should not have changes as new profit share charges should have triggered save.", false, ((ForwardingShipment)Job.Parent).HasChanges);

			creator.CreateCharges();
			AssertEquals(3, Job.Charges.Count);
			AssertEquals(false, Job.HasChanges);
			AssertEquals("Shipment should not have changes as no new profit share charges were added.", false, ((ForwardingShipment)Job.Parent).HasChanges);
		}

		public void TestCreateCharges_Translatable()
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			var shareDetail = ProfitShareDetails.GetProfitShareForOrg(Agent1);
			shareDetail.ProfitShareShipmentDetails[0].ProfitSharePartyDetails.PS_PartyMinimum = 1000;

			shareDetail = ProfitShareDetails.GetProfitShareForOrg(Agent2);
			shareDetail.ProfitShareShipmentDetails[0].ProfitSharePartyDetails.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee;
			Factory.Save();

			using (var mockRes = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var minShareRes = "bc713aeb-c519-42c3-a002-4c62536b3bb2";
				mockRes.Put(minShareRes, new ResourceStringData(minShareRes, "最低利润分成覆盖"));

				var perChargeableRes = "0bf1b1e7-fd5f-4337-a075-81ac3d2a6767";
				mockRes.Put(perChargeableRes, new ResourceStringData(perChargeableRes, "每个收费单位"));

				var flatUnitRes = "be830e81-3fb1-4139-9873-9631587ee9c3";
				mockRes.Put(flatUnitRes, new ResourceStringData(flatUnitRes, "每张房屋单的固定费用"));

				var recAgentRes = "34baa181-8750-4bb0-83e5-3ec9ed4b0f21";
				mockRes.Put(recAgentRes, new ResourceStringData(recAgentRes, "收货代理"));

				var senAgentRes = "3ad7f577-f2f0-4d65-8574-de1b802d81a9";
				mockRes.Put(senAgentRes, new ResourceStringData(senAgentRes, "发送代理"));

				var profitOfRes = "a522a233-d67b-4bf6-bc72-babfa62a568c";
				mockRes.Put(profitOfRes, new ResourceStringData(profitOfRes, "{1} 利润的 {0}%"));

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					creator.CreateCharges();
				}
			}

			AssertEquals(3, Job.Charges.Count);
			AssertEquals("New created PS 1", 1000m, Job.Charges[1].JR_LocalCostAmt);
			AssertEquals("Profit Share / Rebate - 收货代理 - 最低利润分成覆盖 500.00 利润的 30.00% + (10.00 * 3 每个收费单位)", Job.Charges[1].JR_Desc);
			AssertEquals("收货代理 - 最低利润分成覆盖 500.00 利润的 30.00% + (10.00 * 3 每个收费单位)", Job.Charges[1].CostCalculationDescription.ToUTF8());

			AssertEquals("New created PS 2", 185m, Job.Charges[2].JR_LocalCostAmt);
			AssertEquals("Profit Share / Rebate - 发送代理 - 500.00 利润的 18.00% + 15.00 每张房屋单的固定费用, Controlling Agent - 500.00 利润的 4.00% + (20.00 * 3 每个收费单位)", Job.Charges[2].JR_Desc);
			AssertEquals("发送代理 - 500.00 利润的 18.00% + 15.00 每张房屋单的固定费用, Controlling Agent - 500.00 利润的 4.00% + (20.00 * 3 每个收费单位)", Job.Charges[2].CostCalculationDescription.ToUTF8());
			AssertEquals(false, Job.HasChanges);
		}

		public void TestCreateMinimumCharge()
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			var shareDetail = ProfitShareDetails.GetProfitShareForOrg(Agent1);
			shareDetail.ProfitShareShipmentDetails[0].ProfitSharePartyDetails.PS_PartyMinimum = 1000;
			Factory.Save();
			creator.CreateCharges();

			var expectedCharges = new[]
			{
				NewAssertionCharge(localSellAmt: 600, localCostAmt: 100, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 1000,
					chargeDescription: "Profit Share / Rebate - Receiving Agent - Minimum Profit Share overrides 30.00% of profit of 500.00 + (10.00 * 3 Per Chargeable Unit)",
					costCalculationDescription: "Receiving Agent - Minimum Profit Share overrides 30.00% of profit of 500.00 + (10.00 * 3 Per Chargeable Unit)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 215,
					chargeDescription: "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00 + (15.00 * 3 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 500.00 + (20.00 * 3 Per Chargeable Unit)",
					costCalculationDescription: "Sending Agent - 18.00% of profit of 500.00 + (15.00 * 3 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 500.00 + (20.00 * 3 Per Chargeable Unit)"),
			};
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);
		}

		public void TestCreateCharges_Adjustment_NoAdjustmentOrPSChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);

			ErrorReporter.Clear();
			creator.CreateCharges();
			ErrorReporter.Clear();

			AssertNotEquals("No PS created as profit share charge code not set", 3, Job.Charges.Count);
		}

		public void TestCreatePercentageCharges_Adjustment() => AssertPercentageProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value);

		public void TestCreatePercentageCharges_Adjustment_NoAdjustmentChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			AssertPercentageProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
		}

		void AssertPercentageProfitShareAdjustment(ZGuid expectedChargeCode)
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			var expectedCharges = new List<object>
			{
				NewAssertionCharge(localSellAmt: 600, localCostAmt: 100, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 180,
					chargeDescription: "Profit Share / Rebate - Receiving Agent - 30.00% of gross revenue of 600.00",
					costCalculationDescription: "Receiving Agent - 30.00% of gross revenue of 600.00"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 132,
					chargeDescription: "Profit Share / Rebate - Sending Agent - 18.00% of gross revenue of 600.00, Controlling Agent - 4.00% of gross revenue of 600.00",
					costCalculationDescription: "Sending Agent - 18.00% of gross revenue of 600.00, Controlling Agent - 4.00% of gross revenue of 600.00"),
			};
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);

			AddChargeToJob(200m, 35m);
			Factory.Save();
			creator.CreateCharges();

			expectedCharges.AddRange(new[]
			{
				NewAssertionCharge(localSellAmt: 200, localCostAmt: 35, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 60,
					chargeDescription: "Profit Share / Rebate - Adjustment: less 180.00 - Receiving Agent - 30.00% of gross revenue of 800.00",
					costCalculationDescription: "Adjustment: less 180.00 - Receiving Agent - 30.00% of gross revenue of 800.00"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 44,
					chargeDescription: "Profit Share / Rebate - Adjustment: less 132.00 - Sending Agent - 18.00% of gross revenue of 800.00, Controlling Agent - 4.00% of gross revenue of 800.00",
					costCalculationDescription: "Adjustment: less 132.00 - Sending Agent - 18.00% of gross revenue of 800.00, Controlling Agent - 4.00% of gross revenue of 800.00"),
			});
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);
		}

		public void TestCreateChargesWithChargeableUnit_Adjustment() => AssertChargeableUnitProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value);

		public void TestCreateChargesWithChargeableUnit_Adjustment_NoAdjustmentChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			AssertChargeableUnitProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
		}

		static object NewAssertionCharge(decimal localSellAmt, decimal localCostAmt, string chargeDescription, string costCalculationDescription)
		{
			return new
			{
				LocalSellAmt = localSellAmt,
				LocalCostAmt = localCostAmt,
				ChargeDescription = chargeDescription,
				CostCalculationDescription = costCalculationDescription
			};
		}

		static object NewAssertionCharge(Charge charge)
		{
			return NewAssertionCharge(
				localSellAmt: charge.JR_LocalSellAmt,
				localCostAmt: charge.JR_LocalCostAmt,
				chargeDescription: charge.JR_Desc,
				costCalculationDescription: charge.CostCalculationDescription.ToUTF8());
		}

		void AssertChargeableUnitProfitShareAdjustment(ZGuid expectedChargeCode)
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit);
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			var expectedCharges = new List<object>
			{
				NewAssertionCharge(localSellAmt: 600, localCostAmt: 100, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 180,
					chargeDescription: "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00 + (10.00 * 3 Per Chargeable Unit)",
					costCalculationDescription: "Receiving Agent - 30.00% of profit of 500.00 + (10.00 * 3 Per Chargeable Unit)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 215,
					chargeDescription: "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00 + (15.00 * 3 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 500.00 + (20.00 * 3 Per Chargeable Unit)",
					costCalculationDescription: "Sending Agent - 18.00% of profit of 500.00 + (15.00 * 3 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 500.00 + (20.00 * 3 Per Chargeable Unit)"),
			};
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));

			AddChargeToJob(500, 100);
			Factory.Save();
			creator.CreateCharges();

			expectedCharges.AddRange(new[]
			{
				NewAssertionCharge(localSellAmt: 500, localCostAmt: 100, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 120, // adjustment amount: new 300 - old 180
					chargeDescription: "Profit Share / Rebate - Adjustment: less 180.00 - Receiving Agent - 30.00% of profit of 900.00 + (10.00 * 3 Per Chargeable Unit)",
					costCalculationDescription: "Adjustment: less 180.00 - Receiving Agent - 30.00% of profit of 900.00 + (10.00 * 3 Per Chargeable Unit)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 88, // adjustment amount: new 303 - old 215
					chargeDescription: "Profit Share / Rebate - Adjustment: less 215.00 - Sending Agent - 18.00% of profit of 900.00 + (15.00 * 3 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 900.00 + (20.00 * 3 Per Chargeable Unit)",
					costCalculationDescription: "Adjustment: less 215.00 - Sending Agent - 18.00% of profit of 900.00 + (15.00 * 3 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 900.00 + (20.00 * 3 Per Chargeable Unit)"),
			});
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));

			var shipment = Factory.Load<ForwardingShipment>(Job.JH_ParentID);
			shipment.JS_ActualVolume += 2;
			Factory.Save();
			creator.CreateCharges();

			expectedCharges.AddRange(new[]
			{
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 20, // adjustment amount: 2 chargeable * $10
					chargeDescription: "Profit Share / Rebate - Adjustment: less 300.00 - Receiving Agent - 30.00% of profit of 900.00 + (10.00 * 5 Per Chargeable Unit)",
					costCalculationDescription: "Adjustment: less 300.00 - Receiving Agent - 30.00% of profit of 900.00 + (10.00 * 5 Per Chargeable Unit)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 70, // adjustment amount: 2 chargeable * $(15 + 20)
					chargeDescription: "Profit Share / Rebate - Adjustment: less 303.00 - Sending Agent - 18.00% of profit of 900.00 + (15.00 * 5 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 900.00 + (20.00 * 5 Per Chargeable Unit)",
					costCalculationDescription: "Adjustment: less 303.00 - Sending Agent - 18.00% of profit of 900.00 + (15.00 * 5 Per Chargeable Unit), Controlling Agent - 4.00% of profit of 900.00 + (20.00 * 5 Per Chargeable Unit)"),
			});
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));

			AssertEquals("Job should automatically save", false, Job.HasChanges);
		}

		public void TestCreateFlatFeeCharges_Adjustment() => AssertFlatRateProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value);

		public void TestCreateFlatFeeCharges_Adjustment_NoAdjustmentChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			AssertFlatRateProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
		}

		void AssertFlatRateProfitShareAdjustment(ZGuid expectedChargeCode)
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.FlatFee);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			var expectedCharges = new List<object>
			{
				NewAssertionCharge(
					localSellAmt: 600,
					localCostAmt: 100,
					chargeDescription: "International Freight",
					costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 160,
					chargeDescription: "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00 + 10.00 Flat Fee per House-bill",
					costCalculationDescription: "Receiving Agent - 30.00% of profit of 500.00 + 10.00 Flat Fee per House-bill"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 145,
					chargeDescription: "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00 + 15.00 Flat Fee per House-bill, Controlling Agent - 4.00% of profit of 500.00 + 20.00 Flat Fee per House-bill",
					costCalculationDescription: "Sending Agent - 18.00% of profit of 500.00 + 15.00 Flat Fee per House-bill, Controlling Agent - 4.00% of profit of 500.00 + 20.00 Flat Fee per House-bill"),
			};
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);

			AddChargeToJob(500, 100);
			Factory.Save();
			creator.CreateCharges();

			expectedCharges.AddRange(new[]
			{
				NewAssertionCharge(localSellAmt: 500,
					localCostAmt: 100,
					chargeDescription: "International Freight",
					costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 120, // new 280 - old 160
					chargeDescription: "Profit Share / Rebate - Adjustment: less 160.00 - Receiving Agent - 30.00% of profit of 900.00 + 10.00 Flat Fee per House-bill",
					costCalculationDescription: "Adjustment: less 160.00 - Receiving Agent - 30.00% of profit of 900.00 + 10.00 Flat Fee per House-bill"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 88, // new 233 - old 145
					chargeDescription: "Profit Share / Rebate - Adjustment: less 145.00 - Sending Agent - 18.00% of profit of 900.00 + 15.00 Flat Fee per House-bill, Controlling Agent - 4.00% of profit of 900.00 + 20.00 Flat Fee per House-bill",
					costCalculationDescription: "Adjustment: less 145.00 - Sending Agent - 18.00% of profit of 900.00 + 15.00 Flat Fee per House-bill, Controlling Agent - 4.00% of profit of 900.00 + 20.00 Flat Fee per House-bill"),
			});
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);
		}

		public void TestCreatePerContainerCharges_Adjustment() => AssertPerContainerProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.Value);

		public void TestCreatePerContainerCharges_Adjustment_NoAdjustmentChargeCode()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			AssertPerContainerProfitShareAdjustment(AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
		}

		void AssertPerContainerProfitShareAdjustment(ZGuid expectedChargeCode)
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.PerContainer);
			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();

			var expectedCharges = new List<object>
			{
				NewAssertionCharge(localSellAmt: 600, localCostAmt: 100, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 200,
					chargeDescription: "Profit Share / Rebate - Receiving Agent - 30.00% of profit of 500.00 + (10.00 * 5 Per Container)",
					costCalculationDescription: "Receiving Agent - 30.00% of profit of 500.00 + (10.00 * 5 Per Container)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 285,
					chargeDescription: "Profit Share / Rebate - Sending Agent - 18.00% of profit of 500.00 + (15.00 * 5 Per Container), Controlling Agent - 4.00% of profit of 500.00 + (20.00 * 5 Per Container)",
					costCalculationDescription: "Sending Agent - 18.00% of profit of 500.00 + (15.00 * 5 Per Container), Controlling Agent - 4.00% of profit of 500.00 + (20.00 * 5 Per Container)"),
			};
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);

			AddChargeToJob(600, 100);
			Factory.Save();
			creator.CreateCharges();

			expectedCharges.AddRange(new []
			{
				NewAssertionCharge(localSellAmt: 600, localCostAmt: 100, chargeDescription: "International Freight", costCalculationDescription: ""),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 150,
					chargeDescription: "Profit Share / Rebate - Adjustment: less 200.00 - Receiving Agent - 30.00% of profit of 1000.00 + (10.00 * 5 Per Container)",
					costCalculationDescription: "Adjustment: less 200.00 - Receiving Agent - 30.00% of profit of 1000.00 + (10.00 * 5 Per Container)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 110,
					chargeDescription: "Profit Share / Rebate - Adjustment: less 285.00 - Sending Agent - 18.00% of profit of 1000.00 + (15.00 * 5 Per Container), Controlling Agent - 4.00% of profit of 1000.00 + (20.00 * 5 Per Container)",
					costCalculationDescription: "Adjustment: less 285.00 - Sending Agent - 18.00% of profit of 1000.00 + (15.00 * 5 Per Container), Controlling Agent - 4.00% of profit of 1000.00 + (20.00 * 5 Per Container)"),
			});
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);

			var shipment = Factory.Load<ForwardingShipment>(Job.JH_ParentID);
			shipment.Consols[0].Containers[0].JC_ContainerCount = 7;
			Factory.Save();
			creator.CreateCharges();

			expectedCharges.AddRange(new []
			{
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 20,
					chargeDescription: "Profit Share / Rebate - Adjustment: less 350.00 - Receiving Agent - 30.00% of profit of 1000.00 + (10.00 * 7 Per Container)",
					costCalculationDescription: "Adjustment: less 350.00 - Receiving Agent - 30.00% of profit of 1000.00 + (10.00 * 7 Per Container)"),
				NewAssertionCharge(
					localSellAmt: 0,
					localCostAmt: 70,
					chargeDescription: "Profit Share / Rebate - Adjustment: less 395.00 - Sending Agent - 18.00% of profit of 1000.00 + (15.00 * 7 Per Container), Controlling Agent - 4.00% of profit of 1000.00 + (20.00 * 7 Per Container)",
					costCalculationDescription: "Adjustment: less 395.00 - Sending Agent - 18.00% of profit of 1000.00 + (15.00 * 7 Per Container), Controlling Agent - 4.00% of profit of 1000.00 + (20.00 * 7 Per Container)"),
			});
			AssertContainsExactElementsInAnyOrder(expectedCharges, Job.Charges.Select(NewAssertionCharge));
			AssertEquals(false, Job.HasChanges);
		}

		public void TestAutoCompleteJob()
		{
			CreateProfitShareDetails(OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue);

			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();
			creator.CreateCharges();
			AssertEquals(JobHeaderStatus.Working.Code, Job.JH_Status);

			AccountingConfigurationRegistry.Instance.AutoCompleteJobOnProfitShareCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			creator.CreateCharges();
			AssertEquals(JobHeaderStatus.Complete.Code, Job.JH_Status);
		}

		public void TestCreateCharges_ControllingSendingSameOrg()
		{
			Agent1 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(Agent1);
			consol.JK_UniqueConsignRef = "ConsolJob";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "ShipmentJob";
			shipment.DocAddresses.AddNew(DocAddressType.ControllingCustomer).E2_OA_Address = Agent1.MainAddress.PK;

			Job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = shipment;
			AddChargeToJob(600m, 100m);
			Factory.Save();

			OrgAgentRelationship agentProfile = Factory.New<OrgAgentRelationship>();
			agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentProfile.O3_OH_SendingAgent = Agent1.PK;

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent;
			party.PS_PartyProfitSharePercent = 10m;
			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;

			ProfitShareDetails = new ProfitShareDetailCollection();
			ProfitShareDetail detail = ProfitShareDetails.AddNew();
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment, Agent1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
			ProfitShareShipmentDetail shipmentDetail2 = new ProfitShareShipmentDetail(shipment, Agent1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail2);
			shipmentDetail2.ProfitShareAgreement = profitShareAgreement;

			AssertEquals(1, ProfitShareDetails.Count);
			AssertEquals(2, ProfitShareDetails[0].ProfitShareShipmentDetails.Count);
			AssertProfitShare(ProfitShareDetails[0].ProfitShareShipmentDetails[0], expectedToBeFound: true);
			AssertProfitShare(ProfitShareDetails[0].ProfitShareShipmentDetails[1], expectedToBeFound: false);

			AssertEquals(1, Job.Charges.Count);

			ProfitShareShipmentChargeCreator creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			creator.CreateCharges();
			AssertEquals(2, Job.Charges.Count);
			AssertNoErrors("Precondition", Job);

			creator.CreateCharges();
			AssertEquals(2, Job.Charges.Count);
		}

		public void TestCreateProfitShareCharges_UseCorrectCurrency()
		{
			AccountingConfigurationRegistry.Instance.ProfitShareCreateChargesPerCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var cnyCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "CNY"));
			var cnyBuyRate = cnyCurrency.ExchangeRates.AddNew();
			cnyBuyRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
			cnyBuyRate.RE_StartDate = DateTime.Now.AddDays(-1);
			cnyBuyRate.RE_ExpiryDate = DateTime.Now.AddDays(1);
			cnyBuyRate.RE_SellRate = 0.2m;

			var usdCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var usdBuyRate = usdCurrency.ExchangeRates.AddNew();
			usdBuyRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
			usdBuyRate.RE_StartDate = DateTime.Now.AddDays(-1);
			usdBuyRate.RE_ExpiryDate = DateTime.Now.AddDays(1);
			usdBuyRate.RE_SellRate = 1.1m;

			Agent1 = Factory.NewWithValidTestData<OrgHeader>();
			Agent1.CompanyData.OB_RX_NKAPDefltCurrency = "USD";

			var consol = Factory.New<ForwardingConsol>();
			consol.SetDefaultSendingForwarderAddress(Agent1);
			consol.JK_UniqueConsignRef = "ConsolJob";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "ShipmentJob";
			shipment.DocAddresses.AddNew(DocAddressType.ControllingCustomer).E2_OA_Address = Agent1.MainAddress.PK;

			Job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = shipment;
			AddChargeToJob(600m, 100m);
			Job.Charges[0].JR_RX_NKCostCurrency = "CNY";
			Job.Charges[0].JR_RX_NKSellCurrency = "CNY";
			Factory.Save();

			var agentProfile = Factory.New<OrgAgentRelationship>();
			agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
			agentProfile.O3_OH_SendingAgent = Agent1.PK;

			var profitShareAgreement = agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";

			var party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent;
			party.PS_PartyProfitSharePercent = 10m;
			party.PS_PartyRateBasis = OrgProfitSharePartyLookups.FeeBasisCodes.ChargeableUnit;

			ProfitShareDetails = new ProfitShareDetailCollection();
			var detail = ProfitShareDetails.AddNew();
			var shipmentDetail = new ProfitShareShipmentDetail(shipment, Agent1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, Factory);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			var shipmentDetail2 = new ProfitShareShipmentDetail(shipment, Agent1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail2);
			shipmentDetail2.ProfitShareAgreement = profitShareAgreement;

			AssertEquals(1, ProfitShareDetails.Count);
			AssertEquals(2, ProfitShareDetails[0].ProfitShareShipmentDetails.Count);
			AssertProfitShare(ProfitShareDetails[0].ProfitShareShipmentDetails[0], expectedToBeFound: true);
			AssertProfitShare(ProfitShareDetails[0].ProfitShareShipmentDetails[1], expectedToBeFound: false);

			AssertEquals(1, Job.Charges.Count);

			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			Factory.Save();

			creator.CreateCharges();
			AssertEquals(2, Job.Charges.Count);

			AssertEquals("Currency should equal the first charge's", "CNY", Job.Charges[1].JR_RX_NKCostCurrency);
		}

		#region AdjustPostedInvoiceHelper

		[ExpectNoExceptions]
		public void TestAdjustPostedInvoiceHelper_AddRoundingLine()
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var mockValidator = new Mock<IProfitShareShipmentValidator>();
			mockValidator.Setup(x => x.IsChargeValidToBePosted(It.IsAny<Charge>())).Returns(true);
			CreateProfitShareDetails("");
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job, validator: mockValidator.Object);
			Factory.Save();

			var adjustPostedInvoiceHelper = new Mock<IAdjustPostedInvoiceHelper>(MockBehavior.Strict);
			adjustPostedInvoiceHelper.Setup(x => x.AdjustPostedInvoice(It.IsAny<InvoicingBase>()));
			creator.SubstituteAdjustPostedInvoiceHelper_ForTestOnly(adjustPostedInvoiceHelper.Object);

			creator.CreateCharges();

			AssertEquals(3, Job.Charges.Count);

			AssertEquals(-150M, Job.Charges[1].JR_OSSellAmt);
			AssertEquals(-110M, Job.Charges[2].JR_OSSellAmt);

			adjustPostedInvoiceHelper.Verify(x => x.AdjustPostedInvoice(It.Is<InvoicingBase>(y => y.AH_OSTotalAmount == 150m)), Times.Once);
			adjustPostedInvoiceHelper.Verify(x => x.AdjustPostedInvoice(It.Is<InvoicingBase>(y => y.AH_OSTotalAmount == 110m)), Times.Once);
		}

		#endregion

		public void TestTransactionsAreNotCreatedWhenRegistryIsDisabled()
		{
			CreateProfitShareDetails("");
			Factory.Save();
			AssertEquals("Precondition: 1 Job charge", 1, Job.Charges.Count);

			var mockPoster = new Mock<IProfitShareShipmentChargePoster>();
			var mockValidator = new Mock<IProfitShareShipmentValidator>();
			using (AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job, chargePoster: mockPoster.Object, validator: mockValidator.Object);
				var saveCountBefore = Factory.SaveCount;

				creator.CreateCharges();

				var saveCountAfter = Factory.SaveCount;
				AssertEquals("2 new Job charges were created", 3, Job.Charges.Count);
				mockPoster.Verify(x => x.PostCharge(It.IsAny<Charge>(), It.IsAny<Job>(), It.IsAny<IAdjustPostedInvoiceHelper>()), Times.Never(), "Charges should not be posted when registry is false");
				mockPoster.Verify(x => x.UpdateJobStatus(It.IsAny<Job>()), Times.Never(), "Job status should not be updated when registry is false");
				mockValidator.Verify(x => x.IsChargeValidToBePosted(It.IsAny<Charge>()), Times.Never(), "Charge should not be checked when registry is false");
				AssertGreaterThan("Data is saved", saveCountAfter, saveCountBefore);
			}
		}

		public void TestTransactionsAreCreatedWhenRegistryIsEnabled()
		{
			CreateProfitShareDetails("");
			Factory.Save();
			AssertEquals("Precondition: 1 Job charge", 1, Job.Charges.Count);

			var mockPoster = new Mock<IProfitShareShipmentChargePoster>();
			var mockValidator = new Mock<IProfitShareShipmentValidator>();
			mockValidator.Setup(x => x.IsChargeValidToBePosted(It.IsAny<Charge>())).Returns(true);

			using (AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job, chargePoster: mockPoster.Object, validator: mockValidator.Object);
				var saveCountBefore = Factory.SaveCount;

				creator.CreateCharges();

				var saveCountAfter = Factory.SaveCount;
				AssertEquals("2 new Job charges were created", 3, Job.Charges.Count);
				mockPoster.Verify(x => x.PostCharge(It.IsAny<Charge>(), It.IsAny<Job>(), It.IsAny<IAdjustPostedInvoiceHelper>()), Times.AtLeastOnce(), "Charges should be posted when registry is true");
				mockPoster.Verify(x => x.UpdateJobStatus(It.IsAny<Job>()), Times.Once(), "Job status be updated when registry is true");
				mockValidator.Verify(x => x.IsChargeValidToBePosted(It.IsAny<Charge>()), Times.AtLeastOnce(), "charges should be checked when registry is true");
				Assert("All charges are saved", Job.Charges.All(ch => ch.IsInDatabase));
				AssertGreaterThan("Data is saved", saveCountAfter, saveCountBefore);
			}
		}

		public void TestCreatedChargeValidationError()
		{
			CreateProfitShareDetails("");
			Factory.Save();
			AssertEquals("Precondition: 1 Job charge", 1, Job.Charges.Count);

			var mockValidator = new Mock<IProfitShareShipmentValidator>();
			mockValidator.SetupSequence(x => x.IsChargeValidToBePosted(It.IsAny<Charge>())).Returns(true).Returns(false); //there will be 2 new charges, only one will be valid

			using (AccountingConfigurationRegistry.Instance.ProfitSharePostProfitShareOnCreation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job, validator: mockValidator.Object);
				var saveCountBefore = Factory.SaveCount;

				creator.CreateCharges();

				var saveCountAfter = Factory.SaveCount;
				AssertEquals("2 new Job charges were created", 3, Job.Charges.Count);
				AssertEquals(@"Profit Share charge(s) were created but not posted because there are validation error(s).
The Profit Share charge(s) should be posted manually after fixing the validation error(s).", creator.ValidationErrors);
				AssertEquals("Data is not saved", saveCountAfter, saveCountBefore);

				Assert("charge is posted but not saved", Job.Charges[1].IsRevenuePosted && !Job.Charges[1].IsInDatabase);
				Assert("When charge have error, it should not be posted", !Job.Charges[2].IsRevenuePosted && !Job.Charges[2].IsInDatabase);
			}
		}

		public void TestCreateCharges_GatewayConsol_ShouldPickChargeCodeByGatewayAgentTypeFromRegistry()
		{
			const string origin = "AUSYD";
			const string destination = "USLAX";

			var chargeCodeToOverride = TestObjectCreator.CreateChargeCode("TestCode");

			var sendingAgent = TestObjectCreator.CreateOrgHeader("SENORG", true, false);
			var sendingCompany = TestObjectCreator.CreateNewCompany("SEN", orgProxy: sendingAgent);
			var newBranch = TestObjectCreator.CreateNewBranch(sendingCompany, "SEN");
			// Gateway billing requires both sending agent and receiving agent present. At least one of them must be a gateway agent. Receiving agent is GTA in this case.
			var gatewayConsol = TestObjectCreator.CreateGatewayConsol(origin, destination, sendingGatewayCompany: sendingCompany, receivingGatewayCompany: GlbCompany.CurrentCompany);

			var agentRelationship = TestObjectCreator.CreateAgentRelationship(sendingAgent, gatewayConsol.ReceivingForwarder);
			TestObjectCreator.CreateProfitShare(agentRelationship, 80, 20, origin, destination, transportMode: "ALL",
				jobType: JobTypesList.Codes.GCN, gatewayAgentType: GatewayAgentTypesList.Codes.BGW);

			Factory.Save();

			var lookups = new OrgProfitSharePartyLookups(null);
			var profitShareChargeCodesPerPartyCollection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;

			// overridden profit share charge code per party - GatewayAgent
			// It's strange. Accessing a ChargeCodeWithType from the collection requires a description (not a code) to be passed in.
			// See: ChargeCodeWithTypeCollection.this[string key] and ChargeCodeWithTypeCollection.GetDefaultCollection()
			var profitShareChargeCodeGWA = profitShareChargeCodesPerPartyCollection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.GatewayAgent)];
			profitShareChargeCodeGWA.UseDefaultProfitShareChargeCode = false;
			profitShareChargeCodeGWA.ChargeCode = chargeCodeToOverride.PK;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				profitShareChargeCodesPerPartyCollection);

			var profitShareCalculator = new ProfitShareCalculator(Factory, new[] { gatewayConsol });
			using (var consolJob = new Job.Loader(gatewayConsol).TryLoadOrCreateWithoutMutexForTestOnly())
			{
				var existingCharge = consolJob.Charges.AddNew();
				existingCharge.JR_AC = Env.Registry.FreightChargeCode;
				existingCharge.JR_IsIncludedInProfitShare = true;
				// profit = 400
				existingCharge.JR_LocalSellAmt = 500;
				existingCharge.JR_LocalCostAmt = 100;

				gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				var profitShares = profitShareCalculator.CreateProfitShares();
				var profitShareChargeCreator = new ProfitShareShipmentChargeCreator(profitShares, consolJob, false);
				profitShareChargeCreator.CreateCharges();
				var profitShareCharge1 = consolJob.Charges.OfType<Charge>().Single(x => x.JR_AC == chargeCodeToOverride.PK);

				AssertEquals("Sending Agent - 80.00% of profit of 400.00", profitShareCharge1.CostCalculationDescription.ToUTF8());
				AssertEquals(320m, profitShareCharge1.JR_LocalCostAmt);
			}
		}

		public void TestSaveChargesExceptionHandling_ServiceTaskJCS()
		{
			Globals.IsUserInteractive = false;
			using (Env.Instance.TemporaryServiceTaskContext("JCS", canRunInAnyBranch: true))
			{
				AssertSaveChargesExceptionHandling("When running JCS service task, we leave the exception handling to JCS.",
					expectedNonCriticalExpBeingHandled: false
				);
			}
		}

		public void TestSaveChargesExceptionHandling_ServiceTasksNotJCS()
		{
			Globals.IsUserInteractive = false;
			using (Env.Instance.TemporaryServiceTaskContext("JCD", canRunInAnyBranch: true))
			{
				AssertNotEquals("PreCondition", "JCS", Env.Instance.ServiceTaskCode);
				AssertSaveChargesExceptionHandling("When running other service tasks, we do same behaviour as default.",
					expectedNonCriticalExpBeingHandled: true
				);
			}
		}

		public void TestSaveChargesExceptionHandling_NonServiceTask()
		{
			AssertNotEquals("PreCondition", "JCS", Env.Instance.ServiceTaskCode);
			AssertSaveChargesExceptionHandling("By default, we handle exception by ZExceptionReporting.",
				expectedNonCriticalExpBeingHandled: true
			);
		}

		void AssertSaveChargesExceptionHandling(string comment, bool expectedNonCriticalExpBeingHandled)
		{
			CreateProfitShareDetails("");
			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);

			var dummyNonCriticalExp = new ZCannotSaveException("DummyErrorMessage", "DummyErrorHeading", true);
			AssertEquals("PreCondition", false, dummyNonCriticalExp.IsCriticalException());
			AssertNonCriticalException();

			var dummyCriticalExp = new LoginException("Dummy Critical Exception", null);
			AssertEquals("PreCondition", true, dummyCriticalExp.IsCriticalException());
			AssertCriticalException();

			ErrorReporter.Clear();

			void AssertNonCriticalException()
			{
				Job.Factory.Saving += DummyNonCriticalExceptionThrowing;

				ClearProfitCharges();
				if (expectedNonCriticalExpBeingHandled)
				{
					creator.CreateCharges();
					AssertEquals(comment, "DummyErrorMessage", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(comment, "DummyErrorHeading", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
				else
				{
					AssertEquals(comment,
						dummyNonCriticalExp,
						AssertExceptionThrown<ZCannotSaveException>(() => creator.CreateCharges())
					);
				}

				Job.Factory.Saving -= DummyNonCriticalExceptionThrowing;
			}

			void AssertCriticalException()
			{
				Job.Factory.Saving += DummyCriticalExceptionThrowing;

				ClearProfitCharges();
				AssertEquals("We never handle critical exception by ourselves.",
					dummyCriticalExp,
					AssertExceptionThrown<Exception>(() => creator.CreateCharges())
				);

				Job.Factory.Saving -= DummyCriticalExceptionThrowing;
			}

			void DummyCriticalExceptionThrowing(BusinessObjectFactory factory)
			{
				throw dummyCriticalExp;
			}

			void DummyNonCriticalExceptionThrowing(BusinessObjectFactory factory)
			{
				throw dummyNonCriticalExp;
			}

			void ClearProfitCharges()
			{
				using (Job.ChargesLoadSuspender.GetSuspender())
				{
					var profitCharges = Job.Charges
						.Where(x => x.JR_AC == AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value)
						.ToArray();

					profitCharges.ForEach(x => {
						x.APLine.Delete();
						x.JR_AL_APLine = ZGuid.Empty;
						Job.Charges.RemoveAndDelete(x);
					});
				}
			}
		}

		public void TestBusinessContext_CreatingProfitShareCharge()
		{
			AssertEquals("Prerequisite", false, Factory.HasContext(BusinessContext.CreatingProfitShareCharge));

			CreateProfitShareDetails("");
			Factory.Save();

			var creator = new ProfitShareShipmentChargeCreator(ProfitShareDetails, Job);
			creator.CreateCharges();
			AssertEquals("Should set context", true, Factory.HasContext(BusinessContext.CreatingProfitShareCharge));
		}

		#region Implementation

		static void AssertProfitShare(ProfitShareShipmentDetail detail, bool expectedToBeFound)
		{
			var firstProfitShare = detail.ProfitShareCharges.Find(item => item.ProfitShare.HasValue);
			if (expectedToBeFound)
			{
				AssertNotNull(firstProfitShare);
			}
			else
			{
				AssertNull(firstProfitShare);
			}
		}

		void AssertChargeDetails_AP(Charge charge, Guid chargeCode, ZDecimal costAmount, ZString costCurrency, ZGuid creditor, ZString desc, ZDateTime invoiceDate, ZString invoiceNum, ZBool costPosted, string descriptionForAssert = "")
		{
			AssertEquals(descriptionForAssert + " - ChargeCode", chargeCode, charge.JR_AC);
			AssertEquals(descriptionForAssert + " - CostAmount", costAmount, charge.JR_OSCostAmt);
			AssertEquals(descriptionForAssert + " - CostCurrency", costCurrency, charge.JR_RX_NKCostCurrency);
			AssertEquals(descriptionForAssert + " - Creditor", creditor, charge.JR_OH_CostAccount);
			AssertEquals(descriptionForAssert + " - Desc", desc, charge.JR_Desc);
			AssertEquals(descriptionForAssert + " - InvoiceDate", invoiceDate, charge.JR_APInvoiceDate.Date);
			AssertEquals(descriptionForAssert + " - InvoiceNum", invoiceNum, charge.JR_APInvoiceNum);
			//AssertEquals(descriptionForAssert + " - CostPosted", CostPosted, Charge.IsCostPosted); //need to fix this as well. Factory.Save() posts the the charges!
		}

		void AssertChargeDetails_AR(Charge charge, Guid chargeCode, ZDecimal sellAmount, ZString sellCurrency, ZGuid debtor, ZString desc, ZBool sellPosted)
		{
			AssertEquals(chargeCode, charge.JR_AC);
			AssertEquals(sellAmount, charge.JR_OSSellAmt);
			AssertEquals(sellCurrency, charge.JR_RX_NKSellCurrency);
			AssertEquals(debtor, charge.JR_OH_SellAccount);
			AssertEquals(ZGuid.Empty, charge.JR_OH_CostAccount);
			AssertEquals(desc, charge.JR_Desc);
			AssertEquals(ZDateTime.Empty, charge.JR_APInvoiceDate.Date);
			AssertEquals(ZString.Empty, charge.JR_APInvoiceNum);
			AssertEquals(false, charge.IsCostPosted);
			AssertEquals(true, charge.IsRevenuePosted);
		}

		TestObjectCreator fTestObjectCreator;
		public TestObjectCreator TestObjectCreator
		{
			get
			{
				return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
			}
		}

		void CreateProfitShareDetails(string rateBasis)
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "JobNumber";
			shipment.JS_ActualVolume = 3;

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerCount = 5;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;

			var packLine = Factory.New<ForwardingPackLine>();
			packLine.JL_FreightMode = "OUT";
			packLine.JL_ActualVolume = 10;

			container.AddPackLine(packLine);
			shipment.OuterPackLines.Add(packLine);
			shipment.Consols.Add(consol);

			Agent1 = Factory.NewWithValidTestData<OrgHeader>();
			Agent2 = Factory.NewWithValidTestData<OrgHeader>();

			ProfitShareDetails = new ProfitShareDetailCollection();
			CreateProfitShareProfile(Agent1, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 30m, 10, rateBasis, ProfitShareDetails, shipment);
			CreateProfitShareProfile(Agent2, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 18m, 15, rateBasis, ProfitShareDetails, shipment);
			CreateProfitShareProfile(Agent2, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 4m, 20, rateBasis, ProfitShareDetails, shipment);

			Job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = shipment;
			Factory.Save();

			AddChargeToJob(600m, 100m);
		}

		void AddChargeToJob(decimal sell, decimal cost)
		{
			Charge existingCharge = Job.Charges.AddNew();
			existingCharge.JR_AC = Env.Registry.FreightChargeCode;
			existingCharge.JR_IsIncludedInProfitShare = true;
			existingCharge.JR_LocalSellAmt = sell;
			existingCharge.JR_LocalCostAmt = cost;
		}

		void CreateProfitShareProfile(OrgHeader agent, string role, decimal percentage, decimal rateValue, string rateBasis, ProfitShareDetailCollection profitShares, ForwardingShipment shipment)
		{
			OrgAgentRelationship agentProfile = Factory.LoadTop1<OrgAgentRelationship>(new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agent.PK));
			if (agentProfile == null)
			{
				agentProfile = Factory.New<OrgAgentRelationship>();
				agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
				agentProfile.O3_OH_SendingAgent = agent.PK;
			}

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.Count == 1 ? agentProfile.GenericProfitShareDetails[0] : agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = role;
			party.PS_PartyProfitSharePercent = percentage;
			party.PS_PartyRateBasis = rateBasis;
			if (!rateBasis.IsNullOrEmpty() && rateBasis != OrgProfitSharePartyLookups.FeeBasisCodes.GrossRevenue)
			{
				party.PS_PartyRate = rateValue;
			}

			ProfitShareDetail detail = new ProfitShareDetail(agent, Factory, role);
			profitShares.Add(detail);
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment, agent, role, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
		}

		Job Job;
		ProfitShareDetailCollection ProfitShareDetails;
		OrgHeader Agent1, Agent2;

		#endregion
	}
}
