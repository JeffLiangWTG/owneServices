using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class BackDateAPInvoicesConfigurationRegistryItem : StronglyTypedRegistryItem<BackDateAPInvoicesConfiguration>
	{
		public BackDateAPInvoicesConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BackDateAPInvoicesConfigurationRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.BackDateAPInvoicesConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class BackDateAPInvoicesConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BackDateAPInvoicesConfiguration>
	{
		public BackDateAPInvoicesConfigurationRegistryDataType()
		{
		}
	}
}
