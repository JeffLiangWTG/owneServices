using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public class MatchingResult
	{
		public MatchingResult()
		{
			Outcome = MatchingOutcome.MatchingNotPerformed;
		}

		public MatchingResult(List<InvoiceLineGroup> lineGroups, List<UniversalTransactionLineWrapper> allLines)
		{
			if (lineGroups.Any())
			{
				Outcome = InvoiceLineGroup.CheckIfAllLinesContainInLineGroups(allLines, lineGroups) ? MatchingOutcome.FullyMatched : MatchingOutcome.PartiallyMatched;
				InvoiceLineGroupsWithSuggestions = lineGroups;
			}
			else
			{
				Outcome = MatchingOutcome.NoMatchFound;
			}
		}

		public List<InvoiceLineGroup> InvoiceLineGroupsWithSuggestions { get; private set; }
		public MatchingOutcome Outcome { get; private set; }
		public List<MatchingSuggestion> GetBestMatchingSuggestions()
		{
			var bestMatchingsuggestions = new List<MatchingSuggestion>();

			var lines = new List<UniversalTransactionLineWrapper>();
			var orderedLineGroups = InvoiceLineGroupsWithSuggestions.OrderByDescending(x => x.Suggestions.Last().Key).ToList();
			foreach (var lineGroup in orderedLineGroups)
			{
				if (ContainsAllItems(lines, lineGroup.GetLines()))
				{
					lines.AddRange(lineGroup.GetLines());
					bestMatchingsuggestions.Add(lineGroup.Suggestions.Last().Value);
				}
			}

			return bestMatchingsuggestions;
		}

		static bool ContainsAllItems(List<UniversalTransactionLineWrapper> sourceCollection, UniversalTransactionLineWrapper[] targetCollection)
		{
			return targetCollection.Except(sourceCollection).Any();
		}
	}
}
