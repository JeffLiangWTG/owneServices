using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class SimpleCardVisibilityFilter : CardVisibilityFilter
	{
		public SimpleCardVisibilityFilter(CellVisibilityApplicator applicator, string name = "Sempel Feelter", bool allowMultiple = false)
		{
			this.applicator = applicator;
			this.allowMultiple = allowMultiple;
			this.name = name;
		}

		readonly string name;
		readonly bool allowMultiple;
		readonly CellVisibilityApplicator applicator;

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return applicator;
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			// No fetch required.
		}

		public override bool AllowMultiple
		{
			get { return allowMultiple; }
		}

		public override string FilterName
		{
			get { return name; }
		}
	}
}
