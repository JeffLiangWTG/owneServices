using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Testing
{
	[TestedType(typeof(ContainerSummaryFunctionSPR))]
	internal class ContainerSummaryFunctionSPREDWTest : BiCreateScriptTest
	{
		public void TestContainerSummaryFunction()
		{
			string sql = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[dbo].[ContainerSummaryFunctionSPR]('20R, 40F')",
					ScriptDbName
			);

			using (var command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
