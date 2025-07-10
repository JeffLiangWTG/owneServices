using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public abstract class TP5BaseProcessor<T> : ApplicationTypeMessageProcessor
		where T : class
	{
		protected TP5BaseProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override ZQuery MessageFilterCore
		{
			get
			{
				var messageFilter = base.MessageFilterCore;
				messageFilter.AddToFilter(EDIMessageSchema.EM_MessageSubType, GetMessageSubType());
				return messageFilter;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Friendly Name Core")]
		protected override string MessageFriendlyNameCore => "FR NCTS Base Processor";

		protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.TP5 };

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.FRCustomsMessage;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is NCTSFREDIMessage messageFREDIMessage
				&& messageFREDIMessage.MessageDataObject is NCTSMessageDataObject<T> messageDataObject
				&& messageDataObject.ResponseMessage is T messageObject)
			{
				if (messageObject != null)
				{
					message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
					if (messageFREDIMessage.EM_LinkedObject is NctsDepartureMovementHeader movementHeader)
					{
						var nctsHeader = movementHeader.Header;
						UpdateHeader(nctsHeader, messageObject);
						UpdateGuaranteeTransactionsIfNeeded(nctsHeader, messageObject, message);
						DoExtraProcessing(nctsHeader, messageObject, message);
					}
					else if (messageFREDIMessage.EM_LinkedObject is NctsHeader nctsHeader)
					{
						UpdateHeader(nctsHeader, messageObject);
						UpdateGuaranteeTransactionsIfNeeded(nctsHeader, messageObject, message);
						DoExtraProcessing(nctsHeader, messageObject, message);
					}
				}
			}
		}

		protected virtual void UpdateHeader(NctsHeader header, T messageObject)
		{
			var mrn = GetMRNFromResponseMessage(messageObject);
			var messageStatus = GetNewMessageStatus(messageObject);
			var entryDate = GetEntryDate(messageObject);
			var phase = GetNewPhase(messageObject);

			if (!string.IsNullOrEmpty(mrn))
			{
				var cusEntryNumber = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
				if (cusEntryNumber != null)
				{
					cusEntryNumber.CE_EntryNum = mrn;
				}

				if (header.IsDepartureMovement)
				{
					var releaseDate = GetNewReleaseDateFromResponseMessage(messageObject);
					if (!releaseDate.IsEmpty)
					{
						cusEntryNumber.CE_IssueDate = releaseDate;
					}
				}
			}

			if (header.IsDepartureMovement)
			{
				var departureMovementHeader = header.MovementHeader;
				var newDepartureStatus = GetNewDepartureStatus(messageObject);
				if (!newDepartureStatus.IsEmpty)
				{
					SetDepartureStatus(departureMovementHeader, newDepartureStatus);
				}

				if (entryDate.IsValid)
				{
					departureMovementHeader.BM_EntryDate = entryDate;
				}

				if (!messageStatus.IsEmpty)
				{
					departureMovementHeader.BM_MessageStatus = messageStatus;
				}

				if (!phase.IsEmpty)
				{
					departureMovementHeader.BM_Phase = phase;
				}
			}

			if (header.IsArrivalMovement)
			{
				var arrivalMovementHeader = header.ArrivalMovementHeader;
				var newArrivalStatus = GetNewArrivalStatus(messageObject);
				if (!newArrivalStatus.IsEmpty)
				{
					arrivalMovementHeader.BM_CustomsStatus = newArrivalStatus;
				}

				if (!messageStatus.IsEmpty)
				{
					arrivalMovementHeader.BM_MessageStatus = messageStatus;
				}

				if (!phase.IsEmpty)
				{
					arrivalMovementHeader.BM_Phase = phase;
				}
			}
		}

		protected void SetDepartureStatus(NctsDepartureMovementHeader departureMovementHeader, ZString newDepartureStatus)
		{
			if (newDepartureStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated && departureMovementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
			{
				departureMovementHeader.LogCustomsStatus(NCTS5DepartureCustomsStatusList.Codes.MrnAllocated);
			}
			else
			{
				departureMovementHeader.BM_CustomsStatus = newDepartureStatus;
			}
		}

		protected virtual void UpdateGuaranteeTransactionsIfNeeded(NctsHeader header, T messageObject, EDIMessage inboundMessage)
		{
		}

		protected virtual void DoExtraProcessing(NctsHeader header, T messageObject, EDIMessage inboundMessage)
		{
		}

		protected abstract ZString GetMessageSubType();

		protected virtual ZString GetMRNFromResponseMessage(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewDepartureStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewArrivalStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewMessageStatus(T messageObject) => ZString.Empty;

		protected virtual ZString GetNewPhase(T messageObject) => ZString.Empty;

		protected virtual ZDateTime GetNewReleaseDateFromResponseMessage(T messageObject) => ZDateTime.Empty;

		protected virtual ZDateTime GetEntryDate(T messageObject) => ZDateTime.Empty;

		protected ZString GetEmailsubject(string responseReference)
		{
			var readableResponseReference = responseReference != ZString.Empty ? (ZString)Res.GetString("716CB099-55F3-4527-834A-15370852D616", " Reference: {0}", responseReference) : ZString.Empty;

			return Res.GetString("167BFD90-6AC0-4EE1-A5E3-C4504FA3A9FD", "New Transit response received.{0}", readableResponseReference);
		}

		protected static ZString GetEmailBody(string jobReferenceNumber, string messageBody)
		{
			return Res.GetString("873219EE-FFFB-4570-AB85-92B4FB28C580", "An NCTS P5 response for {0}. {1}.", jobReferenceNumber, messageBody);
		}

		protected static IRegistryItem GetEmailGroupRegistryItem() => FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup;
	}
}
