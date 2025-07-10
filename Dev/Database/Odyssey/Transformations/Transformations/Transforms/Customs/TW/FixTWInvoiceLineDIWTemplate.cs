using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW
{
	public class FixTWInvoiceLineDIWTemplate : DataTransformation
	{
		public override string UserDescription => "Fix TW's Invoice Line DIW";

		protected override void OfflinePostUpgradeTransform()
		{
			DIWTemplateUpdateHelper.UpdateTemplateMappingPrefix("TW", "JI", "TW", ExportInvoiceLineDIW, ImportInvoiceLineDIW);
		}

		public const string ExportInvoiceLineDIW = "DIW:GridLayout5vcwZVOmU3T+q0mXmx9tlw==";
		public const string ImportInvoiceLineDIW = "DIW:GridLayoutebbR91N3T2lllWmctQwpCw==";
	}
}
