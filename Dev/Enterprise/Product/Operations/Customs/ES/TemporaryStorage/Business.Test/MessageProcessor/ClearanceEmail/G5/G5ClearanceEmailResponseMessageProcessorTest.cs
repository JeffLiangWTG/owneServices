using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

public class G5ClearanceEmailResponseMessageProcessorTest : ESCommonResponseMessageProcessorTest<G5ClearanceEmailResponseMessageProcessor, TemporaryStorageHeader, IG5ClearanceEmailProvider>
{
	public void TestProcessEmail_G5R()
	{
		var mailBody = GetMailBody();
		var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			var querySUM = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySUM.AddToFilter(CusEntryNumSchema.CE_ParentID, temporaryStorageHeader.PK);
			var cusEntryNumberSummary = Factory.LoadTop1<CusEntryNumber>(querySUM);

			AssertNotNull("cusEntryNumberSummary should not be null", cusEntryNumberSummary);
			AssertEquals("cusEntryNumberSummary.CE_EntryNum", G4MRNCode, cusEntryNumberSummary.CE_EntryNum);

			AssertEquals("Message Status", OriginalMessageStatus, temporaryStorageHeader.AMA_MessageStatus);
			AssertEquals("Customs Status", EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance, temporaryStorageHeader.CustomsStatus);

			GenericCommonAssertProcessResponseOthers(message, emStatus: EDIMessage.Status.Received, messageSubType: "ACC");
		});
	}

	public void TestProcessEmail_ErrorWhenG5E()
	{
		temporaryStorageHeader.AMA_MessageType = "G5E";
		var mailBody = GetMailBody();
		var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, message.EM_Status);

			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to find business object for message ", concatenatedUserLogStrings);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_MessageType = "G5R";
		temporaryStorageHeader.AMA_JobReference = ApplicationReference;
		temporaryStorageHeader.CustomsStatus = OriginalEntryStatus;
		temporaryStorageHeader.AMA_MessageStatus = OriginalMessageStatus;
		temporaryStorageHeader.MRN = MRNCode;
	}
	TemporaryStorageHeader temporaryStorageHeader;

	ZString OriginalMessageStatus => "INI";
	const string MRNCode = "25ESG5G000000749Y0";
	const string G4MRNCode = "25ESG4A000000263U0";

	protected override G5ClearanceEmailResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new G5ClearanceEmailResponseMessageProcessor(logger);

	protected override ZString GetExpectedProcessorFriendlyName() => "G5 Clearance Email Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.G5ClearanceEmail };

	protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
	{
		var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_MessageType = "G5R";
		temporaryStorageHeader.MRN = MRNCode;

		var message = Factory.New<TestEdiMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageType = MessageType;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageSubType = "AAA";
		message.EM_ApplicationReference = MRNCode;
		message.EM_MessageText = GetMailBody();

		Factory.Save();

		return message;
	}

	protected override void AssertLoggerMessagesWhenPreProcessMessageWrongSessionGUID()
	{
		AssertContains("logger", "Unable to find business object for message", logger.UserLogStrings[0]);
	}

	protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
	{
		var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
		AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
		AssertContains("logger exception", "Email Body doesn't have the correct data", concatenatedUserLogStrings);
		AssertEquals("EM_MessageInterpretation",
				string.Format("<H3>Processor Failure</H3><br>" +
				"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
				"<H4>Exception: Email Body doesn't have the correct data</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
				, message.EM_MessageInterpretation);
	}

	ZString GetMailBody()
	{
		return @"La aduana 9998 - Pruebas ha despachado en recepción el G5 con MRN 25ESG5G000000749Y0 al que se le asignó un circuito naranja y ampara el movimiento de mercancías en depósito temporal entre los ADT ES009998DDDD02 y ES009999000002.
 
La notificación de recepción se produjo el 24-04-2025 a las 15:11:48 h. y el despacho de recepción el 24-04-2025 a las 15:11:48 h..
 
Puede consultar los detalles del G5 en el siguiente enlace de la sede electrónica de la AEAT:
 
https://urldefense.com/v3/__https://preintranet.dit.aeat/wlpl/ADDS-JDIT/CtrlG5gSede?op=DDS&tipDec=R&mrn=25ESG5G000000749Y0__;!!Na5NE8kfbMIR6Ys!qSdZ-jMkATJ6Z9hyzPKUAfuZpqvHMoRB536iLSOGlVPJeu6Zop1Bj8Ra02hNs9ZziqivynGJrt4Z44DxK0TtuwzmlGpBlbTxUSUZKpDfzz9Z$
 
Con la información declarada en el G5 de recepción y las posibles modificaciones efectuadas por la aduana durante la gestión del despacho de recepción se ha generado automáticamente un G4 en la aduana de destino 9999 - Pruebas al que se ha asignado el MRN 25ESG4A000000263U0. La mercancía se encuentra disponible para su datado mediante las declaraciones aduaneras oportunas.
 
Por favor, no responda a este mensaje. Se trata de un envío automatizado desde una dirección de correo no atendida.
";
	}
}
