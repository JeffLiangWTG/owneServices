using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface ICustomsOffice
	{
		ZString ReferenceNumber { get; }
	}

	public interface IFirstEntryCustomsOffice : ICustomsOffice
	{
		ZDateTime ExpectedDateAndTimeOfArrival { get; }
	}
}
