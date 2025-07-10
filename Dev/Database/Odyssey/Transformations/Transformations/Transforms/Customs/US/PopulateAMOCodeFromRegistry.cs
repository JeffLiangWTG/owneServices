using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class PopulateAMOCodeFromRegistry : DataTransformation
	{
		public override string UserDescription => "Populate Originator code(AMO) From Registry";

		protected override void OfflinePostUpgradeTransform()
		{
			var script = @"
BEGIN
	DECLARE @OriginatorCode VARCHAR(7);
	SELECT @OriginatorCode = CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue))
	FROM dbo.StmData
	WHERE SD_Name = 'USAirAMSOriginatorCode' AND SD_IsLogged = 1 AND SD_Owner IS NULL;

	WITH OrgProxy AS (
		SELECT 
			GC_OH_OrgProxy,
			OA_PK,
			COALESCE(CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)), @OriginatorCode) AS OriginatorCode,
			ROW_NUMBER() OVER (PARTITION BY GC_OH_OrgProxy ORDER BY 
				CASE 
					WHEN CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)) IS NOT NULL THEN 1 
					ELSE 2 
				END) AS RowNum
		FROM 
			dbo.GlbCompany
		JOIN OrgAddress ON OA_OH = GC_OH_OrgProxy
		JOIN OrgAddressCapability ON PZ_OA = OA_PK
		FULL JOIN 
			dbo.StmData
		ON 
			SD_Owner = GC_PK 
			AND SD_Name = 'USAirAMSOriginatorCode' 
			AND SD_IsLogged = 1
		WHERE 
			GC_IsActive = 1
			AND PZ_AddressType='OFC' 
			AND PZ_IsMainAddress = 1
	)

	INSERT INTO dbo.OrgCusCode (
		OK_PK, OK_CodeType, OK_CustomsRegNo, OK_OA_PremisesAddress, OK_OH, OK_RN_NKCodeCountry, OK_SystemCreateUser, OK_SystemCreateTimeUtc, OK_SystemLastEditUser, OK_SystemLastEditTimeUtc
	)
	SELECT 
		NEWID(), 'AMO', OriginatorCode, OA_PK, GC_OH_OrgProxy, 'US', '~BP', GETUTCDATE(), '~BP', GETUTCDATE() 
	FROM OrgProxy
	WHERE RowNum = 1 and OriginatorCode IS NOT NULL
	AND NOT EXISTS (
		SELECT 1 
		FROM dbo.OrgCusCode 
		WHERE OK_CodeType = 'AMO' 
		AND OK_OA_PremisesAddress = OA_PK 
		AND OK_OH = GC_OH_OrgProxy
		AND OK_RN_NKCodeCountry = 'US'
	);
END
";

			Db.Connection.ExecuteNonQuery(script);
		}
	}
}
