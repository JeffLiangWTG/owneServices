using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.CN.Business
{
	public partial class OperationMatterList : IGroupedCodeDescriptionPairList
	{
		public IEnumerable<string> GetMutuallyExclusiveCodes(string code) => code switch
		{
			Codes.ConsolidatedDutyCollection => [Codes.AssuredInspectClearance],
			Codes.AssuredInspectClearance => [Codes.ConsolidatedDutyCollection],
			_ => Enumerable.Empty<string>()
		};

		bool IGroupedCodeDescriptionPairList.AutoUnselectExclusiveCodes => false;
	}
}
