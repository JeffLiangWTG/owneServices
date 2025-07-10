#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DeprecatedFeatureInPerformanceCounterListener : BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly DeprecatedFeatureInPerformanceCounterListener Instance = new DeprecatedFeatureInPerformanceCounterListener();

		DeprecatedFeatureInPerformanceCounterListener()
		{
		}

		#region SQL

		/**
		 * query PerformanceCounter with white list:
		 *    Feature12(Returning results from trigger):
		 *        Resolve from server level. So it is not counted.
		 *    Feature126(syscomments), Feature148(numbered_procedures), Feature184(String literals as column aliases):
		 *        According to statistics, most of the problems are caused by `EXEC sp_helpconstraint` and `EXEC sp_helpindex`.
		 *        This requires Microsoft to fix it first. Then remove these 3 feature from the white list.
		 *    Feature(Data types: text ntext or image):
		 *        Require regen. Address separately during baseline processing.
		 *    Feature('Data types: text ntext or image', 'Multiple table hints without comma', 'XP_API', 'sysobjects', 'More than two-part column name', 'sysdatabases', 'sysindexes', 'sysfiles')
		 *        Random Issues. Address separately during baseline processing.
		 *    Feature('sysoledbusers', 'sysremotelogins', 'sysservers')
		 *        Caused by built-in store procedure sp_dropserver(which is not annonced deprecated by Microsoft)
		 *        This requires Microsoft to fix it first. Then remove these 3 feature from the white list.
		 *        WI00702362
		 *    Feature('sql_dependencies')
		 *        New DMV/SP does not cover all cases. Will be here until we find a way or WI00702268 (Baselining) completed.
		 */
		const string queryPerformanceCounterSql = @"
select string_agg(trim(instance_name) + ':' + cast(cntr_value as varchar), char(13)) within group (order by instance_name) as deprecated_feature_statistic
FROM sys.dm_os_performance_counters
WHERE object_name = iif(@@SERVICENAME = 'MSSQLSERVER', 'SQLServer:Deprecated Features', 'MSSQL$' + @@SERVICENAME + ':Deprecated Features')
  and cntr_value > 0
  and instance_name not in ('Returning results from trigger', 'syscomments', 'numbered_procedures', 'String literals as column aliases', 'Data types: text ntext or image', 'Multiple table hints without comma', 'XP_API', 'sysobjects', 'More than two-part column name', 'sysdatabases', 'sysindexes', 'sysfiles', 'sql_dependencies', 'sp_addrolemember', 'sysoledbusers', 'sysremotelogins', 'sysservers')
";

		#endregion

		#region Execute

		string queryPerformanceCounter()
		{
			var deprecatedFeatureStatistic = "";
			AdminConnection.ExecuteReader(queryPerformanceCounterSql, reader =>
			{
				deprecatedFeatureStatistic = reader["deprecated_feature_statistic"].ToString();
			});

			return deprecatedFeatureStatistic;
		}

		(string, string) messageExceptIntersect(string statisticStart, string statisticEnd)
		{
			var separator = (char)13;
			string[] firstLines = statisticStart.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
			string[] secondLines = statisticEnd.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);

			string[] duplicateLines = firstLines.Intersect(secondLines).ToArray();
			return (string.Join(separator.ToString(), firstLines.Except(duplicateLines)), string.Join(separator.ToString(), secondLines.Except(duplicateLines)));
		}

		#endregion

		#region override

		public override void StartAllTests(DateTime startTime)
		{
			base.StartAllTests(startTime);
			deprecatedFeatureStatisticEnd = queryPerformanceCounter();
		}

		public override void StartTest(TestCase test, DateTime startTime)
		{
			base.StartTest(test, startTime);

			commandExecuteCount = DbCommandExecuteTracker.Instance.ExecuteCount;
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);

			// minimize DB Hits: end ut if there is no db interaction
			if (DbCommandExecuteTracker.Instance.ExecuteCount == commandExecuteCount)
			{
				return;
			}

			// Arrange
			deprecatedFeatureStatisticStart = deprecatedFeatureStatisticEnd;
			deprecatedFeatureStatisticEnd = queryPerformanceCounter();
			if (test == null || Baseline.Contains(test.ToString()))
			{
				return;
			}

			// Act
			if (deprecatedFeatureStatisticStart != deprecatedFeatureStatisticEnd)
			{
				// Assert
				var (statisticStart, statisticEnd) = messageExceptIntersect(deprecatedFeatureStatisticStart, deprecatedFeatureStatisticEnd);
				var message = $@"
Depreciated feature(s) used in {test}:
Deprecated Feature Statistic (Start): 
{statisticStart}
Deprecated Feature Statistic (End): 
{statisticEnd}

If you want to see detailed information, you can refer to this PR (run it locally): https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/166254
";
				Assertion.Fail(message);
			}
		}

		public override void EndAllTests(DateTime endTime)
		{
			base.EndAllTests(endTime);
			AdminConnection.Dispose();
		}

		#endregion

		#region Baseline

		HashSet<string> GetDBDeprecatedFeatureBaseline()
		{
			var baseline = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DBDeprecatedFeatureBaseline.txt"))
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					baseline.Add(line);
				}
			}

			return baseline;
		}

		HashSet<string> Baseline => baseline ?? (baseline = GetDBDeprecatedFeatureBaseline());
		HashSet<string> baseline;

		#endregion

		AdminConnection NewAdminConnection()
		{
			adminConnection?.Dispose();
			return Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb);
		}

		AdminConnection AdminConnection
		{
			get
			{
				if (adminConnection == null)
				{
					return adminConnection = NewAdminConnection();
				}

				if (adminConnection.State != ConnectionState.Open)
				{
					adminConnection.CloseConnection();
					adminConnection.EnsureIsOpen();
				}

				return adminConnection;
			}
		}

		AdminConnection adminConnection;
		string deprecatedFeatureStatisticStart;
		string deprecatedFeatureStatisticEnd;
		long commandExecuteCount;
	}
}
#endif
