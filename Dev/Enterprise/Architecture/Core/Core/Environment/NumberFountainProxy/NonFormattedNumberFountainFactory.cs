using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class NonFormattedNumberFountainFactory
	{
		public NonFormattedNumberFountainFactory(
			string fountainName,
			Guid? ownerPk = null,
			bool rollOver = NumberFountain.FountainUtils.NoRollOver,
			long minValue = NumberFountain.FountainUtils.MinNumber,
			long maxValue = NumberFountain.FountainUtils.MaxNumber)
		{
			inner = new NumberFountain.NonFormattedNumberFountainFactory(fountainName, ownerPk ?? Guid.Empty, rollOver, minValue, maxValue);
		}
		readonly NumberFountain.NonFormattedNumberFountainFactory inner;

		public INumberFountainProxy New()
		{
			return inner.New().Wrap();
		}
	}
}
