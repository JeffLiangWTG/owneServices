using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class PushAllEntitiesAction : JobNetworkAction
	{
		public PushAllEntitiesAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new CommonNetworkActionExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Overrides

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			throw new InvalidOperationException("Child actions should be called");
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("e73b5622-c03b-466d-9ed7-82d7e59fb958", "Push All Entities");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ActionDescription;
		}

		protected override IEnumerable<INetworkAction> GetDefaultChildActionsCore()
		{
			yield return new PushEntityChildAction(NetworkViewModel, PushDirection.Early, ShouldUpdateOnNetworkEvents);
			yield return new PushEntityChildAction(NetworkViewModel, PushDirection.Late, ShouldUpdateOnNetworkEvents);
		}

		internal static ResourceString ActionDescription => ResString.GetMultilingualString("82d90f5e-2d47-4c7a-a7d0-ec1f29926290", "Pushes all entities either as early or late as possible, without violating dependencies.");

		#endregion

		#region PushEntityChildAction

		public class PushEntityChildAction : JobNetworkAction
		{
			public PushEntityChildAction(INetworkViewModel networkViewModel, PushDirection direction, bool shouldUpdateOnNetworkEvents = true)
				: base(networkViewModel, new CommonNetworkActionExecutionStrategy(), shouldUpdateOnNetworkEvents: shouldUpdateOnNetworkEvents)
			{
				this.direction = direction;
			}

			readonly PushDirection direction;

			#region Overrides

			protected override void ExecuteForShape(BMNCNShape shape)
			{
				PushShapesProvider.PushAllEntities(Network, direction);
				Network.Refresh(RefreshType.RedrawDiagram);
			}

			protected override ResourceString GetDefaultNameCore()
			{
				switch (direction)
				{
					case PushDirection.Early:
						return ResString.GetMultilingualString("ea4e695d-8e56-4466-8564-95fffe374f7c", "As Early As Possible");

					case PushDirection.Late:
						return ResString.GetMultilingualString("13817010-9410-4978-9f9f-d4c471c143be", "As Late As Possible");

					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Wrong push direction: {0}.", direction));
				}
			}

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
			{
				return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
					.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape));
			}

			protected override ResourceString GetDefaultDescriptionCore() => ActionDescription;

			protected override bool CanPerformOnApprovedShape => false;

			protected override string IconName => direction == PushDirection.Early
								? "ArrowLeft"
								: "ArrowRight";

			#endregion
		}

		#endregion
	}
}
