using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging.IE323
{
	public interface IHeader
	{
		ZString TransportModeAtBorder { get; }
		ZString FirstEntryDeclaredOfficeCountryCode { get; }
		ZString InformationType { get; }
		ZString DiversionReferenceNumber { get; }
		ZString MeansOfTransportIdentity { get; }
		ZDateTime ExpectedDateAndTimeOfArrival { get; }
	}
}
