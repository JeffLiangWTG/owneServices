using System.IO;
using TestCaseBaseClass = NUnit.Framework.TestCase;

namespace CWNUnit.TestAdapter.Tests
{
	class TestOptionsManagerTestCase : TestCaseBaseClass
	{
		public void TestLoadOptions_InvalidPath_ReturnsDefaultValues()
		{
			var path = "wrong";
			var optionsFileName = TestOptionsManager.OptionsFileName;
			var testOptions = TestOptionsManager.LoadOptions(path, optionsFileName);
			CombineAssertions(() =>
			{
				AssertEquals("Enabled must be correct", true, testOptions.Enabled);
				AssertEquals("DatabaseName must be correct", "Odyssey", testOptions.DatabaseName);
				AssertEquals("BreakOnPerformanceIssues must be correct", false, testOptions.BreakOnPerformanceIssues);
				AssertEquals("IncludeAmnestyTests must be correct", true, testOptions.IncludeAmnestyTests);
				AssertEquals("IncludeDeveloperOnlyTests must be correct", true, testOptions.IncludeDeveloperOnlyTests);
				AssertEquals("EnableTaskTestListener must be correct", false, testOptions.EnableTaskTestListener);
			});
		}

		public void TestLoadOptions_InvalidOptionsFileName_ReturnsDefaultValues()
		{
			var path = Path.GetDirectoryName(typeof(TestOptionsManager).Assembly.Location);
			var optionsFileName = "wrong";
			var testOptions = TestOptionsManager.LoadOptions(path, optionsFileName);
			CombineAssertions(() =>
			{
				AssertEquals("Enabled must be correct", true, testOptions.Enabled);
				AssertEquals("DatabaseName must be correct", "Odyssey", testOptions.DatabaseName);
				AssertEquals("BreakOnPerformanceIssues must be correct", false, testOptions.BreakOnPerformanceIssues);
				AssertEquals("IncludeAmnestyTests must be correct", true, testOptions.IncludeAmnestyTests);
				AssertEquals("IncludeDeveloperOnlyTests must be correct", true, testOptions.IncludeDeveloperOnlyTests);
				AssertEquals("EnableTaskTestListener must be correct", false, testOptions.EnableTaskTestListener);
			});
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadOptions_InvalidJson_ReturnsDefaultValues()
		{
			var path = BaseSourcePath + @"devtools\unittestadapter\UnitTestAdapter.Tests";
			var optionsFileName = "TestOptionsInvalidJson.json";
			var testOptions = TestOptionsManager.LoadOptions(path, optionsFileName);
			CombineAssertions(() =>
			{
				AssertEquals("Enabled must be correct", true, testOptions.Enabled);
				AssertEquals("DatabaseName must be correct", "Odyssey", testOptions.DatabaseName);
				AssertEquals("BreakOnPerformanceIssues must be correct", false, testOptions.BreakOnPerformanceIssues);
				AssertEquals("IncludeAmnestyTests must be correct", true, testOptions.IncludeAmnestyTests);
				AssertEquals("IncludeDeveloperOnlyTests must be correct", true, testOptions.IncludeDeveloperOnlyTests);
				AssertEquals("EnableTaskTestListener must be correct", false, testOptions.EnableTaskTestListener);
			});
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadOptions_ValidJson_ReturnsCorrectValues()
		{
			var path = BaseSourcePath + @"devtools\unittestadapter\UnitTestAdapter.Tests";
			var optionsFileName = "TestOptionsValidJson.json";
			var testOptions = TestOptionsManager.LoadOptions(path, optionsFileName);
			CombineAssertions(() =>
			{
				AssertEquals("Enabled must be correct (value false)", false, testOptions.Enabled);
				AssertEquals("DatabaseName must be correct", "OdysseyCorrect", testOptions.DatabaseName);
				AssertEquals("BreakOnPerformanceIssues must be correct (value false)", false, testOptions.BreakOnPerformanceIssues);
				AssertEquals("IncludeAmnestyTests must be correct (value false)", false, testOptions.IncludeAmnestyTests);
				AssertEquals("IncludeDeveloperOnlyTests must be correct (value false)", false, testOptions.IncludeDeveloperOnlyTests);
				AssertEquals("EnableTaskTestListener must be correct (value false)", false, testOptions.EnableTaskTestListener);
			});
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadOptions_ValidJson_ReturnsCorrectValues_BooleansTrue()
		{
			var path = BaseSourcePath + @"devtools\unittestadapter\UnitTestAdapter.Tests";
			var optionsFileName = "TestOptionsValidJsonBooleansTrue.json";
			var testOptions = TestOptionsManager.LoadOptions(path, optionsFileName);
			CombineAssertions(() =>
			{
				AssertEquals("Enabled must be correct (value true)", true, testOptions.Enabled);
				AssertEquals("DatabaseName must be correct", "OdysseyCorrect", testOptions.DatabaseName);
				AssertEquals("BreakOnPerformanceIssues must be correct (value true)", true, testOptions.BreakOnPerformanceIssues);
				AssertEquals("IncludeAmnestyTests must be correct (value true)", true, testOptions.IncludeAmnestyTests);
				AssertEquals("IncludeDeveloperOnlyTests must be correct (value true)", true, testOptions.IncludeDeveloperOnlyTests);
				AssertEquals("EnableTaskTestListener must be correct (value true)", true, testOptions.EnableTaskTestListener);
			});
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadOptions_NoValues_ValuesNotSet()
		{
			var path = BaseSourcePath + @"devtools\unittestadapter\UnitTestAdapter.Tests";
			var optionsFileName = "TestOptionsValidJsonNoValues.json";
			var testOptions = TestOptionsManager.LoadOptions(path, optionsFileName);
			CombineAssertions(() =>
			{
				AssertEquals("Enabled is not set a value", false, testOptions.Enabled);
				AssertEquals("DatabaseName is not set a value", null, testOptions.DatabaseName);
				AssertEquals("BreakOnPerformanceIssues is not set a value", false, testOptions.BreakOnPerformanceIssues);
				AssertEquals("IncludeAmnestyTests is not set a value", false, testOptions.IncludeAmnestyTests);
				AssertEquals("IncludeDeveloperOnlyTests is not set a value", false, testOptions.IncludeDeveloperOnlyTests);
				AssertEquals("EnableTaskTestListener is not set a value", false, testOptions.EnableTaskTestListener);
			});
		}
	}
}
