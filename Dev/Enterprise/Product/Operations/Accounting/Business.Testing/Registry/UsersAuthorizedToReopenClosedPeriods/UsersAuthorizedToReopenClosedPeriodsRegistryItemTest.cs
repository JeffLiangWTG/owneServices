using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(UsersAuthorizedToReopenClosedPeriodsRegistryItem))]
	class UsersAuthorizedToReopenClosedPeriodsRegistryItemTest : StronglyTypedRegistryItemTestCase<UsersAuthorizedToReopenClosedPeriodsCollection>
	{
		protected override StronglyTypedRegistryItem<UsersAuthorizedToReopenClosedPeriodsCollection, UsersAuthorizedToReopenClosedPeriodsCollection> GetNewRegistryItem()
		{
			return new UsersAuthorizedToReopenClosedPeriodsRegistryItem("", null, null, null, RegistryStorageFlags.Company, true);
		}
	}
}
