using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface ITrader
	{
		ZString Name { get; }
		ZString CompanyName { get; }
		ZString StreetAndNumber { get; }
		ZString PostalCode { get; }
		ZString City { get; }
		ZString CountryCode { get; }
		ZString NameAndAddressLanguage { get; }
		ZString TIN { get; }
		ZString HolderIDTIR { get; }
		ZString RepresentativeCapacity { get; }
		ZString RepresentativeCapacityLanguage { get; }
	}
}
