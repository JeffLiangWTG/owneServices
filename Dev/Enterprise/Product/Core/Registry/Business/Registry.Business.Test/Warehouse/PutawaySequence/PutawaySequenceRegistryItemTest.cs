using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PutawaySequenceRegistryItem))]
	sealed class PutawaySequenceRegistryItemTest : StronglyTypedRegistryItemTestCase<PutawaySequence, IPutawaySequence>
	{
		protected override StronglyTypedRegistryItem<PutawaySequence, IPutawaySequence> GetNewRegistryItem()
		{
			return new PutawaySequenceRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
