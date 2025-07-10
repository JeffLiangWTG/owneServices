using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(XTConfigurationResponseMessageProcessor))]
sealed class XTConfigurationResponseMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessageError()
	{
		var (company, ediMessageToProcess) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, EDIInterchangeTypeList.Codes.Configuration, MessagingConstants.xTCustomConfiguration.InterchangeTypes, MessageSubTypeCodeList.Codes.Accepted, TestingData.XTConfigurationResponseMessageSuccess, outgoingInterchangeBodyText: TestingData.GetXTConfigurationRequestMessage(string.Empty), companyCode: "CH1");
		MessageProcessor.ProcessMessage(ediMessageToProcess, new LoggingInformationForTesting());
		AssertEquals("Status", EDIMessage.Status.Discarded, ediMessageToProcess.EM_Status);
	}

	public void TestProcessMessage_Success() => AssertProcessMessage(TestingData.XTConfigurationResponseMessageSuccess, EventReferenceConstants.Types.XHC_OK);

	public void TestProcessMessage_Failure() => AssertProcessMessage(TestingData.XTConfigurationResponseMessageFailure, EventReferenceConstants.Types.XHC_Error);

	void AssertProcessMessage(string messageContent, string expectedEventType) => CombineAssertions(() =>
	{
		const string companyCode = "CH1";
		var (company, ediMessageToProcess) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, EDIInterchangeTypeList.Codes.Configuration, MessagingConstants.xTCustomConfiguration.InterchangeTypes, MessageSubTypeCodeList.Codes.Accepted, messageContent, outgoingInterchangeBodyText: TestingData.GetXTConfigurationRequestMessage(companyCode), companyCode: companyCode);

		ediMessageToProcess.EM_LinkedObject = company;

		MessageProcessor.ProcessMessage(ediMessageToProcess, new LoggingInformationForTesting());
		AssertEquals("Status", EDIMessage.Status.ProcessedOK, ediMessageToProcess.EM_Status);

		var logEvent = company.Logs.MostRecentLogByEventTime(Events.MiscellaneousEvent);
		AssertNotNull("Event written", logEvent);
		AssertEquals("Event Reference", $"|ITN={ediMessageToProcess.Interchange.EI_InterchangeNum}|TYP={expectedEventType}", logEvent?.SL_Reference);
	});

	public void TestGetLinkedBusinessObjectMetaData_WithLinkedObject_ReturnsFromLinkedObject()
	{
		const string companyCode = "CH1";
		var messageContent = TestingData.XTConfigurationResponseMessageSuccess;
		var (company, ediMessageToProcess) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, EDIInterchangeTypeList.Codes.Configuration, MessagingConstants.xTCustomConfiguration.InterchangeTypes, MessageSubTypeCodeList.Codes.Accepted, messageContent, outgoingInterchangeBodyText: TestingData.GetXTConfigurationRequestMessage(companyCode), companyCode: companyCode);

		var result = MessageProcessor.GetLinkedBusinessObjectMetaData(ediMessageToProcess, new LoggingInformation());
		AssertEquals("LinkTableName should match the company table", GlbCompany.Schema.TableName, result.ReturnValue.LinkTableName);
		AssertEquals("LinkUniqueId should match the company PK", company.PK, result.ReturnValue.LinkUniqueID);
		AssertEquals("BranchPk should match the message.EM_GB", ediMessageToProcess.EM_GB, result.ReturnValue.BranchPk);
		AssertEquals("JobNumber should match the company code", companyCode, result.ReturnValue.JobNumber);
	}

	public void TestGetLinkedBusinessObjectMetaData_WithoutLinkedObject_ReturnsDefault()
	{
		var messageContent = TestingData.XTConfigurationResponseMessageSuccess;

		var ediMessageToProcess = MessageProcessorTestHelper.CreateMessagesAndInterchangesWithEmptyCompany(Factory, EDIInterchangeTypeList.Codes.Configuration, MessagingConstants.xTCustomConfiguration.InterchangeTypes, MessageSubTypeCodeList.Codes.Accepted, messageContent, TestingData.GetXTConfigurationRequestMessage(""));
		ediMessageToProcess.EM_LinkTable = "ABC";
		ediMessageToProcess.EM_LinkUniqueID = ZGuid.NewZGuid();

		var result = MessageProcessor.GetLinkedBusinessObjectMetaData(ediMessageToProcess, new LoggingInformation());
		AssertEquals("LinkTableName should match the message LinkTable", ediMessageToProcess.EM_LinkTable, result.ReturnValue.LinkTableName);
		AssertEquals("LinkUniqueId should match the message LinkUniqueId", ediMessageToProcess.EM_LinkUniqueID, result.ReturnValue.LinkUniqueID);
		AssertEquals("BranchPk should match the message.EM_GB", ediMessageToProcess.EM_GB, result.ReturnValue.BranchPk);
		AssertEquals("JobNumber should be empty", ZString.Empty, result.ReturnValue.JobNumber);
	}

	XTConfigurationResponseMessageProcessor MessageProcessor => messageProcessor ??= new XTConfigurationResponseMessageProcessor();
	XTConfigurationResponseMessageProcessor messageProcessor;
}
