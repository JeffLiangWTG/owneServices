#define TRACE

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Enterprise.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// Maps and un-maps network drives
	/// </summary>
	public class DriveMapper
	{
		#region API
		[DllImport("mpr.dll")]
		static extern int WNetAddConnection2A(ref structNetResource pstNetRes, string psPassword, string psUsername, int piFlags);

		[DllImport("mpr.dll")]
		static extern int WNetCancelConnection2A(string psName, int piFlags, int pfForce);

		[StructLayout(LayoutKind.Sequential)]
		protected struct structNetResource
		{
			public int iScope;
			public int iType;
			public int iDisplayType;
			public int iUsage;
			public string sLocalName;
			public string sRemoteName;
			public string sComment;
			public string sProvider;
		}

		const int RESOURCETYPE_DISK = 0x1;

		// LookupAccountName
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool LookupAccountName(
			string lpSystemName,
			string lpAccountName,
			[MarshalAs(UnmanagedType.LPArray)] byte[] sid,
			ref uint cbSid,
			StringBuilder referencedDomainName,
			ref uint cchReferencedDomainName,
			out SID_NAME_USE peUse);

		public const int NO_ERROR = 0;    // SUCCESS
		public const int ERROR_ACCESS_DENIED = 5;    // Access to the network resource was denied.
		public const int ERROR_ALREADY_ASSIGNED = 85;   //The local device specified by lpLocalName is already connected to a network resource.
		public const int ERROR_BAD_DEVICE = 1200; // The type of local device and the type of network resource do not match.
		public const int ERROR_BAD_NET_NAME = 67;   // The value specified by lpRemoteName is not acceptable to any network resource provider. The resource name is invalid, or the named resource cannot be located.
		public const int ERROR_BAD_PROFILE = 1206; // The user profile is in an incorrect format.
		public const int ERROR_BAD_PROVIDER = 1204; // The value specified by lpProvider does not match any provider.
		public const int ERROR_BUSY = 170;  // The router or provider is busy, possibly initializing. The caller should retry.
		public const int ERROR_CANCELLED = 1223; // The attempt to make the connection was cancelled by the user through a dialog box from one of the network resource providers, or by a called resource.
		public const int ERROR_CANNOT_OPEN_PROFILE = 1205; // The system is unable to open the user profile to process persistent connections.
		public const int ERROR_DEVICE_ALREADY_REMEMBERED = 1202; // An entry for the device specified in lpLocalName is already in the user profile.
		public const int ERROR_EXTENDED_ERROR = 1208; // A network-specific error occured. Call the WNetGetLastError function to get a description of the error.
		public const int ERROR_INVALID_PASSWORD = 86;   // The specified password is invalid.
		public const int ERROR_NO_NET_OR_BAD_PATH = 1203; // A network component has not started, or the specified name could not be handled.
		public const int ERROR_NO_NETWORK = 1222; // There is no network present.

		public const int ERROR_DEVICE_IN_USE = 2404; // The device is in use by an active process and cannot be disconnected.
		public const int ERROR_NOT_CONNECTED = 2250; // The name specified by the lpName parameter is not a redirected device, or the system is not currently connected to the device specified by the parameter.
		public const int ERROR_OPEN_FILES = 2401; // There are open files, and the fForce parameter is FALSE. 

		public const int ERROR_INSUFFICIENT_BUFFER = 122;  // The data area passed to a system call is too small.

		public const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219; // 	Multiple connections to a server or shared resource by the same user, using more than one user name, are not allowed. Disconnect all previous connections to the server or shared resource and try again.
		public const int ERROR_BAD_NETPATH = 53;   // 	The network path was not found.

		public const int ERROR_LOGON_FAILURE = 1326; // 	Logon failure: unknown user name or bad password.
		public const int ERROR_BAD_USERNAME = 2202; // The specified username is invalid.
		public const int ERROR_NONE_MAPPED = 1332; // No mapping between account names and security IDs was done.

		enum SID_NAME_USE
		{
			SidTypeUser = 1,
			SidTypeGroup,
			SidTypeDomain,
			SidTypeAlias,
			SidTypeWellKnownGroup,
			SidTypeDeletedAccount,
			SidTypeInvalid,
			SidTypeUnknown,
			SidTypeComputer
		}

		#endregion

		public DriveMapper()
		{
			ClearState();
		}

		public bool SkipAccountValidation { get; set; }

		/// <summary>
		///  Map network drive
		/// </summary>
		/// <param name="psUsername">UserName</param>
		/// <param name="psPassword">Password></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Diagnostic message")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic message")]
		public bool ConnectNetworkPath(string psShareName, string psUsername, string psPassword, bool pfForce)
		{
			ClearState();
			bool result = false;
			fShareName = psShareName;
			fUserName = psUsername;
			fForce = pfForce;

			Trace.Write(string.Format("Validating Account Name {0} ... ", psUsername));
			int i = ValidateAccountName(psUsername);
			if (SkipAccountValidation || Win32ErrorHandler(i))
			{
				Trace.WriteLine("Done.");

				//if force, unmap ready for new connection
				if (pfForce)
				{
					DisconnectNetworkPath(psShareName, true);
				}

				//create struct data
				structNetResource stNetRes = new structNetResource();
				stNetRes.iScope = 2;
				stNetRes.iType = RESOURCETYPE_DISK;
				stNetRes.iDisplayType = 3;
				stNetRes.iUsage = 1;
				stNetRes.sRemoteName = fShareName;
				stNetRes.sLocalName = null;
				//prepare params
				int iFlags = 0;

				if (string.IsNullOrEmpty(psUsername))
				{
					psUsername = null;
				}

				if (string.IsNullOrEmpty(psPassword))
				{
					psPassword = null;
				}

				//call and return
				Trace.Write(string.Format("Connecting network path {0} ... ", fShareName));
				i = WNetAddConnectionWrapper(ref stNetRes, psPassword, psUsername, iFlags);

				result = Win32ErrorHandler(i);
			}

			if (result)
			{
				Trace.WriteLine("Done.");
			}
			return result;
		}

		/// <summary>
		/// Unmap network drive
		/// </summary>
		/// <param name="pfForce"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Diagnostic message")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic message")]
		public bool DisconnectNetworkPath(string psShareName, bool pfForce)
		{
			bool result = false;
			//call unmap and return
			int iFlags = 0;
			Trace.Write(string.Format("Disconnecting network path {0} ... ", ShareName));
			int i = WNetCancelConnectionWrapper(psShareName, iFlags, Convert.ToInt32(pfForce));

			result = Win32ErrorHandler(i);

			if (result)
			{
				Trace.WriteLine("Done.");
			}
			return result;
		}

		protected virtual int WNetAddConnectionWrapper(ref structNetResource pstNetRes, string psPassword, string psUsername, int piFlags)
		{
			Mode = MapperMode.Connect;
			return WNetAddConnection2A(ref pstNetRes, psPassword, psUsername, piFlags);
		}

		protected virtual int WNetCancelConnectionWrapper(string psName, int piFlags, int pfForce)
		{
			Mode = MapperMode.Disconnect;
			return WNetCancelConnection2A(psName, piFlags, pfForce);
		}

		#region MapperMode

		protected enum MapperMode
		{
			Connect,
			Disconnect
		}

		#endregion

		protected bool Win32ErrorHandler(int piErrorCode)
		{
			fReturnCode = piErrorCode;
			bool result = false;

			switch (piErrorCode)
			{
				case NO_ERROR:
					{
						result = true;
						break;
					}
				case ERROR_DEVICE_IN_USE:
				case ERROR_OPEN_FILES:
					{
						fMessage = string.Format(CultureInfo.InvariantCulture, InUseMessage, ShareName, Constants.ProductName);
						break;
					}
				case ERROR_NOT_CONNECTED:
					{
						if (Mode == MapperMode.Disconnect)
						{
							result = true;
						}
						break;
					}
				case ERROR_SESSION_CREDENTIAL_CONFLICT:
					{
						if (!Force)
						{
							fReUsing = true;
							result = true;
						}
						else
						{
							fMessage = string.Format(CultureInfo.InvariantCulture, SessionCredentialConflictMessage, ShareName, Constants.ProductName);
						}
						break;
					}
				case ERROR_BAD_NET_NAME:
				case ERROR_BAD_PROVIDER:
				case ERROR_NO_NET_OR_BAD_PATH:
				case ERROR_BAD_NETPATH:
					{
						fMessage = string.Format(CultureInfo.InvariantCulture, BadNetPathMessage, ShareName, Constants.ProductName);
						break;
					}
				case ERROR_ACCESS_DENIED:
					{
						fMessage = string.Format(CultureInfo.InvariantCulture, AccessDeniedMessage, ShareName, Constants.ProductName);
						break;
					}
				case ERROR_INVALID_PASSWORD:
					{
						fMessage = InvalidPasswordMessage;
						break;
					}
				case ERROR_LOGON_FAILURE:
					{
						fMessage = string.Format(LogonFailureMessage, ShareName);
						break;
					}
				case ERROR_BAD_PROFILE:
				case ERROR_BAD_USERNAME:
				case ERROR_CANNOT_OPEN_PROFILE:
				case ERROR_NONE_MAPPED:
					{
						fMessage = string.Format(CultureInfo.InvariantCulture, BadUserNameMessage, UserName, Constants.ProductName);
						break;
					}
				case ERROR_ALREADY_ASSIGNED:
				case ERROR_BAD_DEVICE:
				case ERROR_CANCELLED:
				case ERROR_DEVICE_ALREADY_REMEMBERED:
				case ERROR_EXTENDED_ERROR:
				case ERROR_NO_NETWORK:
				case ERROR_BUSY:
				default:
					{
						fCaughtException = new System.ComponentModel.Win32Exception(piErrorCode);
						fMessage = string.Format(CultureInfo.InvariantCulture, OtherErrorMessage, Mode == MapperMode.Connect ? Res.GetString("e8f0e61f-a0a0-4b08-a5ce-8c1d6a8ebbac", "connecting") : Res.GetString("36985623-cc8c-45d6-84d9-55940a889d60", "disconnecting"), ShareName, Constants.ProductName, fCaughtException.Message);
						break;
					}
			}
			return result;
		}

		#region Messages

		internal static string SessionCredentialConflictMessage
		{
			get { return Res.GetString("08134f77-e95b-4995-9b03-d1cd6812158b", @"Unable to connect the network path {0:G}.
Path is currently connected using different credentials.
Please disconnect the path manually, then log in to {1} again to deploy web components.
If the problem persists, please contact your system administrator."); }
		}

		internal static string BadNetPathMessage
		{
			get { return Res.GetString("a7d3baa1-2c0d-4bc3-84eb-fe84d93057b5", @"The network path {0:G} provided as the destination for web deployment is invalid.
Please check the 'Web -> Deployment Details -> Web Root Path' {1} registry setting."); }
		}

		internal static string AccessDeniedMessage
		{
			get { return Res.GetString("a7f0dafb-0553-4533-a50d-702c86ba4543", @"Access to the network path {0:G} was denied for the user specified in the {1} registry.
Please check the 'Web Admin User Login' and 'Web Admin User Password' values at 'Web -> Deployment Details'. 
For the FTP deployment client specific functionality please check 'Web Server User Name' and 'Web Server Password' located at 'CargoWise Client Extensions -> Release Builds & Upgrades -> Upgrade Package Paths'.
Ensure that suitable access rights for the specified user are granted on the destination network path."); }
		}

		internal static string InvalidPasswordMessage
		{
			get { return Res.GetString("e34eaf36-f320-43d8-aa61-0c52f45144ea", @"The password provided in the {0} registry is invalid.
Please enter the correct password into the 'Web Admin User Password' registry item located at 'Web -> Deployment Details' and try again.", Constants.ProductName); }
		}

		internal static string LogonFailureMessage
		{
			get
			{
				return Res.GetString("3A63A6A3-F78D-481F-AF6C-9522464D93C2", @"Could not connect to the Web Root Path: {0:G}
Please check the registry 'Web -> Deployment Details' has the correct Web Admin User Login and Password.
The User Login should include the domain name to ensure the correct account is used.");
			}
		}

		internal static string BadUserNameMessage
		{
			get { return Res.GetString("33ab0956-8e2a-46aa-a8d8-40981dd7149d", @"The username {0:G} specified in the {1} registry is invalid.
Please provide a valid username at 'Web -> Deployment Details -> Web Admin User Login' and try again. 
For the FTP deployment client specific functionality please check 'CargoWise Client Extensions -> Release Builds & Upgrades -> Upgrade Package Paths -> Web Server User Name'.
If the problem persists, please contact your system administrator."); }
		}

		internal static string OtherErrorMessage
		{
			get { return Res.GetString("0dd6156d-2720-4ddf-bfbe-d6fb45f63c62", @"A network error occurred while {0:G} the network path {1}.
Please check in the {2} registry the 'Web Root Path', 'Web Admin User Login' and 'Web Admin User Password' values at 'Web -> Deployment Details'.
For the FTP deployment client specific functionality please check 'Web Server User Name' and 'Web Server Password' located in 'CargoWise Client Extensions -> Release Builds & Upgrades -> Upgrade Package Paths'.
Ensure that suitable access rights for the specified user are granted on the destination network path.
The original error message is:
{3}"); }
		}

		internal static string InUseMessage
		{
			get
			{
				return Res.GetString("1a3764fb-e355-4049-b876-8a28b926080b", @"Unable to disconnect the network path {0:G} because it is in use.
Please ensure that there are no open files from this path and disconnect the path manually, then log in to {1} again to deploy web components.");
			}
		}

		#endregion

		protected void ClearState()
		{
			fShareName = "";
			fUserName = "";
			fForce = false;
			fReUsing = false;
			fReturnCode = NO_ERROR;
			fMessage = "";
			fCaughtException = null;
		}

		#region Fields

		/// <summary>
		/// Share network address to connect to.
		/// </summary>
		public string ShareName
		{
			get { return fShareName; }
		}

		string fShareName;

		/// <summary>
		/// Share network address to connect to.
		/// </summary>
		public string UserName
		{
			get { return fUserName; }
		}

		string fUserName;

		/// <summary>
		/// Option to force connection if drive is already mapped...
		/// or force disconnection if network path is not responding...
		/// </summary>
		public bool Force
		{
			get { return fForce; }
		}

		bool fForce;

		public bool ReUsing
		{
			get { return fReUsing; }
		}

		bool fReUsing;

		public int ReturnCode
		{
			get { return fReturnCode; }
		}

		int fReturnCode;

		public string Message
		{
			get { return fMessage; }
		}

		string fMessage = "";

		public Exception CaughtException
		{
			get { return fCaughtException; }
		}

		Exception fCaughtException;

		protected MapperMode Mode
		{
			get { return fMode; }
			set { fMode = value; }
		}

		MapperMode fMode;

		#endregion

		int ValidateAccountName(string accountName)
		{
			int result = NO_ERROR;
			byte[] sid = null;
			uint cbSid = 0;
			StringBuilder referencedDomainName = new StringBuilder();
			uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
			SID_NAME_USE sidUse;

			if (!LookupAccountName(null, accountName, sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out sidUse))
			{
				result = Marshal.GetLastWin32Error();
				if (result == ERROR_INSUFFICIENT_BUFFER)
				{
					sid = new byte[cbSid];
					referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
					result = NO_ERROR;
					if (!LookupAccountName(null, accountName, sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out sidUse))
					{
						result = Marshal.GetLastWin32Error();
					}
				}
			}

			return result;
		}
	}
}
