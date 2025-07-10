using System;
using CargoWise.Data;
using Enterprise.Accounting.Web.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Exceptions
{
	[Serializable]
	public class AccountingWebDbConnectionException : AccountingWebException
	{
		AccountingWebDbConnectionException(string messageOverride, Exception innerException, int errorCode)
			: this(messageOverride, innerException)
		{
			ErrorCode = errorCode;
		}

		AccountingWebDbConnectionException(string messageOverride, Exception innerException)
			: base(messageOverride, innerException)
		{
		}

#if NETFRAMEWORK
		protected AccountingWebDbConnectionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public static bool IsGeneralNetworkError(Exception ex)
		{
			SqlException sqlEx = ex as SqlException;
			return (sqlEx != null && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.GeneralNetworkError);
		}

		public static AccountingWebDbConnectionException New(Exception ex)
		{
			int erCode = 530;
			string messageOverride = ex.Message;

			if (ex is DatabaseUpgradeException)
			{
				erCode = 531;
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
							messageOverride += (NoResString)" Please check your database login credentials.";
							break;

						case DbErrorType.GeneralNetworkError:
						case DbErrorType.ServerDoesNotExist:
							erCode = 534;
							messageOverride = GeneralNetworkErrorMessage;
							break;
					}
				}
			}
			else if (ex.Source.StartsWith("System.Data", StringComparison.OrdinalIgnoreCase)
				&& ex.Message.ToLower().Contains((NoResString)"connection is closed"))
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
				+ String.Format((NoResString)" (DB Server: [{0}] / DB Name: [{1}])", WebConfigManager.EnterpriseDbServer, WebConfigManager.EnterpriseDbName);

			return new AccountingWebDbConnectionException(messageOverride, ex, erCode);
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant string in web accounting")]
		const string CheckUpgradeIsRunningMessage = " Please check with your Systems Administrator if the database is being upgraded.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant string in web accounting")]
		const string GeneralNetworkErrorMessage = "Network error or database is unavailable." + CheckUpgradeIsRunningMessage;
	}
}
