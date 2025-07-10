using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Xml;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

#pragma warning disable CA1810

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class PopulateJE_ValuationDateForCADDeclarations : DataTransformation
	{
		public override string UserDescription => "Populate JE_ValuationDate for CAD entries";

		public PopulateJE_ValuationDateForCADDeclarations()
			: this(1000)
		{
		}

		internal PopulateJE_ValuationDateForCADDeclarations(int batchSize)
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (IsCACustoms)
			{
				var watermark = ClusterKeyWatermark.Select() ?? ClusterKeyWatermark.CreateNew();
				var stopWatch = Stopwatch.StartNew();
				var totalProcessedCount = 0;
				var totalUpdatedCount = 0;
				var lastClusterKeyForLog = watermark.Value;
				do
				{
					var startClusterKey = watermark.ValueMinusBatchSize(BatchSize);
					var processingResult = ProcessSingleBatch(startClusterKey, watermark.Value);
					totalProcessedCount += processingResult.ProcessedCount;
					totalUpdatedCount += processingResult.UpdatedCount;
					watermark.Value = watermark.ValueMinusBatchSize(BatchSize);

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						ShowInfoMessage($"Scanned key range: {startClusterKey}-{lastClusterKeyForLog}, processed {totalProcessedCount} jobs, updated {totalUpdatedCount} records.");
						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}
				while (watermark.Value > watermark.MinClusterKey);
				ShowInfoMessage($"Processing completed, processed {totalProcessedCount} jobs, updated {totalUpdatedCount} records.");
				watermark.Delete();
			}
		}

		(int ProcessedCount, int UpdatedCount) ProcessSingleBatch(int startClusterKey, int endClusterKey)
		{
			var queryResult = QueryDeclarationAndMessages(startClusterKey, endClusterKey);
			var jobCount = queryResult.Rows.Count;
			var updatedJobCount = 0;
			if (jobCount > 0)
			{
				using var manager = Db.Connection.BeginTransactionWithManager();
				try
				{
					var xmlDoc = new XmlDocument();
					var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
					nsManager.AddNamespace("ns", "urn:wco:datamodel:WCO:Declaration:1");
					foreach (DataRow row in queryResult.Rows)
					{
						var jePK = row["JE_PK"];
						var messageText = row["EM_MessageText"] as string;
						if (!string.IsNullOrWhiteSpace(messageText))
						{
							try
							{
								xmlDoc.LoadXml(messageText);
								var dutyTaxFeeNode = xmlDoc.SelectSingleNode("/ns:DocumentMetaData/ns:Response/ns:Declaration/ns:DutyTaxFee[ns:TypeCode='TOT'][1]", nsManager);
								var dateTimeValue = dutyTaxFeeNode?.SelectSingleNode("ns:Payment/ns:DueDateTime/ns:DateTimeString", nsManager)?.InnerText;
								if (DateTime.TryParseExact(dateTimeValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var valuationDate))
								{
									UpdateValuationDate((Guid)jePK, valuationDate);
									updatedJobCount++;
								}
							}
							catch (XmlException e)
							{
								ShowInfoMessage($"Failed to parse XML: {e.Message}, job {jePK} is ignored.");
							}
						}
					}
					manager.CommitTransaction();
				}
				catch (Exception)
				{
					manager.RollbackTransaction();
					updatedJobCount = 0;
					throw;
				}
			}
			return (jobCount, updatedJobCount);
		}

		readonly int BatchSize;

		void ShowInfoMessage(string message) => manager?.ShowInfoMessage(message);

		bool IsCACustoms => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'");

		DataTable QueryDeclarationAndMessages(int startClusterKey, int endClusterKey)
		{
			var sql = $@"
SELECT
	JE_PK
	, EM_MessageText
FROM (
	SELECT
		JE_PK
		, EM_MessageText
		, RowNum = ROW_NUMBER() OVER (PARTITION BY EM_LinkUniqueID ORDER BY EM_SystemCreateTimeUtc DESC)
	FROM
		dbo.JobDeclaration WITH (FORCESEEK, INDEX(NR_UC__JE_ClusterKey))
		JOIN dbo.CusEntryHeader ON CH_ClusterKey = JE_ClusterKey
		JOIN dbo.EDIMessage ON EM_LinkUniqueID = CH_PK
	WHERE
		CH_DataModel = 'CA'
		AND CH_MessageType = 'CAD'
		AND EM_ApplicationCode = 'CAI'
		AND EM_ReceiveTransmit = 'RCV'
		AND EM_Status = 'RCV'
		AND EM_MessageSubType IN ('CLO', 'CLC')
		AND JE_DataModel = 'CA'
		AND JE_MessageType IN ('IMP', 'LVS')
		AND JE_ValuationDate IS NULL
		AND JE_ClusterKey BETWEEN @startClusterKey AND @endClusterKey
) AS TEMP
WHERE
	TEMP.RowNum = 1
OPTION (MAXDOP 1, FORCE ORDER)
";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql,
				("@startClusterKey", SqlDbType.Int, 4, (object)startClusterKey),
				("@endClusterKey", SqlDbType.Int, 4, (object)endClusterKey));
		}

		void UpdateValuationDate(Guid jePK, DateTime valuationDate)
		{
			var sql = $@"
UPDATE
	dbo.JobDeclaration
SET
	JE_ValuationDate = @valuationDate
	, JE_SystemLastEditTimeUtc = GETUTCDATE()
	, JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @jePK
	AND JE_ValuationDate IS NULL
";
			Db.Connection.ExecuteNonQuery(sql, cmd =>
			{
				cmd.AddParameter("@valuationDate", SqlDbType.DateTime, valuationDate);
				cmd.AddParameter("@jePK", SqlDbType.UniqueIdentifier, jePK);
			});
		}

		#region Watermark

		internal class ClusterKeyWatermark
		{
			const string Watermark = "PopulateJE_ValuationDateForCADDeclarations_Watermark";

			public int Value
			{
				get => _value;
				set
				{
					if (_value != value)
					{
						_value = value;
						Update();
					}
				}
			}

			int _value;

			public int ValueMinusBatchSize(int batchSize) => Math.Max(Value - batchSize, MinClusterKey);

			public readonly int MinClusterKey;

			ClusterKeyWatermark()
			{
				MinClusterKey = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(MIN(CH_ClusterKey), 0) FROM dbo.CusEntryHeader WHERE CH_MessageType = 'CAD' AND CH_DataModel = 'CA';");
			}

			public static ClusterKeyWatermark CreateNew()
			{
				return new ClusterKeyWatermark
				{
					_value = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(MAX(CH_ClusterKey), 0) FROM dbo.CusEntryHeader WHERE CH_MessageType = 'CAD' AND CH_DataModel = 'CA';")
				};
			}

			public static ClusterKeyWatermark Select()
			{
				var watermark = new ClusterKeyWatermark();
				return int.TryParse(ExtProperty.Database.Select(Db.Connection, Watermark), out watermark._value) ? watermark : null;
			}

			void Update() => ExtProperty.Database.Update(Db.Connection, Watermark, _value.ToString());

			public void Delete() => ExtProperty.Database.Delete(Db.Connection, Watermark);
		}

		#endregion
	}
}
