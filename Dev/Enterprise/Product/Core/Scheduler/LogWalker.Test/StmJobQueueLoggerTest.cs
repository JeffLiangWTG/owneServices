using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.LogWalker.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.LogWalker.Test
{
	class StmJobQueueLoggerTest : TestCaseWithFactory
	{
		public void TestDoTheLogging()
		{
			var logWalkerConfig = LogWalkerConfig.Create();
			var taskRunnerMock = new Mock<ITaskRunner>();
			var logger = new LoggerForTesting();
			var stmJobQueueLogger = new StmJobQueueLogger(
				   () => taskRunnerMock.Object,
				   logWalkerConfig,
				   new LogWalkerCategoryLogger(logger));
			using (Db.Connection.TrackExecutedCommands())
			{
				stmJobQueueLogger.DoTheLogging();
				var queryMatch = Db.Connection.ExecutedCommands.Select(c => Regex.Match(c, @$"SELECT\s+.*\s+FROM\s+dbo\.StmJobQueue\s+.*WHERE(?<filter>.*)ORDER BY\s+(?<order>.*)\/\* Parameter Stats", RegexOptions.Multiline | RegexOptions.Singleline)).FirstOrDefault(r => r.Success);
				AssertNotNull(queryMatch);
				AssertEquals(StmJobQueueSchema.SJ_ProcessOnOrAfterUtc.Name, queryMatch.Groups["order"].Value.Trim());
			}
		}
	}
}
