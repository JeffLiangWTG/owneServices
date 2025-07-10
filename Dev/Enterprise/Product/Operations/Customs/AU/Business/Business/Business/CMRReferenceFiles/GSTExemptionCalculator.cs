using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IGSTExempt
	{
		ZShort CharacteristicCode { get; }
		ZString RateCode { get; }
		ZString RateNumber { get; }
		ZString PrefScheme { get; }
	}

	public class GSTExemptionCalculator
	{
		public GSTExemptionCalculator(IGSTExempt[] gSTExemptRates)
		{
			this.gSTExemptRates = gSTExemptRates;
		}

		readonly IGSTExempt[] gSTExemptRates;

		public bool IsThisGSTExempt(ZString rateCode, ZString rateNumber, ZString prefScheme)
		{
			if (rateCode != "" && rateNumber != "" && prefScheme != "")
			{
				foreach (IGSTExempt gSTExemptRate in gSTExemptRates)
				{
					if (gSTExemptRate.CharacteristicCode == ZShort.Parse(CharacteristicCodeList.Codes.NonTaxableImports) &&
						gSTExemptRate.RateCode == rateCode &&
						gSTExemptRate.RateNumber == rateNumber &&
						gSTExemptRate.PrefScheme == prefScheme)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
