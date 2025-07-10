using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class EntryLineVatCalculator : EU.Business.EntryLineVatCalculator
	{
		public EntryLineVatCalculator(EU.Business.Declaration.CusEntryLine entryLine) : base(entryLine)
		{
		}

		protected override ZDecimal CalculateAmount(ZDecimal baseValue, ZDecimal rate)
		{
			if (entryLine is CusEntryLine deEntryLine && deEntryLine.RandomLine.JI_CessionFlag == CessionFlagList.Codes.WithVatExemptionWithoutInputVatExemption)
			{
				return ZDecimal.Zero;
			}
			return base.CalculateAmount(baseValue, rate);
		}
	}
}
