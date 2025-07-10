using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS
{
	public abstract class CDSInventoryLinkingResponseMessageProcessor<T> : CDSMessageProcessor<T>
		where T : CDSEDIMessage
	{
		protected CDSInventoryLinkingResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
			this.logger = logger;
		}

		protected override ZString ProcessMessageCore(T cdsEDIMessage,
			BusinessObjectFactory factory)
		{
			var outgoingMessage = CDSResponseMessageProcessor.GetOutgoingMessageFromConversationID(cdsEDIMessage.EM_ApplicationReference, factory);
			if (outgoingMessage == null)
			{
				ZGuid.TryParse(cdsEDIMessage?.Interchange?.GBCustomsBusinessResponse?.EHubTrackingId, out var eHubTrackingId);
				var interchange = Helpers.EventParentFinderHelper.GetOutboundInterchange(eHubTrackingId, factory);
				outgoingMessage = interchange != null ? (CDSEDIMessage)Helpers.EventParentFinderHelper.GetOriginalMessage(interchange.PK, factory) : null;
			}

			if (outgoingMessage != null)
			{
				outgoingMessage.EM_Status = GetOutgoingMessageStatus(cdsEDIMessage);
				cdsEDIMessage.EM_MessageSubType = outgoingMessage.EM_MessageSubType;
				cdsEDIMessage.EM_MessageNum = string.Format(CultureInfo.InvariantCulture, "{0}B", outgoingMessage.EM_MessageNum);

				if (outgoingMessage.LinkedEntry != null)
				{
					outgoingMessage.LinkedEntry.Messages.Add(cdsEDIMessage);
					ClearMUCRIfRequired(outgoingMessage, cdsEDIMessage);
				}
				else if (outgoingMessage.LinkedConsol != null)
				{
					outgoingMessage.LinkedConsol.Messages.Add(cdsEDIMessage);
					ShutConsolIfRequired(outgoingMessage, cdsEDIMessage);
				}
			}
			else
			{
				cdsEDIMessage.Interchange.EI_RetryCount += 1;
				cdsEDIMessage.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
				ServiceTaskHelper.NudgeServiceTaskDelay(null, CDSServiceTaskConstants.CDSMessageRetrieverServiceTaskCode, TimeSpan.FromMinutes(1));

				if (cdsEDIMessage.Interchange.EI_RetryCount < 5)
				{
					logger.LogError(string.Format(CultureInfo.InvariantCulture, errorMessage, cdsEDIMessage.EM_MessageText, holdingMessage));
					return EDIMessageStatusList.Codes.Queued;
				}
				logger.LogError(string.Format(CultureInfo.InvariantCulture, errorMessage, cdsEDIMessage.EM_MessageText, ZString.Empty));
				return EDIMessageStatusList.Codes.Failed;
			}

			return EDIMessageStatusList.Codes.ProcessedOK;
		}

		protected virtual void ShutConsolIfRequired(CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
		}

		protected virtual void ClearMUCRIfRequired(CDSEDIMessage outgoingMessage, CDSEDIMessage incomingMessage)
		{
		}
		protected virtual ZString GetOutgoingMessageStatus(CDSEDIMessage message) => EDIMessageStatusList.Codes.Acknowledged;

		protected readonly LoggingInformation logger;

		const string errorMessage = "Failed to find the corresponding CDS Inventory Linking Request for response message: {0}{1}";
		const string holdingMessage = "\r\n\r\nHolding message for future re-processing";
	}
}
