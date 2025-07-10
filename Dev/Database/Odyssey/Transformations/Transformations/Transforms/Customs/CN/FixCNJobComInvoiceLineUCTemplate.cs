using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN
{
	public class FixCNJobComInvoiceLineUCTemplate : DataTransformation
	{
		internal const string ModuleID = "Enterprise.Customs.CN.Business.JobComInvoiceLine_UC";

		public override string UserDescription => "Fix CN's JobComInvoiceLine Universal Copy templates";

		protected override void OfflinePostUpgradeTransform()
		{
			UCTemplateUpdateHelper.UpdateTemplateMappingPrefix(
				oldPrefix: "XC",
				newPrefix: "JI",
				countryCode: "CN",
				moduleIDs: ModuleID);
		}
	}
}
