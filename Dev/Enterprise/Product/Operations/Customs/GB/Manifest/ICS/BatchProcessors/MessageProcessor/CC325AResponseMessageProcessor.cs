using System.Net;
using CargoWise.Customs.GB.MessageDefinitions.ICS.CC325A_v10_0;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.ICS
{
	public class CC325AResponseMessageProcessor : IcsSsGreatBritainResponseMessageProcessorBase<Cc325AType>
	{
		public CC325AResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => IcsSsGreatBritainEDIMessageTypeList.Codes.CC325A;

		protected override void UpdateTransmittedMessageStatus(IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage transmittedMessage)
		{
			transmittedMessage.EM_Status = EDIMessageStatusList.Codes.Acknowledged;
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeaderSS manifestHeader, Cc325AType messageObject)
		{
			manifestHeader.RegistrationStatus = RegistrationStatusList.Codes.DivertedOk;

			var reference = messageObject.Cusofffent730?.RefNumCusofffent731;
			var registrationDateTimeString = messageObject.Heahea?.RegDatTimHea125;

			if (reference != null)
			{
				if (registrationDateTimeString != null)
				{
					_ = ZDateTime.TryParseExact(registrationDateTimeString, out var registrationDate, "yyyyMMddhhmm")
						? manifestHeader.AddOfficeOfActualEntryDiversion(reference, registrationDate)
						: manifestHeader.AddOfficeOfActualEntryDiversion(reference);
				}
				else
				{
					_ = manifestHeader.AddOfficeOfActualEntryDiversion(reference);
				}
			}
		}

		protected override ZString MessageInterpretation(Cc325AType messageObject)
		{
			var result = GetMessageTypeInterpretation(IcsSsGreatBritainEDIMessageTypeList.Descriptions.CC325A);
			if (!string.IsNullOrEmpty(messageObject?.Cusofffent730?.RefNumCusofffent731))
			{
				result += $"<p>Customs Office of First Entry Reference Number: {WebUtility.HtmlEncode(messageObject.Cusofffent730.RefNumCusofffent731)}</p>";
			}
			return result;
		}
	}
}
