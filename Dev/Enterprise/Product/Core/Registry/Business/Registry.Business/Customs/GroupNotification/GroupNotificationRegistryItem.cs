using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	public class GroupNotificationRegistryItem<T> : StronglyTypedRegistryItem<T> where T : GroupNotification
	{
		public GroupNotificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, T defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GroupNotificationRegistryDataType<T>(), storage, options, defaultValue))
		{
		}

		public GroupNotificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, T defaultValue, bool doNotSendStaffMembers)
			: base(new RegistryItemImpl(name, category, caption, hint, new GroupNotificationRegistryDataType<T>(doNotSendStaffMembers), storage, options, defaultValue))
		{
		}
	}
}
