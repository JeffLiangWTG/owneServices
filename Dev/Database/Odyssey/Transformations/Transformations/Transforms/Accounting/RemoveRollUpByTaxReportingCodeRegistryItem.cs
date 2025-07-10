using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	class RemoveRollUpByTaxReportingCodeRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Remove China's Golden Tax Invoice Roll Up Configuration Registry Item";

		protected override void OfflinePostUpgradeTransform()
			=> DeleteRegistryItemRows("RollUpByTaxReportingCode");
	}
}
