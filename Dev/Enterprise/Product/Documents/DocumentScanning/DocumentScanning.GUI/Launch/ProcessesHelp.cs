using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.DocumentScanning.GUI.Res;

namespace Enterprise.DocumentScanning.Launch
{
	public static class ProcessesHelp
	{
		// Returns true if process with same name was already running, and was notified.
		public static void ProcessScanButton(string deviceName, string targetApplicationName, out string statusMessage)
		{
			statusMessage = "";
			IntPtr foundWindow;
			string windowTitle = Constants.AllocateDocumentsFormName;

			Process foundProcess = GetAlreadyRunningDocumentScanningProcess(targetApplicationName, windowTitle, out foundWindow);

			if (foundProcess != null)
			{
				if (foundWindow != IntPtr.Zero)
				{
					SendMessageToWindow(foundWindow, "SCAN");
					statusMessage = (NoResString)"Message : SCAN sent";
				}
			}
			else
			{
				statusMessage = targetApplicationName + (NoResString)" not running.";
			}
		}

		public const string MagicEnterpriseParam = " -IAmDoingTheWrongThingByRunningEnterpriseWithoutLoader";

		/// <summary>
		/// Returns true if application window is already open. 
		/// </summary>
		/// <param name="deviceName"></param>
		/// <param name="targetApplicationName"></param>
		/// <param name="targetApplicationDir"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static bool ProcessPaperLoaded(string deviceName,
			string targetApplicationName, string targetApplicationDir, out string statusMessage)
		{
			statusMessage = "";
			IntPtr foundWindow;

			Process foundProcess = GetAlreadyRunningDocumentScanningProcess(targetApplicationName, BrandingFactory.Instance.ProductName, out foundWindow);

			if (foundProcess != null)
			{
				statusMessage = string.Format((NoResString)"{0} Main Window Found.", BrandingFactory.Instance.ProductName);

				// Bring the Allocate Document window to the front.
				//WinAPIGeneralWindowsUtils.BringWindowToTop(FoundWindow); 

				/*
				WinAPIGeneralWindowsUtils.SetForegroundWindow(FoundWindow); 
				WinAPIGeneralWindowsUtils.OpenIcon(FoundWindow); 
				*/

				//WinAPIGeneralWindowsUtils.ShowWindow(FoundWindow, WinAPIGeneralWindowsUtils.SW_Flags.ShowNormal); 

				string msgValue = "OpenDocAlloc";
				SendMessageToWindow(foundWindow, msgValue);
				statusMessage += (NoResString)" Message : " + msgValue;

				return true;
			}
			else
			{
				return false;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, launching Enterprise not opening a file or url")]
		public static IntPtr StartExternalDiagnosticsWindow(string targetApplicationName, string targetApplicationDir, StringCollection passThroughArgs,
			string windowTitleText, out string statusMessage)
		{
			statusMessage = "";

			IntPtr result = IntPtr.Zero;
			string fullPath = Path.Combine(targetApplicationDir, targetApplicationName + ".exe");

			if (!File.Exists(fullPath))
			{
				statusMessage = Res.GetString("dc9491ee-82d1-4ca2-b8d4-084f31b5c675", "Could not start : {0}", fullPath);
				return result;
			}

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < passThroughArgs.Count; i++)
			{
				sb.Append(" ");
				sb.Append(passThroughArgs[i]);
			}

			sb.Append(MagicEnterpriseParam);
			sb.Append((NoResString)" -scanstart");
			sb.Append(" -OpenDocDiag");
			Process newProcess = Process.Start(fullPath, sb.ToString());
			bool isReadyForAction = newProcess.WaitForInputIdle(10000);
			if (!isReadyForAction)
			{
				statusMessage = Res.GetString("f2bea92b-42e3-4464-95cf-aca1491d6e09", "Timed out : could not open diagnostics window");
				return result;
			}

			IntPtr foundWindow = WinAPIGeneralWindowsUtils.FindWindow(null, windowTitleText);

			if (foundWindow == IntPtr.Zero)
			{
				statusMessage = Res.GetString("c2c4b191-48b0-49b5-b362-499126f760fd", "Could not find diagnostics window");
				return result;
			}

			return foundWindow;
		}

		public static Process GetAlreadyRunningDocumentScanningProcess(string targetApplicationName, string windowTitleText, out IntPtr foundDocScanWindow)
		{
			foundDocScanWindow = IntPtr.Zero;
			IntPtr foundWindow = WinAPIGeneralWindowsUtils.FindWindow(null, windowTitleText);

			if (foundWindow != IntPtr.Zero)
			{
				int foundWindowProcessID = WinAPIGeneralWindowsUtils.GetWindowProcessID(foundWindow);
				Process curProcess = Process.GetProcessById(foundWindowProcessID);

				// For some reason, calling Process.ProcessName is really slow. So comment it out for now.
				if (curProcess != null) // && (string.Compare(CurProcess.ProcessName, TargetApplicationName, true)==0))
				{
					foundDocScanWindow = foundWindow;
					return curProcess;
				}
			}

			return null;
		}

		internal class SafeNativeMethods
		{
			[DllImport("user32.dll")]
			// IntPtr because we never care about return value. 
			public static extern IntPtr SendMessage(
				IntPtr hWnd, // handle to destination window
				UInt32 Msg, // message
				UIntPtr wParam, // first message parameter
				ref COPYDATASTRUCT ds
				// Int32 lParam // second message parameter
				);
		}
		const int WM_USER = 0x0400;
		public const int MSG_COMMAND_SCAN = WM_USER + 200;

		[StructLayout(LayoutKind.Sequential)]
		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")] // never comparing the values of the struct
		public struct COPYDATASTRUCT
		{
			internal IntPtr dwData;
			internal IntPtr cbData;
			[MarshalAs(UnmanagedType.LPStr)]
			public string lpData;
		}

		public const int WM_COPYDATA = 0x004A;

		public const int CopyDataFunction = 100;

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "CopyDataFunction requires a redundant cast")]
		public static void SendMessageToWindow(IntPtr winHandle, string messageString)
		{
			if (winHandle != IntPtr.Zero)
			{
				COPYDATASTRUCT cds;
				cds.dwData = (IntPtr)CopyDataFunction;
				cds.lpData = messageString;
				cds.cbData = (IntPtr)cds.lpData.Length + 1;
				ProcessesHelp.SafeNativeMethods.SendMessage(winHandle, WM_COPYDATA, UIntPtr.Zero, ref cds);
			}
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "CopyDataFunction requires a redundant cast")]
		public static void SendMessageToMainWindowOfProcess(int processID, string messageString)
		{
			Process destProcess = Process.GetProcessById(processID);

			if (destProcess != null)
			{
				COPYDATASTRUCT cds;
				cds.dwData = (IntPtr)CopyDataFunction;
				cds.lpData = messageString;
				cds.cbData = (IntPtr)cds.lpData.Length + 1;
				ProcessesHelp.SafeNativeMethods.SendMessage(destProcess.MainWindowHandle, WM_COPYDATA, UIntPtr.Zero, ref cds);
			}
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "CopyDataFunction requires a redundant cast")]
		public static bool IsScannerMessage(ref Message m, out string messageString)
		{
			messageString = "";

			if (m.Msg == WM_COPYDATA)
			{
#pragma warning disable WFDEV001 // 'Message.LParam' is obsolete: 'Casting to/from IntPtr is unsafe, use LParamInternal.'
				COPYDATASTRUCT foundData = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));
#pragma warning restore WFDEV001
				if (foundData.dwData == (IntPtr)CopyDataFunction)
				{
					messageString = foundData.lpData;
					return true;
				}
			}
			return false;
		}
	}
}
