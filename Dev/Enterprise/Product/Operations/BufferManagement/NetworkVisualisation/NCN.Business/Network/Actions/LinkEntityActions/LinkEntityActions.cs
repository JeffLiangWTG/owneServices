using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class LinkEntityActions : JobNetworkAction
	{
		public LinkEntityActions(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0)
			: base(networkViewModel, new CommonNetworkActionExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents: false)
		{
		}

		#region Main Network Action Attributes

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("426541ff-5f3f-454e-b49d-99a4cba0d311", "Linked Entity");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("ef54224c-f2b2-44cc-9716-3d12ec4be73f", "The open or change the linked business entity this shape represents");

		protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape)
		{
			var group = 0;
			yield return new LinkFromClipboardAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new LinkToWorkflowAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new LinkToDiagramAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);

			group++;
			yield return new OpenLinkedEntityAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new UnlinkEntityAction(NetworkViewModel, group, shouldUpdateOnNetworkEvents: false);
		}

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape) => BMNetworkActionAccessibilityHelper.CheckShapeIsNormalDiagramOrChild(shape);

		#endregion

		#region Execution

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			//it is just a container for other network actions
		}

		#endregion
	}
}
