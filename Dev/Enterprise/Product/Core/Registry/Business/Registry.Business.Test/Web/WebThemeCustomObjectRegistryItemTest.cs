using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebThemeCustomObjectRegistryItem))]
	sealed class WebThemeCustomObjectRegistryItemTest : StronglyTypedRegistryItemTestCase<WebThemeCustomObjectCollection>
	{
		protected override StronglyTypedRegistryItem<WebThemeCustomObjectCollection, WebThemeCustomObjectCollection> GetNewRegistryItem()
		{
			return new WebThemeCustomObjectRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, new WebThemeCustomObjectCollection());
		}
	}
}
