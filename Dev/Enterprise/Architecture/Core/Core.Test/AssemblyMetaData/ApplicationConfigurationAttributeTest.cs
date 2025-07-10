using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test
{
	[TestedType(typeof(ApplicationConfigurationAttribute))]
	sealed class ApplicationConfigurationAttributeTest : AssemblyMetaDataAttributeTestCase<ApplicationConfigurationAttribute>
	{
		public void TestEquals_AllPropertiesInEquals()
		{
			var attribute1 = GetAssemblyMetaDataAttributeForTesting();
			var attribute2 = GetAssemblyMetaDataAttributeForTesting();
			Assert(attribute1.Equals(attribute2));

			attribute1.AssemblyName = "AssemblyName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.AssemblyName = "AssemblyName";
			Assert(attribute1.Equals(attribute2));

			attribute1.ResourceName = "ResourceName";
			Assert(!attribute1.Equals(attribute2));

			attribute2.ResourceName = "ResourceName";
			Assert(attribute1.Equals(attribute2));
		}
	}
}
