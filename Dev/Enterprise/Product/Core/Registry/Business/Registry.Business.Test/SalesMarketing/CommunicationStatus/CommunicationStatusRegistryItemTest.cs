using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommunicationStatusRegistryItem))]
	sealed class CommunicationStatusRegistryItemTest : StronglyTypedRegistryItemTestCase<CommunicationStatusCollection>
	{
		protected override StronglyTypedRegistryItem<CommunicationStatusCollection, CommunicationStatusCollection> GetNewRegistryItem()
		{
			return new CommunicationStatusRegistryItem("", null, null, null, RegistryStorageFlags.System, new CommunicationStatusRegistryEditorInfo(), new CommunicationStatusCollection());
		}
	}
}
