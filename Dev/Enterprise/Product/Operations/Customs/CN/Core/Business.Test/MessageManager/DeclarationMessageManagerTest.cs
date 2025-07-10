using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	public class DeclarationMessageManagerTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName() => AssertEquals("MessageFriendlyName", Sender.MessageTypeDescription, Manager.MessageFriendlyName);

		public void TestBusinessObject() => AssertEquals(Sender, Manager.BusinessObject);

		public void TestCanSendOriginal() => AssertEquals(true, Manager.CanSendOriginal);

		public void TestCanSendWithdrawal() => AssertEquals(false, Manager.CanSendWithdrawal);

		public void TestHasActiveMessages()
		{
			CombineAssertions(() =>
			{
				var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
				entryHeader.EntryNumber = "NO1";
				var importDeclarationMessageManager = new DeclarationMessageManager(new CNJobDeclarationMessageSendingObject(entryHeader));
				AssertEquals("HasActiveMessages is true", true, importDeclarationMessageManager.HasActiveMessages);
				importDeclarationMessageManager = Manager;
				AssertEquals("HasActiveMessages is false", false, importDeclarationMessageManager.HasActiveMessages);
			});
		}

		public void TestGenerateMessages()
		{
			var header = Sender.Header;
			header.CH_BGMReference = "CUS202109030000001";
			var message = Manager.GenerateMessages()[0];

			AssertNotNull(message);
			AssertEquals("EM_LinkTable should be CusEntryHeader", header.TableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should be header.PK", Sender.Header.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageType should be DEC", EDIMessageTypeList.Codes.DEC, message.EM_MessageType);
			AssertEquals("EM_MessageSubType should be header.CH_MessageType", header.CH_MessageType, message.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit should be TRX", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status should be QUE", Messaging.Integration.EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("EM_ApplicationReference should be header.CH_BGMReference", header.CH_BGMReference, message.EM_ApplicationReference);
			AssertEquals("EM_MessageText should be Sender.ToMessageString()", Sender.ToMessageString(), message.EM_MessageText);
			AssertEquals("EM_MessageInterpretation should be header.HtmlFormatEntryData", header.HtmlFormatEntryData, message.EM_MessageInterpretation);
			AssertEquals("Header.CH_Status is changed", JobMessageStatusList.Codes.AwaitingResponseIntegratedDeclaration, header.CH_Status);
			AssertEquals("Should add an MSN event", "Send to Customs;CUS202109030000001", header.Logs.MostRecentLogByEventTime(Events.MessageSent).SL_Reference);

			Manager.RollbackOnSavingFailed();
			AssertNull("Should delete the MSN event on saving failed", header.Logs.MostRecentLogByEventTime(Events.MessageSent));
		}

		public void TestGenerateEDIMessageAttach()
		{
			var message = Manager.GenerateMessages()[0];
			AssertEquals("No attachment generated when Entry Instruction has no Attachments", 0, message.MessageAttachments.Count);

			var eDocs = AddAttachment(Sender);

			message = Manager.GenerateMessages()[0];
			var attachments = message.MessageAttachments.Cast<EDIMessageAttach>();
			AssertEquals("Attachment generated for distinct Attachments on Entry Instruction", 2, attachments.Count());
			Assert("EDIMessageAttach generated for eDocs 1", attachments.Any(x => x.EG_StorageDocsGuid == eDocs[0].UniqueKey && x.EG_FileName == eDocs[0].FileName));
			Assert("EDIMessageAttach generated for eDocs 2", attachments.Any(x => x.EG_StorageDocsGuid == eDocs[1].UniqueKey && x.EG_FileName == eDocs[1].FileName));
		}

		public void TestGenerateEDIMessageAttach_PreliminaryDeclaration()
		{
			using (CNCustomsDataRegistry.Instance.TwoStepDeclarationActive.SetTemporaryValue(new Guid(), new Guid(), new Guid(), true))
			{
				var sendingObject = GetSendingObject();
				var manager = new DeclarationMessageManager(sendingObject);
				AddAttachment(sendingObject);
				var declaration = sendingObject.Header.Declaration;
				declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStep;
				sendingObject.Header.CH_Status = "";
				AssertEquals("Precondition: DeclarationType", DeclarationTypeList.Codes.PreliminaryDeclaration, sendingObject.DeclarationType);
				var message = manager.GenerateMessages()[0];
				AssertEquals("When DeclarationType is 1, No attachment generated", 0, message.MessageAttachments.Count);
			}
		}

		DeclarationMessageManager Manager => manager ?? (manager = new DeclarationMessageManager(Sender));
		DeclarationMessageManager manager;

		CNJobDeclarationMessageSendingObject Sender => sender ?? (sender = GetSendingObject());
		CNJobDeclarationMessageSendingObject sender;

		CNJobDeclarationMessageSendingObject GetSendingObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_MessageType = "CUS";
			entryHeader.CH_BGMReference = "REF";

			return new CNJobDeclarationMessageSendingObject(entryHeader);
		}

		IeDoc[] AddAttachment(CNJobDeclarationMessageSendingObject sendingObject)
		{
			var declaration = sendingObject.Header.Declaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDoc1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDoc2 = storageMain.AddFileOrDocument(new byte[1] { 1 }, "Test1.pdf", "TST");
			var instruction = sendingObject.Header.EntryInstruction;
			var attachment1 = instruction.Attachments.AddNew();
			attachment1.EDoc = eDoc1.UniqueKey;
			attachment1.AttachmentType = "00000001";
			var attachment2 = instruction.Attachments.AddNew();
			attachment2.EDoc = eDoc2.UniqueKey;
			attachment2.AttachmentType = "00000001";
			var attachment3 = instruction.Attachments.AddNew();
			attachment3.EDoc = eDoc2.UniqueKey;
			attachment3.AttachmentType = "00000001";
			var attachment4 = instruction.Attachments.AddNew();
			attachment4.AttachmentType = "10000001";

			return new[] { eDoc1, eDoc2 };
		}
	}
}
