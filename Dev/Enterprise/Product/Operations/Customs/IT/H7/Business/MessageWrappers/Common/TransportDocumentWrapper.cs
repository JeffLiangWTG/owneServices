using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class TransportDocumentWrapper : ITransportDocument
{
	public TransportDocumentWrapper(AdditionalInfo transportDocument)
	{
		this.transportDocument = Argument.NotNull(transportDocument, nameof(transportDocument));
	}

	readonly AdditionalInfo transportDocument;

	public string DocumentType => transportDocument.CSI_Code;

	public string ReferenceNumber => transportDocument.CSI_ReferenceNumber;
}
