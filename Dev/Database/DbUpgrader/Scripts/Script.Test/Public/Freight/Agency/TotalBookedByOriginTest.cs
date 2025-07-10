using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(TotalBookedByOrigin))]
	internal class TotalBookedByOriginTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "SELECT * FROM TotalBookedByOrigin(NULL, NULL, NULL)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

