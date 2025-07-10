using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(GroupNotificationRegistryItem<GroupNotification>))]
	sealed class GroupNotificationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<GroupNotification>
	{
		protected override StronglyTypedRegistryItem<GroupNotification, GroupNotification> GetNewRegistryItem()
		{
			return new GroupNotificationRegistryItem<GroupNotification>("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, GroupNotification.Default);
		}
	}
}
