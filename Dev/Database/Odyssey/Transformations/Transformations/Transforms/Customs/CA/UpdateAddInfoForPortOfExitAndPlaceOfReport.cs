using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class UpdateAddInfoForPortOfExitAndPlaceOfReport : DataTransformation
	{
		public override string UserDescription => "Update codes for Port of Exit and Place of Report on JobDeclaration and delete GenAddOnColumn";

		public UpdateAddInfoForPortOfExitAndPlaceOfReport()
			: this(1000, 25D)
		{
		}

		internal UpdateAddInfoForPortOfExitAndPlaceOfReport(int batchSize, double offlineTransformationTimeLimitInSeconds)
		{
			BatchSize = batchSize;
			OfflineTransformationTimeLimitInSeconds = offlineTransformationTimeLimitInSeconds;
		}

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, JobDeclarationSchema.Constants.TableName)
				&& IsCACustoms)
			{
				var end = GetMaxClusterKey();
				var start = GetNextClusterKeyRangeRollingBackward(end);

				do
				{
					using (var cmd = Db.Connection.Command(CleanUpBadDataQuery))
					{
						cmd.AddParameter("@Start", SqlDbType.Int, start);
						cmd.AddParameter("@End", SqlDbType.Int, end);
						cmd.ExecuteNonQuery();
					}
					end = start;
					start = GetNextClusterKeyRangeRollingBackward(start);
				}
				while (start != end);
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			if (IsCACustoms)
			{
				var stopWatch = Stopwatch.StartNew();
				Db.Connection.ExecuteNonQuery(DeleteStripNonAlphaCharsFunction);
				Db.Connection.ExecuteNonQuery(CreateStripNonAlphaCharsFunction);
				Db.Connection.ExecuteNonQuery(UpdateStmDataQuery);

				using (DataTransformationHelper.SuspendTriggerIfExists("TG_JobDeclaration_UpdateAutoVersion", JobDeclarationSchema.Constants.TableName))
				{
					var query = CommentOutSomeCode(UpdateJobDeclarationQuery, MaxDop);
					var watermark = ClusterKeyWatermark.CreateNew();
					var batchSize = 100;
					var batchExecutionTime = 0D;
					var end = watermark.Value;
					var start = GetNextClusterKeyRangeRollingBackward(end, batchSize);

					do
					{
						var stopWatchForBatch = Stopwatch.StartNew();
						using (var cmd = Db.Connection.Command(query))
						{
							cmd.AddParameter("@Start", SqlDbType.Int, start);
							cmd.AddParameter("@End", SqlDbType.Int, end);
							cmd.ExecuteNonQuery();
						}
						end = start;
						start = GetNextClusterKeyRangeRollingBackward(start, batchSize);
						stopWatchForBatch.Stop();
						batchExecutionTime = Math.Max(batchExecutionTime, stopWatchForBatch.Elapsed.TotalSeconds);
					}
					while (start != end && stopWatch.Elapsed.TotalSeconds + batchExecutionTime < OfflineTransformationTimeLimitInSeconds);

					watermark.Value = end;
				}
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (IsCACustoms)
			{
				var watermark = ClusterKeyWatermark.Select();

				if (watermark != null)
				{
					var end = watermark.Value;
					var start = GetNextClusterKeyRangeRollingBackward(end);
					var query = CommentOutSomeCode(UpdateJobDeclarationQuery, AutoVersion);

					Db.Connection.ExecuteNonQuery(DeleteStripNonAlphaCharsFunction);
					Db.Connection.ExecuteNonQuery(CreateStripNonAlphaCharsFunction);

					do
					{
						using (var cmd = Db.Connection.Command(query))
						{
							cmd.AddParameter("@Start", SqlDbType.Int, start);
							cmd.AddParameter("@End", SqlDbType.Int, end);
							cmd.ExecuteNonQuery();
						}
						end = start;
						start = watermark.Value = GetNextClusterKeyRangeRollingBackward(start);
						token.ThrowIfCancellationRequested();
					}
					while (start != end);

					watermark.Delete();
					Db.Connection.ExecuteNonQuery(DeleteStripNonAlphaCharsFunction);
				}

				while (Db.Connection.ExecuteNonQuery(DeleteGenAddonColumnQuery) > 0)
				{
					token.ThrowIfCancellationRequested();
				}
			}
		}

		static int GetMaxClusterKey()
		{
			return Db.Connection.ExecuteScalar<int>("SELECT ISNULL(MAX(JE_ClusterKey), 0) FROM dbo.JobDeclaration");
		}

		int GetNextClusterKeyRangeRollingBackward(int clusterKey, int batchSizeOverride = 0)
		{
			if (batchSizeOverride == 0)
			{
				batchSizeOverride = BatchSize;
			}

			var sql = @"
SELECT ISNULL(MIN(JE_ClusterKey), @ClusterKey) FROM
(
	SELECT TOP (@BatchSize) JE_ClusterKey
	FROM dbo.JobDeclaration WHERE JE_ClusterKey < @ClusterKey
	ORDER BY JE_ClusterKey DESC
) AS range
";
			return Db.Connection.ExecuteScalar<int>(sql, x =>
			{
				x.AddParameter("@BatchSize", SqlDbType.Int, batchSizeOverride);
				x.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
			});
		}

		string CommentOutSomeCode(string query, params string[] codeToComment)
		{
			Array.ForEach(codeToComment, x => query = query.Replace(x, "/* " + x + " */"));
			return query;
		}

		internal class ClusterKeyWatermark
		{
			const string Watermark = "UpdateAddInfoForPortOfExitAndPlaceOfReport_Watermark";

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

			ClusterKeyWatermark()
			{
			}

			public static ClusterKeyWatermark CreateNew()
			{
				var watermark = new ClusterKeyWatermark();

				try
				{
					watermark.Value = GetMaxClusterKey();
				}
				catch (ExecuteScalarReturnedNullException)
				{
					watermark.Value = 0;
				}

				return watermark;
			}

			public static ClusterKeyWatermark Select()
			{
				var watermark = new ClusterKeyWatermark();
				return int.TryParse(ExtProperty.Database.Select(Db.Connection, Watermark), out watermark._value) ? watermark : null;
			}

			void Update() => ExtProperty.Database.Update(Db.Connection, Watermark, _value.ToString());

			public void Delete() => ExtProperty.Database.Delete(Db.Connection, Watermark);
		}

		readonly int BatchSize;

		readonly double OfflineTransformationTimeLimitInSeconds;

		internal bool IsCACustoms => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'");

		string CleanUpBadDataQuery =>
@"
UPDATE
	dbo.JobDeclaration
SET
	JE_AddInfo = b.AddInfoValue
		+ COALESCE('*PortOfExit=' + NULLIF(result.PortOfExitValue, ''), '')
		+ COALESCE('*PlaceOfReport=' + NULLIF(result.PlaceOfReportValue, ''), '')
	, JE_SystemLastEditTimeUtc = GETUTCDATE()
	, JE_SystemLastEditUser = '~BP'
FROM
	dbo.JobDeclaration 
	OUTER APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'PortOfExit') AS a
	OUTER APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(a.AddInfoValue, 'PlaceOfReport') AS b
	CROSS APPLY
	(
		SELECT
			PortOfExitValue = 
				CASE
					WHEN a.Value LIKE '{Inv%' THEN NULL ELSE a.Value
				END
			, PlaceOfReportValue = 
				CASE
					WHEN b.Value LIKE '{Inv%' THEN NULL ELSE b.Value
				END
	) AS result
