using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_ReconcileBookedActualContainers))]
	internal class Report_ReconcileBookedActualContainersTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from dbo.Report_ReconcileBookedActualContainers(null, null, null, null, null, null, null)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

