using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.GUI.Test
{
	/// <summary>
	/// We changed the current tasks filter to just grey out non-current tasks. Using this filter to preserve the functionality
	/// and tests around TaskVisibilityFilter.
	/// </summary>
	class HideCurrentTasksFilter : CardVisibilityFilter
	{
		public override bool AllowMultiple
		{
			get { return false; }
		}

		public override string FilterName
		{
			get { return "Hide Current Tasks"; }
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			// Ought to already be fetched.
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) => cardContent.IsCurrent;
		}
	}
}
