using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.Client.EDI.DbUpgrader;

public class EdiCreateERQOrganisationRelatedParty : DataTransformation
{
	public override string UserDescription => "Create a Related Party with type ERQ for each related party of type MNG";

	protected override void OfflinePostUpgradeTransform()
	{
		if (DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"))
		{
			var insertSql = @"
IF NOT EXISTS (SELECT NULL FROM dbo.OrgRelatedParty WHERE PR_PartyType = 'ERQ')
BEGIN
	INSERT INTO dbo.OrgRelatedParty
	(PR_PK,PR_PartyType,PR_IsValid,PR_FreightDirection,PR_Service,PR_Location,PR_CustomsStatus,PR_OA,PR_GC,PR_OH_Parent,PR_OH_RelatedParty,PR_FreightTransportMode,PR_FreightContainerMode,PR_RN_NKImporterCountry,PR_AutoVersion,PR_SystemCreateTimeUtc,PR_SystemCreateUser,PR_SystemLastEditTimeUtc,PR_SystemLastEditUser)
	SELECT
		NEWID(),'ERQ',PR_IsValid,PR_FreightDirection,PR_Service,PR_Location,PR_CustomsStatus,PR_OA,PR_GC,PR_OH_Parent,PR_OH_RelatedParty,PR_FreightTransportMode,PR_FreightContainerMode,PR_RN_NKImporterCountry,PR_AutoVersion,GETUTCDATE(),'~BP',GETUTCDATE(),'~BP'
	FROM
		dbo.OrgRelatedParty
	WHERE
		PR_PartyType = 'MNG'
END
";

			Db.Connection.ExecuteNonQuery(insertSql);
		}
	}
}
