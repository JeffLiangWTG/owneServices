using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessJobHeaderProvider
	{
		IProcessJobHeader GetForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true, bool checkTemplates = true);
		IProcessJobHeader GetForParentWithoutCreation(IWorkflowProviderCore parent, BusinessObjectFactory factory);
		IProcessJobHeader GetForTemplate(IProcessTaskTemplate template, IWorkflowProviderCore parent);
		bool SupportsPAVE(string workflowType, BusinessObjectFactory factory);
		IProcessHeader GetDefaultWorkflowForTask(IWorkflowTask task);
		bool BufferManagementEnabledForWorkflowProvider(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory);
		IProcessHeaderCollection GetWorkflowsForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory);
		IProcessHeaderCollection GetWorkflowsForProcessTaskCollection(IProcessTaskCollection parent, BusinessObjectFactory factory);
	}
}
