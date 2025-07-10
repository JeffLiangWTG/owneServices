using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class RemoveTagRuleRunBizoStrategy : BusinessObjectTagRuleRunStrategyBase
	{
		internal RemoveTagRuleRunBizoStrategy(BMServiceTaskProcessor processsHeaderProcessor, IConnectionProvider connectionProvider, ILogger logger)
			: base(processsHeaderProcessor, connectionProvider, logger)
		{
		}

		internal bool GetNonMatchingTagLinks { get; set; }

		public override long Execute(TagRule rule)
		{
			var rowsProcessed = 0L;

			var query = GetTagLinksToDeleteQuery(rule, GetNonMatchingTagLinks);
			query.AddToFilter(TagLinkSchema.TGL_ParentTableCode, "FH");

			BusinessObject lastBizoRead = null;
			IEnumerable<RuleRunnerTagLink> batch;

			var reader = new FilteredBusinessObjectReader(processor.FactoryProvider.InnerProvider, query, typeof(RuleRunnerTagLink)) { BatchSize = BatchSize };

			while ((batch = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<RuleRunnerTagLink>()).Any())
			{
				foreach (var tagLink in batch)
				{
					tagLink.MutatedByTagRulePK = rule.PK;
					tagLink.Delete(); // Loading objects just to delete since we log this in C# land
					rowsProcessed++;
				}

				processor.FactoryProvider.Save(createNew: true);

				lastBizoRead = batch.LastOrDefault();
			}

			return rowsProcessed;
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			if (asSubQuery)
			{
				throw new NotImplementedException();
			}

			return GetWorkflowsDeleteTagLinkQuery(rule, GetNonMatchingTagLinks);
		}
	}
}
