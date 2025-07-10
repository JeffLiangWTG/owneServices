using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public CDSInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreator();

		class InboundMessageCreator : IInboundMessageCreator
		{
			void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
			{
				if (interchange is CDSInterchange cdsInterchange)
				{
					var customsBusinessResponse = cdsInterchange.GBCustomsBusinessResponse;
					var conversationID = customsBusinessResponse.ConversationId;
					if (conversationID == ZString.Empty)
					{
						conversationID = customsBusinessResponse.EHubTrackingId;
					}

					var synchronousResponse = customsBusinessResponse.SynchronousResponse;
					if (synchronousResponse != null)
					{
						Spawn<CDSSynchronousResponseEDIMessage>(interchange, synchronousResponse.XML, conversationID);
					}

					var inventoryLinkingControlResponse = customsBusinessResponse.InventoryLinkingControlResponse;
					if (inventoryLinkingControlResponse != null)
					{
						Spawn<CDSInventoryLinkingControlResponseEDIMessage>(interchange, inventoryLinkingControlResponse.XML, conversationID);
					}

					var inventoryLinkingMovementResponse = customsBusinessResponse.InventoryLinkingMovementResponse;
					if (inventoryLinkingMovementResponse != null)
					{
						Spawn<CDSInventoryLinkingMovementResponseEDIMessage>(interchange, inventoryLinkingMovementResponse.XML, conversationID);
					}

					var inventoryLinkingMovementTotalsResponse = customsBusinessResponse.InventoryLinkingMovementTotalsResponse;
					if (inventoryLinkingMovementTotalsResponse != null)
					{
						Spawn<CDSInventoryLinkingMovementTotalsResponseEDIMessage>(interchange, inventoryLinkingMovementTotalsResponse.XML, conversationID);
					}

					var inventoryLinkingQueryResponse = customsBusinessResponse.InventoryLinkingQueryResponse;
					if (inventoryLinkingQueryResponse != null)
					{
						Spawn<CDSInventoryLinkingQueryResponseEDIMessage>(interchange, inventoryLinkingQueryResponse.XML, conversationID);
					}

					var declarationInfoResponse = customsBusinessResponse.DeclarationInfoResponse;
					if (declarationInfoResponse != null)
					{
						Spawn<CDSDeclarationInfoResponseEDIMessage>(interchange, declarationInfoResponse.XML, conversationID);
					}

					var documentUploadConfirmationRoot = customsBusinessResponse.DocumentUploadConfirmationRoot;
					if (!documentUploadConfirmationRoot.IsEmpty)
					{
						Spawn<CDSDocumentUploadConfirmationResponse>(interchange, documentUploadConfirmationRoot, customsBusinessResponse.EHubTrackingId);
					}

					foreach (var response in customsBusinessResponse.Responses)
					{
						var responsibleParty = customsBusinessResponse.MetaData?.ResponsibleAgencyName?.Value ?? string.Empty;
						var message = Spawn<CDSResponseEDIMessage>(interchange, response.Serialize(), conversationID);
						message.EM_MessageNum = new ZString(message.MessageDataObject?.FunctionalReferenceID?.Value ?? ZString.Empty).SubstringSafe(0, 20);
						message.EM_MessageOwner = responsibleParty; // e.g. CSP
					}

					var inventoryMessage = customsBusinessResponse.InventoryMessage;
					if (!inventoryMessage.IsEmpty)
					{
						var message = Spawn<GbEDIMessage>(interchange, inventoryMessage, conversationID);
						message.EM_ApplicationCode = ApplicationCodeList.Codes.Pentant;
						message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
						message.EM_MessageType = Constants.InterchangeType.CargoMessageType;
						message.EM_Status = EDIMessage.Status.Queued;
						message.EM_MessageNum = GetPentantMessageNumber(inventoryMessage);
						message.EM_LinkedObject = GetCusEntryHeaderFromInterchange(interchange.Factory, new ZGuid(customsBusinessResponse.EHubTrackingId));
					}
				}
			}

			static T Spawn<T>(EDIInterchange interchange, ZString messageText, ZString conversationID) where T : GbEDIMessage
			{
				var message = interchange.Factory.New<T>();
				message.EM_MessageText = messageText;
				message.EM_ApplicationReference = conversationID.KeepAlphanumericCharacters();

				interchange.ContainedMessages.Add(message);
				return message;
			}

			ZString GetPentantMessageNumber(ZString message)
			{
				var elements = message.Split(Constants.Pentant.SeparatorChar);
				return elements.Length >= 2 ? $"{elements[1]}P" : string.Empty;
			}

			CusEntryHeader GetCusEntryHeaderFromInterchange(BusinessObjectFactory factory, ZGuid eHubTrackingId)
			{
				var interchange = Helpers.EventParentFinderHelper.GetOutboundInterchange(eHubTrackingId, factory);
				var message = interchange != null ? (CDSEDIMessage)Helpers.EventParentFinderHelper.GetOriginalMessage(interchange.PK, factory) : null;
				return message?.LinkedEntry;
			}
		}

		protected override Type TypeOfInterchangeToCreate() => typeof(CDSInterchange);

		protected override string[] ApplicationCodes => new[] { GbCustomsDeclarationServices };
	}
}
