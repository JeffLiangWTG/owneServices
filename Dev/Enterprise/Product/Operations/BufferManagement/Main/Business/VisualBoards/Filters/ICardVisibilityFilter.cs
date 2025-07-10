using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public delegate bool CellVisibilityApplicator(ICardContent card, CellContent cell);

	public interface ICardVisibilityFilter
	{
		void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards);

		CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory);
	}
}
