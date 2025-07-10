using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_PrincipalTradingStatementDetail))]
	internal class Report_PrincipalTradingStatementDetailTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			const string sql = "select * from Report_PrincipalTradingStatementDetail('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'DB165592-5A82-47E5-AFAA-079137E27FBF', '2000-01-01', '2010-12-31')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

