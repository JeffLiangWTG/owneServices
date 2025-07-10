using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Microsoft.Win32;

#pragma warning disable IDE0005 //Needed for .Net 4.8 Framework build
using System.Runtime.Serialization;
#pragma warning restore IDE0005

namespace Enterprise.DocumentScanning.OCR
{
	/// <summary>
	/// Converts a TIFF file to text using MS Office Document Imaging's OCR
	/// </summary>
	public class TiffToText
	{
		bool fUseExistingTextInFile = true;

		public bool UseExistingTextInFile
		{
			get { return fUseExistingTextInFile; }
			set { fUseExistingTextInFile = value; }
		}

		[SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public bool IsMODIXPAvailable()
		{
			Type mODIAvailable = Type.GetTypeFromProgID("MSPaper.Document", false);
			if (mODIAvailable != null)
			{
				return (mODIAvailable.GUID == new Guid("F086132E-222E-410A-BED7-343FF4D963A7"));
			}
			else
			{
				return false;
			}
		}

		[SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public bool IsMODI2003Available()
		{
			Type mODIAvailable = Type.GetTypeFromProgID("MSPaper.Document", false);
			if (mODIAvailable != null)
			{
				// GUID is for MODI in office 2003
				return (mODIAvailable.GUID == new Guid("E1D05D9E-AEB3-49A4-A95E-1663676A507E"));
			}
			else
			{
				return false;
			}
		}

		public string Convert(string filename)
		{
			return ConvertUsingMODIXP(filename);
		}

		/// <summary>
		/// Ye Olde Way Of Converting Tiffs.
		/// Used on Microsoft Office Document Imaging for XP (which has no API);
		/// must manually scan Tiff file for text.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, launching conversion program, not opening a file or url")]
		string ConvertUsingMODIXP(string filename)
		{
			ProcessStartInfo mspviewInfo = new ProcessStartInfo();
			Process mspviewProc;
			mspviewInfo.UseShellExecute = false;
			mspviewInfo.FileName = MspviewFullPath();
			mspviewInfo.CreateNoWindow = true;
			mspviewInfo.ErrorDialog = false;
			mspviewInfo.Arguments = UseExistingTextInFile ? (NoResString)"-o " : (NoResString)"-f ";
			mspviewInfo.Arguments = mspviewInfo.Arguments + "\"" + filename + "\"";

			//////////////////////////////////////////////////////////////////////////////////////////////
			// MSPVIEW sends Windows mouse messages to itself when running, even when run in
			// "command-line" mode. Because it doesn't have a visible window in command-line mode,
			// those messages get sent to whatever window is under the mouse pointer when MSPVIEW
			// is run.
			//
			// We need to ensure that:
			// * the window under the mouse pointer will process the messages (otherwise MSPVIEW
			//   hangs)
			//
			// * the window under the mouse pointer will not do unpredictable things when sent mouse
			//   messages
			//
			// * these conditions are fulfilled even if the user moves the mouse while the OCR is running
			//////////////////////////////////////////////////////////////////////////////////////////////

			// The current solution to this problem is to create a 1 x 1 pixel form and constrain the mouse
			// cursor to this 1 x 1 pixel area.
			using (CursorConstrainingForm dummyForm = new CursorConstrainingForm())
			{
				dummyForm.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(Cursor.Position.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Cursor.Position.Y));
				dummyForm.Show();
				try
				{
					mspviewProc = Process.Start(mspviewInfo);

					using (mspviewProc)
					{
						Application.DoEvents();

						double timeoutMax = 25.0D;

						if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
						{
							timeoutMax = 60.0D;
						}

						double seconds = 0;
						while (!mspviewProc.WaitForExit(100))
						{
							Application.DoEvents();

							seconds += 0.1;
							if (seconds > timeoutMax)   // Taken too long
							{
								mspviewProc.Kill();
							}
						}

						if (mspviewProc.ExitCode == OCRError.Success
							|| (UseExistingTextInFile && mspviewProc.ExitCode == OCRError.AlreadyHasOCR))
						{
							OCRTiff returnTiff = new OCRTiff(filename);
							return returnTiff.Text();
						}
						else
						{
							throw new OCRError(mspviewProc.ExitCode);
						}
					}
				}
				finally
				{
					dummyForm.Close();
				}
			}
		}

		/// <summary>
		/// Checks if MS Office Document Imaging is "available", i.e. installed correctly on the local computer.
		/// If it is not installed, or not installed properly, or installed to run from a network, we do not consider it "available".
		/// </summary>
		public bool IsOCRAvailable()
		{
			return IsMODIXPAvailable();

			// TEMPORARY - the current code for examining text from documents is not working
			// for MODI 2003. The programmatic interface is also causing problems because
			// it doesn't release the file it has open once it finishes scanning. 
			// So, we will not let anyone do OCR if they don't have MODI XP.

			// NB ProgID is for MS Document Imaging Application
			// return (Type.GetTypeFromProgID("MSPaper.Document",false) != null);
		}

		protected virtual string MspviewFullPath()
		{
			//RegistryKey mspview = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\MSPVIEW.EXE");
			RegistryKey mspview = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"MSPaper.Document\shell\Open\command");
			if (mspview != null)
			{
				string retval = (string)mspview.GetValue("");
				if (!retval.EndsWith((NoResString)".EXE\""))
				{
					retval = retval.Substring(0, retval.IndexOf(".EXE") + 6); // allow for \"
				}
				return retval;
			}
			else
			{
				throw new DocumentImagingNotAvailableException("MSPVIEW not installed on this machine. To install (you need office XP or above), go to Control Panel --> Add Remove Programs --> Microsoft office. Click 'Change'. Click on 'add/remove'. Expand the 'Office Tools' node and then enable 'Microsoft Office Document Imaging'. You will need to run the program once manually to set the initials for the user on this machine.");
			}
		}
	}

