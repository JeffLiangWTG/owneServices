using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebCustomCssRegistryItem))]
	sealed class WebCustomCssRegistryItemTest : StronglyTypedRegistryItemTestCase<WebTrackerCustomCss[]>
	{
		protected override StronglyTypedRegistryItem<WebTrackerCustomCss[], WebTrackerCustomCss[]> GetNewRegistryItem()
		{
			return new WebCustomCssRegistryItem("a", null, (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}
	}
}