WHERE
	(JE_AddInfo LIKE '%PortOfExit={Inv%' OR JE_AddInfo LIKE '%PlaceOfReport={Inv%')
	AND JE_MessageType = 'EXP'
	AND JE_DataModel = 'CA'
	AND JE_ClusterKey BETWEEN @Start AND @End
OPTION (MAXDOP 1)
";

		string AutoVersion => ", JE_AutoVersion = (JE_AutoVersion + 1) % 32768";

		string MaxDop => "OPTION (MAXDOP 1)";

		string DeleteStripNonAlphaCharsFunction => @"DROP FUNCTION IF EXISTS dbo.StripNonAlphaChars";

		string CreateStripNonAlphaCharsFunction => @"
CREATE FUNCTION dbo.StripNonAlphaChars (@Temp VARCHAR(4000))
RETURNS VARCHAR(4000)
AS
BEGIN
    DECLARE @KeepValues AS VARCHAR(20) = '%[^a-z^0-9]%'
    WHILE PATINDEX(@KeepValues, @Temp) > 0
        SET @Temp = STUFF(@Temp, PATINDEX(@KeepValues, @Temp), 1, '')
    RETURN @Temp
END
";

		protected virtual string UpdateJobDeclarationQuery =>
$@"
UPDATE
	dbo.JobDeclaration
