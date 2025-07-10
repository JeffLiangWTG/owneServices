using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Business
{
	public class GroupNotificationRegistryItem : StronglyTypedRegistryItem<GroupNotification>
	{
		public GroupNotificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, GroupNotification defaultValue) : base(new RegistryItemImpl(
				name: name,
				category: category,
				caption: caption,
				hint: hint,
				dataType: new GroupNotificationRegistryDataType(),
				storage: storage,
				options: options,
				defaultValue: defaultValue
		))
		{
		}

		[RegistryEditor("Enterprise.Customs.IE.GUI.GroupNotificationRegistryItemEditor, Enterprise.Customs.IE.GUI")]
		public class GroupNotificationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GroupNotification> { }
	}
}
