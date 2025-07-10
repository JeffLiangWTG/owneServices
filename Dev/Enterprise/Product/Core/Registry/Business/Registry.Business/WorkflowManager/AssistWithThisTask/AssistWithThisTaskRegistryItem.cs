using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AssistWithThisTaskRegistryItem : StronglyTypedRegistryItem<CategorisedAssistWithThisTaskSettingCollection>
	{
		public AssistWithThisTaskRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new CategorisedAssistWithThisTaskSettingCollection())
		{
		}

		public AssistWithThisTaskRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CategorisedAssistWithThisTaskSettingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AssistWithThisTaskRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WorkflowManagerAssistWithThisTaskRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AssistWithThisTaskRegistryDataType : NonPersistentBusinessObjectCollectionRegistryDataType<CategorisedAssistWithThisTaskSettingCollection>
	{
	}
}
