using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	abstract class AddTagRuleRunBizoStrategy : BusinessObjectTagRuleRunStrategyBase
	{
		internal AddTagRuleRunBizoStrategy(BMServiceTaskProcessor processHeaderProcessor, IConnectionProvider connectionProvider, ILogger logger)
			: base(processHeaderProcessor, connectionProvider, logger)
		{
		}

		public override long Execute(TagRule rule)
		{
			var rowsProcessed = 0L;
			var query = GetAffectedWorkflowsQuery(rule);
			query.OrderBy = OrderByClause;

			BusinessObject lastBizoRead = null;
			IEnumerable<ProcessHeader> batch;

			var reader = new FilteredBusinessObjectReader(processor.FactoryProvider.InnerProvider, query, typeof(ProcessHeader)) { BatchSize = BatchSize };

			while ((batch = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<ProcessHeader>()).Any())
			{
				var templateQuery = new ZQuery(TagLinkSchema.TGL_ParentId, rule.PK);
				templateQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);
				var tagTemplate = reader.Factory.LoadTop1<TagLinkTemplate>(templateQuery);

				if (tagTemplate == null)
				{
					ErrorReporter.ReportOnce("TagLinkTemplateNonExistant", string.Format(CultureInfo.InvariantCulture, "There is a TagRule with no related Template. TagRule: [{0}]", rule.TGR_Name));
				}
				else
				{
					foreach (var workflow in batch)
					{
						tagTemplate.AddTag(workflow, rule);
					}

					rowsProcessed += batch.Count();
					processor.FactoryProvider.Save(createNew: true);
				}

				lastBizoRead = batch.LastOrDefault();
			}

			return rowsProcessed;
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			return GetWorkflowsToTagQuery(rule, asSubQuery);
		}

		protected virtual string OrderByClause
		{
			get { return string.Empty; }
		}
	}
}
