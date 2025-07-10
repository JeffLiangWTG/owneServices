using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ICertificate
{
	ZString DocumentType { get; }
	ZString CountryOfIssue { get; }
	ZString IssuingYear { get; }
	ZString Reference { get; }
	ZDecimal? Quantity { get; }
	ZString UnitOfMeasurement { get; }
	ZBool DerogationFlag { get; }
	ZBool RetrospectiveDerogationFlag { get; }
}
