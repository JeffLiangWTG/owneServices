using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingBasicValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestValueSet()
		{
			SetupUOMs();

			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.CountryOfOrigin = Core.Constants.CountryCodes.Eritrea;
			guidedDecisionMakingBasic.DataGrouping = Core.Constants.CountryCodes.France;
			guidedDecisionMakingBasic.TariffType = "HSN";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.TariffCode = "123456789";
			guidedDecisionMakingBasic.CountryOfOrigin = "FR";
			guidedDecisionMakingBasic.DutyRateTypeCode = "OTH";

			guidedDecisionMakingBasic.Preference = "XXX";
			guidedDecisionMakingBasic.QuotaOrderNumber = "YYY";
			guidedDecisionMakingBasic.CustomsSecondUnitQtyDescription = "Supplementary Qty. in KG";
			guidedDecisionMakingBasic.CustomsSecondQuantity = 1m;
			guidedDecisionMakingBasic.CustomsSecondUnitQty = "KG";

			guidedDecisionMakingBasic.DataGrouping = Core.Constants.CountryCodes.Eritrea;
			CombineAssertions(() =>
			{
				AssertEquals("Preference cleared", ZString.Empty, guidedDecisionMakingBasic.Preference);
				AssertEquals("QuotaOrderNumber cleared", ZString.Empty, guidedDecisionMakingBasic.QuotaOrderNumber);
				AssertEquals("CustomsSecondQuantity NOT clear when CU2 unit Exists", 1m, guidedDecisionMakingBasic.CustomsSecondQuantity);
				AssertEquals("CustomsSecondUnitQty NOT clear when CU2 unit Exists", "KG", guidedDecisionMakingBasic.CustomsSecondUnitQty);
				AssertEquals("CustomsSecondUnitQtyDescription NOT clear when CU2 unit Exists", "Supplementary Qty. in KG", guidedDecisionMakingBasic.CustomsSecondUnitQtyDescription);

				guidedDecisionMakingBasic.TariffCode = "111111111";
				AssertEquals("CustomsSecondQuantity cleared when CU2 unit not Exists", ZDecimal.Zero, guidedDecisionMakingBasic.CustomsSecondQuantity);
				AssertEquals("CustomsSecondUnitQty cleared when CU2 unit not Exists", ZString.Empty, guidedDecisionMakingBasic.CustomsSecondUnitQty);
				AssertEquals("CustomsSecondUnitQtyDescription cleared when CU2 unit not Exists", "Supplementary Qty.", guidedDecisionMakingBasic.CustomsSecondUnitQtyDescription);

				guidedDecisionMakingBasic.CustomsSecondQuantity = 2m;
				guidedDecisionMakingBasic.TariffCode = "123456789";
				AssertEquals("CustomsSecondQuantity NOT clear when CU2 unit Exists(match TradeGroup)", 2m, guidedDecisionMakingBasic.CustomsSecondQuantity);
				AssertEquals("CustomsSecondUnitQty re-set when CU2 unit Exists", "TNE", guidedDecisionMakingBasic.CustomsSecondUnitQty);
				AssertEquals("CustomsSecondUnitQtyDescription re-set when CU2 unit Exists", "Supplementary Qty. in TNE", guidedDecisionMakingBasic.CustomsSecondUnitQtyDescription);
			});
		}

		public void TestReloadDocumentConditions()
		{
			SetupRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.JI_OrderNumber = "ord1";
			invoiceLine.JI_SupplementaryCode1 = "add1";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			invoiceLine.SupportingDocuments.AddNew("C111", "REF 1");
			var guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);

			CombineAssertions(() =>
			{
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				AssertEquals("Precondition 1: DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CustomsFirstQuantity = 2m;
				AssertEquals("DocumentConditions reload: [KGM] <= 10.000 satisfied so no DocumentConditions", 0, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.CustomsFirstQuantity = 100m;
				AssertEquals("Precondition 2: DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CustomsSecondQuantity = 2m;
				guidedDecisionMakingBasic.CustomsSecondUnitQty = "LPA";
				AssertEquals("DocumentConditions reload: [LPA] <= 10.000 satisfied so no DocumentConditions", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CustomsSecondUnitQty = "TLR";
				AssertEquals("DocumentConditions reload: no formula satisfied so DocumentConditions load", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.CustomsSecondUnitQty = "LPA";
				AssertEquals("Precondition 3: formula satisfied so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CustomsSecondQuantity = 20m;
				AssertEquals("DocumentConditions reload: [LPA] <= 10.000 not satisfied so no DocumentConditions", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.CustomsThirdQuantity = 2m;
				guidedDecisionMakingBasic.CustomsThirdUnitQty = "HLT";
				AssertEquals("DocumentConditions cleared and reload: [HLT] <= 10.000 satisfied so no DocumentConditions", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CustomsThirdUnitQty = "TLR";
				AssertEquals("DocumentConditions reload: no formula satisfied so DocumentConditions load again", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.CustomsThirdUnitQty = "HLT";
				AssertEquals("Precondition 4: formula satisfied so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CustomsThirdQuantity = 20m;
				AssertEquals("DocumentConditions reload: [HLT] <= 10.000 not satisfied so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.QuotaOrderNumber = "XX";
				AssertEquals("no QuotaOrderNumber matched data so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = false;
				guidedDecisionMakingBasic.AdditionalCodes[1].IsTicked = false;
				AssertEquals("QuotaOrderNumber matched data but additional codes cleared so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				guidedDecisionMakingBasic.AdditionalCodes[1].IsTicked = true;
				AssertEquals("QuotaOrderNumber matched data and additional codes  matched so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.Preference = "XX";
				AssertEquals("no Preference matched data so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.Preference = "P1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = false;
				AssertEquals("Preference matched data but QuotaOrderNumber cleared so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				AssertEquals("Preference matched data and QuotaOrderNumber matched data so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.TariffCode = "1111111111";
				AssertEquals("no TariffCode matched data so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.TariffCode = "1234567890";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = false;
				AssertEquals("TariffCode matched data but Preference and QuotaOrderNumber cleared so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.Preference = "P1";
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				AssertEquals("Preference matched data and QuotaOrderNumber matched data so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.DataGrouping = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("no DataGrouping matched data so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.DataGrouping = GlbCompany.CurrentCompany.Country.Code;
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = false;
				AssertEquals("DataGrouping matched data but Preference and QuotaOrderNumber cleared so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.Preference = "P1";
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				AssertEquals("DataGrouping matched data and QuotaOrderNumber matched data so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("no CountryOfOrigin matched data so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.CountryOfOrigin = Core.Constants.CountryCodes.France;
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = false;
				AssertEquals("CountryOfOrigin matched data but Preference and QuotaOrderNumber cleared so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.Preference = "P1";
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				AssertEquals("CountryOfOrigin matched data and QuotaOrderNumber matched data so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);

				guidedDecisionMakingBasic.EffectiveDate = ZDateTime.MinSmallDateTimeValue.Date;
				AssertEquals("no EffectiveDate matched data so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.EffectiveDate = ZDateTime.BrettsBirthday.Date;
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = false;
				AssertEquals("EffectiveDate matched data but Preference and QuotaOrderNumber cleared so no DocumentConditions loaded", 0, guidedDecisionMakingBasic.DocumentConditions.Count);
				guidedDecisionMakingBasic.Preference = "P1";
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				guidedDecisionMakingBasic.AdditionalCodes[0].IsTicked = true;
				AssertEquals("EffectiveDate matched data and QuotaOrderNumber matched data so DocumentConditions loaded", 1, guidedDecisionMakingBasic.DocumentConditions.Count);
			});
		}

		public void TestReloadAdditionalCodes()
		{
			SetupRefData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "P1";
			invoiceLine.JI_OrderNumber = "ord1";
			invoiceLine.JI_SupplementaryCode1 = "add1";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
			invoiceLine.SupportingDocuments.AddNew("C111", "REF 1");
			var guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);

			CombineAssertions(() =>
			{
				guidedDecisionMakingBasic.EffectiveDate = ZDateTime.MinSmallDateTimeValue.Date;
				AssertEquals("no EffectiveDate matched data so no AdditionalCodes loaded", 0, guidedDecisionMakingBasic.AdditionalCodes.Count);
				guidedDecisionMakingBasic.EffectiveDate = ZDateTime.BrettsBirthday.Date;
				AssertEquals("EffectiveDate matched data so AdditionalCodes loaded", 2, guidedDecisionMakingBasic.AdditionalCodes.Count);

				guidedDecisionMakingBasic.QuotaOrderNumber = "XX";
				AssertEquals("no QuotaOrderNumber matched data so no AdditionalCodes loaded", 0, guidedDecisionMakingBasic.AdditionalCodes.Count);
				guidedDecisionMakingBasic.QuotaOrderNumber = "ord1";
				AssertEquals("QuotaOrderNumber matched data so AdditionalCodes loaded", 2, guidedDecisionMakingBasic.AdditionalCodes.Count);

				guidedDecisionMakingBasic.Preference = "XX";
				AssertEquals("no Preference matched data so no AdditionalCodes loaded", 0, guidedDecisionMakingBasic.AdditionalCodes.Count);
				guidedDecisionMakingBasic.Preference = "P1";
				AssertEquals("Precondition: QuotaOrderNumber cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.QuotaOrderNumber);
				AssertEquals("Preference matched data so AdditionalCodes loaded", 2, guidedDecisionMakingBasic.AdditionalCodes.Count);

				guidedDecisionMakingBasic.TariffCode = "1111111111";
				AssertEquals("no TariffCode matched data so no AdditionalCodes loaded", 0, guidedDecisionMakingBasic.AdditionalCodes.Count);
				guidedDecisionMakingBasic.TariffCode = "1234567890";
				AssertEquals("Precondition: Preference cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.Preference);
				AssertEquals("Precondition: QuotaOrderNumber cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.QuotaOrderNumber);
				AssertEquals("Preference matched data so AdditionalCodes loaded", 2, guidedDecisionMakingBasic.AdditionalCodes.Count);

				guidedDecisionMakingBasic.DataGrouping = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("no DataGrouping matched data so no AdditionalCodes loaded", 0, guidedDecisionMakingBasic.AdditionalCodes.Count);
				guidedDecisionMakingBasic.DataGrouping = GlbCompany.CurrentCompany.Country.Code;
				AssertEquals("Precondition: Preference cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.Preference);
				AssertEquals("Precondition: QuotaOrderNumber cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.QuotaOrderNumber);
				AssertEquals("DataGrouping matched data so AdditionalCodes loaded", 2, guidedDecisionMakingBasic.AdditionalCodes.Count);

				guidedDecisionMakingBasic.CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("no CountryOfOrigin matched data so no AdditionalCodes loaded", 0, guidedDecisionMakingBasic.AdditionalCodes.Count);
				guidedDecisionMakingBasic.CountryOfOrigin = Core.Constants.CountryCodes.France;
				AssertEquals("Precondition: Preference cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.Preference);
				AssertEquals("Precondition: QuotaOrderNumber cleared as Empty", ZString.Empty, guidedDecisionMakingBasic.QuotaOrderNumber);
				AssertEquals("CountryOfOrigin matched data so AdditionalCodes loaded", 2, guidedDecisionMakingBasic.AdditionalCodes.Count);
			});
		}

		void SetupUOMs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs UQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "TNE DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "KG DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "HSN");
			Factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var tariffUOMView = cusTariff.UnitsOfMeasure.AddNew();
			tariffUOMView.ZZ8_Type = UOMTypeList.Codes.CU2;
			tariffUOMView.ZZ8_UOM = "TNE";

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Germany, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Eritrea, ZDateTime.BrettsBirthday.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.CreateTariffUOM(cusTariff.PK, UOMTypeList.Codes.CU2, "KG", tradeGroup: tradeGroup);
		}

		void SetupRefData()
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var ctrlType = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR1", "Test Ctrl Condition Type");
			var preference = helper.CreatePreferenceForCountry("P1", "TestPreference", dataGrouping);
			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, ZDateTime.BrettsBirthday.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "1234567890", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var dutyRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);

			var condition1ValueType1 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1ValueType2 = helper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.Formula, "Formula,", true);

			var condition = helper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, tariff.PK, "C2", true, false, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, c => c.ZX1_ZZS_Preference = preference.PK);
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType1.PK, condition.PK, "C222");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition.PK, "[KGM] <= 10.000");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition.PK, "[LPA] <= 10.000");
			helper.CreateOrGetExistingRefCusConditionValue(condition1ValueType2.PK, condition.PK, "[HLT] <= 10.000");
			helper.CreateCusApplicability(condition, tradeGroupStandard, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "add1", "ord1");
			Factory.Save();

			var testRate1 = helper.CreateRate(tariff, rateCode1.PK, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "0", preferencePk: preference.PK);
			helper.CreateCusApplicability(testRate1, tradeGroupStandard, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime, "add1", "ord1");
			Factory.Save();
		}
	}
}
