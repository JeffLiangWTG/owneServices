using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADCertificateWrapper : ICertificate
{
	public SADCertificateWrapper(ISupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}

	readonly ISupportingDocument supportingDocument;

	public ZString DocumentType => supportingDocument.Type;

	public ZString CountryOfIssue => supportingDocument.CountryOfIssue;

	public ZString IssuingYear => supportingDocument.YearOfIssue;

	public ZString Reference => supportingDocument.ReferenceNumber;

	public ZDecimal? Quantity => supportingDocument.Quantity.GetValueOrNullIfZero();

	public ZString UnitOfMeasurement => supportingDocument.UnitOfQuantity;

	public ZBool DerogationFlag => supportingDocument.Status == SADConstants.CertificateFlag.DER;

	public ZBool RetrospectiveDerogationFlag => supportingDocument.Status == SADConstants.CertificateFlag.PAP;
}
