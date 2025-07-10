using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using EUAdditionalInfo = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class TransportDocumentWrapper : ITransportDocument
{
	public TransportDocumentWrapper(EUAdditionalInfo additionalDocument)
	{
		Argument.NotNull(additionalDocument, nameof(additionalDocument));
		InitializeLazy(additionalDocument);
	}

	void InitializeLazy(EUAdditionalInfo additionalDocument)
	{
		lazyReferenceNumber = new Lazy<string>(() => additionalDocument.CSI_ReferenceNumber);
		lazyDocumentType = new Lazy<string>(() => additionalDocument.CSI_Code);
	}

	string ITransportDocument.ReferenceNumber => lazyReferenceNumber.Value;
	Lazy<string> lazyReferenceNumber;

	string ITransportDocument.DocumentType => lazyDocumentType.Value;
	Lazy<string> lazyDocumentType;
}
