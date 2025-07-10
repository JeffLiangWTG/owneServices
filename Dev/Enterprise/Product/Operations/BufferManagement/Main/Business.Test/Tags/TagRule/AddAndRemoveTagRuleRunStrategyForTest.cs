using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business.Test
{
	public class AddAndRemoveTagRuleRunSqlStrategyForTest : AddAndRemoveTagRuleRunSqlStrategy
	{
		public AddAndRemoveTagRuleRunSqlStrategyForTest(IConnectionProvider connectionProvider, ILogger logger)
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

		protected override long AddTagsToMatchingWorkflows(TagRule rule)
		{
			return new AddTagRuleRunSqlStrategyForTest(ConnectionProvider, Logger).Execute(rule);
		}

		protected override long RemoveTagsFromNonMatchingWorkflows(TagRule rule)
		{
			var removeTagRuleRunSqlStrategy = new RemoveTagRuleRunSqlStrategyForTest(ConnectionProvider, Logger);
			removeTagRuleRunSqlStrategy.GetNonMatchingTagLinks = true;

			return removeTagRuleRunSqlStrategy.Execute(rule);
		}
	}
}
