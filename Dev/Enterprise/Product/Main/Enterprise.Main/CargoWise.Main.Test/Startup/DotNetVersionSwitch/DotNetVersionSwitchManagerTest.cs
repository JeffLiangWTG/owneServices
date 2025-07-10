using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Startup.DotNetVersionSwitch;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace CargoWise.Main.Test.Startup.DotNetVersionSwitch;

class DotNetVersionSwitchManagerTest : TestCaseWithFactory
{
	IDisposable alpReleaseRing;
	protected override void SetUp()
	{
		// Assuming that the test is running in an ALP release ring
		this.alpReleaseRing = ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("25.1.1.1", DateTime.Today, "ALP"));
		base.SetUp();
	}

	protected override void TearDown()
	{
		base.TearDown();
		alpReleaseRing.Dispose();
	}

	public void TestIsDotNetVersionSwitchEnabled_WhenIsRunningTest()
	{
		var manager = new DotNetVersionSwitchManager(isRunningTest: false);
		Assert("Net version switch should be enable by default", manager.IsNetVersionSwitchEnabled());

		manager = new DotNetVersionSwitchManager(isRunningTest: true);
		Assert("Net version switch should be disabled when running tests.", !manager.IsNetVersionSwitchEnabled());
	}

	public void TestIsDotNetVersionSwitchEnabled_WhenReleaseRingNotALP()
	{
		var manager = new DotNetVersionSwitchManager(isRunningTest: false);
		Assert("Net version switch should be enable by default", manager.IsNetVersionSwitchEnabled());

		using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("23.1.1.1", DateTime.Today, "DPR")))
		{
			Assert("Net version switch should be disabled on non ALP release",
				!manager.IsNetVersionSwitchEnabled());
		}
	}

	public void TestGetDefaultNetVersionItem_WhenDotNetVersionLaunchSwitchIsNetFramework48_ReturnsNetFrameworkVersionManager()
	{
		using (TemporaryEnforcedNetCoreVersion(isEnabled: false, isInGourp: true, isActive: true))
		using (RawDataRegistry.Instance.DefaultDotNetVersionOnLaunch
			.SetTemporaryValue(default, default, default, DotNetBuildVersionTargetTypeList.Codes.NetFramework48))
		{
			var manager = new DotNetVersionSwitchManager(true);
			var result = manager.GetDefaultNetVersionItem();
			AssertEquals("Default net version item should be NetFrameworkVersionManager when DotNetVersionLaunchSwitch is NetFramework48.", DotNetFrameworkVersionManager.Instance, result);
		}
	}

	public void TestGetDefaultNetVersionItem_WhenDotNetVersionLaunchSwitchIsNetCore8_ReturnsNet8VersionManager()
	{
		using (TemporaryEnforcedNetCoreVersion(isEnabled: false, isInGourp: true, isActive: true))
		using (RawDataRegistry.Instance.DefaultDotNetVersionOnLaunch
			.SetTemporaryValue(default, default, default, DotNetBuildVersionTargetTypeList.Codes.NetCore8))
		{
			var manager = new DotNetVersionSwitchManager(true);
			var result = manager.GetDefaultNetVersionItem();
			AssertEquals("Default net version item should be Net8VersionManager when DotNetVersionLaunchSwitch is NetCore8.", DotNetCore8VersionManager.Instance, result);
		}
	}

	public void TestGetDefaultNetVersionItem_WhenEnforcedNetCoreVersion_ReturnsNet8VersionManager()
	{
		using (TemporaryEnforcedNetCoreVersion(isEnabled: true, isInGourp: true, isActive: true))
		{
			var manager = new DotNetVersionSwitchManager(true);
			var result = manager.GetDefaultNetVersionItem();
			AssertEquals("Default net version item should be Net8VersionManager when DotNetVersionLaunchSwitch is NetCore8.", DotNetCore8VersionManager.Instance, result);
		}
	}

	public void TestGetSwitchVersionMenuItems_WhenNetVersionSwitchIsEnabled_ReturnsMenuItems()
	{
		// Arrange
		var versionMockA = new Mock<IDotNetVersionManager>();
		versionMockA.Setup(m => m.MenuCaption).Returns((NoResString)"Version A: Running");
		versionMockA.Setup(m => m.IsVersionCurrentRunning()).Returns(true);

		var versionMockB = new Mock<IDotNetVersionManager>();
		versionMockB.Setup(m => m.MenuCaption).Returns((NoResString)"Version B: Not Running");
		versionMockB.Setup(m => m.IsVersionCurrentRunning()).Returns(false);

		var versionMockC = new Mock<IDotNetVersionManager>();
		versionMockC.Setup(m => m.MenuCaption).Returns((NoResString)"Version C: Not Running");
		versionMockC.Setup(m => m.IsVersionCurrentRunning()).Returns(false);

		var manager = new DotNetVersionSwitchManager(
			isRunningTest: false,
			availableNetVersions: [versionMockB.Object, versionMockA.Object, versionMockC.Object]);

		using (RawDataRegistry.Instance.EnableDotNetVersionSwitchMenu
			.SetTemporaryValue(default, default, default, true))
		{
			// Act
			var result = manager.GetSwitchVersionMenuItems().ToList();

			// Assert
			AssertEquals("There should be two menu item that not currently running.", 2, result.Count);
			AssertEquals("First menu item should be Version B.", "Version B: Not Running", result[0].Text);
			AssertEquals("Second menu item should be Version C.", "Version C: Not Running", result[1].Text);
		}

		using (RawDataRegistry.Instance.EnableDotNetVersionSwitchMenu
			.SetTemporaryValue(default, default, default, false))
		{
			// Act
			var result = manager.GetSwitchVersionMenuItems().ToList();

			// Assert
			AssertEquals("There should be two menu item that not currently running.", 0, result.Count);
		}
	}

	public void TestGetSwitchVersionMenuItems_WhenNetVersionSwitchIsDisabled_ReturnsEmptyList()
	{
		var manager = new DotNetVersionSwitchManager(isRunningTest: true);
		var result = manager.GetSwitchVersionMenuItems().ToList();
		Assert("Menu items should be empty when Net version switch is disabled.", !result.Any());
	}

	public void TestEnforcedNetCoreVersionForUser_IsTrue()
	{
		AssertEnforcedNetCoreVersionForUser(isEnabled: true, isInGourp: true, isActive: true, expected: true, "EnforcedNetCoreVersion should be true.");
	}

	public void TestEnforcedNetCoreVersionForUser_IsFalse()
	{
		CombineAssertions(() =>
		{
			AssertEnforcedNetCoreVersionForUser(isEnabled: false, isInGourp: true, isActive: true, expected: false, "EnforcedNetCoreVersion should be false when feature test is not enabled.");
			AssertEnforcedNetCoreVersionForUser(isEnabled: true, isInGourp: false, isActive: true, expected: false, "EnforcedNetCoreVersion should be false when user is not in the group.");
			AssertEnforcedNetCoreVersionForUser(isEnabled: true, isInGourp: true, isActive: false, expected: false, "EnforcedNetCoreVersion should be false when netcore version is not enabled.");
			AssertEnforcedNetCoreVersionForUser(isEnabled: false, isInGourp: false, isActive: true, expected: false, "EnforcedNetCoreVersion should be false when feature test is not enabled and user is not in the group.");
			AssertEnforcedNetCoreVersionForUser(isEnabled: false, isInGourp: true, isActive: false, expected: false, "EnforcedNetCoreVersion should be false when feature test is not enabled and netcore version is not enabled.");
			AssertEnforcedNetCoreVersionForUser(isEnabled: true, isInGourp: false, isActive: false, expected: false, "EnforcedNetCoreVersion should be false when user is not in the group and netcore version is not enabled.");
			AssertEnforcedNetCoreVersionForUser(isEnabled: false, isInGourp: false, isActive: false, expected: false, "EnforcedNetCoreVersion should be false when feature test is not enabled, user is not in the group and netcore version is not enabled.");
		});
	}

	void AssertEnforcedNetCoreVersionForUser(bool isEnabled, bool isInGourp, bool isActive, bool expected, string message)
	{
		using (TemporaryEnforcedNetCoreVersion(isEnabled, isInGourp, isActive))
		{
			var manager = new DotNetVersionSwitchManager(isRunningTest: false);
			AssertEquals(message, expected, manager.EnforcedNetCoreVersionForUser());
		}
	}

	IDisposable TemporaryEnforcedNetCoreVersion(bool isEnabled, bool isInGourp, bool isActive)
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var group = Factory.NewWithValidTestData<GlbGroup>();

		if (isInGourp)
		{
			staff.Groups.Add(group);
		}

		Factory.Save();
		var tempFeatureTest = RawDataRegistry.Instance.FeatureTestModeEnabled.SetTemporaryValue(default, default, default, isEnabled);
		TemporaryUserContext userContext = new TemporaryUserContext
		{
			StaffLoginName = staff.GS_LoginName
		};
		var tempUserContext = userContext.Set();

		var featureTest = Factory.NewWithValidTestData<StmFeatureTest>();
		featureTest.SFT_FeatureName = StmFeatureTest.NetCoreVersion;
		featureTest.SFT_IsActive = isActive;
		featureTest.SFT_GG_Group = group.PK;
		Factory.Save();

		return new DisposableAction(() =>
		{
			tempUserContext.Dispose();
			tempFeatureTest.Dispose();
		});
	}
}
