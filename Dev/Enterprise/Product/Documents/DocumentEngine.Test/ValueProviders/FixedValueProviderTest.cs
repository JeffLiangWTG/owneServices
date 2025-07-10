using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueReplacers.Testing
{
	[TestedType(typeof(FixedValueProvider))]
	sealed class FixedValueProviderTest : ValueProviderTest
	{
		public override void TestProviderResetProperlyAndShouldNotHoldData()
		{
			Assert(true);//FixedValueProviders will not be shared between different reports because they are not in ValueProviderCollector
		}

		public void TestIsResponsible()
		{
			FixedValueProvider testFixedValueProvider = new FixedValueProvider("Test", 12);
			Assert(testFixedValueProvider.IsResponsibleForReplacing("<test>", Passes.FirstPass));
			Assert(!testFixedValueProvider.IsResponsibleForReplacing("<test1>", Passes.FirstPass));
			Assert(!testFixedValueProvider.IsResponsibleForReplacing("<te st>", Passes.FirstPass));
		}

		public void TestIsResponsibleSecondPass()
		{
			FixedValueProvider testFixedValueProvider = new FixedValueProvider("Test", 12, Passes.SecondPass);
			Assert(!testFixedValueProvider.IsResponsibleForReplacing("<test>", Passes.FirstPass));
			Assert(testFixedValueProvider.IsResponsibleForReplacing("<test>", Passes.SecondPass));
		}

		public void TestGetReplacement()
		{
			FixedValueProvider testFixedValueProvider = new FixedValueProvider("Test", 12);
			AssertEquals(12, testFixedValueProvider.GetReplacement("<test>", new Report(null, null)));
		}

		public override void TestTemplateVisualiserComponentType()
		{
			FixedValueProvider testFixedValueProvider = new FixedValueProvider("Test", 12, Passes.SecondPass);
			AssertEquals(VisualiserComponentTypes.TextEdit, testFixedValueProvider.ComponentType);
		}

		public void TestRegex()
		{
			var testFixedValueProvider = new FixedValueProvider("Consignor - Shipper", 12, Passes.SecondPass);
			AssertEquals(@"^<(?:[\s]*)Consignor\ -\ Shipper(?:[\s]*)>$", testFixedValueProvider.Regex.ToString());
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new FixedValueProvider("Test", "Test");
		}

		public override void TestDocumentation()
		{
			AssertExceptionThrown(typeof(NotImplementedException), () => { base.TestDocumentation(); });
		}

		public override void TestExistsInValueProviderCollection()
		{
			Assert(true);//FixedValueProvider is not added to ValueProviderCollector
		}
	}
}
