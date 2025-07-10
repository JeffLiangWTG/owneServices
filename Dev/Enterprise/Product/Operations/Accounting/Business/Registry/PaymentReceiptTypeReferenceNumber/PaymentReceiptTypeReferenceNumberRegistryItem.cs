using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.ZArchitecture.Environment.RegistryItemImplWithDynamicDefaultValue;

namespace Enterprise.Accounting.Registry.Business
{
	public class PaymentReceiptTypeReferenceNumberRegistryItem : StronglyTypedRegistryItem<PaymentReceiptTypeReferenceNumberCollection>
	{
		public PaymentReceiptTypeReferenceNumberRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DefaultValueGetter defaultValueGetter)
			: base(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new PaymentReceiptTypeReferenceNumberRegistryDataType(), storage, options, defaultValueGetter))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.PaymentReceiptTypeReferenceNumberRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class PaymentReceiptTypeReferenceNumberRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PaymentReceiptTypeReferenceNumberCollection>
	{
		public PaymentReceiptTypeReferenceNumberRegistryDataType()
		{
		}
	}
}
