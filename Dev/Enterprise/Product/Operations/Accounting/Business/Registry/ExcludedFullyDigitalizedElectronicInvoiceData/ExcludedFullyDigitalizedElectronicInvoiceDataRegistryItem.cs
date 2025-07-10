using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem : StronglyTypedRegistryItem<ExcludedFullyDigitalizedElectronicInvoiceData>
	{
		public ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ExcludedFullyDigitalizedElectronicInvoiceData defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class ExcludedFullyDigitalizedElectronicInvoiceDataRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ExcludedFullyDigitalizedElectronicInvoiceData>
	{
	}
}
