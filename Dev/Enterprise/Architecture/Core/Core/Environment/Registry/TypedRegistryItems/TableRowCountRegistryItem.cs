
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class TableRowCountRegistryItem : StronglyTypedRegistryItem<TableRowCountDataType, TableRowCountDataType>
	{
		public TableRowCountRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, TableRowCountDataType defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TableRowCountRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
