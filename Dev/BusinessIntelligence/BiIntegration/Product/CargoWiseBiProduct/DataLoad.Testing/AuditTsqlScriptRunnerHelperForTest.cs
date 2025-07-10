using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;

namespace CargoWise.Bi.Product.DataLoad.Testing;

public class AuditTsqlScriptRunnerHelperForTest : IAuditTsqlScriptRunnerHelperForTest
{
	public void RunAETWithAdminDbConnection(AdminConnection adminConnection)
	{
		var scanner = new EdwEtlExecutionTest.CdcScannerForTest();
		scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

		var logger = new LoggerForTest();
		var auditTsqlScriptRunner = new AuditTsqlScriptRunner(adminConnection, logger);
		auditTsqlScriptRunner.Run();
	}

	public void EnableCdc(AdminConnection connection, string testSchemaName, string testTableName)
	{
		if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
		{
			CdcTestHelper.TruncateLog(connection);
			CdcDatabase.Enable(connection, Db.DatabaseName);
		}

		var cdcTable = new CdcTableForTesting(testSchemaName, testTableName);
		if (!cdcTable.IsCdcEnabled(connection))
		{
			cdcTable.EnableCdc(connection);
		}
	}
}