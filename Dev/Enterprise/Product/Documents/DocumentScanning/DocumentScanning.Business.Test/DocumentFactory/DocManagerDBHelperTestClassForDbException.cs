using CargoWise.Data;
using CargoWise.Data.Testing;

namespace Enterprise.DocumentScanning.Business.Testing
{
	sealed class DocManagerDBHelperTestClassForDbException : DocManagerDBHelper
	{
		public DocManagerDBHelperTestClassForDbException()
		{
			SkipRefreshDbReaderRolePermissionsForTest = true;
		}

		internal override int GetDatabaseSizeMB(int dbNumber)
		{
			ThrowDbInTransitionException();
			return base.GetDatabaseSizeMB(dbNumber);
		}

		void ThrowDbInTransitionException()
		{
			var error = SqlExceptionBuilder.CreateSqlError(952, 1, 1, Db.Connection.ServerName, "Transaction Terminated", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);
			throw exception;
		}
	}
}
