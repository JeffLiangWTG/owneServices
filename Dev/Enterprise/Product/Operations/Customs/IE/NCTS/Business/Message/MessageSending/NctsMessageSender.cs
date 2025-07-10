using CargoWise.Customs.IE.MessageContracts.NCTS;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using NctsDepartureMovementHeader = Enterprise.Customs.IE.NCTS.Business.NctsDepartureMovementHeader;
using NctsHeader = Enterprise.Customs.IE.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IE.Business
{
	public class NctsMessageSender : MessageSender
	{
		public NctsMessageSender(IMessageSendingAction sendingAction) : base(sendingAction) { }

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage relatingMessage)
		{
			IXmlMessageBuilder builder = null;
			NctsHeaderMessageProvider provider = null;

			var messageType = relatingMessage.EM_MessageType;
			switch (messageType)
			{
				case NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment:
					provider = new IE013MessageProvider(NctsHeader);
					builder = new IE013MessageBuilder((IIE013Header)provider);
					break;
				case NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest:
					if (SendingAction is NctsMessageSendingAction nctsMessageSendingActionWithJustification)
					{
						provider = new IE014MessageProvider(NctsHeader, nctsMessageSendingActionWithJustification.Justification);
						builder = new IE014MessageBuilder((IIE014Header)provider);
					}
					break;
				case NCTSOutgoingMessageTypeList.Codes.DeclarationData:
					provider = new IE015MessageProvider(NctsHeader);
					builder = new IE015MessageBuilder((IIE015Header)provider);
					break;
				case NCTSOutgoingMessageTypeList.Codes.GuaranteeAccessCodes:
					if (SendingAction is GuaranteeAccessCodesSendingAction guaranteeAccessCodesSendingObject)
					{
						provider = new IE026MessageProvider(guaranteeAccessCodesSendingObject);
						builder = new IE026MessageBuilder((IIE026Header)provider);
					}
					break;
				case NCTSOutgoingMessageTypeList.Codes.QueryOnGuarantees:
					if (SendingAction is QueryOnGuaranteeSendingAction queryOnGuaranteeSendingAction)
					{
						provider = new IE034MessageProvider(queryOnGuaranteeSendingAction);
						builder = new IE034MessageBuilder((IIE034Header)provider);
					}
					break;
				case NCTSOutgoingMessageTypeList.Codes.RequestOfRelease:
					if (SendingAction is NctsMessageSendingAction nctsMessageSendingActionWithReleaseRequest)
					{
						provider = new IE054MessageProvider(NctsHeader, nctsMessageSendingActionWithReleaseRequest.ReleaseRequest);
						builder = new IE054MessageBuilder((IIE054Header)provider);
					}
					break;
				case NCTSOutgoingMessageTypeList.Codes.ArrivalNotification:
					provider = new IE007MessageProvider(NctsHeader);
					builder = new IE007MessageBuilder((IIE007Header)provider);
					break;
				case NCTSOutgoingMessageTypeList.Codes.UnloadingRemarks:
					provider = new IE044MessageProvider(NctsHeader);
					builder = new IE044MessageBuilder((IIE044Header)provider);
					break;
				case NCTSOutgoingMessageTypeList.Codes.InformationAboutNonArrivedMovement:
					if (SendingAction is NctsMessageSendingAction nctsMessageSendingActionWithInformationAboutNonArrivedMovement)
					{
						provider = new IE141MessageProvider(NctsHeader, nctsMessageSendingActionWithInformationAboutNonArrivedMovement.SendingObject);
						builder = new IE141MessageBuilder((IIE141Header)provider);
					}
					break;
				case NCTSOutgoingMessageTypeList.Codes.PresentationNotification:
					provider = new IE170MessageProvider(NctsHeader);
					builder = new IE170MessageBuilder((IIE170Header)provider);
					break;
			}
			SaveInterpretation(relatingMessage, builder, provider);

			return builder;
		}

		void SaveInterpretation(OutboundEDIMessage outgoingMessage, IXmlMessageBuilder builder, NctsHeaderMessageProvider provider)
		{
			var interpreter = provider?.GetMessageInterpreter(outgoingMessage, builder);
			outgoingMessage.EM_MessageInterpretation = interpreter != null ? interpreter.GetInterpretation() : ZString.Empty;
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<NCTSOutboundEDIMessage>();

		protected override void PreSend()
		{
			var messageType = SendingAction.MessageType;

			switch (messageType)
			{
				case NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment:
					NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(NctsHeader);
					NctsMovementHeaderValuationDateHelper.SetValuationDate(NctsHeader.MovementHeader, true);
					break;

				case NCTSOutgoingMessageTypeList.Codes.DeclarationData:
					NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(NctsHeader);
					NctsMovementHeaderValuationDateHelper.SetValuationDate(NctsHeader.MovementHeader, false);
					break;
			}
		}

		NctsHeader NctsHeader
		{
			get
			{
				if (SendingAction.MessageAttachee is NctsHeader header)
				{
					return header;
				}
				else if (SendingAction.MessageAttachee is NctsDepartureMovementHeader movementHeader && movementHeader.Header is NctsHeader nctsHeader)
				{
					return nctsHeader;
				}
				else
				{
					return null;
				}
			}
		}
	}
}
