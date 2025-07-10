using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class FilterMap
	{
		public FilterMap(CardAllocationMap cardAllocation)
		{
			hiddenCards = new Dictionary<CellContent, HashSet<ZGuid>>(cardAllocation.GetCells().Count);
		}

		readonly Dictionary<CellContent, HashSet<ZGuid>> hiddenCards;

		internal void PopulateFilter(CardVisibilityFilterApplicator applicator, CardAllocationMap cardAllocation, IEnumerable<CellContent> applicableCells)
		{
			applicator.AddFetchHints(applicableCells.SelectMany(cell => cardAllocation.GetCards(cell)).Distinct());

			foreach (var cell in applicableCells)
			{
				hiddenCards[cell] = new HashSet<ZGuid>(cardAllocation.GetCards(cell).Where(card => !applicator.IsApplicable(card, cell)).Select(card => card.Identifier));
			}
		}

		public bool IsVisible(CellContent cell, ICardContent card)
		{
			return !(hiddenCards.TryGetValue(cell, out var hidden) && hidden.Contains(card.Identifier));
		}
	}
}
