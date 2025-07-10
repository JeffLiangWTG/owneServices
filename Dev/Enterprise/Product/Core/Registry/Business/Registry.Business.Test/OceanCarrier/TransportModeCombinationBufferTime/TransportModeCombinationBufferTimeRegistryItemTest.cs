using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportModeCombinationBufferTimeRegistryItem))]
	sealed class TransportModeCombinationBufferTimeRegistryItemTest : StronglyTypedRegistryItemTestCase<TransportModeCombinationBufferTimeCollection>
	{
		protected override StronglyTypedRegistryItem<TransportModeCombinationBufferTimeCollection, TransportModeCombinationBufferTimeCollection> GetNewRegistryItem()
		{
			return new TransportModeCombinationBufferTimeRegistryItem("1", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, TransportModeCombinationBufferTimeCollection.DefaultValue);
		}
	}
}
