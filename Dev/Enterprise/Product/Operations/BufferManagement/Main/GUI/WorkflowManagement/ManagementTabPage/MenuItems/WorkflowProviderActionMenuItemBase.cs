using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public abstract class WorkflowProviderActionMenuItemBase : ZMenuItem
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected WorkflowProviderActionMenuItemBase(ZGrid workflowsGrid, ZString currentWorkflowType, ZString name)
			: base(name)
		{
			this.workflowsGrid = workflowsGrid;

			InitSubMenu(currentWorkflowType);
		}

		void InitSubMenu(ZString currentWorkflowTypeCode)
		{
			var systems = new BusinessObjectFactory { NameForDebugging = string.Format(CultureInfo.InvariantCulture, "Get Systems for {0}", Name) }.Load<BMSystem>(new ZQuery());
			var relatedWorkflowTypes = systems.SelectMany(system => system.RelatedWorkflowTypes).Distinct().ToArray();
			var currentWorkflowType = relatedWorkflowTypes.FirstOrDefault(t => t.FSW_WorkflowType == currentWorkflowTypeCode);

			if (currentWorkflowType != null)
			{
				MenuItems.Add(new ZMenuItem(currentWorkflowType.WorkflowTypeDescription, MenuItemHandler(currentWorkflowType)));
			}

			var otherJobTypes = new ZMenuItem(Res.GetString("4a97db0c-53a3-4101-898f-e8c4a1d2db9e", "Other Job Types"));
			MenuItems.Add(otherJobTypes);

			foreach (var workflowType in relatedWorkflowTypes.Where(t => t.FSW_WorkflowType != currentWorkflowTypeCode).OrderBy(t => t.WorkflowTypeDescription))
			{
				otherJobTypes.MenuItems.Add(new ZMenuItem(workflowType.WorkflowTypeDescription, MenuItemHandler(workflowType)));
			}
		}

		protected readonly ZGrid workflowsGrid;

		protected abstract EventHandler MenuItemHandler(BMSystemWorkflowDeterminer workflowType);
	}
}
