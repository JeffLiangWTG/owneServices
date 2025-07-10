using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class PassarResponseAnalyzerExtensionsTest : TestCase
{
	public void TestGetMessageSubType() => CombineAssertions(() =>
	{
		(string passarMessageType, string ediMessageSubType)[] pairs = new[]
		{
			(PassarMessageTypeList.Codes.NC016, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NC084, MessageSubTypeCodeList.Codes.PassarDocumentNotification),
			(PassarMessageTypeList.Codes.NC123, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NC124, MessageSubTypeCodeList.Codes.PassarActivationResponse),
			(PassarMessageTypeList.Codes.NC909, MessageSubTypeCodeList.Codes.PassarTechnicalError),
			(PassarMessageTypeList.Codes.NE004, MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse),
			(PassarMessageTypeList.Codes.NE009, MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected),
			(PassarMessageTypeList.Codes.NE013, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NE014, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NE015, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NE021, MessageSubTypeCodeList.Codes.PassarExportPayloadRequestGoodsDeclarationResponse),
			(PassarMessageTypeList.Codes.NE028, MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse),
			(PassarMessageTypeList.Codes.NE029, MessageSubTypeCodeList.Codes.PassarExportReleaseResponse),
			(PassarMessageTypeList.Codes.NE060, MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification),
			(PassarMessageTypeList.Codes.NE069, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NE083, MessageSubTypeCodeList.Codes.PassarExportDeclarationIssuanceOfAssessmentDecision),
			(PassarMessageTypeList.Codes.NE096, MessageSubTypeCodeList.Codes.PassarExportDecisionRectification),
			(PassarMessageTypeList.Codes.NE130, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NE131, MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse),
			(PassarMessageTypeList.Codes.NI013, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NI014, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NI015, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NI016, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT004, MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse),
			(PassarMessageTypeList.Codes.NT007, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT008, MessageSubTypeCodeList.Codes.PassarArrivalResponse),
			(PassarMessageTypeList.Codes.NT009, MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse),
			(PassarMessageTypeList.Codes.NT013, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT014, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT015, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT019, MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies),
			(PassarMessageTypeList.Codes.NT021, MessageSubTypeCodeList.Codes.PassarNctsPayloadRequestGoodsDeclarationResponse),
			(PassarMessageTypeList.Codes.NT025, MessageSubTypeCodeList.Codes.PassarArrivalIndication),
			(PassarMessageTypeList.Codes.NT028, MessageSubTypeCodeList.Codes.PassarDepartureResponse),
			(PassarMessageTypeList.Codes.NT029, MessageSubTypeCodeList.Codes.PassarTransitReleased),
			(PassarMessageTypeList.Codes.NT035, MessageSubTypeCodeList.Codes.PassarRecoveryNotification),
			(PassarMessageTypeList.Codes.NT043, MessageSubTypeCodeList.Codes.PassarInventoryRequest),
			(PassarMessageTypeList.Codes.NT044, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT045, MessageSubTypeCodeList.Codes.PassarTransitClosed),
			(PassarMessageTypeList.Codes.NT055, MessageSubTypeCodeList.Codes.PassarInvalidGuarantee),
			(PassarMessageTypeList.Codes.NT057, MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse),
			(PassarMessageTypeList.Codes.NT060, MessageSubTypeCodeList.Codes.PassarIntentionToControl),
			(PassarMessageTypeList.Codes.NT061, MessageSubTypeCodeList.Codes.PassarControlDecisionNotification),
			(PassarMessageTypeList.Codes.NT140, MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit),
			(PassarMessageTypeList.Codes.NT141, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT146, MessageSubTypeCodeList.Codes.PassarNonArrivedTransitMovementInformation),
			(PassarMessageTypeList.Codes.NT182, MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney),
			(PassarMessageTypeList.Codes.NT504, MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse),
			(PassarMessageTypeList.Codes.NT513, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT515, MessageSubTypeCodeList.Codes.Undefined),
			(PassarMessageTypeList.Codes.NT528, MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse),
		};

		var analyzer = new Mock<IPassarResponseAnalyzer>();

		foreach (var pair in pairs)
		{
			analyzer.Setup(x => x.MessageType).Returns(pair.passarMessageType);
			AssertEquals(pair.passarMessageType, pair.ediMessageSubType, analyzer.Object.GetMessageSubType());
		}

		foreach (var passarMessageType in new PassarMessageTypeList().GetAllCodes().Except(pairs.Select(x => x.passarMessageType)))
		{
			Fail($"Missing test for PassarMessageType {passarMessageType}");
		}
	});
}
