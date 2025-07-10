using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.DocManager;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.DocManager
{
	[TestedType(typeof(DbUsedSize))]
	class DbUsedSizeTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			using (DbCommand command = TestConnection.Command(ScriptToTest.Name))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@DbName", SqlDbType.VarChar, TestConnection.CurrentDatabase);
				command.AddOutputParameter("@DbSizeMb", SqlDbType.Int, 0, 0, 0, 0);
				command.ExecuteNonQuery();
				int dbSize = (int)command.GetParameterValue("@DbSizeMb");
				Assert("Main db has some size", dbSize > 100);
			}
		}
	}
}

