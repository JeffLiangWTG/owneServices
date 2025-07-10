using System;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class UpdateDeclarationConsolidatedEntryNumber : DataTransformation
	{
		readonly int batchSize;
		const string ColumnNameNumPivots = "NumPivots";
		const string ColumnNameMinClusterKey = "MinClusterKey";

		public UpdateDeclarationConsolidatedEntryNumber()
		: this(5000)
		{
		}

		internal UpdateDeclarationConsolidatedEntryNumber(int batchSize)
		{
			Argument.GreaterThan(batchSize, 0, nameof(batchSize));
			this.batchSize = batchSize;
		}

		public override string UserDescription => "Update Declaration Consolidated Entry Number";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var clusterKeyEndFlag = int.MaxValue;
			var sqlText = $@"
DECLARE @DecEntryPivot TABLE(PK UNIQUEIDENTIFIER, ConsolEntryNum VARCHAR(100), ClusterKey INT)

INSERT INTO @DecEntryPivot
SELECT TOP {batchSize} GenEntry.XA_PK, CONCAT(AddInfoFilerCode.Value, CE_EntryNum), ReleaseDec.JE_ClusterKey
FROM  dbo.JobDeclaration ReleaseDec
	INNER JOIN dbo.GenAddOnColumn GenJob ON GenJob.XA_ParentID = ReleaseDec.JE_PK AND GenJob.XA_Name = 'US_ConsolidatedJobNumber'
	INNER JOIN dbo.JobDeclaration ConsolDec ON ConsolDec.JE_DeclarationReference = GenJob.XA_Data AND ConsolDec.JE_GB = ReleaseDec.JE_GB
	INNER JOIN dbo.CusEntryNum ON CE_ParentID = ConsolDec.JE_PK AND CE_EntryType = 'ENS' AND CE_RN_NKCountryCode = 'US'
	INNER JOIN dbo.GenAddOnColumn GenEntry ON GenEntry.XA_ParentID =  ReleaseDec.JE_PK AND GenEntry.XA_Name = 'US_ConsolidatedEntryNumber'
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(ConsolDec.JE_AddInfo, 'EntryFilerCode') AS AddInfoFilerCode
WHERE
	ReleaseDec.JE_DataModel = 'US' AND ReleaseDec.JE_MessageType IN ('IMP', 'MSC', 'IMX') AND ReleaseDec.JE_IsCancelled = 0
	AND ReleaseDec.JE_ClusterKey < @ClusterKeyEnd
	AND CONCAT(AddInfoFilerCode.Value, CE_EntryNum) <> GenEntry.XA_Data
ORDER BY ReleaseDec.JE_ClusterKey DESC

UPDATE dbo.GenAddOnColumn
SET XA_Data = ConsolEntryNum,
	XA_SystemLastEditTimeUtc = GETUTCDATE(),
	XA_SystemLastEditUser = '~BP'
FROM dbo.GenAddOnColumn
INNER JOIN @DecEntryPivot ON PK = XA_PK

SELECT
	(SELECT COUNT(*) FROM @DecEntryPivot) AS {ColumnNameNumPivots},
	(SELECT MIN(ClusterKey) FROM @DecEntryPivot) AS {ColumnNameMinClusterKey};
";
			while (true)
			{
				token.ThrowIfCancellationRequested();
				var numPivots = 0;
				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameter("@ClusterKeyEnd", SqlDbType.Int, clusterKeyEndFlag);
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						numPivots = (int)reader[ColumnNameNumPivots];
						var clusterKeyCell = reader[ColumnNameMinClusterKey];
						if (!Convert.IsDBNull(clusterKeyCell))
						{
							clusterKeyEndFlag = (int)clusterKeyCell;
						}
					}
				}
				if (manager != null)
				{
					manager.ShowInfoMessage($"Processed release declaration count [{numPivots}].");
				}
				if (numPivots < batchSize)
				{
					break;
				}
			}
		}
	}
}
