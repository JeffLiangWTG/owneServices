using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	sealed class OperationManagerNoFactoryTest : TestCase
	{
		public void TestLockTimeout()
		{
			using (var admin = Db.NewExtraConnectionToMainDb())
			{
				admin.BeginTransaction();
				admin.ExecuteNonQuery("select count(*) from dbo.StmJobQueue with (TABLOCKX, HOLDLOCK)");
				var manager = new MasterOperationsManager();
				var testNotifier = new LoggerForTesting();
				manager.QueueAndProcessLogs(testNotifier);
				Assert("Should have timeout exceeded in text, but does not", testNotifier.NotifiedEventList.Any(x => x.Contains("Lock request time out period exceeded.")));
			}
		}
	}
}
