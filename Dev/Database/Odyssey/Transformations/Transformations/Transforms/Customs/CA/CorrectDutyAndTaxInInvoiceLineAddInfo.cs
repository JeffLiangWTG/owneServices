using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Customs.CA
{
	public class CorrectDutyAndTaxInInvoiceLineAddInfo : DataTransformation
	{
		public CorrectDutyAndTaxInInvoiceLineAddInfo()
			: this(5000)
		{
		}

		internal CorrectDutyAndTaxInInvoiceLineAddInfo(int batchSize)
		{
			this.batchSize = batchSize;
		}
		readonly int batchSize;
		const string LastClusterKeyWaterMark = "CorrectDutyAndTaxInInvoiceLineAddInfo_LastClusterKey";

		public override string UserDescription => "Correct the duty and tax in InvoiceLine's addInfo according to CusAddInfo";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, LastClusterKeyWaterMark), out var lastClusterKey))
			{
				lastClusterKey = GetMaxClusterKey();
			}

			if (lastClusterKey > 0)
			{
				CreateIndex();
			}

			var stopWatch = Stopwatch.StartNew();
			var endClusterKeyForLog = lastClusterKey;
			manager?.ShowInfoMessage($"Batch processing starts. The start cluster key is '{lastClusterKey}'.");
			while (lastClusterKey > 0)
			{
				var startClusterKey = QueryStartClusterKey(lastClusterKey);
				ProcessBatch(startClusterKey, lastClusterKey);
				lastClusterKey = startClusterKey - 1;

				if (stopWatch.Elapsed.TotalMinutes > 1 || lastClusterKey <= 0)
				{
					manager?.ShowInfoMessage($"Invoice lines with Cluster keys ranging from {startClusterKey} to {endClusterKeyForLog} have been processed.");
					endClusterKeyForLog = lastClusterKey;

					stopWatch.Restart();
				}

				if (lastClusterKey > 0)
				{
					ExtProperty.Database.Update(Db.Connection, LastClusterKeyWaterMark, lastClusterKey.ToString());
				}
				token.ThrowIfCancellationRequested();
			}

			DeleteIndex();
			ExtProperty.Database.Delete(Db.Connection, LastClusterKeyWaterMark);
			manager?.ShowInfoMessage($"Batch processing ends.");
		}

		void CreateIndex()
		{
			var createIndexQuery = @"
IF NOT EXISTS (SELECT Name FROM sys.indexes WHERE name='CusAddInfo_Index_CorrectDutyAndTaxInInvoiceLineAddInfo' AND object_id = OBJECT_ID('dbo.CusAddInfo'))
	CREATE INDEX [CusAddInfo_Index_CorrectDutyAndTaxInInvoiceLineAddInfo] ON [dbo].[CusAddInfo] ([B7_ParentID]) INCLUDE ([B7_AddInfoData])
	WHERE ([B7_Type]='CDT' AND [B7_ParentTableCode]='JI') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)";
			Db.Connection.ExecuteNonQuery(createIndexQuery);
		}

		void DeleteIndex()
		{
			var deleteIndexQuery = @"
DROP INDEX IF EXISTS CusAddInfo_Index_CorrectDutyAndTaxInInvoiceLineAddInfo ON dbo.CusAddInfo";
			Db.Connection.ExecuteNonQuery(deleteIndexQuery);
		}

		int GetMaxClusterKey()
		{
			var result = 0;
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'"))
			{
				result = Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MAX(JE_ClusterKey), 0) maxClusterKey
FROM dbo.JobDeclaration");
			}
			return result;
		}

		int QueryStartClusterKey(int lastClusterKey)
		{
			var sql = $@"
SELECT ISNULL(MIN(InvoiceLineClusterKey.JI_ClusterKey), 0)
FROM
(
	SELECT TOP (@batchSize) JI_ClusterKey
	FROM dbo.JobComInvoiceLine
	WHERE JI_DataModel = 'CA' AND JI_ClusterKey <= @endClusterKey
	ORDER BY JI_ClusterKey DESC
)AS InvoiceLineClusterKey;";
			int startClusterKey;
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@batchSize", SqlDbType.Int, batchSize);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, lastClusterKey);
				startClusterKey = (int)cmd.ExecuteScalar();
			}
			return startClusterKey;
		}

		void ProcessBatch(int startClusterKey, int lastClusterKey)
		{
			var sql = $@"
BEGIN TRY

	BEGIN TRANSACTION

	DECLARE @PKAndAmountTable TABLE(PK UNIQUEIDENTIFIER, JI_DTYAmount DECIMAL(18, 2), DTYAmount DECIMAL(18, 2), JI_EXSAmount DECIMAL(18, 2), EXSAmount DECIMAL(18, 2), JI_SIMAmount DECIMAL(18, 2), SIMAmount DECIMAL(18, 2), JI_GSTAmount DECIMAL(18, 2), GSTAmount DECIMAL(18, 2))

	INSERT INTO @PKAndAmountTable
	SELECT * FROM
	(
		SELECT 
			JI_PK, 
			ISNULL(JI_DTYAmount, 0) JI_DTYAmount, ISNULL(DTYAmount, 0) DTYAmount, 
			ISNULL(JI_EXSAmount, 0) JI_EXSAmount, ISNULL(EXSAmount, 0) EXSAmount, 
			ISNULL(JI_SIMAmount, 0) JI_SIMAmount, ISNULL(SIMAmount, 0) SIMAmount, 
			ISNULL(JI_GSTAmount, 0) JI_GSTAmount, ISNULL(GSTAmount, 0) GSTAmount
		FROM 
			dbo.CAJobComInvoiceLine 
		LEFT JOIN
		(
			SELECT B7_ParentID,
				SUM(CASE WHEN LineOfTaxTypeData.Value = 'DTY' THEN CAST(LineOfAmountData.ValueAsDecimal AS DECIMAL(18, 2)) END) AS DTYAmount,
				SUM(CASE WHEN LineOfTaxTypeData.Value = 'EXS' THEN CAST(LineOfAmountData.ValueAsDecimal AS DECIMAL(18, 2)) END) AS EXSAmount,
				SUM(CASE WHEN LineOfTaxTypeData.Value IN ('SIM', 'ADD', 'CVD', 'SUR') THEN CAST(LineOfAmountData.ValueAsDecimal AS DECIMAL(18, 2)) END) AS SIMAmount,
				SUM(CASE WHEN LineOfTaxTypeData.Value = 'GST' THEN CAST(LineOfAmountData.ValueAsDecimal AS DECIMAL(18, 2)) END) AS GSTAmount
			FROM dbo.CusAddInfo
			CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineAsDecimal(B7_AddInfoData, 'Amount') LineOfAmountData
			CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull(B7_AddInfoData, 'TaxType') LineOfTaxTypeData
			WHERE LineOfTaxTypeData.Value IN ('DTY', 'EXS', 'SIM', 'ADD', 'CVD', 'SUR', 'GST')
				AND B7_Type = 'CDT'
				AND B7_ParentTableCode = 'JI'
				AND B7_AddInfoData LIKE '%TaxType=%'
			GROUP BY B7_ParentID
		) AS InvoiceLineDutiesAndTaxes ON InvoiceLineDutiesAndTaxes.B7_ParentID = JI_PK
		WHERE
			JI_SystemLastEditTimeUtc > '2024-03-24'
			AND JI_ClusterKey Between @startClusterKey AND @endClusterKey
	) T
	WHERE 
		JI_DTYAmount <> DTYAmount
		OR JI_EXSAmount <> EXSAmount
		OR JI_SIMAmount <> SIMAmount
		OR JI_GSTAmount <> GSTAmount

	UPDATE dbo.JobComInvoiceLine
	SET 
		JI_AddInfo = addInfo.AddInfoValue + (CASE WHEN DTYAmount <> 0 THEN '*DTYAmount=' + CAST(DTYAmount AS VARCHAR(20)) ELSE '' END),
		JI_SystemLastEditTimeUtc = GETUTCDATE(),
		JI_SystemLastEditUser = '~BP'
	FROM @PKAndAmountTable
	INNER JOIN dbo.JobComInvoiceLine ON JI_PK = PK
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'DTYAmount') addInfo
	WHERE JI_DTYAmount <> DTYAmount

	UPDATE dbo.JobComInvoiceLine
	SET 
		JI_AddInfo = addInfo.AddInfoValue + (CASE WHEN EXSAmount <> 0 THEN '*EXSAmount=' + CAST(EXSAmount AS VARCHAR(20)) ELSE '' END),
		JI_SystemLastEditTimeUtc = GETUTCDATE(),
		JI_SystemLastEditUser = '~BP'
	FROM @PKAndAmountTable
	INNER JOIN dbo.JobComInvoiceLine ON JI_PK = PK
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'EXSAmount') addInfo
	WHERE JI_EXSAmount <> EXSAmount

	UPDATE dbo.JobComInvoiceLine
	SET 
		JI_AddInfo = addInfo.AddInfoValue + (CASE WHEN SIMAmount <> 0 THEN '*SIMAmount=' + CAST(SIMAmount AS VARCHAR(20)) ELSE '' END),
		JI_SystemLastEditTimeUtc = GETUTCDATE(),
		JI_SystemLastEditUser = '~BP'
	FROM @PKAndAmountTable
	INNER JOIN dbo.JobComInvoiceLine ON JI_PK = PK
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'SIMAmount') addInfo
	WHERE JI_SIMAmount <> SIMAmount

	UPDATE dbo.JobComInvoiceLine
	SET  
		JI_AddInfo = addInfo.AddInfoValue + (CASE WHEN GSTAmount <> 0 THEN '*GSTAmount=' + CAST(GSTAmount AS VARCHAR(20)) ELSE '' END),
		JI_SystemLastEditTimeUtc = GETUTCDATE(),
		JI_SystemLastEditUser = '~BP'
	FROM @PKAndAmountTable
	INNER JOIN dbo.JobComInvoiceLine ON JI_PK = PK
	CROSS APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JI_AddInfo, 'GSTAmount') addInfo
	WHERE JI_GSTAmount <> GSTAmount

	COMMIT TRANSACTION

END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
		ROLLBACK TRANSACTION;
		THROW;
	END
END CATCH
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@startClusterKey", SqlDbType.Int, startClusterKey);
				cmd.AddParameter("@endClusterKey", SqlDbType.Int, lastClusterKey);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
