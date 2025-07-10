using System;
using CargoWise.Common;
using CargoWise.Customs.AE.MessageContracts;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class MPCIAttachmentMessageProvider : IMPCIAttachmentMessageProvider
{
	public MPCIAttachmentMessageProvider(SupportingDocSendingObject sendingObject)
	{
		SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		MessageId = ZGuid.NewZGuid().ToString();
	}
	SupportingDocSendingObject SendingObject { get; }

	public string MessageId { get; }

	public string SenderMPCIPartyID1 => SendingObject.DocMessageDataProvider.SenderIdentification;

	public string SenderMPCIPartyID2 => SendingObject.DocMessageDataProvider.SenderInternalIdentification;

	public string SenderMPCIPartyID3 => SendingObject.DocMessageDataProvider.SenderInternalSubIdentification;

	public string ReferenceDocumentID => SendingObject.DocMessageDataProvider.ReferenceDocumentID;

	public string Su => SendingObject.DocMessageDataProvider.DocumentIdentifier;

	public string ProcessingPriority => "A";

	public int TestIndicator => 0;

	public string Attachment => Convert.ToBase64String(SendingObject.Document.GetImageDataReader().ConvertToByteArrayAndCloseStream());

	public string AttachmentFileType => SendingObject.DocumentType.TrimStart('.');
}
