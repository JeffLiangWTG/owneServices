using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class PopulateAccountingDateAndSecurityCodeForDailyNotice : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Populate Accounting Date And Account Security Code For Daily Notice Statement.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				var script = @"
UPDATE
	dbo.CusStatementHeader
SET 
	B2_ProcessDate = ISNULL(B2_ProcessDate, A.AccountingDate),
	B2_EntryFilerCode = IIF(LEN(A.AccountSecurityCode) >= 5 AND B2_EntryFilerCode = '', LEFT(A.AccountSecurityCode, 5), B2_EntryFilerCode),
	B2_SystemLastEditTimeUtc = GETUTCDATE(), 
	B2_SystemLastEditUser = '~BP'
FROM
	dbo.CusStatementHeader
	INNER JOIN dbo.GlbCompany ON B2_GC = GC_PK AND GC_RN_NKCountryCode = 'CA'
	CROSS APPLY
	(
		SELECT MIN(B3_ScheduledProcessDate) AccountingDate, MAX(B3_EntryNum) AccountSecurityCode FROM dbo.CusStatementLine WHERE B3_B2 = B2_PK
	) A
WHERE 
	B2_SystemCreateTimeUtc >= '2024-10-01' AND
	B2_IsMonthlyStatement = 0 AND
	B2_StatementType <> 'R'  AND
	B2_StatementNumber LIKE 'DN-%' AND
	(B2_ProcessDate IS NULL OR B2_EntryFilerCode = '')
";
				Db.Connection.ExecuteNonQuery(script);
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusStatementHeaderSchema.Instance)
				.Key(CusStatementHeaderSchema.Constants.B2_StatementNumber)
				.Include(
					CusStatementHeaderSchema.Constants.B2_ProcessDate,
					CusStatementHeaderSchema.Constants.B2_EntryFilerCode,
					CusStatementHeaderSchema.Constants.B2_GC,
					CusStatementHeaderSchema.Constants.B2_StatementType,
					CusStatementHeaderSchema.Constants.B2_SystemLastEditTimeUtc,
					CusStatementHeaderSchema.Constants.B2_SystemLastEditUser)
				.Where("[B2_SystemCreateTimeUtc]>='2024-10-01' AND [B2_IsMonthlyStatement]=(0) AND [B2_StatementType]<>'R'")
				.GetInfo();

				return indexProvider;
			}
		}
	}
}
