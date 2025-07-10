using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class Message828ProcessorTest : BaseILBranchCustomsApplicationTypeMessageProcessorTest<Message828Processor, ILDOC828ResponseMessage>
	{
		protected override string ExpectedMessageFriendlyName => "IL Supporting Document Request Decision Response Message";

		protected override string ExpectedMessageTypesToInclude => "DOC";

		protected override string ExpectedMessageSubTypesToInclude => "828";

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.DOC828ResponseMessage_Valid.xml");

		protected override BusinessObject ExpectedLinkedObject => header;

		protected override ZGuid ExpectedBranchPk => header.Branch.PK;

		protected override Message828Processor CreateProcessor(LoggingInformation loggingInformation) => new Message828Processor(loggingInformation);

		public void TestProcessMessage()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = "1055625732";
			var message = CreateResponseMessage("Valid", header);
			factory.Save();
			Processor.ProcessMessage(message);
			factory.Save();
			message.Reload();
			CombineAssertions("When Supporting Document Found By Id", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The CSI_Status is VAL", "VAL", supportingDocument.CSI_Status);
			});
		}

		public void TestProcessMessageWhenGeneralDetailsIsNull()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = "1055625732";
			var message = CreateResponseMessage("IsNull", header);
			factory.Save();
			Processor.ProcessMessage(message);
			factory.Save();
			message.Reload();

			const string expectedNote = "DocumentId is null in this message";
			CombineAssertions("When Supporting Document Not Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "DCD", message.EM_Status);
				AssertEquals("The CSI_Status is empty", "", supportingDocument.CSI_Status);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals("The message have note", 1, notes.Count);
				AssertEquals("The message have note with text", expectedNote, notes[0].ST_NoteText);
			});
		}

		public void TestProcessMessageWhenSupportingDocumentMoreThanOne()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = "1055625732";
			var supportingDocument2 = bill.SupportingDocuments.AddNew();
			supportingDocument2.CSI_ReferenceNumber2 = "1055625732";
			var message = CreateResponseMessage("Valid", header);
			factory.Save();
			Processor.ProcessMessage(message);
			factory.Save();
			message.Reload();

			const string expectedNote = "Find more than 1 supporting document with reference 1055625732";
			CombineAssertions("When Supporting Document Not Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "DCD", message.EM_Status);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals("The message have note", 1, notes.Count);
				AssertEquals("The message have note with text", expectedNote, notes[0].ST_NoteText);
			});
		}

		public void TestProcessMessageWhenSupportingDocumentNotExist()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = "1055625733";
			var message = CreateResponseMessage("Valid", header);
			factory.Save();
			Processor.ProcessMessage(message);
			factory.Save();
			message.Reload();

			const string expectedNote = "Couldn't locate document with reference 1055625732";
			CombineAssertions("When Supporting Document Not Found", () =>
			{
				AssertEquals("The message status is ProcessedOK", "DCD", message.EM_Status);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals("The message have note", 1, notes.Count);
				AssertEquals("The message have note with text", expectedNote, notes[0].ST_NoteText);
			});
		}

		ILDOC828ResponseMessage CreateResponseMessage(string suffix, AsycudaManifestHeader linkedObject)
		{
			var message = Factory.New<ILDOC828ResponseMessage>();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "DOC";
			message.EM_MessageSubType = "828";
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_LinkedObject = linkedObject;
			message.EM_MessageText = new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.DOC828ResponseMessage_{suffix}.xml");

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");

			var factory = Factory;
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			header.AMA_ManifestNumber = "MAN12345";
			bill = header.Bills.AddNew();
			supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber2 = "1055625732";
			factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		IDisposable disposableAction;
		AsycudaManifestHeader header;
		AsycudaBill bill;
		SupportingDocument supportingDocument;
	}
}
