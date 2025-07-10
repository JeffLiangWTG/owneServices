using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(KafkaSecurityRegistryItem))]
	sealed class KafkaSecurityRegistryItemTest : StronglyTypedRegistryItemTestCase<KafkaSecurity>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<KafkaSecurity, KafkaSecurity> GetNewRegistryItem()
		{
			return new KafkaSecurityRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