SET
	JE_AddInfo = b.AddInfoValue 
		+ COALESCE('*PortOfExit=' + NULLIF(result.PortOfExitValue, ''), '')
		+ COALESCE('*PlaceOfReport=' + NULLIF(result.PlaceOfReportValue, ''), '')
	, JE_SystemLastEditTimeUtc = GETUTCDATE()
	, JE_SystemLastEditUser = '~BP'
	{AutoVersion}
FROM
	dbo.JobDeclaration
	OUTER APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(JE_AddInfo, 'PortOfExit') AS a
	OUTER APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(a.AddInfoValue, 'PlaceOfReport') AS b
	LEFT JOIN dbo.RefDbEntCA_CACPortOfExit ON dbo.StripNonAlphaChars(CP_Code) = dbo.StripNonAlphaChars(a.Value)
	LEFT JOIN dbo.RefDbEntCA_CACPlaceOfReport ON dbo.StripNonAlphaChars(CR_Code) = dbo.StripNonAlphaChars(b.Value)
	CROSS APPLY
	(
		SELECT
			PortOfExitValue = 
				CASE
					WHEN CP_OfficialCode IS NOT NULL THEN FORMAT(CONVERT(INT, CP_OfficialCode), 'D4')
					ELSE LEFT(a.Value , 4)
				END
			, PlaceOfReportValue = 
				CASE
					WHEN CR_OfficialCode IS NOT NULL THEN FORMAT(CONVERT(INT, CR_OfficialCode), 'D4') 
					ELSE LEFT(b.Value , 4)
				END
	) AS result
WHERE
	JE_DataModel = 'CA'
	AND JE_MessageType = 'EXP'
	AND JE_ClusterKey BETWEEN @Start AND @End
	AND (JE_AddInfo LIKE '%PortOfExit=%' OR JE_AddInfo LIKE '%PlaceOfReport=%')
	AND (result.PortOfExitValue <> a.Value OR result.PlaceOfReportValue <> b.Value)
{MaxDop}
";

		string UpdateStmDataQuery =>
@"
UPDATE
	dbo.StmData
SET 
	SD_BinaryValue = CONVERT(varbinary(max),  CONVERT(nvarchar(4), FORMAT(CONVERT(int, CR_OfficialCode), 'D4')))
	, SD_SystemLastEditTimeUtc = GETUTCDATE()
	, SD_SystemLastEditUser = '~BP'
FROM
	dbo.StmData
	JOIN dbo.RefDbEntCA_CACPlaceOfReport ON dbo.StripNonAlphaChars(CR_Code) = dbo.StripNonAlphaChars(CONVERT(nvarchar(50),  SD_BinaryValue))
WHERE
	SD_Name = 'DataLoadingModuleDefaultPlaceOfReport'
";

		string DeleteGenAddonColumnQuery =>
@"
DELETE TOP(1000)
	dbo.GenAddOnColumn
WHERE
	XA_Name IN ('CA_PortOfExit', 'CA_PlaceOfReport')
	AND XA_ParentTableCode = 'JE'
OPTION (MAXDOP 1)
";
	}
}
