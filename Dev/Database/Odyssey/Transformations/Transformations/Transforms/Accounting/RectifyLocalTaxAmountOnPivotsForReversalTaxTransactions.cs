using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	class RectifyLocalTaxAmountOnPivotsForReversalTaxTransactions : DataTransformation
	{
		public RectifyLocalTaxAmountOnPivotsForReversalTaxTransactions()
			: this(1000)
		{
		}

		internal RectifyLocalTaxAmountOnPivotsForReversalTaxTransactions(int batchSize)
			: base()
		{
			BatchSize = batchSize;
		}

		public override string UserDescription => "Rectify LocalTaxAmount on tax record-transaction line pivots for reversal tax transactions";

		const string LastProcessedGuidPropertyName = "LastProcessedGuidForRecitfyTaxAmountOnPivot";
		const string RowCountPropertyName = "TransactionHeaderRowCountForRecitfyTaxAmountOnPivot";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var rowCountString = ExtProperty.Table.Select(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, RowCountPropertyName);
			long rowCount;

			if (string.IsNullOrEmpty(rowCountString))
			{
				rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, AccTaxTransactionSchema.Constants.TableName);
				ExtProperty.Table.Update(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, RowCountPropertyName, rowCount.ToString());
			}
			else
			{
				rowCount = long.Parse(rowCountString);
			}

			if (rowCount > 0)
			{
				var lastProcessedGuidString = ExtProperty.Table.Select(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, LastProcessedGuidPropertyName);
				Guid? lastProcessedGuid = null;

				if (!string.IsNullOrEmpty(lastProcessedGuidString))
				{
					lastProcessedGuid = Guid.Parse(lastProcessedGuidString);
				}

				var guidChunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedGuid);
				var stopWatch = Stopwatch.StartNew();

				foreach (var chunk in guidChunks)
				{
					var newUpperBound = chunk.UpperBound;
					ProcessData(chunk.LowerBound, newUpperBound);

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						lastProcessedGuid = newUpperBound;
						ExtProperty.Table.Update(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, LastProcessedGuidPropertyName, lastProcessedGuid.ToString());
						manager.ShowInfoMessage($"Row count: {rowCount}, Last processed PK: {lastProcessedGuid}.");

						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}
			}

			ExtProperty.Table.Delete(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, RowCountPropertyName);
			ExtProperty.Table.Delete(Db.Connection, AccTaxTransactionSchema.Constants.SqlSchemaName, AccTaxTransactionSchema.Constants.TableName, LastProcessedGuidPropertyName);
		}

		void ProcessData(Guid lowerBoundGuid, Guid upperBoundGuid)
		{
			var sql = $@"
DECLARE @TaxTransactionsToProcessInBatch TABLE (
	ATT_PK UNIQUEIDENTIFIER,
	ATT_AH UNIQUEIDENTIFIER
)

DECLARE @ReverseTxnHeadersMatchLinkAndTaxTxnsData TABLE (
	ATT_PK UNIQUEIDENTIFIER,
	ATT_AT_TaxID UNIQUEIDENTIFIER,
	ATT_ETC UNIQUEIDENTIFIER,
	ATT_RateNumerator INT,
	ATT_RateDenominator INT, 
	ATT_A9_TaxMessage UNIQUEIDENTIFIER,
	ATT_TaxAuthorityServiceCode VARCHAR(20) COLLATE DATABASE_DEFAULT,
	AH_TransactionBelongsToGroup UNIQUEIDENTIFIER,
	AP_MatchGroupNum VARCHAR(20) COLLATE DATABASE_DEFAULT
);

DECLARE @ReverseTxnsData TABLE (
	ATT_AT_TaxID UNIQUEIDENTIFIER,
	ATT_ETC UNIQUEIDENTIFIER,
	ATT_RateNumerator INT,
	ATT_RateDenominator INT, 
	ATT_A9_TaxMessage UNIQUEIDENTIFIER,
	ATT_TaxAuthorityServiceCode VARCHAR(20) COLLATE DATABASE_DEFAULT,
	AH_TransactionBelongsToGroup UNIQUEIDENTIFIER,
	AP_MatchGroupNum VARCHAR(20) COLLATE DATABASE_DEFAULT,
	ATP_PK UNIQUEIDENTIFIER,
	AL_Sequence SMALLINT,
	AL_JH UNIQUEIDENTIFIER,
	AL_AC UNIQUEIDENTIFIER,
	AL_GB UNIQUEIDENTIFIER,
	AL_GE UNIQUEIDENTIFIER,
	AL_AT UNIQUEIDENTIFIER,
	AL_LineAmount MONEY
);


DECLARE @OriginalTxnHeadersMatchLinkAndTaxTxnsData TABLE (
	ATT_PK UNIQUEIDENTIFIER,
	ATT_AT_TaxID UNIQUEIDENTIFIER,
	ATT_ETC UNIQUEIDENTIFIER,
	ATT_RateNumerator INT,
	ATT_RateDenominator INT, 
	ATT_A9_TaxMessage UNIQUEIDENTIFIER,
	ATT_TaxAuthorityServiceCode VARCHAR(20) COLLATE DATABASE_DEFAULT,
	AH_PK UNIQUEIDENTIFIER,
	AP_MatchGroupNum VARCHAR(20) COLLATE DATABASE_DEFAULT
);

DECLARE @OriginalTxnsData TABLE (
	ATT_AT_TaxID UNIQUEIDENTIFIER,
	ATT_ETC UNIQUEIDENTIFIER,
	ATT_RateNumerator INT,
	ATT_RateDenominator INT, 
	ATT_A9_TaxMessage UNIQUEIDENTIFIER,
	ATT_TaxAuthorityServiceCode VARCHAR(20) COLLATE DATABASE_DEFAULT,
	AH_PK UNIQUEIDENTIFIER,
	AP_MatchGroupNum VARCHAR(20) COLLATE DATABASE_DEFAULT,
	ATP_LocalTaxAmount MONEY,
	AL_Sequence SMALLINT,
	AL_JH UNIQUEIDENTIFIER,
	AL_AC UNIQUEIDENTIFIER,
	AL_GB UNIQUEIDENTIFIER,
	AL_GE UNIQUEIDENTIFIER,
	AL_AT UNIQUEIDENTIFIER,
	AL_LineAmount MONEY
);

INSERT INTO @TaxTransactionsToProcessInBatch (ATT_PK, ATT_AH)
SELECT ATT_PK, ATT_AH FROM AccTaxTransaction WITH (FORCESEEK, INDEX (PK_UX__ATT_PK))
WHERE ATT_PK BETWEEN @lowerBoundGuid AND @upperBoundGuid

INSERT INTO @ReverseTxnHeadersMatchLinkAndTaxTxnsData (ATT_PK, ATT_AT_TaxID, ATT_ETC, ATT_RateNumerator, ATT_RateDenominator, ATT_A9_TaxMessage, ATT_TaxAuthorityServiceCode, AH_TransactionBelongsToGroup, AP_MatchGroupNum)
SELECT tax.ATT_PK, tax.ATT_AT_TaxID, tax.ATT_ETC, tax.ATT_RateNumerator, tax.ATT_RateDenominator, tax.ATT_A9_TaxMessage, tax.ATT_TaxAuthorityServiceCode, txnHeader.AH_TransactionBelongsToGroup, matchLink.AP_MatchGroupNum
FROM @TaxTransactionsToProcessInBatch AS batch
JOIN dbo.AccTaxTransaction AS tax
	ON tax.ATT_AH = batch.ATT_AH AND tax.ATT_PK = batch.ATT_PK
JOIN dbo.AccTransactionHeader AS txnheader
	ON txnHeader.AH_PK = tax.ATT_AH
LEFT JOIN dbo.AccTransactionMatchLink AS matchLink
	ON matchLink.AP_AH = txnHeader.AH_PK
WHERE 
	txnHeader.AH_IsCancelled = 1
	AND txnHeader.AH_TransactionBelongsToGroup IS NOT NULL

IF EXISTS (SELECT 1 FROM @ReverseTxnHeadersMatchLinkAndTaxTxnsData)
BEGIN

	INSERT INTO @ReverseTxnsData (ATT_AT_TaxID, ATT_ETC, ATT_RateNumerator, ATT_RateDenominator, ATT_A9_TaxMessage, ATT_TaxAuthorityServiceCode, AH_TransactionBelongsToGroup, AP_MatchGroupNum, ATP_PK, AL_Sequence, AL_JH, AL_AC, AL_GB, AL_GE, AL_AT, AL_LineAmount)
	SELECT reverseData.ATT_AT_TaxID, reverseData.ATT_ETC, reverseData.ATT_RateNumerator, reverseData.ATT_RateDenominator, reverseData.ATT_A9_TaxMessage, reverseData.ATT_TaxAuthorityServiceCode, reverseData.AH_TransactionBelongsToGroup, reverseData.AP_MatchGroupNum, taxPivot.ATP_PK, line.AL_Sequence, line.AL_JH, line.AL_AC, line.AL_GB, line.AL_GE, line.AL_AT, line.AL_LineAmount
	FROM @ReverseTxnHeadersMatchLinkAndTaxTxnsData AS reverseData
	LEFT JOIN dbo.AccTaxRecordTransactionLinePivot AS taxPivot
		ON reverseData.ATT_PK = taxPivot.ATP_ATT
	LEFT JOIN dbo.AccTransactionLines AS line
		ON line.AL_PK = taxPivot.ATP_AL_TransactionLine

	INSERT INTO @OriginalTxnHeadersMatchLinkAndTaxTxnsData (ATT_PK, ATT_AT_TaxID, ATT_ETC, ATT_RateNumerator, ATT_RateDenominator, ATT_A9_TaxMessage, ATT_TaxAuthorityServiceCode, AH_PK, AP_MatchGroupNum)
	SELECT tax.ATT_PK, tax.ATT_AT_TaxID, tax.ATT_ETC, tax.ATT_RateNumerator, tax.ATT_RateDenominator, tax.ATT_A9_TaxMessage, tax.ATT_TaxAuthorityServiceCode, tax.ATT_AH, matchLink.AP_MatchGroupNum
	FROM dbo.AccTaxTransaction AS tax
	INNER JOIN dbo.AccTransactionMatchLink AS matchLink
		ON matchLink.AP_AH = tax.ATT_AH
	WHERE 
		matchLink.AP_MatchGroupNum IN (SELECT AP_MatchGroupNum FROM @ReverseTxnHeadersMatchLinkAndTaxTxnsData)
		AND tax.ATT_AH IN (SELECT AH_TransactionBelongsToGroup FROM @ReverseTxnHeadersMatchLinkAndTaxTxnsData)

	INSERT INTO @OriginalTxnsData (ATT_AT_TaxID, ATT_ETC, ATT_RateNumerator, ATT_RateDenominator, ATT_A9_TaxMessage, ATT_TaxAuthorityServiceCode, AH_PK, AP_MatchGroupNum, ATP_LocalTaxAmount, AL_Sequence, AL_JH, AL_AC, AL_GB, AL_GE, AL_AT, AL_LineAmount)
	SELECT originalData.ATT_AT_TaxID, originalData.ATT_ETC, originalData.ATT_RateNumerator, originalData.ATT_RateDenominator, originalData.ATT_A9_TaxMessage, originalData.ATT_TaxAuthorityServiceCode, originalData.AH_PK, originalData.AP_MatchGroupNum, taxPivot.ATP_LocalTaxAmount, line.AL_Sequence, line.AL_JH, line.AL_AC, line.AL_GB, line.AL_GE, line.AL_AT, line.AL_LineAmount
	FROM @OriginalTxnHeadersMatchLinkAndTaxTxnsData AS originalData
	LEFT JOIN dbo.AccTaxRecordTransactionLinePivot AS taxPivot
		ON originalData.ATT_PK = taxPivot.ATP_ATT
	LEFT JOIN dbo.AccTransactionLines AS line
		ON line.AL_PK = taxPivot.ATP_AL_TransactionLine

	ALTER TABLE dbo.AccTaxRecordTransactionLinePivot DISABLE TRIGGER TG_AccTaxRecordTransactionLinePivot_Update;
	
	BEGIN TRY
		WITH updateData AS (
			SELECT reversal.ATP_PK as pk, original.ATP_LocalTaxAmount as localTaxAmount, ROW_NUMBER() OVER (PARTITION BY reversal.ATP_PK ORDER BY reversal.ATP_PK) AS rowNumber
			FROM @ReverseTxnsData AS reversal
			JOIN @OriginalTxnsData AS original
			ON
				reversal.AH_TransactionBelongsToGroup = original.AH_PK
			WHERE
				reversal.AP_MatchGroupNum = original.AP_MatchGroupNum
				AND reversal.ATT_AT_TaxId = original.ATT_AT_TaxId
				AND reversal.ATT_ETC = original.ATT_ETC
				AND reversal.ATT_RateNumerator = original.ATT_RateNumerator
				AND reversal.ATT_RateDenominator = original.ATT_RateDenominator
				AND reversal.ATT_TaxAuthorityServiceCode = original.ATT_TaxAuthorityServiceCode
				AND reversal.AL_Sequence = original.AL_Sequence
				AND (COALESCE(reversal.AL_JH, original.AL_JH) IS NULL OR (reversal.AL_JH = original.AL_JH))
				AND (COALESCE(reversal.AL_AC, original.AL_AC) IS NULL OR (reversal.AL_AC = original.AL_AC))
				AND (COALESCE(reversal.AL_GB, original.AL_GB) IS NULL OR (reversal.AL_GB = original.AL_GB))
				AND (COALESCE(reversal.AL_GE, original.AL_GE) IS NULL OR (reversal.AL_GE = original.AL_GE))
				AND (COALESCE(reversal.AL_AT, original.AL_AT) IS NULL OR (reversal.AL_AT = original.AL_AT))
				AND (COALESCE(reversal.ATT_A9_TaxMessage, original.ATT_A9_TaxMessage) IS NULL OR (reversal.ATT_A9_TaxMessage = original.ATT_A9_TaxMessage))
				AND (ABS(reversal.AL_LineAmount) = ABS(original.AL_LineAmount))
		)
		UPDATE dbo.AccTaxRecordTransactionLinePivot
		SET ATP_LocalTaxAmount = 
			CASE
				WHEN updateData.localTaxAmount <> 0 THEN (-1 * updateData.localTaxAmount)
				ELSE 0
			END,
			ATP_SystemLastEditTimeUtc = GETUTCDATE(),
			ATP_SystemLastEditUser = '~BP'
		FROM updateData
		WHERE 
			updateData.rowNumber = 1
			AND ATP_PK = updateData.pk
	END TRY
	BEGIN CATCH
		ALTER TABLE dbo.AccTaxRecordTransactionLinePivot ENABLE TRIGGER TG_AccTaxRecordTransactionLinePivot_Update; 
		THROW
	END CATCH

	ALTER TABLE dbo.AccTaxRecordTransactionLinePivot ENABLE TRIGGER TG_AccTaxRecordTransactionLinePivot_Update
END
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@lowerBoundGuid", lowerBoundGuid, AccTaxTransactionSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@upperBoundGuid", upperBoundGuid, AccTaxTransactionSchema.PK);
				cmd.ExecuteNonQuery();
			}
		}
			readonly int BatchSize;
	}
}
