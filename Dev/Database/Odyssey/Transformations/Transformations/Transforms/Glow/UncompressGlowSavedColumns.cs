using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow;

public class UncompressGlowSavedColumns : DataTransformation
{
	public override string UserDescription => "Uncompress Glow saved columns that were previously compressed incorrectly";

	protected override void OfflinePostUpgradeTransform()
	{
		var uncompressSql = @"
UPDATE
	dbo.StmData
SET
	SD_BinaryValue = CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), dbo.CLRUncompressAsString(SD_BinaryValue))),
	SD_SystemLastEditTimeUtc = GetUtcDate(),
	SD_SystemLastEditUser = '~BP'
WHERE
	dbo.CLRUncompressAsString(SD_BinaryValue) LIKE '<GridSettings%'
	AND (
		SD_Name IN (
			'GridSettings_SEP_Index_IJobContainer_ea2270f57619407fbe158ff252dd94f5_c81db99f-a712-4fd9-a765-6e789dab2ebb',
			'GridSettings_SDT_Index_IJobShipment_3cfd7b375ace42ddb34220951a47a161_67d54bfc-4eb7-4b7b-9ba4-edbd29c38cbe',
			'GridSettings_SDT_Index_IJobOrderHeader_0441029ff14c4112a19a2df58a95f64f_a76bd90d-1e0b-49a0-bc58-5471e4412347',
			'GridSettings_SDT_Index_IJobOrderHeader_6baeb77b18284dcd888be64f6e0fea2c_98581ec3-bdfc-4740-8207-4f99ce982031',
			'GridSettings_SEP_Index_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4',
			'GridSettings_SEP_Index_IJobOrderHeader_417d399aff514143880c6e9e3fef4226_a6e4ff80-5cbe-426d-89ad-ae0b7fa55998',
			'GridSettings_SEP_Index_NeoDashboard_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4',
			'GridSettings_SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f',
			'GridSettings_SDT_Index_WTJ_3b2982abef5141ba94b8093d1ff752ae_766ae519-8900-4dd3-8b49-06d65eba921f',
			'GridSettings_SDT_Index_IJobShipment_fad31d07b17b44958d9f68f90cef1101_47687897-4072-40d4-999e-008845b7beda',
			'GridSettings_SDT_Index_IJobDeclaration_560bd2c23b634bb683235688d0f9f3de_000ab061-d6ca-4dbc-bb02-0d45bf101896',
			'GridSettings_Search_IJobContainer',
			'GridSettings_Search_IJobShipment',
			'GridSettings_Search_IJobOrderHeader',
			'GridSettings_Search_IHVLVConsignment'
		)
		OR SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IJobShipment[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IJobOrderHeader[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]Index[_]WTJ[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IHVLVConsignment[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]Index[_]NeoDashboard[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]Index[_]IJobContainer[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]IJobShipment[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]IJobOrderHeader[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]WTJ[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]IHVLVConsignment[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]NeoDashboard[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]IJobContainer[_]%'
		OR SD_Name LIKE 'GridSettings[_]Search[_]IJobDeclaration[_]%'
	)
";
		Db.Connection.ExecuteNonQuery(uncompressSql);
	}
}
