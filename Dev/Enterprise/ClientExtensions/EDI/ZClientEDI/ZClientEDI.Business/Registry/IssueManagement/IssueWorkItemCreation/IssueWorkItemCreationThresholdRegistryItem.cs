using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI
{
	public class IssueWorkItemCreationThresholdRegistryItem : StronglyTypedRegistryItem<IssueWorkItemCreationThresholdCollection>
	{
		public IssueWorkItemCreationThresholdRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, new IssueWorkItemCreationThresholdCollection())
		{
		}

		public IssueWorkItemCreationThresholdRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, IssueWorkItemCreationThresholdCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IssueWorkItemCreationThresholdRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.IssueWorkItemCreationThresholdRegistryEditor, ZClientEDI")]
		#if DEBUG
		public
		#endif
		class IssueWorkItemCreationThresholdRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IssueWorkItemCreationThresholdCollection>
		{
		}
	}
}

