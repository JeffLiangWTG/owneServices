using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class AddAndRemoveTagRuleRunSqlStrategy : TagRuleRunStrategyBase
	{
		public AddAndRemoveTagRuleRunSqlStrategy(IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
		}

		public override long Execute(TagRule rule)
		{
			long l = RemoveTagsFromNonMatchingWorkflows(rule) + AddTagsToMatchingWorkflows(rule);
			if (rule.ShouldRunPerformanceVerification)
			{
				rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
			}
			return l;
		}

		protected override void ToggleSingleThreadedBit(TagRule rule)
		{
			throw new NotImplementedException();
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			if (asSubQuery)
			{
				throw new NotImplementedException();
			}

			var superQuery = new ZDBOnlyQuery(typeof(ProcessHeader));

			var addQuery = new AddTagRuleRunSqlStrategy(ConnectionProvider, Logger).GetAffectedWorkflowsQuery(rule, true) as ZDBOnlySubQuery
				?? throw new InvalidOperationException();

			var removeQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			removeQuery.AddToFilter(new RemoveTagRuleRunSqlStrategy(ConnectionProvider, Logger) { GetNonMatchingTagLinks = true }.GetAffectedWorkflowsQuery(rule));

			addQuery.AddAsUnionQuery(removeQuery, true);
			superQuery.AddSubQuery(addQuery, JoinCondition.And);

			return superQuery;
		}

		protected virtual long AddTagsToMatchingWorkflows(TagRule rule)
		{
			return new AddTagRuleRunSqlStrategy(ConnectionProvider, Logger).Execute(rule);
		}

		protected virtual long RemoveTagsFromNonMatchingWorkflows(TagRule rule)
		{
			var removeTagRuleRunSqlStrategy = new RemoveTagRuleRunSqlStrategy(ConnectionProvider, Logger);
			removeTagRuleRunSqlStrategy.GetNonMatchingTagLinks = true;

			return removeTagRuleRunSqlStrategy.Execute(rule);
		}
	}
}
