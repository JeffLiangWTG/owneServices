using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public class InvoiceLineGroup
	{
		public InvoiceLineGroup(IGrouping<GroupingKey, UniversalTransactionLineWrapper> group)
		{
			Suggestions = new SortedList<ZInt, MatchingSuggestion>();
			Key = group.Key;
			lines = group.ToArray();
			TotalAmount = lines.Sum(x => x.OSTotalAmount) != ZDecimal.Zero
							? lines.Sum(x => x.OSTotalAmount)
							: lines.Sum(x => x.OSAmount + x.OSGSTVATAmount);
		}

		public GroupingKey Key { get; }
		public ZDecimal TotalAmount { get; }
		public UniversalTransactionLineWrapper[] GetLines()
		{
			return lines;
		}
		public SortedList<ZInt, MatchingSuggestion> Suggestions { get; }

		readonly UniversalTransactionLineWrapper[] lines;

		public static bool CheckIfAllLinesContainInLineGroups(List<UniversalTransactionLineWrapper> lines, List<InvoiceLineGroup> invoiceLineGroups)
		{
			var result = lines;
			foreach (var invoiceLineGroup in invoiceLineGroups)
			{
				result = result.Except(invoiceLineGroup.GetLines()).ToList();

				if (!result.Any())
				{
					return true;
				}
			}

			return false;
		}

		public static bool CheckIfAnyLinesContainInLineGroups(List<UniversalTransactionLineWrapper> lines, List<InvoiceLineGroup> invoiceLineGroups)
		{
			var result = new List<UniversalTransactionLineWrapper>();
			foreach (var invoiceLineGroup in invoiceLineGroups)
			{
				result = lines.Intersect(invoiceLineGroup.GetLines()).ToList();

				if (result.Any())
				{
					return true;
				}
			}

			return false;
		}
	}
}
