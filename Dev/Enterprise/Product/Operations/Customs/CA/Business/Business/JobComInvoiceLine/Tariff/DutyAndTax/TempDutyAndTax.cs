using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	class TempDutyAndTax : IDutyAndTaxDataForCalculation
	{
		public TempDutyAndTax(IDutyAndTaxData parent)
		{
			this.parent = parent;
		}

		readonly IDutyAndTaxData parent;

		public IDutyAndTaxData Parent => parent;
		public ZString RateType { get; set; }
		public ZDecimal Rate { get; set; }
		public ZString UnitOfMeasure { get; set; }
		public ZDecimal Quantity => DutyAndTaxAmountCalculator.GetQuantity(Parent, UnitOfMeasure, TaxType, RateType, NormalValuePerUnit);
		public ZDecimal ValueForCalculation { get; set; }
		public ZDecimal NormalValuePerUnit => ZDecimal.Zero;
		public ZString TaxType => DutyAndTaxTypes.Codes.CustomsDuty;
		public ZString ExemptCode => ZString.Empty;
		public ZString NormalValueCurrency { get; set; }
		public ZDecimal Amount { get; set; }
		public ZString AmountDescription => DutyAndTaxAmountDescriptor.GetDescription(this);
		public ZBool IsInRefFiles => true;
		public ZBool Override => false;
		public ZString DutyType { get; set; }
	}
}
