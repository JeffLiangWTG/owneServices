using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business.Test
{
	class ModuleGridSectionConfigurationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSectionNameOverrideShouldBeDefinedWhenOverridden()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			var modSection = BMSTestHelper.CreateBoardSection(ModuleIDs.WorkItem, board);
			var config = (ModuleGridSectionConfiguration)modSection.Configuration;

			config.SectionNameIsOverridden = true;

			config.SectionNameOverride = ZString.Empty;
			AssertHasError("There should be an error when section name is overridden, but section name override is empty", config.SectionNameOverrideInfo, "Custom section name cannot be empty when overriding section name.");

			config.SectionNameOverride = "   ";
			AssertHasError("There should be an error when section name is overridden, but section name override is a whitespace", config.SectionNameOverrideInfo, "Custom section name cannot be empty when overriding section name.");

			config.SectionNameOverride = "I'll override you!";
			AssertNoNotifications("There should not be any warnings or errors section name is overridden with a non-empty string", config.SectionNameOverrideInfo);
		}
	}
}
