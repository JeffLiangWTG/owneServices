using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	internal class PricingPageTableLclCellFormatter : PricingPageTableCellFormatter
	{
		public const int MinimumColumn = 1;

		public PricingPageTableLclCellFormatter(string expectedUnit)
			: base(expectedUnit) { }

		public override int RequiredColumnCount
		{
			get { return 2; }
		}

		protected override bool TryFormat(RateLine line, MinimumOrPerUnitCalculator calculator, DocAmount[] value)
		{
			DocAmount minValue;
			DocAmount unitValue;

			if (TryFormatUnit(line, calculator.PerUnit, out unitValue) && TryFormatMinimum(line, calculator.Minimum, out minValue))
			{
				value[UnitColumn] = unitValue;
				value[MinimumColumn] = minValue;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
