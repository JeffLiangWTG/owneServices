using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class WorkflowCapabilityAssignerDataAccessor_ForTest : WorkflowCapabilityAssignerDataAccessor
	{
		public WorkflowCapabilityAssignerDataAccessor_ForTest(ILogger logger)
			: base(logger)
		{
		}

		public List<ZGuid> WorkflowPKs { get; private set; } = new List<ZGuid>();

		protected override ZQuery GetWorkflowsQueryCore(BMComponent component)
		{
			var query = base.GetWorkflowsQueryCore(component);

			if (WorkflowPKs.Count > 0)
			{
				query.AddToFilter(ProcessHeaderSchema.PK, WorkflowPKs.ToArray());
			}

			query.OrderBy = ProcessHeaderSchema.FH_CompletionStatement.Name;

			return query;
		}
	}
}
