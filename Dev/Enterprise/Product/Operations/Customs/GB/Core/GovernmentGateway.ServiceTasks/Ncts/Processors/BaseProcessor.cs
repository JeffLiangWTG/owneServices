using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GovernmentGateway.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors
{
	public abstract class BaseProcessor<T> : BranchCustomsApplicationTypeMessageProcessor
	{
		protected BaseProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(loggingInformation)
		{
			this.serviceLogger = serviceLogger;
		}

		public NctsHeader NctsHeaderItem;

		protected virtual ZString GetNewMessageStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewDeclarationStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewArrivalStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewDetailedStatus(T messageObject) => ZString.Empty;

		protected virtual T DeserializeMessage(string messageText) => CTCExtensions.Deserialize<T>(messageText);

		protected abstract NctsHeader GetNctsHeaderFromMessage(EDIMessage inboundMessage);

		protected virtual void UpdateNCTSHeader(T messageObject)
		{
			if (NctsHeaderItem == null)
			{
				serviceLogger.Log(LogType.Error, FormattableString.Invariant($"No NCTS header found with ID {GetMessageId(messageObject)}"));
			}
			else
			{
				if (NctsHeaderItem.IsDepartureMovement)
				{
					var newDeclarationStatus = GetNewDeclarationStatus(messageObject);
					if (!newDeclarationStatus.IsEmpty)
					{
						NctsHeaderItem.MovementHeader.BM_CustomsStatus = newDeclarationStatus;
					}
				}

				if (NctsHeaderItem.IsArrivalMovement)
				{
					var newArrivalStatus = GetNewArrivalStatus(messageObject);
					if (!newArrivalStatus.IsEmpty)
					{
						NctsHeaderItem.ArrivalMovementHeader.BM_CustomsStatus = newArrivalStatus;
					}
				}

				var newMessageStatus = GetNewMessageStatus(messageObject);
				if (!newMessageStatus.IsEmpty)
				{
					NctsHeaderItem.EffectiveMessageStatus = newMessageStatus;
				}

				AfterUpdateNCTSHeader(messageObject);
			}
		}

		protected virtual void AfterUpdateNCTSHeader(T messageObject) { }

		protected abstract string GetMessageId(T messageObject);

		protected void UpdateEDIMessage(EDIMessage message, T messageObject)
		{
			var messageTypeFomProcessor = GetMessageTypeCode(messageObject);
			if (messageTypeFomProcessor != null && messageTypeFomProcessor.Length >= 3)
			{
				message.EM_MessageType = GetMessageTypeSubstringFromMessageTypeCode(messageTypeFomProcessor);
				message.EM_MessageSubType = MessageSubTypeCode;
				message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
				var messageInterpretation = GetMessageInterpretation(messageObject, message);
				if (!messageInterpretation.IsEmpty)
				{
					message.EM_MessageInterpretation = messageInterpretation;
				}
				if (message.EM_LinkUniqueID.IsEmpty)
				{
					message.EM_LinkedObject = NctsHeaderItem.IsPhase5Departure ? NctsHeaderItem.MovementHeader : NctsHeaderItem;
				}
			}
		}

		protected virtual ZString GetMessageTypeSubstringFromMessageTypeCode(ZString code) => code.Right(3);

		protected abstract string GetMessageTypeCode(T messageObject);
		protected abstract string MessageSubTypeCode { get; }

		protected override void ProcessMessageCore(EDIMessage inboundMessage)
		{
			NctsHeaderItem = GetNctsHeaderFromMessage(inboundMessage);

			if (NctsHeaderItem != null)
			{
				var messageText = inboundMessage.EM_MessageText;
				var messageObject = DeserializeMessage(messageText);
				UpdateEDIMessage(inboundMessage, messageObject);
				UpdateOutgoingMessageStatus(inboundMessage);
				UpdateGuaranteeTransactionsIfNeeded(messageObject, inboundMessage);
				UpdateNCTSHeader(messageObject);
				GenerateDocuments(messageObject, inboundMessage);
				serviceLogger.Log(LogType.Information, () =>
				{
					return string.Format(CultureInfo.InvariantCulture, "Processing received message #{0}, type {1} {2}", inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, inboundMessage.EM_MessageSubType);
				});
			}
		}

		protected NctsHeader FindNctsHeaderUsingDepartureIdOrArrivalId(EDIMessage inboundMessage)
		{
			NctsHeader parentBusinessObject = null;

			if (GBCustomsDataRegistry.Instance.CTC_UseDepartureIdOrArrivalId.Value && !inboundMessage.EM_ApplicationReference.IsEmpty)
			{
				var factory = inboundMessage.Factory;
				var query = new ZQuery()
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit)
					.AddToFilter(EDIMessageSchema.EM_ApplicationReference, inboundMessage.EM_ApplicationReference);

				var incomingDepartureMessageList = new CTCIncomingDepartureMessageTypeList();
				var incomingArrivalMessageList = new CTCIncomingArrivalMessageTypeList();

				string[] filterMessageTypes = null;
				var incomingMessageType = GetMessageTypeWithoutVersion(inboundMessage.EM_MessageSubType);
				if (incomingDepartureMessageList.ContainsCode(incomingMessageType))
				{
					var outgoingDepartureMessageList = new CTCOutgoingDepartureMessageTypeList();
					filterMessageTypes = outgoingDepartureMessageList.GetAllCodes();
				}
				else if (incomingArrivalMessageList.ContainsCode(incomingMessageType))
				{
					var outgoingArrivalMessageList = new CTCOutgoingArrivalMessageTypeList();
					filterMessageTypes = outgoingArrivalMessageList.GetAllCodes();
				}
				if (filterMessageTypes != null)
				{
					query.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.StartsWith, filterMessageTypes);
				}

				string couldNotFind()
				{
					var message = $"Could not find a suitable outgoing message with the departure/arrival ID '{inboundMessage.EM_ApplicationReference}'";
					if (filterMessageTypes != null)
					{
						message += $" with message type in [{string.Join(",", filterMessageTypes)}]";
					}
					return message;
				}

				query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";
				var outboundMessagesMatchingQuery = factory.Load<EDIMessage>(query);
				if (outboundMessagesMatchingQuery.Length > 0)
				{
					var outboundMessage = outboundMessagesMatchingQuery[0];
					var linkUniqueIDs = outboundMessagesMatchingQuery.Select(x => x.EM_LinkUniqueID).Distinct().ToArray();
					if (linkUniqueIDs.Length == 1)
					{
						var actualParentBO = outboundMessagesMatchingQuery[0].EM_LinkedObject;
						if (actualParentBO is Business.NctsDepartureMovementHeader departureMovementHeader)
						{
							parentBusinessObject = departureMovementHeader.Header as NctsHeader;
							if (parentBusinessObject != null)
							{
								serviceLogger.Log(LogType.Information, $"Matched to {parentBusinessObject.BH_JobReference} departure using ID '{inboundMessage.EM_ApplicationReference}' on outgoing message {outboundMessage.EM_MessageNum}");
							}
							else
							{
								serviceLogger.Log(LogType.Information, $"{couldNotFind()}; found a departure on outgoing message {outboundMessage.EM_MessageNum} but the linked header is not of the expected type!");
							}
						}
						else if (actualParentBO is NctsArrivalMovementHeader arrivalMovementHeader)
						{
							parentBusinessObject = arrivalMovementHeader.Header as NctsHeader;
							if (parentBusinessObject != null)
							{
								serviceLogger.Log(LogType.Information, $"Matched to {parentBusinessObject.BH_JobReference} arrival using ID '{inboundMessage.EM_ApplicationReference}' on outgoing message {outboundMessage.EM_MessageNum}");
							}
							else
							{
								serviceLogger.Log(LogType.Information, $"{couldNotFind()}; found an arrival on outgoing message {outboundMessage.EM_MessageNum} but the linked header is not of the expected type!");
							}
						}
						else if (actualParentBO is NctsHeader header)
						{
							parentBusinessObject = header;
							serviceLogger.Log(LogType.Information, $"Matched to {parentBusinessObject.BH_JobReference} header using arrival/departure ID '{inboundMessage.EM_ApplicationReference}' on outgoing message {outboundMessage.EM_MessageNum}");
						}
						else
						{
							serviceLogger.Log(LogType.Information, $"{couldNotFind()}; found outgoing message {outboundMessage.EM_MessageNum} linked to an unexpected object type '{actualParentBO?.GetType()}'");
						}
						if (parentBusinessObject != null)
						{
							inboundMessage.EM_LinkedObject = actualParentBO;
						}
					}
					else
					{
						serviceLogger.Log(LogType.Information, $"{couldNotFind()}; found multiple outgoing messages linked to different objects");
					}
				}
				else
				{
					serviceLogger.Log(LogType.Information, couldNotFind());
				}
			}

			return parentBusinessObject;
		}

		ZString GetMessageTypeWithoutVersion(ZString messageType)
		{
			return messageType.IsNumbersOnlyOrEmpty ? messageType : messageType.RemoveSafe(2, 1);
		}

		protected void UpdateOutgoingMessageStatus(EDIMessage inboundMessage)
		{
			var outgoingRelatedMessage = NctsHeaderItem.GetOutgoingMessage(inboundMessage);
			if (outgoingRelatedMessage != null)
			{
				outgoingRelatedMessage.EM_Status = EDIMessage.Status.Acknowledged;
			}
		}

		protected virtual void UpdateGuaranteeTransactionsIfNeeded(T messageObject, EDIMessage inboundMessage) { }

		protected virtual void GenerateDocuments(T messageObject, EDIMessage inboundMessage) { }

		#region Message Interpretation

		protected virtual ZString GetMessageInterpretation(T messageObject, EDIMessage inboundMessage = null) { return ZString.Empty; }

		protected ZString GetMessageStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New message status: {new NctsMessageStatusList().GetDescriptionFromCode(GetNewMessageStatus(messageObject))}"));
		}

		protected ZString GetDeclarationStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New declaration status: {new NctsTransitStatusList().GetDescriptionFromCode(GetNewDeclarationStatus(messageObject))}"));
		}

		protected ZString GetArrivalStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New arrival status: {new NctsTransitStatusList().GetDescriptionFromCode(GetNewArrivalStatus(messageObject))}"));
		}

		protected ZString GetDetailedStatusInterpretation(T messageObject)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"New detailed status: {new NctsTransitStatusList().GetDescriptionFromCode(GetNewDetailedStatus(messageObject))}"));
		}

		protected ZString GetReadableStatusDateAndTime(string date, string time)
		{
			string dateTime = "20" + date + time;
			if (ZDateTime.TryParseExact(dateTime, out var result, "yyyyMMddhhmm"))
			{
				dateTime = result.ToString("dd/MM/yyyy hh:mm");
			}
			return dateTime;
		}

		protected ZString GetReadableDate(string date)
		{
			if (ZDateTime.TryParseExact(date, out var result, "yyyyMMdd"))
			{
				date = result.ToString("dd/MM/yyyy");
			}
			return date;
		}

		protected ZString GetKeyValuePairInterpretation(ZString key, ZString value)
		{
			return GetParagraphInterpretation(FormattableString.Invariant($"{key}: {value}"));
		}

		protected ZString GetParagraphInterpretation(ZString text)
		{
			return text.IsEmpty ? ZString.Empty : new ZString(FormattableString.Invariant($"<p>{text}</p>"));
		}

		protected ZString GetXmlEnumValue(Enum enumValue)
		{
			var enumType = enumValue.GetType();
			var fieldInfo = enumType.GetField(enumValue.ToString());
			var attribute = (XmlEnumAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(XmlEnumAttribute));
			return attribute != null ? attribute.Name : enumValue.ToString();
		}

		#endregion

		protected readonly ILogger serviceLogger;
	}
}
