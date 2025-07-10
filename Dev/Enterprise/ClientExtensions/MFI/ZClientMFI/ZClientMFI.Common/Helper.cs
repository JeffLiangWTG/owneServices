using System;
using System.IO;
using CargoWise.Common;

namespace ZClientMFI.Common
{
	public static class Helper
	{
		public static string AccessToFolderErrorMessage(string directory)
		{
			var message = string.Empty;
			var userName = string.Empty;

			try
			{
				userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			{
				userName = "(*COULD NOT RETRIEVE CURRENT USER NAME: *)";
			}

			try
			{
				var ds = FileSystemAclExtensions.GetAccessControl(new DirectoryInfo(directory));
			}
			catch (ArgumentNullException)
			{
				message = "The directory path is null.";
			}
			catch (IOException ex)
			{
				message = "An I/O error occurred while opening the directory.\r\n" + ex.ToString();
			}
			catch (PlatformNotSupportedException)
			{
				message = "Platform is not supported. The current operating system is not Windows 2000 or later.";
			}
			catch (UnauthorizedAccessException ex)
			{
				message = @"The path parameter specified a directory that is read-only.
-or-
This operation is not supported on the current platform.
-or-
The caller does not have the required permission.\r\n" + ex.ToString();
			}
			catch (SystemException ex)
			{
				message = "The directory could not be found.\r\n" + ex.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				message = "Unhandled exception.\r\n" + ex.ToString();
			}

			message = string.Format("Could not access folder '{0}' with user '{1}': {2}", directory, userName, message);
			return message;
		}
	}
}
