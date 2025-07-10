using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Customs.Common
{
	public abstract class CustomsValuationConfiguration
	{
		public abstract ICurrency LocalCurrency { get; }
	}

	public abstract class CustomsValuationByFactorConfiguration : CustomsValuationConfiguration
	{
		public abstract bool ApplyDiscountBeforeFactorCalculation { get; }

		public abstract IEnumerable<string> LineLevelCharges { get; }

		public virtual ZInt NumberOfDecimalPlacesForCustomsFactor => 6;
	}
}
