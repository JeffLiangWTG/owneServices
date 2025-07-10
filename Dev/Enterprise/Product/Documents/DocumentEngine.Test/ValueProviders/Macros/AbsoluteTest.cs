using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Absolute))]
	sealed class AbsoluteTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Absolute(12, USD)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Absolute(12)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Absolute(-12)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Absolute(12.345)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<Absolute(-12.345)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals(100m, ValueProviderToTest.GetReplacement("<Absolute(100)>", Report));
			AssertEquals(100m, ValueProviderToTest.GetReplacement("<Absolute(-100)>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Absolute();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("ARInvoice.OSTotal", -50));
		}
	}
}
