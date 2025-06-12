using System;
using System.Data.SqlClient;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using CargoWise.eHub.Common;
using Common.Logging;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway
{
	public class IntegrationUserNamePasswordValidator : UserNamePasswordValidator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(IntegrationUserNamePasswordValidator));

		const int CLIENT_ID_LENGTH = 9;
		const int PASSWORD_LENGTH = 20;

		public override void Validate(string userName, string password)
		{
			if (log.IsDebugEnabled) log.Debug(CreateLogMessage(false, userName));

			if (ServiceHelper.GetSOAPAction() == "Ping\"") return;

			try
			{
				if (!SecurityAccessor.ValidatePassword(userName, SHA512Encryptor.Encrypt(userName + password)))
				{
					throw new FaultException("ClientID or Password invalid.");
				}

				if (log.IsDebugEnabled) log.Debug(CreateLogMessage(true, userName));
			}
			catch (FaultException)
			{
				if (log.IsWarnEnabled) log.Warn(CreateLogMessage(false, userName));

				throw;
			}
			catch (SqlException ex)
			{
				if (log.IsErrorEnabled) log.Error(CreateLogMessage(false, userName), ex);
				SqlExceptionHandler.ThrowFaultExceptionWithSystemUnderMaintananceExceptionMessageIfApplicable(ex);
				throw;
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled) log.Error(CreateLogMessage(false, userName), ex);

				throw;
			}
		}

		private static string CreateLogMessage(bool success, string userName)
		{
			return string.Format("Login [IP Address: {0,-15}, UserName: {1}] - {2}", ServiceHelper.GetClientIPAddress(), userName.PadRight(CLIENT_ID_LENGTH), success ? "SUCCESS" : "FAIL");
		}

		public virtual ISecurityAccessor SecurityAccessor => securityAccessor ?? (securityAccessor = DataAccessFactories.NewSecurityAccessorInstance());
		private ISecurityAccessor securityAccessor;
	}
}
