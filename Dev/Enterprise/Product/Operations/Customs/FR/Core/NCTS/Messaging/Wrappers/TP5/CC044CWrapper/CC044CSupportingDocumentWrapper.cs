using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;

public class CC044CSupportingDocumentWrapper : SupportingDocumentWrapper
{
	public CC044CSupportingDocumentWrapper(NctsSupportingDocument supportingDocument) : base(supportingDocument)
	{
	}

	public new static CC044CSupportingDocumentWrapper New(NctsSupportingDocument supportingDocument) => supportingDocument == null ? null : new CC044CSupportingDocumentWrapper(supportingDocument);

	public override string ComplementOfInformation => complementOfInformation ?? (complementOfInformation = StatusIsNew ? document.CSI_ReferenceNumber2 : null);
	string complementOfInformation;

	public override string Type => type ?? (type = StatusIsNew ? document.CSI_Code : null);
	string type;

	public override string ReferenceNumber => referenceNumber ?? (referenceNumber = StatusIsNew ? document.CSI_ReferenceNumber : null);
	string referenceNumber;

	protected bool StatusIsNew => document.CSI_Status == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
}
