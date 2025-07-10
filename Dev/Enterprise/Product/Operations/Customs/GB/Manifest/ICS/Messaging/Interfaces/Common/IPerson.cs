using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface IPerson
	{
		ZString Name { get; }
		ZString StreetAndNumber { get; }
		ZString PostalCode { get; }
		ZString City { get; }
		ZString CountryCode { get; }
		ZString LanguageCode { get; }
		ZString ConfigCode { get; }
	}
}
