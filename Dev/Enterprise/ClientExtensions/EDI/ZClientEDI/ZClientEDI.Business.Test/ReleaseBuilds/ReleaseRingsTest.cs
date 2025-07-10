using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business.Test
{
	class ReleaseRingsTest : TestCase
	{
		public void TestGetRingCodeByCheckInType()
		{
			foreach (var releaseRing in ReleaseRings.List())
			{
				AssertEquals(releaseRing.Code, ReleaseRingsLookup.GetRingCodeByCheckInType(releaseRing.CheckinTask));
			}
			AssertEquals("Wrong value - empty string should be returned", string.Empty, ReleaseRingsLookup.GetRingCodeByCheckInType("XXX"));
		}

		public void TestGetPreviousRing()
		{
			AssertEquals(ReleaseRings.Codes.ALP, ReleaseRingsLookup.Instance.GetPreviousRing(ReleaseRings.Codes.DPR));
			AssertEquals(ReleaseRings.Codes.DPR, ReleaseRingsLookup.Instance.GetPreviousRing(ReleaseRings.Codes.STD));
			AssertEquals(ReleaseRings.Codes.STD, ReleaseRingsLookup.Instance.GetPreviousRing(ReleaseRings.Codes.GPC));
			AssertEquals(ReleaseRings.Codes.GPC, ReleaseRingsLookup.Instance.GetPreviousRing(ReleaseRings.Codes.GP1));
			AssertEquals(string.Empty, ReleaseRingsLookup.Instance.GetPreviousRing(ReleaseRings.Codes.LPB));
			AssertEquals(ReleaseRings.Codes.LPB, ReleaseRingsLookup.Instance.GetPreviousRing(ReleaseRings.Codes.GPR));
			AssertEquals(string.Empty, ReleaseRingsLookup.Instance.GetPreviousRing("ZZZ"));
			AssertEquals(string.Empty, ReleaseRingsLookup.Instance.GetPreviousRing(string.Empty));
		}
	}
}
