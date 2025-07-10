using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IT.Messaging.MessageStructure;

[CodeAlive("This class will be used in the future.")]
public class RecordHeader
{
	[MessageLayout(Position = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, true)]
	public ZString AuthorizedUserCode { get; set; }

	[MessageLayout(Position = 17)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 12, true)]
	public ZString FileName { get; set; }

	[MessageLayout(Position = 41)]
	[MessageFieldIntegerRepresentation(6, true)]
	public ZInt CustomsOfficeSectionCode { get; set; }

	[MessageLayout(Position = 49)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, true)]
	public ZString CountryCode { get; set; }

	[MessageLayout(Position = 51)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 15, true)]
	public ZString TaxCodeOrVATRegistrationNumber { get; set; }

	[MessageLayout(Position = 67)]
	[MessageFieldIntegerRepresentation(3, true)]
	public ZInt ProgressiveSeatAuthorizedAccount { get; set; }

	[MessageLayout(Position = 71)]
	[MessageFieldIntegerRepresentation(5, true)]
	public ZInt NumberOfRecordsInTheFile { get; set; }
}
