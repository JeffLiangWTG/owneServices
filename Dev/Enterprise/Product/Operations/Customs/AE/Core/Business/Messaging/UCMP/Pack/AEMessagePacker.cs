using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Edifact.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

[assembly: UniversalCustomsEDIMessagePacker(EDIMessage.ApplicationCodes.UnitedArabEmirates, typeof(Enterprise.Customs.AE.Business.AEMessagePacker))]

namespace Enterprise.Customs.AE.Business;

public class AEMessagePacker : IUniversalCustomsEDIMessagePacker
{
	public bool AllowEmptyMessageBody => false;

	public ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
	{
		if (!TryPopulateInterchangeBody(interchange, message))
		{
			var unkownMessageParentMessage = $"Unknown message parent for message {message.EM_MessageNum} {message.EM_ApplicationCode} {message.EM_MessageType}.";
			logger.LogError(unkownMessageParentMessage);
			return unkownMessageParentMessage;
		}
		var sender = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
		var recipient = AEConstants.Messaging.MessageRecipient;

		EDIMessagePackerUtils.PopulateInterchange(interchange, message.EM_ApplicationCode, message.EM_MessageType,
			sender, recipient, message.EM_GB, message.EM_GP, ZGuid.NewZGuid());

		interchange.NumberStrategy = new AEInterchangeNumberStrategy(interchange);
		interchange.Saving += AssignPlaceHolders;
		logger.Log($"Message({message.EM_MessageNum}) AEMessagePacker finished successfully.");
		return string.Empty;
	}

	void AssignPlaceHolders(EDIInterchange interchange)
	{
		var now = ZDateTime.UtcNow;
		var interchangeText = interchange.EI_BodyText;

		interchangeText = interchangeText.Replace(AEConstants.Messaging.Placeholders.DateOfCreation, now.ToString("yyyyMMdd"));
		interchangeText = interchangeText.Replace(AEConstants.Messaging.Placeholders.TimeOfCreation, now.ToString("HHmm"));
		interchangeText = interchangeText.Replace(AEConstants.Messaging.Placeholders.InterchangeNumber, interchange.EI_InterchangeNum);

		interchange.EI_BodyText = interchangeText;
	}

	bool TryPopulateInterchangeBody(EDIInterchange interchange, EDIMessage message)
	{
		if (message.EM_MessageType == AEConstants.Messaging.MessageTypes.DOCSUC)
		{
			interchange.EI_BodyText = message.EM_MessageText;
			return true;
		}

		var interchangeSegmentProvider = GetInterchangeSegmentProvider(message.EM_MessageType);
		var messageParent = GetMessageParent(message);
		if (interchangeSegmentProvider.TryGetHeaderSegment(messageParent, out var headerSegment))
		{
			var footerSegment = GetFooterSegment(headerSegment.InterchangeControlReference);

			var interchangeTextBuilder = new ZStringBuilder();
			var characterSet = AECharacterSet.New();
			interchangeTextBuilder.Append(headerSegment.ToString(characterSet));
			interchangeTextBuilder.Append(message.EM_MessageText);
			interchangeTextBuilder.Append(footerSegment.ToString(characterSet));

			interchange.EI_BodyText = interchangeTextBuilder.ToStringWithNewLineBetweenAppends();
			return true;
		}
		return false;
	}

	IInterchangeSegmentProvider GetInterchangeSegmentProvider(string messageType)
	{
		switch (messageType)
		{
			case AEConstants.Messaging.MessageTypes.CUSCAR:
				return (IInterchangeSegmentProvider)ManifestController.New().InterchangeSegmentProvider;
			case AEConstants.Messaging.MessageTypes.CONTRL:
				return new CONTRLInterchangeSegmentProvider();
			default:
				throw new ArgumentOutOfRangeException(nameof(messageType), messageType,
					$"Interchange segment provider missing for message type {messageType}");
		}
	}

	BusinessObject GetMessageParent(EDIMessage message)
	{
		var messageType = message.EM_MessageType;
		switch (messageType)
		{
			case AEConstants.Messaging.MessageTypes.CUSCAR:
				return message.EM_LinkedObject;
			case AEConstants.Messaging.MessageTypes.CONTRL:
				return message.LinkedEDIMessage;
			default:
				throw new InvalidOperationException($"Message parent missing for message type ({nameof(messageType)}): {messageType}");
		}
	}

	UNZSegment GetFooterSegment(string interchangeReference)
	{
		var uNZSegment = new UNZSegment();
		uNZSegment.InterchangeControlCount = "1";
		uNZSegment.InterchangeControlReference = interchangeReference;
		return uNZSegment;
	}
}
