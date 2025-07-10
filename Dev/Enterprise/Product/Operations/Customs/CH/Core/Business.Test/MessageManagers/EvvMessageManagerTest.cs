using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvMessageManager))]
sealed class EvvMessageManagerTest : TestCaseWithFactory
{
	public void TestGenerateMessages()
	{
		var credential = CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();
		var entryHeader = CreateEntryHeader(JobMessageTypeList.Codes.Export);
		var sendingObject = new EvvRequestSendingObject(entryHeader, "MRN100", 0, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties);
		var messageManager = new EvvMessageManager(sendingObject);
		var messages = messageManager.GenerateMessages();
		CombineAssertions(() =>
		{
			AssertEquals("count", 1, messages.Length);
			AssertMessage("[0]", messages[0], entryHeader.PK, credential, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties);
			AssertMessageText("[0]", messages[0], EvvDocumentType.Codes.TaxationDecisionCustomsDuties);
			AssertEvent(entryHeader, $"|STU=Requested|TYP={EvvDocumentType.Codes.TaxationDecisionCustomsDuties}");
		});
	}

	public void TestRollbackOnSaveFailed()
	{
		var entryHeader = CreateEntryHeader(JobMessageTypeList.Codes.Import);
		var sendingObject = new EvvRequestSendingObject(entryHeader, "MRN100", 0, "subType");
		var messageManager = new EvvMessageManager(sendingObject);
		entryHeader.Logs.AddNew(Events.ElectronicAssessmentDecisionStatus);
		messageManager.RollbackOnSaveFailed();
		AssertEquals(0, entryHeader.Logs.GetAllLogs().Count);
	}

	public void TestMessageNum()
	{
		var entryHeader = CreateEntryHeader(JobMessageTypeList.Codes.Export);
		var sendingObject1 = new EvvRequestSendingObject(entryHeader, "MRN100", 0, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties);
		var sendingObject2 = new EvvRequestSendingObject(entryHeader, "MRN100", 0, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties);
		var messageManager1 = new EvvMessageManager(sendingObject1);
		var messageManager2 = new EvvMessageManager(sendingObject2);
		var messages1 = messageManager1.GenerateMessages();
		var messages2 = messageManager2.GenerateMessages();
		AssertNotEquals(messages1[0], messages2[0]);
	}

	void AssertMessage(string assertionMesssage, EDIMessage message, ZGuid entryHeaderPK, GlbExternalPassword credential, string messageSubType)
	{
		AssertEquals($"{assertionMesssage} EM_ApplicationCode", "CHC", message.EM_ApplicationCode);
		AssertEquals($"{assertionMesssage} EM_MessageType", "EVV", message.EM_MessageType);
		AssertEquals($"{assertionMesssage} EM_MessageSubType", messageSubType, message.EM_MessageSubType);
		AssertEquals($"{assertionMesssage} EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
		AssertEquals($"{assertionMesssage} EM_Status", "QUE", message.EM_Status);
		AssertEquals($"{assertionMesssage} EM_LinkTable", CusEntryHeader.Schema.TableName, message.EM_LinkTable);
		AssertEquals($"{assertionMesssage} EM_LinkUniqueID", entryHeaderPK, message.EM_LinkUniqueID);
		AssertEquals($"{assertionMesssage} EM_GP", credential.PK, message.EM_GP);
	}

	void AssertMessageText(string assertionMesssage, EDIMessage message, string documentType)
	{
		Assert($"{assertionMesssage} documentType \n{message.EM_MessageText}", message.EM_MessageText.Contains($"documentType>{documentType}<"));
	}

	void AssertEvent(CusEntryHeader entryHeader, string expectedReference)
	{
		var testHelper = new SendingObjectsTestHelper();
		testHelper.AssertEvent(entryHeader.Logs.MostRecentLogByEventTime(Events.ElectronicAssessmentDecisionStatus, expectedReference), entryHeader);
	}

	CusEntryHeader CreateEntryHeader(string messageType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return entryHeader;
	}
}
