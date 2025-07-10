using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using static Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests.AccountingDataTransform;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BiCreateScriptTests
{
	[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
	class Report_ChinaGLAccountBalanceEDWTest : Report_ChinaGLAccountBalanceTest
	{
		protected override void PrepareReport_ChinaGLAccountBalanceTest()
		{
			base.PrepareReport_ChinaGLAccountBalanceTest();

			_ = new EDWTestDataCreator(new List<(string, string)>(), Connection, ScriptDbName, Factory);
		}

		protected override string ScriptDbName => Db.EdwDatabaseName;
	}
}
