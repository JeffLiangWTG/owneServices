using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class UnpinShapeAction : JobNetworkAction
	{
		public UnpinShapeAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckEntityIsNotRoot(entity))
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeIsNotAnnotation(shape))
					.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeIsNotApproved(shape)
						.Union(new NetworkActionAccessibility(NetworkViewModel.SelectedEntities.Count() > 1 || entity.IsPinned,
							shape,
							GetShapeShouldBePinnedMessage))));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return new NetworkActionAccessibility(entity.IsPinned,
				() => new NetworkActionDenialReason(shape,
					GetShapeShouldBePinnedMessage(),
					needsNotification: false));
		}

		string GetShapeShouldBePinnedMessage() => Res.GetString("8FD718FE-1B75-494B-9C38-73A98BD7E557", "The shape should be pinned.");

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			using (shape.AllowChangingPinnedStatus())
			{
				shape.IsPinned = false;
			}
			Network.Entities.GetInstance(shape).UnsetFixedCoordinatesAndUpdateParentRelativePosition();
			Network.Refresh(RefreshType.EntityPinnedOrUnpinned, new INetworkEntity[] { shape });
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("e46b16b4-a8df-425a-a9ab-3fc4d22b15c3", "Unpin Shape");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("dc06123f-dde0-404e-be22-4391d3b699cc", "Removes pinned status.");
		}

		protected override string IconName => (NoResString)"Unpin"; // resource name
	}
}
