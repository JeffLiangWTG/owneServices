using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecPreviousDocumentDataProvider : IEdecPreviousDocument
{
	public static EdecPreviousDocumentDataProvider New(PreviousDocument previousDocument) => previousDocument == null ? null : new EdecPreviousDocumentDataProvider(previousDocument);

	EdecPreviousDocumentDataProvider(PreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}

	readonly PreviousDocument previousDocument;

	public string PreviousDocumentType => previousDocument.CSI_Code;

	public string PreviousDocumentReference => previousDocument.CSI_ReferenceNumber;

	public string AdditionalInformation => previousDocument.CSI_Description;
}
