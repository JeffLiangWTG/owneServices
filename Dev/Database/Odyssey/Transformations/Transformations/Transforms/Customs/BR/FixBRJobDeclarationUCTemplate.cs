using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public class FixBRJobDeclarationUCTemplate : DataTransformation
	{
		internal const string ModuleID = "CusDec_UC";

		public override string UserDescription => "Fix BR's Job Declaration Universal Copy templates";

		protected override void OfflinePostUpgradeTransform()
		{
			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix(
				oldPrefix: "BR",
				newPrefix: "JE",
				countryCode: "BR",
				moduleIDs: ModuleID);
		}
	}
}
