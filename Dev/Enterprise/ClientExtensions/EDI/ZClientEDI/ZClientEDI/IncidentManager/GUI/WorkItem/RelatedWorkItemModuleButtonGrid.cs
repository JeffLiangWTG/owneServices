using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class RelatedWorkItemModuleButtonGrid : ZModuleButtonGrid
	{
		public RelatedWorkItemModuleButtonGrid()
		{
			ModuleID = ModuleIDs.WorkItem;
		}

		protected IWorkItemSource WorkItemSource
		{
			get { return (IWorkItemSource)Form.BusinessEntity; }
		}

		protected override IBusiness GetNewBusinessEntity(ZController controller)
		{
			var workItemController = (EDIWorkItemController)controller;
			return workItemController.GetNewAndPopulateFromSource(WorkItemSource);
		}

		internal void InternalAttachButton_Click(object sender, EventArgs e)
		{
			AttachButton_Click(sender, e);
		}
	}
}
