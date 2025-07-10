using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW
{
	public class UpdateRORDutyTreatmentPaymentMethod : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update ROR Duty Treatment Payment Method";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'TW'"))
			{
				UpdateJobComInvoiceLinePaymentMethod();
				UpdateSSTaxPaymentMethod();
				UpdateCTTaxPaymentMethod();
			}
		}

		void UpdateJobComInvoiceLinePaymentMethod()
		{
			var sql = @"UPDATE dbo.JobComInvoiceLine
SET
JI_AddInfo = REPLACE(REPLACE(REPLACE(JI_AddInfo, 'DtyPymntMthd=CAS', 'DtyPymntMthd=ROR'), 'VatPymntMthd=CAS', 'VatPymntMthd=ROR'), 'TpfPymntMthd=CAS', 'TpfPymntMthd=ROR'),
JI_SystemLastEditTimeUtc = GETUTCDATE(),
JI_SystemLastEditUser = '~BP'
FROM
dbo.JobDeclaration
JOIN dbo.CusEntryInstruction ON CEI_ClusterKey = JE_ClusterKey
JOIN dbo.JobComInvoiceLine ON JI_ClusterKey = JE_ClusterKey AND JI_DataModel = 'TW' AND JI_Procedure IN ('38', '3E') 
CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'DtyPymntMthd') as DtyPymntMthd
CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'VatPymntMthd') as VatPymntMthd
CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'TpfPymntMthd') as TpfPymntMthd
INNER JOIN dbo.RefDatabase_RefCusTariff ON ZZ1_TariffCode = JI_Tariff AND ZZ1_ZZZ_NKDataGrouping = 'TW' AND ZZ1_StartDate < ISNULL(CEI_DateForDuty, GETDATE()) AND ZZ1_EndDate >= ISNULL(CEI_DateForDuty, GETDATE())
WHERE
JE_DataModel = 'TW' 
AND JE_MessageType = 'IMP'
AND (DtyPymntMthd.Value = 'CAS' OR VatPymntMthd.Value = 'CAS' OR TpfPymntMthd.Value = 'CAS')
AND EXISTS(SELECT NULL FROM dbo.GetRatesBySingleCriteriaSet(ZZ1_PK, JI_CountryOfOrigin, '', 'TW', JI_PrimaryPreference, NULL, '', ISNULL(CEI_DateForDuty, GETDATE()), 'DTY', 'DTA', 0) WHERE ZZ2_ZZZ_NKDataGrouping = 'TW' AND CASE WHEN ZZ2_RateFormulaDerivedFrom LIKE '[0].[0-9]%' AND ISNUMERIC(ZZ2_RateFormulaDerivedFrom) = 1 THEN CAST(ZZ2_RateFormulaDerivedFrom AS DECIMAL(8, 6)) ELSE 0 END <> 0);";
			Db.Connection.ExecuteNonQuery(sql);
		}

		void UpdateSSTaxPaymentMethod()
		{
			var sql = @"UPDATE dbo.JobComInvoiceLineTax
SET
JLT_MethodOfpayment = 'ROR',
JLT_SystemLastEditTimeUtc = GETUTCDATE(),
JLT_SystemLastEditUser = '~BP'
FROM
dbo.JobDeclaration
JOIN dbo.JobComInvoiceLine ON JI_ClusterKey = JE_ClusterKey AND JI_DataModel = 'TW' AND JI_Procedure IN ('38', '3E')
JOIN dbo.JobComInvoiceLineTax ON JLT_ClusterKey = JI_ClusterKey AND JLT_JI = JI_PK AND JLT_Type = 'SS' AND JLT_MethodOfpayment = 'CAS'
WHERE
JE_DataModel = 'TW'
AND JE_MessageType = 'IMP';";
			Db.Connection.ExecuteNonQuery(sql);
		}

		void UpdateCTTaxPaymentMethod()
		{
			var sql = @"UPDATE dbo.JobComInvoiceLineTax
SET
JLT_MethodOfpayment = 'ROR',
JLT_SystemLastEditTimeUtc = GETUTCDATE(),
JLT_SystemLastEditUser = '~BP'
FROM
dbo.JobDeclaration
JOIN dbo.CusEntryInstruction ON CEI_ClusterKey = JE_ClusterKey
JOIN dbo.JobComInvoiceLine ON JI_ClusterKey = JE_ClusterKey AND JI_DataModel = 'TW' AND JI_Procedure IN ('38', '3E')
INNER JOIN dbo.JobComInvoiceLineTax ON JLT_ClusterKey = JI_ClusterKey AND JLT_JI = JI_PK AND JLT_Type = 'CT' AND JLT_MethodOfpayment = 'CAS'
INNER JOIN dbo.RefDatabase_RefCusTariff ON ZZ1_TariffCode = JLT_Tariff AND ZZ1_ZZZ_NKDataGrouping = 'TW' AND ZZ1_StartDate < ISNULL(CEI_DateForDuty, GETDATE()) AND ZZ1_EndDate >= ISNULL(CEI_DateForDuty, GETDATE())
INNER JOIN dbo.RefDatabase_RefCusTariffType ON ZZI_PK = ZZ1_ZZI_TariffType AND ZZI_TariffType = JLT_Type
WHERE
JE_DataModel = 'TW'
AND JE_MessageType = 'IMP'
AND EXISTS(
	SELECT
		NULL
	FROM
		dbo.RefDatabase_RefCusRate
		JOIN RefDatabase_RefCusRateCode ON ZY1_PK = ZZ2_ZY1_RateCode AND ZY1_RateCode IN ('DTA', 'CTA')
	WHERE
		ZZ2_ZZ1_Tariff = ZZ1_PK);";
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
					.Where($"[JE_DataModel]='TW' AND [JE_MessageType] = 'IMP'")
					.GetInfo();

					indexProvider.New(JobComInvoiceLineSchema.Instance)
					.Key(JobComInvoiceLineSchema.Constants.JI_DataModel, JobComInvoiceLineSchema.Constants.JI_Procedure)
					.Include(JobComInvoiceLineSchema.Constants.PK, JobComInvoiceLineSchema.Constants.JI_Tariff, JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, JobComInvoiceLineSchema.Constants.JI_AddInfo, JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, JobComInvoiceLineSchema.Constants.JI_SystemLastEditTimeUtc, JobComInvoiceLineSchema.Constants.JI_SystemLastEditUser)
					.Where($"[JI_DataModel]='TW' AND [JI_Procedure] IN ('38', '3E')")
					.GetInfo();
				}
				return indexProvider;
			}
		}
	}
}
