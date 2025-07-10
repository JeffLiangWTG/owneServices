using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateWorkItemAction : CreateJobActionBase
	{
		public CreateWorkItemAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return base.IsEnabledCore(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckCanDoActionOnJobType(shape, WorkflowDescriptors.WorkItemWorkflowDescriptorCode));
		}

		protected override ProcessHeader GetNewProcessHeaderForShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			newShape.BNS_JobType = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;
			return base.GetNewProcessHeaderForShape(network, newShape, parentShape);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("e1b5e84c-21c4-450d-9699-484f3bcaafc7", "Create Work Item");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("1cbd8550-c8f6-4717-850b-0180c726fcc4", "Creates and opens a new work item");
		}
	}
}
