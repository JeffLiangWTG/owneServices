using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CHEDIMessage))]
sealed class CHEDIMessageTest : EDIMessageTest
{
	public void TestMessageDefaults()
	{
		var message = Factory.New<CHEDIMessage>();
		AssertEquals(EDIInterchange.ApplicationCodes.CHCustomsEdec, message.EM_ApplicationCode);
	}

	public void TestNewAndLoadType()
	{
		var testMessage = Factory.NewWithValidTestData<EDIMessage>();
		testMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		testMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CHCustomsEdec;
		testMessage.EM_Status = EDIInterchangeStatusList.Codes.Queued;
		testMessage.EM_MessageText = "BBB";
		testMessage.EM_MessageType = "";
		testMessage.EM_MessageNum = "1234567890123456789012345678901234A";
		AssertEquals("Base.EDIMessage", typeof(EDIMessage), testMessage.GetType());
		Factory.Save();
		var newFactory = new BusinessObjectFactory();
		testMessage = newFactory.Load<CHEDIMessage>(testMessage.PK);
		AssertEquals("CH.EDIMessage", typeof(CHEDIMessage), testMessage.GetType());
	}

	public void TestUsesPlaceHolders()
	{
		var ediMessage = Factory.New<CHEDIMessage>();

		CombineAssertions(() =>
		{
			AssertEquals("UsesPlaceHolders false for standard", false, ediMessage.UsesPlaceHolders);

			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.Import;
			AssertEquals("UsesPlaceHoilders true for IMP Messages", false, ediMessage.UsesPlaceHolders);

			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.Export;
			AssertEquals("UsesPlaceHoilders true for EXP Messages", false, ediMessage.UsesPlaceHolders);

			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.EBD;
			AssertEquals("UsesPlaceHoilders true for EBD Messages", false, ediMessage.UsesPlaceHolders);

			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.ECM;
			AssertEquals("UsesPlaceHoilders true for ECM Messages", true, ediMessage.UsesPlaceHolders);
		});
	}

	public void TestEcmMessage_MessageNumberPlaceHolderHtmlReplacedWithMessageNum()
	{
		CombineAssertions(() =>
		{
			var ediMessage = Factory.New<CHEDIMessage>();
			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.Import;
			ediMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolderHtml;
			Factory.Save();

			AssertNotEquals("MessageNumberPlaceHolder not replaced with EM_MessageNum", ediMessage.EM_MessageText, ediMessage.EM_MessageNum);

			ediMessage = Factory.New<CHEDIMessage>();
			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.ECM;
			Factory.Save();

			AssertEquals("EM_MessageText empty", true, ediMessage.EM_MessageText.IsEmpty);
			AssertNotEquals("MessageNumberPlaceHolder not replaced with EM_MessageNum", ediMessage.EM_MessageText, ediMessage.EM_MessageNum);

			ediMessage = Factory.New<CHEDIMessage>();
			ediMessage.EM_MessageType = MessageTypeCodeList.Codes.ECM;
			ediMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolderHtml;
			Factory.Save();

			AssertEquals("MessageNumberPlaceHolder replaced with EM_MessageNum", ediMessage.EM_MessageText, ediMessage.EM_MessageNum);
		});
	}

	public void TestDocumentSupporter()
	{
		var ediMessage = Factory.New<CHEDIMessage>();

		AssertType<CHEDIMessageDocumentSupporter>(ediMessage.DocumentSupporter);
	}

