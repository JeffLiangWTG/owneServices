using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class RemoveDuplicateStatementLineChargeForDN : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Remove the Duplicate Charges of Statement Line of Daily Notice.";

		protected override void OfflinePostUpgradeTransform()
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				var script = @"
WITH CTE AS
(
	SELECT 
		B4_PK PK, 
		ROW_NUMBER() OVER (PARTITION BY B4_B3, B4_ChargeType ORDER BY B4_SystemCreateTimeUtc) RN 
	FROM 
		dbo.CusStatementLineCharge
	INNER JOIN dbo.CusStatementLine ON B4_B3 = B3_PK
	INNER JOIN dbo.CusStatementHeader ON B3_B2 = B2_PK
	INNER JOIN dbo.GlbCompany ON B2_GC = GC_PK AND GC_RN_NKCountryCode = 'CA'
	WHERE B2_SystemCreateTimeUtc >= '2024-10-01' AND
	B2_IsMonthlyStatement = 0 AND
	B2_StatementType <> 'R'  AND
	B2_StatementNumber LIKE 'DN-%'
)

DELETE 
	dbo.CusStatementLineCharge
FROM 
	dbo.CusStatementLineCharge
INNER JOIN CTE ON PK = B4_PK
WHERE RN > 1
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
				.Include(CusStatementHeaderSchema.Constants.B2_GC, CusStatementHeaderSchema.Constants.B2_StatementType)
				.Where("[B2_SystemCreateTimeUtc]>='2024-10-01' AND [B2_IsMonthlyStatement]=(0) AND [B2_StatementType]<>'R'")
				.GetInfo();
				return indexProvider;
			}
		}
	}
}
