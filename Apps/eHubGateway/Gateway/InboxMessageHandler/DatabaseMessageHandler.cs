using System;
using System.Data.SqlClient;
using Common.Logging;

namespace CargoWise.eHub.Gateway
{
	public abstract class DatabaseMessageHandler : MessageHandler
	{
		protected static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseMessageHandler));

		protected void Execute(Action action)
		{
			try
			{
				action();
			}
			catch (SqlException sqlException)
			{
				Logger.Error(sqlException);
				SqlExceptionHandler.ThrowSystemUnderMaintananceExceptionIfApplicable(sqlException);
				throw;
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				throw;
			}
		}

		protected void DoEnqueue(Action action)
		{
			try
			{
				action();
			}
			catch (ApplicationException)
			{
				throw;
			}
			catch (SqlException sqlException)
			{
				Logger.Error(sqlException);
				SqlExceptionHandler.ThrowSystemUnderMaintananceExceptionIfApplicable(sqlException);
				throw;
			}
			catch (Exception ex)
			{
				if (Logger.IsErrorEnabled) Logger.Error(ex);
				throw new SystemUnderMaintananceException(ex);
			}
		}
	}
}
