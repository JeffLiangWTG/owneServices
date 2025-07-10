using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_ReconcileContainerMovementsByVoyage))]
	internal sealed class Report_ReconcileContainerMovementsByVoyageTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from dbo.Report_ReconcileContainerMovementsByVoyage('', '', null, null, 'N')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

