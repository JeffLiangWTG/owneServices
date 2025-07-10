using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public sealed class TariffFormatter : Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.KeepNumericCharacters();
		}

		protected override ZString DottedFormat(ZString unformattedTariff)
		{
			var newTariff = unformattedTariff.KeepNumericCharacters();
			ZString dottedTariff = newTariff.IsEmpty ? string.Empty : $"{newTariff.SubstringSafe(0, 4)}.{newTariff.SubstringSafe(4, 2)}.{newTariff.SubstringSafe(6).Trim()}";
			return dottedTariff.Trim('.');
		}
	}
}
