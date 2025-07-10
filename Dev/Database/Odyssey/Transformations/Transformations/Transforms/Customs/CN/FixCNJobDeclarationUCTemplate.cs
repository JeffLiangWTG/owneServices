using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN
{
	public class FixCNJobDeclarationUCTemplate : DataTransformation
	{
		internal const string ModuleID = "CusDec_UC";

		public override string UserDescription => "Fix CN's Job Declaration Universal Copy templates";

		protected override void OfflinePostUpgradeTransform()
		{
			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix(
				oldPrefix: "XC",
				newPrefix: "JE",
				countryCode: "CN",
				moduleIDs: ModuleID);
		}
	}
}
