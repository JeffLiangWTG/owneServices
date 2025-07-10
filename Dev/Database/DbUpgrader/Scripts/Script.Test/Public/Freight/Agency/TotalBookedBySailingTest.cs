using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(TotalBookedBySailing))]
	internal class TotalBookedBySailingTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from TotalBookedBySailing(null, null, null)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

