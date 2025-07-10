using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
#if DEBUG
	public
#endif
	static class Win32
	{
		public const uint SHGFI_ICON = 0x100;
		public const uint SHGFI_LARGEICON = 0x0;    // Large icon
		public const uint SHGFI_SMALLICON = 0x1;    // Small icon
		public const uint SHGFI_USEFILEATTRIBUTES = 0x10;
		public const int FILE_ATTRIBUTE_NORMAL = 0x80;
		public const int ASSOCSTR_FRIENDLYDOCNAME = 3;
		public const int ASSOCSTR_FRIENDLYAPPNAME = 4;

		[DllImport("shell32.dll")]
		public static extern IntPtr SHGetFileInfo(string pszPath,
			uint dwFileAttributes,
			out SHFILEINFO psfi,
			uint cbSizeFileInfo,
			uint uFlags);

		[DllImport("shlwapi.dll")]
		public static extern int AssocQueryString(int flags,
			int str,
			string pszAssoc,
			string pszExtra,
			StringBuilder pszOut,
			ref int pcchOut
			);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int DestroyIcon(IntPtr hIcon);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SHFILEINFO
		{
			public SHFILEINFO(bool b)
			{
				hIcon = IntPtr.Zero;
				iIcon = 0;
				dwAttributes = 0;
				szDisplayName = "";
				szTypeName = "";
			}

			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szTypeName;
		}
	}

	/// <summary>
	/// see http://support.microsoft.com/kb/319350/
	/// </summary>
	public class FileAssociationRetriever
	{
		/// <summary>
		/// Retrieves the icon used by Windows Explorer for the specified File Extension
		/// </summary>
		public Bitmap GetIconForExtension(string extensionType)
		{
			extensionType = AddDotPrefix(extensionType);
			if (IsTifOrJpg(extensionType.ToUpper()))
			{
				return BrandingFactory.Instance.ProductIcon.ToBitmap();
			}
			else
			{
				Win32.SHFILEINFO shFileInfo;
				shFileInfo.hIcon = IntPtr.Zero;

				Bitmap result;
				try
				{
					Win32.SHGetFileInfo(extensionType, Win32.FILE_ATTRIBUTE_NORMAL, out shFileInfo, (uint)Marshal.SizeOf(typeof(Win32.SHFILEINFO)), Win32.SHGFI_ICON | Win32.SHGFI_USEFILEATTRIBUTES | Win32.SHGFI_LARGEICON);
					result = (shFileInfo.hIcon != IntPtr.Zero) ? Icon.FromHandle(shFileInfo.hIcon).ToBitmap() : new Bitmap(1, 1);
				}
				finally
				{
					if (shFileInfo.hIcon != IntPtr.Zero)
					{
						Win32.DestroyIcon(shFileInfo.hIcon);
					}
				}
				return result;
			}
		}

		bool IsTifOrJpg(string extension)
		{
			var extensionWithoutDotAndUpcase = extension.Replace(".", "").ToUpperInvariant();
			return SerializableEDocsTools.IsImage(extensionWithoutDotAndUpcase);
		}

		/// <summary>
		/// e.g. "Microsoft Office Word Document"
		/// </summary>
		public string GetFriendlyDocumentName(string extensionType)
		{
			string returnDocumentName = (NoResString)"File"; 
			extensionType = AddDotPrefix(extensionType);

			if (IsTifOrJpg(extensionType.ToUpper()))
			{
				returnDocumentName = Core.Constants.ProductName + " eDoc"; 
			}
			else
			{
				StringBuilder applicationName = new StringBuilder(256, 256);
				int bufferLength = applicationName.Capacity;
				int result = Win32.AssocQueryString(0, Win32.ASSOCSTR_FRIENDLYDOCNAME, extensionType, null, applicationName, ref bufferLength);

				if (result == 0)
				{
					returnDocumentName = applicationName.ToString();
				}
				else
				{
					returnDocumentName = !string.IsNullOrEmpty(extensionType) ? extensionType + (NoResString)" File" : (NoResString)"File";
				}
			}

			return returnDocumentName;
		}

		string AddDotPrefix(string extensionType)
		{
			if (!string.IsNullOrEmpty(extensionType))
			{
				return (extensionType[0] != '.') ? '.' + extensionType : extensionType;
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
