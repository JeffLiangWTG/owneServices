using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebThemeUrlRegistryItem))]
	sealed class WebThemeUrlRegistryItemTest : StronglyTypedRegistryItemTestCase<WebThemeUrlCollection>
	{
		protected override StronglyTypedRegistryItem<WebThemeUrlCollection, WebThemeUrlCollection> GetNewRegistryItem()
		{
			return new WebThemeUrlRegistryItem("", null, null, null, RegistryStorageFlags.System, new WebThemeUrlCollection());
		}
	}
}
