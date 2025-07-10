using System;
using System.IO;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;

namespace Enterprise.RemoteDesktopServices
{
	static class FileSystem
	{
		public static ExistenceState FileExists(string path)
		{
			TrackingInfoLogger.Instance.NewLog(() => $"Begin FileExists({path})");

			try
			{
				if (File.Exists(path))
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.Exists = true | {ExistenceState.ExistingNormally}");
					return ExistenceState.ExistingNormally;
				}

				if (Directory.Exists(path))
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.Exists = true | {ExistenceState.ExistingButNotFile}");
					return ExistenceState.ExistingButNotFile;
				}

				try
				{
#if NETFRAMEWORK
					File.GetAccessControl(path);
#elif NETCOREAPP
					_ = new FileInfo(path).GetAccessControl();
#endif
					TrackingInfoLogger.Instance.NewLog(() => $"File.GetAccessControl no exception | {ExistenceState.ExistingButNoPermission}");
					return ExistenceState.ExistingButNoPermission;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.GetAccessControl UnauthorizedAccessException | {ExistenceState.UnauthorizedError}: {unauthorizedAccessException}");
					return ExistenceState.UnauthorizedError;
				}
				catch (FileNotFoundException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.GetAccessControl | {ExistenceState.NotExisting}: {ex}");
					return ExistenceState.NotExisting;
				}
				catch (ArgumentException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.GetAccessControl | {ExistenceState.InvalidCharacterInPath} | '{path}': {ex}");
					return ExistenceState.InvalidCharacterInPath;
				}
				catch (InvalidOperationException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.GetAccessControl | {ExistenceState.ParentDirectoryNotExisting}: {ex}");
					return ExistenceState.ParentDirectoryNotExisting;
				}
#if NETCOREAPP
				catch (PlatformNotSupportedException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.GetAccessControl | {ExistenceState.PlatformNotSupported}: {ex}");
					return ExistenceState.PlatformNotSupported;
				}
#endif
			}
			finally
			{
				TrackingInfoLogger.Instance.NewLog(() => $"End FileExists({path})");
			}
		}

		public static ExistenceState DirectoryExists(string path)
		{
			TrackingInfoLogger.Instance.NewLog(() => $"Begin DirectoryExists({path})");

			try
			{
				if (Directory.Exists(path))
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.Exists = true | {ExistenceState.ExistingNormally}");
					return ExistenceState.ExistingNormally;
				}

				if (File.Exists(path))
				{
					TrackingInfoLogger.Instance.NewLog(() => $"File.Exists = true | {ExistenceState.ExistingButNotDirectory}");
					return ExistenceState.ExistingButNotDirectory;
				}

				try
				{
#if NETFRAMEWORK
					Directory.GetAccessControl(path);
#elif NETCOREAPP
					_ = new DirectoryInfo(path).GetAccessControl();
#endif
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl no exception | {ExistenceState.ExistingButNoPermission}");
					return ExistenceState.ExistingButNoPermission;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl UnauthorizedAccessException | {ExistenceState.UnauthorizedError}: {unauthorizedAccessException}");
					return ExistenceState.UnauthorizedError;
				}
				catch (DirectoryNotFoundException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl | {ExistenceState.NotExisting}: {ex}");
					return ExistenceState.NotExisting;
				}
				catch (ArgumentException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl | {ExistenceState.InvalidCharacterInPath} | '{path}': {ex}");
					return ExistenceState.InvalidCharacterInPath;
				}
				catch (InvalidOperationException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl | {ExistenceState.ParentDirectoryNotExisting}: {ex}");
					return ExistenceState.ParentDirectoryNotExisting;
				}
#if NETCOREAPP
				catch (PlatformNotSupportedException ex)
				{
					TrackingInfoLogger.Instance.NewLog(() => $"Directory.GetAccessControl | {ExistenceState.PlatformNotSupported}: {ex}");
					return ExistenceState.PlatformNotSupported;
				}
#endif
			}
			finally
			{
				TrackingInfoLogger.Instance.NewLog(() => $"End DirectoryExists({path})");
			}
		}
	}
}
