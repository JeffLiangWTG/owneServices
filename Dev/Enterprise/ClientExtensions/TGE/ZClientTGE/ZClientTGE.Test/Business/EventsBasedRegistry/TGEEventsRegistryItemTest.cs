using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TGE.Business.Testing
{
	[TestedType(typeof(TGEEventsRegistryItem))]
	internal class TGEEventsRegistryItemTest : StronglyTypedRegistryItemTestCase<TGEEventRegistryBusinessObjectCollection>
	{
		protected override StronglyTypedRegistryItem<TGEEventRegistryBusinessObjectCollection, TGEEventRegistryBusinessObjectCollection> GetNewRegistryItem()
		{
			return new TGEEventsRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}
	}
}
