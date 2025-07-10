using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	abstract class OrganizationEventProcessorBase<TPayload> : IEventProcessor
	{
		public void ProcessMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var responseType = eventDataObject.EventType;
			var responseText = eventDataObject.Context.ResponseText.GetValueOrDefault();
			try
			{
				responseText = Encoding.UTF8.GetString(Convert.FromBase64String(responseText));
			}
			catch (SystemException)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: Failed to decode response text: '{responseText}'");
				throw;
			}

			TPayload response;
			try
			{
				response = JsonSerializer.Deserialize<TPayload>(responseText, jsonOptions);
			}
			catch (Exception)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: Failed to deserialize object from decoded response text: '{responseText}'");
				throw;
			}

			if (!ShouldProcessResponse(responseType, response))
			{
				return;
			}

			var eHubTrackingId = eventDataObject.Context.EHubTrackingID.GetValueOrDefault();
			Guid outboundInterchangePK;
			try
			{
				outboundInterchangePK = Guid.Parse(eHubTrackingId);
			}
			catch (FormatException)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: Failed to parse eHubTrackingID: '{eHubTrackingId}'");
				throw;
			}

			var outboundInterchange = businessObject.Factory.Load<EDIInterchange>(outboundInterchangePK);
			if (outboundInterchange == null)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: Could not find outbound interchange from the eHubTrackingID: '{eHubTrackingId}'");
				throw new Exception($"Could not load EDIInterchange with key {outboundInterchangePK}");
			}

			var outboundMessage = (EDIMessage)outboundInterchange.ContainedMessages.FirstOrDefault();
			if (outboundMessage == null)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: No message in the outbound interchange {outboundInterchange.EI_InterchangeNum}");
				throw new Exception($"Loaded EDIInterchange with key {outboundInterchangePK} does not contain a message");
			}

			Guid codePK;
			try
			{
				codePK = Guid.Parse(outboundMessage.EM_ApplicationReference);
			}
			catch (FormatException)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: Failed to parse key from outbound message {outboundMessage.EM_MessageNum}: '{outboundMessage.EM_ApplicationReference}'");
				throw;
			}

			var code = businessObject.Factory.Load<OrgCusCode>(codePK);
			if (code == null)
			{
				logger.Log(Integration.LogType.Error, $"{DataProvider}/{responseType}: Could not find code record {codePK}");
				throw new Exception($"Could not load OrgCusCode with key {codePK}");
			}

			outboundMessage.EM_Status = EDIMessageStatusList.Codes.Acknowledged;

			if (ResponseIsPositive(responseType, response))
			{
				code.MarkOrgCusCodeVerifiedOrUnVerified(verified: true, "HMC", responseText);
				code.LastVerifiedTime = TimeZoneInfo.ConvertTimeToUtc(GetVerifiedDateTime(response));
			}
			else
			{
				code.MarkOrgCusCodeVerifiedOrUnVerified(verified: false, ZString.Empty, ZString.Empty);
				code.OrgCusCodeValidity.OCV_SnapShotOfWhatIsVerified = responseText;
			}
		}

		protected virtual bool ShouldProcessResponse(string responseType, TPayload response) => responseType == AutoEvents.ServiceRequestedCode;
		protected abstract bool ResponseIsPositive(string responseType, TPayload response);
		protected abstract DateTime GetVerifiedDateTime(TPayload response);

		protected abstract string DataProvider { get; }

		readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
	}
}
