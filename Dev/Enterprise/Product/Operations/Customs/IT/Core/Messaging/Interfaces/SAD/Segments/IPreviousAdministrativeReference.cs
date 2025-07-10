using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IPreviousAdministrativeReference
{
	ZString Register { get; }
	ZString ReferenceNumber { get; }
	ZString ReferenceCIN { get; }
	ZDate Date { get; }
	ZString Series { get; }
	ZString CustomsOffice { get; }
	ZInt? ItemNumber { get; }
}
