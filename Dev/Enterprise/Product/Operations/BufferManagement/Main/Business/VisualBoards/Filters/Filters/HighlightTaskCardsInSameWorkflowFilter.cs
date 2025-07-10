using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class HighlightTaskCardsInSameWorkflowFilter : CardVisibilityFilter
	{
		public HighlightTaskCardsInSameWorkflowFilter(ProcessHeader sourceProcessHeader)
		{
			Argument.NotNull(sourceProcessHeader, "sourceProcessHeader");

			this.sourceProcessHeader = sourceProcessHeader;
		}

		readonly ProcessHeader sourceProcessHeader;

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			// Nothing to fetch.
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory = null)
		{
			return (cardContent, cell) => cardContent.WorkflowIdentifier == sourceProcessHeader.PK;
		}

		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode() ^ sourceProcessHeader.PK.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var filter = obj as HighlightTaskCardsInSameWorkflowFilter;
			return filter != null && filter.sourceProcessHeader.PK == sourceProcessHeader.PK;
		}

		public override bool AllowMultiple
		{
			get { return false; }
		}

		public override string FilterName
		{
			get { return Res.GetString("6e9968dd-f6ac-4451-a4cd-d8a132797b45", "Highlighting tasks in workflow {0}", sourceProcessHeader.Code); }
		}
	}
}
