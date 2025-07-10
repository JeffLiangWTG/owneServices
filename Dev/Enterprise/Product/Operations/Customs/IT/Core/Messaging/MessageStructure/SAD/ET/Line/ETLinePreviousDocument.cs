using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLinePreviousDocument
{
	public ETLinePreviousDocument(IPreviousDocument iPreviousDocument)
	{
		this.iPreviousDocument = Argument.NotNull(iPreviousDocument, nameof(iPreviousDocument));
	}
	readonly IPreviousDocument iPreviousDocument;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString DocType => iPreviousDocument.DocType;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Category => iPreviousDocument.Category;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Register => iPreviousDocument.Register;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZString Reference => iPreviousDocument.ReferenceNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString ReferenceLng => ZString.Empty;

	[MessageLayout(Order = 5)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString ReferenceCIN => iPreviousDocument.ReferenceCIN;

	[MessageLayout(Order = 6)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZDate Date => iPreviousDocument.Date;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString Series => iPreviousDocument.Series;

	[MessageLayout(Order = 8)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString CustomsOffice => iPreviousDocument.CustomsOffice;

	[MessageLayout(Order = 9)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldExportRules("O")]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZInt? ItemNumber => iPreviousDocument.ItemNumber;

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldExportRules("D", "CN17")]
	[MessageFieldExportWithTransitRules("D", "CN17")]
	[MessageFieldTransitRules("D", "CN17")]
	[MessageFieldInternationalRoadTransportsRules("D", "CN17")]
	public ZString Mrn => iPreviousDocument.Mrn;

	[MessageLayout(Order = 11)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 26, false)]
	[MessageFieldExportWithTransitRules("O")]
	[MessageFieldTransitRules("O")]
	[MessageFieldInternationalRoadTransportsRules("O")]
	public ZString ComplementOfInformation => iPreviousDocument.ComplementOfInformation;

	[MessageLayout(Order = 12)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	public ZString ComplementOfInformationLng => ZString.Empty;
}
