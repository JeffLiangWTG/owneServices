using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	public static class LoginAttemptRecorder
	{
		public static bool RecordFailedLoginAttempts(string loginName, string tableCode, byte[] hash, int maxLoginAttempts, int lockoutMinutes, string systemCreateUser)
		{
			if (maxLoginAttempts == 0) // If value is zero then we allow unlimited attempts
			{
				return false;
			}

			bool result;
			using (var command = Db.Connection.Command("RecordLoginFailureLog"))
			{
				command.CommandType = CommandType.StoredProcedure;

				if (string.IsNullOrEmpty(systemCreateUser))
				{
					systemCreateUser = User.ServiceUserCode;
				}

				command.AddParameter("@LoginHash", SqlDbType.VarBinary, hash as object ?? DBNull.Value);
				command.AddParameter("@LoginName", SqlDbType.NVarChar, loginName);
				command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);
				command.AddParameter("@MaxAttempts", SqlDbType.Int, maxLoginAttempts);
				command.AddParameter("@LockoutMinutes", SqlDbType.Int, lockoutMinutes);
				command.AddParameter("@SystemCreateEditUser", SqlDbType.VarChar, systemCreateUser);
				command.AddOutputParameter("@IsLockedOut", SqlDbType.Bit, 0, 0, 0, 0);

				command.ExecuteNonQuery();
				result = (bool)command.GetParameterValue("@IsLockedOut");
			}

			return result;
		}

		public static bool IsLockedOut(string loginName, string tableCode, byte[] hash, int lockoutMinutes, bool ignoreHash = false)
		{
			bool result;
			using (var command = Db.Connection.Command("IsUserLockedOut"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@LoginHash", SqlDbType.VarBinary, hash as object ?? DBNull.Value);
				command.AddParameter("@LoginName", SqlDbType.NVarChar, loginName);
				command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);
				command.AddParameter("@LockoutMinutes", SqlDbType.Int, lockoutMinutes);
				command.AddParameter("@IgnoreHash", SqlDbType.Bit, ignoreHash);
				command.AddOutputParameter("@IsLockedOut", SqlDbType.Bit, 0, 0, 0, 0);

				command.ExecuteNonQuery();
				result = (bool)command.GetParameterValue("@IsLockedOut");
			}

			return result;
		}

		public static void Unlock(string loginName, string tableCode)
		{
			using (var command = Db.Connection.Command("RemoveLoginFailureLogs"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@LoginName", SqlDbType.NVarChar, loginName);
				command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);
				command.ExecuteNonQuery();
			}
		}

		public static ZDateTime LockoutDateTimeLocal(string loginName, string tableCode, int lockoutMinutes)
		{
			var result = ZDateTime.Empty;

			var query = new ZQuery();
			query.AddToFilter(StmLoginFailureLogSchema.SFL_IsLockOut, true);
			query.AddToFilter(StmLoginFailureLogSchema.SFL_TableCode, tableCode);

			if (lockoutMinutes != 0)
			{
				query.AddToFilter(StmLoginFailureLogSchema.SFL_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddMinutes(-lockoutMinutes));
			}

			query.AddToFilter(StmLoginFailureLogSchema.SFL_LoginName, loginName);
			query.OrderBy = StmLoginFailureLogSchema.Constants.SFL_SystemCreateTimeUtc + " DESC";

			var lockedoutUser = new BusinessObjectFactory().LoadTop1<StmLoginFailureLog>(query);
			if (lockedoutUser != null)
			{
				if (lockoutMinutes == 0)
				{
					result = ZDateTime.MaxSmallDateTime;
				}
				else
				{
					result = lockedoutUser.SFL_SystemCreateTimeUtc.AddMinutes(lockoutMinutes).ToLocalBranchTime();
				}
			}

			return result;
		}
	}
}
