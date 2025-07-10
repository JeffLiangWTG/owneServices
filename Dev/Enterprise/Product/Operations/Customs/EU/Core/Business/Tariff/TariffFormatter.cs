using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public abstract class TariffFormatter : Customs.Business.TariffFormatter
	{
		public static TariffFormatter New(string dataGrouping)
		{
			switch (dataGrouping)
			{
				case Core.Constants.CountryCodes.France:
				case Core.Constants.CountryCodes.Italy:
				case Core.Constants.CountryCodes.Spain:
				case Core.Constants.CountryCodes.UnitedKingdom:
					return new TariffFormatterTen();
				case Core.Constants.CountryCodes.Germany:
					return new TariffFormatterEleven();
				default:
					return new TariffFormatterThirteen();
			}
		}

		internal TariffFormatter(int length)
		{
			lengthToKeep = length;
		}
		readonly int lengthToKeep = 13;

		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.KeepNumericCharacters().Left(lengthToKeep);
		}

		public static ZString GetTariffType(bool isExport)
		{
			return isExport ? Universal.Constants.TariffTypes.Export : Universal.Constants.TariffTypes.Import;
		}

		public static ZString GetExportTariffNumber(ZString tariffNumber)
		{
			return tariffNumber.KeepNumericCharacters().Left(8);
		}
	}
}
