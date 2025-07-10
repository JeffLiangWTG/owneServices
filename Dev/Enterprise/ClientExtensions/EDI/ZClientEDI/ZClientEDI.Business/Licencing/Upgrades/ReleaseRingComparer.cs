using System;
using System.Collections.Generic;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class ReleaseRingComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			return LookupSafe(x).RingAgeOrdinal.CompareTo(LookupSafe(y).RingAgeOrdinal);
		}

		ReleaseRing LookupSafe(string releaseRing)
		{
			var ring = ReleaseRings.Lookup(releaseRing) ?? throw new ArgumentException("\"" + releaseRing + "\" is not a valid release ring.");
			return ring;
		}
	}
}

