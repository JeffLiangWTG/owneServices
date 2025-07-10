using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.Common;

namespace CargoWise.IO
{
	public static class FileSystemAccessControlUtils
	{
		internal static readonly Overridable<WindowsPrincipal> CurrentPrincipal = new Overridable<WindowsPrincipal>(new WindowsPrincipal(WindowsIdentity.GetCurrent()));

		public static bool IsDirectoryWritable(string directory, out Exception principalIsInRoleException)
		{
			principalIsInRoleException = null;

			var directoryInfo = new DirectoryInfo(directory);
			var collection = directoryInfo.GetAccessControl().GetAccessRules(true, true, typeof(NTAccount));
			var currentIdentity = WindowsIdentity.GetCurrent().User.Translate(typeof(NTAccount)).Value.ToUpper();

			var currentUserSecurity = collection.OfType<FileSystemAccessRule>().Where(rule => rule.IdentityReference.Value.ToUpper() == currentIdentity);
			var currentUserRule = currentUserSecurity.FirstOrDefault();
			if (currentUserRule != null)
			{
				return (currentUserRule.FileSystemRights & FileSystemRights.Read) != 0 && (currentUserRule.FileSystemRights & FileSystemRights.Write) != 0;
			}

			try
			{
				foreach (FileSystemAccessRule rule in collection)
				{
					var isInRole = CurrentPrincipal.Value.IsInRole(rule.IdentityReference.Value);
					if (isInRole && (rule.FileSystemRights & FileSystemRights.Read) != 0 && (rule.FileSystemRights & FileSystemRights.Write) != 0)
					{
						return true;
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				principalIsInRoleException = ex;
			}
			return false;
		}
	}
}
