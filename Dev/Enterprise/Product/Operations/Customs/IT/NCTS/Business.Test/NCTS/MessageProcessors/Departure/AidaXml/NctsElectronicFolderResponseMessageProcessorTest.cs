using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsElectronicFolderResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<ElectronicFolderResponseMessageProcessor>
{
	public void TestProcessResponse_SetCustomsChannel_CodeZero()
	{
		var messageContent = ManifestResourceHelper.ReadManifestResourceContent(ElectronicFolderResponse_CodeZero_ResourceName);
		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageContent, responseMessageType: MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType, sentMessageType: EDIMessageTypeList.Codes.ElectronicFolderQuery);
		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		AssertEquals("VM", nctsHeader.MovementHeader.BM_ControlChannel);
	}

	public void TestProcessResponse_SetCustomsChannel_CodeD_024()
	{
		var messageContent = ManifestResourceHelper.ReadManifestResourceContent(ElectronicFolderResponse_CodeD_024_ResourceName);
		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageContent, responseMessageType: MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType, sentMessageType: EDIMessageTypeList.Codes.ElectronicFolderQuery);
		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);
		AssertEquals("CA", nctsHeader.MovementHeader.BM_ControlChannel);
	}

	protected override ElectronicFolderResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new(logger);

	const string ElectronicFolderResponse_CodeZero_ResourceName = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.ElectronicFolderResponse_CodeZero.xml";
	const string ElectronicFolderResponse_CodeD_024_ResourceName = "Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.ElectronicFolderResponse_CodeD_024.xml";
}
