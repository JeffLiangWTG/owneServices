using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow;

public class UncompressGlowFilters : DataTransformation
{
	public override string UserDescription => "Uncompress Glow filters that were previously compressed incorrectly";

	protected override void OfflinePostUpgradeTransform()
	{
		var uncompressSql = @"
UPDATE
	dbo.StmModuleFilter
SET
	S9_FilterData = CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), dbo.CLRUncompressAsString(S9_FilterData))),
	S9_SystemLastEditTimeUtc = GetUtcDate(),
	S9_SystemLastEditUser = '~BP'
WHERE
	dbo.CLRUncompressAsString(S9_FilterData) LIKE '<ArrayOfFilterGroup%'
	AND S9_ModuleID IN (
			'IEntityInfo_IJobShipment',
			'IEntityInfo_IJobOrderHeader',
			'IEntityInfo_WTJ',
			'IEntityInfo_Trackable',
			'IEntityInfo_IHVLVConsignment',
			'IEntityInfo_IJobContainer',
			'IEntityInfo_IJobDeclaration',
			'SDT_Index_IJobShipment_3cfd7b375ace42ddb34220951a47a161_67d54bfc-4eb7-4b7b-9ba4-edbd29c38cbe',
			'SDT_Index_IJobOrderHeader_0441029ff14c4112a19a2df58a95f64f_a76bd90d-1e0b-49a0-bc58-5471e4412347',
			'SEP_Index_IJobShipment_189a43c94e784fa28743ce5e22a8951e_490465d1-5497-4aa9-bba8-fbac663311f4',
			'SDT_Index_WTJ_3b2982abef5141ba94b8093d1ff752ae_766ae519-8900-4dd3-8b49-06d65eba921f',
			'SEP_Index_IJobOrderHeader_417d399aff514143880c6e9e3fef4226_a6e4ff80-5cbe-426d-89ad-ae0b7fa55998',
			'SDT_Index_IJobOrderHeader_6baeb77b18284dcd888be64f6e0fea2c_98581ec3-bdfc-4740-8207-4f99ce982031',
			'SEP_Index_Trackable_88498f6e7d394d2ea3a01d1f35248609_0c192a43-d287-4c30-b024-dcf0edd08ae4',
			'SEP_Index_IHVLVConsignment_b06fea5fb0f74b5fb61df6973b79c290_83f94d8c-5c9d-4ec3-b94b-6f8400b97b0f',
			'SEP_Index_IJobContainer_ea2270f57619407fbe158ff252dd94f5_c81db99f-a712-4fd9-a765-6e789dab2ebb',
			'SDT_Index_IJobShipment_fad31d07b17b44958d9f68f90cef1101_47687897-4072-40d4-999e-008845b7beda',
			'SDT_Index_IJobDeclaration_560bd2c23b634bb683235688d0f9f3de_000ab061-d6ca-4dbc-bb02-0d45bf101896',
			'IHVLVConsignment_3c95f54513ea4829aebfe9ffb9b4c647',
			'IJobOrderHeader_5a61be8e8e9546e39ad78e87ef0e93f6',
			'IJobShipment_6304763e5b644c069fb98fe353716da5',
			'IJobShipment_79cf3e9f175a4a67af77918cc5984c3d',
			'IHVLVConsignment_8a85c607db774a4da7ce0622d9c51236',
			'IJobShipment_8c36363ac8a541969bf0b0f8ed7bfd61',
			'IJobShipment_8e1d92d31df945e7ac840a4a2efab8bb',
			'IHVLVConsignment_94d76ffabddb4f35938a7dfb833a35a6',
			'IJobOrderHeader_be9b2a18f1f7494f9c451fbad12f43bb',
			'IJobShipment_d4f1974737b64b2c9ccfc3fe6214292e',
			'IJobOrderHeader_f423378ba5ab443a8a5c3e81e2c77628',
			'IJobContainer_a48bcc8375ee4a069ca9687b85595233',
			'IEntityInfo',
			'IEntityInfo_3cfd7b375ace42ddb34220951a47a161',
			'IEntityInfo_0441029ff14c4112a19a2df58a95f64f',
			'IEntityInfo_1597cef7fc1046cdab2164a4185ccef7',
			'IEntityInfo_189a43c94e784fa28743ce5e22a8951e',
			'IEntityInfo_3b2982abef5141ba94b8093d1ff752ae',
			'IEntityInfo_3c95f54513ea4829aebfe9ffb9b4c647',
			'IEntityInfo_417d399aff514143880c6e9e3fef4226',
			'IEntityInfo_5a61be8e8e9546e39ad78e87ef0e93f6',
			'IEntityInfo_6304763e5b644c069fb98fe353716da5',
			'IEntityInfo_6baeb77b18284dcd888be64f6e0fea2c',
			'IEntityInfo_79cf3e9f175a4a67af77918cc5984c3d',
			'IEntityInfo_88498f6e7d394d2ea3a01d1f35248609',
			'IEntityInfo_8a85c607db774a4da7ce0622d9c51236',
			'IEntityInfo_8c36363ac8a541969bf0b0f8ed7bfd61',
			'IEntityInfo_8e1d92d31df945e7ac840a4a2efab8bb',
			'IEntityInfo_94d76ffabddb4f35938a7dfb833a35a6',
			'IEntityInfo_b06fea5fb0f74b5fb61df6973b79c290',
			'IEntityInfo_be9b2a18f1f7494f9c451fbad12f43bb',
			'IEntityInfo_d4f1974737b64b2c9ccfc3fe6214292e',
			'IEntityInfo_eda73b9308a84133bec54216379d5bc4',
			'IEntityInfo_f423378ba5ab443a8a5c3e81e2c77628',
			'IEntityInfo_f9007835c94f40d6b3c6a36c2be10948',
			'IEntityInfo_a48bcc8375ee4a069ca9687b85595233',
			'IEntityInfo_ea2270f57619407fbe158ff252dd94f5',
			'IEntityInfo_fad31d07b17b44958d9f68f90cef1101',
			'IEntityInfo_560bd2c23b634bb683235688d0f9f3de'
		)
";
		Db.Connection.ExecuteNonQuery(uncompressSql);
	}
}
