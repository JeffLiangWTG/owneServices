using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingBasic))]
	sealed class GuidedDecisionMakingBasicTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNoDuplicateAdditionalCodeWhenUpdatingTarget()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			gDMBasic.MeursingResult = "C161";

			var additionalCodes = gDMBasic.AdditionalCodes;
			var additionalCode_ADD_C161 = additionalCodes.AddNew();
			additionalCode_ADD_C161.AdditionalCode = "C161";
			additionalCode_ADD_C161.ApplicableToType = "ADD";
			var additionalCode_CVD_C161 = additionalCodes.AddNew();
			additionalCode_CVD_C161.AdditionalCode = "C161";
			additionalCode_CVD_C161.ApplicableToType = "CVD";
			var additionalCode_552_C161 = additionalCodes.AddNew();
			additionalCode_552_C161.AdditionalCode = "C161";
			additionalCode_552_C161.ApplicableToType = "552";
			var additionalCode_554_C161 = additionalCodes.AddNew();
			additionalCode_554_C161.AdditionalCode = "C161";
			additionalCode_554_C161.ApplicableToType = "554";

			var additionalCode_ADD_C161_2 = additionalCodes.AddNew();
			additionalCode_ADD_C161_2.AdditionalCode = "C161";
			additionalCode_ADD_C161_2.ApplicableToType = "ADD";

			additionalCode_ADD_C161.IsTicked = true;
			AssertEquals("Prerequisite", true, additionalCode_ADD_C161.IsTicked);
			AssertEquals("Prerequisite", false, additionalCode_CVD_C161.IsTicked);
			AssertEquals("Prerequisite", false, additionalCode_552_C161.IsTicked);
			AssertEquals("Prerequisite", false, additionalCode_554_C161.IsTicked);
			AssertEquals("Prerequisite", true, additionalCode_ADD_C161_2.IsTicked);
			AssertEquals("No duplicate additional codes in SelectedSupplementaryCodes", 1, gDMBasic.SelectedSupplementaryCodes.Count);

			var target = new Mock<IGuidedDecisionMakingTarget>();
			gDMBasic.TransferToTarget(target.Object);

			target.Verify(x => x.SetAdditionalCodes(It.Is<List<ZString>>(y => y.ContainsSameElementsInAnyOrder(new ZString[]
			{
				"C161"
			}))), Times.Once);
		}

		public void TestTariff()
		{
			var gdmBasic = GetNewBusinessObject() as GuidedDecisionMakingBasic;
			gdmBasic.DataGrouping = "FR";
			gdmBasic.EffectiveDate = ZDateTime.Today.Date;
			var tariff = gdmBasic.Tariff;
			CombineAssertions("Corresponding tariff should be loaded.", () =>
			{
				AssertType<TariffView>("Tariff type should be `TariffView`.", tariff);
				AssertEquals("Tariff code should equal to the one entered in gdmBasic.", "1111111111", tariff.ZZ1_TariffCode);
			});
		}

		public void TestTariffAdditionalCodeSelectionCriteria()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			guidedDecisionMakingBasic.CountryOfOrigin = "FR";
			guidedDecisionMakingBasic.CountryOfDestination = "US";
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.EffectiveDate = new ZDate(2024, 03, 22);
			guidedDecisionMakingBasic.IsImport = true;
			var tariffAdditionalCodeSelectionCriterias = guidedDecisionMakingBasic.TariffAdditionalCodeSelectionCriteria;

			AssertNull(tariffAdditionalCodeSelectionCriterias);
		}

		public void TestReadOnlyEffectiveDate()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			Assert("Effective Date should be not read only", !guidedDecisionMakingBasic.EffectiveDateInfo.ReadOnly);
		}

		public void TestReadOnlyTariffCode()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			Assert("Tariff Code should be not read only", !guidedDecisionMakingBasic.TariffCodeInfo.ReadOnly);
		}

		public void TestReadOnlyCountryOfOrigin()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			Assert("Country Of Origin should be not read only", !guidedDecisionMakingBasic.CountryOfOriginInfo.ReadOnly);
		}

		public void TestReadOnlyPreference()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.TariffType = "DTY";
			guidedDecisionMakingBasic.TariffCode = "1111111111";

			CombineAssertions(() =>
			{
				var lookups = guidedDecisionMakingBasic.Lookups;
				AssertEquals("Empty Preference list as empty DutyRateTypeCode", 0, lookups.PreferenceList.Count);
				AssertEquals("Preference should be read only as the list is empty", true, guidedDecisionMakingBasic.PreferenceInfo.ReadOnly);

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
				Assert("Preference list have value for RT1", lookups.PreferenceList.Count > 0);
				AssertEquals("Preference should not be read only as the list is not empty", false, guidedDecisionMakingBasic.PreferenceInfo.ReadOnly);

				guidedDecisionMakingBasic.TariffType = "ADD";
				Assert("Preference list have value for RT1 regardless tariffType", lookups.PreferenceList.Count > 0);
				AssertEquals("TariffCode has error that tariff1 is not valid tariff for ADD", true, guidedDecisionMakingBasic.PreferenceInfo.ReadOnly);

				guidedDecisionMakingBasic.TariffType = "DTY";
				guidedDecisionMakingBasic.CountryOfOrigin = "XX";
				AssertEquals("Empty Preference list as invalid CountryOfOrigin", 0, lookups.PreferenceList.Count);
				AssertEquals("CountryOfOrigin has error", true, guidedDecisionMakingBasic.PreferenceInfo.ReadOnly);

				guidedDecisionMakingBasic.CountryOfOrigin = "US";
				guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;
				AssertEquals("Empty Preference list as invalid EffectiveDate", 0, lookups.PreferenceList.Count);
				AssertEquals("EffectiveDate has error", true, guidedDecisionMakingBasic.PreferenceInfo.ReadOnly);
			});
		}

		public void TestReadOnlyQuotaOrderNumber()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.ParentDataGrouping = "EU";
			guidedDecisionMakingBasic.TariffType = "DTY";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.TariffCode = "1111111111";

			CombineAssertions(() =>
			{
				var lookups = guidedDecisionMakingBasic.Lookups;
				AssertEquals("Empty QuotaOrderNumber list as empty DutyRateTypeCode", 0, lookups.QuotaOrderNumberList.Count);
				AssertEquals("QuotaOrderNumber should be read only as the list is empty", true, guidedDecisionMakingBasic.QuotaOrderNumberInfo.ReadOnly);

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
				Assert("QuotaOrderNumber list have value for RT1", lookups.QuotaOrderNumberList.Count > 1);
				AssertEquals("QuotaOrderNumber should not be read only as the list is not empty", false, guidedDecisionMakingBasic.QuotaOrderNumberInfo.ReadOnly);

				guidedDecisionMakingBasic.TariffType = "ADD";
				Assert("QuotaOrderNumber list have value for RT1 regardless tariffType", lookups.QuotaOrderNumberList.Count > 0);
				AssertEquals("TariffCode has error that tariff1 is not valid tariff for ADD", true, guidedDecisionMakingBasic.QuotaOrderNumberInfo.ReadOnly);

				guidedDecisionMakingBasic.TariffType = "DTY";
				guidedDecisionMakingBasic.CountryOfOrigin = "XX";
				AssertEquals("Empty QuotaOrderNumber list as invalid CountryOfOrigin", 0, lookups.QuotaOrderNumberList.Count);
				AssertEquals("CountryOfOrigin has error", true, guidedDecisionMakingBasic.QuotaOrderNumberInfo.ReadOnly);

				guidedDecisionMakingBasic.CountryOfOrigin = "US";
				guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;
				AssertEquals("Empty QuotaOrderNumber list as invalid EffectiveDate", 0, lookups.QuotaOrderNumberList.Count);
				AssertEquals("EffectiveDate has error", true, guidedDecisionMakingBasic.QuotaOrderNumberInfo.ReadOnly);
			});
		}

		public void TestReadOnlyCustomsFirstQuantity()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.IsCustomsFirstQuantityReadOnly = false;

			Assert("First Quantity should be not read only", !guidedDecisionMakingBasic.CustomsFirstQuantityInfo.ReadOnly);

			guidedDecisionMakingBasic.IsCustomsFirstQuantityReadOnly = true;
			Assert("First Quantity should be read only", guidedDecisionMakingBasic.CustomsFirstQuantityInfo.ReadOnly);
		}

		public void TestReadOnlyCustomsSecondQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = Core.Constants.CountryCodes.Botswana;

			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			helper.AddCountry(tradeGroupStandard, dataGrouping, date1, date4);
			Factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
			Factory.Save();

			var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "1100110011", date1, date4, "dummy Description 0");
			helper.CreateTariff(dataGrouping, hsnTariffType.PK, "1212121212", date1, date4, "dummy Description 0");
			Factory.Save();

			helper.CreateTariffUOM(cusTariff, UOMTypeList.Codes.CU2, "T", dataGrouping);
			Factory.Save();

			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.IsCustomsSecondQuantityReadOnly = false;

			guidedDecisionMakingBasic.DataGrouping = ZString.Empty;
			guidedDecisionMakingBasic.TariffType = ZString.Empty;
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;

			guidedDecisionMakingBasic.TariffCode = "1212121212";
			Assert("Customs Second Quantity should be read only as tariffcode doesn't need a CU2", guidedDecisionMakingBasic.CustomsSecondQuantityInfo.ReadOnly);

			guidedDecisionMakingBasic.TariffCode = "1100110011";
			guidedDecisionMakingBasic.DataGrouping = "FR";
			Assert("Customs Second Quantity should be read only as tariffcode doesn't need a CU2 for FR", guidedDecisionMakingBasic.CustomsSecondQuantityInfo.ReadOnly);

			guidedDecisionMakingBasic.DataGrouping = "BW";
			guidedDecisionMakingBasic.TariffCode = "1100110011";

			Assert("Customs Second Quantity should be not read only as tariff code for this country need a CU2", !guidedDecisionMakingBasic.CustomsSecondQuantityInfo.ReadOnly);

			guidedDecisionMakingBasic.IsCustomsSecondQuantityReadOnly = true;
			Assert("Customs Second Quantity should be read only as IsCustomsSecondQuantityReadOnly is read only", guidedDecisionMakingBasic.CustomsSecondQuantityInfo.ReadOnly);
		}

		public void TestReadOnlyCustomsThirdQuantity()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.IsCustomsThirdQuantityReadOnly = false;

			Assert("First Quantity should be not read only", !guidedDecisionMakingBasic.CustomsThirdQuantityInfo.ReadOnly);

			guidedDecisionMakingBasic.IsCustomsThirdQuantityReadOnly = true;
			Assert("First Quantity should be read only", guidedDecisionMakingBasic.CustomsThirdQuantityInfo.ReadOnly);
		}

		public void TestRateSelectionCriteriaWithoutAdditionalCodes()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var rateSelectionCriteriaWithoutAdditionalCodes = guidedDecisionMakingBasic.RateSelectionCriteriaWithoutAdditionalCodes;
			CombineAssertions("RateSelectionCriteriaWithoutAdditionalCodes default value", () =>
			{
				AssertType<SpecificRateSelectionCriteria>(rateSelectionCriteriaWithoutAdditionalCodes);
				AssertEquals("TradeGroupCountry", guidedDecisionMakingBasic.EffectiveTradeGroupCountry, rateSelectionCriteriaWithoutAdditionalCodes.TradeGroupCountry);
				AssertEquals("DataGrouping", guidedDecisionMakingBasic.DataGrouping, rateSelectionCriteriaWithoutAdditionalCodes.DataGrouping);
				AssertEquals("PrimaryPreference", guidedDecisionMakingBasic.Preference, rateSelectionCriteriaWithoutAdditionalCodes.PrimaryPreference);
				AssertEquals("ConcessionOrder", guidedDecisionMakingBasic.QuotaOrderNumber, rateSelectionCriteriaWithoutAdditionalCodes.ConcessionOrder);
				AssertEquals("AdditionalCodes", 0, rateSelectionCriteriaWithoutAdditionalCodes.AdditionalCodes.Count);
				AssertEquals("EffectiveDate", guidedDecisionMakingBasic.EffectiveDate, rateSelectionCriteriaWithoutAdditionalCodes.EffectiveDate);
				AssertEquals("RateType", "", rateSelectionCriteriaWithoutAdditionalCodes.RateType);
				AssertEquals("RateCode", "", rateSelectionCriteriaWithoutAdditionalCodes.RateCode);
			});
		}

		public void TestRateSelectionCriteria()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var rateSelectionCriteria = guidedDecisionMakingBasic.RateSelectionCriteria;
			CombineAssertions("RateSelectionCriteria default value", () =>
			{
				AssertType<SpecificRateSelectionCriteria>(rateSelectionCriteria);
				AssertEquals("TradeGroupCountry", guidedDecisionMakingBasic.EffectiveTradeGroupCountry, rateSelectionCriteria.TradeGroupCountry);
				AssertEquals("DataGrouping", guidedDecisionMakingBasic.DataGrouping, rateSelectionCriteria.DataGrouping);
				AssertEquals("PrimaryPreference", guidedDecisionMakingBasic.Preference, rateSelectionCriteria.PrimaryPreference);
				AssertEquals("ConcessionOrder", guidedDecisionMakingBasic.QuotaOrderNumber, rateSelectionCriteria.ConcessionOrder);
				AssertContainsExactElementsInAnyOrder("AdditionalCodes", guidedDecisionMakingBasic.AdditionalCodes.Select(x => x.AdditionalCode), rateSelectionCriteria.AdditionalCodes.ToList());
				AssertEquals("EffectiveDate", guidedDecisionMakingBasic.EffectiveDate, rateSelectionCriteria.EffectiveDate);
				AssertEquals("RateType", "", rateSelectionCriteria.RateType);
				AssertEquals("RateCode", "", rateSelectionCriteria.RateCode);
			});
		}

		public void TestConditionSelectionCriteria()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var conditionSelectionCriteria = guidedDecisionMakingBasic.ConditionSelectionCriteria;
			CombineAssertions("ConditionSelectionCriteria default value", () =>
			{
				AssertType<ZZConditionSelectionCriteria>(guidedDecisionMakingBasic.ConditionSelectionCriteria);
				AssertEquals("EffectiveDate", guidedDecisionMakingBasic.EffectiveDate, conditionSelectionCriteria.EffectiveDate);
				AssertEquals("TradeGroupCountry", guidedDecisionMakingBasic.EffectiveTradeGroupCountry, conditionSelectionCriteria.TradeGroupCountry);
				AssertEquals("PrimaryPreference", guidedDecisionMakingBasic.Preference, conditionSelectionCriteria.PrimaryPreference);
				AssertContainsExactElementsInAnyOrder("AdditionalCodes", guidedDecisionMakingBasic.AdditionalCodes, conditionSelectionCriteria.AdditionalCodes);
				AssertEquals("ConcessionOrder", guidedDecisionMakingBasic.QuotaOrderNumber, conditionSelectionCriteria.ConcessionOrder);
				AssertEquals("DataGrouping", guidedDecisionMakingBasic.DataGrouping, conditionSelectionCriteria.DataGrouping);
				AssertEquals("Direction", ConditionChecker.ConditionDirection.Either, conditionSelectionCriteria.Direction);
				AssertEquals("ConditionClass", "", conditionSelectionCriteria.ConditionClass);
				AssertEquals("ConditionType", "", conditionSelectionCriteria.ConditionType);
			});
		}

		public void TestIsMerusingApplicable()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDate.Today.AddDays(-2);
			var endDate = ZDate.Today.AddDays(2);
			var tradeGroup5 = helper.CreateTradeGroup("FR", "TG5", startDate, endDate);
			helper.AddCountry(tradeGroup5, "US", startDate, endDate, "America");

			var addType = helper.CreateNewOrGetExistingTariffType("FR", "ADD");
			var tariff5 = helper.CreateTariff("FR", addType.PK, "55555555", startDate, endDate);
			var rateType2 = helper.CreateNewOrGetExistingRateType("FR", "RT2");
			var rateCode5 = helper.CreateCusRateCode(Factory, "RC5", rateType2.PK);
			var perference5 = helper.CreatePreferenceView("Pre5", "Preference 5", "FR");

			var rate5 = helper.CreateRate(tariff5, rateCode5.PK, startDate, endDate, "#ADFM(2)#", perference5.PK, dataGrouping: "FR");
			helper.CreateCusApplicability(rate5, tradeGroup5, startDate, endDate, "AD1", "ORD1");
			Factory.Save();

			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.IsImport = true;
			AssertEquals("IsMerusingApplicable is false if data not filled properly", false, guidedDecisionMakingBasic.IsMeursingApplicable);

			guidedDecisionMakingBasic.TariffCode = "55555555";
			guidedDecisionMakingBasic.TariffType = "ADD";
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.DutyRateTypeCode = "ADD";
			guidedDecisionMakingBasic.Preference = "Pre5";
			guidedDecisionMakingBasic.QuotaOrderNumber = "ORD1";
			guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;

			AssertEquals("IsMerusingApplicable is true if data filled properly", true, guidedDecisionMakingBasic.IsMeursingApplicable);
		}

		public void TestIsVATApplicable()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.IsImport = true;
			guidedDecisionMakingBasic.VATApplicabilities.AddNew();
			Assert("IsVATApplicable is true if import and VATApplicabilities has element", guidedDecisionMakingBasic.IsVATApplicable);

			guidedDecisionMakingBasic.IsImport = false;
			Assert("IsVATApplicable is false if import is false", !guidedDecisionMakingBasic.IsVATApplicable);

			guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			guidedDecisionMakingBasic.IsImport = true;
			Assert("IsVATApplicable is false if no VATApplicability exists", !guidedDecisionMakingBasic.IsVATApplicable);
		}

		public void TestCaptionEffectiveDate()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : Effective Date", "Effective Date", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.EffectiveDateInfo).Caption);
		}

		public void TestCaptionCountryOfOrigin()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : Country Of Origin", "Country Of Origin", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.CountryOfOriginInfo).Caption);
		}

		public void TestCaptionCountryOfDestination()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : Country Of Destination", "Country Of Destination", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.CountryOfDestinationInfo).Caption);
		}

		public void TestCaptionPreference()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : Preference", "Preference", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.PreferenceInfo).Caption);
		}

		public void TestCaptionQuotaOrderNumber()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : Quota/Order Number", "Quota/Order Number", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.QuotaOrderNumberInfo).Caption);
		}

		public void TestCaptionCustomsFirstQuantity()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : CustomsFirstQuantity", "Net Weight in KGM", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.CustomsFirstQuantityInfo).Caption);
		}

		public void TestCaptionCustomsSecondQuantity()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : CustomsSecondQuantity", "Supplementary Qty.", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.CustomsSecondQuantityInfo).Caption);
		}

		public void TestCaptionCustomsThirdQuantity()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : CustomsThirdQuantity", "Third Quantity", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.CustomsThirdQuantityInfo).Caption);
		}

		public void TestCustomsThirdQuantityReadOnly()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("CustomsThirdQuantity not ReadOnly", false, guidedDecisionMakingBasic.CustomsThirdQuantityInfo.ReadOnly);

			guidedDecisionMakingBasic.CustomsThirdUnitQty = "";
			AssertEquals("CustomsThirdQuantity not ReadOnly, CustomsThirdQuantityReadOnly is deleted", false, guidedDecisionMakingBasic.CustomsThirdQuantityInfo.ReadOnly);
		}

		public void TestCaptionMeursingResult()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			AssertEquals("Caption : Meursing Result", "Meursing Result", DataBoundResourceStrings.GetDataForProperty(guidedDecisionMakingBasic.MeursingResultInfo).Caption);
		}

		public void TestDefaultFromIGuidedDecisionMakingSource()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("EffectiveDate", ZDate.BrettsBirthday, guidedDecisionMakingBasic.EffectiveDate);
				AssertEquals("DataGrouping", "DG1", guidedDecisionMakingBasic.DataGrouping);
				AssertEquals("ParentDataGrouping", "PDG", guidedDecisionMakingBasic.ParentDataGrouping);
				AssertEquals("UserLanguage", "EN", guidedDecisionMakingBasic.UserLanguage);
				AssertEquals("DutyRateTypeCode", "OTH", guidedDecisionMakingBasic.DutyRateTypeCode);
				AssertEquals("TariffCode", "1111111111", guidedDecisionMakingBasic.TariffCode);
				AssertEquals("CountryOfOrigin", "FR", guidedDecisionMakingBasic.CountryOfOrigin);
				AssertEquals("Preference", "P1", guidedDecisionMakingBasic.Preference);
				AssertEquals("QuotaOrderNumber", "Number1", guidedDecisionMakingBasic.QuotaOrderNumber);
				AssertEquals("CustomsFirstQuantity", 100m, guidedDecisionMakingBasic.CustomsFirstQuantity);
				AssertEquals("CustomsSecondQuantity", 200m, guidedDecisionMakingBasic.CustomsSecondQuantity);
				AssertEquals("CustomsSecondUnitQty", "LPA", guidedDecisionMakingBasic.CustomsSecondUnitQty);
				AssertEquals("CustomsSecondQuantity", 300m, guidedDecisionMakingBasic.CustomsThirdQuantity);
				AssertEquals("CustomsSecondUnitQty", "HLT", guidedDecisionMakingBasic.CustomsThirdUnitQty);
				AssertEquals("TariffType", "DEF", guidedDecisionMakingBasic.TariffType);
				AssertEquals("IsImport", false, guidedDecisionMakingBasic.IsImport);
				AssertEquals("IsExport", false, guidedDecisionMakingBasic.IsExport);
				AssertEquals("SupportingDocuments", 0, guidedDecisionMakingBasic.CapturedDocumentConditions.Count);
				AssertSequencesEqual("SupplementaryCodes", new ZString[] { "ADD1" }, guidedDecisionMakingBasic.CapturedSupplementaryCodes);
			});

			var gDMBasicSource = new Mock<IGuidedDecisionMakingSource>();
			gDMBasicSource.Setup(x => x.EffectiveCountryOfDestination).Returns("DE");
			var gdm = new GuidedDecisionMakingBasic(gDMBasicSource.Object, Factory);
			AssertEquals("CountryOfDestination", "DE", gdm.CountryOfDestination);
		}

		public void TestSelectedSupplementaryCodes()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var add1 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add1.IsTicked = true;
			add1.AdditionalCode = "ADD1";

			var add2 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add2.IsTicked = true;
			add2.AdditionalCode = "ADD2";

			var add3 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add3.IsTicked = false;
			add3.AdditionalCode = "ADD3";

			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				"ADD1", "ADD2"
			}, guidedDecisionMakingBasic.SelectedSupplementaryCodes);
		}

		public void TestGroupedAdditionalCodes()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var add1 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add1.IsTicked = true;
			add1.AdditionalCode = "ADD1";
			add1.ApplicableToType = "B";
			add1.ApplicableToDescription = "F";

			var add2 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add2.IsTicked = true;
			add2.AdditionalCode = "ADD4";
			add2.ApplicableToType = "A";
			add2.ApplicableToDescription = "Z";

			var add3 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add3.IsTicked = false;
			add3.AdditionalCode = "ADD3";
			add3.ApplicableToType = "C";
			add3.ApplicableToDescription = "Z";

			var add4 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add4.IsTicked = true;
			add4.AdditionalCode = "ADD5";
			add4.ApplicableToType = "A";
			add4.ApplicableToDescription = "F";

			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				"ADD5", "ADD1"
			}, guidedDecisionMakingBasic.GroupedAdditionalCodes.First(x => x.Key == "F").Select(g => g.AdditionalCode).ToList());

			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				"ADD4", "ADD3"
			}, guidedDecisionMakingBasic.GroupedAdditionalCodes.First(x => x.Key == "Z").Select(g => g.AdditionalCode).ToList());
		}

		public void TestSelectedSupplementaryCodes_MeursingResult_VAT()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var add1 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add1.IsTicked = true;
			add1.AdditionalCode = "ADD1";

			guidedDecisionMakingBasic.MeursingResult = "7010";

			var vat1 = guidedDecisionMakingBasic.VATApplicabilities.AddNew();
			vat1.AdditionalCode = "VAT1";
			vat1.IsTicked = true;
			AssertContainsExactElementsInAnyOrder(new ZString[]
			{
				"ADD1", "7010", "VAT1"
			}, guidedDecisionMakingBasic.SelectedSupplementaryCodes);
		}

		public void TestSelectedDocuments()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var docA = guidedDecisionMakingBasic.DocumentConditions.AddNew();
			var con1 = docA.ConditionDetails.AddNew();
			con1.Code = "4001";
			con1.Reference = "REF001";
			con1.DateOfIssue = new ZDateTime(2023, 06, 22);
			con1.IsTicked = true;

			var con2 = docA.ConditionDetails.AddNew();
			con2.Code = "4002";
			con2.Reference = "REF002";
			con2.DateOfIssue = new ZDateTime(2023, 06, 23);
			con2.IsTicked = false;

			var docB = guidedDecisionMakingBasic.DocumentConditions.AddNew();
			var con3 = docB.ConditionDetails.AddNew();
			con3.Code = "4003";
			con3.Reference = "";
			con3.DateOfIssue = new ZDateTime(2023, 06, 24);
			con3.IsTicked = true;

			AssertContainsExactElementsInAnyOrder(new (ZString, ZString, ZDateTime)[]
			{
				("4001", "REF001", new ZDateTime(2023, 06, 22)), ("4003", "", new ZDateTime(2023, 06, 24))
			}, guidedDecisionMakingBasic.SelectedDocuments);
		}

		public void TestTransferToTarget()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var vat1 = guidedDecisionMakingBasic.VATApplicabilities.AddNew();
			vat1.AdditionalCode = "VAT1";
			vat1.IsTicked = true;
			var add1 = guidedDecisionMakingBasic.AdditionalCodes.AddNew();
			add1.IsTicked = true;
			add1.AdditionalCode = "ADD1";
			guidedDecisionMakingBasic.MeursingResult = "7010";
			var docA = guidedDecisionMakingBasic.DocumentConditions.AddNew();
			var con1 = docA.ConditionDetails.AddNew();
			con1.Code = "4001";
			con1.Reference = "REF001";
			con1.DateOfIssue = new ZDateTime(2023, 06, 22);
			con1.IsTicked = true;
			
			var target = new Mock<IGuidedDecisionMakingTarget>();
			guidedDecisionMakingBasic.TransferToTarget(target.Object);

			target.Verify(x => x.SetAdditionalCodes(It.Is<List<ZString>>(y => y.ContainsSameElementsInAnyOrder(new ZString[]
			{
				"ADD1", "7010", "VAT1"
			}))), Times.Once);
			var expectedSupportingDocuments = new List<(ZString, ZString, ZDateTime)>
			{
				("4001", "REF001", new ZDateTime(2023, 06, 22))
			};
			target.Verify(x => x.SetSupportingAndAdditionalDocuments(It.Is<List<(ZString, ZString, ZDateTime)>>(y => y.ContainsSameElementsInAnyOrder(expectedSupportingDocuments))), Times.Once);
			target.VerifySet(x => x.CountryOfOrigin = "FR", Times.Once);
			target.VerifySet(x => x.Preference = "P1", Times.Once);
			target.VerifySet(x => x.QuotaOrderNumber = "Number1", Times.Once);
			target.VerifySet(x => x.TariffCode = "1111111111", Times.Once);
			target.VerifySet(x => x.CustomsFirstQuantity = 100m, Times.Once);
			target.VerifySet(x => x.CustomsFirstUnitQty = "KGM", Times.Once);
			target.VerifySet(x => x.CustomsSecondQuantity = 200m, Times.Once);
			target.VerifySet(x => x.CustomsSecondUnitQty = "LPA", Times.Once);
			target.VerifySet(x => x.CustomsThirdQuantity = 300m, Times.Once);
			target.VerifySet(x => x.CustomsThirdUnitQty = "HLT", Times.Once);

			guidedDecisionMakingBasic.CountryOfDestination = "DE";
			target = new Mock<IGuidedDecisionMakingTarget>();
			guidedDecisionMakingBasic.TransferToTarget(target.Object);
			target.VerifySet(x => x.CountryOfDestination = "DE", Times.Once);
			target.Verify(x => x.SetVATCode("", "VAT1"), Times.Never);

			guidedDecisionMakingBasic.IsImport = true;
			guidedDecisionMakingBasic.TransferToTarget(target.Object);
			target.Verify(x => x.SetVATCode("", "VAT1"), Times.Once);

			Assert(true);
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name.Equals("TariffCode"))
			{
				return;
			}
			base.TestBizObjectField(info);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGroup = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("FR", "France", parentDataGroup);
			helper.CreateNewOrGetExistingDataGrouping("DE", "German", parentDataGroup);
			helper.CreateNewOrGetExistingDataGrouping("AU", "AU");
			helper.CreateNewOrGetExistingDataGrouping("US", "US");

			var dtyType = helper.CreateTariffType("EUN", "DTY");
			var addType = helper.CreateTariffType("FR", "ADD");
			Factory.Save();

			var startDate = ZDate.Today.AddDays(-2);
			var endDate = ZDate.Today.AddDays(2);
			var tariff1 = helper.CreateTariff("FR", dtyType.PK, "1111111111", startDate, endDate);
			var tariff2 = helper.CreateTariff("EUN", dtyType.PK, "3333333333", startDate, endDate);
			var tariff3 = helper.CreateTariff("FR", dtyType.PK, "2222222222", startDate, endDate);
			var tariff4 = helper.CreateTariff("FR", addType.PK, "4444444444", startDate, endDate);

			var tradeGroup1 = helper.CreateTradeGroup("EUN", "TG1", startDate, endDate);
			var tradeGroup2 = helper.CreateTradeGroup("FR", "TG2", startDate, endDate);
			var tradeGroup3 = helper.CreateTradeGroup("AU", "TG3", startDate, endDate);
			var tradeGroup4 = helper.CreateTradeGroup("FR", "TG4", startDate, endDate);
			helper.AddCountry(tradeGroup1, "US", startDate, endDate, "America");
			helper.AddCountry(tradeGroup2, "DE", startDate, endDate, "German");
			helper.AddCountry(tradeGroup2, "AU", startDate, endDate, "Australia");
			helper.AddCountry(tradeGroup3, "NZ", startDate, endDate, "Nealand");
			helper.AddCountry(tradeGroup4, "US", startDate, endDate, "America");

			var perference1 = helper.CreatePreferenceView("Pre1", "Preference 1", "EUN");
			var perference2 = helper.CreatePreferenceView("Pre2", "Preference 2", "FR");
			var perference3 = helper.CreatePreferenceView("Pre3", "Preference 3", "AU");
			var perference4 = helper.CreatePreferenceView("Pre4", "Preference 4", "FR");
			var rateType1 = helper.CreateCusRateType("EUN", "RT1");
			var rateType2 = helper.CreateCusRateType("FR", "RT2");
			var rateCode1 = helper.CreateCusRateCode(Factory, "RC1", rateType1.PK);
			var rateCode2 = helper.CreateCusRateCode(Factory, "RC2", rateType1.PK);
			var rateCode3 = helper.CreateCusRateCode(Factory, "RC3", rateType1.PK);
			var rateCode4 = helper.CreateCusRateCode(Factory, "RC4", rateType1.PK);

			var rate1 = helper.CreateRate(tariff1, rateCode1.PK, startDate, endDate, "0.1", perference1.PK);
			var rate2 = helper.CreateRate(tariff1, rateCode2.PK, startDate, endDate, "0.2", perference2.PK);
			var rate3 = helper.CreateRate(tariff3, rateCode3.PK, startDate, endDate, "0.3", perference3.PK);
			var rate4 = helper.CreateRate(tariff1, rateCode4.PK, startDate, endDate, "0.4", perference4.PK);
			helper.CreateCusApplicability(rate1, tradeGroup1, startDate, endDate, orderNumber: "Ord1");
			helper.CreateCusApplicability(rate2, tradeGroup1, startDate, endDate, orderNumber: "Ord2");
			helper.CreateCusApplicability(rate2, tradeGroup2, startDate, endDate, orderNumber: "Ord3");
			helper.CreateCusApplicability(rate3, tradeGroup2, startDate, endDate, orderNumber: "Ord4");
			helper.CreateCusApplicability(rate3, tradeGroup2, startDate, endDate, orderNumber: "Ord5");

			var applicability4 = helper.CreateCusApplicability(rate4, tradeGroup4, startDate, endDate, orderNumber: "Ord6");
			helper.CreateExcludedTradeGroup(tradeGroup4, applicability4);
			Factory.Save();
		}

		public void TestEffectiveTradeGroupCountry()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.CountryOfOrigin = "FR";
			guidedDecisionMakingBasic.CountryOfDestination = "DE";

			AssertEquals("Import : EffectiveTradeGroupCountry is equal to CountryOfOrigin", "FR", guidedDecisionMakingBasic.EffectiveTradeGroupCountry);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			guidedDecisionMakingBasic.CountryOfOrigin = "FR";
			guidedDecisionMakingBasic.CountryOfDestination = "DE";

			AssertEquals("Export : EffectiveTradeGroupCountry is equal to CountryOfDestination", "DE", guidedDecisionMakingBasic.EffectiveTradeGroupCountry);
		}

		public void TestPreferredLanguage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkingLanguage = Core.SharedConstants.Languages.German;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_WorkingLanguage = Core.SharedConstants.Languages.Afrikaans;
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_WorkingLanguage = ZString.Empty;
			var homeBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), homeBranch.PK.ToGuid(), Guid.Empty))
			{
				var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
				AssertEquals("PreferredLanguage should return staff working language, german.", Core.SharedConstants.Languages.German, gdmBasic.PreferredLanguage);
			}

			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), homeBranch.PK.ToGuid(), Guid.Empty))
			{
				var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
				AssertEquals("PreferredLanguage should return staff working language, afrikaans.", Core.SharedConstants.Languages.Afrikaans, gdmBasic.PreferredLanguage);
			}

			var gdmBasicEmpty = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			AssertEquals("PreferredLanguage is english by default.", Core.SharedConstants.Languages.English, gdmBasicEmpty.PreferredLanguage);
		}

		public void TestConfiguration()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			AssertType<GdmConfiguration>(guidedDecisionMakingBasic.Configuration);
		}

		public void TestSelectAllWaviersCaption()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			AssertEquals("Select all Y - series waivers", guidedDecisionMakingBasic.SelectAllWaiversCaption);
		}

		protected override BusinessObject GetNewBusinessObject() => GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
	}
}
