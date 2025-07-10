using System.Net;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC328A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS
{
	public class CC328AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc328AType>
	{
		public CC328AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC328A;

		protected override void UpdateTransmittedMessageStatus(IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage transmittedMessage)
		{
			transmittedMessage.EM_Status = EDIMessageStatusList.Codes.Acknowledged;
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeaderSS manifestHeader, Cc328AType messageObject)
		{
			manifestHeader.RegistrationStatus = RegistrationStatusList.Codes.Acknowledged;
			manifestHeader.RegistrationNumber = messageObject.Heahea?.DocNumHea5;
			ZDateTime.TryParseExact(messageObject.Heahea?.DecRegDatTimHea115, out var registrationDate, "yyyyMMddhhmm");
			manifestHeader.RegistrationDate = registrationDate;
		}

		protected override ZString MessageInterpretation(Cc328AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC328A);
			if (!string.IsNullOrEmpty(messageObject?.Heahea?.DocNumHea5))
			{
				result += $"<p>MRN: {WebUtility.HtmlEncode(messageObject.Heahea.DocNumHea5)}</p>";
			}
			return result;
		}
	}
}
