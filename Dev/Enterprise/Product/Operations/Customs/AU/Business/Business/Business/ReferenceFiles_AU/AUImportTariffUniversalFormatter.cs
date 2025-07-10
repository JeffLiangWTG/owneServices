using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business;

public class AUImportTariffUniversalFormatter : AUImportTariffFormatter, IAUTariffFormatter
{
	public override ZString Format(ZString unformattedTariff)
	{
		if (AUCClassWrapper.UseCustomsReferenceData)
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
