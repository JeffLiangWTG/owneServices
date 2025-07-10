using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateResourceDependencyAction : JobNetworkAction
	{
		public CreateResourceDependencyAction(INetworkViewModel networkViewModel, DependencyDirection direction, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new CommonNetworkActionExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
			this.direction = direction;
		}

		readonly DependencyDirection direction;

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
			var otherShape = GetOtherSelectedShape(shape, NetworkViewModel.SelectedEntities);
			return new NetworkActionAccessibility(
				otherShape != null,
				shape,
				() => Res.GetString("C4531924-989F-4076-B1CE-9A1B79F089B4", "Two entities must be selected."))
				.UnionIfAllowed(() => new NetworkActionAccessibility(
					GetExistingDependency(Network, shape, otherShape) == null,
					shape,
					() => Res.GetString("13a7deb0-7844-4834-ace3-35757ad6a897", "A dependency already exists between these two shapes."))
				);
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			var otherShape = GetOtherSelectedShape(shape, NetworkViewModel.SelectedEntities);
			ExecuteForShapes(shape, otherShape);
		}

#if DEBUG
		public
#endif
		void ExecuteForShapes(BMNCNShape shape1, BMNCNShape shape2)
		{
			var fromShape = GetFrom(shape1, shape2);
			var toShape = GetTo(shape1, shape2);

			var fromEntity = Network.Entities.GetInstance(fromShape);
			var toEntity = Network.Entities.GetInstance(toShape);

			var ownerShape = JobNetwork.GetOwnerForRelationship(fromEntity, toEntity, Network.DiagramEntity);

			var attachment = shape1.Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_FromShape = fromShape.PK;
			attachment.BNA_BNS_ToShape = toShape.PK;
			attachment.BNA_BNS_Owner = ownerShape.PK;
			attachment.BNA_Type = AttachmentTypeList.Codes.ResourceDependency;

			var earliestAllowedStartPosition = fromEntity.X + fromEntity.Width;
			if (toEntity.X < earliestAllowedStartPosition)
			{
				toEntity.X = (int)earliestAllowedStartPosition;
			}

			Network.Refresh(RefreshType.ResourceDependencyAdded, fromShape, toShape);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("f64ffe2e-cdd2-4aa7-80bd-6db272363836", "Create Resource Dependency");
		}

		protected override ResourceString GetNameCore(BMNCNShape shape)
		{
			var otherShape = GetOtherSelectedShape(shape, NetworkViewModel.SelectedEntities);
			if (otherShape != null)
			{
				var fromShape = GetFrom(shape, otherShape);
				var toShape = GetTo(shape, otherShape);

				return ResString.GetMultilingualString("b8030534-7b5d-48bb-a3bb-74fca89fa541", "Create Resource Dependency from [{0}] to [{1}]", fromShape.Name, toShape.Name);
			}
			else
			{
				return GetDefaultNameCore();
			}
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("6a93ff97-6383-4e74-843f-9d11d4e61b62", "Adds a resource dependency between two selected shapes.");
		}

		protected override string IconName => "ArrowRight";

		#endregion

		#region Implementation

		static BMNCNShape GetOtherSelectedShape(BMNCNShape lastSelectedShape, IEnumerable<INetworkEntity> selectedEntities)
		{
			return selectedEntities.Count() == 2 && !selectedEntities.All(s => s.EntityPK != lastSelectedShape.PK) ? selectedEntities.Single(s => s.EntityPK != lastSelectedShape.PK).AsShape() : null;
		}

		BMNCNShape GetFrom(BMNCNShape first, BMNCNShape second)
		{
			return direction == DependencyDirection.Uniform
				? GetLeftOrTopShape(first, second)
				: direction == DependencyDirection.PreRequisite
					? first
					: second;
		}

		BMNCNShape GetLeftOrTopShape(BMNCNShape first, BMNCNShape second)
		{
			return first.Left != second.Left
				? first.Left < second.Left ? first : second
				: first.Top <= second.Top ? first : second;
		}

		BMNCNShape GetTo(BMNCNShape first, BMNCNShape second)
		{
			return first == GetFrom(first, second) ? second : first;
		}

		static BMNCNAttachment GetExistingDependency(IJobNetwork network, BMNCNShape shape, BMNCNShape otherShape)
		{
			var ownerShape = JobNetwork.GetOwnerForRelationship(network.Entities.GetInstance(otherShape), network.Entities.GetInstance(shape), network.DiagramEntity);
			var shapes = new[]
			{
				shape.PK, otherShape.PK
			};

			var query = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, ownerShape.PK);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_FromShape, shapes);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_ToShape, shapes);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_Type, new[]
			{
				AttachmentTypeList.Codes.Dependency, AttachmentTypeList.Codes.ResourceDependency
			});
			return ownerShape.Factory.LoadTop1<BMNCNAttachment>(query);
		}

		#endregion
	}
}
