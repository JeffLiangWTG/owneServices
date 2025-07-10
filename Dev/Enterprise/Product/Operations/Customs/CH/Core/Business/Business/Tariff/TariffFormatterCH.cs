using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class TariffFormatterCH : TariffFormatter
{
	const int lengthToKeep = 14;

	public TariffFormatterCH()
	{
	}

	public override ZString Format(ZString unformattedTariff)
	{
		return unformattedTariff.KeepNumericCharacters().Left(lengthToKeep);
	}

	protected override ZString DottedFormat(ZString unformattedTariff)
	{
		var newTariff = Format(unformattedTariff);
		var dottedTariff = newTariff.IsEmpty ? "" : newTariff.SubstringSafe(0, 4) + "." + newTariff.SubstringSafe(4, 4);
		if (newTariff.Length > 8)
		{
			dottedTariff = dottedTariff + " " + newTariff.SubstringSafe(8, 3);
			if (newTariff.Length > 11)
			{
				dottedTariff = dottedTariff + " " + newTariff.SubstringSafe(11, 3);
			}
		}
		return dottedTariff;
	}
}
