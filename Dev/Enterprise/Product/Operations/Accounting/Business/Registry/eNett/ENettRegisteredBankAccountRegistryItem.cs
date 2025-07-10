using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ENettRegisteredBankAccountRegistryItem : StronglyTypedRegistryItem<ENettRegisteredBankAccountCollection>
	{
		public ENettRegisteredBankAccountRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ENettRegisteredBankAccountRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ENettRegisteredBankAccountRegistryItemEditor, Enterprise.Accounting.GUI")]
	#if DEBUG
	public
	#endif
	class ENettRegisteredBankAccountRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ENettRegisteredBankAccountCollection>
	{
		public ENettRegisteredBankAccountRegistryDataType()
		{
		}
	}
}