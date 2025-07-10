using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateDefaultWorkflowAction : CreateWorkflowActionBase
	{
		public CreateDefaultWorkflowAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override void SetDefaultsForNewShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			base.SetDefaultsForNewShape(network, newShape, parentShape);
			newShape.BNS_ShapeType = ShapeTypeList.Codes.DefaultWorkflow;
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return base.IsApplicableCore(shape).Union(BMNetworkActionAccessibilityHelper.CheckShapeBelongsToDefaultDiagram(shape));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return NetworkActionAccessibility.Allowed;
		}
	}
}
