using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceTotalRoundingRegistryItem : StronglyTypedRegistryItem<InvoiceTotalRoundingCollection>
	{
		public InvoiceTotalRoundingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceTotalRoundingRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.InvoiceTotalRoundingRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class InvoiceTotalRoundingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceTotalRoundingCollection>
	{
	}
}