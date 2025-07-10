using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IIMHeaderEntryCustomsOffice
{
	ZString Nationality { get; }
	ZString ReferenceNumber { get; }
	ZString Name { get; }
}
