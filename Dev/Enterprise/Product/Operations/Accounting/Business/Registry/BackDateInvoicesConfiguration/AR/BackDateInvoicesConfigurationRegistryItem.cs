using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class BackDateInvoicesConfigurationRegistryItem : StronglyTypedRegistryItem<BackDateInvoicesConfiguration>
	{
		public BackDateInvoicesConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BackDateInvoicesConfigurationRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.BackDateInvoicesConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class BackDateInvoicesConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BackDateInvoicesConfiguration>
	{
		public BackDateInvoicesConfigurationRegistryDataType()
		{
		}
	}
}
