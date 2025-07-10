using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsMessagePrettyFormatterProvider))]
sealed class NctsMessagePrettyFormatterProviderTest : TestCaseWithFactory
{
	public void TestGetMessageFormatter() => CombineAssertions(() =>
	{
		var ediMessage = Factory.New<CHEDIMessage>();

		ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.MSG;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarTechnicalError;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarDepartureAmendmentResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT004());
		AssertType<Nxx04ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationAmendmentResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT504());
		AssertType<Nxx04ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarArrivalResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT008());
		AssertType<NT008ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarDepartureWithdrawalResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT009());
		AssertType<NT009ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarArrivalIndication;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT025());
		AssertType<NT025ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarDepartureResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT028(correlationId: Guid.NewGuid().ToString()));
		AssertType<NTx28ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarNationalTransitDeclarationResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT528());
		AssertType<NTx28ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarTransitReleased;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT029());
		AssertType<NT029ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarRecoveryNotification;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT035());
		AssertType<NT035ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarInvalidGuarantee;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT055());
		AssertType<NT055ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT057());
		AssertType<NT057ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarIntentionToControl;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT060());
		AssertType<NT060ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarEventDuringTheJourney;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT182());
		AssertType<NT182ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarControlDecisionNotification;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT061());
		AssertType<NT061ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarEnquiryOfNotArrivedTransit;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT140());
		AssertType<NT140ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarTransitDiscrepancies;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT019());
		AssertType<NT019ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarTransitClosed;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT045());
		AssertType<NT045ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		IMessagePrettyFormatter GetMessageFormatter() => new NctsMessagePrettyFormatterProvider().GetFormatter(ediMessage);
		string GetAssertionMessage() => $"Type={ediMessage.EM_MessageType} SubType={ediMessage.EM_MessageSubType}";
	});
}
