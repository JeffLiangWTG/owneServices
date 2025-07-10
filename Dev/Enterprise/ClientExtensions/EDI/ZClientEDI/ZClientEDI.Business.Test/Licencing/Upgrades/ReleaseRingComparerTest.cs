using System;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class ReleaseRingComparerTest : TestCase
	{
		public void TestCompare()
		{
			ReleaseRingComparer comparer = new ReleaseRingComparer();

			AssertEquals("Compare()", 0, comparer.Compare(ReleaseRings.Codes.ALP, ReleaseRings.Codes.ALP));
			AssertEquals("Compare()", 0, comparer.Compare(ReleaseRings.Codes.DPR, ReleaseRings.Codes.DPR));
			AssertEquals("Compare()", 0, comparer.Compare(ReleaseRings.Codes.STD, ReleaseRings.Codes.STD));
			AssertEquals("Compare()", 0, comparer.Compare(ReleaseRings.Codes.GPC, ReleaseRings.Codes.GPC));
			AssertEquals("Compare()", 0, comparer.Compare(ReleaseRings.Codes.GPR, ReleaseRings.Codes.GPR));

			AssertEquals("Compare()", -1, comparer.Compare(ReleaseRings.Codes.ALP, ReleaseRings.Codes.DPR));
			AssertEquals("Compare()", -1, comparer.Compare(ReleaseRings.Codes.STD, ReleaseRings.Codes.GPC));
			AssertEquals("Compare()", -1, comparer.Compare(ReleaseRings.Codes.GPC, ReleaseRings.Codes.GP1));
			AssertEquals("Compare()", -1, comparer.Compare(ReleaseRings.Codes.LPB, ReleaseRings.Codes.GPR));
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "\"ABC\" is not a valid release ring.")]
		public void TestCompareInvalidReleaseRing()
		{
			new ReleaseRingComparer().Compare("ABC", "");
		}
	}
}
