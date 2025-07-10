using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Test
{
	sealed class RemoteLaunchDbUpgradeTest : TestCase
	{
		public void TestOldVersionsAreRemovedInStartExeProcessSpawnedByRemoteLaunchDbUpgrader()
		{
			AssertEquals(
				"A B -ScheduledDbUpgrader -NoUI -RemoveOldVersions -ForceRemoveOldVersionsInFrontOfInstallation",
				string.Format(RemoteLaunchDbUpgrade.CWStartExeArgumentFormat, "A", "B"));
		}
	}
}
