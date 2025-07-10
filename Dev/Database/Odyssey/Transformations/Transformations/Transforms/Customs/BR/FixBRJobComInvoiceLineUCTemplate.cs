using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public class FixBRJobComInvoiceLineUCTemplate : DataTransformation
	{
		internal const string ModuleID = "Enterprise.Customs.BR.Business.JobComInvoiceLine_UC";

		public override string UserDescription => "Fix BR's JobComInvoiceLine Universal Copy templates";

		protected override void OfflinePostUpgradeTransform()
		{
			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix(
				oldPrefix: "BR",
				newPrefix: "JI",
				countryCode: "BR",
				moduleIDs: ModuleID);
		}
	}
}
