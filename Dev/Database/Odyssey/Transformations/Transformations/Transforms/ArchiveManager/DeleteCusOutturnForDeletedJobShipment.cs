using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager
{
	public class DeleteCusOutturnForDeletedJobShipment : DataTransformation
	{
		public override string UserDescription => "Remove CusOutturn and Related Records for Deleted JobShipment";

		public const int BatchSize = 50;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var watermarkString = ExtProperty.Database.Select(Db.Connection, "DeleteCusOutturnForDeletedJobShipmentWatermark");
			var watermark = watermarkString != null ? SqlFormatInfo.FromSqlDateTime(watermarkString)
				: new SqlDateTime(new DateTime(1900, 01, 01, 00, 00, 00));

			var sql = @$"
CREATE TABLE #AllOutturns
(
	C5_PK uniqueidentifier NOT NULL,
	C5_C6 uniqueidentifier NULL,
	C5_SystemCreateTimeUtc smalldatetime NULL
);

INSERT INTO #AllOutturns
SELECT TOP {BatchSize} C5_PK, C5_C6, C5_SystemCreateTimeUtc
FROM dbo.CusOutturn WHERE
(C5_SystemCreateTimeUtc IS NULL OR C5_SystemCreateTimeUtc >= @watermark)
AND
C5_ParentTableCode = 'JS'
AND NOT EXISTS (SELECT 1 FROM dbo.JobShipment WHERE JS_PK = C5_ParentID)
ORDER BY C5_SystemCreateTimeUtc;

CREATE TABLE #AllUnderbonds
(
	C4_PK uniqueidentifier NOT NULL
);

INSERT INTO #AllUnderbonds
SELECT C4_PK FROM dbo.CusUnderbond
WHERE C4_C6 IN (SELECT DISTINCT C5_C6 FROM #AllOutturns WHERE C5_C6 IS NOT NULL);

BEGIN TRY
	BEGIN TRANSACTION
	DELETE FROM dbo.CusOutturn WHERE C5_PK IN (SELECT C5_PK FROM #AllOutturns);
	DELETE FROM dbo.CusOutturn WHERE C5_C6 IN (SELECT DISTINCT C5_C6 FROM #AllOutturns WHERE C5_C6 IS NOT NULL);
	DELETE FROM dbo.CusOutturn WHERE C5_C4_Underbond IN (SELECT C4_PK FROM #AllUnderbonds);
	DELETE FROM dbo.CusUnderbond WHERE C4_PK IN (SELECT C4_PK FROM #AllUnderbonds);
	DELETE FROM dbo.CusOutturnHeader WHERE C6_PK IN (SELECT DISTINCT C5_C6 FROM #AllOutturns WHERE C5_C6 IS NOT NULL);
	COMMIT TRANSACTION
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
		ROLLBACK TRANSACTION;
		THROW;
	END
END CATCH

SELECT TOP 1 @newWatermark = C5_SystemCreateTimeUtc FROM #AllOutturns ORDER BY C5_SystemCreateTimeUtc DESC;
SELECT @numberOfOutturns = count(*) FROM #AllOutturns;"
;

			while (true)
			{
				token.ThrowIfCancellationRequested();

				using (var cmd = Db.Connection.Command(sql))
				{
					cmd.AddParameter("@watermark", SqlDbType.SmallDateTime, watermark);
					cmd.AddOutputParameter("@newWatermark", SqlDbType.SmallDateTime, 0, 0, 0, null);
					cmd.AddOutputParameter("@numberOfOutturns", SqlDbType.Int, 0, 0, 0, null);

					cmd.ExecuteNonQuery();

					var newWatermark = (DateTime)(cmd.GetParameterValue("@newWatermark") != DBNull.Value ?
						cmd.GetParameterValue("@newWatermark") : new DateTime(1900, 01, 01, 00, 00, 00));
					watermark = newWatermark;
					ExtProperty.Database.Update(Db.Connection, "DeleteCusOutturnForDeletedJobShipmentWatermark", SqlFormatInfo.ToSqlDateTimeString(newWatermark));

					var numberOfOutturns = (int)cmd.GetParameterValue("@numberOfOutturns");

					if (numberOfOutturns == 0)
					{
						ExtProperty.Database.Delete(Db.Connection, "DeleteCusOutturnForDeletedJobShipmentWatermark");
						return;
					}
				}
			}
		}
	}
}
