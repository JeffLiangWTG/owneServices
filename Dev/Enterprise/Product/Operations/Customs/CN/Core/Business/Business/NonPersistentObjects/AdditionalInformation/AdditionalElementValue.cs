using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business;

internal class AdditionalElementValue
{
	public AdditionalElementValue(TariffView tariff)
	{
		this.tariff = tariff;
	}
	readonly TariffView tariff;

	public ZString Code { get; set; }
	public ZString Value { get; set; }

	public ZString Description => fDescription ?? (fDescription = tariff?.GetAdditionalElementDescription(Code) ?? ZString.Empty);
	string fDescription;

	public IAdditionalElementStrategy AdditionalElementStrategy => AdditionalElementStrategyProvider.GetAdditionalElementStrategy(Code);
}
