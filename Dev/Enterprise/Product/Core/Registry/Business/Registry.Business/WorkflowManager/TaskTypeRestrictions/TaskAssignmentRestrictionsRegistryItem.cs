using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class TaskAssignmentRestrictionsRegistryItem : StronglyTypedRegistryItem<TaskTypeRestrictionsCollection>
	{
		public TaskAssignmentRestrictionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new TaskTypeRestrictionsCollection())
		{
		}

		public TaskAssignmentRestrictionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TaskTypeRestrictionsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TaskAssignmentRestrictionsRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WorkflowManagerTaskTypeRestrictionsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class TaskAssignmentRestrictionsRegistryDataType : NonPersistentBusinessObjectCollectionRegistryDataType<TaskTypeRestrictionsCollection>
	{
		public TaskAssignmentRestrictionsRegistryDataType()
		{
		}
	}
}
