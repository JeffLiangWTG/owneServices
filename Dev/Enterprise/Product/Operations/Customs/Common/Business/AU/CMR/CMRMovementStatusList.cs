using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRMovementStatusList : CodeDescriptionPairList
	{
		public CMRMovementStatusList()
		{
			Add(CMRMovementStatus.Consolidate);
			Add(CMRMovementStatus.DoNotConsolidate);
			Add(CMRMovementStatus.DoNotLoad);
			Add(CMRMovementStatus.HoldForCustoms);
			Add(CMRMovementStatus.Load);
			Add(CMRMovementStatus.Match);
			Add(CMRMovementStatus.NoMatch);
			Add(CMRMovementStatus.Rejected);
		}
	}
}
