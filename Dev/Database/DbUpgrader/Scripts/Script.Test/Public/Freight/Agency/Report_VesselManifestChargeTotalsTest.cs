using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_VesselManifestChargeTotals))]
	internal class Report_VesselManifestChargeTotalsTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from Report_VesselManifestChargeTotals(null, null, null, null, null, null, null, null, null, 'ALL')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

