using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CoreCustomActions : JobNetworkAction
	{
		public CoreCustomActions(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0)
			: base(networkViewModel, new CommonNetworkActionExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents: false)
		{
		}

		#region Main Network Action Attributes

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("edd3157d-2b6c-4c14-a507-886655ec9b39", "Actions");

		protected override ResourceString GetDefaultDescriptionCore() => null;

		protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape) => GetCoreCustomNetworkActions();

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => new NetworkActionAccessibility(
			GetCoreCustomNetworkActions().Any(a => a.IsApplicableToEntity(shape).IsAllowed),
			shape,
			() => Res.GetString("2B1B0382-4E4D-4333-9A6A-76587916C931", "There are no custom actions defined for this entity."));

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			//it is just a container for other network actions
		}

		#endregion

		#region Implementation

		IEnumerable<JobNetworkActionBase> GetCoreCustomNetworkActions()
		{
			int group = 0;
			yield return new ExtendedNetworkAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new CloneDiagramAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new OpenShapeListAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new CreateJobAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false, linkOnly: true);
			yield return new ConvertToWorkflowsAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new SetStatusAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new ValidateWorkflowLoopsAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			group++;
			yield return new AcceptBufferAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			yield return new UnapproveDiagramAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new ApproveDiagramAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new ApproveNonApprovedShapesAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			yield return new SwitchToScaledModeAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			yield return new ToggleResourceDependencyVisibilityAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			yield return new SuggestBufferAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new AddBufferAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new CreateProjectBufferAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new DecoupleAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			yield return new PinShapeAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new UnpinShapeAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new MoveToOtherSectionAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			yield return new CreateResourceDependencyAction(NetworkViewModel, DependencyDirection.PreRequisite, group, shouldUpdateOnNetworkEvents: false);
			yield return new CreateResourceDependencyAction(NetworkViewModel, DependencyDirection.PostRequisite, group, shouldUpdateOnNetworkEvents: false);

			yield return new PushAllEntitiesAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
		}

		#endregion
	}
}
