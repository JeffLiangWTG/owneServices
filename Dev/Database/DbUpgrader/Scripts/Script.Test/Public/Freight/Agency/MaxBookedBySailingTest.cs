using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(MaxBookedBySailing))]
	internal class MaxBookedBySailingTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "SELECT * FROM MaxBookedBySailing(NULL, NULL, NULL, NULL, NULL, NULL, NULL)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

