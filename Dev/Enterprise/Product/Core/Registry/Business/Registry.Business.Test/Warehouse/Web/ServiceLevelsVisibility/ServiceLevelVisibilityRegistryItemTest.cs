using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ServiceLevelVisibilityRegistryItem))]
	sealed class ServiceLevelVisibilityRegistryItemTest : StronglyTypedRegistryItemTestCase<RegistryServiceLevelCollection>
	{
		public void TestDescriptionColumnTranslatable()
		{
			var registryItem = GetNewRegistryItem();
			Assert(((CodeDescriptionBoolRegistryEditorInfo)registryItem.EditorInfo).IsDescriptionColumnTranslatable);
		}

		protected override StronglyTypedRegistryItem<RegistryServiceLevelCollection, RegistryServiceLevelCollection> GetNewRegistryItem()
		{
			return new ServiceLevelVisibilityRegistryItem(string.Empty, null, null, null, RegistryOptions.Default);
		}
	}
}
