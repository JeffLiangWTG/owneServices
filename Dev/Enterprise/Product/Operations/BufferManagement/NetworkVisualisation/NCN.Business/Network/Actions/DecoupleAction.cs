using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class DecoupleAction : JobNetworkAction
	{
		public DecoupleAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Overrides

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckEntityIsNotRoot(entity))
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape))
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeIsApproved(shape)));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(
				GetArrows(Network.Entities.GetInstance(shape)).Any(),
				shape,
				() => Res.GetString("F33C7DA2-C772-4C31-8CB9-120EFF9ABD58", "The shape should have prerequisites."));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("a05d205f-f59d-4c70-bddf-c2e23c147fd0", "Decouple");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return Tooltip;
		}

		protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			foreach (var arrow in GetArrows(entity).OrderBy(a => a.FromShape.Name))
			{
				yield return new DecoupleChildAction(NetworkViewModel, arrow, ShouldUpdateOnNetworkEvents);
			}
		}

		#endregion

		#region Implementation

		internal static ResourceString Tooltip
		{
			get { return ResString.GetMultilingualString("818c4fb6-df9f-4a0e-85c8-c7ae38562be3", "Decouples this shape from the selected prerequisite shape, making it startable without the prerequisite being complete"); }
		}

		static IEnumerable<BMNCNAttachment> GetArrows(ShapeNetworkEntity entity)
		{
			return entity.PreRequisiteLinks.Where(a => !a.Attachment.BNA_IsDecouple).Select(a => a.Attachment);
		}

		#endregion

		#region DecoupleArrowAction

		public class DecoupleChildAction : JobNetworkAction
		{
			public DecoupleChildAction(INetworkViewModel networkViewModel, BMNCNAttachment arrow, bool shouldUpdateOnNetworkEvents = true)
				: base(networkViewModel, shouldUpdateOnNetworkEvents)
			{
				this.arrow = arrow;
			}

			readonly BMNCNAttachment arrow;

			protected override void ExecuteForShape(BMNCNShape shape)
			{
				arrow.Decouple();
			}

			protected override ResourceString GetDefaultNameCore()
			{
				return ResString.GetMultilingualString("b22b477c-f2bd-463e-9546-447b5c5eddf5", "From [{0}]", arrow.FromShape.Name);
			}

			protected override ResourceString GetDefaultDescriptionCore()
			{
				return Tooltip;
			}

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
			{
				return NetworkActionAccessibility.Allowed;
			}
		}

		#endregion
	}
}
