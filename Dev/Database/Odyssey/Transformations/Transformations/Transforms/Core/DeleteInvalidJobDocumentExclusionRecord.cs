using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core;

public class DeleteInvalidJobDocumentExclusionRecord : DataTransformation
{
	public override string UserDescription => "Delete the records associated with dummy contact in JobDocumentExclusion";

	public override bool IsRequired => base.IsRequired && DbObjectCreator.ColumnExists(Db.Connection, "JobDocumentExclusion", "JDE_OD_Document");

	protected override void OfflinePostUpgradeTransform()
	{
		var sql = @"DELETE FROM dbo.JobDocumentExclusion
WHERE JDE_OD_Document IN (
    SELECT doc.OD_PK
    FROM dbo.OrgDocument doc
    LEFT JOIN dbo.OrgContact contact ON doc.OD_OC = contact.OC_PK
    WHERE doc.OD_DeliverBy = 'DND' AND (doc.OD_OC IS NULL OR contact.OC_ContactName = 'DUMMY CONTACT TO SUPPRESS DOCS')
)";
		Db.Connection.ExecuteNonQuery(sql);
	}
}