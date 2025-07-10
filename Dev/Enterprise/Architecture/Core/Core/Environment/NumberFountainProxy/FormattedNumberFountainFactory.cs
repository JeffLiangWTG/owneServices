using Enterprise.NumberFountain;

namespace Enterprise.ZArchitecture.Environment
{
	public class FormattedNumberFountainFactory
	{
		public FormattedNumberFountainFactory(
			string fountainName,
			string prefix = "",
			bool rollOver = FountainUtils.NoRollOver,
			long minValue = FountainUtils.MinNumber,
			long maxValue = FountainUtils.MaxNumber,
			int formatDigits = FountainUtils.DefaultFormatDigits)
		{
			inner = new NumberFountain.FormattedNumberFountainFactory(fountainName, prefix, rollOver, minValue, maxValue, formatDigits);
		}

		readonly NumberFountain.FormattedNumberFountainFactory inner;

		public INumberFountainProxy New()
		{
			return inner.New().Wrap();
		}

		public static int DefaultFormatDigits => FountainUtils.DefaultFormatDigits;
	}
}
