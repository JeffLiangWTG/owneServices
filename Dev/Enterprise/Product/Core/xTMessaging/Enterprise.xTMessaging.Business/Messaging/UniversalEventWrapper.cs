using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.Shared.MessageDefinitions.Universal.UniversalEvent;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = CargoWise.Customs.Shared.MessageDefinitions.Universal.UniversalEvent.Event;

namespace Enterprise.xTMessaging.Business
{
	public class UniversalEventWrapper : ICFGMessageResponse
	{
		public UniversalEventWrapper(string messageText)
		{
			eventData = XmlObjectSerializer.Deserialize<UniversalEventData>(messageText)?.Event;
			eventParameters = eventData?.EventParameters;
		}

		readonly EventEventParameters eventParameters;
		readonly UniversalEvent eventData;

		public string EventType
		{
			get
			{
				if (eventType == null)
				{
					eventType = eventData?.EventType ?? string.Empty;
				}

				return eventType;
			}
		}
		string eventType;

		public string ResponseType
		{
			get
			{
				if (responseType == null)
				{
					responseType = eventParameters?.Type ?? string.Empty;
				}

				return responseType;
			}
		}
		string responseType;

		public string MessageType
		{
			get
			{
				if (messageType == null)
				{
					messageType = eventParameters?.MessageType ?? string.Empty;
				}

				return messageType;
			}
		}
		string messageType;

		public string Reason
		{
			get
			{
				if (reason == null)
				{
					reason = eventParameters?.Reason ?? string.Empty;
				}

				return reason;
			}
		}
		string reason;

		public string OriginalAttributes
		{
			get
			{
				if (originalAttributes == null)
				{
					originalAttributes = GetContextValueByType(Shared.Constants.xTUniversalEventContextTypes.OriginalAttributes);
				}

				return originalAttributes;
			}
		}
		string originalAttributes;

		public string GetResponseMessage() => GetContextValueByType(Shared.Constants.xTUniversalEventContextTypes.ResponseMessage);

		public bool HasPreProcessingError => ResponseType == Shared.Constants.UniversalEventTypeErrorCategories.PreProcessingError
			|| (Regex.IsMatch(Reason, Shared.Constants.xTNotificationErrorRegexStrings.ProcessingComponentStepErrorRegex) && Regex.IsMatch(OriginalAttributes, Shared.Constants.xTNotificationErrorRegexStrings.PreProcessingComponentStepErrorRegex));

		public bool HasPostProcessingError => ResponseType == Shared.Constants.UniversalEventTypeErrorCategories.PostProcessingError
			|| (Regex.IsMatch(Reason, Shared.Constants.xTNotificationErrorRegexStrings.ProcessingComponentStepErrorRegex) && !Regex.IsMatch(OriginalAttributes, Shared.Constants.xTNotificationErrorRegexStrings.PreProcessingComponentStepErrorRegex));

		public bool HasMessageTransmissionFailure => ResponseType == Shared.Constants.UniversalEventTypeErrorCategories.TransmissionError || Regex.IsMatch(Reason, Shared.Constants.xTNotificationErrorRegexStrings.MessageTransmissionFailureRegex);

		public bool HasBusinessError => ResponseType == Shared.Constants.UniversalEventTypeErrorCategories.BusinessError;

		public bool HasUnauthorizedFailure => ResponseType == Shared.Constants.UniversalEventTypeErrorCategories.Unauthorized || Regex.IsMatch(Reason, Shared.Constants.xTNotificationErrorRegexStrings.MessageUnauthorizedFailureRegex);

		public bool IsAcknowledgement => EventType == Events.InterchangeAcknowledgedCode;

		public bool IsRejection => EventType == Events.InterchangeRejectedCode;

		public string GetContextValueByType(string type)
		{
			return eventData?.ContextCollection?.FirstOrDefault(o => o.Type?.Value == type)?.Value;
		}
	}
}
