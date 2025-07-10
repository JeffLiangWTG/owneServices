using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineCertificate
{
	public IMLineCertificate(ICertificate certificate)
	{
		this.certificate = Argument.NotNull(certificate, "certificate");
	}

	readonly ICertificate certificate;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString DocumentType => certificate.DocumentType;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldImportRules("D", "CN43")]
	[MessageFieldDepositoRules("D", "CN43")]
	public ZString CountryOfIssue => certificate.CountryOfIssue;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldImportRules("D", "CN43")]
	[MessageFieldDepositoRules("D", "CN43")]
	public ZString IssuingYear => certificate.IssuingYear;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldImportRules("D", "CN43")]
	[MessageFieldDepositoRules("D", "CN43")]
	public ZString Reference => certificate.Reference;

	[MessageLayout(Order = 5)]
	[MessageFieldDecimalRepresentation(16, 5, false)]
	[MessageFieldImportRules("D", "CN43", "TRN0002")]
	[MessageFieldDepositoRules("D", "CN43", "TRN0002")]
	public ZDecimal? Quantity => certificate.Quantity;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldImportRules("D", "CN43")]
	[MessageFieldDepositoRules("D", "CN43")]
	public ZString UnitOfMeasurement => certificate.UnitOfMeasurement;

	[MessageLayout(Order = 7)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZBool DerogationFlag => certificate.DerogationFlag;

	[MessageLayout(Order = 8)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZBool RetrospectiveDerogationFlag => certificate.RetrospectiveDerogationFlag;
}
