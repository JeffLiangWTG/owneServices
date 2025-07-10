using System;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class MessageHeaderWrapper : IMessageHeader
	{
		public string Sender => PNTSConstants.MessageHeaderWrapperConstants.Sender;

		virtual public string Recipient => PNTSConstants.MessageHeaderWrapperConstants.Recipient;

		public DateTime MessageTimestamp => ZDateTime.UtcNow.ToDateTime();

		public string MessageId => EDIMessage.MessageNumberPlaceHolder;

		public string RefToMessageId => MessageBuilderBase<object>.CorrelationidPlaceholder;

		public string CorrelationId => null;

		public string LanguageCode => Core.Constants.CountryCodes.France;
	}
}
