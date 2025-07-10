using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	public class ExecuteAsReader
	{
		public object Execute(DbConnection connection, Func<object> action)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(action, nameof(action));

			if (connection is DbConnection<RestrictedReaderLoginCredentials>)
			{
				return action();
			}

			var userName = RestrictedReaderLoginCredentials.UserNameFor(connection.CurrentDatabase);

			var cookieName = "@cookie" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture); // used only by developer

			var executeAsReaderLoginSql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE {0} VARBINARY(8000);
EXECUTE AS USER = '{1}' WITH COOKIE INTO {0};
SELECT CONVERT(varchar(max), {0}, 2);
", cookieName, userName); // used only by developer

			var cookieValue = String.Empty;
			try
			{
				cookieValue = connection.ExecuteScalar<string>(executeAsReaderLoginSql);

				connection.ImpersonatedLogin = userName;
			}
			catch (SqlException ex)
			{
				var errorHandler = new DbErrorHandler(ex, connection);

				if (errorHandler.ExceptionType == DbErrorType.CannotExecuteAsDatabasePrincipal ||
					errorHandler.ExceptionType == DbErrorType.CannotExecuteAsServerPrincipal ||
					errorHandler.ExceptionType == DbErrorType.DatabasePrincipalDoesNotExist)
				{
					Db.FixReaderLogin(connection.ServerName);
					cookieValue = connection.ExecuteScalar<string>(executeAsReaderLoginSql);
				}
				else
				{
					throw;
				}
			}

			var revertSql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE {0} VARBINARY(8000);
SET {0} = 0x{1};
REVERT WITH COOKIE = {0};
", cookieName, cookieValue); // used only by developer

			Action revertExecuteAsUserAction = () =>
			{
				try
				{
					connection.ImpersonatedLogin = null;

					if (!connection.IsInTransaction && connection.AppTransactionCount > 0)
					{
						connection.Dispose();
					}
					else
					{
						connection.ExecuteNonQuery(revertSql);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					connection.Dispose();
					throw;
				}
			};

			object result = null;
			IDataReader dataReader = null;

			try
			{
				result = action();
				dataReader = result as IDataReader;

				if (dataReader != null)
				{
					result = new DataReaderExecuteAsReaderLogin(dataReader, revertExecuteAsUserAction);
				}
			}
			finally
			{
				if (dataReader == null)
				{
					revertExecuteAsUserAction();
				}
			}

			return result;
		}
	}
}
