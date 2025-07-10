using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface ITariffData
	{
		ZString TariffCode { get; }
		ZString TariffDescription { get; }
		ZString TariffUnits { get; }
		ZBool ConveyanceIDRequired { get; }
	}
}
