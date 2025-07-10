using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_MovementsOutOfSequence))]
	internal class Report_MovementOutOfSequenceTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			const string sql = "select * from dbo.Report_MovementsOutOfSequence(null, null, null, null, null)";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}

