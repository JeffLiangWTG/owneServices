using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules
{
	/// <summary>
	/// The Url Handler remoting server object for the edient: url protocol.
	/// The edient: url protocol for ediEnterprise works as follows:
	///
	/// edient: Url Protocol Registration
	/// ---------------------------------
	/// In ediLoad, HKEY_CURRENT_USER\Software\Classes\edient\shell\open\command is registered to instruct Windows to run
	/// "C:\Program Files\Eagle Datamation International\Enterprise.exe" "%1"
	/// when a hyperlink with 'edient:xxx' is clicked.
	///
	/// On Enterprise login
	/// HKEY_CURRENT_USER\Software\Classes\edient\shell\open\command\[licence identifier] is registered with values:
	///   (default) - location of ediLoad.exe
	///   BranchCode - the 3 letter code for the last logged in branch for that licence
	/// This is to facilitate automatically spawning a new instance of Enterprise if one isn't already running.
	///
	/// Remoting Server
	/// ---------------
	/// The remoting server class EnterpriseUrlHandlerService is published by calling
	/// EnterpriseUrlHandlerService.RegisterRemotingServer() when ediEnterprise is started with Enterprise.exe.
	///
	/// Remoting Client
	/// ---------------
	/// The remoting client class EnterpriseUrlHandlerClient is invoked when 'Enterprise.exe edient:xxx' is run.
	/// This gets an instance of the remote EnterpriseUrlHandlerService class for all ediEnterprise processes and
	/// invokes them until it finds one that responds to the request nicely.
	///
	/// Spawning Enterprise when Required
	/// ---------------------------------
	/// If no running Enterprise instance could be found with a matching licence key, a new instance is spawned using
	/// the above registry key to find ediLoad.exe, passing the -Branch option to default the login branch.
	/// </summary>
	[Serializable]
	public sealed class EnterpriseUrlHandlerService : MarshalByRefObject, IEnterpriseUrlHandlerService
	{
		public static EnterpriseUrlHandlerService Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EnterpriseUrlHandlerService();
				}
				return instance;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static EnterpriseUrlHandlerService instance;

		#region Registration

		public void RegisterRemotingServer()
		{
			URLHandlerServiceRegistration.RegisterServer(Instance);
		}

		public void RegisterInstance()
		{
			URLHandlerServiceRegistration.RegisterInstance(
				LicenceKeyIdentifier,
				CurrentBranchCode,
				Db.ServerName + " " + Db.DatabaseName,
				ApplicationType.CargoWiseRDP, InstanceDetails.Current.Domain, InstanceDetails.Current.Instance, string.Empty);
		}

		string IEnterpriseUrlHandlerService.LicenceKeyIdentifier
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return LicenceKeyIdentifier;
				}
			}
		}

		static string CurrentBranchCode
		{
			get
			{
				var branch = StaticCurrentFetcher.Instance.CurrentBranch;
				return branch == null ? null : branch.GB_Code;
			}
		}

		static string LicenceKeyIdentifier
		{
			get
			{
				var company = StaticCurrentFetcher.Instance.CurrentCompany;
				return company == null ? null : company.LicenceKeyIdentifier;
			}
		}
		#endregion

		#region Handlers

		static EnterpriseUrlHandlerService()
		{
			UrlHandlersCore.Add(ShowEditFormUrlHandler.Instance);
			UrlHandlersCore.Add(ShowNewFormUrlHandler.Instance);
			UrlHandlersCore.Add(ShowViewFormUrlHandler.Instance);
			UrlHandlersCore.Add(ShowDeleteFormUrlHandler.Instance);
			UrlHandlersCore.Add(ShowModuleUrlHandler.Instance);
			UrlHandlersCore.Add(ShowStorageDocUrlHandler.Instance);
			UrlHandlersCore.Add(new WebTranslationFeedbackUrlHandler());
		}

		public static void RegisterUrlHandler(UrlHandler handler)
		{
			UrlHandlersCore.Add(handler);
		}

		public static void UnregisterUrlHandler(UrlHandler handler)
		{
			UrlHandlersCore.Remove(handler);
		}

		public static UrlHandler[] UrlHandlers
		{
			get { return UrlHandlersCore.ToArray(); }
		}

		static List<UrlHandler> UrlHandlersCore
		{
			get { return urlHandlersCore ?? (urlHandlersCore = new List<UrlHandler>()); }
		}

		[SuppressThreadStaticFieldMessage]
		static List<UrlHandler> urlHandlersCore = new List<UrlHandler>();

		#endregion

		#region Execute

		public bool ExecuteUrl(string url, bool waitForAppToStart) => ExecuteUrlCore(url, waitForAppToStart, false);

		public bool ExecuteUrlForWindowPersister(string url, bool waitForAppToStart) => ExecuteUrlCore(url, waitForAppToStart, true);

		bool ExecuteUrlCore(string url, bool waitForAppToStart, bool isOpenFromWindowPersister)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var result = false;
				try
				{
					if (url.StartsWith(UrlHandler.EdiUrlPrefix))
					{
						url = SetWindowPersisterIdentifierIfRequired(url, isOpenFromWindowPersister);
						var queryString = GetEnterpriseUrlQueryString(url);
						WaitForAppToStartIfRequired(waitForAppToStart);
						WaitForLogin();
						result = ApplicationDispatcher.Current.Invoke(() => ExecuteUrlCore(url, queryString));
					}
				}
				catch (UrlHandlerVerificationException)
				{
					return false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (ex is QueryStringException)
					{
						throw new EnterpriseUrlHandlerException(ex.Message);
					}
					else if (!(ex is EnterpriseUrlHandlerException))
					{
						ErrorReporter.ReportOnce("EnterpriseUrlHandler.ExecUrl", ex);
					}
					else
					{
						throw;
					}
				}

				return result;
			}
		}

		string SetWindowPersisterIdentifierIfRequired(string url, bool isOpenFromWindowPersister)
		{
			if (isOpenFromWindowPersister && !url.Contains(UrlHandler.WindowPersisterIdentifier))
			{
				url += $"&{UrlHandler.WindowPersisterIdentifier}={isOpenFromWindowPersister}";
			}
			return url;
		}

		internal static QueryString GetEnterpriseUrlQueryString(string url)
		{
			return UrlHandlerServiceUtils.GetVerifiedQueryString(url, s => IsNewUrl(s)
				? VerifyNewUrl(s)
				: VerifyLegacyUrl(s));

			bool IsNewUrl(QueryString url)
			{
				return !string.IsNullOrWhiteSpace(url["ServerName"]);
			}

			EnterpriseUrlHandlerException VerifyNewUrl(QueryString s)
			{
				return UrlHandlerServiceUtils.VerifyLicenceKey(s) ?? VerifyServerName(s) ?? VerifyDatabaseName(s) ?? UrlHandlerServiceUtils.VerifyUserOrLoginIfRequested(s);
			}

			EnterpriseUrlHandlerException VerifyLegacyUrl(QueryString s)
			{
				return UrlHandlerServiceUtils.VerifyLicenceKey(s) ?? VerifyInstanceName(s) ?? UrlHandlerServiceUtils.VerifyUserOrLoginIfRequested(s);
			}
		}

		static EnterpriseUrlHandlerException VerifyServerName(QueryString queryString)
		{
			Argument.NotNull(queryString, "queryString");

			var serverNameInUrl = queryString["ServerName"];

			return !string.Equals(InstanceDetails.Current?.ServerName, serverNameInUrl, StringComparison.OrdinalIgnoreCase)
				? new UrlHandlerVerificationException("Cannot process url for the serverName specified.")
				: null;
		}

		static EnterpriseUrlHandlerException VerifyDatabaseName(QueryString queryString)
		{
			Argument.NotNull(queryString, "queryString");

			var databaseNameInUrl = queryString["DatabaseName"];

			return !string.Equals(InstanceDetails.Current?.DatabaseName, databaseNameInUrl, StringComparison.OrdinalIgnoreCase)
				? new UrlHandlerVerificationException("Cannot process url for the databaseName specified.")
				: null;
		}

		static void WaitForAppToStartIfRequired(bool wait)
		{
			if (wait)
			{
				var timeout = 0;
				while (ApplicationDispatcher.Current == null)
				{
					Thread.Sleep(1000);
					if (++timeout > 60)
					{
						throw new EnterpriseUrlHandlerException(Constants.ProductName + " cannot handle your request, it may be opening or closing at this time. Try again later.");
					}
				}
			}
			else if (ApplicationDispatcher.Current == null)
			{
				throw new EnterpriseUrlHandlerException(Constants.ProductName + " cannot handle your request, it may be opening or closing at this time. Try again later.");
			}
		}

		static void WaitForLogin()
		{
			var stopwatch = Stopwatch.StartNew();
			while (ApplicationDispatcher.Current.Invoke(() => !EnvProxy.Instance.IsAuthenticated))
			{
				if (stopwatch.Elapsed > TimeSpan.FromMinutes(1))
				{
					throw new EnterpriseUrlHandlerException("User did not log in within 1 minute when trying to execute the url.");
				}
				Thread.Sleep(500);
			}
		}

		bool ExecuteUrlCore(string url, QueryString queryString)
		{
			return (from handler in UrlHandlers
					where handler.CanHandle(queryString)
					let query = UrlHandlerServiceUtils.GetVerifiedQueryString(url, s => VerifyHashIfProvided(s, handler))
					select handler.Handle(query)).FirstOrDefault();
		}

		static EnterpriseUrlHandlerException VerifyInstanceName(QueryString queryString)
		{
			Argument.NotNull(queryString, "queryString");

			var instanceNameInUrl = queryString["Instance"];

			if (instanceNameInUrl != null && InstanceDetails.Current?.Instance == null)
			{
				return null;
			}

			return instanceNameInUrl != null && InstanceDetails.Current?.Instance != instanceNameInUrl
				? new UrlHandlerVerificationException("Cannot process url for the instance specified.")
				: null;
		}

		EnterpriseUrlHandlerException VerifyHashIfProvided(QueryString queryString, UrlHandler handler)
		{
			Argument.NotNull(queryString, "queryString");
			Argument.NotNull(handler, "handler");

			var securityHashAsString = queryString["Hash"];
			var result = string.IsNullOrEmpty(securityHashAsString);

			if (!result && securityHashAsString.StartsWith("+"))
			{
				try
				{
					var securityHash = Convert.FromBase64String(securityHashAsString.Substring(1));
					var securedDataAsString = SecureURLHashProvider.CreateDataSecuredBySecurityHash(queryString, handler.GetQueryNamesSecuredBySecurityHash(queryString));
					result = CryptoProvider.IsValidSecurityHash(Encoding.ASCII.GetBytes(securedDataAsString), securityHash);
				}
				catch (FormatException)
				{
				}
			}
			return result ? null : new EnterpriseUrlHandlerException("This is not a valid " + Constants.ProductName + " shortcut or hyperlink.");
		}

		TripleDES CryptoProvider
		{
			get
			{
				if (cryptoProvider == null)
				{
					cryptoProvider = TripleDES.Create();
					cryptoProvider.InitializeForCargoWise();
				}

				return cryptoProvider;
			}
		}
		TripleDES cryptoProvider;

		#endregion

		[Serializable]
		class UrlHandlerVerificationException : EnterpriseUrlHandlerException
		{
			public UrlHandlerVerificationException(string message) : base(message)
			{
			}

#if NETFRAMEWORK
			[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
			protected UrlHandlerVerificationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif
		}
	}
}
