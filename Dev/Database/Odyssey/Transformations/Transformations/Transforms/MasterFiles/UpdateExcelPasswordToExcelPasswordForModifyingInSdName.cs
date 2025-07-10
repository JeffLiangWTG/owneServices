using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	public class UpdateExcelPasswordToExcelPasswordForModifyingInSdName : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from ExcelPassword to ExcelPasswordForModifying";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("ExcelPassword", "ExcelPasswordForModifying");
		}
	}
}
