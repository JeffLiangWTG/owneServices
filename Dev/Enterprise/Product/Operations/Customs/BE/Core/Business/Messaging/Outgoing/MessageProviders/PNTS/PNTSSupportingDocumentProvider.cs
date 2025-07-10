using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business;

public class PNTSSupportingDocumentProvider : IPNTSDocument
{
	public PNTSSupportingDocumentProvider(TemporaryStorageSupportingDocument supportingDocument)
	{
		this.supportingDocument = Argument.NotNull(supportingDocument, nameof(supportingDocument));
	}
	readonly TemporaryStorageSupportingDocument supportingDocument;

	public string ReferenceNumber => supportingDocument.CSI_ReferenceNumber;

	public string Type => supportingDocument.CSI_Code;
}
