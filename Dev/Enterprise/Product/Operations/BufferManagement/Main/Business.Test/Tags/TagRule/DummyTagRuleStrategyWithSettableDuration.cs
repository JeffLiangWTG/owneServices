using System.Threading;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class DummyTagRuleStrategyWithSettableDuration : TagRuleRunStrategyBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		readonly int MillisecondsToWait;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public DummyTagRuleStrategyWithSettableDuration(int secondsToWait)
			: base(null, null)
		{
			MillisecondsToWait = (secondsToWait - 1) * 1000; // we use Math.Cieling to avoid numerous 0 second durations, which means we often get 11seconds when we expected 10
		}

		public override long Execute(TagRule rule)
		{
			Thread.Sleep(MillisecondsToWait);

			return 1;
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			return new ZQuery();
		}
	}
}
