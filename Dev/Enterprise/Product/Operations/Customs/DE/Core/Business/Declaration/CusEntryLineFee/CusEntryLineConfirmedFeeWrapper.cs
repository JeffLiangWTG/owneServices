using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusEntryLineConfirmedFeeWrapper : EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper
	{
		public CusEntryLineConfirmedFeeWrapper(EU.Business.Declaration.CusEntryLineFee cusEntryLineFee, string methodOfCalculation, string methodOfCalculationDE, string quickViewText) : base(cusEntryLineFee)
		{
			cF_MethodOfCalculation = methodOfCalculation;
			this.quickViewText = quickViewText;
			this.methodOfCalculationDE = methodOfCalculationDE;
		}

		public string MethodOfCalculationDE => methodOfCalculationDE;

		public override string QuickViewCard => quickViewText;

		protected override ZString CF_MethodOfCalculationCore => cF_MethodOfCalculation;

		protected override ZString HumanReadableNameCore => CF_MethodOfCalculation + ":";

		readonly ZString cF_MethodOfCalculation;

		readonly string quickViewText;

		readonly string methodOfCalculationDE;
	}
}
