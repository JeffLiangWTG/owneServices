using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class PopulateCusEntryHeaderChargesThroughCADResponse : DataTransformation
	{
		readonly int batchSize;
		const string ExtPropertyLastProcessedKey = "PopulateCusEntryHeaderChargesThroughCADResponse_LastProcessedKey";

		public PopulateCusEntryHeaderChargesThroughCADResponse()
			: this(1000)
		{
		}

		internal PopulateCusEntryHeaderChargesThroughCADResponse(int batchSize)
		{
			this.batchSize = batchSize;
		}

		public override string UserDescription => "Populate CusEntryHeader charges through CAD response";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (ExistCACompany)
			{
				if (!int.TryParse(ExtProperty.Database.Select(Db.Connection, ExtPropertyLastProcessedKey), out var lastClusterKey))
				{
					lastClusterKey = MaxClusterKey;
				}
				var stopWatch = Stopwatch.StartNew();
				var totalProcessedEntryCount = 0;
				var totalCreatedChargeCount = 0;
				var lastClusterKeyForLog = lastClusterKey;

				var minClusterKey = MinClusterKey;
				while (lastClusterKey >= minClusterKey)
				{
					var startClusterKey = Math.Max(0, lastClusterKey - batchSize);
					var processingResult = ProcessSingleBatch(startClusterKey, lastClusterKey);
					totalProcessedEntryCount += processingResult.ProcessedEntryCount;
					totalCreatedChargeCount += processingResult.CreatedChargeCount;

					lastClusterKey = startClusterKey - 1;

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						ShowInfoMessage($"Scanned key range: {startClusterKey}-{lastClusterKeyForLog}, processed {totalProcessedEntryCount} entries, created {totalCreatedChargeCount} charges.");
						ExtProperty.Database.Update(Db.Connection, ExtPropertyLastProcessedKey, lastClusterKey.ToString());
						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}
				ShowInfoMessage($"Processing completed, processed {totalProcessedEntryCount} entries, created {totalCreatedChargeCount} charges.");
				ExtProperty.Database.Delete(Db.Connection, ExtPropertyLastProcessedKey);
			}
		}

		(int ProcessedEntryCount, int CreatedChargeCount) ProcessSingleBatch(int startClusterKey, int endClusterKey)
		{
			var entryAndMessages = QueryEntryAndMessages(startClusterKey, endClusterKey);
			var entryCount = entryAndMessages.Rows.Count;
			var createdChargeCount = 0;
			if (entryCount > 0)
			{
				using var manager = Db.Connection.BeginTransactionWithManager();
				try
				{
					var xmlDoc = new XmlDocument();
					var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
					nsManager.AddNamespace("ns", "urn:wco:datamodel:WCO:Declaration:1");
					foreach (DataRow row in entryAndMessages.Rows)
					{
						var chPK = row["CH_PK"];
						var messageText = row["EM_MessageText"] as string;
						if (string.IsNullOrWhiteSpace(messageText))
						{
							continue;
						}

						var chargesToBeCreated = new List<(string, decimal)>();
						try
						{
							xmlDoc.LoadXml(messageText);
							foreach (XmlNode dutyTaxFeeNode in xmlDoc.SelectNodes("/ns:DocumentMetaData/ns:Response/ns:Declaration/ns:DutyTaxFee", nsManager))
							{
								var typeCodeNode = dutyTaxFeeNode.SelectSingleNode("ns:TypeCode", nsManager);
								var chargeType = typeCodeNode?.InnerText.Trim();
								if (!string.IsNullOrEmpty(chargeType) && chargeType.Length <= 5)
								{
									string amountStr = null;
									var amount = 0m;
									if (chargeType == "TOT")
									{
										amountStr = dutyTaxFeeNode.SelectSingleNode("ns:AdValoremTaxBaseAmount", nsManager)?.InnerText.Trim();
									}
									else
									{
										amountStr = dutyTaxFeeNode.SelectSingleNode("ns:Payment/ns:PaymentAmount", nsManager)?.InnerText.Trim();
									}
									decimal.TryParse(amountStr, out amount);
									chargesToBeCreated.Add((chargeType, amount));
								}
							}
							CreateEntryCharges(chargesToBeCreated, chPK, row["CH_ClusterKey"]);
							createdChargeCount += chargesToBeCreated.Count;
						}
						catch (XmlException e)
						{
							ShowInfoMessage($"Failed to parse XML: {e.Message}, entry {chPK} is ignored.");
						}
					}
					manager.CommitTransaction();
				}
				catch (Exception)
				{
					manager.RollbackTransaction();
					createdChargeCount = 0;
					throw;
				}
			}
			return (entryCount, createdChargeCount);
		}

		DataTable QueryEntryAndMessages(int startClusterKey, int endClusterKey)
		{
			var sql = $@"
SELECT
	CH_PK
	, CH_ClusterKey
	, EM_MessageText
FROM (
	SELECT
		CH_PK
		, CH_ClusterKey
		, EM_MessageText
		, ROW_NUMBER() OVER (PARTITION BY CH_PK ORDER BY EM_SystemCreateTimeUtc DESC) AS rn
	FROM dbo.CusEntryHeader WITH (FORCESEEK, INDEX(NR_RC__CH_ClusterKey))
	LEFT JOIN dbo.CusEntryHeaderCharges ON C1_ClusterKey = CH_ClusterKey AND C1_CH = CH_PK
	JOIN dbo.EDIMessage ON EM_LinkUniqueID = CH_PK
	WHERE CH_DataModel = 'CA'
		AND CH_MessageType = 'CAD'
		AND CH_ClusterKey BETWEEN @startClusterKey AND @endClusterKey
		AND C1_PK IS NULL
		AND EM_ApplicationCode = 'CAI'
		AND EM_ReceiveTransmit = 'RCV'
		AND EM_Status = 'RCV'
		AND EM_MessageType = 'CAD'
		AND EM_MessageSubType IN ('CLO', 'CLC')
) AS TEMP
WHERE TEMP.rn = 1
OPTION (MAXDOP 1, FORCE ORDER)
";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql,
				("@startClusterKey", SqlDbType.Int, 4, (object)startClusterKey),
				("@endClusterKey", SqlDbType.Int, 4, (object)endClusterKey));
		}

		void CreateEntryCharges(List<(string chargeType, decimal amount)> chargesToBeCreated, object chPK, object chClusterKey)
		{
			if (chargesToBeCreated.Count == 0)
			{
				return;
			}
			var sql = new StringBuilder($@"
IF NOT EXISTS (SELECT 1 FROM dbo.CusEntryHeaderCharges WHERE C1_ClusterKey = '{chClusterKey}' AND C1_CH = '{chPK}')
BEGIN
	INSERT INTO dbo.CusEntryHeaderCharges
		(C1_PK, C1_IsValid, C1_ChargeType, C1_ChargeAmount, C1_CH, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser, C1_Source)
	VALUES
		{string.Join(",", chargesToBeCreated.Select(charge => $"(NEWID(), 1, '{charge.chargeType}', '{charge.amount}', '{chPK}', '{chClusterKey}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'CUS')"))}
END;
");
			Db.Connection.ExecuteNonQuery(sql.ToString());
		}

		int MaxClusterKey => Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MAX(CH_ClusterKey), 0) AS maxClusterKey
FROM dbo.CusEntryHeader
WHERE CH_MessageType = 'CAD'
    AND CH_DataModel = 'CA';");

		int MinClusterKey => Db.Connection.ExecuteScalar<int>(@"
SELECT ISNULL(MIN(CH_ClusterKey), 0) AS minClusterKey
FROM dbo.CusEntryHeader
WHERE CH_MessageType = 'CAD'
    AND CH_DataModel = 'CA';");

		bool ExistCACompany => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'");

		void ShowInfoMessage(string message) => manager?.ShowInfoMessage(message);
	}
}
