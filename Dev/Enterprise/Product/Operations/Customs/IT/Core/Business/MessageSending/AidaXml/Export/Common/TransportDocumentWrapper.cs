using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

sealed class TransportDocumentWrapper : ITransportDocument
{
	public TransportDocumentWrapper(AdditionalInfo additionalReference)
	{
		this.transportDocument = Argument.NotNull(additionalReference, nameof(additionalReference));
	}

	readonly AdditionalInfo transportDocument;

	string ITransportDocument.ReferenceNumber => transportDocument.CSI_ReferenceNumber;

	string ITransportDocument.DocumentType => transportDocument.CSI_Code;
}
