using System;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;
using EuCommonPreviousDocument = Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

sealed class PreviousDocumentWrapper : IPreviousDocument
{
	public PreviousDocumentWrapper(EuCommonPreviousDocument previousDocument)
	{
		Argument.NotNull(previousDocument, nameof(previousDocument));
		InitializeLazy(previousDocument);
	}

	void InitializeLazy(EuCommonPreviousDocument previousDocument)
	{
		lazyReferenceNumber = new Lazy<string>(() => previousDocument.CSI_ReferenceNumber);
		lazyDocumentType = new Lazy<string>(() => previousDocument.CSI_Code);
		lazyComplementOfInformation = new Lazy<string>(() => previousDocument.CSI_ReferenceNumber2);
	}

	string IPreviousDocument.ReferenceNumber => lazyReferenceNumber.Value;
	Lazy<string> lazyReferenceNumber;

	string IPreviousDocument.DocumentType => lazyDocumentType.Value;
	Lazy<string> lazyDocumentType;

	string IPreviousDocument.ComplementOfInformation => lazyComplementOfInformation.Value;
	Lazy<string> lazyComplementOfInformation;
}
