using System;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.US
{
	public class SplitInBondQPAndWPStatus : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Split InBond QP And WP Status";
		readonly int batchDays;

		public SplitInBondQPAndWPStatus()
			: this(180)
		{
		}

		internal SplitInBondQPAndWPStatus(int batchDays)
		{
			this.batchDays = batchDays;
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(CusInBondMoveHeaderSchema.Instance)
					.Key(CusInBondMoveHeaderSchema.Constants.BM_SystemCreateTimeUtc)
					.Include(
								CusInBondMoveHeaderSchema.Constants.BM_CustomsStatus,
								CusInBondMoveHeaderSchema.Constants.BM_MessageStatus,
								"BM_AutoVersion",
								CusInBondMoveHeaderSchema.Constants.BM_SystemLastEditTimeUtc,
								CusInBondMoveHeaderSchema.Constants.BM_SystemLastEditUser,
								CusInBondMoveHeaderSchema.Constants.BM_BH)
					.Where($"[{CusInBondMoveHeaderSchema.Constants.BM_CustomsStatus}] IN ('AAV', 'AEX', 'ATL', 'CAV', 'CEX', 'CTL', 'EAV', 'EEX', 'ETL')")
					.GetInfo();
				return indexProvider;
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			if (IsUSCustoms)
			{
				var startTime = QueryStarTimeFlag(true);
				RunTransformationCore(startTime, new DateTime(2079, 12, 31), true);
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (IsUSCustoms)
			{
				var startTime = QueryStarTimeFlag(false);
				if (startTime.HasValue)
				{
					var minStartTime = QueryMinimumCreateTimeOfMoveHeader();

					do
					{
						if (!minStartTime.HasValue || startTime <= minStartTime)
						{
							startTime = null;
						}
						var endTime = startTime;
						if (endTime.HasValue)
						{
							startTime = endTime.Value.AddDays(-batchDays).Date;
							if (startTime < minStartTime)
							{
								startTime = minStartTime;
							}
						}

						RunTransformationCore(startTime, endTime, false);
						token.ThrowIfCancellationRequested();
					}
					while (startTime.HasValue && minStartTime.HasValue && startTime >= minStartTime);

					ExtProperty.Database.Delete(Db.Connection, LastStartDateWaterMark);
				}
			}
		}

		void RunTransformationCore(DateTime? startTime, DateTime? endTime, bool suspendVersionTrigger)
		{
			ShowInfoMessage($"Processing batch time range {GenerateTimeRangeLog(startTime, endTime)}...");

			var updatedLinesNum = 0;
			using (suspendVersionTrigger ? DataTransformationHelper.SuspendTriggerIfExists("TG_CusInBondMoveHeader_UpdateAutoVersion", CusInBondMoveHeaderSchema.Constants.TableName) : DisposableAction.NoAction)
			{
				updatedLinesNum = Db.Connection.ExecuteScalar<int>(
					GenerateUpdateScript(startTime.HasValue, suspendVersionTrigger),
					cmd =>
					{
						cmd.AddParameter("@startTime", SqlDbType.DateTime, startTime.HasValue ? startTime : DBNull.Value);
						cmd.AddParameter("@endTime", SqlDbType.DateTime, endTime.HasValue ? endTime : DBNull.Value);
					});
			}

			ShowInfoMessage($"[{updatedLinesNum}] records updated.");

			if (startTime.HasValue)
			{
				SaveStarTimeFlag(startTime.Value);
			}
		}

		#region Time Flag

		internal const string LastStartDateWaterMark = "PopulateInBondQPAndWPStatusFromMessageStatus_LastStartDate";

		const string DateFormat = "yyyy-MM-dd";

		DateTime? QueryStarTimeFlag(bool createIfNotExist)
		{
			DateTime? result = null;
			if (DateTime.TryParse(ExtProperty.Database.Select(Db.Connection, LastStartDateWaterMark), out var tempTime))
			{
				result = tempTime;
			}
			else if (createIfNotExist)
			{
#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
				result = DateTime.UtcNow.AddYears(-1).Date;
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
			}
			return result;
		}

		void SaveStarTimeFlag(DateTime startTime)
		{
			ExtProperty.Database.Update(Db.Connection, LastStartDateWaterMark, startTime.ToString(DateFormat));
		}

		DateTime? QueryMinimumCreateTimeOfMoveHeader()
		{
			DateTime? result = null;
			using (var cmd = Db.Connection.Command("SELECT MIN(BM_SystemCreateTimeUtc) FROM dbo.CusInBondMoveHeader"))
			{
				var dbResult = cmd.ExecuteScalar();
				if (dbResult != DBNull.Value)
				{
					result = ((DateTime)dbResult).Date;
				}
			}
			return result;
		}

		#endregion

		#region Scripts

		bool IsUSCustoms => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode IN ('US', 'PR')");

		string GenerateUpdateScript(bool isQueryTimeRange, bool shouldUpdateVersion)
		{
			var timeCondition = isQueryTimeRange ? "BM_SystemCreateTimeUtc BETWEEN @startTime AND @endTime" : "BM_SystemCreateTimeUtc IS NULL";
			var setVersion = shouldUpdateVersion ? "BM_AutoVersion = (BM_AutoVersion + 1) % 32768," : string.Empty;
			return $@"
UPDATE dbo.CusInBondMoveHeader
SET
	BM_CustomsStatus = '',
	BM_MessageStatus = BM_CustomsStatus,
	{setVersion}
	BM_SystemLastEditTimeUtc = GETUTCDATE(),
	BM_SystemLastEditUser = '~BP'
FROM dbo.CusInBondMoveHeader
JOIN dbo.CusInBondHeader ON BH_PK = BM_BH AND BH_ApplicationCode = 'INB'
WHERE
	{timeCondition}
	AND BM_CustomsStatus IN ('AAV', 'AEX', 'ATL', 'CAV', 'CEX', 'CTL', 'EAV', 'EEX', 'ETL')

SELECT @@ROWCOUNT";
		}

		#endregion

		public static string GenerateTimeRangeLog(DateTime? startTime, DateTime? endTime) => $"[{startTime?.ToString(DateFormat) ?? string.Empty}]-[{endTime?.ToString(DateFormat) ?? string.Empty}]";

		void ShowInfoMessage(string message) => manager?.ShowInfoMessage(message);
	}
}
