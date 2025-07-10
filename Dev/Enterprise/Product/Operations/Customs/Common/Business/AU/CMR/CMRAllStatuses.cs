using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRAllStatuses : CodeDescriptionPairList
	{
		public CMRAllStatuses()
		{
			AddRange(new CMRBaseStatuses());
			AddRange(new CMRConsolidatedCargoStatuses());
			AddRange(new CMRUnderbondStatuses());
		}
	}
}
