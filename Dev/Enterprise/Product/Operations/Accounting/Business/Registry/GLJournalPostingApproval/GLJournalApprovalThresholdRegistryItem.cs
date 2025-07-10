using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class GLJournalApprovalThresholdRegistryItem : StronglyTypedRegistryItem<GLJournalApprovalThresholdCollection>
	{
		public GLJournalApprovalThresholdRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, GLJournalApprovalThresholdCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GLJournalApprovalThresholdRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}

		public GLJournalApprovalThresholdRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options,  GLJournalApprovalThresholdCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GLJournalApprovalThresholdRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.GLJournalApprovalThresholdRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class GLJournalApprovalThresholdRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GLJournalApprovalThresholdCollection>
	{
		public GLJournalApprovalThresholdRegistryDataType()
		{
		}
	}
}
