using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class LinkToDiagramAction : JobNetworkAction
	{
		public LinkToDiagramAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Network Action Attributes

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("c70eb170-f538-4e91-a213-f43899565457", "Select Diagram...");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("a426a5a8-0779-4331-969e-c8465c140662", "Choose a record from the Network Diagrams module to which this shape will be linked.");

		protected override string IconName => (NoResString)"Link"; // resource name

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckShapeIsNormalDiagramOrChild(shape);

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckIsShape(shape);

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			NetworkViewModel.GetJobNetwork().LinkEntity(shape, ModuleIDs.NetworkDiagram);
		}

		#endregion
	}
}
