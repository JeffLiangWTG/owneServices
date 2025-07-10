using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class LinkToWorkflowAction : JobNetworkAction
	{
		public LinkToWorkflowAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Network Action Attributes

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("cf4b73a7-e74c-4cc1-8585-e79442b11b25", "Select Workflow...");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("c81ff9c0-8192-4477-bed8-61e6570739a8", "Choose a record from the Job Workflows module to which this shape will be linked.");

		protected override string IconName => (NoResString)"Link"; // resource name

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckShapeIsNormalDiagramOrChild(shape);

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			NetworkViewModel.GetJobNetwork().LinkEntity(shape, ModuleIDs.ProcessHeader);
		}

		#endregion
	}
}
