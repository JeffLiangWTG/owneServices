using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class DemoteWorkflowMenuItem : ZMenuItem
	{
		public DemoteWorkflowMenuItem(ZGrid workflowsGrid, ZString currentWorkflowType, IEnumerable<ProcessHeader> parentProcessHeaders)
			: base(Res.GetString("58565a94-2fb3-4208-9bb5-b41ffbc9f79a", "Demote to Workflow"))
		{
			this.workflowsGrid = workflowsGrid;

			InitParentMenu(parentProcessHeaders);

			MenuItems.Add(new DemoteWorkflowByJobTypeMenuItem(workflowsGrid, currentWorkflowType));
		}

		readonly ZGrid workflowsGrid;

		void InitParentMenu(IEnumerable<ProcessHeader> parentWorkflows)
		{
			var parents = new ZMenuItem(Res.GetString("00d72e96-b913-4074-9a47-920c16cc93f5", "Within Parent Job"));

			foreach (var processHeader in parentWorkflows.OrderBy(p => p.ParentJobDescription))
			{
				parents.MenuItems.Add(new ZMenuItem(string.Format(CultureInfo.InvariantCulture, "{0}", processHeader.ProviderJobDescription), ParentHandler(processHeader)));
			}

			MenuItems.Add(parents);
		}

		EventHandler ParentHandler(ProcessHeader menuItemHeader)
		{
			return (s, e) =>
			{
				var jobHeader = DemoteWorkflowByJobTypeMenuItem.GetJobHeaderToDemote(workflowsGrid);
				if (jobHeader != null)
				{
					jobHeader.Demote(menuItemHeader.Parent, jobHeader.Factory);
				}
			};
		}
	}
}
