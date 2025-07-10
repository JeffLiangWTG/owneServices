using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class TransactionTypePrefixRegistryItem : StronglyTypedRegistryItem<TransactionTypePrefixCollection>
	{
		public TransactionTypePrefixRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new TransactionTypePrefixRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.TransactionTypePrefixRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class TransactionTypePrefixRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TransactionTypePrefixCollection>
	{
		public TransactionTypePrefixRegistryDataType()
		{
		}
	}
}
