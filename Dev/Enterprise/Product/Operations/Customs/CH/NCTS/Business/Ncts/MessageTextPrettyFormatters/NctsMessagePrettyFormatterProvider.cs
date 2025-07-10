using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using Enterprise.Customs.CH.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsMessagePrettyFormatterProvider : ICHNctsMessagePrettyFormatterProvider
{
	public IMessagePrettyFormatter GetFormatter(CHEDIMessage message)
	{
		if (message.EM_MessageType == MessageTypeCodeList.Codes.MSG && message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
		{
			switch (message.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse:
				case MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse:
					return new Nxx04ResponsePrettyFormatter(message.Factory, message.MessageDetail as INxx04ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarArrivalResponse:
					return new NT008ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT008ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse:
					return new NT009ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT009ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies:
					return new NT019ResponsePrettyFormatter(message);
				case MessageSubTypeCodeList.Codes.PassarArrivalIndication:
					return new NT025ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT025ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarDepartureResponse:
				case MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse:
					return new NTx28ResponsePrettyFormatter(message.Factory, message.MessageDetail as INTx28ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney:
					return new NT182ResponsePrettyFormatter(message);
				case MessageSubTypeCodeList.Codes.PassarControlDecisionNotification:
					return new NT061ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT061ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarTransitReleased:
					return new NT029ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT029ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarRecoveryNotification:
					return new NT035ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT035ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarInvalidGuarantee:
					return new NT055ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT055ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse:
					return new NT057ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT057ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarIntentionToControl:
					return new NT060ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT060ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit:
					return new NT140ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT140ResponseDetail);
				case MessageSubTypeCodeList.Codes.PassarTransitClosed:
					return new NT045ResponsePrettyFormatter(message.Factory, message.MessageDetail as INT045ResponseDetail);
			}
		}
		return null;
	}
}
