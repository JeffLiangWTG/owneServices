using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(TableRowCountRegistryItem))]
	sealed class TableRowCountRegistryItemTest : StronglyTypedRegistryItemTestCase<TableRowCountDataType>
	{
		protected override StronglyTypedRegistryItem<TableRowCountDataType, TableRowCountDataType> GetNewRegistryItem()
		{
			return new TableRowCountRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden, new TableRowCountDataType());
		}
	}
}
