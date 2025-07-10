using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class RemoveDuplicatedCMRForConsolAndContainer : DataTransformation
	{
		public override string UserDescription => "Remove duplicated CMR reference numbers for consols and containers.";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedEndDate = GetLastEndDate();
			var endDate = Db.Connection.ExecuteScalar<DateTime>("SELECT GETUTCDATE()").Date.AddDays(1);

			while (lastProcessedEndDate < endDate)
			{
				ProcessDate(lastProcessedEndDate);

				lastProcessedEndDate = lastProcessedEndDate.AddDays(1);
				ExtProperty.Database.Update(Db.Connection, lastEndDatePropertyName, SqlFormatInfo.ToSqlDateString(lastProcessedEndDate));

				token.ThrowIfCancellationRequested();
			}

			ExtProperty.Database.Delete(Db.Connection, lastEndDatePropertyName);
		}

		#region Implementation

		DateTime GetLastEndDate()
		{
			var startDate = new DateTime(2023, 11, 1);

			var lastEndDateStr = ExtProperty.Database.Select(Db.Connection, lastEndDatePropertyName);
			if (string.IsNullOrEmpty(lastEndDateStr))
			{
				return startDate;
			}

			return SqlFormatInfo.FromSqlDate(lastEndDateStr);
		}

		void ProcessDate(DateTime startDate)
		{
			var sql = @"
DROP TABLE IF EXISTS #RemoveDuplicatedCMRForConsolAndContainer_DayOfData;
DROP TABLE IF EXISTS #RemoveDuplicatedCMRForConsolAndContainer_WillDelete;

CREATE TABLE #RemoveDuplicatedCMRForConsolAndContainer_DayOfData (
    CE_ParentID UNIQUEIDENTIFIER
);

CREATE TABLE #RemoveDuplicatedCMRForConsolAndContainer_WillDelete (
    CE_PK UNIQUEIDENTIFIER
);

WITH DayOfData AS
(
	SELECT DISTINCT CE_ParentID
	FROM dbo.CusEntryNum WITH (INDEX(NR_RX__CE_SystemCreateTimeUTC) FORCESEEK)
	WHERE
		CE_SystemCreateTimeUTC BETWEEN @dateStart and @dateEnd
		AND CE_ParentTable IN ('JobConsol', 'JobContainer')
		AND CE_Category = 'OTH'
		AND CE_EntryType = 'CMR'
		AND CE_EntryIsSystemGenerated = 1
)
INSERT INTO #RemoveDuplicatedCMRForConsolAndContainer_DayOfData(CE_ParentID) SELECT CE_ParentID from DayOfData
OPTION (MAXDOP 1);

DECLARE @RowCount int = 2000000000;
WITH MightDelete AS
(
	SELECT TOP (@RowCount)
		CE_PK,
		ROW_Number() OVER (PARTITION BY cen.CE_ParentID, CE_EntryNum ORDER BY CE_SystemCreateTimeUTC) RN
	FROM #RemoveDuplicatedCMRForConsolAndContainer_DayOfData dod
	JOIN dbo.CusEntryNum cen with (index(NR_RC__CE_ParentID_CE_EntryType_CE_RN_NKCountryCode) FORCESEEK) ON dod.CE_ParentID = cen.CE_ParentID
	WHERE
		cen.CE_ParentTable IN ('JobConsol', 'JobContainer')
		AND cen.CE_Category = 'OTH'
		AND cen.CE_EntryType = 'CMR'
		AND cen.CE_EntryIsSystemGenerated = 1
)
INSERT INTO #RemoveDuplicatedCMRForConsolAndContainer_WillDelete(CE_PK) SELECT CE_PK from MightDelete
WHERE RN > 1
OPTION (MAXDOP 1);

DELETE FROM dbo.CusEntryNum
WHERE CE_PK IN (SELECT CE_PK FROM #RemoveDuplicatedCMRForConsolAndContainer_WillDelete);

DROP TABLE #RemoveDuplicatedCMRForConsolAndContainer_DayOfData;
DROP TABLE #RemoveDuplicatedCMRForConsolAndContainer_WillDelete;
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@dateStart", startDate, CusEntryNumSchema.CE_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@dateEnd", startDate.AddDays(1), CusEntryNumSchema.CE_SystemCreateTimeUtc);
				cmd.ExecuteNonQuery();
			}
		}

		const string lastEndDatePropertyName = "LastEndDateToRemoveDuplicatedCMRForConsolAndContainer";

		#endregion
	}
}
