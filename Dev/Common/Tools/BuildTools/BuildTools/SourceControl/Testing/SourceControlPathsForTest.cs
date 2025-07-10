#if DEBUG
using System;
using System.IO;
using CargoWise.Common;

namespace CargoWise.BuildTools.Testing
{
	public static class SourceControlPathsForTest
	{
		public static string ConvertLocalPathToServerPath(string localPath)
		{
			Argument.NotNullOrEmpty(localPath, nameof(localPath));
			var result = ConvertSourceControlUriToLocalUri(localPath).Replace(MockSourceControl.WorkspaceFolderName, MockSourceControl.SourceControlFolderName);
			return result;
		}

		public static string ConvertServerPathToLocalPath(string serverPath)
		{
			Argument.NotNullOrEmpty(serverPath, nameof(serverPath));
			var result = ConvertSourceControlUriToLocalUri(serverPath).Replace(MockSourceControl.SourceControlFolderName, MockSourceControl.WorkspaceFolderName);
			return result;
		}

		internal static string ConvertSourceControlUriToLocalUri(string uri)
		{
			Argument.NotNullOrEmpty(uri, nameof(uri));
			string result = uri;
			if (result.StartsWith("$/"))
			{
				result = result.Replace("$/", String.Empty);
				result = result.Replace("/", @"\");
				result = result.Replace("Enterprise", MockSourceControl.SourceControlFolderName);
				result = Path.Combine(WTG.TestHelpers.TestingState.TempPath, result);
			}
			return result;
		}

		public static bool IsInSourceControlBounds(string path)
		{
			Argument.NotNullOrEmpty(path, nameof(path));
			string localPath = path;
			if (path.StartsWith("$"))
			{
				localPath = ConvertSourceControlUriToLocalUri(path);
			}
			return Path.GetFullPath(localPath).Contains(MockSourceControl.WorkspaceFolderName);
		}
	}
}
#endif
