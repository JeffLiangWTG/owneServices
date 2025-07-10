using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FormatCanadianTaxRate))]
	sealed class FormatCanadianTaxRateTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<FormatCanadianTaxRate(V, 1,233.5)>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("<FormatCanadianTaxRate(<Format>, <Value>)>", Passes.SecondPass);
		}

		public void TestGetReplacement()
		{
			AssertIsReplacedWith("5.7", "<FormatCanadianTaxRate(V, 5.66)>", Passes.SecondPass);
			AssertIsReplacedWith("5.0", "<FormatCanadianTaxRate(V, 5)>", Passes.SecondPass);
			AssertIsReplacedWith("5.6666", "<FormatCanadianTaxRate(S, 5.6666)>", Passes.SecondPass);
			AssertIsReplacedWith("5.00", "<FormatCanadianTaxRate(S, 5)>", Passes.SecondPass);
			AssertIsReplacedWith("5.00", "<FormatCanadianTaxRate(F, 5)>", Passes.SecondPass);
			AssertIsReplacedWith("", "<FormatCanadianTaxRate(F, 5a)>", Passes.SecondPass);
			AssertIsReplacedWith("", "<FormatCanadianTaxRate(F, 0)>", Passes.SecondPass);

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Format", "S"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Value", 5));
			Report.Renderer.CurrentPass = Passes.SecondPass;
			var areaToTest = new ConfigArea(4, 12, Report, "");
			AssertEquals("5.00", areaToTest.ReplaceMacros("<FormatCanadianTaxRate(<Format>, <Value>)>"));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FormatCanadianTaxRate();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			var areaToTest = new ConfigArea(4, 12, Report, "");
			AssertEquals(expectedResult.ToString(), areaToTest.ReplaceMacros(example));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("RateType", "S"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("RateValue", 5));
			Report.Renderer.CurrentPass = Passes.SecondPass;
		}
	}
}
