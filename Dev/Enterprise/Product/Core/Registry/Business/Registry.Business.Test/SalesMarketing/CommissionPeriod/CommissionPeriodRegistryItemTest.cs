using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CommissionPeriodListRegistryItem))]
	sealed class CommissionPeriodRegistryItemTest : StronglyTypedRegistryItemTestCase<CommissionPeriodCollection>
	{
		protected override StronglyTypedRegistryItem<CommissionPeriodCollection, CommissionPeriodCollection> GetNewRegistryItem()
		{
			return new CommissionPeriodListRegistryItem("", null, null, null, RegistryStorageFlags.System, new CommissionPeriodListRegistryEditorInfo(), new CommissionPeriodCollection());
		}
	}
}
