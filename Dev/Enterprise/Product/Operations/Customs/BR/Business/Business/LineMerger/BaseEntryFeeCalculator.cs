using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public abstract class BaseEntryFeeCalculator
	{
		public BaseEntryFeeCalculator(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		protected readonly CusEntryHeader EntryHeader;

		public void UpdateFeeOnEntryLines()
		{
			var totalFeeAmount = CalculateTotalFee();
			if (totalFeeAmount > 0)
			{
				ApportionFeeOnEntryLines(totalFeeAmount);
			}
		}

		protected virtual void ApportionFeeOnEntryLines(ZDecimal totalFeeAmount)
		{
			ApportionFeeOnEntryLines(totalFeeAmount, EntryHeader);
		}

		protected void ApportionFeeOnEntryLines(ZDecimal totalFeeAmount, CusEntryHeader entryHeader)
		{
			var apportionList = ApportionManager.Apportion(entryHeader, totalFeeAmount);
			if (apportionList != null)
			{
				foreach (var (entryLine, dutyAmout) in apportionList)
				{
					entryLine.Fees.AddOrUpdate(FeeType, dutyAmout);
				}
			}
		}

		protected abstract string FeeType { get; }

		protected internal abstract ZDecimal CalculateTotalFee();

		protected abstract IFeeApportionManager ApportionManager { get; }
	}
}
