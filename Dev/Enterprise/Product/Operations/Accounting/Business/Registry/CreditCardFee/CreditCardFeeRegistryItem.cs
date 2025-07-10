using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CreditCardFeeRegistryItem : StronglyTypedRegistryItem<CreditCardFeeCollection>
	{
		public CreditCardFeeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CreditCardFeeRegistryDataType(), storage, RegistryOptions.IsHidden))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CreditCardFeeRegistryItemEditor, Enterprise.Accounting.GUI")]
	#if DEBUG
	public
	#endif
	class CreditCardFeeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CreditCardFeeCollection>
	{
		public CreditCardFeeRegistryDataType()
		{
		}
	}
}