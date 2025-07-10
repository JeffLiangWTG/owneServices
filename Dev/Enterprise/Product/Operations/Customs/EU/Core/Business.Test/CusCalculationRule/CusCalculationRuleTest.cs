using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusCalculationRule))]
	sealed class CusCalculationRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDescriptionProperty()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			cusCalculationRule.CCR_StartDate = new ZDateTimeOffset(2024, 06, 21);
			AssertEquals("INS Rule - 21-Jun-24", DescriptionPropertyAttribute.DescriptionFromBusinessObject(cusCalculationRule));
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "SYDCO";
			cusCalculationRule.CCR_OH_Importer = importer.PK;
			AssertEquals("INS Rule - SYDCO 21-Jun-24", DescriptionPropertyAttribute.DescriptionFromBusinessObject(cusCalculationRule));
			cusCalculationRule.CCR_TransportMode = "AIR";
			AssertEquals("INS Rule - SYDCO AIR 21-Jun-24", DescriptionPropertyAttribute.DescriptionFromBusinessObject(cusCalculationRule));
			cusCalculationRule.CCR_OH_Importer = ZGuid.Empty;
			AssertEquals("INS Rule - AIR 21-Jun-24", DescriptionPropertyAttribute.DescriptionFromBusinessObject(cusCalculationRule));
		}

		public void TestLookups()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			AssertType<CusCalculationRuleLookups>(cusCalculationRule.Lookups);
		}

		public void TestValidation()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			AssertType<CusCalculationRuleValidation>(cusCalculationRule.Validation);
		}

		public void TestGetFormulaFromRates()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			var rates = cusCalculationRule.CalculationRuleRateCollection;
			AssertEquals("We will add a default rate for new rule", 1, rates.Count);
			var rate1 = rates[0];
			rate1.FlatRate = 5;
			var formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("5", formula);
			AssertFormula(formula, 5);
			var rate2 = rates.AddNew();
			rate2.ValueFrom = 5;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 5, 0, 5)", formula);
			AssertFormula(formula, 0);
			rate2.Uplift = 6;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 5, 0.06*VFD, 5)", formula);
			AssertFormula(formula, 60);
			var rate3 = rates.AddNew();
			rate3.ValueFrom = 10;
			rate3.FlatRate = 66;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5))", formula);
			AssertFormula(formula, 66);
			var rate4 = rates.AddNew();
			rate4.ValueFrom = 1001;
			rate4.FlatRate = 998;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 1001, 998, IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5)))", formula);
			AssertFormula(formula, 66);

			var rate5 = rates.AddNew();
			rate5.ValueFrom = 100;
			rate5.Uplift = 0.75;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 1001, 998, IF(VFD >= 100, 0.0075*VFD, IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5))))", formula);
			AssertFormula(formula, 7.5000M);

			var rate6 = rates.AddNew();
			rate6.ValueFrom = 200;
			rate6.Uplift = 2.3;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 1001, 998, IF(VFD >= 200, 0.023*VFD, IF(VFD >= 100, 0.0075*VFD, IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5)))))", formula);
			AssertFormula(formula, 23);

			var rate7 = rates.AddNew();
			rate7.ValueFrom = 300;
			rate7.Uplift = 0.08773;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 1001, 998, IF(VFD >= 300, 0.0008773*VFD, IF(VFD >= 200, 0.023*VFD, IF(VFD >= 100, 0.0075*VFD, IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5))))))", formula);
			AssertFormula(formula, 0.8773m);

			var rate8 = rates.AddNew();
			rate8.ValueFrom = 400;
			rate8.Uplift = 0.087756;
			formula = InsuranceRuleCalculation.GetFormulaFromRates(rates);
			AssertEquals("IF(VFD >= 1001, 998, IF(VFD >= 400, 0.0008776*VFD, IF(VFD >= 300, 0.0008773*VFD, IF(VFD >= 200, 0.023*VFD, IF(VFD >= 100, 0.0075*VFD, IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5)))))))", formula);
			AssertFormula(formula, 0.8776m);

			void AssertFormula(string formula, decimal expectedResult)
			{
				CombineAssertions(() =>
				{
					try
					{
						var testRateData = new UniversalRateDataForTestWithData();
						var calculator = new UniversalRateCalculator(formula, testRateData);
						testRateData.AddDefaultFormulaSpecificAnswer(calculator.AddAnswer);
						AssertEquals(string.Format("IF VFD = 1000: {0}=>{1}", formula, expectedResult), expectedResult, calculator.Calculate());
					}
					catch (Exception e)
					{
						AssertEquals(formula + " Exception", string.Empty, e.Message);
					}
				});
			}
		}

		public void TestLoadRatesFromFormula_FormulaNotEmpty() => CombineAssertions(() =>
		{
			var cusCalculationRule1 = Factory.New<CusCalculationRule>();
			cusCalculationRule1.CCR_Formula = "IF(VFD >= 1001, 998, IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 0)))";
			cusCalculationRule1.ClearHasChanges();
			AssertEquals("Before loading CalculationRuleRateCollection, cusCalculationRule HasChanges", false, cusCalculationRule1.HasChanges);
			var rates = cusCalculationRule1.CalculationRuleRateCollection;
			AssertEquals("Collection Count", 4, rates.Count);
			AssertEquals("After loading CalculationRuleRateCollection, cusCalculationRule HasChanges", false, cusCalculationRule1.HasChanges);

			AssertEquals("First rate", 0m, rates[0].ValueFrom);
			AssertEquals("First rate", 0m, rates[0].FlatRate);
			AssertEquals("First rate", 0m, rates[0].Uplift);
			AssertEquals("Second rate", 5m, rates[1].ValueFrom);
			AssertEquals("Second rate", 6m, rates[1].Uplift);
			AssertEquals("Third rate", 10m, rates[2].ValueFrom);
			AssertEquals("Third rate", 66m, rates[2].FlatRate);
			AssertEquals("Fourth rate", 1001m, rates[3].ValueFrom);
			AssertEquals("Fourth rate", 998m, rates[3].FlatRate);

			var cusCalculationRule2 = Factory.New<CusCalculationRule>();
			cusCalculationRule2.CCR_Formula = "0";
			rates = cusCalculationRule2.CalculationRuleRateCollection;
			AssertEquals("Collection Count", 1, rates.Count);
			AssertEquals("First rate", 0m, rates[0].ValueFrom);
			AssertEquals("First rate", 0m, rates[0].FlatRate);
			AssertEquals("First rate", 0m, rates[0].Uplift);
		});

		public void TestLoadRatesFromFormula_FormulaEmpty() => CombineAssertions(() =>
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			AssertEquals("Before loading CalculationRuleRateCollection, cusCalculationRule HasChanges", false, cusCalculationRule.HasChanges);
			var rates = cusCalculationRule.CalculationRuleRateCollection;
			AssertEquals("Collection Count", 1, rates.Count);
			AssertEquals("After loading CalculationRuleRateCollection, cusCalculationRule HasChanges", true, cusCalculationRule.HasChanges);

			var rate1 = rates[0];
			AssertEquals("First rate", 0m, rate1.ValueFrom);
			AssertEquals("First rate", 0m, rate1.FlatRate);
			AssertEquals("First rate", 0m, rate1.Uplift);
		});

		public void TestSetDefaultValues()
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			AssertEquals("Company PK", GlbCompany.CurrentCompany.PK, cusCalculationRule.CCR_GC_Company);
			AssertEquals("Rule Type", CusCalculationRuleTypeList.Codes.INS, cusCalculationRule.CCR_RuleType);
			AssertEquals("End Date", ZDateTime.MaxSmallDateTime.ToOffset(), cusCalculationRule.CCR_EndDate);
		}

		public void TestOnSaving_FormulaIsUpdated() => CombineAssertions(() =>
		{
			var cusCalculationRule1 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule1.CCR_TransportMode = "FIX";
			cusCalculationRule1.CCR_Formula = ZString.Empty;
			var cusCalculationRule2 = Factory.NewWithValidTestData<CusCalculationRule>();
			cusCalculationRule2.CCR_Formula = ZString.Empty;
			var rates = cusCalculationRule2.CalculationRuleRateCollection;
			var rate1 = rates[0];
			rate1.FlatRate = 5;
			var rate2 = rates.AddNew();
			rate2.ValueFrom = 5;
			rate2.Uplift = 6;
			var rate3 = rates.AddNew();
			rate3.ValueFrom = 10;
			rate3.FlatRate = 66;
			Factory.Save();
			AssertEquals("0", cusCalculationRule1.CCR_Formula);
			AssertEquals("IF(VFD >= 10, 66, IF(VFD >= 5, 0.06*VFD, 5))", cusCalculationRule2.CCR_Formula);
		});

		public void TestCCR_EndDate_DefaultWhenEmpty() => CombineAssertions(() =>
		{
			var cusCalculationRule = Factory.New<CusCalculationRule>();
			AssertEquals("CCR_EndDate defaults to MaxSmallDateTime when created", ZDateTime.MaxSmallDateTime.ToOffset(), cusCalculationRule.CCR_EndDate);
			cusCalculationRule.CCR_EndDate = new ZDateTime(2024, 02, 10).ToOffset();
			AssertEquals("CCR_EndDate updated by user", new ZDateTime(2024, 02, 10).ToOffset(), cusCalculationRule.CCR_EndDate);
			cusCalculationRule.CCR_EndDate = ZDateTimeOffset.Empty;
			AssertEquals("CCR_EndDate defaults to MaxSmallDateTime when empty", ZDateTime.MaxSmallDateTime.ToOffset(), cusCalculationRule.CCR_EndDate);
		});

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(base.Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusCalculationRule = factory.New<CusCalculationRule>();
			cusCalculationRule.CCR_RuleType = "INS";
			cusCalculationRule.CCR_TransportMode = "AIR";
			cusCalculationRule.CCR_Formula = "F";
			cusCalculationRule.CCR_RX_NKCurrency = "AUD";
			cusCalculationRule.CCR_StartDate = ZDateTimeOffset.Today;
			cusCalculationRule.CCR_EndDate = ZDateTimeOffset.Today;
			cusCalculationRule.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			return cusCalculationRule;
		}
	}

	[TestedType(typeof(CusCalculationRule.Loader))]
	sealed class CusCalculationRuleLoaderTest : LoaderTestCase
	{
		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_ImporterAndTransportModeSpecified() => CombineAssertions(() =>
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_OH_Importer = importer.PK;
			insurance.CCR_TransportMode = "AIR";
			Factory.Save();

			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull("(empty, empty)", loader.LoadApplicableInsuranceRule(ZGuid.Empty, ZString.Empty, ZDateTimeOffset.Today));
			AssertNull("(empty, AIR)", loader.LoadApplicableInsuranceRule(ZGuid.Empty, "AIR", ZDateTimeOffset.Today));
			AssertNull("(importer, empty)", loader.LoadApplicableInsuranceRule(importer.PK, ZString.Empty, ZDateTimeOffset.Today));
			AssertEquals("(importer, AIR)", insurance, loader.LoadApplicableInsuranceRule(importer.PK, "AIR", ZDateTimeOffset.Today));
		});

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_ImporterAndTransportModeNotSpecified() => CombineAssertions(() =>
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			var loader = new CusCalculationRule.Loader(Factory);
			AssertEquals("(empty, empty)", insurance, loader.LoadApplicableInsuranceRule(ZGuid.Empty, ZString.Empty, ZDateTimeOffset.Today));
			AssertEquals("(empty, AIR)", insurance, loader.LoadApplicableInsuranceRule(ZGuid.Empty, "AIR", ZDateTimeOffset.Today));
			AssertEquals("(importer, empty)", insurance, loader.LoadApplicableInsuranceRule(importer.PK, ZString.Empty, ZDateTimeOffset.Today));
			AssertEquals("(importer, AIR)", insurance, loader.LoadApplicableInsuranceRule(importer.PK, "AIR", ZDateTimeOffset.Today));
		});

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_ImporterSpecified() => CombineAssertions(() =>
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_OH_Importer = importer.PK;
			insurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull("(empty, empty)", loader.LoadApplicableInsuranceRule(ZGuid.Empty, ZString.Empty, ZDateTimeOffset.Today));
			AssertNull("(empty, AIR)", loader.LoadApplicableInsuranceRule(ZGuid.Empty, "AIR", ZDateTimeOffset.Today));
			AssertEquals("(importer, empty)", insurance, loader.LoadApplicableInsuranceRule(importer.PK, ZString.Empty, ZDateTimeOffset.Today));
			AssertEquals("(importer, AIR)", insurance, loader.LoadApplicableInsuranceRule(importer.PK, "AIR", ZDateTimeOffset.Today));
		});

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_TransportModeSpecified() => CombineAssertions(() =>
		{
			var insurance = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance.CCR_TransportMode = "AIR";
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull("(empty, empty)", loader.LoadApplicableInsuranceRule(ZGuid.Empty, ZString.Empty, ZDateTimeOffset.Today));
			AssertEquals("(empty, AIR)", insurance, loader.LoadApplicableInsuranceRule(ZGuid.Empty, "AIR", ZDateTimeOffset.Today));
			AssertNull("(importer, empty)", loader.LoadApplicableInsuranceRule(importer.PK, ZString.Empty, ZDateTimeOffset.Today));
			AssertEquals("(importer, AIR)", insurance, loader.LoadApplicableInsuranceRule(importer.PK, "AIR", ZDateTimeOffset.Today));
		});

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_InvalidDate()
		{
			var invalidInsurance = Factory.NewWithValidTestData<CusCalculationRule>();
			invalidInsurance.CCR_StartDate = new ZDateTime(2024, 04, 01).ToOffset();
			invalidInsurance.CCR_EndDate = new ZDateTime(2024, 04, 30).ToOffset();
			invalidInsurance.CCR_TransportMode = ZString.Empty;
			Factory.Save();

			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull(loader.LoadApplicableInsuranceRule(ZGuid.Empty, ZString.Empty, ZDateTimeOffset.Today));
		}

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_InvalidTransportMode()
		{
			var invalidInsurance = Factory.NewWithValidTestData<CusCalculationRule>();
			invalidInsurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			invalidInsurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			invalidInsurance.CCR_TransportMode = "FIX";
			Factory.Save();

			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull(loader.LoadApplicableInsuranceRule(ZGuid.Empty, "AIR", ZDateTimeOffset.Today));
		}

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_InvalidImporter()
		{
			var otherImporter = Factory.NewWithValidTestData<OrgHeader>();
			var invalidInsurance = Factory.NewWithValidTestData<CusCalculationRule>();
			invalidInsurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			invalidInsurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			invalidInsurance.CCR_TransportMode = ZString.Empty;
			invalidInsurance.CCR_OH_Importer = otherImporter.PK;
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull(loader.LoadApplicableInsuranceRule(importer.PK, ZString.Empty, ZDateTimeOffset.Today));
		}

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_InvalidCompany()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var invalidInsurance = Factory.NewWithValidTestData<CusCalculationRule>(); // invalid company
			invalidInsurance.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			invalidInsurance.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			invalidInsurance.CCR_TransportMode = ZString.Empty;
			invalidInsurance.CCR_GC_Company = otherCompany.PK;
			Factory.Save();

			var importer = Factory.New<OrgHeader>();
			var loader = new CusCalculationRule.Loader(Factory);
			AssertNull(loader.LoadApplicableInsuranceRule(ZGuid.Empty, ZString.Empty, ZDateTimeOffset.Today));
		}

		[TestDate(2024, 5, 7, 1, 1, 2)]
		public void TestLoadApplicableInsuranceRule_SortingMultipleResults() => CombineAssertions(() =>
		{
			var importerA = Factory.NewWithValidTestData<OrgHeader>();
			var importerB = Factory.NewWithValidTestData<OrgHeader>();

			var insurance1 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance1.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance1.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance1.CCR_OH_Importer = ZGuid.Empty;
			insurance1.CCR_TransportMode = ZString.Empty;

			var insurance2 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance2.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance2.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance2.CCR_OH_Importer = importerA.PK;
			insurance2.CCR_TransportMode = ZString.Empty;

			var insurance3 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance3.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance3.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance3.CCR_OH_Importer = importerA.PK;
			insurance3.CCR_TransportMode = "AIR";

			var insurance4 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance4.CCR_StartDate = new ZDateTime(2024, 05, 01).ToOffset();
			insurance4.CCR_EndDate = new ZDateTime(2024, 05, 31).ToOffset();
			insurance4.CCR_OH_Importer = ZGuid.Empty;
			insurance4.CCR_TransportMode = "AIR";
			Factory.Save();

			var loader = new CusCalculationRule.Loader(Factory);
			AssertEquals("Match Importer and Transport", insurance3, loader.LoadApplicableInsuranceRule(importerA.PK, "AIR", ZDateTimeOffset.Today));
			AssertEquals("Match Transport Only. Gets rule with NULL Importer", insurance4, loader.LoadApplicableInsuranceRule(importerB.PK, "AIR", ZDateTimeOffset.Today));
			AssertEquals("Match Importer Only. Gets rule with Empty Transport", insurance2, loader.LoadApplicableInsuranceRule(importerA.PK, "SEA", ZDateTimeOffset.Today));
			AssertEquals("No Match. Gets rule with NULL Importer and Empty Transport", insurance1, loader.LoadApplicableInsuranceRule(importerB.PK, "SEA", ZDateTimeOffset.Today));
		});

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusCalculationRule.Loader(Factory);
		}
	}
}
