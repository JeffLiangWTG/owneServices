using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public sealed class MoveGoodsCatalogLocalPartNumbersToCusGoodsCatalogProductionInfo : DataTransformation
	{
		public override string UserDescription => "Move Goods Catalog Local Part Numbers from CusReference to CusGoodsCatalogProductionInfo.";

		protected override void OfflinePostUpgradeTransform()
		{
			MoveDataFromCusReferenceToCusGoodsCatalogProductionInfo();
		}

		void MoveDataFromCusReferenceToCusGoodsCatalogProductionInfo()
		{
			var sql = @"
	UPDATE
		dbo.CusGoodsCatalogProductionInfo
	SET
		CGI_Type = 'FOR',
		CGI_SystemLastEditTimeUtc = GETUTCDATE(),
		CGI_SystemLastEditUser = '~BP'
	WHERE
		CGI_Type = ''

	INSERT INTO dbo.CusGoodsCatalogProductionInfo (CGI_PK, CGI_CGC_Catalog, CGI_Reference, CGI_Type, CGI_CustomsStatus, CGI_SystemCreateTimeUtc, CGI_SystemLastEditTimeUtc, CGI_SystemCreateUser, CGI_SystemLastEditUser)
	SELECT NEWID(), CFR_ParentID, CFR_Reference, 'LPN', 'ACT', COALESCE(NULLIF(CFR_SystemCreateTimeUtc, ''), GETUTCDATE()), COALESCE(NULLIF(CFR_SystemLastEditTimeUtc, ''), GETUTCDATE()), COALESCE(NULLIF(CFR_SystemCreateUser, ''), '~BP'), COALESCE(NULLIF(CFR_SystemLastEditUser, ''), '~BP')
	FROM dbo.CusReference
	WHERE CFR_Type = 'CGC' AND CFR_ParentTableCode = 'CGC'

	DELETE FROM dbo.CusReference
	WHERE CFR_Type = 'CGC' OR CFR_ParentTableCode = 'CGC'";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
