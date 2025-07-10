using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR
{
	public sealed class SetJLT_MethodOfCalculationAndClearJLT_RateOverrideReasonCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Clear JLT_RateOverrideReasonCode on JobComInvoiceLineTax and set JLT_MethodOfCalculation for BR Customs Declarations.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'BR'"))
			{
				SetDataOnJLT_MethodOfCalculationAndClearJLT_RateOverrideReasonCode();
			}
		}

		void SetDataOnJLT_MethodOfCalculationAndClearJLT_RateOverrideReasonCode()
		{
			var sql = @"
UPDATE Tax
SET
	Tax.JLT_MethodOfCalculation =
		CASE
			WHEN Tax.JLT_RateOverrideReasonCode = 'FTA' AND Tax.JLT_MethodOfCalculation = '' THEN 'FTA'
			WHEN Tax.JLT_RateOverrideReasonCode = 'RMA' AND Tax.JLT_MethodOfCalculation = '' THEN 'MAR'
			WHEN Tax.JLT_RateOverrideReasonCode = 'RRA' AND Tax.JLT_MethodOfCalculation = '' THEN 'RED'
			WHEN Tax.JLT_MethodOfCalculation = '' THEN 'ADV'
			ELSE Tax.JLT_MethodOfCalculation END,
	Tax.JLT_RateOverrideReasonCode = '',
	Tax.JLT_SystemLastEditTimeUtc = GETUTCDATE(),
	Tax.JLT_SystemLastEditUser = '~BP'
FROM dbo.JobComInvoiceLineTax AS Tax
INNER JOIN dbo.JobComInvoiceLine AS Line
ON Tax.JLT_ClusterKey = Line.JI_ClusterKey AND Tax.JLT_JI = Line.JI_PK
WHERE
	Tax.JLT_Type IN ('0086', '1038', '5602', '5629') AND
	Tax.JLT_MethodOfCalculation = '' AND
	Line.JI_DataModel = 'BR'";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName) && Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'BR'"))
				{
					indexProvider.New(JobComInvoiceLineSchema.Instance)
						.Key(JobComInvoiceLineSchema.Constants.JI_DataModel)
						.Include(JobComInvoiceLineSchema.Constants.PK, JobComInvoiceLineSchema.Constants.JI_ClusterKey)
						.Where($"[JI_DataModel] = 'BR'").GetInfo();

					indexProvider.New(JobComInvoiceLineTaxSchema.Instance)
						.Key(JobComInvoiceLineTaxSchema.Constants.JLT_MethodOfCalculation, JobComInvoiceLineTaxSchema.Constants.JLT_Type)
						.Include(JobComInvoiceLineTaxSchema.Constants.JLT_JI, JobComInvoiceLineTaxSchema.Constants.JLT_ClusterKey, JobComInvoiceLineTaxSchema.Constants.JLT_RateOverrideReasonCode, JobComInvoiceLineTaxSchema.Constants.JLT_SystemLastEditTimeUtc, JobComInvoiceLineTaxSchema.Constants.JLT_SystemLastEditUser)
						.Where($"[JLT_Type] IN ('0086', '1038', '5602', '5629') AND [JLT_MethodOfCalculation] = ''").GetInfo();
				}
				return indexProvider;
			}
		}
	}
}
