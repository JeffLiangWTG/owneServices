using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ReportOrderRegistryItem))]
	class ReportOrderRegistryItemTest : StronglyTypedRegistryItemTestCase<ReportOrderCollection>
	{
		protected override StronglyTypedRegistryItem<ReportOrderCollection, ReportOrderCollection> GetNewRegistryItem()
		{
			return new ReportOrderRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
