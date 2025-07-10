using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class JobOrWorkflowFilter : ModuleFlagsFilter, IJobOrWorkflowFilter
	{
		public JobOrWorkflowFilter()
			: base(ProcessHeader.ModuleFilterConstants.JobOrWorkflow, Options, Queries)
		{
			ArePropertiesMutuallyExclusive = true;
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|JobOrWorkflow", "Job or Workflow");
		}

		static string[] Options => new[]
		{
			Res.GetString("b26e69e8-b945-4164-b893-e9564d6c3899", "Job"),
			Res.GetString("b0ea0529-b3f4-4280-91bc-c40f2ab63b51", "Workflow"),
			Res.GetString("d3fbba38-0405-4843-a141-7dcdabd7b6ea", "Job AND Workflow")
		};

		static GetFlagsQuery[] Queries => new GetFlagsQuery[]
		{
			GetJobFlagQuery,
			GetWorkflowFlagQuery,
			GetJobAndWorkflowFlagQuery,
		};

		static ZQuery GetJobFlagQuery(ZBool value)
		{
			return value ? new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, null) : new ZQuery();
		}

		static ZQuery GetWorkflowFlagQuery(ZBool value)
		{
			return value ? new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null) : new ZQuery();
		}

		static ZQuery GetJobAndWorkflowFlagQuery(ZBool value)
		{
			return new ZQuery();
		}

		void InvalidateAllOtherFilters()
		{
			if (FilterBusinessObject != null)
			{
				FilterBusinessObject.ModuleFilters.InvalidateCachedQuery();

				foreach (var filter in FilterBusinessObject.ModuleFilters.Where(x => !x.IsQueryStale))
				{
					filter.InvalidateCachedQuery();
				}
			}
		}

		protected override void OnIsActiveChanged()
		{
			base.OnIsActiveChanged();

			InvalidateAllOtherFilters();
		}

		protected override void OnCachedQueryInvalidated()
		{
			base.OnCachedQueryInvalidated();

			InvalidateAllOtherFilters();
		}

		#region Shortcut Methods

		public void SetJobOnly()
		{
			Property0 = true;
		}

		public void SetWorkflowOnly()
		{
			Property1 = true;
		}

		public void SetJobAndWorkflow()
		{
			Property2 = true;
		}

		public bool IsJobOnly => Property0;

		public bool IsWorkflowOnly => Property1;

		public bool IsJobAndWorkflow => Property2;

		#endregion
	}
}
