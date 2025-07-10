using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EbdMessageManager))]
sealed class EbdMessageManagerTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new EbdMessageManager(null));
	}

	public void TestMessageFriendlyName()
	{
		var manager = new EbdMessageManager(sendingObject);
		AssertEquals(MessageTypeCodeList.Descriptions.EBD, manager.MessageFriendlyName);
	}

	public void TestEDIMessage()
	{
		var manager = new EbdMessageManager(sendingObject);
		var messages = manager.GenerateMessages();
		CombineAssertions(() =>
		{
			AssertEquals("# of messages", messages.Length, 1);
			AssertEquals("EM_ApplicationCode", "CHC", messages[0].EM_ApplicationCode);
			AssertEquals("EM_MessageType", "EBD", messages[0].EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", "TRX", messages[0].EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", messages[0].EM_Status);
			AssertEquals("EM_LinkTable", "CusEntryHeader", messages[0].EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", entryHeader.PK, messages[0].EM_LinkUniqueID);
			AssertEquals("EM_GP", companyCrecdential.PK, messages[0].EM_GP);
		});
	}

	public void TestEDIMessageAttach()
	{
		var manager = new EbdMessageManager(sendingObject);
		var messages = manager.GenerateMessages();
		CombineAssertions(() =>
		{
			var attach = messages[0].MessageAttachments.OfType<EDIMessageAttach>().FirstOrDefault();
			AssertNotNull("EDIMessageAttach", attach);
			AssertEquals("EG_StorageDocsGuid", sendingObject.EDoc, attach.EG_StorageDocsGuid);
			AssertEquals("EG_FileName", sendingObject.Document.FileName, attach.EG_FileName);
		});
	}

	public void TestEDIMessage_Text()
	{
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";

		var manager = new EbdMessageManager(sendingObject);
		var messages = manager.GenerateMessages();

		var contentPlaceholder = Convert.ToBase64String(sendingObject.EDoc.ToGuid().ToByteArray());
		var expectedMessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ebdDocumentImportRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.ebd.ezv.admin.ch/xml/schema/ebdDocumentImportRequest/v1"">
  <uidNumber>123456</uidNumber>
  <customsDeclarationNumber>MRN100</customsDeclarationNumber>
  <accompanyingDocuments>
    <accompanyingDocument>
      <filename>Invoice.pdf</filename>
      <type>doc-type</type>
      <content>{contentPlaceholder}</content>
    </accompanyingDocument>
  </accompanyingDocuments>
</ebdDocumentImportRequest>";

		AssertXMLEquals(expectedMessageText, messages[0].EM_MessageText);
	}

	public void TestEDIMessage_MessageNum()
	{
		var message1 = new EbdMessageManager(sendingObject).GenerateMessages()[0];
		var message2 = new EbdMessageManager(sendingObject).GenerateMessages()[0];
		Factory.Save();

		AssertNotEquals(message1.EM_MessageNum, message2.EM_MessageNum);
	}

	public void TestEvent()
	{
		var manager = new EbdMessageManager(sendingObject);
		manager.GenerateMessages();
		Factory.Save();

		CombineAssertions(() =>
		{
			var log = entryHeader.Logs.MostRecentLogByEventTime(Events.DocumentSent);
			AssertEquals("SL_Table", CusEntryHeader.Schema.TableName, log.SL_Table);
			AssertEquals("SL_Parent", entryHeader.PK, log.SL_Parent);
			AssertEquals("|FIL=Invoice.pdf|REF=doc-reference|TYP=doc-type", log.SL_Reference);
		});
	}

	public void TestRollbackOnSaveFailed()
	{
		var manager = new EbdMessageManager(sendingObject);
		CombineAssertions(() =>
		{
			var entryHeaderLogCountBefore = entryHeader.Logs.GetAllLogs().Count;

			entryHeader.Logs.AddNew(Events.DeclarationSentToCustoms);

			manager.RollbackOnSaveFailed();
			AssertEquals("# of created log events on entry header", 0, entryHeader.Logs.GetAllLogs().Count - entryHeaderLogCountBefore);
		});
	}

	public void TestSingleMessageManagerConfig()
	{
		var manager = new EbdMessageManager(sendingObject);
		CombineAssertions(() =>
		{
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);
			AssertEquals("manager", false, manager.CanSendWithdrawal);
			AssertEquals("IsWaitingForResponse", false, manager.IsWaitingForResponse);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		companyCrecdential = CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();

		declaration = Factory.New<JobDeclaration>();
		var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN100");

		sendingObject = new SupportingDocSendingObject(declaration);
		sendingObject.EDoc = eDoc.UniqueKey;
		sendingObject.DocumentType = "doc-type";
		sendingObject.LocalReferenceNumber = entryHeader.MovementReferenceNumber;
		sendingObject.CaseNumber = "doc-reference";
	}
	GlbCompanyCredential companyCrecdential;
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	SupportingDocSendingObject sendingObject;
}
