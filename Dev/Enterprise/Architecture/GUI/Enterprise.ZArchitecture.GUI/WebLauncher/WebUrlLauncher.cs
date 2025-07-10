using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.WebLauncher;
using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "This is the implemenation of the alternative to Process.Start")]
	public static class WebUrlLauncher
	{
		[ThreadSafe]
		static IWebUrlLaunchValidator webUrlLaunchValidator = new WebUrlLaunchValidator();

		[ThreadSafe]
		static readonly IWebUrlEdientParser webUrlEdientParser = new WebUrlEdientParser();

		public static void Launch(string url)
		{
			bool launched = false;

			try
			{
				webUrlLaunchValidator.ValidateUrl(url);
			}
			catch (WebUrlValidationException)
			{
				return;
			}

			if (webUrlEdientParser.TryParse(url, out string result))
			{
				url = result;
			}

			// Shortcut and attempt to launch the URL against the current Enterprise application.
			if (url.StartsWith(UrlHandler.EdiUrlPrefix))
			{
				try
				{
					launched = EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				}
				catch
				{
					// Don't care why it failed, it did so we will use fallback/original method.
					launched = false;
				}
			}

			if (!launched)
			{
				var mode = DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode;
				switch (mode)
				{
					case RemoteConnectingModes.ServerOnly:
						OpenUrlOnTerminalServer();
						return;
					case RemoteConnectingModes.ConnectorOnly:
						if (EnterpriseChannel.Instance is null)
						{
							Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
							return;
						}

						if (!EnterpriseChannel.Instance.IsConnected)
						{
							Globals.Message.ShowError(UnableToLaunchUrlOnClientSideError);
							return;
						}

						OpenUrlOnClientSide();
						return;
					case RemoteConnectingModes.ConnectorOrServer:
						if (EnterpriseChannel.Instance != null && EnterpriseChannel.Instance.IsConnected)
						{
							OpenUrlOnClientSide();
							return;
						}

						OpenUrlOnTerminalServer();
						return;
					default:
						throw new NotSupportedException($"Unknown RemoteAppAllowEDocAccessWithoutConnectorMode [{mode}]");
				}
			}

			void OpenUrlOnTerminalServer()
			{
				try
				{
					if (!Globals.IsTest)
					{
						Process.Start(url); // This is the implementation of the alternative to Process.Start
					}

					SetLastUrlLaunchedForTest(url);
				}
				catch (Exception e) when (e is InvalidOperationException || e is FileNotFoundException || e is Win32Exception)
				{
					try
					{
						LaunchBrowser(url);
						SetLastUrlLaunchedForTest(url);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(Res.GetString("248c84f2-6085-4b08-8036-b3c47c432740", "The link could not be opened.") + System.Environment.NewLine + System.Environment.NewLine +
							url + System.Environment.NewLine + System.Environment.NewLine +
							Res.GetString("6ae32acb-411d-451a-8bec-6dba1390963e", "Please verify that:") + System.Environment.NewLine +
							Res.GetString("5c5dad1f-68dc-4d61-87c5-352ef254330e", "• You have a web browser installed on this system") + System.Environment.NewLine +
							Res.GetString("a0a53358-8e8b-43f5-848d-289cc5f02229", "• Your Internet connection is working"),
							Res.GetString("7dc2705a-d0e0-4111-b14a-a4abd093d229", "Error Opening URL"));
					}
				}
			}

			void OpenUrlOnClientSide()
			{
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.WebUrl, Encoding.UTF8.GetBytes(url));
				SetLastUrlLaunchedForTest(url);
			}
		}

		internal static string UnableToLaunchUrlOnClientSideError => Res.GetString(
			"375A296B-3156-42AA-B3B9-DC32807CDB41",
			"Unable to launch URL, please wait a few minutes and try again. If issue persists, restart the application.");

		public static void Launch(string url, string docType, string businessObjectPK, string parentType)
		{
			var messageToSend = docType + businessObjectPK + parentType + url;
			EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.SendOutlookEmail, Encoding.UTF8.GetBytes(messageToSend));
		}

		public static bool IsRemote => EnterpriseChannel.Instance != null && EnterpriseChannel.Instance.IsConnected;

		static void LaunchBrowser(string url)
		{
			using (var httpKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command"))
			{
				var cmd = httpKey.GetValue(string.Empty) as string;
				string[] splitStr;
				string fileName;
				string args;
				if (cmd.Substring(0, 1) == "\"")
				{
					splitStr = cmd.Split(new string[] { "\" " }, StringSplitOptions.None);
					fileName = splitStr[0] + "\"";
					args = cmd.Substring(splitStr[0].Length + 2);
				}
				else
				{
					splitStr = cmd.Split(new string[] { " " }, StringSplitOptions.None);
					fileName = splitStr[0];
					args = cmd.Substring(splitStr[0].Length + 1);
				}

				if (!Globals.IsTest)
				{
					Process.Start(fileName, args.Replace("%1", url)); // This is the implemenation of the alternative to Process.Start
				}
			}
		}

		[Conditional("DEBUG")]
		static void SetLastUrlLaunchedForTest(string url)
		{
#if DEBUG
			LastUrlLaunched = url;
#endif
		}

#if DEBUG
		public static IDisposable SetTemporaryWebUrlLaunchValidatorForTest(IWebUrlLaunchValidator tempLaunchValidator)
		{
			IWebUrlLaunchValidator prevtempLaunchValidator = webUrlLaunchValidator;
			webUrlLaunchValidator = tempLaunchValidator;
			return new DisposableAction(delegate
			{
				webUrlLaunchValidator = prevtempLaunchValidator;
			});
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static string LastUrlLaunched { get; private set; }

		public static void ClearLastUrlLaunched()
		{
			LastUrlLaunched = string.Empty;
		}
#endif
	}
}
