using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BoardMeetingModeFilterApplicator : CardVisibilityFilter
	{
		public BoardMeetingModeFilterApplicator(BoardMeetingModeFilter boardMeetingModeFilter)
		{
			filter = boardMeetingModeFilter;
		}

		readonly BoardMeetingModeFilter filter;

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) =>
			{
				var workflow = cardContent.GetWorkflow(factory);
				var component = workflow != null ? workflow.CurrentComponent : null;
				return component != null && (!component.IsBuffer || ShouldTaskBeOnBoardMeetingAgenda(factory, cardContent, cell, viewModel)); // Hides tasks that are NOT applicable to board meetings
			};
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			ConstrainedModeHelper.FetchForConstraintStatus(factory, cards);
		}

		public static bool ShouldCellBeOnBoardMeetingAgenda(CellContent cell)
		{
			return cell.Zone.HasValue && (cell.Zone == 0 || cell.Zone == 1 || (cell.Zone == 2 && cell.IsLastAgeIndexForZone));
		}

		public static bool ShouldTaskBeOnBoardMeetingAgenda(BusinessObjectFactory factory, ICardContent cardContent, CellContent cell, BMBoardSectionViewModel viewModel)
		{
			var task = cardContent.GetTask(factory);

			if (task != null)
			{
				var workflow = cardContent.GetWorkflow(factory);
				var boardGUIThreadFactory = viewModel.FactoryProvider.GetBoardGUIThreadFactory();

				switch (ConstrainedModeHelper.GetConstraintStatus(task, boardGUIThreadFactory))
				{
					case ConstraintStatus.ReadyForConstraint:
						return viewModel.Cache.GetCachedValue<bool>(task.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent);

					case ConstraintStatus.PreConstraint:
						var preConstraintPenetration = workflow.GetSubComponentBufferPenetration(ConstraintStatus.PreConstraint);
						if (preConstraintPenetration == null || ZoneCalculator.CalculateZone(preConstraintPenetration.Penetration) <= 1)
						{
							return true;
						}
						break;

					case ConstraintStatus.PostConstraint:
						var postConstraintPenetration = workflow.GetSubComponentBufferPenetration(ConstraintStatus.PostConstraint);
						if (postConstraintPenetration == null || ZoneCalculator.CalculateZone(postConstraintPenetration.Penetration) <= 1)
						{
							return true;
						}
						break;

					case ConstraintStatus.NonConstrained:
						if (ShouldCellBeOnBoardMeetingAgenda(cell))
						{
							return true;
						}
						break;
				}
			}

			return false;
		}

		public override bool AllowMultiple
		{
			get { return filter.AllowMultiple; }
		}

		public override string FilterName
		{
			get { return filter.FilterName; }
		}

		public override bool RequiresUndo
		{
			get { return filter.RequiresUndo; }
			set { filter.RequiresUndo = value; }
		}
	}
}
