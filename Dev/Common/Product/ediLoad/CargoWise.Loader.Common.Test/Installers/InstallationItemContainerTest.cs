using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	sealed class InstallationItemContainerTest : TestCase
	{
		public void TestInstall()
		{
			Installation installation = new Installation(new Configuration());
			InstallationItemContainer parentItem = new InstallationItemContainer(installation);
			DummyInstallationItem childItem1 = new DummyInstallationItem(installation);
			DummyInstallationItem childItem2 = new DummyInstallationItem(installation);
			childItem1.SetNeedsToInstall(true);
			childItem2.SetNeedsToInstall(false);
			parentItem.AddDependency(childItem1);
			parentItem.AddDependency(childItem2);
			parentItem.Install(new InstallationResultCollection());
			AssertEquals("childItem1.InstallCount", 1, childItem1.InstallCount);
			AssertEquals("childItem1.InstallCount", 0, childItem2.InstallCount);
		}
	}
}