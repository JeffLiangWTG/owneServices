using System.Data.Common;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	//
	// This is part of the old Db class, responsible for Db login error handling.
	//
	// In the future, the way connection credentials are managed in the first place should be modified.
	//
	// Static dependencies from Db:
	// - NewAdminConnection()
	// - Connection.EnsureIsOpen()
	//   - This has the side effect of creating a new connection if one wasn't already open
	//

	public partial class Db
	{
		public static bool HandleDbConnectionSetupErrors<TCredentials>(DbException sqlException, string mainServerName, string mainDatabaseName, string targetServerName) where TCredentials : DBCredentials
		{
			Argument.NotNull(sqlException, nameof(sqlException));

			var errorType = new DbErrorMatch(sqlException).ExceptionType;
			if (errorType == DbErrorType.LoginFailedForUser || errorType == DbErrorType.CannotOpenDbRequestedInLogin)
			{
				return HandleDbSpecificLoginError<TCredentials>(mainServerName, mainDatabaseName, targetServerName);
			}
			else if (errorType == DbErrorType.LoginFailedBecauseItIsLockedOut)
			{
				return HandleDbSpecificLoginPasswordAndLockoutErrors<TCredentials>(mainServerName, mainDatabaseName, targetServerName);
			}

			return false;
		}

		static bool HandleDbSpecificLoginError<TCredentials>(string mainServerName, string mainDatabaseName, string targetServerName) where TCredentials : DBCredentials
		{
			using (var adminConnection = NewAdminConnection())
			{
				((IDbLoginRepair)adminConnection).EnsureRestrictedWriterDbLogin();
			}

			try
			{
				Connection.EnsureIsOpen();
			}
			catch (SqlException sqlEx)
			{
				var errorType = new DbErrorMatch(sqlEx).ExceptionType;

				if (errorType == DbErrorType.LoginFailedForUser || errorType == DbErrorType.LoginFailedBecauseItIsLockedOut)
				{
					return HandleDbSpecificLoginPasswordAndLockoutErrors<TCredentials>(mainServerName, mainDatabaseName, targetServerName);
				}

				throw;
			}

			return true;
		}

		static bool HandleDbSpecificLoginPasswordAndLockoutErrors<TCredentials>(string mainServerName, string mainDatabaseName, string targetServerName) where TCredentials : DBCredentials
		{
			using (var targetServerConnection = NewAdminConnection(targetServerName))
			{
				var mainDbConnection = (mainServerName == targetServerName) ? targetServerConnection : NewAdminConnection(mainServerName);
				LoginRepairService.Instance.ReviveApplicationLogin<TCredentials>(mainDbConnection, mainDatabaseName, targetServerConnection);
			}

			Connection.EnsureIsOpen();
			return true;
		}
	}
}
