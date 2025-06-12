using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.Text.RegularExpressions;
using CargoWise.eHub.Common;
using CargoWise.eServices.Authentication.ServiceClient;
using Common.Logging;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway
{
	public class eHubUserNamePasswordValidator : UserNamePasswordValidator
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(eHubUserNamePasswordValidator));

		const int CLIENT_ID_LENGTH = 9;
		const int PASSWORD_LENGTH = 20;

		private bool isPermitted(string userName)
		{
			return PartyAccessor.IsUnrestrictedClient(userName) || userName.StartsWith("T_____");
		}

		public override void Validate(string userName, string password)
		{
			if (log.IsDebugEnabled) log.Debug(CreateLogMessage(false, userName));

			if (ServiceHelper.GetSOAPAction() == "Ping\"") return;

			try
			{
				ValidateCore(userName, password);
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

		private void ValidateCore(string userName, string password)
		{
			if (userName.Length == 9 && userName.Substring(3, 3) == "???")
			{
				ValidateEHubClient(userName, password);
			}
			else
			{
				if (isPermitted(userName))
				{
					ValidateEHubClient(userName, password);
				}
				else
				{
					if (ValidateCW1System(userName, password))
					{
						AddEHubClientIfNotExists(userName);
					}
					else
					{
						ThrowInvalidClientIDorPasswordException();
					}
				}
			}
		}

		private void ValidateEHubClient(string userName, string password)
		{
			if (!SecurityAccessor.ValidatePassword(userName, SHA512Encryptor.Encrypt(userName + password)))
			{
				ThrowInvalidClientIDorPasswordException();
			}

			if (log.IsDebugEnabled) log.Debug(CreateLogMessage(true, userName));
		}

		/// <summary>
		/// Validates CW1 system password against ediProd.LicenceDatabase.LD_Password
		/// </summary>
		/// <param name="userName">eHub client ID</param>
		/// <param name="password">eHub client password</param>
		/// <returns>true if validation was successful; false if validation could not be performed</returns>
		private bool ValidateCW1System(string userName, string password)
		{
			var result = false;

			if (userName != null && userName.Length > 9)
			{
				userName = GetUsernameFromAuthWsExtensionConfig(userName);
			}

			if (userName != null && userName.Length == 9)
			{
				var enterpriseCode = userName.Substring(0, 3);
				var serverCode = userName.Substring(6);

				try
				{
					result = AuthWSApi.ValidateSystem(enterpriseCode, serverCode, password);
				}
				catch (Exception ex)
				{
					if (log.IsErrorEnabled) log.Error(CreateLogMessage(false, userName), ex);
				}
			}

			return result;
		}

		static string GetUsernameFromAuthWsExtensionConfig(string userName)
		{
			var authWsExtensions = (System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("authWsExtensions");
			if (authWsExtensions != null)
			{
				foreach (var key in authWsExtensions.AllKeys)
				{
					var regexMatch = Regex.Match(userName, key);
					if (regexMatch.Success && regexMatch.Groups.Count > 0)
					{
						userName = regexMatch.Groups[1].Value;
						if (!string.IsNullOrEmpty(userName)) return userName;
					}
				}
			}

			return null;
		}

		static void ThrowInvalidClientIDorPasswordException()
		{
			throw new FaultException("ClientID or Password invalid.");
		}

		private void AddEHubClientIfNotExists(string clientID)
		{
			if (!PartyAccessor.ClientExists(clientID))
			{
				var authClient = GetUsernameFromAuthWsExtensionConfig(clientID);
				var sysCategory = "Enterprise";
				if (authClient != null)
				{
					sysCategory = "Third Party";
				}
				AddEHubClient(clientID, sysCategory);
			}
		}

		private void AddEHubClient(string clientID, string systemCategory)
		{
			var clientDetails = PartyAccessor.GetEdiProdClientDetailsForSystem(clientID);
			if (clientDetails == null) ThrowInvalidClientIDorPasswordException();
			PartyAccessor.InsertClientAndClientSystem(clientID, clientDetails.FullName, clientDetails.Email, clientDetails.EdiProdLink, systemCategory);
		}

		private static string CreateLogMessage(bool success, string userName)
		{
			return string.Format("Login [IP Address: {0,-15}, UserName: {1}] - {2}", ServiceHelper.GetClientIPAddress(), userName.PadRight(CLIENT_ID_LENGTH), success ? "SUCCESS" : "FAIL");
		}

		public virtual ISecurityAccessor SecurityAccessor
		{
			get { return securityAccessor ?? (securityAccessor = DataAccessFactories.NewSecurityAccessorInstance()); }
		}
		private ISecurityAccessor securityAccessor;

		public virtual IPartyAccessor PartyAccessor
		{
			get { return partyAccessor ?? (partyAccessor = DataAccessFactories.NewPartyAccessorInstance()); }
		}
		private IPartyAccessor partyAccessor;

		public virtual IAuthWebserviceApi AuthWSApi
		{
			get { return authWSApi ?? (authWSApi = new AuthWebServiceApi()); }
		}
		private IAuthWebserviceApi authWSApi;
	}
}
