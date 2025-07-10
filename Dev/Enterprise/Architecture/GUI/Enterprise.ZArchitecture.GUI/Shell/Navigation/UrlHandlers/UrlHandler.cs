using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.MasterFiles.Integration;
using Enterprise.RemoteDesktopServices;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.Res;

#if !WINZOR
#endif

namespace Enterprise.ZArchitecture
{
	public abstract class UrlHandler
	{
		#region Static

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Querystring argument name")]
		protected const string ArgsQueryStringIdentifier = "Args";
		protected const char ArgsDelimiter = '|';
		const string EscapedArgsDelimiter = "_ArgsDelimiter_";
		public static readonly string EdiUrlPrefix = Enterprise.URLHandler.EdiUrlPrefix.Value;
		internal const string WindowPersisterIdentifier = "WindowPersister";

		protected internal static string GetUrlFromQueryString(QueryString queryString)
		{
			return EdiUrlPrefix + queryString.ToString();
		}

		public static string GetQueryStringTextFromUrl(string url)
		{
			return url.Substring(UrlHandler.EdiUrlPrefix.Length);
		}

		internal protected static Guid GetBusinessEntityPk(QueryString queryString)
		{
			return GetQueryStringPk(queryString, "BusinessEntityPK");
		}

		internal protected static IEnumerable<string> GetArgs(QueryString queryString)
		{
			var argsString = queryString[ArgsQueryStringIdentifier];
			var args = argsString?.Split(ArgsDelimiter);

			return args?.Select(x => x.Replace(EscapedArgsDelimiter, ArgsDelimiter.ToString()));
		}

		protected IEnumerable<string> GetEscapedArgs(IEnumerable<string> args)
		{
			return args.Select(x => x.Replace(ArgsDelimiter.ToString(), EscapedArgsDelimiter));
		}

		protected static bool GetWindowPersisterStatus(QueryString queryString)
		{
			var val = queryString[WindowPersisterIdentifier];
			if (string.IsNullOrEmpty(val))
			{
				return false;
			}
			if (bool.TryParse(val, out var result))
			{
				return result;
			}
			return false;
		}

		protected internal static Guid GetQueryStringPk(QueryString queryString, string value)
		{
			Guid pk;
			var pkAsString = queryString[value];
			if (pkAsString == null)
			{
				pk = Guid.Empty;
			}
			else if (!Guid.TryParse(pkAsString, out pk))
			{
				ThrowInvalidUrlException();
			}
			return pk;
		}

		internal static void ThrowInvalidUrlException()
		{
			throw new EnterpriseUrlHandlerException("This is not a valid " + Constants.ProductName + " shortcut or hyperlink.");
		}

		#endregion

		#region Handle

		public bool CanHandle(QueryString queryString)
		{
			return CanHandleCore(queryString);
		}

		protected virtual bool CanHandleCore(QueryString queryString)
		{
			return queryString["Command"] == ExpectedCommandText;
		}

		protected abstract string ExpectedCommandText { get; }

		public bool Handle(QueryString queryString)
		{
			CheckLoginState(queryString);

			return HandleCore(queryString);
		}

		protected virtual void CheckLoginState(QueryString queryString)
		{
			var licenceCode = queryString["LicenceCode"];
			if (licenceCode != null)
			{
				EnsureLoggedWithCorrectProductKey(licenceCode);
			}

			EnsureUserLoggedIn();
		}

		static void EnsureUserLoggedIn()
		{
			if (!EnvProxy.Instance.IsAuthenticated)
			{
				throw new EnterpriseUrlHandlerException("User is not logged in when trying to execute the url.");
			}
		}

		protected abstract bool HandleCore(QueryString queryString);

		#endregion

		#region Security

		protected internal virtual string[] GetQueryNamesSecuredBySecurityHash(QueryString queryString)
		{
			return Array.Empty<string>();
		}

		protected internal string CreateQueryStringSecurityHash(QueryString queryString)
		{
			return SecureURLHashProvider.GetSecurityHash(queryString, GetQueryNamesSecuredBySecurityHash(queryString));
		}

		#endregion

		#region Licence

