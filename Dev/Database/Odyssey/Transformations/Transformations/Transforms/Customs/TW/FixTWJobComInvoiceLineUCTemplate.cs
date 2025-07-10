using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW
{
	public class FixTWJobComInvoiceLineUCTemplate : DataTransformation
	{
		internal const string ModuleID = "Enterprise.Customs.TW.Business.JobComInvoiceLine_UC";

		public override string UserDescription => "Fix TW's JobComInvoiceLine Universal Copy templates";

		protected override void OfflinePostUpgradeTransform()
		{
			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix(
				oldPrefix: "TW",
				newPrefix: "JI",
				countryCode: "TW",
				moduleIDs: ModuleID);
		}
	}
}
