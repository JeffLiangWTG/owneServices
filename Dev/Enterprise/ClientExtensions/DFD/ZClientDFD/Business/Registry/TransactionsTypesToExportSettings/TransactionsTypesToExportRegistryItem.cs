using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD.Registry
{
	internal class TransactionsTypesToExportRegistryItem : StronglyTypedRegistryItem<TransactionsTypesToExportBusinessObject>
	{
		public TransactionsTypesToExportRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new TransactionsTypesToExportDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.DFD.Registry.TransactionsTypesToExportRegistryItemEditor, ZClientDFD")] // assembly path, assembly name
	class TransactionsTypesToExportDataType : NonPersistentBusinessObjectRegistryDataType<TransactionsTypesToExportBusinessObject>
	{
	}
}
