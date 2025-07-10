using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting;

class ClearRootFileExportPathRegistryItemValues : RegistryDataTransformation
{
	public override string UserDescription => "Clear saved values of ComplianceReportConfigurationRootFileExportPath registry item";

	protected override void OfflinePostUpgradeTransform()
		=> DeleteRegistryItemRows("ComplianceReportConfigurationRootFileExportPath");
}
