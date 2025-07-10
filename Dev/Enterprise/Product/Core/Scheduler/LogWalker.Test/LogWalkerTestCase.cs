using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.LogWalker.Test
{
	public abstract class LogWalkerTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery("delete from dbo.StmALogQueue"); // Clean up so we don't trip over crap in the DB.
			Db.Connection.ExecuteNonQuery("delete from dbo.StmALogQueueWTE"); // Clean up so we don't trip over crap in the DB.
			Db.Connection.ExecuteNonQuery("delete from dbo.StmJobQueue"); // Clean up so we don't trip over crap in the DB.
			base.SetUp();
		}

		public LoggerForLogWalkerTest Logger => logger ?? (logger = new LoggerForLogWalkerTest());
		LoggerForLogWalkerTest logger;

		public void AssertLoggerHasNoErrors()
		{
			foreach (var logEntry in Logger.LogEntries)
			{
				if (logEntry.LogType == LogType.Error || logEntry.LogType == LogType.Warning)
				{
					HtmlFail(logEntry.ToString());
				}
			}
		}
	}
}
