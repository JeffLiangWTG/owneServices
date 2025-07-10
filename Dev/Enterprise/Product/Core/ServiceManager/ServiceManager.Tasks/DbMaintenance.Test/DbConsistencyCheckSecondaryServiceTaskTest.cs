using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Environment;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	sealed class DbConsistencyCheckSecondaryServiceTaskTest : TransactionedTestCase
	{
		public void TestSkipCheck()
		{
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;

			var logger = RunDbccTask();

			AssertEquals($"Full log:\r\n{string.Join(System.Environment.NewLine, logger)}", 1, logger.Count);
			AssertLoggerContains(Invariant($"Information|Skip database consistency check due to main database [{Db.DatabaseName}] is not a part of AlwaysOn"), logger);
		}

		public void TestRun_SerialByParts()
		{
			Env.Registry.DbccRunSecondariesInParallel = false;
			Env.Registry.DbccRunSecondariesByParts = true;

			var replica = Db.ServerName;
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo> { new AlwaysOnReplicaInfo { ReplicaServerName = replica, AvailabilityMode = 1 } };

			var logger = RunDbccTask();

			CombineAssertions($"Full log:\r\n{string.Join(System.Environment.NewLine, logger)}", () =>
			{
				AssertLoggerContains("Information|Checking secondary servers in a loop", logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Check database does not have consistency/allocation errors"), logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Checking [{Db.DatabaseName}] database - by parts"), logger);
				AssertEquals(Invariant($"Information|Server [{replica}]: DbHealthCheck is completed"), logger[logger.Count - 1]);
				AssertLoggerNotContains("has consistency/allocation errors.", logger, (actual, message) => actual.EndsWith(message));
				AssertLoggerNotContains("has failed a consistency check.", logger, (actual, message) => actual.EndsWith(message));
			});
		}

		public void TestRun_SerialInOneGo()
		{
			Env.Registry.DbccRunSecondariesInParallel = false;
			Env.Registry.DbccRunSecondariesByParts = false;

			var replica = Db.ServerName;
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo> { new AlwaysOnReplicaInfo { ReplicaServerName = replica, AvailabilityMode = 1 } };

			var logger = RunDbccTask();

			CombineAssertions($"Full log:\r\n{string.Join(System.Environment.NewLine, logger)}", () =>
			{
				AssertLoggerContains("Information|Checking secondary servers in a loop", logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Check database does not have consistency/allocation errors"), logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Checking [{Db.DatabaseName}] database - using DBCC CHECKDB"), logger);
				AssertEquals(Invariant($"Information|Server [{replica}]: DbHealthCheck is completed"), logger[logger.Count - 1]);
				AssertLoggerNotContains("has consistency/allocation errors.", logger, (actual, message) => actual.EndsWith(message));
				AssertLoggerNotContains("has failed a consistency check.", logger, (actual, message) => actual.EndsWith(message));
			});
		}

		public void TestRun_ParallelByParts()
		{
			Env.Registry.DbccRunSecondariesInParallel = true;
			Env.Registry.DbccRunSecondariesByParts = true;

			var replica = Db.ServerName;
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo> { new AlwaysOnReplicaInfo { ReplicaServerName = replica, AvailabilityMode = 1 } };

			var logger = RunDbccTask();

			CombineAssertions($"Full log:\r\n{string.Join(System.Environment.NewLine, logger)}", () =>
			{
				AssertLoggerContains("Information|Checking secondary servers in parallel", logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Check database does not have consistency/allocation errors"), logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Checking [{Db.DatabaseName}] database - by parts"), logger);
				AssertEquals(Invariant($"Information|Server [{replica}]: DbHealthCheck is completed"), logger[logger.Count - 1]);
				AssertLoggerNotContains("has consistency/allocation errors.", logger, (actual, message) => actual.EndsWith(message));
				AssertLoggerNotContains("has failed a consistency check.", logger, (actual, message) => actual.EndsWith(message));
			});
		}

		public void TestRun_ParallelInOneGo()
		{
			Env.Registry.DbccRunSecondariesInParallel = true;
			Env.Registry.DbccRunSecondariesByParts = false;

			var replica = Db.ServerName;
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo> { new AlwaysOnReplicaInfo { ReplicaServerName = replica, AvailabilityMode = 1 } };

			var logger = RunDbccTask();

			CombineAssertions($"Full log:\r\n{string.Join(System.Environment.NewLine, logger)}", () =>
			{
				AssertLoggerContains("Information|Checking secondary servers in parallel", logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Check database does not have consistency/allocation errors"), logger);
				AssertLoggerContains(Invariant($"Information|Server [{replica}]: Checking [{Db.DatabaseName}] database - using DBCC CHECKDB"), logger);
				AssertEquals(Invariant($"Information|Server [{replica}]: DbHealthCheck is completed"), logger[logger.Count - 1]);
				AssertLoggerNotContains("has consistency/allocation errors.", logger, (actual, message) => actual.EndsWith(message));
				AssertLoggerNotContains("has failed a consistency check.", logger, (actual, message) => actual.EndsWith(message));
			});
		}

		public void TestRun_CheckSingleReferenceDb()
		{
			Env.Registry.DbccRunSecondariesInParallel = false;
			Env.Registry.DbccRunSecondariesByParts = false;

			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;
			var logger = RunDbccTask();
			AssertLoggerNotContains($"{RefDbTableNameResolver.SingleRefDatabaseName} must NOT be in AlwaysOn group.", logger, (actual, message) => actual.EndsWith(message));

			var replica = Db.ServerName;
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			logger = RunDbccTask();
			AssertLoggerContains($"Warning|{RefDbTableNameResolver.SingleRefDatabaseName} must NOT be in AlwaysOn group.", logger);
		}

		#region Implementation

		static void AssertLoggerContains(string message, IEnumerable<string> logger)
		{
			AssertCollectionContains(message, message, logger);
		}

		static void AssertLoggerNotContains(string message, IEnumerable<string> logger, Func<string, string, bool> checkActualOnTargetMessage)
		{
			Assert(message, !logger.Any(x => checkActualOnTargetMessage.Invoke(x, message)));
		}

		IList<string> RunDbccTask()
		{
			var messages = new List<string>();
			var loggerMock = new Mock<ILogger>();
			loggerMock.Setup(x => x
				.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType type, string message) => messages.Add($"{type}|{message}"));

			var task = new DbConsistencyCheckSecondaryServiceTask { ServiceLogger = loggerMock.Object };
			task.RunTask(CancellationToken.None);

			return messages;
		}

		#endregion // Implementation
	}
}
