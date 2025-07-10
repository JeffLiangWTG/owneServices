using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;

namespace Enterprise.Customs.CH.Business;

public static class PassarResponseAnalyzerExtensions
{
	public static string GetMessageSubType(this IPassarResponseAnalyzer analyzer)
	{
		switch (analyzer.MessageType)
		{
			case PassarMessageTypeList.Codes.NC909:
				return MessageSubTypeCodeList.Codes.PassarTechnicalError;
			case PassarMessageTypeList.Codes.NC084:
				return MessageSubTypeCodeList.Codes.PassarDocumentNotification;
			case PassarMessageTypeList.Codes.NE004:
				return MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse;
			case PassarMessageTypeList.Codes.NE009:
				return MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected;
			case PassarMessageTypeList.Codes.NE021:
				return MessageSubTypeCodeList.Codes.PassarExportPayloadRequestGoodsDeclarationResponse;
			case PassarMessageTypeList.Codes.NE028:
				return MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse;
			case PassarMessageTypeList.Codes.NE029:
				return MessageSubTypeCodeList.Codes.PassarExportReleaseResponse;
			case PassarMessageTypeList.Codes.NE060:
				return MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification;
			case PassarMessageTypeList.Codes.NE083:
				return MessageSubTypeCodeList.Codes.PassarExportDeclarationIssuanceOfAssessmentDecision;
			case PassarMessageTypeList.Codes.NE096:
				return MessageSubTypeCodeList.Codes.PassarExportDecisionRectification;
			case PassarMessageTypeList.Codes.NE131:
				return MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse;
			case PassarMessageTypeList.Codes.NT028:
				return MessageSubTypeCodeList.Codes.PassarDepartureResponse;
			case PassarMessageTypeList.Codes.NT029:
				return MessageSubTypeCodeList.Codes.PassarTransitReleased;
			case PassarMessageTypeList.Codes.NT008:
				return MessageSubTypeCodeList.Codes.PassarArrivalResponse;
			case PassarMessageTypeList.Codes.NT061:
				return MessageSubTypeCodeList.Codes.PassarControlDecisionNotification;
			case PassarMessageTypeList.Codes.NT019:
				return MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies;
			case PassarMessageTypeList.Codes.NT055:
				return MessageSubTypeCodeList.Codes.PassarInvalidGuarantee;
			case PassarMessageTypeList.Codes.NT060:
				return MessageSubTypeCodeList.Codes.PassarIntentionToControl;
			case PassarMessageTypeList.Codes.NT140:
				return MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit;
			case PassarMessageTypeList.Codes.NT045:
				return MessageSubTypeCodeList.Codes.PassarTransitClosed;
			case PassarMessageTypeList.Codes.NT035:
				return MessageSubTypeCodeList.Codes.PassarRecoveryNotification;
			case PassarMessageTypeList.Codes.NC124:
				return MessageSubTypeCodeList.Codes.PassarActivationResponse;
			case PassarMessageTypeList.Codes.NT004:
				return MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse;
			case PassarMessageTypeList.Codes.NT182:
				return MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney;
			case PassarMessageTypeList.Codes.NT057:
				return MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse;
			case PassarMessageTypeList.Codes.NT009:
				return MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse;
			case PassarMessageTypeList.Codes.NT025:
				return MessageSubTypeCodeList.Codes.PassarArrivalIndication;
			case PassarMessageTypeList.Codes.NT043:
				return MessageSubTypeCodeList.Codes.PassarInventoryRequest;
			case PassarMessageTypeList.Codes.NT146:
				return MessageSubTypeCodeList.Codes.PassarNonArrivedTransitMovementInformation;
			case PassarMessageTypeList.Codes.NT021:
				return MessageSubTypeCodeList.Codes.PassarNctsPayloadRequestGoodsDeclarationResponse;
			case PassarMessageTypeList.Codes.NT504:
				return MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse;
			case PassarMessageTypeList.Codes.NT528:
				return MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse;
			default:
				return MessageSubTypeCodeList.Codes.Undefined;
		}
	}
}
