using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class TariffFormatter : Customs.Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return Regex.IsMatch(unformattedTariff, @"^99[0-9][0-9]$")
					? unformattedTariff.PadRight(8, '0')
					: unformattedTariff.KeepNumericCharacters().Left(10);
		}
	}
}
