using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class CopyObsoleteColumnsFromAccTaxRateToGenAddOnColumnTable : DataTransformation
	{
		public override string UserDescription => "Copy Obsolete Rate Columns From AccTaxRate To GenAddOnColumn table.";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, AccTaxRateSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, AccTaxRateSchema.Constants.TableName, "AT_RateObsolete"))
			{
				Db.Connection.ExecuteNonQuery(TransformationMainSqlText);
			}
		}

		const string TransformationMainSqlText = @"
INSERT INTO dbo.GenAddOnColumn
(
	XA_PK,
	XA_Name,
	XA_Type,
	XA_ParentTableCode,
	XA_ParentID,
	XA_Data,
	XA_SystemCreateTimeUtc,
	XA_SystemCreateUser,
	XA_SystemLastEditTimeUtc,
	XA_SystemLastEditUser
)
SELECT
	NEWID(),
	'RateObsolete',
	'DEC',
	'AT',
	AT_PK,
	CAST(AT_RateObsolete AS VARCHAR(100)),
	GetUTCDate(),
	'E',
	GetUTCDate(),
	'E'
FROM dbo.AccTaxRate
WHERE AT_ReferenceRateType = '' AND AT_RateObsolete <> 0 
AND NOT EXISTS (SELECT 1 FROM dbo.GenAddOnColumn WHERE XA_ParentID = AT_PK AND XA_Name = 'RateObsolete')

UNION ALL

SELECT
	NEWID(),
	'ExtraTaxRateNumeratorObsolete',
	'INT',
	'AT',
	AT_PK,
	CAST(AT_ExtraTaxRateNumeratorObsolete AS VARCHAR(100)),
	GetUTCDate(),
	'E',
	GetUTCDate(),
	'E'
FROM dbo.AccTaxRate
WHERE AT_ReferenceRateType = '' AND AT_ExtraTaxRateNumeratorObsolete <> 0
AND NOT EXISTS (SELECT 1 FROM dbo.GenAddOnColumn WHERE XA_ParentID = AT_PK AND XA_Name = 'ExtraTaxRateNumeratorObsolete')

UNION ALL

SELECT
	NEWID(),
	'ExtraTaxRateDenominatorObsolete',
	'INT',
	'AT',
	AT_PK,
	CAST(AT_ExtraTaxRateDenominatorObsolete AS VARCHAR(100)),
	GetUTCDate(),
	'E',
	GetUTCDate(),
	'E'
FROM dbo.AccTaxRate
WHERE AT_ReferenceRateType = '' AND AT_ExtraTaxRateDenominatorObsolete <> 1
AND NOT EXISTS (SELECT 1 FROM dbo.GenAddOnColumn WHERE XA_ParentID = AT_PK and XA_Name = 'ExtraTaxRateDenominatorObsolete')
";
	}
}
