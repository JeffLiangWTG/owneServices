using System;
using System.Data.Common;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core
{
	public static class SqlExceptionExtensions
	{
		public static bool IsDeadlock(this DbException ex)
		{
			return new DbErrorMatch(ex).ExceptionType == DbErrorType.DeadlockError;
		}

		public static bool IsLockOutException(this DbException ex)
		{
			return Db.IsUpgradeLockoutError(ex);
		}

		public static bool IsTimeoutExpired(this DbException ex)
		{
			return new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired;
		}

		public static bool IsLockTimeoutExpired(this DbException ex)
		{
			return new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired;
		}

		public static bool IsInnermostDeadlock(this Exception ex)
		{
			var innermostEx = ex.GetInnermostException();

			if (innermostEx is DbException sqlEx)
			{
				return sqlEx.IsDeadlock();
			}
			return false;
		}

		public static bool IsInnermostLockTimeoutExpired(this Exception ex)
		{
			var innermostEx = ex.GetInnermostException();

			if (innermostEx is DbException sqlEx)
			{
				return sqlEx.IsLockTimeoutExpired();
			}
			return false;
		}

		public static bool IsInnermostTimeoutExpired(this Exception ex)
		{
			var innermostEx = ex.GetInnermostException();

			if (innermostEx is DbException sqlEx)
			{
				return sqlEx.IsTimeoutExpired();
			}
			return false;
		}

		public static bool IsInnermostSpecifiedError(this Exception ex, DbErrorType errorType)
		{
			var innermostEx = ex.GetInnermostException();

			if (innermostEx is DbException sqlEx)
			{
				return sqlEx.IsSpecifiedError(errorType);
			}
			return false;
		}

		public static bool IsInvalidObjectName(this DbException ex)
		{
			return new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName;
		}

		public static bool IsSpecifiedError(this DbException ex, DbErrorType errorType)
		{
			return new DbErrorMatch(ex).ExceptionType == errorType;
		}

		public static bool IsSevereError(this DbException ex)
		{
			return new DbErrorMatch(ex).ExceptionType == DbErrorType.SevereError;
		}

		public static bool ShouldReprocess(this Exception ex)
		{
			if (ex == null)
			{
				return false;
			}

			if (ex is ZCannotSaveException cannotSaveException)
			{
				return cannotSaveException.ShouldReprocess;
			}

			if (ex is ZSaveConcurrencyException
				|| ex is TransactionException
				|| (ex is ZSaveException saveException && saveException.CanRecover)
				|| ex is InvalidOperationException && ex.Message.StartsWith((NoResString)"This SqlTransaction has completed; it is no longer usable"))
			{
				return true;
			}

			if (ex is DbException sqlEx && new DbErrorMatch(sqlEx).ExceptionType is DbErrorType.GeneralNetworkError or DbErrorType.SevereError)
			{
				return true;
			}

			var isTimeoutOrDeadLock = ExceptionExtensions.FlattenInnerExceptions(ex).Any(e =>
			{
				if (e is DbException sqlEx && (sqlEx.IsTimeoutExpired() || sqlEx.IsInnermostDeadlock() || sqlEx.IsLockTimeoutExpired()))
				{
					return true;
				}
				return false;
			});

			if (isTimeoutOrDeadLock)
			{
				return true;
			}

			return false;
		}
	}
}
