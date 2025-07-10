using System;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Segments;
using Enterprise.Edifact.Generic.V4;

namespace Enterprise.Customs.AE.Business;

public static class CommonMessageBuilder
{
	public static UNBSegment GetUNBSegment(IInterchangeHeaderProvider interchangeHeaderProvider)
	{
		var uNBSegment = new UNBSegment();
		uNBSegment.SyntaxIdentifier.SyntaxIdentifier = interchangeHeaderProvider.SyntaxIdentifier;
		uNBSegment.SyntaxIdentifier.SyntaxVersionNumber = interchangeHeaderProvider.SyntaxVersionNumber;
		uNBSegment.SyntaxIdentifier.CharacterEncoding = interchangeHeaderProvider.CharacterEncoding;
		uNBSegment.SyntaxIdentifier.SyntaxReleaseNumber = interchangeHeaderProvider.SyntaxReleaseNumber;
		uNBSegment.InterchangeSender.SenderIdentification = interchangeHeaderProvider.SenderId;
		uNBSegment.InterchangeSender.InterchangeSenderInternalIdentification = interchangeHeaderProvider.SenderInternalId;
		uNBSegment.InterchangeSender.InterchangeSenderInternalSubIdentification = interchangeHeaderProvider.SenderInternalSubId;
		uNBSegment.InterchangeRecipient.RecipientIdentification = interchangeHeaderProvider.RecipientId;
		uNBSegment.DateTimeOfPreparation.Date = interchangeHeaderProvider.Date;
		uNBSegment.DateTimeOfPreparation.Time = interchangeHeaderProvider.Time;
		uNBSegment.InterchangeControlReference = interchangeHeaderProvider.ReferenceNumber;
		uNBSegment.ProcessingPriorityCode = interchangeHeaderProvider.ProcessingPriority;
		uNBSegment.TestIndicator = interchangeHeaderProvider.TestIndicator;

		return uNBSegment;
	}

	public static void PopulateUNHSegment(UNHSegment uNH, IMessageHeaderProvider messageHeaderProvider)
	{
		uNH.MessageReferenceNumber = messageHeaderProvider.ReferenceNumber;
		uNH.MessageIdentifier.MessageType = MessageTypeList.GetFromString(messageHeaderProvider.MessageType);
		uNH.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.GetFromString(messageHeaderProvider.MessageVersion);
		uNH.MessageIdentifier.MessageReleaseNumber = MessageReleaseNumberList.GetFromString(messageHeaderProvider.MessageReleaseNumber);
		uNH.MessageIdentifier.ControllingAgencyCoded = ControllingAgencyCodedList.GetFromString(messageHeaderProvider.ControllingAgency);
	}

	public static void PopulateDTMSegment(Func<DTMSegment> dTMProvider, IDateTimePeriodProvider dateTimePeriodProvider)
	{
		if (dateTimePeriodProvider == null)
		{
			return;
		}
		var dTM = dTMProvider();
		dTM.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = DateOrTimeOrPeriodFunctionCodeQualifierList.GetFromString(dateTimePeriodProvider.DateTimePeriodFunctionCode);
		dTM.DateTimePeriod.DateOrTimeOrPeriodText = dateTimePeriodProvider.DateTimePeriodText;
		dTM.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.GetFromString(dateTimePeriodProvider.DateTimePeriodFormat);
	}
}
