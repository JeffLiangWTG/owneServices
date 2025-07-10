using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ModuleGridSectionPanelConfigurationCollection))]
	class ModuleGridSectionPanelConfigurationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ModuleGridSectionPanelConfigurationCollection>
	{
		protected override ModuleGridSectionPanelConfigurationCollection GetCollectionToTest()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			return new ModuleGridSectionPanelConfigurationCollection(section);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var section = Factory.LoadTop1<BMBoardSection>(new ZQuery());
			return new ModuleGridSectionPanelConfiguration(section);
		}
	}
}
