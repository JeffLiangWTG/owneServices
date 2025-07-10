using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Test
{
	[TestedType(typeof(GetInstanceId))]
	sealed class GetInstanceIdTest : DbCreateScriptTest
	{
		public void TestGetInstanceId()
		{
			var expectedInstanceId = Db.Connection.ExecuteScalar<string>("select LEFT(convert(varchar(256), HASHBYTES('SHA2_256', CONCAT(DB_Name(), recovery_fork_guid)), 2), 10) FROM sys.database_recovery_status WHERE database_id = DB_ID()");
			using (var command = Db.Connection.Command("GetInstanceId"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddOutputParameter("@InstanceId", SqlDbType.VarChar, 10, 0, 0, null);
				command.ExecuteNonQuery();

				var actualInstanceId = (string)command.GetParameterValue("@InstanceId");
				AssertEquals(expectedInstanceId, actualInstanceId);
			}
		}
	}
}

