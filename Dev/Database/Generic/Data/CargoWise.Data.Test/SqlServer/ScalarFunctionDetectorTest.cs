using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	sealed class ScalarFunctionDetectorTest : TransactionedTestCase
	{
		public void TestScalarFunction()
		{
			Db.Connection.ExecuteNonQuery(@"CREATE FUNCTION [dbo].[Foo] (@Text varchar(128))
RETURNS int with schemabinding
AS
BEGIN
	DECLARE @notImportantTime datetimeoffset = GETDATE(); -- A statement to avoid inline function
    return 1;
END");
			using (var cmd = Db.Connection.Command("select dbo.Foo(Z0_Code) from dbo.DummyBizO"))
			{
				Assert(new ScalarFunctionDetector().IsScalarFunction(cmd, Db.Connection));
			}
		}

		public void TestNonScalarFunction()
		{
			using (var cmd = Db.Connection.Command("select Z0_Code from dbo.DummyBizO"))
			{
				Assert(!new ScalarFunctionDetector().IsScalarFunction(cmd, Db.Connection));
			}
		}
	}
}
