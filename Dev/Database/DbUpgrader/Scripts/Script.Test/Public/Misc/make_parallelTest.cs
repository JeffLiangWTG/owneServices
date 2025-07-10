using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Test
{
	[TestedType(typeof(make_parallel))]
	class make_parallelTest : DbCreateScriptTest
	{
		public void TestResult()
		{
			AssertEquals("function must return exactly one row", 1, TestConnection.ExecuteScalar<int>("select count(*) from make_parallel()"));
		}
	}
}
