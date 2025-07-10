using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	public class JCDActionStrategyTest : TestCaseWithFactory
	{
		public void TestTimeoutOnSynchronizeDBObjects()
		{
			var strategy = new JCDActionStrategyForTest(TestConnection);
			strategy.Process();
			AssertEquals("Timeout should be infinite to ensure all JCD DB objects are synchronized successfully", DbCommand.Timeout.Infinite, strategy.LastSyncTimeout);
		}

		class JCDActionStrategyForTest : JCDActionStrategy
		{
			public JCDActionStrategyForTest(DbConnection connection) : base(connection, new TestServiceLogger())
			{
			}

			public int LastSyncTimeout { get; set; } = -1;

			protected override void SynchronizeDBObjectsCore()
			{
				LastSyncTimeout = connection.DefaultCommandTimeOutInSeconds;
				base.SynchronizeDBObjectsCore();
			}

			protected override bool CanPerform()
			{
				return true;
			}

			protected override string GetCannotPerformMessage()
			{
				return "";
			}

			protected override void ProcessData()
			{
			}
		}
	}
}
