using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRNoticeStatusList : CodeDescriptionPairList
	{
		public CMRNoticeStatusList()
		{
			Add(CMRNoticeStatus.Clear);
			Add(CMRNoticeStatus.Error);
			Add(CMRNoticeStatus.Rejected);
			Add(CMRNoticeStatus.Withdrawn);
		}
	}
}
