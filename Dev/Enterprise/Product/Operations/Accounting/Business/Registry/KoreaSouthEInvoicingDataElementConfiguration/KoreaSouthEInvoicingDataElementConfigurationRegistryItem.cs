using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class KoreaSouthEInvoicingDataElementConfigurationRegistryItem : StronglyTypedRegistryItem<KoreaSouthEInvoicingDataElementConfigurationCollection>
	{
		public KoreaSouthEInvoicingDataElementConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, KoreaSouthEInvoicingDataElementConfigurationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new KoreaSouthEInvoicingDataElementConfigurationDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.KoreaSouthEInvoicingDataElementConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class KoreaSouthEInvoicingDataElementConfigurationDataType : NonPersistentBusinessObjectRegistryDataType<KoreaSouthEInvoicingDataElementConfigurationCollection>
	{
		public KoreaSouthEInvoicingDataElementConfigurationDataType()
		{
		}
	}
}
