using System;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ReleaseInfoTest : TestCase
	{
		public void TestCreateNewInstanceForTesting()
		{
			DateTime exeDate = new DateTime(2006, 4, 6, 13, 42, 14);
			ReleaseInfo info = ReleaseInfo.CreateNewInstanceForTesting("1.2.3.4", exeDate, ReleaseRings.Codes.GPR);
			AssertEquals("VersionNumber", new VersionNumber(1, 2, 3, 4), info.VersionNumber);
			AssertEquals("ExeDate", exeDate, info.ExeDate);
			AssertEquals("ReleaseRing", ReleaseRings.Codes.GPR, info.ReleaseRing);
			AssertEquals("ReleaseDisplayText", "GP Release 2000 Jan 04 patch 4", info.ReleaseDisplayText);
		}

		public void TestGetReleaseDisplayText()
		{
			AssertEquals("GetReleaseDisplayText()", "2000 Jan 01", ReleaseInfo.GetReleaseDisplayText("", new VersionNumber()));
			AssertEquals("GetReleaseDisplayText()", "Alpha Release 2000 Jan 02", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.ALP, new VersionNumber(1, 4, 1, 0)));
			AssertEquals("GetReleaseDisplayText()", "Alpha Release 2000 Jan 02 patch 1", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.ALP, new VersionNumber(1, 4, 1, 1)));
			AssertEquals("GetReleaseDisplayText()", "DP Release 2000 Jan 03 patch 1", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.DPR, new VersionNumber(1, 4, 2, 1)));
			AssertEquals("GetReleaseDisplayText()", "Standard Release 2006 Aug 17 patch 22", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.STD, new VersionNumber(1, 4, 2420, 22)));
			AssertEquals("GetReleaseDisplayText()", "GP Candidate 2006 Aug 19 patch 2", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.GPC, new VersionNumber(1, 4, 2422, 2)));
			AssertEquals("GetReleaseDisplayText()", "GP Release 2006 Aug 19 patch 2", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.GPR, new VersionNumber(1, 4, 2422, 2)));
			AssertEquals("GetReleaseDisplayText()", "Alpha Release 2013 Aug 19 patch 1", ReleaseInfo.GetReleaseDisplayText(ReleaseRings.Codes.ALP, new VersionNumber(2, 0, 11, 1)));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullXmlFilePath()
		{
			ReleaseInfo info = new ReleaseInfo(null);
		}
	}
}
