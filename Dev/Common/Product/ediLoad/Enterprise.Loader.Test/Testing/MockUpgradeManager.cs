using System;
using System.IO;
using Enterprise.Upgrades;

namespace Enterprise.Loader.Testing
{
	internal sealed class MockUpgradeManager : UpgradeManager
	{
		public MockUpgradeManager() : base("NoServer", "NoDatabase")
		{
		}

		public bool InstallUpgradePackageCalledForTest { get; set; }

		public override void DownloadUpgradePackageFile(Guid upgradePk, string filePath, Progress progress)
		{
			InstallUpgradePackageCalledForTest = true;
		}

		public override void DownloadUpgradePackageStream(Guid upgradePk, Stream outputStream)
		{
			throw new NotImplementedException();
		}

		public override string GetExpectedClientDll()
		{
			throw new NotImplementedException();
		}

		public UpgradeInfo CurrentVersionForTest { get; set; }

		public override UpgradeInfo QueryCurrentVersion()
		{
			return CurrentVersionForTest;
		}

		public UpgradeInfoExtendedCollection RunnablePackages { get; set; }
		public override UpgradeInfoExtended UploadUpgradePackage(Version version, DateTime versionDate, string status, string statusComment)
		{
			throw new NotImplementedException();
		}

		public override UpgradeInfoExtendedCollection QueryRunnablePackages()
		{
			return RunnablePackages;
		}

		public override UpgradeInfo QueryVersion(Guid upgradePk)
		{
			throw new NotImplementedException();
		}

		public override void SetAsCurrentVersion(UpgradeInfo upgrade, string userCode, string userName)
		{
			throw new NotImplementedException();
		}

		public override UpgradeInfoExtended UploadUpgradePackage(Stream fileStream, Version version, DateTime versionDate, string status, string statusComment, Progress progress)
		{
			throw new NotImplementedException();
		}
	}
}
