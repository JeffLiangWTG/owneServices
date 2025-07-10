using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public static class StatisticsTestHelper
	{
		#region SetUserData

		public static void SetLegacyUserData(DateTime dateFrom, DateTime dateTo, string exeVersion, string companyCode, string branchCode, string staffCode, string actionName, string actionSubName, int actionCount, decimal actionElapsed)
		{
			SetLegacyUserData(Guid.NewGuid(), Guid.NewGuid(), true, dateFrom, dateTo, exeVersion, companyCode, branchCode, staffCode, actionName, actionSubName, actionCount, actionElapsed);
		}

		public static void SetLegacyUserData(Guid usagePK, Guid actionPK, bool createQueue, DateTime dateFrom, DateTime dateTo, string exeVersion, string companyCode, string branchCode, string staffCode, string actionName, string actionSubName, int actionCount, decimal actionElapsed)
		{
			InsertLegacyUsage(usagePK, companyCode, branchCode, staffCode, exeVersion, dateFrom, dateTo);
			InsertLegacyAction(usagePK, actionPK, actionName, actionSubName, dateFrom, dateTo, actionCount, actionElapsed);
			if (!createQueue)
			{
				DeleteQueue(usagePK);
			}
		}

		public static StmUsage CreateUsage(BusinessObjectFactory factory, DateTime dateUTC, string exeVersion, string companyCode, string branchCode, string staffCode, string actionName, string actionSubName, int actionCount, decimal actionElapsed)
		{
			return CreateUsage(factory, dateUTC, dateUTC, exeVersion, companyCode, branchCode, staffCode, actionName, actionSubName, actionCount, actionElapsed);
		}

		public static StmUsage CreateUsage(BusinessObjectFactory factory, DateTime dateFrom, DateTime dateTo, string exeVersion, string companyCode, string branchCode, string staffCode, string actionName, string actionSubName, int actionCount, decimal actionElapsed)
		{
			var usage = factory.New<StmUsage>();
			usage.XW_MachineName = "TST";
			usage.XW_CompanyCode = companyCode;
			usage.XW_GB_NKBranchCode = branchCode;
			usage.XW_GS_NKStaffCode = staffCode;
			usage.XW_ExeVersion = exeVersion;
			usage.XW_StartTimeUtc = dateFrom;
			usage.XW_EndTimeUtc = dateTo;

			var elapsed = TimeSpan.FromSeconds((double)actionElapsed);
			var token = new Mock<IPerformanceStatisticCollectorToken>();
			token.Setup(t => t.Name).Returns(actionName);
			token.Setup(t => t.SubName).Returns(actionSubName);
			token.Setup(t => t.TimeStartedUtc).Returns(dateFrom);
			token.Setup(t => t.TimeEndedUtc).Returns(dateTo);
			token.Setup(t => t.ElapsedIncludingChildren).Returns(elapsed);
			token.Setup(t => t.ElapsedExcludingChildren).Returns(elapsed);
			token.Setup(t => t.ActionCount).Returns(actionCount);
			var usages = new Statistics.Xml.Usages();
			usages.AddNode(Statistics.Xml.Usage.New(token.Object));
			usage.UsageActionsSettings = usages;

			return usage;
		}

		public static IEnumerable<StmUsage> GetUsagesWithMatchingActions(BusinessObjectFactory factory, string actionName) => GetUsageActions(factory, usage => usage.Name.Equals(actionName)).Select(t => t.Item2);
		public static IEnumerable<Statistics.Xml.IUsage> GetUsageActions(BusinessObjectFactory factory, string actionName) => GetUsageActions(factory, usage => usage.Name.Equals(actionName)).Select(t => t.Item1);
		public static IEnumerable<Statistics.Xml.IUsage> GetUsageActionsBySubString(BusinessObjectFactory factory, string partialActionName) => GetUsageActions(factory, usage => usage.Name.Contains(partialActionName)).Select(t => t.Item1);

		public static IEnumerable<Tuple<Statistics.Xml.IUsage, StmUsage>> GetUsageActions(BusinessObjectFactory factory, Func<Statistics.Xml.IUsage, bool> matchFunc)
		{
			var allUsages = factory.Load<StmUsage>(new ZQuery());
			var usageActions = new List<Tuple<Statistics.Xml.IUsage, StmUsage>>();
			foreach (var usage in allUsages)
			{
				usageActions.AddRange(usage.UsageActionsSettings.Where(stmAction => matchFunc(stmAction)).Select(stmAction => new Tuple<Statistics.Xml.IUsage, StmUsage>(stmAction, usage)));
			}
			return usageActions;
		}

		public static void InsertLegacyUsage(Guid usagePK, string companyCode, string branchCode, string staffCode, string exeVersion, DateTime dateFrom, DateTime dateTo)
		{
			var sql = @"
				INSERT dbo.StmUsage (XW_PK, XW_MachineName, XW_CompanyCode, XW_GB_NKBranchCode, XW_GS_NKStaffCode, XW_ExeVersion, XW_StartTimeUtc, XW_EndTimeUtc)
				SELECT @XW_PK, @XW_MachineName, @XW_CompanyCode, @XW_GB_NKBranchCode, @XW_GS_NKStaffCode, @XW_ExeVersion, @XW_StartTimeUtc, @XW_EndTimeUtc;
				";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XW_PK", SqlDbType.UniqueIdentifier, usagePK);
				cmd.AddParameter("@XW_MachineName", SqlDbType.VarChar, "TST");
				cmd.AddParameter("@XW_CompanyCode", SqlDbType.VarChar, companyCode);
				cmd.AddParameter("@XW_GB_NKBranchCode", SqlDbType.VarChar, branchCode);
				cmd.AddParameter("@XW_GS_NKStaffCode", SqlDbType.VarChar, staffCode);
				cmd.AddParameter("@XW_ExeVersion", SqlDbType.VarChar, exeVersion);
				cmd.AddParameter("@XW_StartTimeUtc", SqlDbType.DateTime, dateFrom);
				cmd.AddParameter("@XW_EndTimeUtc", SqlDbType.DateTime, dateTo);

				cmd.ExecuteNonQuery();
			}
		}

		public static void InsertLegacyAction(Guid usagePK, Guid actionPK, string name, string subName, DateTime dateFrom, DateTime dateTo, int actionCount, decimal elapsed)
		{
			var sql = @"
				INSERT dbo.StmUsageAction (XO_PK, XO_Name, XO_SubName, XO_StartTimeUtc, XO_EndTimeUtc, XO_ElapsedWithoutChildrenSeconds, XO_ElapsedWithChildrenSeconds, XO_ActionCount, XO_XW_Usage, XO_XO_Parent)
				SELECT @XO_PK, @XO_Name, @XO_SubName, @XO_StartTimeUtc, @XO_EndTimeUtc, @XO_ElapsedWithoutChildrenSeconds, @XO_ElapsedWithChildrenSeconds, @XO_ActionCount, @XO_XW_Usage, @XO_XO_Parent;
				";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XO_PK", SqlDbType.UniqueIdentifier, actionPK);
				cmd.AddParameter("@XO_Name", SqlDbType.VarChar, name);
				cmd.AddParameter("@XO_SubName", SqlDbType.VarChar, subName);
				cmd.AddParameter("@XO_StartTimeUtc", SqlDbType.DateTime, dateFrom);
				cmd.AddParameter("@XO_EndTimeUtc", SqlDbType.DateTime, dateTo);
				cmd.AddParameter("@XO_ElapsedWithoutChildrenSeconds", SqlDbType.Decimal, elapsed);
				cmd.AddParameter("@XO_ElapsedWithChildrenSeconds", SqlDbType.Decimal, elapsed);
				cmd.AddParameter("@XO_ActionCount", SqlDbType.Int, actionCount);
				cmd.AddParameter("@XO_XW_Usage", SqlDbType.UniqueIdentifier, usagePK);
				cmd.AddParameter("@XO_XO_Parent", SqlDbType.UniqueIdentifier, DBNull.Value);

				cmd.ExecuteNonQuery();
			}
		}

		public static void DeleteQueue(Guid usagePK)
		{
			var sql = @"DELETE FROM dbo.StmUsageQueue WHERE XI_XW = @XI_XW;";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XI_XW", SqlDbType.UniqueIdentifier, usagePK);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion //SetUserData

		#region StmUsageDateRange

		public static int InsertDateRange(DateTime dateFrom, DateTime dateTo, int aggregationPeriod, bool hasSubNameData)
		{
			int id = 0;
			var sql = @"
				INSERT dbo.StmUsageDateRange (XJ_DateFrom, XJ_DateTo, XJ_AggregationPeriod, XJ_HasSubNameData)
					SELECT @XJ_DateFrom, @XJ_DateTo, @XJ_AggregationPeriod, @XJ_HasSubNameData;
				SELECT CONVERT(int, SCOPE_IDENTITY());
				";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XJ_DateFrom", SqlDbType.SmallDateTime, dateFrom);
				cmd.AddParameter("@XJ_DateTo", SqlDbType.SmallDateTime, dateTo);
				cmd.AddParameter("@XJ_AggregationPeriod", SqlDbType.Int, aggregationPeriod);
				cmd.AddParameter("@XJ_HasSubNameData", SqlDbType.Bit, hasSubNameData);

				id = (int)cmd.ExecuteScalar();
			}

			return id;
		}

		public static IEnumerable<string> SelectDateRange()
		{
			var list = new List<string>();
			var sql = "SELECT XJ_DateFrom, XJ_DateTo, XJ_AggregationPeriod, XJ_HasSubNameData FROM dbo.StmUsageDateRange;";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					list.Add(FormatDateRange(
						(DateTime)reader["XJ_DateFrom"],
						(DateTime)reader["XJ_DateTo"],
						(int)reader["XJ_AggregationPeriod"],
						(bool)reader["XJ_HasSubNameData"]));
				}
			}

			return list;
		}

		public static string FormatDateRange(DateTime dateFrom, DateTime dateTo, int period, bool hasSubName)
		{
			return string.Format(
				"DateFrom: {0:yyyy-MM-dd HH:mm:ss.fff}, DateTo: {1:yyyy-MM-dd HH:mm:ss.fff}, AggregationPeriod: {2}, HasSubNameData: {3}",
				dateFrom, dateTo, period, hasSubName);
		}

		#endregion //StmUsageDateRange

		#region StmUsageVersion

		public static int InsertVersion(string version)
		{
			int id = 0;
			var sql = @"
				INSERT dbo.StmUsageVersion (XN_Version)
					SELECT @XN_Version;
				SELECT CONVERT(int, SCOPE_IDENTITY());
				";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XN_Version", SqlDbType.VarChar, version);

				id = (int)cmd.ExecuteScalar();
			}

			return id;
		}

		public static IEnumerable<string> SelectVersion()
		{
			var list = new List<string>();
			var sql = "SELECT XN_Version FROM dbo.StmUsageVersion;";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					list.Add(FormatVersion((string)reader["XN_Version"]));
				}
			}

			return list;
		}

		public static string FormatVersion(string version)
		{
			return string.Format("ExeVersion: '{0}'", version);
		}

		#endregion //StmUsageVersion

		#region StmUsageActionKey

		public static int InsertActionKey(string name, string subName)
		{
			int id = 0;
			var sql = @"
				INSERT dbo.StmUsageActionKey (XM_Name, XM_SubName)
					SELECT @XM_Name, @XM_SubName;
				SELECT CONVERT(int, SCOPE_IDENTITY());
				";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XM_Name", SqlDbType.VarChar, 450, name);
				cmd.AddParameter("@XM_SubName", SqlDbType.VarChar, 450, subName);

				id = (int)cmd.ExecuteScalar();
			}

			return id;
		}

		public static IEnumerable<string> SelectActionKey()
		{
			var list = new List<string>();
			var sql = "SELECT XM_Name, XM_SubName FROM dbo.StmUsageActionKey;";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					list.Add(FormatActionKey((string)reader["XM_Name"], (string)reader["XM_SubName"]));
				}
			}

			return list;
		}

		public static string FormatActionKey(string name, string subname)
		{
			return string.Format("Name: '{0}', SubName: '{1}'", name, subname);
		}

		#endregion //StmUsageActionKey

		#region StmUsageSummary

		public static void InsertSummary(int dateRange, int version, string companyCode, string branchCode, string staffCode, int actionKey, long actionCount, decimal seconds, decimal secondsSquare, decimal secondsCube)
		{
			var sql = @"
				INSERT dbo.StmUsageSummary (XY_CompanyCode, XY_GB_NKBranchCode, XY_GS_NKStaffCode, XY_XM_ActionKey, XY_XJ_DateRange, XY_XN_Version, XY_ActionCount, XY_SumOfSeconds, XY_SumOfSquaresSeconds, XY_SumOfCubesSeconds, XY_SumOfSecondsIncludingChildren, XY_SumOfSquaresSecondsIncludingChildren, XY_SumOfCubesSecondsIncludingChildren)
				SELECT @XY_CompanyCode, @XY_GB_NKBranchCode, @XY_GS_NKStaffCode, @XY_XM_ActionKey, @XY_XJ_DateRange, @XY_XN_Version, @XY_ActionCount, @XY_SumOfSeconds, @XY_SumOfSquaresSeconds, @XY_SumOfCubesSeconds, @XY_SumOfSecondsIncludingChildren, @XY_SumOfSquaresSecondsIncludingChildren, @XY_SumOfCubesSecondsIncludingChildren;
				";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@XY_CompanyCode", SqlDbType.VarChar, companyCode);
				cmd.AddParameter("@XY_GB_NKBranchCode", SqlDbType.VarChar, branchCode);
				cmd.AddParameter("@XY_GS_NKStaffCode", SqlDbType.VarChar, staffCode);
				cmd.AddParameter("@XY_XM_ActionKey", SqlDbType.Int, actionKey);
				cmd.AddParameter("@XY_XJ_DateRange", SqlDbType.Int, dateRange);
				cmd.AddParameter("@XY_XN_Version", SqlDbType.Int, version);
				cmd.AddParameter("@XY_ActionCount", SqlDbType.BigInt, actionCount);
				cmd.AddParameter("@XY_SumOfSeconds", SqlDbType.Decimal, seconds);
				cmd.AddParameter("@XY_SumOfSquaresSeconds", SqlDbType.Decimal, secondsSquare);
				cmd.AddParameter("@XY_SumOfCubesSeconds", SqlDbType.Decimal, secondsCube);
				cmd.AddParameter("@XY_SumOfSecondsIncludingChildren", SqlDbType.Decimal, seconds);
				cmd.AddParameter("@XY_SumOfSquaresSecondsIncludingChildren", SqlDbType.Decimal, secondsSquare);
				cmd.AddParameter("@XY_SumOfCubesSecondsIncludingChildren", SqlDbType.Decimal, secondsCube);

				cmd.ExecuteNonQuery();
			}
		}

		public static IEnumerable<string> SelectSummary()
		{
			var list = new List<string>();
			var sql = @"
				SELECT
					XJ_DateFrom, XJ_DateTo, XJ_AggregationPeriod, XJ_HasSubNameData = CONVERT(int, XJ_HasSubNameData),
					XN_Version,
					XY_CompanyCode = rtrim(XY_CompanyCode),
                    XY_GB_NKBranchCode = rtrim(XY_GB_NKBranchCode),
                    XY_GS_NKStaffCode = rtrim(XY_GS_NKStaffCode),
					XM_Name, XM_SubName,
					XY_ActionCount,
					XY_SumOfSeconds, XY_SumOfSquaresSeconds, XY_SumOfCubesSeconds,
					XY_SumOfSecondsIncludingChildren, XY_SumOfSquaresSecondsIncludingChildren, XY_SumOfCubesSecondsIncludingChildren
				FROM
					dbo.StmUsageSummary
					JOIN dbo.StmUsageDateRange ON XJ_Id = XY_XJ_DateRange
					JOIN dbo.StmUsageVersion   ON XN_Id = XY_XN_Version
					JOIN dbo.StmUsageActionKey ON XM_Id = XY_XM_ActionKey;
				";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					list.Add(FormatSummary(
						(DateTime)reader["XJ_DateFrom"],
						(DateTime)reader["XJ_DateTo"],
						(int)reader["XJ_AggregationPeriod"],
						(int)reader["XJ_HasSubNameData"],
						(string)reader["XN_Version"],
						(string)reader["XY_CompanyCode"],
						(string)reader["XY_GB_NKBranchCode"],
						(string)reader["XY_GS_NKStaffCode"],
						(string)reader["XM_Name"],
						(string)reader["XM_SubName"],
						(long)reader["XY_ActionCount"],
						(decimal)reader["XY_SumOfSeconds"],
						(decimal)reader["XY_SumOfSquaresSeconds"],
						(decimal)reader["XY_SumOfCubesSeconds"],
						(decimal)reader["XY_SumOfSecondsIncludingChildren"],
						(decimal)reader["XY_SumOfSquaresSecondsIncludingChildren"],
						(decimal)reader["XY_SumOfCubesSecondsIncludingChildren"]
						));
				}
			}

			return list;
		}

		public static string FormatSummary(DateTime dateFrom, DateTime dateTo, int period, int hasSubNameData, string version, string companyCode, string branchCode, string staffCode, string name, string subName, long actionCount, decimal seconds, decimal secondsSquare, decimal secondsCube, decimal childrenSeconds, decimal childrenSecondsSquare, decimal childrenSecondsCube)
		{
			return string.Format(
				"DateFrom: {0:yyyy-MM-dd HH:mm}, DateTo: {1:yyyy-MM-dd HH:mm}, AggregationPeriod: {2}, HasSubNameData: {3}, Version: '{4}', CompanyCode: '{5}', BranchCode: '{6}', StaffCode: '{7}', Name: '{8}', SubName: '{9}', ActionCount: {10}, Seconds: {11}, SecondsSquare: {12}, SecondsCube: {13}, ChildrenSeconds: {14}, ChildrenSecondsSquare: {15}, ChildrenSecondsCube: {16}",
				 dateFrom, dateTo, period, hasSubNameData, version, companyCode, branchCode, staffCode, name, subName, actionCount, seconds, secondsSquare, secondsCube, childrenSeconds, childrenSecondsSquare, childrenSecondsCube);
		}

		#endregion //StmUsageSummary

		#region TableCounts

		public static void AssertTableCounts(string message, int expectedUsageCount, int expectedActionCount, int expectedQueueCount, int expectedDateRangeCount, int expectedVersionCount, int expectedActionKeyCount, int expectedSummaryCount)
		{
			var expectedTableCounts = StatisticsTestHelper.FormatTableCounts(expectedUsageCount, expectedActionCount, expectedQueueCount, expectedDateRangeCount, expectedVersionCount, expectedActionKeyCount, expectedSummaryCount);
			Assertion.AssertEquals(message, expectedTableCounts, StatisticsTestHelper.SelectTableCounts());
		}

		public static string SelectTableCounts()
		{
			var tableCounts = string.Empty;
			var sql = @"
						SELECT
							UsageCount     = (SELECT COUNT(*) FROM dbo.StmUsage         ),
							ActionCount    = (SELECT COUNT(*) FROM dbo.StmUsageAction   ),
							QueueCount     = (SELECT COUNT(*) FROM dbo.StmUsageQueue    ),
							DateRangeCount = (SELECT COUNT(*) FROM dbo.StmUsageDateRange),
							VersionCount   = (SELECT COUNT(*) FROM dbo.StmUsageVersion  ),
							ActionKeyCount = (SELECT COUNT(*) FROM dbo.StmUsageActionKey),
							SummaryCount   = (SELECT COUNT(*) FROM dbo.StmUsageSummary  )
						";

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				tableCounts = FormatTableCounts(
					(int)reader["usageCount"],
					(int)reader["actionCount"],
					(int)reader["queueCount"],
					(int)reader["dateRangeCount"],
					(int)reader["versionCount"],
					(int)reader["actionKeyCount"],
					(int)reader["summaryCount"]);
			}

			return tableCounts;
		}

		public static string FormatTableCounts(int usageCount, int actionCount, int queueCount, int dateRangeCount, int versionCount, int actionKeyCount, int summaryCount)
		{
			return string.Format(
				"UsageCount: {0}, ActionCount: {1}, QueueCount: {2}, VersionCount: {3}, DateRangeCount: {4}, ActionKeyCount: {5}, SummaryCount: {6}",
				usageCount, actionCount, queueCount, dateRangeCount, versionCount, actionKeyCount, summaryCount);
		}

		#endregion //TableCounts

		#region Stored Procedures

		public static int ExecLegacyStatisticsServiceSummarise(int topRowCount)
		{
			int result = -1;
			var sql = @"LegacyStatisticsServiceSummarise";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@TopRowCount", SqlDbType.BigInt, topRowCount);

				result = (int)cmd.ExecuteScalar();
			}

			return result;
		}

		public static int ExecStatisticsServiceSummarise(int topRowCount) => ExecStatisticsServiceSummarise(topRowCount, Db.Connection);
		public static int ExecStatisticsServiceSummarise(DbConnection connection) => ExecStatisticsServiceSummarise(100, connection);

		public static int ExecStatisticsServiceSummarise(int topRowCount, DbConnection connection)
		{
			int result = -1;
			var sql = @"StatisticsServiceSummarise";

			using (var cmd = connection.Command(sql))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@TopRowCount", SqlDbType.BigInt, topRowCount);

				result = (int)cmd.ExecuteScalar();
			}

			return result;
		}

		public static void ExecStatisticsServiceAggregate(DateTime date_start, string dbCommandName)
		{
			using (var cmd = Db.Connection.Command(dbCommandName))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@date_start", SqlDbType.SmallDateTime, date_start);

				cmd.ExecuteNonQuery();
			}
		}

		public static void ExecStatisticsServiceAggregateSubName(DateTime date_start)
		{
			ExecStatisticsServiceAggregate(date_start, "StatisticsServiceAggregateSubName");
		}

		public static void ExecStatisticsServiceAggregateBranch(DateTime date_start)
		{
			ExecStatisticsServiceAggregate(date_start, "StatisticsServiceAggregateBranch");
		}

		public static void ExecStatisticsServiceAggregateStaff(DateTime date_start)
		{
			ExecStatisticsServiceAggregate(date_start, "StatisticsServiceAggregateStaff");
		}

		public static void ExecStatisticsServiceAggregatePeriod(DateTime date_start, DateTime now)
		{
			using (var cmd = Db.Connection.Command("StatisticsServiceAggregatePeriod"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@date_start", SqlDbType.SmallDateTime, date_start);
				cmd.AddParameter("@now", SqlDbType.SmallDateTime, now);

				cmd.ExecuteNonQuery();
			}
		}

		#endregion //Stored Procedures
	}
}
