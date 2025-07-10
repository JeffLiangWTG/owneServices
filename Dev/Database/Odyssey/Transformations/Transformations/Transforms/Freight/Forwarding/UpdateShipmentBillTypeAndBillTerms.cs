using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class UpdateShipmentBillTypeAndBillTerms : DataTransformation
	{
		public override string UserDescription => "Update JobShipment JS_ElectronicBillOfLadingType and JS_ElectronicBillOfLadingTerms";

		const string StoredFromUniqueConsignRef = "UpdateShipmentBillTypeAndBillTerms.FromUniqueConsignRef";
		const string StoredTotalUpdatedCount = "UpdateShipmentBillTypeAndBillTerms.TotalUpdatedCount";
		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var fromUniqueConsignRef = ExtProperty.Database.Select(Db.Connection, StoredFromUniqueConsignRef) ?? string.Empty;
			var totalUpdatedCountString = ExtProperty.Database.Select(Db.Connection, StoredTotalUpdatedCount);
			var totalUpdatedCount = !string.IsNullOrEmpty(totalUpdatedCountString) ? int.Parse(totalUpdatedCountString) : 0;

			var loggingStopWatch = Stopwatch.StartNew();
			while (fromUniqueConsignRef != null)
			{
				var (updatedCount, newFromUniqueConsignRef) = UpdateChunk(fromUniqueConsignRef);
				fromUniqueConsignRef = newFromUniqueConsignRef;
				totalUpdatedCount += updatedCount;

				if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 1)
				{
					if (newFromUniqueConsignRef != null)
					{
						ExtProperty.Database.Update(Db.Connection, StoredFromUniqueConsignRef, newFromUniqueConsignRef);
						ExtProperty.Database.Update(Db.Connection, StoredTotalUpdatedCount, totalUpdatedCount.ToString());
						manager?.ShowInfoMessage($"Updated {totalUpdatedCount} JobShipment rows.");
					}

					token.ThrowIfCancellationRequested();
					loggingStopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, StoredFromUniqueConsignRef);
			ExtProperty.Database.Delete(Db.Connection, StoredTotalUpdatedCount);
		}

		(int, string) UpdateChunk(string fromUniqueConsignRef)
		{
			string newFromUniqueConsignRef = null;
			var updatedCount = 0;

			var updateSql = $@"
DROP TABLE IF EXISTS #SHIPMENTSWITHMAX;

WITH SHIPMENTS AS (
	SELECT TOP {BatchSize}
		JS_UniqueConsignRef as UniqueConsignRef,
		JS_ElectronicBillOfLadingType as ElectronicBillOfLadingType,
		JS_ElectronicBillOfLadingTerms as ElectronicBillOfLadingTerms,
		JS_TransportMode as TransportMode
	FROM dbo.JobShipment
	WHERE JS_UniqueConsignRef > @FromUniqueConsignRef
)

SELECT UniqueConsignRef, ElectronicBillOfLadingType, ElectronicBillOfLadingTerms, TransportMode
INTO #SHIPMENTSWITHMAX
FROM SHIPMENTS
OPTION (MAXDOP 1)

CREATE CLUSTERED INDEX [NR_RC__TempUniqueConsignRef] ON [#SHIPMENTSWITHMAX] ([UniqueConsignRef] ASC)

UPDATE dbo.JobShipment
SET 
	JS_ElectronicBillOfLadingType = CASE 
				WHEN JS_ElectronicBillOfLadingType = '' THEN 'STR'
				ELSE JS_ElectronicBillOfLadingType 
			END,
    JS_ElectronicBillOfLadingTerms = CASE 
				WHEN JS_ElectronicBillOfLadingTerms = '' THEN 'NTR' 
				ELSE JS_ElectronicBillOfLadingTerms 
			END,
	JS_SystemLastEditTimeUtc = GetUtcDate(),
	JS_SystemLastEditUser = '~BP'
FROM
	#SHIPMENTSWITHMAX
	JOIN dbo.JobShipment on UniqueConsignRef = JS_UniqueConsignRef
WHERE
	(ElectronicBillOfLadingType = '' OR ElectronicBillOfLadingTerms = '')
	AND TransportMode IN ('SEA', 'FSA')
OPTION (MAXDOP 1)

SELECT TOP 1 UniqueConsignRef AS NewFromUniqueConsignRef, @@ROWCOUNT AS UpdatedCount FROM #SHIPMENTSWITHMAX ORDER BY UniqueConsignRef DESC
";

			using (var command = Db.Connection.Command(updateSql))
			{
				command.AddParameterBasedOnDbColumn("@FromUniqueConsignRef", fromUniqueConsignRef, JobShipmentSchema.JS_UniqueConsignRef);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						var readerNewFromUniqueConsignRef = reader["NewFromUniqueConsignRef"];
						updatedCount = (int)reader["UpdatedCount"];
						newFromUniqueConsignRef = readerNewFromUniqueConsignRef == DBNull.Value ? null : (string)readerNewFromUniqueConsignRef;
					}
				}
			}

			return (updatedCount, newFromUniqueConsignRef);
		}
	}
}
