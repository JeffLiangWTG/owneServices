using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	abstract class ClusterKeyDoerWhereDataIsInsertedWhileRunningTest : TransactionedTestCase
	{
		protected class OnlineClusterKeyDoerForOnlineTransformationSimulation : ClusterKeyDoer.OnlineClusterKeyDoer
		{
			public OnlineClusterKeyDoerForOnlineTransformationSimulation(string concurrentInsertScript, DbConnection testConnection) : base(new DummyUpgradeManager())
			{
				this.concurrentInsertScript = concurrentInsertScript;
				this.testConnection = testConnection;
			}

			readonly string concurrentInsertScript;
			readonly DbConnection testConnection;

			protected override void InsertTestDataForOnlineTransformation()
			{
				testConnection.ExecuteNonQuery(concurrentInsertScript);
			}
		}
	}
}
