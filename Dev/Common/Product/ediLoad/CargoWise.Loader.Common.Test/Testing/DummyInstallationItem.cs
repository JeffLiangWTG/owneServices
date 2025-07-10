using System;

namespace CargoWise.Loader.Common.Testing
{
	public sealed class DummyInstallationItem : InstallationItem
	{
		readonly string dummyString;
		bool needsToInstall;

		public DummyInstallationItem(Installation installation)
			: base(installation)
		{
			needsToInstall = true;
		}

		public DummyInstallationItem(string dummyString)
			: this((Installation)null)
		{
			this.dummyString = dummyString;
		}

		public string DummyString
		{
			get { return dummyString; }
		}

		public Exception ExceptionToThrowDuringInstall { get; set; }

		public int InstallCount { get; private set; }

		protected override bool NeedsToInstallCore()
		{
			return needsToInstall;
		}

		public void SetNeedsToInstall(bool value)
		{
			needsToInstall = value;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			InstallCount++;
			if (ExceptionToThrowDuringInstall != null)
			{
				throw ExceptionToThrowDuringInstall;
			}
			return base.InstallExcludingDependencies();
		}
	}

	public sealed class DummyRemoteExclusiveInstallationItem : InstallationItem
	{
		public DummyRemoteExclusiveInstallationItem(Installation installation)
			: base(installation)
		{
			needsToInstall = true;
		}

		protected override bool NeedsToInstallCore()
		{
			return needsToInstall;
		}
		readonly bool needsToInstall;

		protected override bool IsRemoteExclusive
		{
			get
			{
				return true;
			}
		}
	}
}