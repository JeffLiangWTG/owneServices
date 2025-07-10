using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse
{
	public class RemoveEnableOrderLinePalletIDEntryRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnableOrderLinePalletIDEntry" };
	}
}
