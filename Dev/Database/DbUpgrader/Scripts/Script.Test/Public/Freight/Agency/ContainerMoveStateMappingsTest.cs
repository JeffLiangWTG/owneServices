using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(ContainerMoveStateMappings))]
	internal class ContainerMoveStateMappingsTest : DbCreateScriptTest
	{
		public void TestRun()
		{
			const string sql = "select * from dbo.ContainerMoveStateMappings()";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}
		}
	}
}

