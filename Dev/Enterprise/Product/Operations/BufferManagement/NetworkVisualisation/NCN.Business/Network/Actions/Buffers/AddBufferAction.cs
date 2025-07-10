using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class AddBufferAction : JobNetworkAction
	{
		public AddBufferAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
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
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape)));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(GetLinksWithoutBuffersToOtherShapes(
				Network.Entities.GetInstance(shape)).Any(),
				shape,
				() => Res.GetString("346E7D8C-8EB9-4667-B228-7A0F1530D72A", "Network should have links without buffers."));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("24c57311-2ba4-4007-9a70-c51f213822b2", "Add Buffer");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return GetTooltip();
		}

		protected override string IconName => (NoResString)"Plus"; // resource name

		protected override IEnumerable<INetworkAction> GetChildActionsCore(BMNCNShape shape)
		{
			foreach (var link in GetLinksWithoutBuffersToOtherShapes(Network.Entities.GetInstance(shape)))
			{
				yield return new AddBufferForLinkAction(NetworkViewModel, link.Attachment, ShouldUpdateOnNetworkEvents);
			}
		}

		#endregion

		#region Implementation

		static IEnumerable<NetworkAttachment> GetLinksWithoutBuffersToOtherShapes(ShapeNetworkEntity shape)
		{
			return from attachment in shape.PostRequisiteLinks
				   where !attachment.Attachment.IsResourceDependency
				   let toShape = attachment.To
				   where (toShape != null && !toShape.IsBufferShape) && attachment.GetBuffer() == null
				   select attachment;
		}

		static ResourceString GetTooltip()
		{
			return ResString.GetMultilingualString("b6b07e13-2df2-4fa5-81ab-fdc7f3c081d1", "Adds a buffer for the specified dependency link.");
		}

		#endregion

		#region AddBufferForLinkAction

		public class AddBufferForLinkAction : JobNetworkAction
		{
			public AddBufferForLinkAction(INetworkViewModel networkViewModel, BMNCNAttachment relationship, bool shouldUpdateOnNetworkEvents = true)
				: base(networkViewModel, shouldUpdateOnNetworkEvents)
			{
				dependency = Network.Entities.GetInstance(relationship);
			}

			readonly NetworkAttachment dependency;

			#region Overrides

			protected override ResourceString GetDefaultDescriptionCore()
			{
				return GetTooltip();
			}

			protected override string IconName => (NoResString)"Link"; // resource name

			protected override void ExecuteForShape(BMNCNShape shape)
			{
				dependency.CreateBuffer();

				BufferCreator.SetIsBufferedFlag(Network);

				Network.Refresh(RefreshType.RedrawDiagram);
			}

			protected override ResourceString GetDefaultNameCore()
			{
				return ResString.GetMultilingualString("3DA2F97D-B329-469F-8CF2-03FDD6877C3F", "{0}", !dependency.IsDeleted ? dependency.DisplayText : string.Empty); // dependency may be deleted when action is still in memory and updates its properties
			}

			protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
			{
				return NetworkActionAccessibility.Allowed;
			}

			protected override bool CanPerformOnApprovedShape
			{
				get { return false; }
			}

			#endregion
		}

		#endregion
	}
}