	public void TestMessageAnalyzer()
	{
		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.Import, string.Empty, TestingData.InputEdecResponseAcceptanceXml,
		typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations.Version4_0.GoodsDeclarationsResponseAnalyzer),
		typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations.Version4_0.GoodsDeclarationsResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.Import, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.Export, string.Empty, TestingData.InputEdecResponseRejectionXml,
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations.Version4_0.GoodsDeclarationsResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations.Version4_0.GoodsDeclarationsResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.Export, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.EBD, string.Empty, EbdDocumentImportResponseMessageProcessorTest.AcceptanceResponse,
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd.Version0_2.DocumentImportResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd.Version0_2.DocumentImportResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.EBD, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.ECM, MessageSubTypeCodeList.Codes.Request, EComRcvRequestPrettyFormatterTest.MessageText,
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom.Version1_0.EdecComplaintRequestAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom.Version1_0.EdecComplaintRequestProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.ECM, string.Empty, TestingData.InputEComResponseAcceptance,
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom.Version1_0.EdecComplaintResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom.Version1_0.EdecComplaintResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.ECM, string.Empty, EComTrxRequestPrettyFormatterTest.MessageText,
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom.Version1_0.EdecComplaintRequestAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom.Version1_0.EdecComplaintRequestProvider));

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.EVV, string.Empty, TestingData.InputEvvResponseDTY(),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv.Version3_0.EvvResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv.Version3_0.EvvTaxationDecisionProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.EVV, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.BOR, string.Empty, TestingData.EdecBordereauTypeBordereauListXml(),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau.Version1_0.EdecBordereauResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau.Version1_0.EdecBordereauResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.BOR, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.MSG, string.Empty, UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNC909()),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar.NC909V2ResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar.NC909V2ResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.MSG, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.MSG, string.Empty, UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNT008()),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar.NT008V1ResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar.NT008V1ResponseProvider));
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.MSG, string.Empty);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, MessageTypeCodeList.Codes.MSG, string.Empty, UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.CharteraDocument),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera.DocumentV2ResponseAnalyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera.DocumentV2ResponseProvider),
			applicationCode: ApplicationCodeList.Codes.CHCustomsCharteraOutput);
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.REQ, MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest, CharteraOutputDocumentRejectionMessageProcessorTest.GetDocumentDeliveryRequestForTesting(Guid.NewGuid().ToString()),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera.DocumentDeliveryRequestV1Analyzer),
			typeof(CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera.DocumentDeliveryRequestV1Provider),
			applicationCode: ApplicationCodeList.Codes.CHCustomsCharteraOutput);
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, MessageTypeCodeList.Codes.MSG, string.Empty, applicationCode: ApplicationCodeList.Codes.CHCustomsCharteraOutput);

		AssertAnalyzerType(ReceiveTransmitList.Codes.Receive, string.Empty, string.Empty);
		AssertAnalyzerType(ReceiveTransmitList.Codes.Transmit, string.Empty, string.Empty);

		void AssertAnalyzerType(string receiveTransmit, string messageType, string messageSubType, string messageText = "XXX", Type expectedAnalyzerType = null, Type expectedDetailType = null, string applicationCode = "")
		{
			var ediMessage = Factory.New<CHEDIMessage>();
			ediMessage.EM_ApplicationCode = applicationCode;
			ediMessage.EM_ReceiveTransmit = receiveTransmit;
			ediMessage.EM_MessageType = messageType;
			ediMessage.EM_MessageSubType = messageSubType;
			ediMessage.EM_MessageText = messageText;

			var message = $"EM_ApplicationCode={ediMessage.EM_ApplicationCode} EM_ReceiveTransmit={ediMessage.EM_ReceiveTransmit} EM_MessageType={ediMessage.EM_MessageType} EM_MessageSubType={ediMessage.EM_MessageSubType}";

			if (expectedAnalyzerType != null)
			{
				AssertType(message + " MessageAnalyzer", expectedAnalyzerType, ediMessage.MessageAnalyzer);
			}
			else
			{
				AssertNull(message + " MessageAnalyzer", ediMessage.MessageAnalyzer);
			}

			if (expectedDetailType != null)
			{
				AssertType(message + " MessageDetail", expectedDetailType, ediMessage.MessageDetail);
			}
			else
			{
				AssertNull(message + " MessageDetail", ediMessage.MessageDetail);
			}
		}
	}

	public void TestIsUniversalEvent() => CombineAssertions(() =>
	{
		var ediMessage = Factory.New<CHEDIMessage>();
		foreach (var messageType in new MessageTypeCodeList().GetAllCodes())
		{
			ediMessage.EM_MessageType = messageType;
			switch (messageType)
			{
				case MessageTypeCodeList.Codes.PassarNcts:
				case MessageTypeCodeList.Codes.MSL:
				case MessageTypeCodeList.Codes.MSG:
				case MessageTypeCodeList.Codes.REQ:
					AssertIsUniversalEvent(messageType, true);
					break;
				case MessageTypeCodeList.Codes.Import:
				case MessageTypeCodeList.Codes.EBD:
				case MessageTypeCodeList.Codes.ECM:
				case MessageTypeCodeList.Codes.Export:
					ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.Rejected;
					AssertIsUniversalEvent(messageType, true);
					ediMessage.EM_MessageSubType = ZString.Empty;
					AssertIsUniversalEvent(messageType, false);
					break;
				default:
					AssertIsUniversalEvent(messageType, false);
					break;
			}
		}

		void AssertIsUniversalEvent(string messageType, bool expectedResult)
		{
			ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			AssertEquals($"MessageType={messageType}", expectedResult, ediMessage.IsUniversalEvent);
			ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			AssertEquals($"MessageType={messageType}", false, ediMessage.IsUniversalEvent);
		}
	});

	public void TestUniversalEventData() => CombineAssertions(() =>
	{
		var ediMessage = Factory.New<CHEDIMessage>();

		AssertNull("not a UniversalEvent", ediMessage.UniversalEventData);

		ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.PassarNcts;
		ediMessage.EM_MessageText = TestingData.InputUniversalEvent;
		AssertEquals("event reason", "test event reason", ediMessage.UniversalEventData?.Reason);
		AssertSame("cached", ediMessage.UniversalEventData, ediMessage.UniversalEventData);

		ediMessage.EM_MessageText = TestingData.InputUniversalEvent.Replace("<Reason>test event reason</Reason>", "<Reason>test event reason 2</Reason>");
		AssertEquals("event reason", "test event reason 2", ediMessage.UniversalEventData?.Reason);
	});

	public void TestLinkedCusPollingTransactionType()
	{
		var messageSave = Factory.New<CHEDIMessage>();
		var transaction = Factory.New<CusPollingTransaction>();
		transaction.CPT_TransactionID = "1";
		transaction.CPT_ApplicationCode = CusPollingTransaction.ApplicationCodes.CHCustomsPassar;
		transaction.CPT_Type = CompanyPollingTransaction.TransactionTypes.MessageId;
		transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.New;
		messageSave.EM_LinkedObject = transaction;
		Factory.Save();

		var messageLoad = new BusinessObjectFactory().Load<CHEDIMessage>(messageSave.PK);
		AssertType<CusPollingTransaction>(messageLoad.EM_LinkedObject);
	}
}
