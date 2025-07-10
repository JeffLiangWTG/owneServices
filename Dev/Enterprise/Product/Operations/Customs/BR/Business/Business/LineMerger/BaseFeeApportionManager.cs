using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public interface IFeeApportionManager
	{
		IEnumerable<(CusEntryLine, ZDecimal)> Apportion(CusEntryHeader entryHeader, ZDecimal totalFeeAmount);
	}

	public abstract class BaseFeeApportionManager : IFeeApportionManager
	{
		public BaseFeeApportionManager()
		{
		}

		public IEnumerable<(CusEntryLine, ZDecimal)> Apportion(CusEntryHeader entryHeader, ZDecimal totalFeeAmount)
		{
			entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			var candidateEntryLines = GetCandidateEntryLines(entryHeader.MergedLines);

			if (candidateEntryLines.Count() > 1)
			{
				var apportionToLines = new List<ApportionLine>();

				foreach (var entryLine in candidateEntryLines)
				{
					apportionToLines.Add(new ApportionLine() { EntryLine = entryLine, LineValue = GetLineValue(entryLine) });
				}

				var totalValue = apportionToLines.Sum(x => x.LineValue);

				foreach (var line in apportionToLines)
				{
					line.FeeAmount = ApportionAmount(totalValue, line.LineValue, totalFeeAmount);
				}

				var difference = totalFeeAmount - apportionToLines.Sum(x => x.FeeAmount);
				apportionToLines.OrderByDescending(x => x.LineValue).First().FeeAmount += difference;

				return apportionToLines.Select(x => (x.EntryLine, x.FeeAmount));
			}
			if (candidateEntryLines.Any())
			{
				return candidateEntryLines.Select(x => (x, totalFeeAmount));
			}
			return null;
		}

		class ApportionLine
		{
			public CusEntryLine EntryLine;
			public ZDecimal LineValue;
			public ZDecimal FeeAmount;
		}

		ZDecimal ApportionAmount(ZDecimal totalLineValue, ZDecimal lineValue, ZDecimal totalFeeAmount)
			=> totalLineValue == 0m ? ZDecimal.Zero : new ZDecimal((totalFeeAmount / totalLineValue) * lineValue).Round(2);

		protected abstract ZDecimal GetLineValue(CusEntryLine entryLine);

		protected virtual IEnumerable<CusEntryLine> GetCandidateEntryLines(ICusEntryLineCollection<CusEntryLine> entryLines) => entryLines;
	}
}
