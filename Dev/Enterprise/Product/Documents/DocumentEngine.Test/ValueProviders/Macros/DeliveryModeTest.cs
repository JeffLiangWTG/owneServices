using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DeliveryMode))]
	sealed class DeliveryModeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <Delivery Mode>", ValueProviderToTest.IsResponsibleForReplacing("<Delivery Mode>", Passes.FirstPass));
			Assert("should match < delivery      mode       >", ValueProviderToTest.IsResponsibleForReplacing("< delivery      mode       >", Passes.FirstPass));
			Assert("should match <DeliveryMode>", ValueProviderToTest.IsResponsibleForReplacing("<DeliveryMode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();

			Report.PrintCopyType = PrintCopyType.PRN;
			AssertEquals("PRN", ValueProviderToTest.GetReplacement("<DeliveryMode>", Report));

			Report.PrintCopyType = PrintCopyType.EML;
			AssertEquals("EML", ValueProviderToTest.GetReplacement("<DeliveryMode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DeliveryMode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.PrintCopyType = PrintCopyType.PRN;
		}
	}
}
