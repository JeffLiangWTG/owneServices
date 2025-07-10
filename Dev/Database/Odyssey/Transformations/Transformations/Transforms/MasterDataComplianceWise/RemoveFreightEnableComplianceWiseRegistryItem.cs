using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	class RemoveFreightEnableComplianceWiseRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "FreightEnableComplianceWise" };
	}
}
