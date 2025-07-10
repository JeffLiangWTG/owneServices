using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUExportTariffFormatter : TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return DottedFormat(unformattedTariff);
		}

		protected override ZString DottedFormat(ZString unformattedTariff)
		{
			var newTariff = KeepNumericCharacters(unformattedTariff);
			ZString dottedTariff = newTariff.IsEmpty ? string.Empty : newTariff.Substring(0, 4) + "." + newTariff.Substring(4, 2) + "." + newTariff.Substring(6, 2);
			return dottedTariff.Trim(' ', '.');
		}

		protected ZString KeepNumericCharacters(ZString unformattedTariff)
		{
			var minNumericChars = MaxTarifflength - 2;
			return unformattedTariff.KeepNumericCharacters().PadRight(minNumericChars).SubstringSafe(0, MaxTarifflength);
		}

		public const int MaxTarifflength = 10;
	}
}
