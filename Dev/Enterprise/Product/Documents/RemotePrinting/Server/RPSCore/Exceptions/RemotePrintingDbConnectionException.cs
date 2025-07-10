using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;

namespace Enterprise.RemotePrinting.Server.RPSCore
{
	[Serializable]
	public class RemotePrintingDbConnectionException : RemotePrintingException
	{
		RemotePrintingDbConnectionException(string messageOverride, Exception innerException, int errorCode)
			: this(messageOverride, innerException)
		{
			ErrorCode = errorCode;
		}

		RemotePrintingDbConnectionException(string messageOverride, Exception innerException)
			: base(messageOverride, innerException)
		{
		}

#if NETFRAMEWORK
		protected RemotePrintingDbConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public static bool IsGeneralNetworkError(Exception ex)
		{
			SqlException sqlEx = ex as SqlException;
			return (sqlEx != null && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.GeneralNetworkError);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static RemotePrintingDbConnectionException New(Exception ex)
		{
			Argument.NotNull(ex, nameof(ex));

			int erCode = 530;
			var messageOverride = ex.Message;

			if (ex is DatabaseUpgradeException)
			{
				if (ex is DatabaseUpgradedException upgradedException)
				{
					erCode = upgradedException.IsDatabaseVersionOlder ? 535 : 536;
					WebUpgradeManager.NotifyUpgradeRequired();
				}
				else if (ex is DatabaseUpgradeInProgressException)
				{
					erCode = 537;
				}
				messageOverride += CheckUpgradeIsRunningMessage;
			}
			else if (ex is SqlException sqlException)
			{
				if (Db.IsUpgradeLockoutError(sqlException))
				{
					erCode = 531;
					messageOverride += CheckUpgradeIsRunningMessage;
				}
				else
				{
					DbErrorMatch errorHandler = new DbErrorMatch(sqlException);

					switch (errorHandler.ExceptionType)
					{
						case DbErrorType.LoginFailedForUser:
							erCode = 533;
							messageOverride += " Please check your database login credentials.";
							break;

						case DbErrorType.GeneralNetworkError:
						case DbErrorType.ServerDoesNotExist:
							erCode = 534;
							messageOverride = GeneralNetworkErrorMessage;
							break;
					}
				}
			}
			else if (ex.Source != null && ex.Source.StartsWith("System.Data", StringComparison.OrdinalIgnoreCase)
				&& ex.Message.ToLower().Contains("connection is closed"))
			{
				erCode = 534;
				messageOverride = GeneralNetworkErrorMessage;
			}

			int crPosition = messageOverride.IndexOf('\r');
			int lfPosition = messageOverride.IndexOf('\n');
			if (crPosition < 0)
			{
				crPosition = messageOverride.Length;
			}

			if (lfPosition < 0)
			{
				lfPosition = messageOverride.Length;
			}

			messageOverride =
				messageOverride.Substring(0, Math.Min(crPosition, lfPosition)).TrimEnd(new char[] { '.' })
				+ String.Format(" (DB Server: [{0}] / DB Name: [{1}])", Db.ServerName, Db.DatabaseName);

			return new RemotePrintingDbConnectionException(messageOverride, ex, erCode);
		}

		public int ErrorCode
		{
			get
			{
				return errorCode;
			}
			set
			{
				errorCode = value;
			}
		}
		int errorCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string CheckUpgradeIsRunningMessage = " Please check with your Systems Administrator if the database is being upgraded.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static readonly string GeneralNetworkErrorMessage = "Network error or database is unavailable." + CheckUpgradeIsRunningMessage;
	}
}
