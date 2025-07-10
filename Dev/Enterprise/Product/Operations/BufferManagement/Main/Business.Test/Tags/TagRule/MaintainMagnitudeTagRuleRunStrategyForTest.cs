using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	public class MaintainMagnitudeTagRuleRunStrategyForTest : MaintainMagnitudeTagRuleRunStrategy
	{
		public MaintainMagnitudeTagRuleRunStrategyForTest(IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
		}

		protected override double GetFirstRunTime(TagRule rule, ZDateTime performanceTimestampStart)
		{
			if (rule.FirstRunTimeForTest != 0.0)
			{
				return rule.FirstRunTimeForTest;
			}

			return base.GetFirstRunTime(rule, performanceTimestampStart);
		}
	}
}
