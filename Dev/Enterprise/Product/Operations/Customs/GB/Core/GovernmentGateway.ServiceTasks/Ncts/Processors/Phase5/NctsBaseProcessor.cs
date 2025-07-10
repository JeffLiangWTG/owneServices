using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	public abstract class NctsBaseProcessor<T> : BaseProcessor<T>
	{
		protected NctsBaseProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.GbCustomsNCTS;

		protected override NctsHeader GetNctsHeaderFromMessage(EDIMessage inboundMessage)
		{
			NctsHeader header = null;
			var messageText = inboundMessage.EM_MessageText;
			MessageObject = DeserializeMessage(messageText);

			if (MessageObject != null)
			{
				var factory = inboundMessage.Factory;
				header = FindNctsHeaderUsingDepartureIdOrArrivalId(inboundMessage) ?? LocateHeaderByLRN(factory) ?? LocateHeaderByMRN(factory);

				if (header != null)
				{
					var branchPk = header.BH_GB;
					if (branchPk.IsValid)
					{
						inboundMessage.EM_GB = branchPk;
					}
					if (MessageShouldBeDiscarded(header, out var discardReasonText))
					{
						DiscardMessage(inboundMessage, discardReasonText);
						header = null;
					}
				}
				else
				{
					var errorText = ZString.Format("Unable to find a linked business object for message (Interchange Number:{0}, Message Number:{1}, Message Type:{2}). Message Status set to {3}.", inboundMessage.EM_InterchangeNumber, inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, StatusForUnableToFindALinkedBusinessObject);
					Logger.LogError(errorText);
					inboundMessage.EM_Status = StatusForUnableToFindALinkedBusinessObject;
					var noteForUnableToFindALinkedBusinessObject = NoteForUnableToFindALinkedBusinessObject;
					if (!noteForUnableToFindALinkedBusinessObject.IsEmpty)
					{
						inboundMessage.Notes.AddNew(isCustomDescription: true, "Constants.MessageProcessingNotes.ProcessingLog", noteForUnableToFindALinkedBusinessObject);
					}

					SendJobNotFoundEmailToNotificationGroup(inboundMessage, errorText);
				}
			}
			else
			{
				var error = "Could not interpret the message data";
				inboundMessage.Notes.AddNew(isCustomDescription: true, "Constants.MessageProcessingNotes.ProcessingLog", error);
				Logger.LogError(ZString.Format("Something went wrong: {3}. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to ERROR.", inboundMessage.EM_InterchangeNumber, inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, error));
			}
			return header;
		}

		void SendJobNotFoundEmailToNotificationGroup(EDIMessage inboundMessage, ZString errorText)
		{
			var emailSubject = string.Format(CultureInfo.InvariantCulture, "NCTS Phase 5 job not found for incoming message");
			var emailBody = string.Format(CultureInfo.InvariantCulture, @$"<p>{errorText}</p>");

			var uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Messaging.EDIMessage, inboundMessage.PK.ToGuid());
			if (!string.IsNullOrEmpty(uri))
			{
				var responseMessageUri = string.Format(CultureInfo.InvariantCulture, @$"<p><a href=""{uri}"">Click here to view the message</a></p>");
				emailBody = emailBody + responseMessageUri;
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(
				inboundMessage.Factory
				, string.Empty
				, string.Empty
				, emailSubject
				, emailBody
				, false
				, inboundMessage.Branch
				, null
				, () => string.Empty);
		}

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsErrors;

		protected override ZString GetMessageTypeSubstringFromMessageTypeCode(ZString code) => code.SubstringSafe(code.Length - 4, 3);

		protected abstract ZString LRN { get; }
		protected abstract ZString MRN { get; }
		protected virtual ZString GetNewPhase(T messageObject) => ZString.Empty;

		protected NctsHeader LocateHeaderByLRN(BusinessObjectFactory factory, string subApplicationCode = "")
		{
			NctsHeader result = null;
			if (!LRN.IsEmpty)
			{
				var cusInBoundHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));

				cusInBoundHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_IsActive, true);

				var cusInBondMoveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				cusInBondMoveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PaperlessInbondNum, LRN);

				if (!string.IsNullOrEmpty(subApplicationCode))
				{
					cusInBondMoveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
				}
				cusInBoundHeaderQuery.AddSubQuery(cusInBondMoveHeaderQuery, JoinCondition.And);

				result = factory.LoadTop1<NctsHeader>(cusInBoundHeaderQuery);
				if (result != null)
				{
					serviceLogger.Log(LogType.Information, $"Found a movement with the LRN '{LRN}'");
				}
				else
				{
					serviceLogger.Log(LogType.Information, $"Could not find a movement with the LRN '{LRN}' ({cusInBoundHeaderQuery.GetAsWhereClause(true).Trim()})");
				}
			}

			return result;
		}

		protected NctsHeader LocateHeaderByMRN(BusinessObjectFactory factory, string subApplicationCode = "", string[] messageStatusArray = null)
		{
			NctsHeader result = null;

			if (!MRN.IsEmpty)
			{
				var hasSubApplicationCode = !string.IsNullOrEmpty(subApplicationCode);
				var hasMessageStatusArray = messageStatusArray != null && messageStatusArray.Length > 0;

				var cusInBoundHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
				if (!AcceptMovementType.IsEmpty)
				{
					var headerTypes = new List<string> { AcceptMovementType };
					switch (AcceptMovementType)
					{
						case NctsMovementType.Codes.Departure:
						case NctsMovementType.Codes.Arrival:
							headerTypes.Add(NctsMovementType.Codes.DepartureAndArrival);
							break;
						case NctsMovementType.Codes.DepartureAndArrival:
							headerTypes.Add(NctsMovementType.Codes.Departure);
							headerTypes.Add(NctsMovementType.Codes.Arrival);
							break;
					}
					cusInBoundHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, headerTypes);
				}
				if (!CorrelationIdentifier.IsEmpty)
				{
					var jobNumber = CorrelationIdentifier.Split('/')[0];
					cusInBoundHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_JobReference, jobNumber);
				}
				if (hasMessageStatusArray)
				{
					var inBondHeaderArray = factory.Load<CusInBondHeader>(cusInBoundHeaderQuery);
					if (!inBondHeaderArray.IsNullOrEmpty() && inBondHeaderArray.Length > 1)
					{
						cusInBoundHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_MessageStatus, messageStatusArray);
					}
				}

				cusInBoundHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_IsActive, true);

				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, MRN);
				cusInBoundHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
				if (hasSubApplicationCode)
				{
					var cusInBondMoveHeader = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
					if (hasSubApplicationCode)
					{
						cusInBondMoveHeader.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, subApplicationCode);
					}
					cusInBoundHeaderQuery.AddSubQuery(cusInBondMoveHeader, JoinCondition.And);
				}
				result = factory.LoadTop1<NctsHeader>(cusInBoundHeaderQuery);

				if (result != null)
				{
					serviceLogger.Log(LogType.Information, $"Found a movement with the MRN '{MRN}'");
				}
				else
				{
					serviceLogger.Log(LogType.Information, $"Could not find a movement with the MRN '{MRN}' ({cusInBoundHeaderQuery.GetAsWhereClause(true).Trim()})");
				}
			}

			return result;
		}

		protected virtual ZString StatusForUnableToFindALinkedBusinessObject => EDIMessage.Status.Failed;

		protected virtual ZString NoteForUnableToFindALinkedBusinessObject => "The processing of the message with interchange failed because the message could not be linked to a NCTS declaration.";

		internal bool MessageShouldBeDiscarded(NctsHeader header, out string discardReasonText) => MessageShouldBeDiscardedCore(header, out discardReasonText);

		protected virtual bool MessageShouldBeDiscardedCore(NctsHeader header, out string discardReasonText)
		{
			discardReasonText = "";
			return false;
		}

		protected void DiscardMessage(EDIMessage message, string log)
		{
			Logger.LogError(log);
			message.EM_Status = EDIMessage.Status.Discarded;
			var details = ZString.Format("(Interchange Number:{0}, Number:{1}, Type:{2}); message status set to {3}.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, EDIMessage.Status.Discarded);
			message.Notes.AddNew(isCustomDescription: true, "Processing Log", log + " " + details);
		}

		protected string BecauseWrongCustomsStatusReasonText(ZString customsStatus) => ZString.Format("The message was discarded, because the Status at Customs of the declaration was {0}.", customsStatus);
		protected string BecauseWrongPhaseReasonText(ZString phase) => ZString.Format("The message was discarded, because the phase status of the declaration was {0}.", phase);
		protected string BecauseWrongMessageStatusReasonText(ZString messageStatus) => ZString.Format("The message was discarded, because the message status of the declaration was {0}.", messageStatus);

		protected override void UpdateNCTSHeader(T messageObject)
		{
			if (NctsHeaderItem != null && NctsHeaderItem.IsDepartureMovement)
			{
				OriginalCustomsStatus = NctsHeaderItem?.MovementHeader?.BM_CustomsStatus ?? ZString.Empty;
			}
			if (NctsHeaderItem != null && NctsHeaderItem.IsArrivalMovement)
			{
				OriginalCustomsStatus = NctsHeaderItem?.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty;
			}

			base.UpdateNCTSHeader(messageObject);

			if (NctsHeaderItem != null)
			{
				var newPhase = GetNewPhase(messageObject);
				if (!newPhase.IsEmpty)
				{
					if (NctsHeaderItem.IsDepartureMovement)
					{
						NctsHeaderItem.MovementHeader.BM_Phase = newPhase;
					}
					if (NctsHeaderItem.IsArrivalMovement)
					{
						NctsHeaderItem.ArrivalMovementHeader.BM_Phase = newPhase;
					}
				}
			}
		}

		protected ZString GetReadableDate(DateTime? date)
		{
			return new ZDateTime(date).ToString("dd/MM/yyyy");
		}

		protected ZString GetReadableDateAndTime(DateTime? date)
		{
			return new ZDateTime(date).ToString("dd/MM/yyyy HH:mm:ss");
		}

		protected void AppendGuarantorIfPresent(ZStringBuilder note, GuarantorType06 guarantor)
		{
			if (guarantor != null)
			{
				note.Append("The guarantor for this declaration is " + guarantor.IdentificationNumber + " " + guarantor.Name);

				if (guarantor.Address != null)
				{
					note.AppendIfNotEmpty(guarantor.Address.StreetAndNumber);
					note.AppendIfNotEmpty(guarantor.Address.Postcode);
					note.AppendIfNotEmpty(guarantor.Address.City);
					note.Append(GetXmlEnumValue(guarantor.Address.Country));
				}
			}
		}

		protected ZString GetXmlRepresentation(Enum value)
		{
			var valueString = value.ToString();
			var fieldInfo = value.GetType().GetField(valueString);
			var xmlAttrib = fieldInfo?.GetCustomAttributes(typeof(XmlEnumAttribute), inherit: false);
			if (xmlAttrib != null && xmlAttrib.Length > 0)
			{
				valueString = ((XmlEnumAttribute)xmlAttrib[0]).Name;
			}
			return valueString;
		}

		protected ZBool IsSentAcknowledgedOrOK(ZString messageStatus) => new ZString[] { LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged, NctsMessageStatusList.Codes.Ok }.Contains(messageStatus);

		protected T MessageObject { get; private set; }

		protected override string MessageSubTypeCode => string.Empty;

		protected ZString OriginalCustomsStatus { get; private set; }

		internal virtual ZString AcceptMovementType => NctsMovementType.Codes.DepartureAndArrival;

		protected abstract ZString CorrelationIdentifier { get; }
	}
}
