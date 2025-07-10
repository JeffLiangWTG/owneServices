using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowCategoriesRegistryItem : StronglyTypedRegistryItem<CategorisedWorkflowCategoriesCollection>
	{
		public WorkflowCategoriesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new CategorisedWorkflowCategoriesCollection())
		{
		}

		public WorkflowCategoriesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CategorisedWorkflowCategoriesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkflowCategoriesRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.BufferManagement.GUI.WorkflowCategoriesRegistryItemEditor, Enterprise.BufferManagement.GUI")]
	public class WorkflowCategoriesRegistryDataType : NonPersistentBusinessObjectCollectionRegistryDataType<CategorisedWorkflowCategoriesCollection>
	{
	}
}

