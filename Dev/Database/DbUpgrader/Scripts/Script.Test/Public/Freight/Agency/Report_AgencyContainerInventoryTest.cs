using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_AgencyContainerInventory))]
	internal class Report_AgencyContainerInventoryTest : DbCreateScriptTest
	{
		public void TestRun()
		{
			const string sql = "select * from dbo.Report_AgencyContainerInventory(null, null, '', '', '')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

