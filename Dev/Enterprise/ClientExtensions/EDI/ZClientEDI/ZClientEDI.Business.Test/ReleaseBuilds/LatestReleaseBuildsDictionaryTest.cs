using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ReleaseBuilds.Test
{
	public class LatestReleaseBuildsDictionaryTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var build1 = ReleaseBuild.NewForTesting(Factory, "x", true);
			var build2 = ReleaseBuild.NewForTesting(Factory, "x", false);
			var build3 = ReleaseBuild.NewForTesting(Factory, "y", false);
			var build4 = ReleaseBuild.NewForTesting(Factory, "z", true);

			build1.HL_ExeVersionDate = ZDateTime.Now;
			build2.HL_ExeVersionDate = ZDateTime.Now.AddDays(1);

			Factory.Save();

			var buildsDictionary = new LatestReleaseBuildsDictionary(Factory);
			buildsDictionary.Load();
			AssertEquals("buildsDictionary[\"x\"].PK", build2.PK, buildsDictionary.GetLatestAvailableBuild("ENT", "x", takeWeeklyBuildsInsteadOfLatest: false).PK);
			AssertEquals("buildsDictionary[\"y\"].PK", build3.PK, buildsDictionary.GetLatestAvailableBuild("ENT", "y", takeWeeklyBuildsInsteadOfLatest: false).PK);
			AssertNull("buildsDictionary.ContainsKey(\"z\")", buildsDictionary.GetLatestAvailableBuild("ENT", "z", takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestLoadifWeeklyBuildCutOffEnabled()
		{
			var build1 = ReleaseBuild.NewForTesting(Factory, "x", true);
			var build2 = ReleaseBuild.NewForTesting(Factory, "x", false);
			var build3 = ReleaseBuild.NewForTesting(Factory, "x", false);
			var build4 = ReleaseBuild.NewForTesting(Factory, "y", false);
			var build5 = ReleaseBuild.NewForTesting(Factory, "y", true);

			build1.HL_ExeVersionDate = ZDateTime.Now;
			build2.HL_ExeVersionDate = ZDateTime.Now.AddMinutes(-1);
			build3.HL_ExeVersionDate = ZDateTime.Now.AddMinutes(-2);

			build4.HL_ExeVersionDate = ZDateTime.Now;
			build5.HL_ExeVersionDate = ZDateTime.Now.AddMinutes(-2);

			Factory.Save();

			var buildsDictionary = new LatestReleaseBuildsDictionary(Factory);
			buildsDictionary.Load();
			AssertEquals("buildsDictionary[\"x\"].PK", build3.PK, buildsDictionary.GetLatestAvailableBuild("ENT", "x", takeWeeklyBuildsInsteadOfLatest: true).PK);
			AssertEquals("buildsDictionary[\"y\"].PK", build4.PK, buildsDictionary.GetLatestAvailableBuild("ENT", "y", takeWeeklyBuildsInsteadOfLatest: true).PK);
		}

		public void TestLoad_IgnoreInactiveBuilds()
		{
			var build1 = ReleaseBuild.NewForTesting(Factory, "v", false);
			build1.HL_IsActive = true;
			var build2 = ReleaseBuild.NewForTesting(Factory, "x", false);
			build2.HL_IsActive = false;
			var build3 = ReleaseBuild.NewForTesting(Factory, "y", false);
			build3.HL_IsActive = true;
			var build4 = ReleaseBuild.NewForTesting(Factory, "z", false);
			build4.HL_IsActive = false;

			Factory.Save();

			var buildsDictionary = new LatestReleaseBuildsDictionary(Factory);
			buildsDictionary.Load();
			AssertEquals("buildsDictionary[\"v\"].PK", build1.PK, buildsDictionary.GetLatestAvailableBuild("ENT", "v", takeWeeklyBuildsInsteadOfLatest: false).PK);
			AssertNull("buildsDictionary.ContainsKey(\"x\")", buildsDictionary.GetLatestAvailableBuild("ENT", "x", takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals("buildsDictionary[\"y\"].PK", build3.PK, buildsDictionary.GetLatestAvailableBuild("ENT", "y", takeWeeklyBuildsInsteadOfLatest: false).PK);
			AssertNull("buildsDictionary.ContainsKey(\"z\")", buildsDictionary.GetLatestAvailableBuild("ENT", "z", takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGpcReleaseBuildIsAvailable()
		{
			var buildsDictionary = new LatestReleaseBuildsDictionary(Factory);
			buildsDictionary.Load();

			AssertEquals("GPC Release Build is not available becuase there is not created", false, buildsDictionary.GpcReleaseBuildIsAvailable);

			var gprBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.GPR, false);
			var gpcBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.GPC, false);
			gprBuild.HL_MajorVersion = 1;
			gprBuild.HL_MinorVersion = 4;
			gprBuild.HL_Release = 5000;
			gprBuild.HL_Patch = 1000;
			gpcBuild.HL_MajorVersion = gprBuild.HL_MajorVersion;
			gpcBuild.HL_MinorVersion = gprBuild.HL_MinorVersion;
			gpcBuild.HL_Release = gprBuild.HL_Release;
			gpcBuild.HL_Patch = 500;
			Factory.Save();

			buildsDictionary.Load();
			AssertEquals("GPC Release Build is not available becuase it is older than GPR", false, buildsDictionary.GpcReleaseBuildIsAvailable);

			gpcBuild.HL_Patch = 2000;
			Factory.Save();
			buildsDictionary.Load();
			AssertEquals("GPC Release Build is available becuase it is newer than GPR", true, buildsDictionary.GpcReleaseBuildIsAvailable);
		}

		#region TestGetLatestAvailableBuild

		public void TestGetLatestAvailableBuild()
		{
			var gprBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.GPR, false);
			gprBuild.HL_MajorVersion = 1;
			gprBuild.HL_MinorVersion = 4;
			gprBuild.HL_Release = 5000;
			gprBuild.HL_Patch = 1000;

			var gpcBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.GPC, false);
			gpcBuild.HL_MajorVersion = gprBuild.HL_MajorVersion;
			gpcBuild.HL_MinorVersion = gprBuild.HL_MinorVersion;
			gpcBuild.HL_Release = gprBuild.HL_Release;
			gpcBuild.HL_Patch = 500;

			var alpBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.ALP, false);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			AssertEquals(gprBuild, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(gprBuild, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPC, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(alpBuild, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(null, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildOnRing()
		{
			var gprBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.GPR, false);
			gprBuild.HL_MajorVersion = 1;
			gprBuild.HL_MinorVersion = 4;
			gprBuild.HL_Release = 5000;
			gprBuild.HL_Patch = 1000;

			var gpcBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.GPC, false);
			gpcBuild.HL_MajorVersion = gprBuild.HL_MajorVersion;
			gpcBuild.HL_MinorVersion = gprBuild.HL_MinorVersion;
			gpcBuild.HL_Release = gprBuild.HL_Release;
			gpcBuild.HL_Patch = 500;

			var alpBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.ALP, false);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			AssertEquals(gprBuild, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(gprBuild, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPC, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(alpBuild, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(null, builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, takeWeeklyBuildsInsteadOfLatest: false));
		}

		#endregion

		#region TestGetLatestAvailableBuildForInstalledVersion

		public void TestGetLatestAvailableBuildForInstalledVersion_UpgradeGP1ReturnsGP1()
		{
			// create build entries as exist at 22 June 2016
			ReleaseBuild releaseGP1_old = CreateNewReleaseBuild(16, 2, 17, 0, true, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 5, 11, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 6, 8, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 6, 8, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 6, 22, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			// cust requests upgrade from earlier GP1, should get GP1
			ReleaseBuild clientBuild = releaseGP1_old;
			AssertEquals("Should get latest GP1",
				releaseGP1, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, clientBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_UpgradeGP1ReturnsSame()
		{
			// create build entries as exist at 22 June 2016
			ReleaseBuild releaseGP1_old = CreateNewReleaseBuild(16, 2, 17, 0, true, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 5, 11, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 6, 8, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 6, 8, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 6, 22, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			// cust requests upgrade from latest GP1, should get same version.
			ReleaseBuild clientBuild = releaseGP1;
			AssertEquals("Should get latest GP1",
				releaseGP1, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, clientBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_EscalatedDPRReturnsDPR()
		{
			// create build entries as exist at 22 June 2016
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 2, 17, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 3, 16, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 4, 6, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 4, 13, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			// cust requests upgrade, should get DPR
			var clientBuild = releaseDPR_old;
			AssertEquals("Should get latest DPR since STD is too old",
				releaseDPR, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, clientBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_EscalatedDPRReturnsSTD()
		{
			// create build entries as exist at 22 June 2016
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 2, 17, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 3, 16, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 4, 13, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 4, 13, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 4, 27, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			// cust requests upgrade from earlier DPR, should get latest STD since GP1 is too old.
			var clientBuild = releaseDPR_old;
			AssertEquals("Should get latest STD since GP1 is too old",
				releaseSTD, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, clientBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_EscalatedSTDReturnsGP1()
		{
			// create build entries as at 22 June 2016
			ReleaseBuild releaseGP1_old = CreateNewReleaseBuild(16, 2, 17, 0, true, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 5, 11, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR_old = CreateNewReleaseBuild(16, 6, 8, 0, true, ReleaseRings.Codes.DPR);
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 6, 8, 0, false, ReleaseRings.Codes.STD);
			ReleaseBuild releaseDPR = CreateNewReleaseBuild(16, 6, 22, 0, false, ReleaseRings.Codes.DPR);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			// cust requests upgrade from STD, should get GP1
			ReleaseBuild clientBuild = releaseSTD_old;
			AssertEquals("Should get latest GP1",
				releaseGP1, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, clientBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_HasSTDLicencedLPR()
		{
			ReleaseBuild releaseLPR_old = CreateNewReleaseBuild(16, 1, 1, 0, true, ReleaseRings.Codes.LPR);
			ReleaseBuild releaseSTD_old = CreateNewReleaseBuild(16, 3, 3, 0, true, ReleaseRings.Codes.STD);
			ReleaseBuild releaseLPR = CreateNewReleaseBuild(16, 2, 2, 0, false, ReleaseRings.Codes.LPR);
			ReleaseBuild releaseSTD = CreateNewReleaseBuild(16, 4, 4, 0, false, ReleaseRings.Codes.STD);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			// cust requests upgrade from STD, should get GP1
			ReleaseBuild clientBuild = releaseSTD_old;
			AssertEquals("Should not get an update",
				releaseSTD_old, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.LPR, clientBuild.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_UnrecognisedInstalledVersion()
		{
			ReleaseBuild releaseGP1 = CreateNewReleaseBuild(16, 5, 11, 0, false, ReleaseRings.Codes.GP1);
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			var clientVersion = new VersionNumber("16.6.18.0");

			var build = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, clientVersion, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals(releaseGP1, build);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_PatchOnly()
		{
			var b16110 = CreateNewReleaseBuild(16, 1, 1, 0, false, ReleaseRings.Codes.ALP);
			var b16111 = CreateNewReleaseBuild(16, 1, 1, 1, false, ReleaseRings.Codes.ALP);
			var b16112 = CreateNewReleaseBuild(16, 1, 1, 2, false, ReleaseRings.Codes.ALP);
			var b16113 = CreateNewReleaseBuild(16, 1, 1, 3, false, ReleaseRings.Codes.ALP);
			var b16120 = CreateNewReleaseBuild(16, 1, 2, 0, false, ReleaseRings.Codes.ALP);
			var b16121 = CreateNewReleaseBuild(16, 1, 2, 1, false, ReleaseRings.Codes.ALP);
			var b16122 = CreateNewReleaseBuild(16, 1, 2, 2, false, ReleaseRings.Codes.ALP);
			var b16130 = CreateNewReleaseBuild(16, 1, 3, 0, false, ReleaseRings.Codes.ALP);
			var b16131 = CreateNewReleaseBuild(16, 1, 3, 1, false, ReleaseRings.Codes.ALP);

			var idx = 0;
			var now = ZDateTime.Now.Date;
			foreach (var rb in new[] { b16110, b16111, b16112, b16113, b16120, b16121, b16122, b16130, b16131 }.Reverse())
			{
				rb.HL_ExeVersionDate = now.AddDays(idx -= 3);
			}

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			AssertEquals(b16113, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("16.1.1.0"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(b16113, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("16.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(b16113, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("16.1.1.2"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(b16113, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("16.1.1.3"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false));

			AssertEquals(b16122, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("16.1.2.0"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(b16131, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("16.1.3.0"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_ShouldSendLatestGP2()
		{
			var gp1_1611123 = CreateNewReleaseBuild(16, 1, 1, 123, false, ReleaseRings.Codes.GP1);
			var gp1_1611124 = CreateNewReleaseBuild(16, 1, 1, 124, false, ReleaseRings.Codes.GP1);
			var gp1_1613125 = CreateNewReleaseBuild(16, 1, 3, 125, false, ReleaseRings.Codes.GP1);
			var gp1_1613126 = CreateNewReleaseBuild(16, 1, 3, 126, false, ReleaseRings.Codes.GP1);

			var gp2_1611900 = CreateNewReleaseBuild(16, 1, 1, 900, false, ReleaseRings.Codes.GP2);
			var gp2_1611901 = CreateNewReleaseBuild(16, 1, 1, 901, false, ReleaseRings.Codes.GP2);
			var gp2_1613902 = CreateNewReleaseBuild(16, 1, 3, 902, false, ReleaseRings.Codes.GP2);
			var gp2_1613903 = CreateNewReleaseBuild(16, 1, 3, 903, false, ReleaseRings.Codes.GP2);

			var now = ZDateTime.Now.Date;
			gp2_1613903.HL_ExeVersionDate = gp1_1613126.HL_ExeVersionDate = now;
			gp2_1613902.HL_ExeVersionDate = gp1_1613125.HL_ExeVersionDate = now.AddDays(-3);
			gp2_1611901.HL_ExeVersionDate = gp1_1611124.HL_ExeVersionDate = now.AddDays(-6);
			gp2_1611900.HL_ExeVersionDate = gp1_1611123.HL_ExeVersionDate = now.AddDays(-9);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//When the installedVersion is GP2 and the licesenedReleaseRing is GP1, we should again send the GP2 not GP1.
			AssertEquals(gp2_1613903, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("16.1.1.900"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(gp2_1613903, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("16.1.3.902"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));

			/// Where the installedVersion ring is GP1 and the licensedReleaseRing is GP1 but we should send a GP2 release if the GP1 they are on is now GP2. 
			/// If the installed version release date or major/minor/release as the same as the current GP2
			/// E.g. current GP1 is 17-DEC-18 / 18.12.17.793, next week when we have a new GP1 the GP2 will be 17-DEC-18 / 18.12.17.??? 
			AssertEquals(gp2_1613903, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("16.1.1.123"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
			AssertEquals(gp2_1613903, builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("16.1.3.125"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false));
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_ShouldNotSendLatestGP2ForCW1UpgradeToCGW()
		{
			//LD is licenced to GP1, running on ENT GP2 24.10.30.700, latest ENT GP2 is 24.10.30.900, latest CGW GP1 is 25.4.7.120
			//LD should get the latest CGW GP1 build

			var cw1PreviousGP2 = CreateNewReleaseBuild(24, 10, 30, 700, superceded: true, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);
			var cw1LatestGP2 = CreateNewReleaseBuild(24, 10, 30, 900, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);
			var cgwLatestGP1 = CreateNewReleaseBuild(25, 4, 7, 120, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWise);
			Factory.Save();
			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			var latestBuild = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.700"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals(cgwLatestGP1, latestBuild);
		}

		#endregion

		#region CW1 to CWN Transition

		public void TestGetLatestAvailableBuildForInstalledVersion_CGWALP()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 21, 426, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 245, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 451, superceded: true, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 10, 30, 713, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(25, 3, 18, 110, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(25, 3, 5, 94, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(25, 2, 19, 117, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);
			var cwnGP1 = CreateNewReleaseBuild(24, 12, 25, 308, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWiseNext);

			//CGW
			var cgwALP = CreateNewReleaseBuild(25, 3, 19, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWise);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.21.426"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.115"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.245"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.451"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.10.30.700"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no fallback, so there should be no available build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 GP2 build since it's on the same version", cw1GP2, patchForCW1DPR);
			AssertEquals("CW1 STD should get CW1 GP2 build since it's on the same version", cw1GP2, patchForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP2 build since it's on the same version", cw1GP2, patchForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.21.426"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.115"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.245"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.451"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.10.30.700"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should get CGW ALP build", cgwALP, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 GP2 build", cw1GP2, upgradeForCW1DPR);
			AssertEquals("CW1 STD should get CW1 GP2 build", cw1GP2, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP2 build", cw1GP2, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("25.3.18.100"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("25.3.5.50"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.2.19.100"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.12.25.300"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should have no available build since it's superceded", null, patchForCWNALP);
			AssertEquals("CWN ALP should get CWN DPR build", cwnDPR, patchForCWNDPR);
			AssertEquals("CWN ALP should get CWN STD build", cwnSTD, patchForCWNSTD);
			AssertEquals("CWN ALP should get CWN GP1 build", cwnGP1, patchForCWNGP1);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("25.3.18.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("25.3.5.50"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.2.19.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.12.25.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CGW ALP build", cgwALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN GP1 build", cwnGP1, upgradeForCWNGP1);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CGWDPR()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 21, 426, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 245, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 451, superceded: true, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 10, 30, 713, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(25, 3, 18, 110, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(25, 3, 5, 94, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(25, 2, 19, 117, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);
			var cwnGP1 = CreateNewReleaseBuild(24, 12, 25, 308, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWiseNext);

			//CGW
			var cgwALP = CreateNewReleaseBuild(25, 3, 16, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWise);
			var cgwDPR = CreateNewReleaseBuild(25, 3, 19, 62, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWise);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.21.426"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.115"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.245"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.451"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.10.30.700"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no fallback, so there should be no available build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 GP2 build since it's on the same version", cw1GP2, patchForCW1DPR);
			AssertEquals("CW1 STD should get CW1 GP2 build since it's on the same version", cw1GP2, patchForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP2 build since it's on the same version", cw1GP2, patchForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.21.426"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.115"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.245"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.451"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.10.30.700"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should get CGW ALP build", cgwALP, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should get CGW GP2 build", cgwDPR, upgradeForCW1DPR);
			AssertEquals("CW1 STD should get CW1 GP2 build", cw1GP2, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP2 build", cw1GP2, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("25.3.18.100"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("25.3.5.50"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.2.19.100"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.12.25.300"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should have no available build since it's superceded", null, patchForCWNALP);
			AssertEquals("CWN DPR should have no available build since it's superceded", null, patchForCWNDPR);
			AssertEquals("CWN STD should have no available build", cwnSTD, patchForCWNSTD);
			AssertEquals("CWN GP1 should have no available build", cwnGP1, patchForCWNGP1);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("25.3.18.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("25.3.5.50"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.2.19.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.12.25.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CGW ALP build", cgwALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CGW DPR build", cgwDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN GP1 build", cwnGP1, upgradeForCWNGP1);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNALP()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 7, 12, 807, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 4, 19, 745, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 11, 20, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.110"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.130"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.7.12.800"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.4.19.700"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no available build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 DPR build", cw1DPR, patchForCW1DPR);
			AssertEquals("CW1 STD should get CW1 STD build", cw1STD, patchForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP1 build", cw1GP1, patchForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no available build", null, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 DPR build", cw1DPR, upgradeForCW1DPR);
			AssertEquals("CW1 STD should get CW1 STD build", cw1STD, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP1 build", cw1GP1, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.20.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should have no avaiable build", null, upgradeForCWNDPR);
			AssertEquals("CWN STD should have no avaiable build", null, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should have no avaiable build", null, upgradeForCWNGP1);
			AssertEquals("CWN GP2 should have no avaiable build", null, upgradeForCWNGP2);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNDPR()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 7, 12, 807, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 4, 19, 745, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 11, 28, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(24, 11, 27, 5, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.110"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.130"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.7.12.800"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.4.19.700"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no avaiable build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 STD build as lastest DPR is CWN", cw1STD, patchForCW1DPR);
			AssertEquals("CW1 STD should get CW1 STD build", cw1STD, patchForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP1 build", cw1GP1, patchForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			//CWN major upgrade
			AssertEquals("CW1 ALP should have no avaiable build", null, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should get CW1 STD build as lastest DPR is CWN", cw1STD, upgradeForCW1DPR);
			AssertEquals("CW1 STD should get CW1 STD build", cw1STD, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should get CW1 GP1 build", cw1GP1, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should get CW1 GP2 build", cw1GP2, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.28.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, patchForCWNDPR);
			AssertEquals("CWN STD should get CWN DPR build as as highest ring CWN is DPR", cwnDPR, patchForCWNSTD);
			AssertEquals("CWN GP1 should get CWN DPR build as as highest ring CWN is DPR", cwnDPR, patchForCWNGP1);
			AssertEquals("CWN GP2 should get CWN DPR build as as highest ring CWN is DPR", cwnDPR, patchForCWNGP2);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.27.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false );

			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN DPR build as as highest ring CWN is DPR", cwnDPR, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN DPR build as as highest ring CWN is DPR", cwnDPR, upgradeForCWNGP1);
			AssertEquals("CWN GP2 should get CWN DPR build as as highest ring CWN is DPR", cwnDPR, upgradeForCWNGP2);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNSTD()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 7, 12, 940, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 12, 12, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(24, 12, 11, 9, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(24, 11, 27, 40, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.20.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.110"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.130"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.300"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.7.12.900"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no avaiable build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should get GP1 build as lastest DPR is CWN", cw1GP1, patchForCW1DPR);
			AssertEquals("CW1 STD should get GP1 build as lastest STD is CWN", cw1GP1, patchForCW1STD);
			AssertEquals("CW1 GP1 should get GP1 build", cw1GP1, patchForCW1GP1);
			AssertEquals("CW1 GP2 should get GP2 build", cw1GP2, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no avaiable build", null, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should get GP1 build as lastest DPR is CWN", cw1GP1, upgradeForCW1DPR);
			AssertEquals("CW1 STD should get GP1 build as lastest STD is CWN", cw1GP1, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should get GP1 build", cw1GP1, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should get GP2 build", cw1GP2, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.12.12.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.12.11.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, patchForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, patchForCWNSTD);
			AssertEquals("CWN GP1 should get CWN STD build as highest ring CWN is STD", cwnSTD, patchForCWNGP1);
			AssertEquals("CWN GP2 should get CWN STD build as highest ring CWN is STD", cwnSTD, patchForCWNGP2);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN STD build as highest ring CWN is STD", cwnSTD, upgradeForCWNGP1);
			AssertEquals("CWN GP2 should get CWN STD build as highest ring CWN is STD", cwnSTD, upgradeForCWNGP2);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNSTD_NewVersion()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 7, 12, 940, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 12, 12, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(24, 12, 11, 9, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(24, 11, 27, 40, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);
			var cwnSTDNew = CreateNewReleaseBuild(25, 1, 1, 10, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.12.12.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.12.11.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, patchForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTDNew, patchForCWNSTD);
			AssertEquals("CWN GP1 should get CWN STD build as highest ring CWN is STD", cwnSTDNew, patchForCWNGP1);
			AssertEquals("CWN GP2 should get CWN STD build as highest ring CWN is STD", cwnSTDNew, patchForCWNGP2);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTDNew, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN STD build as highest ring CWN is STD", cwnSTDNew, upgradeForCWNGP1);
			AssertEquals("CWN GP2 should get CWN STD build as highest ring CWN is STD", cwnSTDNew, upgradeForCWNGP2);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNSTD_AllSuperseded()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 7, 12, 940, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 12, 12, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(24, 12, 11, 9, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(24, 11, 27, 40, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CWN patch
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN STD should have no avaiable build", null, patchForCWNSTD);

			//CWN major upgrade
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN STD should have no avaiable build", null, upgradeForCWNSTD);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNSTD_TurnOffFallbackRule()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 7, 12, 940, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 12, 12, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(24, 12, 11, 9, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(24, 11, 27, 40, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			using (EDIDataRegistry.Instance.EnableCargoWiseNextTransitionVersionFallbackRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var builds = new LatestReleaseBuildsDictionary(Factory);
				builds.Load();

				//CW1 patch
				var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.20.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.110"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.130"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.300"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.7.12.900"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

				AssertEquals("CW1 ALP should have no avaiable build", null, patchForCW1ALP);
				AssertEquals("CW1 DPR should have no avaiable build - fallback rule is disabled", null, patchForCW1DPR);
				AssertEquals("CW1 STD should have no avaiable build - fallback rule is disabled", null, patchForCW1STD);
				AssertEquals("CW1 GP1 should get GP1 build", cw1GP1, patchForCW1GP1);
				AssertEquals("CW1 GP2 should get GP2 build", cw1GP2, patchForCW1GP2);

				//CW1 major upgrade
				var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

				AssertEquals("CW1 ALP should have no avaiable build", null, upgradeForCW1ALP);
				AssertEquals("CW1 DPR should have no avaiable build - fallback rule is disabled", null, upgradeForCW1DPR);
				AssertEquals("CW1 STD should have no avaiable build - fallback rule is disabled", null, upgradeForCW1STD);
				AssertEquals("CW1 GP1 should get GP1 build", cw1GP1, upgradeForCW1GP1);
				AssertEquals("CW1 GP2 should get GP2 build", cw1GP2, upgradeForCW1GP2);

				//CWN patch
				var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.12.12.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.12.11.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.11.27.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				var patchForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
				AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);
				AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, patchForCWNDPR);
				AssertEquals("CWN STD should get CWN STD build", cwnSTD, patchForCWNSTD);
				AssertEquals("CWN GP1 should have no avaiable build - fallback rule is disabled", null, patchForCWNGP1);
				AssertEquals("CWN GP2 should have no avaiable build - fallback rule is disabled", null, patchForCWNGP2);

				//CWN major upgrade
				var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
				var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

				AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
				AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
				AssertEquals("CWN STD should get CWN STD build", cwnSTD, upgradeForCWNSTD);
				AssertEquals("CWN GP1 should have no avaiable build - fallback rule is disabled", null, upgradeForCWNGP1);
				AssertEquals("CWN GP2 should have no avaiable build - fallback rule is disabled", null, upgradeForCWNGP2);
			}
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNGP1()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: true, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 10, 30, 592, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(25, 2, 6, 5, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(25, 2, 5, 19, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(25, 1, 22, 40, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);
			var cwnGP1 = CreateNewReleaseBuild(25, 1, 1, 242, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.20.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.110"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.130"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.300"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.10.30.500"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no avaiable build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should get GP2 build as lastest DPR is CWN", cw1GP2, patchForCW1DPR);
			AssertEquals("CW1 STD should get GP2 build as lastest STD is CWN", cw1GP2, patchForCW1STD);
			AssertEquals("CW1 GP1 should get GP2 build as lastest GP1 is CWN", cw1GP2, patchForCW1GP1);
			AssertEquals("CW1 GP2 should get GP2 build", cw1GP2, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CW1 ALP should have no avaiable build", null, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should get GP2 build as lastest DPR is CWN", cw1GP2, upgradeForCW1DPR);
			AssertEquals("CW1 STD should get GP2 build as lastest STD is CWN", cw1GP2, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should get GP2 build as lastest GP1 is CWN", cw1GP2, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should get GP2 build", cw1GP2, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("25.2.6.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("25.2.5.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.1.22.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, patchForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, patchForCWNSTD);
			AssertEquals("CWN GP1 should get CWN GP1 build", cwnGP1, patchForCWNGP1);
			AssertEquals("CWN GP2 should get CWN GP1 build as highest ring CWN is GP1", cwnGP1, patchForCWNGP2);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN GP1 build", cwnGP1, upgradeForCWNGP1);
			AssertEquals("CWN GP2 should get CWN GP1 build as highest ring CWN is GP1", cwnGP1, upgradeForCWNGP2);
		}

		public void TestGetLatestAvailableBuildForInstalledVersion_CWNGP2()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise);
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: true, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise);
			var cw1GP2 = CreateNewReleaseBuild(24, 10, 30, 592, superceded: true, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise);

			//CWN
			var cwnALP = CreateNewReleaseBuild(25, 5, 10, 5, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext);
			var cwnDPR = CreateNewReleaseBuild(25, 5, 7, 19, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cwnSTD = CreateNewReleaseBuild(25, 4, 16, 242, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext);
			var cwnGP1 = CreateNewReleaseBuild(25, 3, 19, 40, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWiseNext);
			var cwnGP2 = CreateNewReleaseBuild(25, 1, 1, 242, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.CargoWiseNext);

			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			//CW1 patch
			var patchForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.20.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.30.110"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.10.30.130"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.10.30.300"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.10.30.500"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);

			//TODO: Until we have CW1 release ring for legacy CW1 support
			AssertEquals("CW1 ALP should have no avaiable build", null, patchForCW1ALP);
			AssertEquals("CW1 DPR should have no avaiable build", null, patchForCW1DPR);
			AssertEquals("CW1 STD should have no avaiable build", null, patchForCW1STD);
			AssertEquals("CW1 GP1 should have no avaiable build", null, patchForCW1GP1);
			AssertEquals("CW1 GP2 should have no avaiable build", null, patchForCW1GP2);

			//CW1 major upgrade
			var upgradeForCW1ALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1STD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCW1GP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			//TODO: Until we have CW1 release ring for legacy CW1 support
			AssertEquals("CW1 ALP should have no avaiable build", null, upgradeForCW1ALP);
			AssertEquals("CW1 DPR should have no avaiable build", null, upgradeForCW1DPR);
			AssertEquals("CW1 STD should have no avaiable build", null, upgradeForCW1STD);
			AssertEquals("CW1 GP1 should have no avaiable build", null, upgradeForCW1GP1);
			AssertEquals("CW1 GP2 should have no avaiable build", null, upgradeForCW1GP2);

			//CWN patch
			var patchForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("25.5.10.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("25.5.7.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("25.4.16.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("25.3.19.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			var patchForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("25.1.1.1"), isPatchOnly: true, takeWeeklyBuildsInsteadOfLatest: false);
			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, patchForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, patchForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, patchForCWNSTD);
			AssertEquals("CWN GP1 should get CWN GP1 build", cwnGP1, patchForCWNGP1);
			AssertEquals("CWN GP1 should get CWN GP2 build", cwnGP2, patchForCWNGP2);

			//CWN major upgrade
			var upgradeForCWNALP = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.ALP, new VersionNumber("24.11.19.10"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNDPR = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.DPR, new VersionNumber("24.10.23.100"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNSTD = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.STD, new VersionNumber("24.9.18.300"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP1 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP1, new VersionNumber("24.4.19.600"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);
			var upgradeForCWNGP2 = builds.GetLatestAvailableBuildForInstalledVersion(ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GP2, new VersionNumber("24.1.31.500"), isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: false);

			AssertEquals("CWN ALP should get CWN ALP build", cwnALP, upgradeForCWNALP);
			AssertEquals("CWN DPR should get CWN DPR build", cwnDPR, upgradeForCWNDPR);
			AssertEquals("CWN STD should get CWN STD build", cwnSTD, upgradeForCWNSTD);
			AssertEquals("CWN GP1 should get CWN GP1 build", cwnGP1, upgradeForCWNGP1);
			AssertEquals("CWN GP2 should get CWN GP2 build", cwnGP2, upgradeForCWNGP2);
		}

		#endregion

		ReleaseBuild CreateNewReleaseBuild(int majorVersion, int minorVersion, int release, int patch, bool superceded, string releaseRing, string product = "ENT")
		{
			var releaseBuild = ReleaseBuild.NewForTesting(Factory, releaseRing, superceded);
			releaseBuild.HL_Product = product;
			releaseBuild.HL_MajorVersion = majorVersion;
			releaseBuild.HL_MinorVersion = minorVersion;
			releaseBuild.HL_Release = release;
			releaseBuild.HL_Patch = patch;
			releaseBuild.HL_ExeVersionDate = superceded ? ZDateTime.Now.AddHours(-1) : ZDateTime.Now;

			return releaseBuild;
		}
	}
}
