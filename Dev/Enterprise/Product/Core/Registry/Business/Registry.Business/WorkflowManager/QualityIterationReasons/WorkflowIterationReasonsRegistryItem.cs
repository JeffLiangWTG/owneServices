using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WorkflowIterationReasonsRegistryItem : StronglyTypedRegistryItem<CategorisedWorkflowIterationReasonsCollection>
	{
		public WorkflowIterationReasonsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new CategorisedWorkflowIterationReasonsCollection())
		{
		}

		public WorkflowIterationReasonsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CategorisedWorkflowIterationReasonsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkflowIterationReasonsRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WorkflowManagerIterationReasonsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class WorkflowIterationReasonsRegistryDataType : NonPersistentBusinessObjectCollectionRegistryDataType<CategorisedWorkflowIterationReasonsCollection>
	{
	}
}
