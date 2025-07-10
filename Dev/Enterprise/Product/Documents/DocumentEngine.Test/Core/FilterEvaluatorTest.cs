using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class FilterEvaluatorTest : TestCaseWithFactory
	{
		public void TestCompanyCountryCodeGetsHardMacroInPlaceOfFieldOnBODocDataProvider()
		{
			AssertEquals("Precondition: GlbCompany.CurrentCompany.CountryCode.Code", "AU", GlbCompany.CurrentCompany.Country.Code);

			var dataSource = new CompanyCountryCodeDocDataProvider(Enterprise.Core.Constants.CountryCodes.Netherlands);
			var filterEvaluator = new FilterEvaluator(dataSource);

			AssertEquals("filterEvaluator.MatchesFilter(''<CompanyCountryCode>' == 'AU'')", true, filterEvaluator.MatchesFilter("\"<CompanyCountryCode>\" == \"AU\""));
		}

		public void TestMatchesFilter()
		{
			var filterEvaluator = new FilterEvaluator(null, null);
			AssertMatchesFilter(filterEvaluator, true, "");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_VarCharMax>' == 'LIGHT'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_VarCharMax>' == 'DARK'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_ImaginaryField>' == 'LIGHT'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_VarCharMax>' =?=?= 'LIGHT'");

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "LIGHT";

			filterEvaluator = new FilterEvaluator(BODocDataProvider.Get(dummy), null);
			AssertMatchesFilter(filterEvaluator, true, "");
			AssertMatchesFilter(filterEvaluator, true, "'<Z0_VarCharMax>' == 'LIGHT'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_VarCharMax>' == 'DARK'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_ImaginaryField>' == 'LIGHT'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_VarCharMax>' =?=?= 'LIGHT'");

			var dummy2 = Factory.New<DummyBusinessObjectWithCalculatedCodeProperty>();
			dummy2.TestCodeProperty = "DARK";

			filterEvaluator = new FilterEvaluator(BODocDataProvider.Get(dummy), BODocDataProvider.Get(dummy2));
			AssertMatchesFilter(filterEvaluator, true, "");
			AssertMatchesFilter(filterEvaluator, true, "'<Z0_VarCharMax>' == 'LIGHT'");
			AssertMatchesFilter(filterEvaluator, false, "'<Z0_VarCharMax>' == 'DARK'");
			AssertMatchesFilter(filterEvaluator, false, "'<TestCodeProperty>' == 'LIGHT'");
			AssertMatchesFilter(filterEvaluator, true, "'<TestCodeProperty>' == 'DARK'");
		}

		static void AssertMatchesFilter(FilterEvaluator filterEvaluator, bool expectedResult, string filterToTest)
		{
			AssertEquals(filterToTest, expectedResult, filterEvaluator.MatchesFilter(filterToTest.Replace('\'', '"')));
		}

		public void TestReplaceMacrosWithoutDataSource()
		{
			var evaluator = new FilterEvaluator();
			AssertEquals("Nothing", evaluator.ReplaceMacros("Nothing"));
		}

		public void TestCanGetTranslatedValueFromMatchesFilter()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "LIGHT";

			var filterEvaluator = new FilterEvaluator(BODocDataProvider.Get(dummy), null);

			ZString translatedValue;
			filterEvaluator.MatchesFilter("'<Z0_VarCharMax>' == 'LIGHT'", out translatedValue);

			AssertEquals("'LIGHT' == 'LIGHT'", translatedValue);
		}
	}
}
