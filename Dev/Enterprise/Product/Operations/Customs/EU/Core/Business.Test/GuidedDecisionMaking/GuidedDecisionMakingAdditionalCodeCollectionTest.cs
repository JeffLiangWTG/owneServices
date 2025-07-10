using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingAdditionalCodeCollection))]
	sealed class GuidedDecisionMakingAdditionalCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuidedDecisionMakingAdditionalCodeCollection>
	{
		public void TestLoadExcludesRateClassConditionAdditionalCodes()
		{
			SetupTariffAndRate();
			SetupCusCodeList();
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "RED";
			invoiceLine.JI_ConcessionOrder = "";
			var guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
			var additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic).Cast<GuidedDecisionMakingAdditionalCode>();
			Assert("Additional codes from control conditions should be in the list", additionalCodes.Any(x => x.AdditionalCode == "AddCon"));
			Assert("Additional codes from rate conditions should not be in the list", !additionalCodes.Any(x => x.AdditionalCode == "AddRate"));
		}

		public void TestLoad()
		{
			SetupTariffAndRate();
			SetupCusCodeList();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "RED";
			invoiceLine.JI_ConcessionOrder = "";
			var guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
			var additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic);

			CombineAssertions("Loaded list of additional codes properties, no additional code on invoice line.", () =>
			{
				AssertEquals("Count", 4, additionalCodes.Count);

				AssertAdditionalCode(additionalCodes[0], "ADD", "Rate Type: ADD Add desc", "Add31", false);
				AssertAdditionalCode(additionalCodes[1], "DTY", "Rate Type: DTY Duty desc", "Add21", false);
				AssertAdditionalCode(additionalCodes[2], "DTY", "Rate Type: DTY Duty desc", "Add22", false);
				AssertAdditionalCode(additionalCodes[3], "CTR", "Condition Type: CTR Test Ctrl Condition Type", "AddCon", false);
			});

			invoiceLine.JI_SupplementaryCode1 = "Add31";
			guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
			additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic);

			CombineAssertions("Loaded list of additional codes properties, Add31 set as additional code in invoice line.", () =>
			{
				AssertEquals("Count", 2, additionalCodes.Count);
				AssertAdditionalCode(additionalCodes[0], "ADD", "Rate Type: ADD Add desc", "Add31", true);
				AssertAdditionalCode(additionalCodes[1], "CTR", "Condition Type: CTR Test Ctrl Condition Type", "AddCon", false);
			});
		}

		void AssertAdditionalCode(GuidedDecisionMakingAdditionalCode gDMAdditionalCode, ZString applicableToType, ZString applicableToDescription, ZString additionalCode, bool isTicked)
		{
			AssertEquals("ApplicableToType", applicableToType, gDMAdditionalCode.ApplicableToType);
			AssertEquals("ApplicableToDescription", applicableToDescription, gDMAdditionalCode.ApplicableToDescription);
			AssertEquals("AdditionalCode", additionalCode, gDMAdditionalCode.AdditionalCode);
			AssertEquals("Add31 should not be ticked because it was captured as additional code in Invoice Line", isTicked, gDMAdditionalCode.IsTicked);
		}

		public void TestLoadDescriptionForLegacyInvoice()
		{
			SetupTariffAndRate();
			SetupCusCodeList();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.JZ_ValuationDateOverride = new ZDate(2010, 12, 10);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1122334455667";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_PrimaryPreference = "RED";
			invoiceLine.JI_ConcessionOrder = "";

			var guidedDecisionMakingBasic = new GuidedDecisionMakingBasic(new GuidedDecisionMakingSingleInvoiceLineSource(invoiceLine), Factory);
			var additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic);

			CombineAssertions("Loaded list of additional codes properties, no additional code on invoice line.", () =>
			{
				AssertEquals("Count", 4, additionalCodes.Count);

				AssertEquals("Ordered 1: AdditionalDescription", "Add31", additionalCodes[0].AdditionalCode);
				AssertEquals("Ordered 2: AdditionalDescription", "Add21", additionalCodes[1].AdditionalCode);
				AssertEquals("Ordered 3: AdditionalDescription", "Add22", additionalCodes[2].AdditionalCode);
			});
		}

		public void TestSetParentForNewElement()
		{
			var collection = GetCollectionToTest();
			AssertNotNull("Parent", collection.AddNew().Parent);
		}

		public void TestAdditionalCodeCollectionLoadByTradeGroup()
		{
			SetUpTariffAndConditionWithAdditionalCodesOfDifferentTradeGroup();

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.JZ_ValuationDateOverride = new ZDate(2010, 12, 10);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.France;
			invoiceLine.JI_ConcessionOrder = "";
			invoiceLine.JI_Tariff = "19990728";
			invoiceLine.JI_SupplementaryCode1 = "ADD1";
			invoiceLine.JI_SupplementaryCode2 = "ADD2";

			var guidedDecisionMakingBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var additionalCodes = new GuidedDecisionMakingAdditionalCodeCollection(guidedDecisionMakingBasic);
			CombineAssertions("Only ADD1 of TradeGroupStandard should be loaded as FR is one of the member of TradeGroupStandard", () =>
			{
				AssertEquals(1, additionalCodes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "ADD1" }, additionalCodes.Select(x => x.AdditionalCode.ToString()));
			});

			void SetUpTariffAndConditionWithAdditionalCodesOfDifferentTradeGroup()
			{
				var date1 = new ZDate(2010, 12, 10);
				var date2 = new ZDate(2079, 06, 06);
				var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

				var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2);
				var tradeGroupSpecial = RefDataHelper.CreateTradeGroup(dataGrouping, "SPECIAL", date1, date2);
				RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, date1, date2);
				RefDataHelper.AddCountry(tradeGroupSpecial, Core.Constants.CountryCodes.Germany, date1, date2);
				var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
				Factory.Save();

				var cusTariff = RefDataHelper.CreateTariff(dataGrouping, tariffType.PK, "19990728", date1, date2, "Dummy Description");
				var ctrlType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR", "Test Ctrl Condition Type");
				var conditionValueType = RefDataHelper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
				var condition1 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, cusTariff.PK, "C1", true, false, date1, date2);
				RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "C111");
				RefDataHelper.CreateCusApplicability(condition1, tradeGroupStandard, date1, date2, "ADD1");
				RefDataHelper.CreateCusApplicability(condition1, tradeGroupSpecial, date1, date2, "ADD2");

				var cmpType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CMP", "Test CMP Condition Type");
				var condition2 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, cusTariff.PK, "C1", false, false, date1, date2);
				RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition2.PK, "C222");
				RefDataHelper.CreateCusApplicability(condition2, tradeGroupStandard, date1, date2, "ADD3");
				RefDataHelper.CreateCusApplicability(condition2, tradeGroupSpecial, date1, date2, "ADD4");

				Factory.Save();
			}
		}

		protected override GuidedDecisionMakingAdditionalCodeCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			return new GuidedDecisionMakingAdditionalCodeCollection(gDMBasic);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var gDMBasic = new GuidedDecisionMakingBasic(invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineSource(), Factory);
			return new GuidedDecisionMakingAdditionalCode(gDMBasic);
		}

		void SetupTariffAndRate()
		{
			var date1 = new ZDate(2010, 12, 10);
			var date2 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date2);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.France, date1, date2);
			var tariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			Factory.Save();

			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, tariffType.PK, "1122334455667", date1, date2, "dummy Description 0");

			var dutyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty desc");
			var rateCode1 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var rateCode3 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC2", dutyRateType.PK);
			var rateCode4 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC3", dutyRateType.PK);
			var addRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.AntiDumping, "Add desc");
			var rateCode2 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			var preferenceSTD = RefDataHelper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			RefDataHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", dataGrouping);
			Factory.Save();

			var testRate1 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date2, "0", preferencePk: preferenceSTD.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date2, "Add11", "ord11");
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date2, "Add12", "ord12");

			var testRate2 = RefDataHelper.CreateRate(cusTariff, rateCode3.PK, date1, date2, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date2, "Add21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date2, "Add22", "ord22");

			var testRate4 = RefDataHelper.CreateRate(cusTariff, rateCode4.PK, date1, date2, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date2, "Add21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate4, tradeGroupStandard, date1, date2, "Add22", "ord22");

			var testRate3 = RefDataHelper.CreateRate(cusTariff, rateCode2.PK, date1, date2, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard, date1, date2, "Add31", "ord31");

			var ctrlType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "CTR", "Test Ctrl Condition Type");
			var conditionValueType = RefDataHelper.CreateOrGetExistingRefCusConditionValueType(dataGrouping, Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument, "SupportingDocument");
			var condition1 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, ctrlType.PK, cusTariff.PK, "C1", true, false, date1, date2);
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "C111");
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition1.PK, "R111");
			RefDataHelper.CreateCusApplicability(condition1, tradeGroupStandard, date1, date2, "AddCon");

			var classType = RefDataHelper.CreateOrGetExistingRefCusConditionType(dataGrouping, Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "RATE", "Test Class  Condition Type");
			var condition2 = RefDataHelper.CreateOrGetExistingRefCusCondition(dataGrouping, classType.PK, cusTariff.PK, "C2", true, false, date1, date2);
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition2.PK, "C211");
			RefDataHelper.CreateOrGetExistingRefCusConditionValue(conditionValueType.PK, condition2.PK, "C222");
			RefDataHelper.CreateCusApplicability(condition2, tradeGroupStandard, date1, date2, "AddRate");

			Factory.Save();
		}

		void SetupCusCodeList()
		{
			const string additionalCodes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes;
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var datetime1 = ZDateTime.Today.AddDays(-1);
			var datetime2 = ZDateTime.Today.AddDays(1);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", currentCountry);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "Add11", "Add11 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "Add12", "Add12 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "Add21", "Add21 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "Add22", "Add22 Descriptions", datetime1, datetime2);
			RefDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "Add31", "Add31 Descriptions", datetime1, datetime2);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
