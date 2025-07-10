using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public class FixBRInvoiceLineDIWTemplate : DataTransformation
	{
		public override string UserDescription => "Fix BR's Invoice Line DIW";

		protected override void OfflinePostUpgradeTransform()
		{
			DIWTemplateUpdateHelper.UpdateTemplateMappingPrefix("BR", "JI", "BR", ExportInvoiceLineDIW, ImportInvoiceLineDIW, ImportSiscomexInvoiceLineDIW, ImportLicenseInvoiceLineDIW);
		}

		public const string ExportInvoiceLineDIW = "DIW:GridLayoutMo67pW0l6MTKfFK9j67JkQ==";
		public const string ImportInvoiceLineDIW = "DIW:GridLayoutRxG0nZ9+XZupqJiNG63YNw==";
		public const string ImportSiscomexInvoiceLineDIW = "DIW:GridLayouthuj8BhPmWyk5PxnMIuIVbg==";
		public const string ImportLicenseInvoiceLineDIW = "DIW:GridLayoutpLJAYGMMUkO7eAZvmuBmmA==";
	}
}
