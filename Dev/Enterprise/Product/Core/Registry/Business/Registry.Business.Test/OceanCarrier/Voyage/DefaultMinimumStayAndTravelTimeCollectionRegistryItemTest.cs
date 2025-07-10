using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultMinimumStayAndTravelTimeCollectionRegistryItem))]
	sealed class DefaultMinimumStayAndTravelTimeCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultMinimumStayAndTravelTimeCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultMinimumStayAndTravelTimeCollection, DefaultMinimumStayAndTravelTimeCollection> GetNewRegistryItem()
		{
			return new DefaultMinimumStayAndTravelTimeCollectionRegistryItem("1", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, DefaultMinimumStayAndTravelTimeCollection.DefaultValue);
		}
	}
}
