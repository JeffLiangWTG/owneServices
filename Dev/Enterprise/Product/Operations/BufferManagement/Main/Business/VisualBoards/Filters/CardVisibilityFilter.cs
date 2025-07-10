using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public abstract class CardVisibilityFilter : BoardFilterBase, ICardVisibilityFilter, IUndoableFilter
	{
		public sealed override bool RequiresRedraw
		{
			get { return false; }
		}

		public override bool RequiresRemoval { get; set; }

		public virtual bool RequiresUndo { get; set; }

		public abstract CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory);

		public abstract void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards);
	}
}
