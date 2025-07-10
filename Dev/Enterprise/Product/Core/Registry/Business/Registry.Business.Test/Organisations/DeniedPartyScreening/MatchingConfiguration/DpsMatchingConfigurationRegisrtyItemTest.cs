using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsMatchingConfigurationRegisrtyItem))]
	sealed class DpsMatchingConfigurationRegisrtyItemTest : StronglyTypedRegistryItemTestCase<DpsMatchingConfigurationBusinessObject>
	{
		protected override StronglyTypedRegistryItem<DpsMatchingConfigurationBusinessObject, DpsMatchingConfigurationBusinessObject> GetNewRegistryItem()
		{
			return new DpsMatchingConfigurationRegisrtyItem("name", null, (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new DpsMatchingConfigurationBusinessObject());
		}
	}
}
