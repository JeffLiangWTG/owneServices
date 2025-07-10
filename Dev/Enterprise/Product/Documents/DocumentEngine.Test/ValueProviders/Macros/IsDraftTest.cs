using System;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsDraft))]
	sealed class IsDraftTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<IsDraft>");
			AssertIsResponsibleForReplacing("< IsDraft>");
			AssertIsResponsibleForReplacing("<IsDraft >");
			AssertIsResponsibleForReplacing("<  IsDraft    >");
			AssertNotResponsibleForReplacing("<AutoHeight>");
			AssertNotResponsibleForReplacing("<Is Draft>");
		}

		public void TestReplacement()
		{
			Report.Parent.DeliveryInstructions.IsDraft = false;
			PrepareRenderer();
			AssertEquals("Report <IsDraft> should be [N].", "N", ValueProviderToTest.GetReplacement("<IsDraft>", Report));

			Report.Parent.DeliveryInstructions.IsDraft = true;
			PrepareRenderer();
			AssertEquals("Report <IsDraft> should be [Y].", "Y", ValueProviderToTest.GetReplacement("<IsDraft>", Report));

			Report report = new Report(null, null);
			AssertEquals("Report <IsDraft> should be [N].", "N", ValueProviderToTest.GetReplacement("<IsDraft>", report));

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ ValueProviderToTest.GetReplacement("<IsDraft>", null); });
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new IsDraft();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.Parent.DeliveryInstructions.IsDraft = false;
			PrepareRenderer();
		}
	}
}
