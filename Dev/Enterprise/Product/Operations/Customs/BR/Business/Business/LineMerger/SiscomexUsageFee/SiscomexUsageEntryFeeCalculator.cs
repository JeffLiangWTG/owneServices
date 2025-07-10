using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class SiscomexUsageEntryFeeCalculator : BaseEntryFeeCalculator
	{
		public SiscomexUsageEntryFeeCalculator(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override string FeeType => Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee;

		protected override IFeeApportionManager ApportionManager => new SiscomexUsageFeeApportionManager();

		protected internal override ZDecimal CalculateTotalFee()
		{
			var result = 0m;
			var numberOfLines = EntryHeader.MergedLines.Count;
			var refCusTaxOrFee = GetRefCusTaxOrFee(numberOfLines);

			if (refCusTaxOrFee != null)
			{
				result = refCusTaxOrFee.ZZF_Maximum - ((refCusTaxOrFee.ZZF_Threshold - numberOfLines) * refCusTaxOrFee.ZZF_Value);
			}
			return result;
		}

		internal RefCusTaxOrFee GetRefCusTaxOrFee(int numberOfLines)
		{
			var taxOrFeeCodes = BRRefCusTaxOrFee.GetSiscomexUsageEntryFees(EntryHeader.Factory, EntryHeader.EffectiveValuationDate);
			return taxOrFeeCodes.Where(x => numberOfLines <= x.ZZF_Threshold).OrderBy(x => x.ZZF_Threshold).FirstOrDefault();
		}

		protected override void ApportionFeeOnEntryLines(ZDecimal totalFeeAmount)
		{
			base.ApportionFeeOnEntryLines(totalFeeAmount);

			if (EntryHeader.CH_MessageType == MessageTypeList.Codes.SUF
				&& EntryHeader.Declaration.ActiveEntryHeaders.FormalEntries.FirstOrDefault(h => h.CH_CEI_Instruction == EntryHeader.CH_CEI_Instruction) is CusEntryHeader entryHeader)
			{
				ApportionFeeOnEntryLines(totalFeeAmount, entryHeader);
			}
		}
	}
}
