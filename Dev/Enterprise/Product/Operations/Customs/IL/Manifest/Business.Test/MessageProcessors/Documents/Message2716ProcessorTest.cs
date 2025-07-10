using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class Message2716ProcessorTest : BaseILBranchCustomsApplicationTypeMessageProcessorTest<Message2716Processor, ILDOC276ResponseMessage>
	{
		public void TestProcessMessage_Discarded_WhenNoDocumentFoundById()
		{
			var factory = Factory;
			const string expectedNote = "Couldn't locate document with reference 112233";
			var message = CreateResponseMessage("OK", null, "112233", "123");
			factory.Save();

			var loggingInformation = new LoggingInformation();
			var result = Processor.GetLinkedBusinessObjectMetaData(message, loggingInformation);

			CombineAssertions("When Supporting Document Not Found", () =>
			{
				Assert("The should be discard message", !result.DiscardReason.IsEmpty);
				AssertEquals("The message have expected discard message", expectedNote, result.DiscardReason);
			});
		}

		public void TestProcessMessage_ProcessedAndSent_WhenDocumentFoundById()
		{
			var factory = Factory;
			var message = CreateResponseMessage("OK", header, supportingDocument.PK.ToString(), "123");
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Supporting Document Found By Id", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The CSI_Status is SNT", "SNT", supportingDocument.CSI_Status);
			});
		}

		public void TestProcessMessage_ProcessedAndFailed_WhenDocumentFoundById()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = ZString.Empty;
			var message = CreateResponseMessage("OK", header, supportingDocument.PK.ToString());
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Supporting Document Found By Id", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The CSI_Status is FAL", "FAL", supportingDocument.CSI_Status);
				AssertEquals("The CSI_ReferenceNumber2 remains empty", ZString.Empty, supportingDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestProcessMessage_MessageLinked_WhenNoLinkedObjectProvided()
		{
			var factory = Factory;
			var message = CreateResponseMessage("OK", null, supportingDocument.PK.ToString(), "123");
			message.EM_LinkedObject = header;
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Supporting Document Found By Id", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The CSI_Status is SNT", "SNT", supportingDocument.CSI_Status);
				AssertEquals("The message should be linked to the header", header.PK, message.EM_LinkedObject.PK);
			});
		}

		public void TestProcessMessage_CSI_ReferenceNumber2_WhenEmpty()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = ZString.Empty;
			var message = CreateResponseMessage("OK", header, supportingDocument.PK.ToString(), "123");
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Supporting Document Found By Id", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The CSI_Status is SNT", "SNT", supportingDocument.CSI_Status);
				AssertEquals("The CSI_ReferenceNumber2 is 123", "123", supportingDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestProcessMessage_CSI_ReferenceNumber2_WhenNotEmpty()
		{
			var factory = Factory;
			supportingDocument.CSI_ReferenceNumber2 = "456";
			var message = CreateResponseMessage("OK", header, supportingDocument.PK.ToString(), "123");
			factory.Save();

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			CombineAssertions("When Supporting Document Found By Id", () =>
			{
				AssertEquals("The message status is ProcessedOK", "PRS", message.EM_Status);
				AssertEquals("The CSI_Status is SNT", "SNT", supportingDocument.CSI_Status);
				AssertEquals("The CSI_ReferenceNumber2 is 456", "456", supportingDocument.CSI_ReferenceNumber2);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			supportingDocument = CreateSupportingDocument();
			factory.Save();
		}

		protected override string ExpectedMessageFriendlyName => "IL Supporting Document Response Message";

		protected override string ExpectedMessageTypesToInclude => "DOC";

		protected override string ExpectedMessageSubTypesToInclude => "276";

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.AddAttachmentResponse_276_OK.xml")
				.Replace("##ExternalAttachmentID##", supportingDocument.PK.ToString())
				.Replace("##ApplicationID##", "123");

		protected override BusinessObject ExpectedLinkedObject => header;

		protected override ZGuid ExpectedBranchPk => header.Branch.PK;

		protected override Message2716Processor CreateProcessor(LoggingInformation loggingInformation) => new Message2716Processor(loggingInformation);

		ILDOC276ResponseMessage CreateResponseMessage(string suffix, AsycudaManifestHeader linkedObject, string externalAttachmentId = null, string applicationId = "0")
		{
			var message = Factory.New<ILDOC276ResponseMessage>();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "DOC";
			message.EM_MessageSubType = "276";
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_LinkedObject = linkedObject;
			message.EM_MessageText = new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.AddAttachmentResponse_276_{suffix}.xml")
				.Replace("##ExternalAttachmentID##", externalAttachmentId)
				.Replace("##ApplicationID##", applicationId);

			return message;
		}

		SupportingDocument CreateSupportingDocument()
		{
			var factory = Factory;
			header = factory.New<AsycudaManifestHeader>();
			var declaration = header.Bills.AddNew();
			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_ReferenceNumber2 = "456";

			var supportingDocument2 = declaration.SupportingDocuments.AddNew();
			supportingDocument2.CSI_ReferenceNumber2 = "789";

			factory.Save();

			return supportingDocument1;
		}

		AsycudaManifestHeader header;
		SupportingDocument supportingDocument;
	}
}
