using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NctsMessageSender
	{
		public NctsMessageSender(NctsMessageSendingAction sendingAction)
		{
			SendingAction = CargoWise.Common.Argument.NotNull(sendingAction, "sendingAction");
			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(SendingAction.NctsHeader);
		}

		NctsMessageSendingAction SendingAction { get; }

		public NCTSOutboundEDIMessage Send()
		{
			var nctsHeader = SendingAction.NctsHeader;
			var messageType = SendingAction.MessageType;
			var outboundEDIMessage = nctsHeader.Factory.New<NCTSOutboundEDIMessage>();
			var branch = nctsHeader.Branch;
			outboundEDIMessage.EM_GB = branch.PK;
			outboundEDIMessage.EM_MessageType = messageType;
			outboundEDIMessage.EM_MessageOwner = nctsHeader.DeclarantId.SubstringSafe(0, NCTSOutboundEDIMessage.Schema.EM_MessageOwnerMaxLength);
			var messageBuilder = CreateMessageBuilder(outboundEDIMessage);
			if (messageBuilder == null)
			{
				outboundEDIMessage.Delete();
				outboundEDIMessage = null;
			}
			else
			{
				var xmlMessage = messageBuilder.GenerateXmlMessage();
				outboundEDIMessage.EM_MessageText = SendingAction.MessageCreated(xmlMessage.GetSerializedString());
				outboundEDIMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
				outboundEDIMessage.EM_MessageInterpretation = new NctsEdiMessagePrettier(outboundEDIMessage).MakeOutboundPrettyForInterpretation(nctsHeader);
				SetHeaderStatuses(nctsHeader, messageType);
				SetValuationDate(nctsHeader, messageType);
				SendingAction.AddMessage(outboundEDIMessage);
				AddPermitRecord(nctsHeader, outboundEDIMessage);
			}

			return outboundEDIMessage;
		}

		protected IXmlMessageBuilder CreateMessageBuilder(NCTSOutboundEDIMessage outboundEDIMessage)
		{
			IXmlMessageBuilder result = null;
			var header = SendingAction.NctsHeader;
			var messageType = outboundEDIMessage.EM_MessageType;
			var movementHeader = GetMovementHeader(header);
			movementHeader.BM_Phase = messageType;
			switch (messageType)
			{
				case GB_NCTS5DeparturePhaseList.Codes.Amendment:
					result = new CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.CC013CMessageBuilder(new CC013CHeaderProvider(header));
					break;
				case GB_NCTS5DeparturePhaseList.Codes.CancellationAmendment:
					if (SendingAction is NctsMessageSendingAction nctsMessageSendingActionWithJustification)
					{
						result = new CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.CC014CMessageBuilder(
							new CC014CHeaderProvider(header, nctsMessageSendingActionWithJustification.Justification));
					}
					break;
				case GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration:
					result = new CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.CC015CMessageBuilder(new CC015CHeaderProvider(header));
					break;
				case GB_NCTS5DeparturePhaseList.Codes.PresentationOfAPreLodgedDeclaration:
					result = new CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.CC170CMessageBuilder(new CC170CProvider(header));
					break;
				case GB_NCTS5ArrivalPhaseList.Codes.ArrivalNotification:
					result = new CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.CC007CMessageBuilder(new CC007CProvider(header));
					break;
				case GB_NCTS5ArrivalPhaseList.Codes.UnloadingRemarks:
					result = new CargoWise.Customs.GB.MessageContracts.NCTS.Phase5.CC044CMessageBuilder(new CC044CProvider(header));
					break;
				default:
					break;
			}
			return result;
		}

		void SetHeaderStatuses(NctsHeader nctsHeader, ZString messageType)
		{
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			var movementHeader = GetMovementHeader(nctsHeader);
			switch (messageType)
			{
				case GB_NCTS5DeparturePhaseList.Codes.Amendment:
					movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
					break;
				case GB_NCTS5DeparturePhaseList.Codes.PresentationOfAPreLodgedDeclaration:
					movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
					break;
				case GB_NCTS5ArrivalPhaseList.Codes.UnloadingRemarks:
					movementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
					break;
				default:
					break;
			}
		}

		static void SetValuationDate(NctsHeader nctsHeader, ZString messageType)
		{
			if (messageType == GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration)
			{
				NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, false);
			}
			else if (messageType == GB_NCTS5DeparturePhaseList.Codes.Amendment)
			{
				NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, true);
			}
		}

		NctsCommonMovementHeader GetMovementHeader(NctsHeader nctsHeader)
		{
			return nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : nctsHeader.ArrivalMovementHeader;
		}

		void AddPermitRecord(NctsHeader header, NCTSOutboundEDIMessage message)
		{
			if (header.IsPhase5Departure && MessageTypeToAddPermitRecords.Contains(message.EM_MessageType))
			{
				NctsMessageGeneratorHelper.AddPermitRecords(header, message);
			}
		}

		ImmutableList<ZString> MessageTypeToAddPermitRecords => messageTypeToAddPermitRecords ??= new List<ZString>()
		{
			GB_NCTS5DeparturePhaseList.Codes.Amendment,
			GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration
		}.ToImmutableList();
		ImmutableList<ZString> messageTypeToAddPermitRecords;
	}
}
