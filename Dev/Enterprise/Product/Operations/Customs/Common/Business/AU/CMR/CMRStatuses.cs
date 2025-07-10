using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRStatuses : CodeDescriptionPairList
	{
		public CMRStatuses()
		{
			AddRange(new CMRBaseStatuses());
			AddRange(new CMRConsolidatedCargoStatuses());
		}
	}
}
