using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_ReconcileBookedActualContainerSummary))]
	internal class Report_ReconcileBookedActualContainerSummaryTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from dbo.Report_ReconcileBookedActualContainerSummary(null, null, null, null, null, null, null)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

