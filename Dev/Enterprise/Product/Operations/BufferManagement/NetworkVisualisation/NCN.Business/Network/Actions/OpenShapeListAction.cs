using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class OpenShapeListAction : JobNetworkAction
	{
		public OpenShapeListAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(Network.Entities.GetInstance(shape)));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape) => NetworkActionAccessibility.Allowed;

		protected override INetworkActionAccessibility PerformPreExecutionChecksForShape(BMNCNShape shape) => NetworkActionAccessibility.Allowed;

		protected override bool RequiresSaveBeforeExecute => true;

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("d7c64d2c-2167-48fc-93f2-096dcc18c681", "Open Shape List");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("851bb0c0-40d0-4a2f-99fa-f47c09f37e1f", "Opens a Network Diagrams module popup containing a list of shapes within this diagram.");
		}

		protected override string IconName => "ShapeStack";

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			NetworkViewModel.GetJobNetwork().ShowNetworkDiagramsModule(shape);
		}
	}
}
