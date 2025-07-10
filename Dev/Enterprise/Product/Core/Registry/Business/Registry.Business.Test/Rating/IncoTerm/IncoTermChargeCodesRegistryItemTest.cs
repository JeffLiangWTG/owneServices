using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IncoTermChargeCodesRegistryItem))]
	sealed class IncoTermChargeCodesRegistryItemTest : StronglyTypedRegistryItemTestCase<IncoTermChargeCodesCollection>
	{
		protected override StronglyTypedRegistryItem<IncoTermChargeCodesCollection, IncoTermChargeCodesCollection> GetNewRegistryItem()
		{
			return new IncoTermChargeCodesRegistryItem("", null, null, null, RegistryStorageFlags.System, new IncoTermChargeCodesCollection());
		}
	}
}
