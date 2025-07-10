using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GVMS;
using Enterprise.Customs.GB.ICS;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using GVMSAsycudaManifestHeader = Enterprise.Customs.GB.GVMS.AsycudaManifestHeader;
using ICSAsycudaManifestHeaderBase = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderBase;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class AsycudaManifestHeaderDataEventParentFinder : EventParentFinder
	{
		public AsycudaManifestHeaderDataEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var header = UpdateEntry(eventDataObject);
			return header != null ? new[] { header } : null;
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			UpdateEntry(eventData);
			return base.GetChildrenIfSpecifiedInContext(logParents, eventData);
		}

		BusinessObject UpdateEntry(UniversalEvent eventDataObject)
		{
			BusinessObject header = null;

			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			if (ZGuid.TryParse(eventValueObject.Context.EHubTrackingID, out var eHubTrackingID))
			{
				var originalInterchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(eHubTrackingID, factory);
				if (originalInterchange != null)
				{
					var originalMessage = CDS.Helpers.EventParentFinderHelper.GetOriginalMessage(originalInterchange.PK, factory);
					var dataProvider = eventValueObject.DataContext?.DataProviderForCodeMapping ?? ZString.Empty;
					switch (dataProvider)
					{
						case Constants.GVMS:
							if (originalMessage != null)
							{
								var processed = ProcessNotificationMessageId(eventDataObject, originalMessage, eHubTrackingID);

								if (!processed)
								{
									logger.Log(Integration.LogType.Warning, "Event was not processed");
								}

								header = originalMessage.EM_LinkedObject as ASYCUDA.Business.AsycudaManifestHeader;
							}

							if (eventValueObject.EventType.EqualsAny(new[] { CDS.Constants.EHubEventTypes.MessageRejected, CDS.Constants.EHubEventTypes.MessageResponseRejected }))
							{
								ProcessErrorEvent(eventDataObject, originalInterchange.PK);
							}
							break;

						case Constants.ICSGB:
						case Constants.ICSNI:
							if (originalMessage != null)
							{
								if (originalMessage.EM_LinkedObject is ICSAsycudaManifestHeaderBase icsHeader)
								{
									if (eventValueObject.EventType == AutoEvents.MessageRejectedCode)
									{
										ProcessICSErrorResponse(icsHeader, eventDataObject, originalMessage);
									}
									else
									{
										ProcessCorrelationID(icsHeader, eventValueObject, originalMessage);
									}
									header = icsHeader;
								}
							}
							if (header == null)
							{
								logger.Log(Integration.LogType.Warning, "ICS event was not processed");
							}
							break;
					}
				}
			}
			return header;
		}

		void ProcessCorrelationID(ICSAsycudaManifestHeaderBase header, IXmlEventValueObject eventValueObject, EDIMessage originalMessage)
		{
			var correlationID = eventValueObject.Context.CorrelationID;
			var correlationIDWithNoHyphens = correlationID.HasValue ? correlationID.Value.Replace("-", ZString.Empty) : ZString.Empty;
			CreateICSResponseMessage(isRejected: false, header, originalMessage, correlationIDWithNoHyphens, eventValueObject.EventType, string.Empty);
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
		}

		void ProcessICSErrorResponse(ICSAsycudaManifestHeaderBase header, IXmlEventValueObject eventValueObject, EDIMessage originalMessage)
		{
			var responseTextValue = eventValueObject.Context.ResponseText ?? ZString.Empty;
			var messageText = Encoding.UTF8.GetString(Convert.FromBase64String(responseTextValue)).Trim();

			CreateICSResponseMessage(isRejected: true, header, originalMessage, ZString.Empty, eventValueObject.EventType, messageText);
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
		}

		void CreateICSResponseMessage(bool isRejected, ICSAsycudaManifestHeaderBase header, EDIMessage originalMessage, ZString correlationID, string messageSubType, string messageText)
		{
			var isSAS = header.AMA_ManifestType == ICSManifestTypes.Codes.SAS;
			var newIcsResponseMessage = header.Messages.AddNew(isSAS ? typeof(IcsSsGreatBritainEDIMessage) : typeof(IcsNorthernIrelandEDIMessage));
			newIcsResponseMessage.EM_LinkedObject = header;
			newIcsResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			newIcsResponseMessage.EM_Status = EDIMessage.Status.Received;
			newIcsResponseMessage.EM_MessageSubType = messageSubType;
			newIcsResponseMessage.EM_ApplicationReference = correlationID;
			newIcsResponseMessage.EM_MessageText = messageText;
			newIcsResponseMessage.EM_MessageInterpretation = isRejected
				? ICSInterpretationPrettier.CreatePrettyInterpretation(messageText)
				: GetCorrelationIDMessageInterpretation(originalMessage, newIcsResponseMessage) + MoreInfo;

			if (!originalMessage.EM_Status.EqualsAny(new ZString[] { EDIMessage.Status.Acknowledged, EDIMessage.Status.Rejected }))
			{
				originalMessage.EM_Status = isRejected ? EDIMessage.Status.Rejected : EDIMessage.Status.Acknowledged;
			}
			newIcsResponseMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + (isRejected ? 'E' : 'R');
			if (!correlationID.IsEmpty)
			{
				originalMessage.EM_ApplicationReference = correlationID;
			}
		}

		ZString GetCorrelationIDMessageInterpretation(EDIMessage originalMessage, EDIMessage message)
		{
			return message.EM_ApplicationReference.IsEmpty
				? $"Message {originalMessage.EM_MessageNum} received a response without a correlation ID."
				: $"Message {originalMessage.EM_MessageNum} received correlation ID {message.EM_ApplicationReference}";
		}

		bool ProcessNotificationMessageId(UniversalEvent eventDataObject, EDIMessage originalMessage, ZGuid eHubTrackingID)
		{
			var processed = false;
			var notificationMessageId = ((IXmlEventValueObject)eventDataObject).Context.NotificationMessageID;
			if (notificationMessageId.HasValue && !string.IsNullOrEmpty(notificationMessageId.Value))
			{
				var notificationMessageIdWithNoHyphens = notificationMessageId.Value.Replace("-", string.Empty);
				processed = CreateGVMSResponseMessage(eventDataObject, originalMessage, notificationMessageIdWithNoHyphens, GVMS.Constants.GVMSMessageSubTypes.NOTIFICATIONMESSAGEID, eHubTrackingID, GetNotificationIdMessage);
			}

			return processed;
		}

		delegate ZString GetMessageInterpretation(UniversalEvent eventDataObject, ZGuid ehubTrackingID, EDIMessage originalMessage, EDIMessage message, GVMSAsycudaManifestHeader header);
		bool CreateGVMSResponseMessage(UniversalEvent eventDataObject, EDIMessage originalMessage, ZString reference, string messageSubType, ZGuid eHubTrackingID, GetMessageInterpretation getInterpretation)
		{
			var created = false;
			var header = originalMessage.EM_LinkedObject as GVMSAsycudaManifestHeader;

			if (header != null)
			{
				var newGvmsResponseMessage = CreateAndLinkEDIMessage<GVMSEDIMessage>(header);
				newGvmsResponseMessage.EM_Status = EDIMessage.Status.Received;
				newGvmsResponseMessage.EM_MessageSubType = messageSubType;
				newGvmsResponseMessage.EM_ApplicationReference = reference;
				newGvmsResponseMessage.EM_MessageInterpretation = getInterpretation.Invoke(eventDataObject, eHubTrackingID, originalMessage, newGvmsResponseMessage, header) + MoreInfo;

				if (!originalMessage.EM_Status.EqualsAny(new ZString[] { EDIMessage.Status.Acknowledged, EDIMessage.Status.Rejected }))
				{
					originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
				}
				newGvmsResponseMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + "R";
				originalMessage.EM_ApplicationReference = reference;
				created = true;

				var status = (ZString)MessageStatusCodeList.Codes.Sent;
				if (!status.IsEmpty && header != null)
				{ header.AMA_MessageStatus = status; }

				UpdateRegistrationStatusIfRequired(header, originalMessage, eventDataObject);
			}

			return created;
		}

		void UpdateRegistrationStatusIfRequired(GVMSAsycudaManifestHeader header, EDIMessage originalMessage, UniversalEvent eventDataObject)
		{
			var eventType = eventDataObject.EventType ?? ZString.Empty;
			if (eventType == ZArchitecture.Business.AutoEvents.MessageSentCode && originalMessage.EM_MessageSubType == GVMS.Constants.GVMSMessageSubTypes.CANCEL)
			{
				header.RegistrationStatus = GVMSCustomsStatus.Codes.Cancelled;
			}
		}

		void ProcessErrorEvent(UniversalEvent eventDataObject, ZGuid originalInterchangePK)
		{
			var errorMessage = ZString.Empty;

			var errorResponse = ((IXmlEventValueObject)eventDataObject).Context.ResponseText;
			var errorType = ((IXmlEventValueObject)eventDataObject).Context.ErrorSummary;
			var errorDetail = ((IXmlEventValueObject)eventDataObject).Context.ErrorDescription;

			if (errorResponse.HasValue && !errorResponse.Value.IsEmpty)
			{
				var data = Convert.FromBase64String(errorResponse.Value);
				errorMessage = Encoding.UTF8.GetString(data);
			}
			else if (errorDetail.HasValue && !errorDetail.Value.IsEmpty)
			{
				errorMessage = CDS.Helpers.EventParentFinderHelper.GenericErrorWrapper(errorType, errorDetail.Value);
			}

			if (!errorMessage.IsEmpty)
			{
				ProcessError(originalInterchangePK, errorMessage);
			}
		}

		void ProcessError(ZGuid originalInterchangePK, ZString errorMessage)
		{
			var originalMessage = CDS.Helpers.EventParentFinderHelper.GetOriginalMessage(originalInterchangePK, factory);

			if (originalMessage != null)
			{
				var header = originalMessage.EM_LinkedObject as GVMSAsycudaManifestHeader;

				if (header != null)
				{
					var newRejectionMessage = CreateAndLinkEDIMessage<GVMSErrorResponseEDIMessage>(header);
					newRejectionMessage.EM_MessageText = errorMessage;
					newRejectionMessage.EM_MessageNum = originalMessage.EM_MessageNum.Left(originalMessage.EM_MessageNumInfo.MaxLength - 1) + "E";

					var canBeProcessed = ErrorMessageContainsValidJsonContent(newRejectionMessage);
					newRejectionMessage.EM_Status = canBeProcessed ? EDIMessage.Status.Queued : EDIMessage.Status.Received;

					originalMessage.EM_Status = EDIMessage.Status.Rejected;
					header.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
				}
			}
		}

		bool ErrorMessageContainsValidJsonContent(GVMSErrorResponseEDIMessage msg)
		{
			var msgDataObj = msg.MessageDataObject;
			return msgDataObj != null;
		}

		ZString GetNotificationIdMessage(UniversalEvent eventDataObject, ZGuid ehubTrackingID, EDIMessage originalMessage, EDIMessage message, GVMSAsycudaManifestHeader header)
		{
			var interchange = CDS.Helpers.EventParentFinderHelper.GetOutboundInterchange(ehubTrackingID, factory);

			return GVMSEDIMessagePrettier.GetCIDMessage(ehubTrackingID, originalMessage, message, CDS.Helpers.EventParentFinderHelper.ehubRecipient, eventDataObject.EventTime.GetValueOrDefault().ToUtcZDateTime());
		}

		T CreateAndLinkEDIMessage<T>(GVMSAsycudaManifestHeader header) where T : EDIMessage
		{
			var newMessage = (T)header.Messages.AddNew(typeof(T));
			newMessage.EM_LinkedObject = header;
			return newMessage;
		}

		public const string MoreInfo = @"
<p>
<h4>For further information please see our learning units</h4>
";
	}
}
