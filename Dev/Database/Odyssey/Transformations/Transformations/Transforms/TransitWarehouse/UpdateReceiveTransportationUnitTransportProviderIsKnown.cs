using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class UpdateReceiveTransportationUnitTransportProviderIsKnown : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update RTU TransportProviderIsKnown based on Org Certification or Driver Certification";

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				indexProvider.New(OrgContactSchema.Instance)
					.Key(OrgContactSchema.Constants.OC_ContactName)
					.Include(OrgContactSchema.Constants.PK)
					.Where($"([{OrgContactSchema.Constants.OC_ContactName}]<>'')")
					.GetInfo();

				indexProvider.New(GenRegCertAccredMaintListSchema.Instance)
					.Key(GenRegCertAccredMaintListSchema.Constants.XZ_ParentID,
						GenRegCertAccredMaintListSchema.Constants.XZ_ExpiryOrDueDate,
						GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode,
						GenRegCertAccredMaintListSchema.Constants.XZ_Type)
					.Where($"([{GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode}]='OC' AND [{GenRegCertAccredMaintListSchema.Constants.XZ_Type}]='BKG')")
					.GetInfo();

				indexProvider.New(GenRegCertAccredMaintListSchema.Instance)
					.Key(GenRegCertAccredMaintListSchema.Constants.XZ_ParentID,
						GenRegCertAccredMaintListSchema.Constants.XZ_ExpiryOrDueDate,
						GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode,
						GenRegCertAccredMaintListSchema.Constants.XZ_Type)
					.Where($"([{GenRegCertAccredMaintListSchema.Constants.XZ_ParentTableCode}]='OC' AND [{GenRegCertAccredMaintListSchema.Constants.XZ_Type}]='DTA')")
					.GetInfo();

				indexProvider.New(JobDocAddressSchema.Instance)
					.Key(JobDocAddressSchema.Constants.E2_AddressType, JobDocAddressSchema.Constants.E2_ParentTableCode)
					.Include(JobDocAddressSchema.Constants.E2_OA_Address, JobDocAddressSchema.Constants.E2_ParentID)
					.Where($"([{JobDocAddressSchema.Constants.E2_AddressType}]='TRA' AND [{JobDocAddressSchema.Constants.E2_ParentTableCode}]='WRH')")
					.GetInfo();

				indexProvider.New(WhsItemReceiveTransportationUnitSchema.Instance)
					.Key(WhsItemReceiveTransportationUnitSchema.Constants.WRH_GateOutTime)
					.Include(WhsItemReceiveTransportationUnitSchema.Constants.PK,
						WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemLastEditTimeUtc,
						WhsItemReceiveTransportationUnitSchema.Constants.WRH_SystemLastEditUser,
						WhsItemReceiveTransportationUnitSchema.Constants.WRH_SignedBy,
						WhsItemReceiveTransportationUnitSchema.Constants.WRH_WW_Warehouse,
						"WRH_AutoVersion")
					.GetInfo();

				indexProvider.New(OrgCountryDataSchema.Instance)
					.Key(OrgCountryDataSchema.Constants.OV_EXApprovalExpiryDate,
						OrgCountryDataSchema.Constants.OV_EXApprovedOrMajorExporter,
						OrgCountryDataSchema.Constants.OV_OH_OrgHeader,
						OrgCountryDataSchema.Constants.OV_RN_NKClientCountryRelation,
						OrgCountryDataSchema.Constants.OV_OA_ApprovedLocation)
					.GetInfo();

				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_WhsItemReceiveTransportationUnit_UpdateAutoVersion", WhsItemReceiveTransportationUnitSchema.Constants.TableName))
			{
				Db.Connection.ExecuteNonQuery(UpdateTransportCompanyKnownSql);
			}
		}

		const string UpdateTransportCompanyKnownSql =
@"
CREATE TABLE #RTUAddressData(
    WRH_ReferenceNumber varchar(15) NOT NULL,
    WRH_SignedBy nvarchar(100) NOT NULL,
	OA_PK uniqueidentifier NOT NULL,
	OA_OH uniqueidentifier NOT NULL,
    CountryCode varchar(2) NOT NULL,
	CheckDriver bit NOT NULL
);

CREATE CLUSTERED INDEX IX_#RTUAddressData_WRH_SignedBy
ON #RTUAddressData (WRH_SignedBy)

CREATE NONCLUSTERED INDEX IX_#RTUAddressData_OA_OH
ON #RTUAddressData (OA_OH)
INCLUDE (WRH_SignedBy)
WHERE WRH_SignedBy <> ''

DECLARE @DriverSecurityCertificationSystemCheckingActivated bit = 1;
DECLARE @now smalldatetime;
DECLARE @today date;
DECLARE @GateOutDelta datetimeoffset;
DECLARE @EUCountryCodes TABLE (EUCountryCode varchar(2) PRIMARY KEY);

SELECT
	@DriverSecurityCertificationSystemCheckingActivated = 
	CASE WHEN(CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'False')
	THEN 0
	ELSE 1 
END,
	@now = SYSDATETIMEOFFSET(),
	@today = cast(@now as date)
FROM
	dbo.StmData
WHERE
	SD_Name = 'DriverSecurityCertificationCheckingActivated'
	AND SD_Owner is NULL
	AND SD_DepartmentGuid is NULL;

SET @GateOutDelta = TODATETIMEOFFSET(DATEADD(DAY, -30, CAST(@now AS date)), DATEPART(TZOFFSET, SYSDATETIMEOFFSET()));

