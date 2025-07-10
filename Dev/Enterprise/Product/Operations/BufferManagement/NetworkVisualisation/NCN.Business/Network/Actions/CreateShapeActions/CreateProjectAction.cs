using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateProjectAction : CreateJobActionBase
	{
		public CreateProjectAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return base.IsEnabledCore(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckCanDoActionOnJobType(shape, WorkflowDescriptors.ProjectWorkflowDescriptorCode));
		}

		protected override ProcessHeader GetNewProcessHeaderForShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			newShape.BNS_JobType = WorkflowDescriptors.ProjectWorkflowDescriptorCode;
			return base.GetNewProcessHeaderForShape(network, newShape, parentShape);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("0fd4b005-35f4-4d28-96dc-2ae61dba44c9", "Create Project");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("a89a6cd2-4a13-42ad-b2ba-178112356d28", "Creates and opens a new project");
		}
	}
}
