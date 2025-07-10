using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Enterprise.RemotePrinting.Client
{
	[Flags]
	enum RegSam
	{
		/// <summary>
		/// Combines the STANDARD_RIGHTS_REQUIRED, KEY_QUERY_VALUE, KEY_SET_VALUE, KEY_CREATE_SUB_KEY, KEY_ENUMERATE_SUB_KEYS, KEY_NOTIFY, and KEY_CREATE_LINK access rights.
		/// </summary>
		KEY_ALL_ACCESS = 0xF003F,
		/// <summary>
		///  Reserved for system use.
		/// </summary>
		KEY_CREATE_LINK = 0x0020,
		/// <summary>
		/// Required to create a subkey of a registry key.
		/// </summary>
		KEY_CREATE_SUB_KEY = 0x0004,
		/// <summary>
		/// Required to enumerate the subkeys of a registry key. 
		/// </summary>
		KEY_ENUMERATE_SUB_KEYS = 0x0008,
		/// <summary>
		/// Equivalent to KEY_READ. 
		/// </summary>
		KEY_EXECUTE = 0x20019,
		/// <summary>
		///  Required to request change notifications for a registry key or for subkeys of a registry key.
		/// </summary>
		KEY_NOTIFY = 0x0010,
		/// <summary>
		/// Required to query the values of a registry key.
		/// </summary>
		KEY_QUERY_VALUE = 0x0001,
		/// <summary>
		/// Combines the STANDARD_RIGHTS_READ, KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS, and KEY_NOTIFY values.
		/// </summary>
		KEY_READ = 0x20019,
		/// <summary>
		/// Required to create, delete, or set a registry value.
		/// </summary>
		KEY_SET_VALUE = 0x0002,
		/// <summary>
		///	Indicates that an application on 64-bit Windows should operate on the 32-bit registry view. For more information, see Accessing an Alternate Registry View.
		/// </summary>
		KEY_WOW64_32KEY = 0x0200,
		/// <summary>
		///	Indicates that an application on 64-bit Windows should operate on the 64-bit registry view. For more information, see Accessing an Alternate Registry View. 
		/// </summary>
		KEY_WOW64_64KEY = 0x0100,
		/// <summary>
		/// Combines the STANDARD_RIGHTS_WRITE, KEY_SET_VALUE, and KEY_CREATE_SUB_KEY access rights.
		/// </summary>
		KEY_WRITE = 0x20006,
	}

	enum WinError
	{
		/// <summary>
		/// The operation completed successfully.
		/// </summary>
		ERROR_SUCCESS = 0,
		/// <summary>
		///	More data is available.
		/// </summary>
		ERROR_MORE_DATA = 234,
	}

	static class Registry64
	{
		public static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
		public static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
		public static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
		public static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
		public static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(unchecked((int)0x80000004));
		public static readonly IntPtr HKEY_PERFORMANCE_TEXT = new IntPtr(unchecked((int)0x80000050));
		public static readonly IntPtr HKEY_PERFORMANCE_NLSTEXT = new IntPtr(unchecked((int)0x80000060));
		public static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));
		public static readonly IntPtr HKEY_DYN_DATA = new IntPtr(unchecked((int)0x80000006));

		[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
		public static extern WinError RegOpenKeyEx(
			IntPtr hKey,
			string subKey,
			uint options,
			[MarshalAs(UnmanagedType.U4)]
			RegSam sam,
			out IntPtr phkResult
		);

		[DllImport("advapi32.dll")]
		public static extern WinError RegCloseKey(
			IntPtr hKey
		);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto)]
		public static extern WinError RegQueryValueEx(
			IntPtr hKey,
			string valueName,
			IntPtr lpReserved,
			out uint type,
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)]
			byte[] data,
			ref uint cbData
		);

		public static string RegQueryString(IntPtr key, string valueName)
		{
			uint type;
			uint cbData = 0;
			if (RegQueryValueEx(key, valueName, IntPtr.Zero, out type, Array.Empty<byte>(), ref cbData) == WinError.ERROR_MORE_DATA)
			{
				byte[] data = new byte[cbData];
				if (RegQueryValueEx(key, valueName, IntPtr.Zero, out type, data, ref cbData) == WinError.ERROR_SUCCESS)
				{
					return Encoding.Unicode.GetString(data).TrimEnd('\0');
				}
			}

			return null;
		}
	}
}
