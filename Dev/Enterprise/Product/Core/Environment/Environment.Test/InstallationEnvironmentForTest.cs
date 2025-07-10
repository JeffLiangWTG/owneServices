using System;
using System.IO;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Environment.Testing
{
	public sealed class InstallationEnvironmentForTest
	{
		public static IDisposable TempDirectoryForTest(string basePathOverride = null)
		{
			var tempDir = new TempDirectory(basePathOverride);
			Directory.CreateDirectory(Path.Combine(tempDir, ReleaseInfo.Instance.VersionNumber.ToString()));

			var path = tempDir.DirectoryName;
			InstallationEnvironment.OverridableGetBinPath.Value = (() => { return path; });

			return new DisposableAction(() => tempDir.Dispose());
		}
	}
}
