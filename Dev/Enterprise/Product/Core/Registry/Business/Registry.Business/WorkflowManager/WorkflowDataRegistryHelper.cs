using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class WorkflowDataRegistryHelper
	{
		public static CodeDescriptionPairList GetTaskTypeList(ZString workflowType)
		{
			return WorkflowDataRegistry.Instance.TaskTypes.GetTaskTypesCodePairListFromWorkflowCode(workflowType);
		}

		public static CodeDescriptionPairList GetWorkflowDescriptorList()
		{
			return (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorListWithStandaloneTaskType>();
		}
	}
}