INSERT INTO @EUCountryCodes (EUCountryCode)
VALUES
  ('AT'), ('BE'), ('BG'), ('HR'), ('CY'), ('CZ'), ('DK'), ('EE'), ('FI'), ('FR'),
  ('DE'), ('GR'), ('HU'), ('IS'), ('IE'), ('IT'), ('LV'), ('LI'), ('LT'), ('LU'),
  ('MT'), ('NL'), ('NO'), ('PL'), ('PT'), ('RO'), ('SK'), ('SI'), ('ES'), ('SE'),
  ('CH'), ('GB');

INSERT INTO #RTUAddressData
SELECT WRH_ReferenceNumber,
	WRH_SignedBy,
	OA_PK,
	OA_OH,
	CountryCode = CASE
	    WHEN EU1.EUCountryCode IS NOT NULL AND GC_RN_NKCountryCode <> 'GB' AND OA_RN_NKCountryCode <> 'GB' THEN 'EU'
	    WHEN EU1.EUCountryCode IS NOT NULL AND GC_RN_NKCountryCode <> 'GB' AND OA_RN_NKCountryCode = 'GB' THEN 'GB'
	    ELSE GC_RN_NKCountryCode
	END,
	CheckDriver = CASE
		WHEN(CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'True') THEN 1
		WHEN(CONVERT(NVARCHAR(MAX), SD_BinaryValue) = 'False') THEN 0
		ELSE @DriverSecurityCertificationSystemCheckingActivated
	END
FROM
	dbo.WhsItemReceiveTransportationUnit
	JOIN dbo.WhsWarehouse ON WW_PK = WRH_WW_Warehouse
	JOIN dbo.GlbBranch ON GB_PK = WW_GB_RelatedCompanyBranch
	JOIN dbo.GlbCompany ON GC_PK = GB_GC
	JOIN dbo.JobDocAddress ON E2_ParentID = WRH_PK AND E2_AddressType = 'TRA' AND E2_ParentTableCode = 'WRH'
	JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
	LEFT JOIN dbo.stmdata ON WW_GB_RelatedCompanyBranch = SD_Owner AND SD_Name = 'DriverSecurityCertificationCheckingActivated'
	LEFT JOIN @EUCountryCodes AS EU1 ON EU1.EUCountryCode COLLATE SQL_Latin1_General_CP1_CI_AS = GC_RN_NKCountryCode
	WHERE
	WRH_GateOutTime IS NULL OR WRH_GateOutTime >= @GateOutDelta;

WITH ValidContacts
AS
(
	SELECT DISTINCT
		OC_OH,
		OC_ContactName
	FROM
		dbo.OrgContact OC
		JOIN (
			SELECT DISTINCT WRH_SignedBy, OA_OH
			FROM #RTUAddressData
			WHERE
			WRH_SignedBy <> ''
		) AS RTU ON OC.OC_ContactName = RTU.WRH_SignedBy COLLATE SQL_Latin1_General_CP1_CI_AS
		AND OC.OC_OH = RTU.OA_OH
	WHERE
		EXISTS (
			SELECT NULL FROM
				dbo.GenRegCertAccredMaintList
				WHERE XZ_ParentID = OC_PK AND XZ_ParentTableCode = 'OC'
				AND XZ_Type = 'BKG' AND OC_ContactName <> '' AND XZ_ExpiryOrDueDate > @now
			)
		AND
		EXISTS (
			SELECT NULL FROM
			dbo.GenRegCertAccredMaintList
			WHERE XZ_ParentID = OC_PK AND XZ_ParentTableCode = 'OC'
			AND XZ_Type = 'DTA' AND OC_ContactName <> '' AND XZ_ExpiryOrDueDate > @now
		)
),
TransportCompanyKnown
AS
(
	SELECT WRH_ReferenceNumber
	FROM
		#RTUAddressData
	WHERE 
	(
		EXISTS ( 
			SELECT NULL
			FROM (
				SELECT 
					OV_OH_OrgHeader,
					OV_RN_NKClientCountryRelation,
					OV_EXApprovedOrMajorExporter,
					OV_OA_ApprovedLocation
				FROM dbo.OrgCountryData
				WHERE OV_EXApprovalExpiryDate IS NULL OR OV_EXApprovalExpiryDate >= @today
			) OrgCtrData
			WHERE OV_OH_OrgHeader = OA_OH
			AND #RTUAddressData.CountryCode COLLATE SQL_Latin1_General_CP1_CI_AS = OV_RN_NKClientCountryRelation
			AND (
				(OV_EXApprovedOrMajorExporter IN ('CH', 'AH'))
				OR
				(OV_EXApprovedOrMajorExporter = 'RA' AND OV_OA_ApprovedLocation = #RTUAddressData.OA_PK)
			)
		)
		AND (
			CheckDriver = 0  
			OR 
			EXISTS (
				SELECT NULL FROM
				ValidContacts
				WHERE OC_OH = OA_OH
					AND WRH_SignedBy COLLATE SQL_Latin1_General_CP1_CI_AS = OC_ContactName
					AND CheckDriver = 1 AND WRH_SignedBy <> ''
			)
		)
	)
)

UPDATE
	RTU
SET
	WRH_TransportProviderIsKnown = 1,
	WRH_SystemLastEditUser = '~BP',
	WRH_SystemLastEditTimeUtc = GetUtcDate(),
	WRH_AutoVersion = (WRH_AutoVersion + 1) % 32768
FROM
	dbo.WhsItemReceiveTransportationUnit RTU
	JOIN TransportCompanyKnown ON TransportCompanyKnown.WRH_ReferenceNumber COLLATE SQL_Latin1_General_CP1_CI_AS = RTU.WRH_ReferenceNumber

DROP TABLE #RTUAddressData
";
	}
}
