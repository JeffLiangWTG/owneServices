using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class AddAndRemoveTagRuleBizoStrategy : BusinessObjectTagRuleRunStrategyBase
	{
		internal AddAndRemoveTagRuleBizoStrategy(BMServiceTaskProcessor processsHeaderProcessor, IConnectionProvider connectionProvider, ILogger logger)
			: base(processsHeaderProcessor, connectionProvider, logger)
		{
		}

		public override long Execute(TagRule rule)
		{
			return RemoveTagsFromNonMatchingWorkflows(rule) + AddTagsToMatchingWorkflows(rule);
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			if (asSubQuery)
			{
				throw new NotImplementedException();
			}

			var superQuery = new ZDBOnlyQuery(typeof(ProcessHeader));

			var addQuery = new AddToQueueRuleRunBizoStrategy(processor, ConnectionProvider, Logger).GetAffectedWorkflowsQuery(rule, true) as ZDBOnlySubQuery
				?? throw new InvalidOperationException();

			var removeQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			removeQuery.AddToFilter(new RemoveTagRuleRunBizoStrategy(processor, ConnectionProvider, Logger) { GetNonMatchingTagLinks = true }.GetAffectedWorkflowsQuery(rule));

			addQuery.AddAsUnionQuery(removeQuery, true);
			superQuery.AddSubQuery(addQuery, JoinCondition.And);

			return superQuery;
		}

		long AddTagsToMatchingWorkflows(TagRule rule)
		{
			return new AddToQueueRuleRunBizoStrategy(this.processor, ConnectionProvider, Logger).Execute(rule);
		}

		long RemoveTagsFromNonMatchingWorkflows(TagRule rule)
		{
			var removeTagRuleRunBizoStrategy = new RemoveTagRuleRunBizoStrategy(processor, ConnectionProvider, Logger);
			removeTagRuleRunBizoStrategy.GetNonMatchingTagLinks = true;

			return removeTagRuleRunBizoStrategy.Execute(rule);
		}
	}
}
