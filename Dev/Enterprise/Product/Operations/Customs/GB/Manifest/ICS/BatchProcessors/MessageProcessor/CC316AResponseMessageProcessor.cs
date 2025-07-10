using System.Linq;
using System.Net;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC316A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS
{
	public class CC316AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc316AType>
	{
		public CC316AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC316A;

		protected override ZString MessageInterpretation(Cc316AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC316A);
			if (messageObject?.Heahea?.DecRejReaHea252 is string reason)
			{
				result += $"<p>Rejection Reason: {WebUtility.HtmlEncode(reason)}</p>";
			}
			result += GenerateFunctionalErrorsInterpretation(messageObject?.Funerrer1?.Select(error => (error.ErrTypEr11, error.ErrPoiEr12, error.ErrReaEr13, error.OriAttValEr14)).ToList());
			return result;
		}

		protected override void UpdateTransmittedMessageStatus(IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage transmittedMessage)
		{
			transmittedMessage.EM_Status = EDIMessageStatusList.Codes.Rejected;
		}
	}
}
