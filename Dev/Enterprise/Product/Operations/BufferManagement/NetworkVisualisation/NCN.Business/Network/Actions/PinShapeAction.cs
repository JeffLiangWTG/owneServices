using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class PinShapeAction : JobNetworkAction
	{
		public PinShapeAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
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
						.Union(new NetworkActionAccessibility(NetworkViewModel.SelectedEntities.Count() > 1 || !entity.IsPinned,
							shape,
							GetShapeShouldNotBePinnedMessage))));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return new NetworkActionAccessibility(!entity.IsPinned,
				() => new NetworkActionDenialReason(shape,
					GetShapeShouldNotBePinnedMessage(),
					needsNotification: false));
		}

		string GetShapeShouldNotBePinnedMessage() => Res.GetString("F1AA1F51-B2EF-4155-84E6-D9B0A3016AA0", "The shape should not be pinned.");

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			using (shape.AllowChangingPinnedStatus())
			{
				shape.IsPinned = true;
				Network.Refresh(RefreshType.EntityPinnedOrUnpinned, new INetworkEntity[] { shape });
			}
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("22A8AE58-107F-4123-8EFA-1C9BBC0DC5B4", "Pin Shape");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("ab25b6b1-e322-450e-a4d0-caee26b8618d", "Pins this shape so it cannot be resized or moved.");
		}

		protected override string IconName => (NoResString)"Pin"; // resource name
	}
}
