using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	sealed class TestDataTest : TransactionedTestCase
	{
		public TestDataTest()
			: base()
		{
		}

		public void TestCreateDocEngineTestTable()
		{
			TestData.CreateDocEngineTestTable();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from tempdb.sys.objects where name like '" + TestData.DocEngineTestTableName + "%'"));
			AssertEquals(160, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName));
			AssertEquals(40, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName + " where CharField1 = '0'"));
			AssertEquals(40, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName + " where CharField1 = '1'"));
			AssertEquals(40, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName + " where CharField1 = '2'"));
			AssertEquals(40, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName + " where CharField1 = '3'"));
			AssertEquals(20, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName + " where CharField2 = '3'"));
			AssertEquals(20, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.DocEngineTestTableName + " where CharField2 = '1' and CharField1 = '0'"));
		}

		public void TestCreateJobTestTable()
		{
			TestData.CreateJobTestTable();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from tempdb.sys.objects where name like '" + TestData.JobTempTableName + "%'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.JobTempTableName));
		}

		public void TestCreateHeaderTestTable()
		{
			TestData.CreateHeaderTestTable();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from tempdb.sys.objects where name like '" + TestData.HeaderTempTableName + "%'"));
			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.HeaderTempTableName));
		}

		public void TestCreateLinesTestTable()
		{
			TestData.CreateLinesTestTable();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from tempdb.sys.objects where name like '" + TestData.LinesTempTableName + "%'"));
			AssertEquals(160, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.LinesTempTableName));
		}

		public void TestCreateEmptyLinesTestTable()
		{
			TestData.CreateEmptyLinesTestTable();

			AssertEquals(1, (int)Db.Connection.ExecuteScalar("select count(*) from tempdb.sys.objects where name like '" + TestData.LinesEmptyTableName + "%'"));
			AssertEquals(0, (int)Db.Connection.ExecuteScalar("select count(*) from " + TestData.LinesEmptyTableName));
		}
	}
}
