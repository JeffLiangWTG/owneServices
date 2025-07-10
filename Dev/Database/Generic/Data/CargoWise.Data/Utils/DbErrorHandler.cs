using System;
using System.Data.Common;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using CargoWise.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	/// <summary>
	/// Centralise all DB (SQL Server) exception comparisons whenever a specific action 
	/// is required based upon the type of exception caught
	/// </summary>
	public class DbErrorHandler
	{
		public DbErrorHandler(DbException ex, DbConnection connection)
		{
			Argument.NotNull(ex, nameof(ex));

			sqlServerConnection = connection;
			sqlServerExceptionMessage = ex.Message;
			sqlServerExceptionStackTrace = ex.StackTrace;
			dbErrorMatch = new DbErrorMatch(ex);
		}

		public bool IsInfrastructureDbError => dbErrorMatch.IsInfrastructureDbError;

		#region Exception Type

		public bool ShouldBeRetried
		{
			get
			{
				return ExceptionType == DbErrorType.DeadlockError
					|| ExceptionType == DbErrorType.GeneralNetworkError
					|| ExceptionType == DbErrorType.TimeoutExpired
					|| ExceptionType == DbErrorType.LockTimeoutExpired;
			}
		}

		public DbErrorType ExceptionType
		{
			get
			{
				return dbErrorMatch.ExceptionType;
			}
		}

		#endregion

		#region Friendly Error Message

		public string GetDBErrorUserFriendlyMessage()
		{
			return dbErrorMatch.GetUserFriendlyMessage(sqlServerConnection);
		}

		public string IndexNameIfUniqueIndexViolation
		{
			get
			{
				if (indexNameIfUniqueIndexViolation == null)
				{
					indexNameIfUniqueIndexViolation = dbErrorMatch.GetIndexNameIfUniqueIndexViolation();
				}

				return indexNameIfUniqueIndexViolation;
			}
		}
		string indexNameIfUniqueIndexViolation;

		public string GetExtraDebugInformation()
		{
			string result = "";

			if (ExceptionType == DbErrorType.DeadlockError
				|| ExceptionType == DbErrorType.TimeoutExpired
				|| ExceptionType == DbErrorType.LockTimeoutExpired
				)
			{
				try
				{
					result = SqlEventTracker.Instance.LastSqlQuery + GetUsers() + GetStackTrace();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = "Can't generate debug info since: " + ex; // exception message
				}
			}

			return result;
		}

		#region Deadlock and Timeout

		string GetUsers()
		{
			StringBuilder result = new StringBuilder();

			try
			{
				var query = GlobalServiceProvider.Instance.GetRequiredService<IActiveUserQuery>();
				foreach (string userName in query.GetActiveUsers(true))
				{
					result.Append(Environment.NewLine + userName);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.Append("Error getting logged in users: " + ex); // exception message
			}

			return "\r\n\r\n-- LOGGED IN USERS --\r\n" + result; // diagnostic message
		}

		string GetStackTrace()
		{
			return string.IsNullOrWhiteSpace(sqlServerExceptionStackTrace) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "\r\n\r\n-- STACK TRACE -- \r\n{0}", sqlServerExceptionStackTrace); // diagnostic message
		}

		#endregion

		#endregion

		#region CanRecover

		public bool CanRecover
		{
			get
			{
				if (dbErrorMatch.ExceptionType == DbErrorType.CannotInsertDuplicateConstraintKey)
				{
					var constraintName = MetaData.GetUniqueIndexNameFromErrorMessage(sqlServerExceptionMessage);
					if (constraintName.StartsWith("PK_"))
					{
						return false;
					}
				}

				return true;
			}
		}

		#endregion

		readonly DbConnection sqlServerConnection;

		readonly string sqlServerExceptionMessage;
		readonly string sqlServerExceptionStackTrace;

		readonly IDbErrorMatch dbErrorMatch;
	}
}
