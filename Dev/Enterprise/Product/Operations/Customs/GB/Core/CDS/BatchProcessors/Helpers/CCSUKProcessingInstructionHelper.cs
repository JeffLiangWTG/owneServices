using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Helpers
{
	public sealed class CCSUKProcessingInstructionHelper
	{
		readonly ZString data;

		public CCSUKProcessingInstructionHelper(ZString processingInstructionData)
		{
			data = processingInstructionData;
		}

		public ZString SenderId => Regex.Match(data, "senderid=\"(?<senderid>.*?)\"").Groups["senderid"].Value;

		public ZString RecipientId => Regex.Match(data, "recipientid=\"(?<recipientid>.*?)\"").Groups["recipientid"].Value;

		public ZString ExtCorrelationId => Regex.Match(data, "ext-correlation-id=\"(?<extcorrelationid>.*?)\"").Groups["extcorrelationid"].Value;

		public ZGuid CorrelationId => ZGuid.TryParse(ExtCorrelationId, out var id) ? id : ZGuid.Empty;

		public ZString XConversationId => Regex.Match(data, "x-conversation-id=\"(?<xConversationId>.*?)\"").Groups["xConversationId"].Value;

		public ZGuid ConversationId => ZGuid.TryParse(XConversationId, out var id) ? id : ZGuid.Empty;
	}
}
