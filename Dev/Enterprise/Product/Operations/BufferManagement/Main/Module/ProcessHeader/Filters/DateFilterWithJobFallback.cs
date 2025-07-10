using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class DateFilterWithJobFallback : ModuleDateFilter
	{
		internal DateFilterWithJobFallback(ZString description, SchemaDateTimeColumn filterColumn, bool convertFromLocalToUTC, MultilingualString multilingualDescription)
			: base(description, filterColumn, convertFromLocalToUTC)
		{
			MultilingualDescription = multilingualDescription;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZQuery();
			AddFilterParameters(query);

			if (!BMFilterStripsHelper.IsJobOrWorkflowFilterPresentInGroupWithJobOnlySelected(this))
			{
				var workflowQuery = new ZDBOnlyQuery(typeof(ProcessHeader));
				var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);

				AddFilterParameters(jobHeaderSubQuery);

				if (IsPropertySearchUsingHasNoDateEntered)
				{
					workflowQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
				}
				else if (!BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this))
				{
					workflowQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null);
				}

				workflowQuery.AddToFilter(FilterColumn, SQLComparisonOperator.Equal, null);

				// Either this is a job and has no parent, OR it is a workflow with a parent, and matches the conditions (if we fall back to it)
				workflowQuery.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeaderSubQuery, IsPropertySearchUsingHasNoDateEntered ? JoinCondition.Or : JoinCondition.And);
				query.AddToFilter(workflowQuery, IsPropertySearchUsingHasNoDateEntered ? JoinCondition.And : JoinCondition.Or);
			}

			return query;
		}
	}
}
