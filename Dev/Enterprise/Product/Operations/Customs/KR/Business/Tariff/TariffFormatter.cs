using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class TariffFormatter : Customs.Business.TariffFormatter
	{
		public override ZString Format(ZString unformattedTariff)
		{
			return unformattedTariff.KeepNumericCharacters();
		}

		protected override ZString DottedFormat(ZString unformattedTariff)
		{
			return MessageFunctions.HSCodeFormat(Format(unformattedTariff));
		}
	}
}
