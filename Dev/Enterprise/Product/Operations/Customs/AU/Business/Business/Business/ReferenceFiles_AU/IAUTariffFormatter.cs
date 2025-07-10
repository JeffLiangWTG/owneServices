using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAUTariffFormatter : Common.ITariffFormatter
	{
		ZString FormatDotted(ZString unformattedTariff);
	}
}
