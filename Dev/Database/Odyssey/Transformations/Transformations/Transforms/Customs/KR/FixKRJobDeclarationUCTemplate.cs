using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.KR
{
	public class FixKRJobDeclarationUCTemplate : DataTransformation
	{
		internal const string ModuleID = "CusDec_UC";

		public override string UserDescription => "Fix KR's Job Declaration Universal Copy templates";

		protected override void OfflinePostUpgradeTransform()
		{
			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix(
				oldPrefix: "KR",
				newPrefix: "JE",
				countryCode: "KR",
				moduleIDs: ModuleID);
		}
	}
}
