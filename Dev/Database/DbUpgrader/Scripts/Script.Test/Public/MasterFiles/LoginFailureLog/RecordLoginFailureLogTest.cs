using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.LoginFailureLog;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.LoginFailureLog
{
	[TestedType(typeof(RecordLoginFailureLog))]
	class RecordLoginFailureLogTest : DbCreateScriptTest
	{
		public void TestProcedure()
		{
			var hash = CreateHash("contact@wisetech.com");

			var commandText = string.Format(@"
-- Should not be locked out and create one Log
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- Add second record to 'contact@wisetech.com' but with hash, should not be locked out
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', @LoginHash, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- Should be lockedout as last attempt and create a lockedout log
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'newcontact@gmail.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'newcontact@gmail.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- Should already be locked out and not create new log record
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'lockedoutcontact@hotmail.com', null, 1, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');");

			using (var cmd = TestConnection.Command(commandText))
			{
				cmd.AddParameter("@LoginHash", SqlDbType.VarBinary, hash);
				cmd.ExecuteNonQuery();
			}

			var logRecords = CountLogs("contact@wisetech.com");
			AssertEquals("Precondition", 1, logRecords);
			var result = RecordLoginFailureLogResults("contact@wisetech.com", null, "OC", 3);
			AssertEquals("Has just one log, should not be blocked", false, result);
			AssertEquals("New log added", 2, CountLogs("contact@wisetech.com"));

			logRecords = CountLogs("newcontact@gmail.com");
			AssertEquals("Precondition", 2, logRecords);
			result = RecordLoginFailureLogResults("newcontact@gmail.com", null, "OC", 3);
			AssertEquals("Has 2 logs, should create a new log and lock it out", true, result);
			AssertEquals("New log added", 3, CountLogs("newcontact@gmail.com"));

			logRecords = CountLogs("lockedoutcontact@hotmail.com");
			AssertEquals("Precondition", 1, logRecords);
			result = RecordLoginFailureLogResults("lockedoutcontact@hotmail.com", null, "OC", 3);
			AssertEquals("It's already locked out", true, result);
			AssertEquals("No new log", 1, CountLogs("lockedoutcontact@hotmail.com"));
		}

		public void TestProcedure_ContactWithHash()
		{
			var hash = CreateHash("contact@wisetech.com");

			var commandText = string.Format(@"
-- Should not be locked out and create one Log
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- Add second record to 'contact@wisetech.com' but with hash, should not be locked out
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', @LoginHash, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');");

			using (var cmd = TestConnection.Command(commandText))
			{
				cmd.AddParameter("@LoginHash", SqlDbType.VarBinary, hash);
				cmd.ExecuteNonQuery();
			}

			var logRecords = CountLogs("contact@wisetech.com");
			var logRecordsWithHash = CountLogs("contact@wisetech.com", hash);
			AssertEquals("Precondition", 1, logRecords);
			AssertEquals("Precondition", 1, logRecordsWithHash);

			var result = RecordLoginFailureLogResults("contact@wisetech.com", null, "OC", 3);
			AssertEquals("Has just one log, should not be blocked", false, result);
			AssertEquals("New log added", 2, CountLogs("contact@wisetech.com"));
			AssertEquals("No log added for contact with hash", 1, CountLogs("contact@wisetech.com", hash));
			result = RecordLoginFailureLogResults("contact@wisetech.com", hash, "OC", 3);
			AssertEquals("Has just one log, should not be blocked", false, result);
			AssertEquals("Contact without hash should still have 2 logs", 2, CountLogs("contact@wisetech.com"));
			AssertEquals("New log added for contact with hash", 2, CountLogs("contact@wisetech.com", hash));
		}

		public void TestProcedure_LockoutMinutes()
		{
			var commandText = string.Format(@"
-- Should not be locked out and create one Log
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- Should not consider this record because it's too old unless LockoutMinutes is '0'
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'OC', DATEADD(MINUTE, -16, GetUtcDate()), 'E', GetUtcDate(), 'E');");

			using (var cmd = TestConnection.Command(commandText))
			{
				cmd.ExecuteNonQuery();
			}

			// LockoutMinutes = 15
			var logRecords = CountLogs("contact@wisetech.com");
			AssertEquals("Precondition", 1, logRecords);

			// LockoutMinutes = 0
			logRecords = CountLogs("contact@wisetech.com", null, 0);
			AssertEquals("Precondition", 2, logRecords);

			var result = RecordLoginFailureLogResults("contact@wisetech.com", null, "OC", 3, 0);
			AssertEquals("Should be locked out because we consider all logs", true, result);
			AssertEquals("New log added", 3, CountLogs("contact@wisetech.com", null, 0));

			// LockoutMinutes = 15
			logRecords = CountLogs("contact@wisetech.com");
			AssertEquals("Load logs within the lockout time", 2, logRecords);
		}

		public void TestProcedure_LoadLogsForTheRightTable()
		{
			var commandText = string.Format(@"
-- Should not be locked out and create one Log
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- Should not consider this record because it's from a different table
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES (NEWID(), 'contact@wisetech.com', null, 0, 'PER', GetUtcDate(), 'E', GetUtcDate(), 'E');");

			using (var cmd = TestConnection.Command(commandText))
			{
				cmd.ExecuteNonQuery();
			}

			var logRecords = CountLogs("contact@wisetech.com");
			AssertEquals("Precondition", 1, logRecords);

			var result = RecordLoginFailureLogResults("contact@wisetech.com", null, "OC", 3, 0);
			AssertEquals("Has just one log for the OrgContact table, should not be blocked", false, result);
			AssertEquals("New log added", 2, CountLogs("contact@wisetech.com"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int CountLogs(string loginName, byte[] hash = null, int lockoutMinutes = 15)
		{
			object result;
			var sql = $"SELECT COUNT(*) FROM dbo.StmLoginFailureLog WHERE SFL_LoginName = '{loginName}' AND SFL_TableCode = 'OC' AND (@LockoutMinutes = 0 OR SFL_SystemCreateTimeUtc > DATEADD(MINUTE, -@LockoutMinutes, GETUTCDATE())) ";

			if (hash == null)
			{
				sql += " AND SFL_LoginHash IS NULL";
			}
			else
			{
				sql += " AND SFL_LoginHash = @LoginHash";
			}

			using (var cmd = Db.Connection.Command(sql))
			{
				if (hash != null)
				{
					cmd.AddParameter("@LoginHash", SqlDbType.VarBinary, hash);
				}
				cmd.AddParameter("@LockoutMinutes", SqlDbType.Int, lockoutMinutes);
				result = cmd.ExecuteScalar();
			}
			return (int)result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		bool RecordLoginFailureLogResults(string loginName, byte[] loginHash, string tableCode, int maxAttempts, int lockoutMinutes = 15)
		{
			var result = false;

			using (var command = Db.Connection.Command("RecordLoginFailureLog"))
			{
				command.CommandType = CommandType.StoredProcedure;

				if (loginHash != null)
				{
					command.AddParameter("@LoginHash", SqlDbType.VarBinary, loginHash);
				}
				else
				{
					command.AddParameter("@LoginHash", SqlDbType.VarBinary, DBNull.Value);
				}

				command.AddParameter("@LoginName", SqlDbType.NVarChar, loginName);
				command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);
				command.AddParameter("@MaxAttempts", SqlDbType.Int, maxAttempts);
				command.AddParameter("@LockoutMinutes", SqlDbType.Int, lockoutMinutes);
				command.AddParameter("@SystemCreateEditUser", SqlDbType.VarChar, 'E');
				command.AddOutputParameter("@IsLockedOut", SqlDbType.Bit, 0, 0, 0, 0);

				command.ExecuteNonQuery();
				result = (bool)command.GetParameterValue("@IsLockedOut");
			}
			return result;
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
