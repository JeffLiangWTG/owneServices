using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class ResponseFileManualUploader
{
	public ResponseFileManualUploader(ITEDIMessage sentMessage, ZString uploadedFileContent)
	{
		this.sentMessage = Argument.NotNull(sentMessage, nameof(sentMessage));
		sentInterchange = Argument.NotNull(sentMessage.Interchange, nameof(sentMessage.Interchange));
		this.uploadedFileContent = Argument.NotNullOrEmpty(uploadedFileContent, nameof(uploadedFileContent));
		CheckValidMessageType(sentMessage);
	}

	readonly ITEDIMessage sentMessage;
	readonly EDIInterchange sentInterchange;
	readonly ZString uploadedFileContent;

	public void ProcessExternalResponseMessage()
	{
		var internalFilename = GetFilenameFromCustomsHeader(uploadedFileContent);
		EnsureUploadedFileIsResponseOfSentMessage(internalFilename);
		QueueInterchangeForProcessingAndSetSentMessageStatus(internalFilename);
	}

	#region Implementation

	void CheckValidMessageType(ITEDIMessage previouslySentMessage)
	{
		var messageType = previouslySentMessage.EM_MessageType;
		if (messageType != SADConstants.CustomsInterchangeType.IdocR && messageType != SADConstants.CustomsInterchangeType.IdocT)
		{
			throw new InvalidOperationException(ValidationCaptions.UnsupportedMessageType(messageType));
		}
	}

	void EnsureUploadedFileIsResponseOfSentMessage(ZString internalFilename)
	{
		var internalFilenameToMatch = RemoveFirstCharOfExtension(internalFilename);
		var sentMessageFilenameToMatch = RemoveFirstCharOfExtension(GetSentMessageFilename());
		if (internalFilenameToMatch != sentMessageFilenameToMatch)
		{
			throw new IncomingMessageDoesNotMatchWithIdocException(ValidationCaptions.FilenameDoesntMatch);
		}
	}

	ZString GetFilenameFromCustomsHeader(ZString interchangeHeader) => interchangeHeader.SubstringSafe(16, 12);
	ZString RemoveFirstCharOfExtension(ZString filename) => filename.RemoveSafe(9, 1);

	ZString GetSentMessageFilename()
	{
		var sentMessageHeader = MessageProcessorHelper.RetrieveValueOfXmlNode(sentInterchange.EI_HeaderText, (NoResString)"Header");
		var sentMessageFilename = GetFilenameFromCustomsHeader(sentMessageHeader);
		return sentMessageFilename;
	}

	void QueueInterchangeForProcessingAndSetSentMessageStatus(ZString internalFilename)
	{
		var factory = new BusinessObjectFactory();
		var interchange = factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		interchange.EI_InterchangeType = internalFilename.SubstringSafe(9, 1);
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_From = ManualUpload;
		interchange.EI_To = sentInterchange.EI_From;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		SetInterchangeHeaderText(interchange, internalFilename);
		interchange.EI_BodyText = uploadedFileContent;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
		interchange.EI_TransportType = "";
		sentMessage.EM_Status = EDIMessage.Status.Sent;
		factory.Save();
	}

	void SetInterchangeHeaderText(EDIInterchange interchange, ZString internalFilename)
	{
		interchange.EI_HeaderText = new InterchangeHeaderTextBuilder()
			.AppendMessageType(interchange.EI_InterchangeType)
			.AppendFileName(internalFilename)
			.AppendTrackingID(sentInterchange.EI_SessionGUID)
			.Build();
	}

	const string ManualUpload = "ManualUpload";

	static class ValidationCaptions
	{
		public static ZString UnsupportedMessageType(ZString messageType) => Res.GetString("8C2900CD-E6F1-482E-9D4A-0D4C60BA1292", "'{0}' is not a supported message type. Supported message types are: ['{1}','{2}'].", messageType, SADConstants.CustomsInterchangeType.IdocR, SADConstants.CustomsInterchangeType.IdocT);
		public static ZString FilenameDoesntMatch => Res.GetString("D0DA05F1-4103-4547-8D6D-DF8AF33B5ACF", "The filename in the first row of the uploaded file does not match the filename of the selected IDOC.");
	}

	#endregion
}
