using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class CurrentTaskFilter : CardVisibilityFilter
	{
		public override bool AllowMultiple
		{
			get { return false; }
		}

		public override string FilterName
		{
			get { return Res.GetString("ccee02f9-f7e3-4910-93a3-aa2d09145344", "Show only startable tasks"); }
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			// Will be cached already.
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) => cardContent.IsCurrent;
		}

		public override bool Equals(object obj)
		{
			return obj is CurrentTaskFilter;
		}

		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode();
		}
	}
}
