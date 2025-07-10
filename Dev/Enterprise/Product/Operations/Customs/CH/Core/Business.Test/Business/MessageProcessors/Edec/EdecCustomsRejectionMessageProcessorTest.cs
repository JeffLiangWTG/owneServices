using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecCustomsRejectionMessageProcessor))]
sealed class EdecCustomsRejectionMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { (JobMessageTypeList.Codes.Import, Events.DeclarationRejected), (JobMessageTypeList.Codes.Export, Events.DeclarationRejected) };

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.CustomsRejected;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EdecCustomsRejectionMessageProcessor(logger);

	protected override ZString ExpectedMessageFriendlyName => "Customs Rejection Message Processor";

	protected override ZString BGMReference => "0100011210809998";

	ZString customsRejectionType = "correctionRejection";

	#region Message Response

	protected override ZString ReceivedMessage => $@"<goodsDeclarationsResponse schemaVersion=""4.0"" xmlns=""http://www.e-dec.ch/xml/schema/edecResponse/v4"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.e-dec.ch/xml/schema/edecResponse/v4 http://www.ezv.admin.ch/pdf_linker.php?doc=edecResponse_v_4_0"">
		<goodsDeclarationRejection>
			<rejectionDate>2022-07-28</rejectionDate>
			<rejectionTime>11:36:24</rejectionTime>
			<errors>
				<customsRejection>
					<traderDeclarationNumber>{BGMReference}</traderDeclarationNumber>
					<traderReference>Test_NI_Korrektur</traderReference>
					<declarant>
						<traderIdentificationNumber>CHE375081047</traderIdentificationNumber>
						<declarantNumber>72</declarantNumber>
					</declarant>
					<type>{customsRejectionType}</type>
				</customsRejection>
			</errors>
		</goodsDeclarationRejection>
	</goodsDeclarationsResponse>";

	#endregion

	public void TestRejectedCorrectionMessageResponse() => AssertCustomsRejectedMessage("correctionRejection");

	public void TestRejectedCancellationMessageResponse() => AssertCustomsRejectedMessage("cancellationRejection");

	void AssertCustomsRejectedMessage(string rejectionType)
	{
		customsRejectionType = rejectionType;
		foreach (var (messageType, expectedEvent) in MessageTypes)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.Codes.Amendment;
			entryHeader.CH_BGMReference = BGMReference;

			var ediMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType, MessageSubType, messageText: ReceivedMessage);
			Factory.Save();

			ProcessMessage(ediMessage);
			Factory.Save();

			AssertEquals("CH_Status", CHLogicalStatusList.Codes.Invalid, entryHeader.CH_Status);
			AssertEquals("CH_PhaseStatus", PassarDeclarationPhaseList.Codes.Amendment, entryHeader.CH_PhaseStatus);
			AssertEquals(new ZDateTime(2022, 7, 28, 11, 36, 24), entryHeader.Logs.MostRecentLogByPostedTime(expectedEvent).SL_EventTime);

			entryHeader.CH_BGMReference = "11111";
		}
	}
}
