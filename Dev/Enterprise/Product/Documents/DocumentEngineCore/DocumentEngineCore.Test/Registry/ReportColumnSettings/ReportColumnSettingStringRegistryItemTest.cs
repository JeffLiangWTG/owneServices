using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(ReportColumnSettingStringRegistryItem))]
	class ReportColumnSettingStringRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new ReportColumnSettingStringRegistryItem(string.Empty);
		}

		public void TestRegistryItemOptions()
		{
			var registryItem = GetNewRegistryItem();
			AssertEquals(RegistryOptions.IsHidden | RegistryOptions.NotCached, registryItem.Options);
		}
	}
}
