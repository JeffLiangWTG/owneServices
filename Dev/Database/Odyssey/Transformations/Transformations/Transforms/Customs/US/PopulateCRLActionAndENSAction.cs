using System;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable CW1061		// Do not use System.DateTime.UtcNow Rule

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class PopulateCRLActionAndENSAction : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Populate JE_AddInfo with US_CRLAction and US_ENSAction";

		public PopulateCRLActionAndENSAction()
			: this(1000)
		{
		}

		internal PopulateCRLActionAndENSAction(int batchSize)
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, JobDeclarationSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, EDIMessageSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, StmALogSchema.Constants.TableName)
				&& DbObjectCreator.TableExists(Db.Connection, GlbCompanySchema.Constants.TableName)
				&& IsUSCustoms)
			{
				DateTimeWatermarkOfflinePostUpgrade.CreateNew();
				var watermark = ClusterKeyWatermarkOnlinePreUpgrade.Select() ?? ClusterKeyWatermarkOnlinePreUpgrade.CreateNew(ClusterKeyWatermarkOnlinePostUpgrade.CreateNew().Value);
				var start = watermark.Value;
				var end = GetNextClusterKeyRangeRollingForward(start);

				do
				{
					using (var cmd = Db.Connection.Command(GetMainQuery(SearchConditionByClusterKey)))
					{
						cmd.AddParameter("@Start", SqlDbType.Int, start);
						cmd.AddParameter("@End", SqlDbType.Int, end);
						cmd.ExecuteNonQuery();
					}
					watermark.Value = start = end;
					end = GetNextClusterKeyRangeRollingForward(start);
				}
				while (start != end);

				watermark.Delete();
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			DateTimeWatermarkOfflinePostUpgrade watermark;
			if (IsUSCustoms && (watermark = DateTimeWatermarkOfflinePostUpgrade.Select()) != null)
			{
				using (var cmd = Db.Connection.Command(GetMainQuery(SearchConditionBySystemLastEditTimeUtc)))
				{
					cmd.AddParameter("@Date", SqlDbType.SmallDateTime, watermark.Value);
					cmd.ExecuteNonQuery();
				}

				watermark.Delete();
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			ClusterKeyWatermarkOnlinePostUpgrade watermark;
			if (IsUSCustoms && (watermark = ClusterKeyWatermarkOnlinePostUpgrade.Select()) != null)
			{
				var end = watermark.Value;
				var start = GetNextClusterKeyRangeRollingBackward(end);

				do
				{
					ShowInfoMessage($"Processing batch with cluster key ranging from {start} to {end}...");
					using (var cmd = Db.Connection.Command(GetMainQuery(SearchConditionByClusterKey)))
					{
						cmd.AddParameter("@Start", SqlDbType.Int, start);
						cmd.AddParameter("@End", SqlDbType.Int, end);
						ShowInfoMessage($"\t{cmd.ExecuteNonQuery()} records processed.");
					}
					end = start;
					start = watermark.Value = GetNextClusterKeyRangeRollingBackward(start);
					token.ThrowIfCancellationRequested();
				}
				while (start != end);

				watermark.Delete();
			}
		}

		int GetNextClusterKeyRangeRollingForward(int clusterKey)
		{
			var sql = @"
SELECT ISNULL(MAX(JE_ClusterKey), @ClusterKey) FROM
(
	SELECT TOP (@BatchSize) JE_ClusterKey
	FROM dbo.JobDeclaration WHERE JE_ClusterKey > @ClusterKey
	ORDER BY JE_ClusterKey ASC
) AS range
";
			return Db.Connection.ExecuteScalar<int>(sql, x =>
			{
				x.AddParameter("@BatchSize", SqlDbType.Int, BatchSize);
				x.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
			});
		}

		int GetNextClusterKeyRangeRollingBackward(int clusterKey)
		{
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
				x.AddParameter("@BatchSize", SqlDbType.Int, BatchSize);
				x.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
			});
		}

		void ShowInfoMessage(string message) => manager?.ShowInfoMessage(message);

		readonly int BatchSize;

		internal bool IsUSCustoms => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode IN ('US', 'PR')");

		internal class ClusterKeyWatermarkOnlinePostUpgrade
		{
			const string Watermark = "PopulateCRLActionAndENSAction_Watermark";

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

			ClusterKeyWatermarkOnlinePostUpgrade()
			{
			}

			public static ClusterKeyWatermarkOnlinePostUpgrade CreateNew()
			{
				var watermark = new ClusterKeyWatermarkOnlinePostUpgrade();

				try
				{
					watermark.Value = Db.Connection.ExecuteScalar<int>(@"
SELECT TOP 1
	JE_ClusterKey
FROM
	dbo.JobDeclaration 
WHERE
	JE_DataModel IN ('US', 'USR') 
	AND JE_MessageType IN ('IMP', 'REC', 'DRW') 
	AND JE_SystemCreateTimeUtc <= DATEADD(YEAR, -1, GETUTCDATE())
ORDER BY
	JE_ClusterKey DESC
");
				}
				catch (ExecuteScalarReturnedNullException)
				{
					watermark.Value = 0;
				}

				return watermark;
			}

			public static ClusterKeyWatermarkOnlinePostUpgrade Select()
			{
				var watermark = new ClusterKeyWatermarkOnlinePostUpgrade();
				return int.TryParse(ExtProperty.Database.Select(Db.Connection, Watermark), out watermark._value) ? watermark : null;
			}

			void Update() => ExtProperty.Database.Update(Db.Connection, Watermark, _value.ToString());

			public void Delete() => ExtProperty.Database.Delete(Db.Connection, Watermark);
		}

		internal class ClusterKeyWatermarkOnlinePreUpgrade
		{
			const string Watermark = "PopulateCRLActionAndENSAction_Watermark_OnlinePreUpgrade";

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

			ClusterKeyWatermarkOnlinePreUpgrade()
			{
			}

			public static ClusterKeyWatermarkOnlinePreUpgrade CreateNew(int seed)
			{
				return new ClusterKeyWatermarkOnlinePreUpgrade()
				{
					Value = seed
				};
			}

			public static ClusterKeyWatermarkOnlinePreUpgrade Select()
			{
				var watermark = new ClusterKeyWatermarkOnlinePreUpgrade();
				return int.TryParse(ExtProperty.Database.Select(Db.Connection, Watermark), out watermark._value) ? watermark : null;
			}

			void Update() => ExtProperty.Database.Update(Db.Connection, Watermark, _value.ToString());

			public void Delete() => ExtProperty.Database.Delete(Db.Connection, Watermark);
		}

		internal class DateTimeWatermarkOfflinePostUpgrade
		{
			const string Watermark = "PopulateCRLActionAndENSAction_Watermark_OnlinePreUpgrade_Date";

			public DateTime Value
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

			DateTime _value;

			DateTimeWatermarkOfflinePostUpgrade()
			{
			}

			public static DateTimeWatermarkOfflinePostUpgrade CreateNew()
			{
				var watermark = new DateTimeWatermarkOfflinePostUpgrade();
				watermark.Value = Db.Connection.ExecuteScalar<DateTime>("SELECT GETUTCDATE()");
				return watermark;
			}

			public static DateTimeWatermarkOfflinePostUpgrade Select()
			{
				var watermark = new DateTimeWatermarkOfflinePostUpgrade();
				return DateTime.TryParse(ExtProperty.Database.Select(Db.Connection, Watermark), out watermark._value) ? watermark : null;
			}

			void Update() => ExtProperty.Database.Update(Db.Connection, Watermark, _value.ToString());

			public void Delete() => ExtProperty.Database.Delete(Db.Connection, Watermark);
		}

		#region Queries

		string GetMainQuery(string searchCondition)
		{
			var sql = $@"
BEGIN TRY
	{PopulateENSActionForImportAndReconQuery}
	{PopulateENSActionForDrawbackQuery}
	{PopulateCRLActionForImportQuery}
END TRY
BEGIN CATCH
	THROW
END CATCH
";
			return string.Format(sql, searchCondition);
		}

		string PopulateENSActionForImportAndReconQuery => @"
UPDATE
	dbo.JobDeclaration
SET
	JE_AddInfo = JE_AddInfo + '*ENSAction=' + COALESCE(a.Action, 'Complete')
	, JE_SystemLastEditTimeUtc = GETUTCDATE()
	, JE_SystemLastEditUser = '~BP'
FROM
	dbo.JobDeclaration
	JOIN dbo.CusEntryHeader ON CH_ClusterKey = JE_ClusterKey AND CH_MessageType IN ('ENS', 'REC')
	OUTER APPLY
	(
		SELECT TOP 1
			'Incomplete' AS Action
		FROM
			dbo.EDIMessage
			LEFT JOIN dbo.StmALog ON SL_Parent = EM_PK AND SL_SE_NKEvent = 'ATH' AND SL_IsCancelled = 'N'
		WHERE
			EM_LinkUniqueID = CH_PK AND EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'UC' AND EM_ApplicationCode = 'USI' AND EM_ApplicationReference LIKE '%:%' AND LEN(EM_ApplicationReference) > 2 AND SL_Parent IS NULL
	) AS a
WHERE
	EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.EDIMessage
		WHERE
			EM_LinkUniqueID = CH_PK AND EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'UC' AND EM_ApplicationCode = 'USI'
	)
	AND JE_AddInfo NOT LIKE '%ENSAction=%'
	AND JE_DataModel IN ('US', 'USR')
	AND JE_MessageType IN ('IMP', 'REC')
	AND JE_ApplicationCode = 'ACE'
	AND {0}
