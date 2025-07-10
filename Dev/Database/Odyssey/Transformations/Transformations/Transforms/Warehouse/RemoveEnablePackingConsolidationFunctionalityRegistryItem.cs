using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse
{
	public class RemoveEnablePackingConsolidationFunctionalityRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "EnablePackingConsolidationFunctionality" };
	}
}
