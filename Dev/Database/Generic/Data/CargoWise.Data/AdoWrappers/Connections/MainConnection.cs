using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	class MainConnection : DbConnection<RestrictedWriterLoginCredentials>, IOpenConnectionErrorHandler
	{
		/// <summary>
		/// Private Constructor (singleton pattern used) 
		/// </summary>
		protected MainConnection()
			: base()
		{
		}

		#region Singleton Pattern

		internal static MainConnection Instance
		{
			get
			{
				try
				{
					return lazyInstance.Value;
				}
				catch (TypeInitializationException ex)
				{
					var errorToThrow = ex.InnerException ?? ex;
					throw new Exception(errorToThrow.Message, errorToThrow);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<MainConnection> lazyInstance = new Lazy<MainConnection>(() => new MainConnection(), isThreadSafe: true);

		#endregion

		#region Internal Database Connection

		/// <summary>
		/// No point pooling on the Main connection as it is static singleton for the whole application.
		/// The reference to the DB connection is never released and therefore pooling would not work effectively anyway.
		/// </summary>
		protected override IConnectionPooling ConnectionPoolingValue
		{
			get { return noPooling; }
		}

		readonly IConnectionPooling noPooling = new NoConnectionPooling();

		protected override void RunTasksAfterOpenConnection()
		{
			base.RunTasksAfterOpenConnection();

			OnConnectionOpened?.Invoke(this, null);
			SetSecurityProtocolsThatWeWantToSupport();
		}

		void SetSecurityProtocolsThatWeWantToSupport()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
		}

		public static event EventHandler OnConnectionOpened;

		#endregion

		#region IOpenConnectionErrorHandler Members

		public override bool HandleError(System.Data.Common.DbException e)
		{
			bool result = base.HandleError(e);
			if (!result)
			{
				DbErrorType errorType = GetErrorType(e);

				if (errorType == DbErrorType.ServerDoesNotExist)
				{
					HandleServerDoesNotExistOpenConnectionError(e, errorType);
					result = true;
				}
				else if (
					errorType == DbErrorType.LoginFailedForUser
					|| errorType == DbErrorType.CannotOpenDbRequestedInLogin
				)
				{
					if (hasConnectedBefore)
					{
						// This caters for failover scenarios, when the login doesn't exist in the failover server.
						try
						{
							result = Db.HandleDbConnectionSetupErrors<RestrictedWriterLoginCredentials>(e, ServerName, fInitialDatabaseName, ServerName);
						}
						catch (SqlException)
						{
							// Returns handled = false instead of throwing another exception which would hide the original one
							result = false;
						}
					}
				}
			}
			return result;
		}

		#region Handle ServerDoesNotExist

		void HandleServerDoesNotExistOpenConnectionError(System.Data.Common.DbException e, DbErrorType errorType, int retriesRemaining = 3)
		{
			if (retriesRemaining > 0 && errorType == DbErrorType.ServerDoesNotExist && ConfirmReopenConnection())
			{
				try
				{
					OpenConnectionWithSplashInfo();
				}
				catch (SqlException retryEx)
				{
					HandleServerDoesNotExistOpenConnectionError(retryEx, GetErrorType(retryEx), retriesRemaining - 1);
				}
			}
			else
			{
				throw new LoginException("Failed to connect to database", e);
			}
		}

		/// <summary>
		/// Handle network/server problems.
		///  - 1st connection attempt:
		///    Pings to the server machine. If it replies it's a server configuration problem (Exit Application).
		///  - Subsequent connection attempts:
		///    Requests user confirmation to keep on trying to reconnect.
		/// </summary>
		/// <returns>
		/// True : if should try again to connect
		/// False: should exit application
		/// </returns>
#if DEBUG
		protected
#endif
 bool ConfirmReopenConnection()
		{
			if (!hasConnectedBefore && DataUtils.CheckPing(fServerName))
			{
				string errorMessage = string.Format("Database server {0} is not running\r\nContact Systems Administrator", fServerName); // Exception message should not be translated
				throw new LoginException(errorMessage, null);
			}

			// Handle genuine network problems to allow the user to 
			// - keep on trying to reconnect or 
			// - abort the application
			// - Give them a maximum of MaxRetryAttemptsWithInTimespan attempts within MaximumTimeSpentTryingToReconnect before we tell the user they should close instead

			AddAttempt();

			return HasReachedMaxAttempts ? !RequestPermissionToExit() : GetUserConfirmationToReopenConnection();
		}

		#region Retry handling

		const int MaxRetryAttemptsWithInTimespan = 3;
		protected virtual TimeSpan MaximumTimeSpentTryingToReconnect
		{
			get
			{
				return TimeSpan.FromSeconds(1.5 * DefaultCommandTimeOutInSeconds * (MaxRetryAttemptsWithInTimespan));
			}
		}

		readonly Queue<DateTime> lastReconnectAttempts = new Queue<DateTime>();

		void AddAttempt()
		{
			lastReconnectAttempts.Enqueue(DateTime.UtcNow);
		}

		bool HasReachedMaxAttempts
		{
			get
			{
				return lastReconnectAttempts.Count > MaxRetryAttemptsWithInTimespan && lastReconnectAttempts.Dequeue() + MaximumTimeSpentTryingToReconnect > DateTime.UtcNow;
			}
		}

		#endregion

		#region User dialogs
		#region SuppressResourceStringsCheckRegion

		bool RequestPermissionToExit()
		{
			string closeMessage = "The network error has not been resolved after several attempts, so the application will close. \r\n" +
				"If you would prefer to continue attempting to reopen connection, select No \r\n\r\n" +
				"Would you like to exit?";

			return DbEnv.Instance.ConnectionGuiPlugin.GetUserConfirmation("Exit", closeMessage);
		}

		bool GetUserConfirmationToReopenConnection()
		{
			string reopenMessage =
				"The application has detected a continuing network error.\r\n" +
				"It will close unless you choose to attempt to reconnect.\r\n" +
				"Speak with your network administrator if this problem persists.\r\n\r\n" +
				"Do you want to reopen the connection?";

			return DbEnv.Instance.ConnectionGuiPlugin.GetUserConfirmation("Network Error", reopenMessage);
		}

		#endregion
		#endregion

		#endregion
		#endregion

		protected virtual DbErrorType GetErrorType(System.Data.Common.DbException sqlEx)
		{
			Argument.NotNull(sqlEx, nameof(sqlEx));

			return new DbErrorMatch(sqlEx).ExceptionType;
		}

		public override bool IsInTransactionOtherThanTransactionedTestCase
		{
			get
			{
#if DEBUG
				return AppTransactionCount > (NUnit.Framework.TransactionedTestCase.InTransactionedTestCase ? 1 : 0);
#else
				return base.IsInTransactionOtherThanTransactionedTestCase;
#endif
			}
		}

		public override string UserLogin => RestrictedWriterLoginCredentials.UserNameFor(fInitialDatabaseName);

		protected override IDisposable NewConnectingSplashFormManager()
		{
			return DbEnv.Instance.ConnectionGuiPlugin.NewConnectingSplashFormManager();
		}
	}

	#region Interfaces

	#region IOpenConnectionErrorHandler
	public interface IOpenConnectionErrorHandler
	{
		bool HandleError(System.Data.Common.DbException e);
	}
	#endregion
	#endregion
}
