using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Ebd;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

internal class EbdDocumentImportDataProvider : IEbdDocumentImport
{
	public EbdDocumentImportDataProvider(SupportingDocSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	readonly SupportingDocSendingObject sendingObject;

	public string UidNumber => GlbCompany.CurrentCompany?.GC_CustomsRegistrationNo;

	public string CustomsDeclarationNumber => sendingObject.LocalReferenceNumber;

	public IEbdAccompanyingDocument AccompanyingDocument => accompanyingDocument ?? (accompanyingDocument = new EbdAccompanyingDocumentDataProvider(sendingObject));
	IEbdAccompanyingDocument accompanyingDocument;
}
