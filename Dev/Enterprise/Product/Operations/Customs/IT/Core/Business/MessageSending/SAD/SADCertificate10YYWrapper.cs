using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADCertificate10YYWrapper : ICertificate
{
	public SADCertificate10YYWrapper(ZDecimal quantity)
	{
		Quantity = quantity;
	}
	public ZString DocumentType => UniversalReferenceConstants.SupportingDocumentTypes.FeeCalculationThirdUomCertificate;

	public ZString CountryOfIssue => ZString.Empty;

	public ZString IssuingYear => ZString.Empty;

	public ZString Reference => ZString.Empty;

	public ZDecimal? Quantity { get; }

	public ZString UnitOfMeasurement => ZString.Empty;

	public ZBool DerogationFlag => ZBool.False;

	public ZBool RetrospectiveDerogationFlag => ZBool.False;
}
