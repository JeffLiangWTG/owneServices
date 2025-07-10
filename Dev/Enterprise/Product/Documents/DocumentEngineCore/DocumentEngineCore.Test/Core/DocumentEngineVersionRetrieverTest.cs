using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class DocumentEngineVersionRetrieverTest : TestCaseWithFactory
	{
		public void TestActivePrinters()
		{
			CreateNewPrintQueue("Test", "Queue 1", true);
			CreateNewPrintQueue("Test", "Queue 2", true);
			CreateNewPrintQueue("Test", "Queue 3", false);
			CreateNewPrintQueue("Test", "Queue 4", false);
			CreateNewPrintQueue("TestServer2", "Queue 1", true);
			CreateNewPrintQueue("TestServer2", "Queue 2", true);
			CreateNewPrintQueue("TestServer2", "Queue 3", false);
			CreateNewPrintQueue("TestServer2", "Queue 4", false);

			DocumentEngineVersionRetriever versionRetriever = new DocumentEngineVersionRetriever();
			AssertEquals("There should be 4 active printers", 4, versionRetriever.ActivePrintersCount);
		}

		IStmPrintQueue CreateNewPrintQueue(ZString serverName, ZString queueName, ZBool isActive)
		{
			var queue = Factory.New<IStmPrintQueue>();
			queue.SQ_ServerName = serverName;
			queue.QueueName = queueName;
			queue.SQ_AllowPrinting = isActive;
			Factory.Save();
			return queue;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.ClearTable(StmPrintQueueSchema.Constants.TableName);
		}
	}
}
