using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EComplaintMessageManager))]
sealed class EComplaintMessageManagerTest : TestCaseWithFactory
{
	public void TestBusinessObject() => AssertEquals(SendingObject, Manager.BusinessObject);

	public void TestGenerateMessages()
	{
		var testHelper = new SendingObjectsTestHelper();
		var credential = CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
		{
			CombineAssertions(() =>
			{
				var entryHeader = SendingObject.EntryHeader;
				var entryLine1 = entryHeader.AllEntryLines.AddNew();
				entryLine1.CL_LineNumber = 1;

				var sendingLine1 = SendingObject.SendingObjectLines.AddNew();
				sendingLine1.Location = EComplaintLocationList.Codes.Header;
				sendingLine1.FieldName = "Field1";
				sendingLine1.Remark = "Remark 1";
				var sendingLine2 = SendingObject.SendingObjectLines.AddNew();
				sendingLine2.Location = EComplaintLocationList.Codes.Line;
				sendingLine2.EntryLinePK = entryLine1.PK;
				sendingLine2.FieldName = "Field2";
				sendingLine2.Remark = "Remark 2";

				var messages = Manager.GenerateMessages();

				Factory.Save();

				testHelper.AssertEDIMessages(messages, SendingObject.MessageTypeForEDIMessage, SendingObject.MessageSubTypeForEDIMessage, entryHeader.PK, credential.PK, ApplicationCodeList.Codes.CHCustomsEdec);

				Assert("EM_MessageText generated correctly (place holder replaced with message num)", messages[0].EM_MessageText.Contains($"<requestorCorrelationID>{messages[0].EM_MessageNum}</requestorCorrelationID>"));
			});
		}
	}

	public void TestAfterGenerateMessage()
	{
		var entryHeader = SendingObject.EntryHeader;
		var sendingLine = SendingObject.SendingObjectLines.AddNew();
		sendingLine.Location = EComplaintLocationList.Codes.Header;
		sendingLine.FieldName = "Field1";
		sendingLine.Remark = "Remark 1";

		var entryHeaderLogCountBefore = entryHeader.Logs.GetAllLogs().Count;

		Manager.GenerateMessages();

		var addedLog = entryHeader.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault();

		CombineAssertions(() =>
		{
			AssertEquals("LastEComplaintStatus should be updated", EComplaintStatusList.Codes.Sent, entryHeader.CH_LastEComplaintStatus);

			AssertEquals("no logs before generating message", 0, entryHeaderLogCountBefore);
			AssertEquals("no logs after generating message", 1, entryHeader.Logs.GetAllLogs().Count);
			AssertEquals("added log event description", "ECom: Status Change", addedLog.SL_EventDescription);
			AssertEquals("added log reference", "|NEW=SNT", addedLog.SL_Reference);
		});
	}

	public void TestRollbackOnSaveFailed()
	{
		var entryHeader = SendingObject.EntryHeader;
		var sendingLine = SendingObject.SendingObjectLines.AddNew();
		sendingLine.Location = EComplaintLocationList.Codes.Header;
		sendingLine.FieldName = "Field1";
		sendingLine.Remark = "Remark 1";
		entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Rejected;
		Factory.Save();

		var entryHeaderLogCountBefore = entryHeader.Logs.GetAllLogs().Count;

		Factory.Saving += (f) => throw new ZSaveException(new ZDataExceptionForTesting("*** TEST ERROR ***"), f);
		SendingObject.SendMessagesAndSave(MessageManagerFactory.CreateNew);

		CombineAssertions(() =>
		{
			AssertEquals("Messages deleted on saving failed", 0, entryHeader.Messages.Count);
			AssertEquals("LastEComplaintStatus should not be updated on saving failed", EComplaintStatusList.Codes.Rejected, entryHeader.CH_LastEComplaintStatus);
			AssertEquals("# of created log events on entry header", 0, entryHeader.Logs.GetAllLogs().Count - entryHeaderLogCountBefore);
		});
	}

	EComplaintMessageSendingObject SendingObject => sendingObject ?? (sendingObject = GetMessageSender());
	EComplaintMessageSendingObject sendingObject;

	EComplaintMessageManager Manager => manager ?? (manager = new EComplaintMessageManager(SendingObject));
	EComplaintMessageManager manager;

	EComplaintMessageSendingObject GetMessageSender()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Description = "DESCRIPTION";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.EntryNumber = "NO1";
		return new EComplaintMessageSendingObject(entryHeader);
	}
}
