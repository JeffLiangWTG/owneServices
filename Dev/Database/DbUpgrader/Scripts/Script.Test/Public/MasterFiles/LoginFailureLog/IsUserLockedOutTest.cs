using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.LoginFailureLog;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.LoginFailureLog
{
	[TestedType(typeof(IsUserLockedOut))]
	class IsUserLockedOutTest : DbCreateScriptTest
	{
		public void TestProcedure()
		{
			var hash = CreateHash("contactWithHash@wisetech.com");

			var commandText = string.Format(@"
-- not locked out
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'notLockedOut@wisetech.com', null, 0, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- with hash
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'contactWithHash@wisetech.com', @LoginHash, 1, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- null hash
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'contactNoHash@wisetech.com', null, 1, 'OC', GetUtcDate(), 'E', GetUtcDate(), 'E');

-- SFL_SystemCreateTimeUtc
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'contactCreateTime@wisetech.com', null, 1, 'OC', DATEADD(MINUTE, -15, GETUTCDATE()), 'E', DATEADD(MINUTE, -15, GETUTCDATE()), 'E');

-- different table
INSERT INTO dbo.StmLoginFailureLog (SFL_PK, SFL_LoginName, SFL_LoginHash, SFL_IsLockOut, SFL_TableCode, SFL_SystemCreateTimeUtc, SFL_SystemCreateUser, SFL_SystemLastEditTimeUtc, SFL_SystemLastEditUser) VALUES
							   (NEWID(), 'lockedoutcontact', null, 1, 'GS', GetUtcDate(), 'E', GetUtcDate(), 'E');");

			using (var cmd = TestConnection.Command(commandText))
			{
				cmd.AddParameter("@LoginHash", SqlDbType.VarBinary, hash);
				cmd.ExecuteNonQuery();
			}

			AssertEquals("Should not be locked out", false, IsUserLockedOutResults("notLockedOut@wisetech.com", null, "OC"));

			AssertEquals("Should be locked out with hash", true, IsUserLockedOutResults("contactWithHash@wisetech.com", hash, "OC"));
			AssertEquals("Should not be locked out without hash", false, IsUserLockedOutResults("contactWithHash@wisetech.com", null, "OC"));
			AssertEquals("Should be locked out without hash but ignoreHash true", true, IsUserLockedOutResults("contactWithHash@wisetech.com", null, "OC", true));

			AssertEquals("Should be locked out without hash", true, IsUserLockedOutResults("contactNoHash@wisetech.com", null, "OC"));
			AssertEquals("Should not be locked out with hash", false, IsUserLockedOutResults("contactNoHash@wisetech.com", hash, "OC"));

			AssertEquals("Should be locked out", true, IsUserLockedOutResults("contactCreateTime@wisetech.com", null, "OC", false, 20));
			AssertEquals("Should not be locked out", false, IsUserLockedOutResults("contactCreateTime@wisetech.com", null, "OC"));

			AssertEquals("Should not be locked out with wrong", false, IsUserLockedOutResults("lockedoutcontact", null, "OC"));
			AssertEquals("Should be locked out", true, IsUserLockedOutResults("lockedoutcontact", null, "GS"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		bool IsUserLockedOutResults(string loginName, byte[] loginHash, string tableCode, bool ignoreHash = false, int lockoutMinutes = 10)
		{
			var result = false;

			using (var command = Db.Connection.Command("IsUserLockedOut"))
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
				command.AddParameter("@LockoutMinutes", SqlDbType.Int, lockoutMinutes);
				command.AddParameter("@IgnoreHash", SqlDbType.Bit, ignoreHash);
				command.AddOutputParameter("@IsLockedOut", SqlDbType.Bit, 0, 0, 0, 0);

				//@LoginName NVARCHAR(254),
				//@LoginHash VARBINARY(32),
				//@TableCode VARCHAR(3),
				//@LockoutMinutes INT,
				//@IgnoreHash BIT,
				//@IsLockedOut BIT OUTPUT

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
