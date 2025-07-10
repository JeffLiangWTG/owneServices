using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	[TestedType(typeof(ContainerSummaryFunction))]
	internal class ContainerSummaryFunctionTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var sql = "Select * From ContainerSummaryFunction('20R, 40F')";

			using (var command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

