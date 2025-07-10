using System;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	class MinimumDotNetVersionDefinition : IMinimumDotNetVersionDefinition
	{
		public string MinimumDotNetVersionRequired => "4.8";

		public bool IsDotNetSupportedVersion(DotNetVersion dotNetVersion)
		{
			var expectedVersion = new Version(MinimumDotNetVersionRequired);
			if (expectedVersion <= dotNetVersion.Version)
			{
				return true;
			}

			// If the version recorded is lower than our current version, there are two possibilities:
			// 1. They are legitimately running an older .NET Framework
			// 2. They are running a newer .NET Framework that the older version of CargoWise One did not know about.
			// To handle the latter case we reinterpret their release key using our nice new up-to-date build of CW1 that is trying to run the upgrade.
			var reinterpretedVersion = new Version(ClientDotNetRetriever.Get45PlusVersionFromReleaseKey(dotNetVersion.ReleaseNumber));
			return reinterpretedVersion >= expectedVersion;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public bool IsAnOldRecord(PcWithDotNetVersionRecord pcWithDotNetVersion)
		{
			return pcWithDotNetVersion.RecordingDate.AddDays(15) < DateTime.UtcNow;
		}
	}
}
