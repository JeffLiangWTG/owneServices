using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.GUI
{
	class WorkflowManagementTabPageProvider : IWorkflowManagementTabPageProvider
	{
		IWorkflowManagementTabPage IWorkflowManagementTabPageProvider.GetTabPage(IWorkflowProviderCore provider)
		{
			var businessObject = provider as BusinessObject;
			var factory = businessObject != null ? businessObject.Factory : new BusinessObjectFactory() { NameForDebugging = GetType().Name };

			if (ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(provider, factory))
			{
				return new WorkflowManagementTabPage();
			}

			return null;
		}
	}
}
