using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(MessagePrettyFormatterFactory))]
sealed class MessagePrettyFormatterFactoryTest : TestCaseWithFactory
{
	public void TestGetMessageFormatter() => CombineAssertions(() =>
	{
		var ediMessage = Factory.New<CHEDIMessage>();

		ediMessage.EM_MessageType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.Import;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Accepted;
		AssertType<AcceptanceMessagePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		AssertType<XMLSchemaErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.RuleError;
		AssertType<RuleErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.Export;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Accepted;
		AssertType<AcceptanceMessagePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		AssertType<XMLSchemaErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.RuleError;
		AssertType<RuleErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.EBD;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Accepted;
		AssertType<EbdResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.CustomsRejected;
		AssertType<EbdResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.ECM;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Accepted;
		AssertType<EComAcceptancePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Request;
		AssertType<EComRequestPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.RuleError;
		AssertType<EComRuleErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		AssertType<EComXMLSchemaErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.RuleError;
		AssertType<EvvRuleErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.XmlSchemaError;
		AssertType<EvvXMLSchemaErrorPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertNull(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.PassarNcts;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Rejected;
		AssertType<UniversalEventPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Acknowledged;
		AssertType<UniversalEventPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertType<UniversalEventXmlPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.MSL;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Rejected;
		AssertType<UniversalEventPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertType<UniversalEventXmlPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.MSG;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Rejected;
		AssertType<UniversalEventPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Accepted;
		AssertType<UniversalEventXmlPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarTechnicalError;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNC909());
		AssertNotNull("NCTS message formatters recognized", GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarExportPayloadRequestGoodsDeclarationResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE021());
		AssertType<UniversalEventXmlPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE060());
		AssertType<NE060ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = ZString.Empty;
		AssertType<UniversalEventXmlPrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.MSG;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarTechnicalError;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNC909(Guid.NewGuid().ToString()));
		AssertType<NC909ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarActivationResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNC124());
		AssertType<NC124ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE131());
		AssertType<NE131ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE004());
		AssertType<Nxx04ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE009());
		AssertType<NE009ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse;
		ediMessage.EM_MessageText = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE028());
		AssertType<NE028ResponsePrettyFormatter>(GetAssertionMessage(), GetMessageFormatter());

		IMessagePrettyFormatter GetMessageFormatter() => MessagePrettyFormatterFactory.GetMessageFormatter(ediMessage);
		string GetAssertionMessage() => $"Type={ediMessage.EM_MessageType} SubType={ediMessage.EM_MessageSubType} IsUniversalEvent={ediMessage.IsUniversalEvent}";
	});
}
