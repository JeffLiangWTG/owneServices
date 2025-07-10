using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebCustomImagesRegistryItem))]
	sealed class WebCustomImagesRegistryItemTest : StronglyTypedRegistryItemTestCase<WebTrackerCustomImage[]>
	{
		protected override StronglyTypedRegistryItem<WebTrackerCustomImage[], WebTrackerCustomImage[]> GetNewRegistryItem()
		{
			return new WebCustomImagesRegistryItem("a", null, (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}
	}
}
