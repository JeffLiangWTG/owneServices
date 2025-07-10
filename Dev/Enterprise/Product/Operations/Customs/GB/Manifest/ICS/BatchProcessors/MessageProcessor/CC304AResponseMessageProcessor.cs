using System.Net;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC304A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS
{
	public class CC304AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc304AType>
	{
		public CC304AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC304A;

		protected override void UpdateTransmittedMessageStatus(IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage transmittedMessage)
		{
			transmittedMessage.EM_Status = EDIMessageStatusList.Codes.Acknowledged;
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeaderSS manifestHeader, Cc304AType messageObject)
		{
			manifestHeader.RegistrationStatus = RegistrationStatusList.Codes.Acknowledged;
		}

		protected override ZString MessageInterpretation(Cc304AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC304A);
			if (ZDateTime.TryParseExact(messageObject?.Heahea?.AmeAccDatTimHea111, out var acceptanceDate, "yyyyMMddHHmm"))
			{
				result += $"<p>Acceptance Date: {WebUtility.HtmlEncode(acceptanceDate.ToString("dd/MM/yyyy HH:mm"))}</p>";
			}
			return result;
		}
	}
}
