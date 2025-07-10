using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Dash.Business
{
	public class WorkflowConfigurationStepRegistryItem : StronglyTypedRegistryItem<WorkflowConfigurationStepCollection>
	{
		public WorkflowConfigurationStepRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			CodeDescriptionPairListProvider codesProvider)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkflowConfigurationStepCollectionRegistryDataType(codesProvider), storage))
		{
		}

		public WorkflowConfigurationStepRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			CodeDescriptionPairListProvider codesProvider,
			RegistryOptions options,
			WorkflowConfigurationStepCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WorkflowConfigurationStepCollectionRegistryDataType(codesProvider), storage, options, defaultValue))
		{
		}
	}
}
