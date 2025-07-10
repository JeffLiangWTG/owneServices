using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsCycleCountLocationJobIDColumn : DataTransformation
	{
		public override string UserDescription => "Populate empty Warehouse Cycle Count Location JobID.";
		const string TransformationFromDate = "PopulateWhsCycleCountLocationJobIDColumn.WCL_SystemCreateTimeUtc.FromDate";
		const int BatchSize = 1000;

		#region OnlinePreUpgrade

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.WCL_JobID, "varchar(25)", "''"))
			{
				DbObjectCreator.CreateIndexIfNotExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.Indexes.NR_UX__WCL_JobID, "CREATE UNIQUE NONCLUSTERED INDEX [NR_UX__WCL_JobID] ON [WhsCycleCountLocation] ([WCL_JobID] ASC) WHERE WCL_JobID <> ''");
				DoTransformOnline();
			}
		}

		void DoTransformOnline()
		{
			var fromDate = GetFromDate();
			var stopWatch = Stopwatch.StartNew();

			do
			{
				fromDate = UpdateChunk(fromDate);

				if (stopWatch.Elapsed.TotalMinutes > 1)
				{
					if (fromDate.HasValue)
					{
						var fromDateString = SqlFormatInfo.ToSqlDateTimeString(fromDate.Value);
						ExtProperty.Database.Update(Db.Connection, TransformationFromDate, fromDateString);
						manager.ShowInfoMessage($"Finished processing add Job ID for Cycle Count Location to : {fromDateString}.");
					}

					stopWatch.Restart();
				}
			} while (fromDate.HasValue);
		}

		DateTime? UpdateChunk(DateTime? fromDate)
		{
			using (var command = Db.Connection.Command(GetOnlinePreUpgradeSQL(fromDate)))
			{
				if (fromDate.HasValue)
				{
					command.AddParameter("@fromDate", SqlDbType.SmallDateTime, fromDate);
				}
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					DateTime? maxUpdatedDate = reader["MaxUpdatedDate"] is DateTime dateTime ? dateTime : null;
					return maxUpdatedDate;
				}
			}
		}

		string GetOnlinePreUpgradeSQL(DateTime? fromDate)
		{
			var fromDateWhereClause = fromDate.HasValue
				? Invariant($@"AND WCL_SystemCreateTimeUtc > @fromDate")
				: "";

			var updateSQL = $@"
BEGIN TRY
	DECLARE @MaxJobIDString varchar(25)
	DECLARE @MaxJobID int
	DECLARE @MaxUpdatedDate smalldatetime

	SELECT @MaxJobIDString = MAX(WCL_JobID) FROM dbo.WhsCycleCountLocation WHERE WCL_JobID like 'WC%'
	SELECT @MaxJobID = CASE WHEN @MaxJobIDString is NULL THEN 0 ELSE CAST(SUBSTRING(@MaxJobIDString, 3, 25) AS INT) END

	;WITH WhsCycleCountLocationWithoutJobID AS
	(
		SELECT
			TOP ({BatchSize}) WITH TIES *
		FROM
			dbo.WhsCycleCountLocation
		WHERE
			WCL_JobID = ''
			{fromDateWhereClause}
		ORDER BY
			WCL_SystemCreateTimeUtc
	), MaxUpdatedDateWhsCycleCountLocationWithoutJobID AS
	(
		SELECT
			WhsCycleCountLocationWithoutJobID.*,
			MaxUpdatedDate = Max(WCL_SystemCreateTimeUtc) OVER()
		FROM
			WhsCycleCountLocationWithoutJobID
	)

	UPDATE
		MaxUpdatedDateWhsCycleCountLocationWithoutJobID
	SET
		@MaxUpdatedDate = MaxUpdatedDate,
		@MaxJobID = @MaxJobID + 1,
		WCL_JobID = 'WC' + RIGHT('00000000' + cast(@MaxJobID AS VARCHAR), 8),
		WCL_SystemLastEditTimeUtc = GetUtcDate(),
		WCL_SystemLastEditUser = '~BP'

	SELECT @MaxUpdatedDate AS MaxUpdatedDate
END TRY
BEGIN CATCH
	THROW
END CATCH
";
			return updateSQL;
		}

		#endregion

		#region OfflinePreUpgrade

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.ColumnExists(Db.Connection, WhsCycleCountLocationSchema.Constants.TableName, WhsCycleCountLocationSchema.Constants.WCL_JobID))
			{
				Db.Connection.ExecuteNonQuery(GetOfflinePreUpgradeSQL(GetFromDate()));
			}

			ExtProperty.Database.Delete(Db.Connection, TransformationFromDate);
		}

		string GetOfflinePreUpgradeSQL(DateTime? fromDate)
		{
			var fromDateWhereClause = fromDate.HasValue
				? Invariant($@"AND WCL_SystemCreateTimeUtc > @fromDate")
				: "";

			var updateSQL = $@"
DECLARE @MaxJobIDString varchar(25)
DECLARE @MaxJobID int

SELECT @MaxJobIDString = MAX(WCL_JobID) FROM dbo.WhsCycleCountLocation WHERE WCL_JobID like 'WC%'
SELECT @MaxJobID = CASE WHEN @MaxJobIDString is NULL THEN 0 ELSE CAST(SUBSTRING(@MaxJobIDString, 3, 25) AS INT) END

;WITH WhsCycleCountLocationWithoutJobID AS
(
	SELECT
		*
	FROM
		dbo.WhsCycleCountLocation
	WHERE
		WCL_JobID = ''
		{fromDateWhereClause}
)

UPDATE
	WhsCycleCountLocationWithoutJobID
SET
	@MaxJobID = @MaxJobID + 1,
	WCL_JobID = 'WC' + RIGHT('00000000' + cast(@MaxJobID AS VARCHAR), 8),
	WCL_SystemLastEditTimeUtc = GetUtcDate(),
	WCL_SystemLastEditUser = '~BP'
";

			return updateSQL;
		}

		#endregion

		#region OfflinePostUpgrade

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(CreateCycleCountLocationIDNumberFountain());
		}

		string CreateCycleCountLocationIDNumberFountain()
		{
			return $@"
IF NOT EXISTS (SELECT NULL FROM dbo.StmNums WHERE SN_Name = '{NumberFountainName}')
BEGIN
	DECLARE @MaxJobIDString varchar(25)
	DECLARE @MaxJobID int

	SELECT @MaxJobIDString = MAX(WCL_JobID) FROM dbo.WhsCycleCountLocation WHERE WCL_JobID like 'WC%'
	SELECT @MaxJobID = (CASE WHEN @MaxJobIDString is NULL THEN 0 ELSE CAST(SUBSTRING(@MaxJobIDString, 3, 25) AS INT) END) + 1

	INSERT INTO dbo.StmNums (SN_Name, SN_Value, SN_MinimumValue, SN_SystemCreateTimeUtc) VALUES ('{NumberFountainName}', @MaxJobID, 1, GETUTCDATE())
END";
		}

		const string NumberFountainName = "WhsCycleCountLocationID";

		#endregion

		DateTime? GetFromDate()
		{
			var fromDateString = ExtProperty.Database.Select(Db.Connection, TransformationFromDate);
			return string.IsNullOrEmpty(fromDateString)
				? null
				: SqlFormatInfo.TryParseFromSqlDateTime(fromDateString, out var parsedFromDate) ? parsedFromDate : null;
		}
	}
}
