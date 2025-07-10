using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class LandedCostDutyRateSummaryCollection : DocBaseWrapperCollection<LandedCostDutyRateSummary>
	{
		public LandedCostDutyRateSummaryCollection(DocLandedCostHistoryCollection histories, BusinessObjectFactory factory)
			: base(factory)
		{
			if (histories != null)
			{
				LoadAndSummarise(histories);
			}
		}

		void LoadAndSummarise(DocLandedCostHistoryCollection histories)
		{
			Dictionary<decimal, LandedCostDutyRateSummary> list = new Dictionary<decimal, LandedCostDutyRateSummary>();
			foreach (DocLandedCostHistory lcLine in histories)
			{
				ZDecimal dutyPercent = lcLine.DutyPercent;
				LandedCostDutyRateSummary dutySummary;
				if (!list.TryGetValue(dutyPercent, out dutySummary))
				{
					dutySummary = LandedCostDutyRateSummary.New(dutyPercent, Factory);
					Add(dutySummary);
					list[dutyPercent] = dutySummary;
				}
				dutySummary.AddHistoryLine(lcLine.LCHistory);
			}
		}
	}
}
