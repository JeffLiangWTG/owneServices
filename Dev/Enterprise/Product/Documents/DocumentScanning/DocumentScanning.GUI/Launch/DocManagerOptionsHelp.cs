using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Launch
{
	public class DocManagerOptionsHelp
	{
		public enum OptionsResult
		{
			opNone,
			opClose,
			opOpenAllocateDocuments
		}

		public DocManagerOptionsHelp()
		{
			fArgsHelp = new CommandLineArgsHelp();
			fDiagManager = new DiagnosticsManager();
		}

		/// <summary>
		/// Process Doc Manager options. 
		/// </summary>
		/// <param name="Args"></param>
		/// <returns></returns>
		public OptionsResult ProcessOptions(CommandLineArguments aCmdLineArguments)
		{
			OptionsResult result = OptionsResult.opClose;
			fArgsHelp.ReadInArguments(aCmdLineArguments);

			if (!fArgsHelp.IsOption((NoResString)"scanstart"))
			{
				return OptionsResult.opNone;
			}

			if (fArgsHelp.IsOption("OpenDocDiag"))
			{
				// Run Diagnostics window. 
				// This won't return until the window is closed by the user.
				DocManagerDiagnosticsForm.RunDiagnosticsWindow(fArgsHelp.MandatoryArgs());
				return OptionsResult.opClose;
			}

			fDiagManager.Initialize();
			string deviceName = "";

			if (fDiagManager.DebugMode)
			{
				AppendCommandLineInfo(fArgsHelp.CmdLineArgsAsLine());
			}

			StilmonEventType eventType = GetParams(out deviceName);

			if (eventType != StilmonEventType.None)
			{
				if (fDiagManager.DebugMode)
				{
					fDiagManager.AppendMessageLine(string.Format((NoResString)"Event : {0}", eventType));
				}

				// Params are ok.
				string statusMessage = "";

				if (eventType == StilmonEventType.ScanPressed)
				{
					ProcessesHelp.ProcessScanButton(deviceName, BrandingFactory.Instance.ProductName, out statusMessage);
					result = OptionsResult.opClose;

					if (fDiagManager.DebugMode)
					{
						fDiagManager.AppendMessageLine(statusMessage);
					}
				}
				else if (eventType == StilmonEventType.PaperLoaded)
				{
					bool alreadyRunning = ProcessesHelp.ProcessPaperLoaded(deviceName, BrandingFactory.Instance.ProductName,
						Application.StartupPath, out statusMessage);

					if (fDiagManager.DebugMode)
					{
						fDiagManager.AppendMessageLine(statusMessage);
					}

					if (alreadyRunning)
					{
						result = OptionsResult.opClose;
					}
					else
					{
						result = OptionsResult.opOpenAllocateDocuments;
					}
				}
			}

			if (fDiagManager.DebugMode)
			{
				IntPtr docManagerDiagnosticsWindow = EnsureDiagnosticWindowIsOpen();
				ProcessesHelp.SendMessageToWindow(docManagerDiagnosticsWindow, fDiagManager.TheMessage);
			}

			return result;
		}

		public StilmonEventType GetParams(out string deviceName)
		{
			deviceName = "";

			deviceName = fArgsHelp.GetOptionValue("StiDevice");
			string eventName = fArgsHelp.GetOptionValue("StiEvent");

			if (!string.IsNullOrEmpty(deviceName) && !string.IsNullOrEmpty(eventName))
			{
				return StilmonEventList.StilmonEventNameToType(eventName);
			}

			return StilmonEventType.None;
		}

		public IntPtr EnsureDiagnosticWindowIsOpen()
		{
			IntPtr docManagerDiagnosticsWindow = WinAPIGeneralWindowsUtils.FindWindow(null, (NoResString)"DocManager Diagnostics");

			if (docManagerDiagnosticsWindow == IntPtr.Zero)
			{
				string statusMessage = "";
				docManagerDiagnosticsWindow = ProcessesHelp.StartExternalDiagnosticsWindow(BrandingFactory.Instance.ProductName,
					Application.StartupPath, fArgsHelp.MandatoryArgs(), (NoResString)"DocManager Diagnostics", out statusMessage);

				if (docManagerDiagnosticsWindow == IntPtr.Zero)
				{
					throw new ApplicationException("Could not open diagnostics window : " + statusMessage);
				}
			}

			return docManagerDiagnosticsWindow;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "this time is used to display to the user only")]
		protected void AppendCommandLineInfo(string[] args)
		{
			fDiagManager.AppendMessageLine(DateTime.Now.ToString(CultureInfo.CurrentCulture)); // this time is used to display to the user only

			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0)
				{
					fDiagManager.AppendMessage(" ");
				}
				fDiagManager.AppendMessage(args[i]);
			}
			fDiagManager.AppendCRLF();
		}

		readonly CommandLineArgsHelp fArgsHelp;
		readonly DiagnosticsManager fDiagManager;
	}
}
