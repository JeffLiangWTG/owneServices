using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class InvoiceDetailGroupByRegistryItem : StronglyTypedRegistryItem<InvoiceDetailGroupBy>
	{
		public InvoiceDetailGroupByRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, InvoiceDetailGroupBy defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceDetailGroupByRegistryDataType(), storage, defaultValue))
		{
		}

		public InvoiceDetailGroupByRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, InvoiceDetailGroupBy defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceDetailGroupByRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.InvoiceDetailGroupByRegistryItemEditor, Enterprise.Registry.GUI")]
	class InvoiceDetailGroupByRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceDetailGroupBy>
	{
		public InvoiceDetailGroupByRegistryDataType()
		{
		}
	}
}
