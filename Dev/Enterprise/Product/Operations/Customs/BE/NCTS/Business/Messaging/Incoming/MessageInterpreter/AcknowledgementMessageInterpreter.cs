using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AcknowledgementMessageInterpreter : BaseMessageInterpreter<IAcknowledgementMessageDataProvider>
	{
		public override string Interpret(IAcknowledgementMessageDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			var originalOutgoingEdiMessage = NctsMessageHelper.LocateOriginalOutgoingEdiMessage(ediMessage.Interchange);

			note.Append($"Correlation id: {dataProvider.CorrelationId}");
			note.Append($"Customs acknowledged the reception of the message with id: {originalOutgoingEdiMessage.EM_MessageNum}");

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
