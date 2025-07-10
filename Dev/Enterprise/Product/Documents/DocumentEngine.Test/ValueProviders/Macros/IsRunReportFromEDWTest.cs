using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsRunReportFromEDW))]
	sealed class IsRunReportFromEDWTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<IsRunReportFromEDW>");
			AssertIsResponsibleForReplacing("< IsRunReportFromEDW>");
			AssertIsResponsibleForReplacing("<IsRunReportFromEDW >");
			AssertIsResponsibleForReplacing("<  IsRunReportFromEDW    >");
			AssertNotResponsibleForReplacing("<AutoHeight>");
			AssertNotResponsibleForReplacing("<Is Production System>");
		}

		public void TestReplacement()
		{
			AssertEquals("Report <IsRunReportFromEDW> should be [N].", "N", ValueProviderToTest.GetReplacement("<IsRunReportFromEDW>", Report));

			Report.IsEdwDataSource = ZBool.True;
			AssertEquals("Report <IsRunReportFromEDW> should be [Y].", "Y", ValueProviderToTest.GetReplacement("<IsRunReportFromEDW>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new IsRunReportFromEDW();
		}
	}
}