";

		string PopulateENSActionForDrawbackQuery => @"
UPDATE
	dbo.JobDeclaration
SET
	JE_AddInfo = JE_AddInfo + '*ENSAction=' + COALESCE(a.Action, 'Complete')
	, JE_SystemLastEditTimeUtc = GETUTCDATE()
	, JE_SystemLastEditUser = '~BP'
FROM
	dbo.JobDeclaration
	OUTER APPLY
	(
		SELECT TOP 1
			'Incomplete' AS Action
		FROM
			dbo.EDIMessage
			LEFT JOIN dbo.StmALog ON SL_Parent = EM_PK AND SL_SE_NKEvent = 'ATH' AND SL_IsCancelled = 'N'
		WHERE
			EM_LinkUniqueID = JE_PK AND EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'UC' AND EM_ApplicationCode = 'USI' AND EM_ApplicationReference LIKE '%:%' AND LEN(EM_ApplicationReference) > 2 AND SL_Parent IS NULL
	) AS a
WHERE
	EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.EDIMessage
		WHERE
			EM_LinkUniqueID = JE_PK AND EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'UC' AND EM_ApplicationCode = 'USI'
	)
	AND JE_AddInfo NOT LIKE '%ENSAction=%'
	AND JE_DataModel = 'US'
	AND JE_MessageType = 'DRW'
	AND JE_ApplicationCode = 'ACE'
	AND {0}
