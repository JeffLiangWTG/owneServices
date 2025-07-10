using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateWorkflowAction : CreateWorkflowActionBase
	{
		public CreateWorkflowAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return base.IsApplicableCore(shape).Union(BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape));
		}

		protected override string IconName => (NoResString)"Workflow"; // resource name
	}
}
