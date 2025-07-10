using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business.Test
{
	class SqlFailureCheckerTest : TestCaseWithFactory
	{
		public void TestIsAcceptableFailureFalse()
		{
			try
			{
				Factory.Load<DummyBusinessObject>(new ZQuery(StmNoteSchema.ST_Description, "XYZ"));
			}
			catch (SqlException ex)
			{
				AssertEquals(false, SqlFailureChecker.IsAcceptableFailure(ex));
			}
		}

		public void TestIsAcceptableFailureTrue()
		{
			try
			{
				Db.Connection.ExecuteScalar("select * from missingdatabase.dbo.hello"); // Need to get an SQL Exception out to test.
			}
			catch (SqlException ex)
			{
				AssertEquals(true, SqlFailureChecker.IsAcceptableFailure(ex));
			}
		}
	}
}
