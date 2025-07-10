using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreditReportItemCollectionRegistryItem))]
	sealed class CreditReportsRegistryItemTest : StronglyTypedRegistryItemTestCase<CreditReportItemCollection>
	{
		protected override StronglyTypedRegistryItem<CreditReportItemCollection, CreditReportItemCollection> GetNewRegistryItem()
		{
			var collection = new CreditReportItemCollection();
			return new CreditReportItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, collection);
		}
	}
}