";

		string PopulateCRLActionForImportQuery => @"
UPDATE
	dbo.JobDeclaration
SET
	JE_AddInfo = JE_AddInfo + '*CRLAction=' + COALESCE(a.Action, 'Complete')
	, JE_SystemLastEditTimeUtc = GETUTCDATE()
	, JE_SystemLastEditUser = '~BP'
FROM
	dbo.JobDeclaration
	JOIN dbo.CusEntryHeader ON CH_ClusterKey = JE_ClusterKey AND CH_MessageType = 'SE'
	OUTER APPLY
	(
		SELECT TOP 1
			'Incomplete' AS Action
		FROM
			dbo.EDIMessage
			LEFT JOIN dbo.StmALog ON SL_Parent = EM_PK AND SL_SE_NKEvent = 'ATH' AND SL_IsCancelled = 'N'
		WHERE
			EM_LinkUniqueID = CH_PK AND EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'SO' AND EM_ApplicationCode = 'USI' AND EM_ApplicationReference = 'CMT' AND SL_Parent IS NULL
	) AS a
WHERE
	EXISTS
	(
		SELECT
			NULL
		FROM
			dbo.EDIMessage
		WHERE
			EM_LinkUniqueID = CH_PK AND EM_ReceiveTransmit = 'RCV' AND EM_MessageType = 'SO' AND EM_ApplicationCode = 'USI' AND EM_ApplicationReference = 'CMT'
	)
	AND JE_AddInfo NOT LIKE '%CRLAction=%'
	AND JE_DataModel = 'US'
	AND JE_MessageType = 'IMP'
	AND JE_ApplicationCode = 'ACE'
	AND {0}
";

		string SearchConditionByClusterKey => "JE_ClusterKey BETWEEN @Start AND @End";

		string SearchConditionBySystemLastEditTimeUtc => "JE_SystemLastEditTimeUtc >= @Date";

		#endregion

		#region Index

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexInfo1 = IndexInfo.Builder
					.New(Db.SqlDbOwnerSchema, JobDeclarationSchema.Constants.TableName, $"_WTG_{UserDescription}_1")
					.Key(JobDeclarationSchema.Constants.JE_DataModel, JobDeclarationSchema.Constants.JE_MessageType, JobDeclarationSchema.Constants.JE_ApplicationCode, JobDeclarationSchema.Constants.JE_SystemLastEditTimeUtc)
					.Where($"([JE_DataModel] IN ('US', 'USR')) AND ([JE_MessageType] IN ('IMP', 'REC', 'DRW')) AND [JE_ApplicationCode]='ACE' AND [JE_SystemLastEditTimeUtc]>='{DateTime.UtcNow.Date.AddDays(-5).ToString("yyyy-MM-dd")}'")
					.Include(JobDeclarationSchema.Constants.JE_ClusterKey, JobDeclarationSchema.Constants.JE_AddInfo, JobDeclarationSchema.Constants.JE_SystemLastEditUser)
					.GetInfo();

				var indexInfo2 = IndexInfo.Builder
					.New(Db.SqlDbOwnerSchema, CusEntryHeaderSchema.Constants.TableName, $"_WTG_{UserDescription}_2")
					.Key(CusEntryHeaderSchema.Constants.CH_MessageType)
					.Where("[CH_MessageType] IN ('ENS', 'REC', 'SE')")
					.Include(CusEntryHeaderSchema.Constants.CH_ClusterKey, CusEntryHeaderSchema.Constants.PK)
					.GetInfo();

				return new TransformationIndexProvider(this) { indexInfo1.Yield(), indexInfo2.Yield() };
			}
		}

		#endregion
	}
}
