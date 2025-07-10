using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ActivitySubtypeAssignmentsRegistryItem : StronglyTypedRegistryItem<ActivitySubtypeAssignmentCollection, ActivitySubtypeAssignmentCollection>
	{
		public ActivitySubtypeAssignmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
	: this(name, category, caption, hint, storage, new ActivitySubtypeAssignmentCollection())
		{
		}

		public ActivitySubtypeAssignmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ActivitySubtypeAssignmentCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ActivitySubtypeAssignmentsRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.ActivitySubtypeAssignmentsRegistryEditor, ZClientEDI")]
	public class ActivitySubtypeAssignmentsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ActivitySubtypeAssignmentCollection>
	{
		public ActivitySubtypeAssignmentsRegistryDataType()
		{
		}
	}
}
