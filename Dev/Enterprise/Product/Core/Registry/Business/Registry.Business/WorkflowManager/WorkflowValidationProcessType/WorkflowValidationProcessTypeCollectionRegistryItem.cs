using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WorkflowValidationProcessTypeCollectionRegistryItem : StronglyTypedRegistryItem<WorkflowValidationProcessTypeCollection>
	{
		public WorkflowValidationProcessTypeCollectionRegistryItem
			(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WorkflowValidationProcessTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkflowValidationProcessTypeRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
