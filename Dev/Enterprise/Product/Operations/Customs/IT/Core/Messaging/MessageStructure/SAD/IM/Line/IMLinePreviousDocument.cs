using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLinePreviousDocument
{
	public IMLinePreviousDocument(IPreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}

	readonly IPreviousDocument previousDocument;

	[MessageLayout(Order = 0)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString DocType => previousDocument.DocType;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 3, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Category => previousDocument.Category;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZString Register => previousDocument.Register;

	[MessageLayout(Order = 3)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldImportRules("D", "CN42")]
	[MessageFieldDepositoRules("D", "CN42")]
	public ZString Reference => previousDocument.ReferenceNumber;

	[MessageLayout(Order = 4)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 1, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString ReferenceCIN => previousDocument.ReferenceCIN;

	[MessageLayout(Order = 5)]
	[MessageFieldDateDDMMYYYYRepresentation]
	[MessageFieldImportRules("D", "CN42")]
	[MessageFieldDepositoRules("D", "CN42")]
	public ZDate Date => previousDocument.Date;

	[MessageLayout(Order = 6)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 2, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString Series => previousDocument.Series;

	[MessageLayout(Order = 7)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public ZString CustomsOffice => previousDocument.CustomsOffice;

	[MessageLayout(Order = 8)]
	[MessageFieldIntegerRepresentation(3, false)]
	[MessageFieldImportRules("O", "CN17")]
	[MessageFieldDepositoRules("O", "CN17")]
	public ZInt? ItemNumber => previousDocument.ItemNumber;

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 18, false)]
	[MessageFieldImportRules("D", "CN17")]
	[MessageFieldDepositoRules("D", "CN17")]
	public ZString Mrn => previousDocument.Mrn;

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 500, false)]
	public ZString ComplementOfInformation => previousDocument.ComplementOfInformation;
}
