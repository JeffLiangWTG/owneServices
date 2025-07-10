using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebThemeRegistryItem))]
	sealed class WebThemeRegistryItemTest : StronglyTypedRegistryItemTestCase<WebTrackerTheme[]>
	{
		protected override StronglyTypedRegistryItem<WebTrackerTheme[], WebTrackerTheme[]> GetNewRegistryItem()
		{
			return new WebThemeRegistryItem("a", null, (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}
	}
}
