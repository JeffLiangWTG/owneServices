using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class EInvoicingReceivingFileTypeConfigurationRegistryItem : StronglyTypedRegistryItem<EInvoicingReceivingFileTypeConfigurationCollection>
	{
		public EInvoicingReceivingFileTypeConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
		: base(new RegistryItemImpl(name, category, caption, hint, new EInvoicingReceivingFileTypeConfigurationRegistryDataType(), storage, option))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.EInvoicingReceivingFileTypeConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class EInvoicingReceivingFileTypeConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EInvoicingReceivingFileTypeConfigurationCollection>
	{
	}
}
