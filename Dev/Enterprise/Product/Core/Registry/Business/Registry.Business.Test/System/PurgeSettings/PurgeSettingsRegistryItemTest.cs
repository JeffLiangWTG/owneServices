using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PurgeSettingsRegistryItem))]
	sealed class PurgeSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<PurgeSettings>
	{
		public void TestProperties()
		{
			AssertNotNull(Item.DefaultValue);
			AssertEquals(typeof(PurgeSettingsRegistryDataType), Item.DataType.GetType());
		}

		protected override StronglyTypedRegistryItem<PurgeSettings, PurgeSettings> GetNewRegistryItem()
		{
			return new PurgeSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, new PurgeSettings(new PurgeSettingsRegistryDataType()));
		}
	}
}
