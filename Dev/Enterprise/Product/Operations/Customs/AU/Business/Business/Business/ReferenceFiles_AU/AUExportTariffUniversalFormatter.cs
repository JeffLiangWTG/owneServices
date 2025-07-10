using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUExportTariffUniversalFormatter : AUExportTariffFormatter, IAUTariffFormatter
{
	public override ZString Format(ZString unformattedTariff)
	{
		if (AUCAHECCWrapper.EnableCWRefForAHECC)
		{
			return KeepNumericCharacters(unformattedTariff).Trim();
		}
		else
		{
			return DottedFormat(unformattedTariff);
		}
	}

	public ZString FormatDotted(ZString unformattedTariff) => DottedFormat(unformattedTariff);
}
