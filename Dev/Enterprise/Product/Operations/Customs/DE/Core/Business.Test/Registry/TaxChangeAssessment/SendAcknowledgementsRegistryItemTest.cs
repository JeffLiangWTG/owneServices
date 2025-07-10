using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Registry.Testing
{
	[TestedType(typeof(SendAcknowledgementsRegistryItem))]
	class SendAcknowledgementsRegistryItemTest : StronglyTypedRegistryItemTestCase<SendAcknowledgementsRegistryCollection>
	{
		protected override StronglyTypedRegistryItem<SendAcknowledgementsRegistryCollection, SendAcknowledgementsRegistryCollection> GetNewRegistryItem()
			=> new SendAcknowledgementsRegistryItem("", null, null, null, RegistryStorageFlags.All);
	}
}
