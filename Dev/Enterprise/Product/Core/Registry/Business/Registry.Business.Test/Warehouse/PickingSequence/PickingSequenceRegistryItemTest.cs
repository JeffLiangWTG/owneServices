using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickingSequenceRegistryItem))]
	sealed class PickingSequenceRegistryItemTest : StronglyTypedRegistryItemTestCase<PickingSequence, IPickingSequence>
	{
		protected override StronglyTypedRegistryItem<PickingSequence, IPickingSequence> GetNewRegistryItem()
		{
			return new PickingSequenceRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
