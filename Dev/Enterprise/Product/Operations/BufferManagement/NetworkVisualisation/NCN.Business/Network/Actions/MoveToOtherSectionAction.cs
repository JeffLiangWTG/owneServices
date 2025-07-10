using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class MoveToOtherSectionAction : JobNetworkActionBase
	{
		public MoveToOtherSectionAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, new MultipleSelectedEntitiesExecutionStrategy(), group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		bool IsNonScheduled => ((MultipleSelectedEntitiesExecutionStrategy)ExecutionStrategy).GetEntitiesToExecute(this).FirstOrDefault()?.IsNonScheduled ?? false;

		protected override ResourceString GetDefaultNameCore()
		{
			return IsNonScheduled
				? ResString.GetMultilingualString("220e7168-71ae-4b85-8346-c5c66c822c66", "Move to Scheduled Section")
				: ResString.GetMultilingualString("cc1ccc87-d7ba-4bff-92cc-f83d00eae0ba", "Move to Non-Scheduled Section");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return IsNonScheduled
				? ResString.GetMultilingualString("c4fb2430-6362-4b8e-9f5d-109cf8e456c3", "Moves this item to the scheduled section of the diagram.")
				: ResString.GetMultilingualString("c7e1a163-f110-440f-a3a2-1a568ad6a8e5", "Moves this item to the non-scheduled section of the diagram.");
		}

		protected override INetworkActionResult ExecuteForEntityCore(INetworkEntity entity)
		{
			var executionStrategy = ExecutionStrategy as MultipleSelectedEntitiesExecutionStrategy;
			var selectedShapes = executionStrategy.GetEntitiesToExecute(this).Select(e => e.AsShape()).ToArray();
			var shape = entity.AsShape();

			var attachments = GetAttachmentsToRemoveFromShapeAndChildShapes(shape, shape, selectedShapes);
			if (attachments.Any())
			{
				if (!executionStrategy.IsUserConfirmationForSubsequentEntitiesConfirmed)
				{
					var message = ResString.GetMultilingualString("a43ccfcc-4057-4988-9d80-e175699ff59c", @"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?");
					executionStrategy.IsExecutionForSubsequentEntitiesConfirmed = GetUserConfirmation(message);
					executionStrategy.IsUserConfirmationForSubsequentEntitiesConfirmed = true;
				}

				if (executionStrategy.IsExecutionForSubsequentEntitiesConfirmed)
				{
					attachments.ForEach(x => x.Delete());
				}
			}

			if (executionStrategy.IsExecutionForSubsequentEntitiesConfirmed)
			{
				MoveEntityAndChildren(entity);
				Network.Refresh(RefreshType.EntitiesMovedToDiagramSection, entity);
			}

			return null;
		}

		static void MoveEntityAndChildren(INetworkEntity entity)
		{
			entity.SwapNonScheduledState();

			foreach (var childEntity in entity.Children)
			{
				MoveEntityAndChildren(childEntity);
			}
		}

		IEnumerable<BMNCNAttachment> GetAttachmentsToRemoveFromShapeAndChildShapes(BMNCNShape shapeToCheckAttachments, BMNCNShape shapeToMove, BMNCNShape[] selectedShapes)
		{
			var results = new HashSet<BMNCNAttachment>();

			foreach (var attachment in shapeToCheckAttachments.DependencyAttachments.Where(x => !x.IsDeleted && !results.Contains(x)))
			{
				var attachedToShape = attachment.ToShape;
				var attachedFromShape = attachment.FromShape;

				if ((attachedToShape != null && !attachedToShape.IsDescendantOf(shapeToMove, descendantStrategy) && !selectedShapes.Contains(attachedToShape))
					|| (attachedFromShape != null && !attachedFromShape.IsDescendantOf(shapeToMove, descendantStrategy) && !selectedShapes.Contains(attachedFromShape)))
				{
					results.Add(attachment);
				}
			}

			foreach (var childShape in shapeToCheckAttachments.ChildShapes)
			{
				results.AddRange(GetAttachmentsToRemoveFromShapeAndChildShapes(childShape, shapeToMove, selectedShapes));
			}

			return results;
		}

		readonly BMNCNShapeDescendantsStrategy descendantStrategy = new BMNCNShapeDescendantsStrategy();

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);

			return BMNetworkActionAccessibilityHelper.CheckDiagramIsShowingNonScheduledSection(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckEntityIsNotRoot(entity))
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckIsShapeOrAnnotation(shape))
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckParentOfShapeIsRootDiagram(shape));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeIsNotApproved(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckShapeIsNotPinned(shape));
		}
	}
}
