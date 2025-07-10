using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocAmount;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	sealed partial class CompactTableStrategy
	{
		sealed class CompactFormatter : PricingPageTableCellFormatter
		{
			public CompactFormatter()
				: base(string.Empty)
			{
			}

			protected override bool TryFormatFlat(RateLine rateLine, ZDecimal amount, out DocAmount value)
			{
				value = DocAmount.Create(amount, rateLine.Currency.Decimals);
				return true;
			}

			protected override bool TryFormatUnit(RateLine rateLine, ZDecimal amount, out DocAmount value)
				=> TryFormatFlat(rateLine, amount, out value);

			protected override bool TryFormatMinimum(RateLine rateLine, ZDecimal amount, out DocAmount value)
			{
				value = null;
				return false;
			}
		}
	}
}
