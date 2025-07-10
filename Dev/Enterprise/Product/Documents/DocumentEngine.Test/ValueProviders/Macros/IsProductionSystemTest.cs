using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsProductionSystem))]
	sealed class IsProductionSystemTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<IsProductionSystem>");
			AssertIsResponsibleForReplacing("< IsProductionSystem>");
			AssertIsResponsibleForReplacing("<IsProductionSystem >");
			AssertIsResponsibleForReplacing("<  IsProductionSystem    >");
			AssertNotResponsibleForReplacing("<AutoHeight>");
			AssertNotResponsibleForReplacing("<Is Production System>");
		}

		public void TestReplacement()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			AssertEquals("Report <IsProductionSystem> should be [N].", "N", ValueProviderToTest.GetReplacement("<IsProductionSystem>", Report));

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			AssertEquals("Report <IsProductionSystem> should be [Y].", "Y", ValueProviderToTest.GetReplacement("<IsProductionSystem>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new IsProductionSystem();
		}
	}
}
