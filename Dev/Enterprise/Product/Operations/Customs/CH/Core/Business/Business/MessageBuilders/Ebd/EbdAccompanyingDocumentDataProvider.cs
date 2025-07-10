using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Ebd;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.Business;

internal class EbdAccompanyingDocumentDataProvider : IEbdAccompanyingDocument
{
	internal EbdAccompanyingDocumentDataProvider(SupportingDocSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		this.eDoc = Argument.NotNull(sendingObject.Document, nameof(sendingObject.Document));
	}

	readonly SupportingDocSendingObject sendingObject;
	readonly IeDoc eDoc;

	public string Filename => eDoc.FileName;

	public string Type => sendingObject.DocumentType;

	public IReadOnlyCollection<byte> Content => eDoc.UniqueKey.ToGuid().ToByteArray();
}
