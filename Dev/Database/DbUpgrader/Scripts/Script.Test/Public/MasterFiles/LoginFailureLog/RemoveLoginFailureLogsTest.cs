using System.Data;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.LoginFailureLog;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.LoginFailureLog
{
	[TestedType(typeof(RemoveLoginFailureLogs))]
	class RemoveLoginFailureLogsTest : DbCreateScriptTest
	{
		public void TestProcedure()
		{
			var hash = CreateHash("contact@wisetech.com");

			var commandText = string.Format(@"
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'contact@wisetech.com', @LoginHash, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'newcontact@wisetech.com', null, 1, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'newcontact@wisetech.com', null, 1, 'GS', GetUtcDate(), 'E', GetUtcDate(), 'E');

INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'othercontact@wisetech.com', null, 1, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');");

			using (var cmd = TestConnection.Command(commandText))
			{
				cmd.AddParameter("@LoginHash", SqlDbType.VarBinary, hash);
				cmd.ExecuteNonQuery();
			}

			AssertEquals("Precondition", 1, CountLogs("contact@wisetech.com"));
			RemoveLoginFailureLogsResult("contact@wisetech.com", "OC");
			AssertEquals("Should delete record", 0, CountLogs("contact@wisetech.com"));

			AssertEquals("Precondition", 1, CountLogs("newcontact@wisetech.com"));
			AssertEquals("Precondition", 1, CountLogs("newcontact@wisetech.com", "GS"));
			RemoveLoginFailureLogsResult("newcontact@wisetech.com", "OC");
			AssertEquals("Should delete record with the contact table code", 0, CountLogs("newcontact@wisetech.com"));
			AssertEquals("Should not delete record with the staff table code", 1, CountLogs("newcontact@wisetech.com", "GS"));

			AssertEquals("Precondition", 1, CountLogs("othercontact@wisetech.com"));
			RemoveLoginFailureLogsResult("othercontact2@wisetech.com", "OC");
			AssertEquals("Should not delete with wrong email", 1, CountLogs("othercontact@wisetech.com"));
			RemoveLoginFailureLogsResult("othercontact@wisetech.com", "OC");
			AssertEquals("Should delete", 0, CountLogs("othercontact@wisetech.com"));
		}

		int CountLogs(string loginName, string tableCode = "OC")
		{
			object result;
			var sql = $"SELECT COUNT(*) FROM dbo.StmLoginFailureLog WHERE SFL_LoginName = '{loginName}' AND SFL_TableCode = '{tableCode}'";

			using (var cmd = Db.Connection.Command(sql))
			{
				result = cmd.ExecuteScalar();
			}
			return (int)result;
		}

		void RemoveLoginFailureLogsResult(string loginName, string tableCode)
		{
			using (var command = Db.Connection.Command("RemoveLoginFailureLogs"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@LoginName", SqlDbType.NVarChar, loginName);
				command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);

				command.ExecuteNonQuery();
			}
		}

		byte[] CreateHash(string loginName)
		{
			using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("secretKey")))
			{
				return hmac.ComputeHash(Encoding.UTF8.GetBytes(loginName + "nonceData"));
			}
		}
	}
}
