using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ScavengingPurgeSettingsRegistryItem))]
	class ScavengingPurgeSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<ScavengingPurgeSettings>
	{
		protected override StronglyTypedRegistryItem<ScavengingPurgeSettings, ScavengingPurgeSettings> GetNewRegistryItem()
		{
			return new ScavengingPurgeSettingsRegistryItem("Category");
		}
	}
}