	[Serializable]
	public class DocumentImagingNotAvailableException : ApplicationException
	{
		public DocumentImagingNotAvailableException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected DocumentImagingNotAvailableException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class OCRError : ApplicationException
	{
		public const int Timeout = -1; // Set by Kill() if the process is killed
		public const int Success = 0; // Defined by MSPView
		public const int AlreadyHasOCR = 1; // Defined by MSPView

		public OCRError(int exitCode)
			: base(FriendlyMessage(exitCode))
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected OCRError(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif

		static string FriendlyMessage(int errorCode)
		{
			// Strings used in diagnostic tool
			#region SuppressResourceStringsCheckRegion

			switch (errorCode)
			{
				// Our error codes
				case Timeout:
					return @"Timed out.";
				// MSPVIEW error codes (See mk:@MSITStore:C:\Program%20Files\Microsoft%20Office\Office10\1033\msphelp.chm::/html/epCommandLine.htm)
				case 0:
					return @"Command successful.";
				case 1:
					return @"File already has OCR information.";
				case 3:
					return @"Could not locate application.";
				case 4606:
					return @"Unable to open file. Check file name and permissions.";
				case 4610:
					return @"The file is not a valid Microsoft Office Document Imaging file.";
				case 4645:
					return @"Microsoft Office Document Imaging does not support this type of TIFF file.";
				case 4648:
					return @"Unable to save file. Check disk space and permissions.";
				case 4654:
					return @"Cannot recognize text in this document. OCR was not successful.";
				case 4655:
					return @"Cannot perform this operation due to system resource constraints.";
				case 4657:
					return @"There is not enough memory to complete this operation.";
				case 4661:
					return @"You do not have sufficient permissions to open or save this file.";
				case 4662:
					return @"Unable to read file due to disk or network error.";
				case 4663:
					return @"Not enough temporary disk space to complete the operation.";
				case 4665:
					return @"A component of the application is missing. Please reinstall the application.";
				case 4673:
					return @"There are too many instances of Microsoft Office Document Imaging running. Please close one or more and then retry this operation.";
				case 4678:
					return @"Unable to complete operation. Check access permissions on the file.";
				case 4679:
					return @"Unable to complete operation. If this is a network file, network connection may have failed.";
				case 4681:
					return @"This file is read-only.";
				case 4682:
					return @"Unable to complete operation. The file is locked for exclusive use by another user. Please try again later.";
				case 4683:
					return @"Unable to copy file to local drive. Check disk space and permissions.";
				case 4684:
					return @"Unable to open document. If this is a Web document, check your connection and try again.";
				case 4685:
					return @"File already has OCR text information.";
				case 4692:
					return @"Unable to save <filename>. This file is in use by another user.";
				case 4699:
					return @"Microsoft Office Document Imaging requires the system locale and the application language to be the same.";
				case 4700:
					return @"Error completing operation. Could not create temporary file.";
				case 4701:
					return @"This operation has been canceled due to restrictions in effect on this computer. Please contact your system administrator.";
				case 9009:
					return @"Bad command line argument.";
				default:
					return @"Unrecognised error code.";
			}
			#endregion
		}
	}
}