		protected void EnsureLoggedWithCorrectProductKey(ZString urlCreatedFromLicenceCode)
		{
			var licenceKeyIdentifier = CurrentCompanyLicenceKeyIdentifier;
			var currentEnterpriseCode = string.IsNullOrEmpty(licenceKeyIdentifier) ? string.Empty : licenceKeyIdentifier.Substring(0, 3);
			var currentServerCode = string.IsNullOrEmpty(licenceKeyIdentifier) ? string.Empty : licenceKeyIdentifier.Substring(6, 3);
			var urlEnterpriseCode = urlCreatedFromLicenceCode.IsEmpty ? ZString.Empty : urlCreatedFromLicenceCode.Substring(0, 3);
			var urlServerCode = urlCreatedFromLicenceCode.IsEmpty ? ZString.Empty : urlCreatedFromLicenceCode.Substring(6, 3);

			if (urlEnterpriseCode != currentEnterpriseCode || urlServerCode != currentServerCode)
			{
				throw new EnterpriseUrlHandlerException(
					Res.GetString("0050d13b-c93c-4a7d-8f63-3532454c126a", @"{0} is not running with the correct product key or from the correct licensed server installation directory.
Log into the application with enterprise code '{1}' and server code '{2}', and try again.", Constants.ProductName, urlEnterpriseCode, urlServerCode));
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
#if DEBUG
		virtual
#endif
 protected BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory() { NameForDebugging = "UrlHandler Factory" };
		}

		string CurrentCompanyLicenceKeyIdentifier
		{
			get { return GetCurrentCompanyLicenceKeyIdentifier(CurrentCompany); }
		}

#if DEBUG
		internal
#endif
 protected static string GetCurrentCompanyLicenceKeyIdentifier(IGlbCompany company)
		{
			if (company != null && !Db.Connection.DatabaseUpgradedExceptionHasBeenThrown)
			{
				return company.LicenceKeyIdentifier;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected virtual IGlbCompany CurrentCompany
		{
			get { return StaticCurrentFetcher.Instance.CurrentCompany; }
		}

		#endregion

		#region Form activation

#if !WINZOR

		#region APIs

		[DllImport("user32.dll")]
		static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		internal static class NativeMethods
		{
			[DllImport("user32.dll")]
			internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint ProcessId);
		}

		[DllImport("user32.dll")]
		static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

		#endregion

		protected void ForceFormToActivate(Form form, bool isOpenFromWindowPersister = false)
		{
			if (form != null && !form.IsDisposed)
			{
				StealFocusFromOtherProcess(form);
				ActivateForm(form, isOpenFromWindowPersister);
			}
		}

		protected void StealFocusFromOtherProcess(Form form)
		{
			// Please use it to response user input ONLY. The idea is to guarantee user operation will be responded to,
			// rather than to steal the conducting baton and turn our software into annoying malware.
#if DEBUG
			if (!Environment.Globals.IsTest)
#endif
			{
				try
				{
					var foregroundWindowHandle = GetForegroundWindow();
					var formWindowHandle = form.Handle;
					if (foregroundWindowHandle != formWindowHandle)
					{
						uint zero = 0;
						var foregroundWindowThread = UrlHandler.NativeMethods.GetWindowThreadProcessId(foregroundWindowHandle, ref zero);
						var formWindowThread = UrlHandler.NativeMethods.GetWindowThreadProcessId(formWindowHandle, ref zero);
						if (foregroundWindowThread != formWindowThread)
						{
							AttachThreadInput((IntPtr)foregroundWindowThread, (IntPtr)formWindowThread, 1);
							SetForegroundWindow(formWindowHandle);
							AttachThreadInput((IntPtr)foregroundWindowThread, (IntPtr)formWindowThread, 0);
						}
						else
						{
							SetForegroundWindow(formWindowHandle);
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("UrlHandler.StealFocusFromOtherProcess", "Exception thrown while stealing active handle from other process.", ex);
				}
			}
		}

		protected virtual Form LocateMainForm()
		{
			return ZApplication.GetOpenForms().FirstOrDefault(x => x is IMainForm);
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void ActivateForm(Form form, bool isOpenFromWindowPersister = false)
		{
			var savedTopMost = form.TopMost;
			try
			{
				form.TopMost = true;
				if (form.WindowState == FormWindowState.Minimized)
				{
					form.WindowState = FormWindowState.Normal;
				}

				if (!isOpenFromWindowPersister && !(form is IMainForm) && ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
				{
					ActivateMainForm();
				}

				Application.DoEvents();
			}
			finally
			{
				form.TopMost = savedTopMost;
			}
		}

		protected virtual void ActivateMainForm()
		{
			var mainForm = LocateMainForm();
			if (mainForm != null)
			{
				mainForm.Activate();
			}
		}

#endif

		#endregion
	}
}
