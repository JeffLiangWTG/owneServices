using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Common.Testing
{
	[TestedType(typeof(ContainerSummaryFunction))]
	internal class ContainerSummaryFunctionEDWTest : BiCreateScriptTest
	{
		public void TestContainerSummaryFunction()
		{
			string sql = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[dbo].[ContainerSummaryFunction]('20R, 40F')",
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
