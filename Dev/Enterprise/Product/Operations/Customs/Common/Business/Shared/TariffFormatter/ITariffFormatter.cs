
namespace Enterprise.Customs.Common
{
	using CargoWise.Types;

	/// <summary>
	///		Defines methods for formatting the codes to a proper format.
	/// </summary>
	public interface ITariffFormatter
	{
		ZString Format(ZString unformattedTariff);

		ZString DisplayFormat(ZString unformattedTariff);
	}
}
