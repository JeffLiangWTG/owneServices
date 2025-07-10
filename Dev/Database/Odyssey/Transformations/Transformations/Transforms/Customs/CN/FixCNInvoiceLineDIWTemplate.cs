using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN
{
	public class FixCNInvoiceLineDIWTemplate : DataTransformation
	{
		public override string UserDescription => "Fix CN's Invoice Line DIW";

		protected override void OfflinePostUpgradeTransform()
		{
			DIWTemplateUpdateHelper.UpdateTemplateMappingPrefix("XC", "JI", "CN", ExportInvoiceLineDIW);
		}

		public const string ExportInvoiceLineDIW = "DIW:GridLayoutXck8XO8CuBSdJ5AFb6AneQ==";
	}
}
