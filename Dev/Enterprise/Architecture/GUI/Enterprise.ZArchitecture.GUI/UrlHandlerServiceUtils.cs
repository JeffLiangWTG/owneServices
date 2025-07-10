using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[Serializable]
	public static class UrlHandlerServiceUtils
	{
		public static QueryString GetVerifiedQueryString(string url, Func<QueryString, EnterpriseUrlHandlerException> verifyQueryString)
		{
			var queryString = new QueryString { UrlEncodeNameAndValue = true, EscapeAmpersandAndEquals = true };
			queryString.Deserialize(UrlHandler.GetQueryStringTextFromUrl(url));

			if (verifyQueryString(queryString) != null)
			{
				// IE decodes the URL before it gets to the application! If this has happened try again without url decoding.
				queryString.UrlEncodeNameAndValue = false;
				queryString.Deserialize(UrlHandler.GetQueryStringTextFromUrl(url));

				var exception = verifyQueryString(queryString);
				if (exception != null)
				{
					throw exception;
				}
			}

			return queryString;
		}

		public static string LicenceKeyIdentifier => StaticCurrentFetcher.Instance.CurrentCompany?.LicenceKeyIdentifier;

		public static string LicenceEnterpriseCode => StaticCurrentFetcher.Instance.CurrentCompany?.LicenceEnterpriseCode;

		public static string LicenceServerID => StaticCurrentFetcher.Instance.CurrentCompany?.LicenceServerID;

		public static EnterpriseUrlHandlerException VerifyLicenceKey(QueryString queryString)
		{
			Argument.NotNull(queryString, "queryString");

			var currentLicenceKey = LicenceKeyIdentifier;
			var currentEnterpriseCode = LicenceEnterpriseCode;
			var currentServerID = LicenceServerID;
			var licenceKeyProvided = queryString["LicenceCode"];

			return string.IsNullOrWhiteSpace(currentLicenceKey)
						|| string.IsNullOrWhiteSpace(licenceKeyProvided)
						|| (string.Equals(licenceKeyProvided.Substring(0, 3), currentEnterpriseCode, StringComparison.OrdinalIgnoreCase) && string.Equals(licenceKeyProvided.Substring(6, 3), currentServerID, StringComparison.OrdinalIgnoreCase))
				? null : new NotSupportedLicenceKeyException("Cannot process url for the licence key specified.");
		}

		public static EnterpriseUrlHandlerException VerifyUserOrLoginIfRequested(QueryString queryString)
		{
			Argument.NotNull(queryString, "queryString");

			var authenticationKey = queryString["UrlAuthenticationKey"];
			var result = string.IsNullOrWhiteSpace(authenticationKey);
			if (!result)
			{
				var userPk = GetAuthenticationUserKey(authenticationKey);
				if (userPk.HasValue)
				{
					var currentUser = EnvProxy.Instance.CurrentUser;
					if (currentUser != null)
					{
						result = currentUser.PK == userPk;
					}
					else
					{
						result = Login(queryString, userPk.Value);
					}
				}
			}
			return result ? null : new InvalidUrlCredentialsException("Invalid user credentials.");
		}

		public static Guid? GetAuthenticationUserKey(string authenticationKey)
		{
			var semaphore = new UrlAuthenticationSemaphore(authenticationKey);
			var handle = EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(semaphore).FirstOrDefault();
			return handle != null ? handle.OwnerSession.UserPk : new Guid?();
		}

		static Func<bool> loginAction;
		public static bool Login(QueryString queryString, Guid userPk)
		{
			bool loginSuccessful = false;
			loginAction = () =>
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "UrlAuthenticationFactory" };
				var user = (IUser)factory.Load(ObjectFactory.GetType(typeof(IGlbStaff)), userPk);
				if (user != null)
				{
					var authenticatedUser = LoginAuthenticationInfo.NewSuccessfulLogin(user);
					loginSuccessful = EnvProxy.Instance.LoginController.LoginLocation(authenticatedUser, queryString["Branch"], queryString["Department"]);
				}
				using (var completedEvent = GetUrlAuthenticationCompletedEvent())
				{
					completedEvent.Set();
				}
				return loginSuccessful;
			};

			using (var ready = GetUrlAuthenticationActionReadyEvent())
			{
				ready.Set();
			}

			using (var completedEvent = GetUrlAuthenticationCompletedEvent())
			{
				completedEvent.WaitOne(TimeSpan.FromMinutes(1));
			}

			return loginSuccessful;
		}

		public static EventWaitHandle GetUrlAuthenticationActionReadyEvent()
		{
			return new EventWaitHandle(false, EventResetMode.ManualReset, "EnterpriseUrlAuthenticationActionReadyEvent");
		}

		public static EventWaitHandle GetUrlAuthenticationCompletedEvent()
		{
			return new EventWaitHandle(false, EventResetMode.ManualReset, "EnterpriseUrlAuthenticationCompleted");
		}

		public static Func<bool> GetLoginAction()
		{
			using (var ready = GetUrlAuthenticationActionReadyEvent())
			{
				ready.WaitOne(TimeSpan.FromMinutes(1));
			}
			return loginAction;
		}
	}
}
