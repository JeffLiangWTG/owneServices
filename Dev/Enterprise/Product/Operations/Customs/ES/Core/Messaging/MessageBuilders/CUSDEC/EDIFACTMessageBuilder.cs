using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class EDIFACTMessageBuilder<TProvider, TSegmentGroup> : MessageBuilder<TProvider>
		where TProvider : IEDIFACTMessageDataProvider
		where TSegmentGroup : SegmentGroup
	{
		protected EDIFACTMessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
			characterSet = new UNOAESCharacterSet();
			edifactMessage = Activator.CreateInstance<TSegmentGroup>();
			interpretation = new MessageInterpretation(edifactMessage, characterSet);
		}

		protected readonly UNOAESCharacterSet characterSet;
		protected TSegmentGroup edifactMessage;
		protected MessageInterpretation interpretation;

		public override ZString UnsignedMessageText
		{
			get
			{
				if (unsignedMessageText.IsEmpty)
				{
					PopulateEdifactMessage();
					var message = edifactMessage.ToString(characterSet);
					unsignedMessageText = GetUNBSegment() + message + GetUNZSegment();
					return unsignedMessageText;
				}
				else
				{
					return base.UnsignedMessageText;
				}
			}
		}

		protected override ZString SignMessageText(ZString messageText) => messageText;

		protected abstract void PopulateEdifactMessage();

		string GetUNBSegment()
		{
			var uNB = new UNBSegment();
			uNB.SyntaxIdentifier.SyntaxIdentifier = "UNOA";
			uNB.SyntaxIdentifier.SyntaxVersionNumber = "1";

			uNB.InterchangeSender.SenderIdentification = provider.DeclarantIdForUNBSegment;
			uNB.InterchangeSender.PartnerIdentificationCodeQualifier = "ZZ";
			uNB.InterchangeRecipient.RecipientIdentification = "AEATADUE";
			uNB.InterchangeRecipient.PartnerIdentificationCodeQualifier = "ZZ";

			var nowTime = ZDateTime.Now;
			uNB.DateTimeOfPreparation.Date = nowTime.ToShortCustomsFormatDateString();
			uNB.DateTimeOfPreparation.Time = nowTime.ToCustomsFormatTimeString();

			uNB.InterchangeControlReference = EDIMessage.MessageNumberPlaceHolder;
			uNB.ApplicationReference = "&EE";

			if (provider.IsTest)
			{
				uNB.TestIndicator = "1";
			}

			return uNB.ToString(new UNOACharacterSet());
		}

		string GetUNZSegment()
		{
			var uNZ = new UNZSegment();
			uNZ.InterchangeControlCount = "1";
			uNZ.InterchangeControlReference = EDIMessage.MessageNumberPlaceHolder;
			return uNZ.ToString(new UNOACharacterSet());
		}
	}
}
