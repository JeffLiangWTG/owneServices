using System;
using System.Collections.Generic;
using CargoWise.Main.Navigation;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace CargoWise.Main.Test.Navigation;

public class HomeFeaturesManagerProviderTest : TestCase
{
	public void TestHomeFeaturesManager()
	{
		var uut = HomeFeaturesManagerProvider.GetInstance();

		AssertNotNull(uut);

		foreach (HomeFeature feature in Enum.GetValues(typeof(HomeFeature)))
		{
			AssertType<bool>(uut.IsEnabled(feature));
		}
	}

	public void TestHomeFeaturesManager_WhenSnapshots_WhenCWSupport()
	{
		var uut = HomeFeaturesManagerProvider.GetInstance();

		Assert("CWSupport", uut.IsEnabled(HomeFeature.Snapshots));
	}

#if !WINZOR
	public void TestHomeFeaturesManager_WhenSnapshots_WhenDifferentUser()
	{
		GlbStaff.CurrentUser.GS_Code = "LNS";
		GlbStaff.CurrentUser.GS_LoginName = "john.doe";

		var uut = HomeFeaturesManagerProvider.GetInstance();

		Assert("John Doe", !uut.IsEnabled(HomeFeature.Snapshots));
	}
#endif

	public void TestHomeFeaturesManager_WhenCaseVariant()
	{
		List<(string, string)> testCases = [
			("MS", "MIKE.SVERDLOV"),
			("ms", "mike.sverdlov"),
			("Ms", "Mike.Sverdlov"),
		];

		var uut = HomeFeaturesManagerProvider.GetInstance();

		foreach (var (testCode, testLoginName) in testCases)
		{
			GlbStaff.CurrentUser.GS_Code = testCode;
			GlbStaff.CurrentUser.GS_LoginName = testLoginName;

			Assert(testLoginName, uut.IsEnabled(HomeFeature.Snapshots));
		}
	}
}
