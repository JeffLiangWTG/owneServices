using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "This is the implemenation of the alternative to Process.Start")]
	public class ProgramLauncher : IProgramLauncher
	{
		public void Launch(string filepath, string arguments)
		{
			if (IsRemoteHandled)
			{
				var message = new RunProgramMessage(filepath, arguments);
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.RunProgram, message);
			}
			else if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}
			else
			{
				try
				{
					Process.Start(filepath, arguments); // This is the implemenation of the alternative to Process.Start
				}
				catch (Exception e) when (e is InvalidOperationException || e is FileNotFoundException || e is Win32Exception)
				{
					Globals.Message.ShowError(Res.GetString("252cd3fc-2d4c-4ebe-99b8-828209bb7c0f", "The system could not call the following program. Please check it is valid:\r\n{0}", filepath + " " + arguments));
				}
			}
		}

		bool IsRemoteHandled
		{
			get { return ObjectFactory.Get<TerminalService>().IsWTSSession && Array.IndexOf(InitializationMessageHandler.RegisteredRemoteMessageTypes, EnterpriseChannelMessageTypes.RunProgram) > -1; }
		}
	}
}
