using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.IO;
using CargoWise.Shared;

namespace Enterprise.Dat.Implementation
{
	public static class NetworkFileCacher
	{
		public static void CacheFileLocally(string networkFilePath, string localFilePath)
		{
			var localFileExists = File.Exists(localFilePath);
			if (!localFileExists || File.GetLastWriteTime(networkFilePath) > File.GetLastWriteTime(localFilePath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
				if (localFileExists)
				{
					var attributes = File.GetAttributes(localFilePath);
					if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					{
						File.SetAttributes(localFilePath, attributes & ~FileAttributes.ReadOnly);
					}
				}

				var tempLocation = Temp.GetTempFileName();
				try
				{
					File.Copy(networkFilePath, tempLocation, true);
					File.Delete(localFilePath);
					File.Move(tempLocation, localFilePath);

					var identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
					var accessRule = new FileSystemAccessRule(identity, FileSystemRights.FullControl, AccessControlType.Allow);

					var fileInfo = new FileInfo(localFilePath);
					var accessControl = fileInfo.GetAccessControl(AccessControlSections.Access);
					accessControl.AddAccessRule(accessRule);
					fileInfo.SetAccessControl(accessControl);
				}
				finally
				{
					if (File.Exists(tempLocation))
					{
						File.Delete(tempLocation);
					}
				}
			}
		}

		public static void CleanupOldCachedFiles(string cacheDirectory, string currentReleaseRing)
		{
			foreach (var file in Directory.GetFiles(cacheDirectory))
			{
				var fileName = Path.GetFileName(file);
				if (fileName.StartsWith("CW", StringComparison.OrdinalIgnoreCase) && File.GetLastAccessTimeUtc(file) < DateTime.UtcNow.Subtract(TimeSpan.FromDays(7)))
				{
					FileIO.DeleteFile(file);
				}
				else if (fileName.StartsWith(currentReleaseRing, StringComparison.OrdinalIgnoreCase) || fileName.EndsWith("." + currentReleaseRing, StringComparison.OrdinalIgnoreCase))
				{
					FileIO.DeleteFile(file);
				}
			}
		}
	}
}
