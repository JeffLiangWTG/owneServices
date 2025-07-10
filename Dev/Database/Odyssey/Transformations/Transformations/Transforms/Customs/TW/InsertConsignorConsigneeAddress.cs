using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW
{
	public class InsertConsignorConsigneeAddress : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Insert Consignor And Consignee To JobDocAddress";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'TW'"))
			{
				InsertConsignorAddress();
				InsertConsigneeAddress();
			}
		}

		void InsertConsignorAddress()
		{
			var sql = @"
INSERT INTO dbo.JobDocAddress
(
E2_PK,
E2_ParentID,
E2_ParentTableCode,
E2_AddressType,
E2_OA_Address,
E2_GovRegNumType,
E2_GovRegNum,
E2_SystemCreateTimeUtc,
E2_SystemCreateUser,
E2_SystemLastEditTimeUtc,
E2_SystemLastEditUser
)
SELECT
NEWID(),
JE_PK,
'JE',
'CRA',
ConsignorMainOfficeAddress.OA_PK,
ISNULL(
	CASE WHEN LEFT(ConsignorOrg.OH_RL_NKClosestPort, 2) = 'TW' THEN COALESCE(ConsignorVATCode.OK_CodeType, ConsignorPIDCode.OK_CodeType, ConsignorPASCode.OK_CodeType)
		WHEN CEI_Style IN ('D5', 'D2', 'D7', 'F4', 'F5', 'B6', 'D8', 'F2', 'F3') THEN 'FFF'
	END
, ''),
ISNULL(
	LEFT(
		CASE LEFT(ConsignorOrg.OH_RL_NKClosestPort, 2)
		WHEN 'TW' THEN COALESCE(ConsignorVATCode.OK_CustomsRegNo,ConsignorPIDCode.OK_CustomsRegNo, ConsignorPASCode.OK_CustomsRegNo)
		ELSE
			CASE WHEN CEI_Style IN ('D5', 'D2', 'D7') THEN 'FFF' + COALESCE(FromWarehouseCPWCode.OK_CustomsRegNo, FromWarehouseCCPCode.OK_CustomsRegNo)
				WHEN CEI_Style IN ('F4', 'F5', 'B6', 'D8', 'F2', 'F3') THEN
					CASE SupplierDocumentaryAddress.E2_AddressOverride
					WHEN 
						1 THEN 'FFF' + SupplierDocumentaryAddressBondedId.E2N_Number
					WHEN
						0 THEN 'FFF' + COALESCE(SupplierEPZCode.OK_CustomsRegNo, SupplierCBFCode.OK_CustomsRegNo, SupplierFTZCode.OK_CustomsRegNo, SupplierATPCode.OK_CustomsRegNo, SupplierSPKCode.OK_CustomsRegNo)
					END
				ELSE ''
			END
		END
	, 35)
, ''),
GETUTCDATE(),
'~BP',
GETUTCDATE(),
'~BP'
FROM
dbo.JobDeclaration
OUTER APPLY (
	SELECT TOP 1
		CEI_OA_Warehouse, CEI_Style
	FROM
		dbo.CusEntryInstruction
	WHERE
		CEI_ClusterKey = JE_ClusterKey
) AS CusEntryInstruction
LEFT JOIN dbo.OrgAddress AS FromWarehouseAddress ON FromWarehouseAddress.OA_PK = CEI_OA_Warehouse
LEFT JOIN dbo.OrgCusCode AS FromWarehouseCPWCode ON FromWarehouseCPWCode.OK_CodeType = 'CPW' AND FromWarehouseCPWCode.OK_OH = FromWarehouseAddress.OA_OH AND FromWarehouseCPWCode.OK_RN_NKCodeCountry = 'TW' AND FromWarehouseCPWCode.OK_OA_PremisesAddress = FromWarehouseAddress.OA_PK
LEFT JOIN dbo.OrgCusCode AS FromWarehouseCCPCode ON FromWarehouseCCPCode.OK_CodeType = 'CCP' AND FromWarehouseCCPCode.OK_OH = FromWarehouseAddress.OA_OH AND FromWarehouseCCPCode.OK_RN_NKCodeCountry = 'TW' AND FromWarehouseCCPCode.OK_OA_PremisesAddress = FromWarehouseAddress.OA_PK
LEFT JOIN dbo.JobDocAddress AS SupplierDocumentaryAddress ON SupplierDocumentaryAddress.E2_ParentID = JE_PK AND SupplierDocumentaryAddress.E2_AddressType = 'SUD' AND SupplierDocumentaryAddress.E2_AddressSequence = 0
LEFT JOIN dbo.OrgAddress AS SupplierAddress ON SupplierAddress.OA_PK = SupplierDocumentaryAddress.E2_OA_Address
OUTER APPLY (
	SELECT TOP 1
		E2N_Number
	FROM
		dbo.JobDocAddressNumber
	WHERE
		E2N_E2 = SupplierDocumentaryAddress.E2_PK AND E2N_RN_NKCountryCode = 'TW' AND E2N_NumberType IN ('EPZ', 'CBF', 'FTZ', 'ATP', 'SPK')
) AS SupplierDocumentaryAddressBondedId
JOIN dbo.OrgHeader AS ConsignorOrg ON ConsignorOrg.OH_PK = JE_OH_Exporter
OUTER APPLY (
	SELECT TOP 1
		OA_PK
	FROM
		dbo.OrgAddress
	WHERE
		OA_OH = ConsignorOrg.OH_PK AND EXISTS(SELECT NULL FROM dbo.OrgAddressCapability WHERE PZ_OA = OA_PK AND PZ_AddressType = 'OFC' AND PZ_IsMainAddress = 1)
) AS ConsignorMainOfficeAddress
LEFT JOIN dbo.OrgCusCode AS ConsignorVATCode ON ConsignorVATCode.OK_OH = ConsignorOrg.OH_PK AND ConsignorVATCode.OK_RN_NKCodeCountry = 'TW' AND ConsignorVATCode.OK_CodeType = 'VAT'
LEFT JOIN dbo.OrgCusCode AS ConsignorPIDCode ON ConsignorPIDCode.OK_OH = ConsignorOrg.OH_PK AND ConsignorPIDCode.OK_RN_NKCodeCountry = 'TW' AND ConsignorPIDCode.OK_CodeType = 'PID'
LEFT JOIN dbo.OrgCusCode AS ConsignorPASCode ON ConsignorPASCode.OK_OH = ConsignorOrg.OH_PK AND ConsignorPASCode.OK_RN_NKCodeCountry = 'TW' AND ConsignorPASCode.OK_CodeType = 'PAS'
LEFT JOIN dbo.OrgCusCode AS SupplierEPZCode ON SupplierEPZCode.OK_CodeType = 'EPZ' AND SupplierEPZCode.OK_OH = SupplierAddress.OA_OH AND SupplierEPZCode.OK_RN_NKCodeCountry = 'TW' AND SupplierEPZCode.OK_OA_PremisesAddress = SupplierDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS SupplierCBFCode ON SupplierCBFCode.OK_CodeType = 'CBF' AND SupplierCBFCode.OK_OH = SupplierAddress.OA_OH AND SupplierCBFCode.OK_RN_NKCodeCountry = 'TW' AND SupplierCBFCode.OK_OA_PremisesAddress = SupplierDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS SupplierFTZCode ON SupplierFTZCode.OK_CodeType = 'FTZ' AND SupplierFTZCode.OK_OH = SupplierAddress.OA_OH AND SupplierFTZCode.OK_RN_NKCodeCountry = 'TW' AND SupplierFTZCode.OK_OA_PremisesAddress = SupplierDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS SupplierATPCode ON SupplierATPCode.OK_CodeType = 'ATP' AND SupplierATPCode.OK_OH = SupplierAddress.OA_OH AND SupplierATPCode.OK_RN_NKCodeCountry = 'TW' AND SupplierATPCode.OK_OA_PremisesAddress = SupplierDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS SupplierSPKCode ON SupplierSPKCode.OK_CodeType = 'SPK' AND SupplierSPKCode.OK_OH = SupplierAddress.OA_OH AND SupplierSPKCode.OK_RN_NKCodeCountry = 'TW' AND SupplierSPKCode.OK_OA_PremisesAddress = SupplierDocumentaryAddress.E2_OA_Address
WHERE
JE_DataModel = 'TW' AND JE_OH_Exporter IS NOT NULL AND NOT EXISTS(SELECT NULL FROM dbo.JobDocAddress WHERE E2_ParentID = JE_PK AND E2_AddressType = 'CRA' AND E2_AddressSequence = 0)
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		void InsertConsigneeAddress()
		{
			var sql = @"
INSERT INTO dbo.JobDocAddress
(
E2_PK,
E2_ParentID,
E2_ParentTableCode,
E2_AddressType,
E2_OA_Address,
E2_GovRegNumType,
E2_GovRegNum,
E2_SystemCreateTimeUtc,
E2_SystemCreateUser,
E2_SystemLastEditTimeUtc,
E2_SystemLastEditUser
)
SELECT
NEWID(),
JE_PK,
'JE',
'CEA',
ConsigneeMainOfficeAddress.OA_PK,
ISNULL(
	CASE WHEN LEFT(ConsigneeOrg.OH_RL_NKClosestPort, 2) = 'TW' THEN COALESCE(ConsigneeVATCode.OK_CodeType, ConsigneePIDCode.OK_CodeType, ConsigneePASCode.OK_CodeType)
		WHEN CEI_Style IN ('B2', 'D7', 'D1', 'D8', 'B8', 'B9', 'D5', 'F4', 'F1', 'F2') THEN 'FFF'
	END
, ''),
LEFT(
	ISNULL(
		CASE LEFT(ConsigneeOrg.OH_RL_NKClosestPort, 2)
		WHEN 'TW' THEN COALESCE(ConsigneeVATCode.OK_CustomsRegNo, ConsigneePIDCode.OK_CustomsRegNo, ConsigneePASCode.OK_CustomsRegNo)
		ELSE
			CASE WHEN CEI_Style IN ('B2', 'D7') THEN 
					CASE ImporterDocumentaryAddress.E2_AddressOverride
					WHEN 
						1 THEN 'FFF' + COALESCE(ImporterDocumentaryAddressBondedId.E2N_Number, ToWarehouseCPWCode.OK_CustomsRegNo, ToWarehouseCCPCode.OK_CustomsRegNo)
					WHEN
						0 THEN 'FFF' + COALESCE(ImporterCCCCode.OK_CustomsRegNo, ImporterEPZCode.OK_CustomsRegNo, ImporterCBFCode.OK_CustomsRegNo, ImporterFTZCode.OK_CustomsRegNo, ImporterATPCode.OK_CustomsRegNo, ImporterSPKCode.OK_CustomsRegNo, ToWarehouseCPWCode.OK_CustomsRegNo, ToWarehouseCCPCode.OK_CustomsRegNo)
					END
				WHEN CEI_Style IN ('D1', 'D8') THEN 'FFF' + COALESCE(ToWarehouseCPWCode.OK_CustomsRegNo, ToWarehouseCCPCode.OK_CustomsRegNo)
				WHEN CEI_Style IN ('B8', 'B9', 'D5', 'F4', 'F1', 'F2') THEN
					CASE ImporterDocumentaryAddress.E2_AddressOverride
					WHEN 
						1 THEN 'FFF' + ImporterDocumentaryAddressBondedId.E2N_Number
					WHEN
						0 THEN 'FFF' + COALESCE(ImporterCCCCode.OK_CustomsRegNo, ImporterEPZCode.OK_CustomsRegNo, ImporterCBFCode.OK_CustomsRegNo, ImporterFTZCode.OK_CustomsRegNo, ImporterATPCode.OK_CustomsRegNo, ImporterSPKCode.OK_CustomsRegNo)
					END
				ELSE ''
			END
		END
	, '')
, 35),
GETUTCDATE(),
'~BP',
GETUTCDATE(),
'~BP'
FROM
dbo.JobDeclaration
OUTER APPLY (
	SELECT TOP 1
		CEI_OA_Warehouse2, CEI_Style
	FROM
		dbo.CusEntryInstruction
	WHERE
		CEI_ClusterKey = JE_ClusterKey
) AS CusEntryInstruction
LEFT JOIN dbo.OrgAddress AS ToWarehouseAddress ON ToWarehouseAddress.OA_PK = CEI_OA_Warehouse2
LEFT JOIN dbo.OrgCusCode AS ToWarehouseCPWCode ON ToWarehouseCPWCode.OK_CodeType = 'CPW' AND ToWarehouseCPWCode.OK_OH = ToWarehouseAddress.OA_OH AND ToWarehouseCPWCode.OK_RN_NKCodeCountry = 'TW' AND ToWarehouseCPWCode.OK_OA_PremisesAddress = ToWarehouseAddress.OA_PK
LEFT JOIN dbo.OrgCusCode AS ToWarehouseCCPCode ON ToWarehouseCCPCode.OK_CodeType = 'CCP' AND ToWarehouseCCPCode.OK_OH = ToWarehouseAddress.OA_OH AND ToWarehouseCCPCode.OK_RN_NKCodeCountry = 'TW' AND ToWarehouseCCPCode.OK_OA_PremisesAddress = ToWarehouseAddress.OA_PK
LEFT JOIN dbo.JobDocAddress AS ImporterDocumentaryAddress ON ImporterDocumentaryAddress.E2_ParentID = JE_PK AND ImporterDocumentaryAddress.E2_AddressType = 'IMD' AND ImporterDocumentaryAddress.E2_AddressSequence = 0
LEFT JOIN dbo.OrgAddress AS ImporterAddress ON ImporterAddress.OA_PK = ImporterDocumentaryAddress.E2_OA_Address
OUTER APPLY (
	SELECT TOP 1
		E2N_Number
	FROM
		dbo.JobDocAddressNumber
	WHERE
		E2N_E2 = ImporterDocumentaryAddress.E2_PK AND E2N_RN_NKCountryCode = 'TW' AND E2N_NumberType IN ('CCC', 'EPZ', 'CBF', 'FTZ', 'ATP', 'SPK')
) AS ImporterDocumentaryAddressBondedId
JOIN dbo.OrgHeader AS ConsigneeOrg ON ConsigneeOrg.OH_PK = JE_OH_Consignee
OUTER APPLY (
	SELECT TOP 1
		OA_PK
	FROM
		dbo.OrgAddress
	WHERE
		OA_OH = ConsigneeOrg.OH_PK AND EXISTS(SELECT NULL FROM dbo.OrgAddressCapability WHERE PZ_OA = OA_PK AND PZ_AddressType = 'OFC' AND PZ_IsMainAddress = 1)
) AS ConsigneeMainOfficeAddress
LEFT JOIN dbo.OrgCusCode AS ConsigneeVATCode ON ConsigneeVATCode.OK_OH = ConsigneeOrg.OH_PK AND ConsigneeVATCode.OK_RN_NKCodeCountry = 'TW' AND ConsigneeVATCode.OK_CodeType = 'VAT'
LEFT JOIN dbo.OrgCusCode AS ConsigneePIDCode ON ConsigneePIDCode.OK_OH = ConsigneeOrg.OH_PK AND ConsigneePIDCode.OK_RN_NKCodeCountry = 'TW' AND ConsigneePIDCode.OK_CodeType = 'PID'
LEFT JOIN dbo.OrgCusCode AS ConsigneePASCode ON ConsigneePASCode.OK_OH = ConsigneeOrg.OH_PK AND ConsigneePASCode.OK_RN_NKCodeCountry = 'TW' AND ConsigneePASCode.OK_CodeType = 'PAS'
LEFT JOIN dbo.OrgCusCode AS ImporterCCCCode ON ImporterCCCCode.OK_CodeType = 'CCC' AND ImporterCCCCode.OK_OH = ImporterAddress.OA_OH AND ImporterCCCCode.OK_RN_NKCodeCountry = 'TW' AND ImporterCCCCode.OK_OA_PremisesAddress = ImporterDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS ImporterEPZCode ON ImporterEPZCode.OK_CodeType = 'EPZ' AND ImporterEPZCode.OK_OH = ImporterAddress.OA_OH AND ImporterEPZCode.OK_RN_NKCodeCountry = 'TW' AND ImporterEPZCode.OK_OA_PremisesAddress = ImporterDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS ImporterCBFCode ON ImporterCBFCode.OK_CodeType = 'CBF' AND ImporterCBFCode.OK_OH = ImporterAddress.OA_OH AND ImporterCBFCode.OK_RN_NKCodeCountry = 'TW' AND ImporterCBFCode.OK_OA_PremisesAddress = ImporterDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS ImporterFTZCode ON ImporterFTZCode.OK_CodeType = 'FTZ' AND ImporterFTZCode.OK_OH = ImporterAddress.OA_OH AND ImporterFTZCode.OK_RN_NKCodeCountry = 'TW' AND ImporterFTZCode.OK_OA_PremisesAddress = ImporterDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS ImporterATPCode ON ImporterATPCode.OK_CodeType = 'ATP' AND ImporterATPCode.OK_OH = ImporterAddress.OA_OH AND ImporterATPCode.OK_RN_NKCodeCountry = 'TW' AND ImporterATPCode.OK_OA_PremisesAddress = ImporterDocumentaryAddress.E2_OA_Address
LEFT JOIN dbo.OrgCusCode AS ImporterSPKCode ON ImporterSPKCode.OK_CodeType = 'SPK' AND ImporterSPKCode.OK_OH = ImporterAddress.OA_OH AND ImporterSPKCode.OK_RN_NKCodeCountry = 'TW' AND ImporterSPKCode.OK_OA_PremisesAddress = ImporterDocumentaryAddress.E2_OA_Address
WHERE
JE_DataModel = 'TW' AND JE_OH_Consignee IS NOT NULL AND NOT EXISTS(SELECT NULL FROM dbo.JobDocAddress WHERE E2_ParentID = JE_PK AND E2_AddressType = 'CEA' AND E2_AddressSequence = 0)
";
			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName) && Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'TW'"))
				{
					indexProvider.New(JobDeclarationSchema.Instance)
					.Key(JobDeclarationSchema.Constants.JE_DataModel)
					.Include(JobDeclarationSchema.Constants.JE_OH_Consignee, JobDeclarationSchema.Constants.JE_OH_Exporter, JobDeclarationSchema.Constants.PK)
					.Where($"[JE_DataModel]='TW'")
					.GetInfo();
				}
				return indexProvider;
			}
		}
	}
}
