using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowComponentsData
	{
		public WorkflowComponentsData(ICollection<ZQuery> taskSubQueries, ZQuery workflowSectionFilter, ICollection<ZGuid> components)
		{
			TaskSubQueries = taskSubQueries;
			ComponentPKs = components;
			WorkflowSectionFilter = workflowSectionFilter;
		}

		internal ZGuid ReleaseGroupPK { get; set; }
		internal ICollection<ZGuid> ComponentPKs { get; }
		internal ICollection<ZQuery> TaskSubQueries { get; }
		internal ZQuery WorkflowSectionFilter { get; }

		internal bool HasTaskQuery => TaskSubQueries != null && TaskSubQueries.Any(t => !t.IsEmpty);
	}
}
