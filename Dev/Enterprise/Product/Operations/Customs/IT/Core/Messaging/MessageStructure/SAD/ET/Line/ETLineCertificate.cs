using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineCertificate
{
	public ETLineCertificate(ICertificate iCertificate)
	{
		this.iCertificate = Argument.NotNull(iCertificate, "iCertificate");
	}
	readonly ICertificate iCertificate;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R", "C901")]
	public ZString DocumentType => iCertificate.DocumentType;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldExportRules("D", "CN43")]
	[MessageFieldExportWithTransitRules("D", "CN43")]
	[MessageFieldTransitRules("D", "CN43")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN43")]
	public ZString CountryOfIssue => iCertificate.CountryOfIssue;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldExportRules("D", "CN43")]
	[MessageFieldExportWithTransitRules("D", "CN43")]
	[MessageFieldTransitRules("D", "CN43")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN43")]
	public ZString IssuingYear => iCertificate.IssuingYear;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 35, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R", "C901")]
	public ZString Reference => iCertificate.Reference;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString ReferenceLng => ZString.Empty;

	[MessageLayout(Order = 5)]
	[MessageFieldDecimalRepresentation(14, 5, false)]
	[MessageFieldExportRules("D", "CN43")]
	[MessageFieldExportWithTransitRules("D", "CN43")]
	[MessageFieldTransitRules("D", "CN43")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN43")]
	public ZDecimal? Quantity => iCertificate.Quantity;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldExportRules("D", "CN43")]
	[MessageFieldExportWithTransitRules("D", "CN43")]
	[MessageFieldTransitRules("D", "CN43")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN43")]
	public ZString UnitOfMeasurement => iCertificate.UnitOfMeasurement;

	[MessageLayout(Order = 7)]
	[MessageFieldBoolRepresentation]
	[MessageFieldExportRules("D", "CN43")]
	[MessageFieldExportWithTransitRules("D", "CN43")]
	[MessageFieldTransitRules("D", "CN43")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN43")]
	public ZBool DerogationFlag => iCertificate.DerogationFlag;

	[MessageLayout(Order = 8)]
	[MessageFieldBoolRepresentation]
	[MessageFieldExportRules("D", "CN43")]
	[MessageFieldExportWithTransitRules("D", "CN43")]
	[MessageFieldTransitRules("D", "CN43")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN43")]
	public ZBool RetrospectiveDerogationFlag => iCertificate.RetrospectiveDerogationFlag;
}
