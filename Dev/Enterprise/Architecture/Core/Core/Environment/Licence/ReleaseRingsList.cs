using WTG.DevTools.Definitions;

namespace Enterprise.ZArchitecture.Core
{
	public class ReleaseRingsList : CodeDescriptionPairList
	{
		public ReleaseRingsList()
		{
			foreach (var releaseRing in ReleaseRings.List())
			{
				AddPair(releaseRing.Code, releaseRing.LongDescription);
			}
		}
	}
}
