using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class UpdateISFJobBF_JS_Shipment : DataTransformation
	{
		public override string UserDescription => "Update ISF jobs which have wrong shipment links.";

		const int batchSizeDays = 30;
		internal readonly string LastKeyWaterMark = "LastKeyWaterMark";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastKey = LastProcessedISFHeaderCreateTimeUtcWatermark();
			var endDate = Db.Connection.ExecuteScalar<DateTime>("SELECT GETUTCDATE()").Date.AddDays(1);

			while (lastKey < endDate)
			{
				var newLastKey = lastKey.AddDays(batchSizeDays);
				ProcessBatch(lastKey, newLastKey);
				ExtProperty.Database.Update(Db.Connection, LastKeyWaterMark, SqlFormatInfo.ToSqlDateString(newLastKey));
				lastKey = newLastKey;

				token.ThrowIfCancellationRequested();
			}

			ExtProperty.Database.Delete(Db.Connection, LastKeyWaterMark);
		}

		void ProcessBatch(DateTime startDate, DateTime endDate)
		{
			var query = @"
UPDATE dbo.CusISFHeader
SET BF_JS_Shipment = NULL, BF_SystemLastEditTimeUtc = GetUtcDate(), BF_SystemLastEditUser= '~BP'
FROM dbo.CusISFHeader
JOIN dbo.JobShipment ON JS_PK = BF_JS_Shipment
	WHERE BF_JS_Shipment IS NOT NULL
	AND BF_SystemCreateTimeUtc BETWEEN @startDate AND @endDate
	AND JS_ShipmentType != 'HVL'
OPTION (MAXDOP 1)
";

			using (var cmd = Db.Connection.Command(query))
			{
				cmd.AddParameter("@startDate", SqlDbType.DateTime, startDate);
				cmd.AddParameter("@endDate", SqlDbType.DateTime, endDate);
				cmd.ExecuteNonQuery();
			}
		}

		DateTime LastProcessedISFHeaderCreateTimeUtcWatermark()
		{
			var startDate = new DateTime(2022, 2, 1);

			var lastEndDateStr = ExtProperty.Database.Select(Db.Connection, LastKeyWaterMark);
			if (string.IsNullOrEmpty(lastEndDateStr))
			{
				return startDate;
			}

			return SqlFormatInfo.FromSqlDate(lastEndDateStr);
		}
	}
}
