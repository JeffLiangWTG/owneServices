using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class DocumentMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { (JobMessageTypeList.Codes.Import, Events.CustomsCleared), (JobMessageTypeList.Codes.Export, Events.ExportCustomsCleared) };

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.Document;

	protected override ZString BGMReference => "0100011210809998";

	protected override ZString ExpectedMessageFriendlyName => "Document Message Processor";

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new DocumentMessageProcessor(logger);

	protected override ZString ReceivedMessage => $@"<AttachedDocument>
	<FileName>{FileName}</FileName>
	<ImageData>{ImageData}</ImageData>
	<Type>
		<Code>{DocTypeCode}</Code>
		<Description>Customs Authority</Description>
	</Type>
</AttachedDocument>";

	ZString ImageData => "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bQplbmRvYmoKOCAwIG9iajw8L0tpZHNbOSAwIFJdL1R5cGUvUGFnZXMvQ291bnQgMT4+CmVuZG9iagoxMCAwIG9iajw8L1R5cGUvQ2F0YWxvZy9QYWdlcyA4IDAgUj4+CmVuZG9iagoxMSAwIG9iajw8L01vZERhdGUoRDoyMDIxMTIyMTEwNTczNSswMScwMCcpL0NyZWF0aW9uRGF0ZShEOjIwMjExMjIxMTA1NzM1KzAxJzAwJykvUHJvZHVjZXIoaVRleHQxLjMuMSBieSBsb3dhZ2llLmNvbSBcKGJhc2VkIG9uIGl0ZXh0LXBhdWxvLTE1NFwpKT4+CmVuZG9iagp4cmVmCjAgMTIKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAyNjM1IDAwMDAwIG4gCjAwMDAwMDIzNTggMDAwMDAgbiAKMDAwMDAwMTI1OCAwMDAwMCBuIAowMDAwMDAxNDM3IDAwMDAwIG4gCjAwMDAwMDEzNDUgMDAwMDAgbiAKMDAwMDAwMTgzMCAwMDAwMCBuIAowMDAwMDAwMDE1IDAwMDAwIG4gCjAwMDAwMDI4ODEgMDAwMDAgbiAKMDAwMDAwMTA1MSAwMDAwMCBuIAowMDAwMDAyOTMxIDAwMDAwIG4gCjAwMDAwMDI5NzYgMDAwMDAgbiAKdHJhaWxlcgo8PC9JbmZvIDExIDAgUi9JRCBbPDEzYWUxYTEzNTgzYjQ3OGY3Y2JhNjgyM2EwMTQyZGM1PjwxM2FlMWExMzU4M2I0NzhmN2NiYTY4MjNhMDE0MmRjNT5dL1Jvb3QgMTAgMCBSL1NpemUgMTI+PgpzdGFydHhyZWYKMzEzMgolJUVPRgo=";

	ZString FileName => $"e-dec_Import_BS_{BGMReference}_21CHEI000042027074_1_CHE293274655_72.pdf";

	ZString DocTypeCode => "CAU";

	ZString DocSource => "CUS";

	public void TestProcessMessage()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();

				var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, ReceivedMessage, BGMReference);

				Processor.ProcessMessage(ediMessage);

				var allEdocs = ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs;
				AssertEquals("eDocs count", 1, allEdocs.Count);
				var uniqueEdoc = allEdocs[0];
				AssertEquals("FileName", FileName, uniqueEdoc.FileName);
				AssertEquals("DocType", DocTypeCode, uniqueEdoc.DocType);
				AssertEquals("CUS", DocSource, uniqueEdoc.DocSource);
				AssertEquals("ImageData", new ZBlob(Convert.FromBase64String(ImageData)), uniqueEdoc.ImageData);
			}
		});
	}

	public void TestProcessMessageWithExistingDocument()
	{
		CombineAssertions(() =>
		{
			foreach (var messageType in MessageTypes)
			{
				var factory = new BusinessObjectFactory();

				var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessage(factory, messageType.MessageType, MessageSubType, ReceivedMessage, BGMReference);

				AssertEquals("eDocs initial count", 0, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);

				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after first process", 1, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				AssertEquals("eDocs file name", FileName, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().Single().FileName);

				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after second process", 1, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				AssertEquals("eDocs file name", FileName, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().Single().FileName);

				((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Remove(((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().Single());
				AssertEquals("eDocs count after delete", 0, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);

				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after delete and re-process", 1, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				AssertEquals("eDocs file name", FileName, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().Single().FileName);

				((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().Single().IsDeleted = true;
				AssertEquals("eDocs count after delete and re-process", 1, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after delete and re-process", 2, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				var fileName2 = $"e-dec_Import_BS_{BGMReference}_21CHEI000042027074_1_CHE293274655_72[2].pdf";
				AssertEquals("eDocs file 2 name", fileName2, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => d.FileName == fileName2).FileName);

				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after second process", 2, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);

				((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => d.FileName == fileName2).IsDeleted = true;
				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after delete and re-process", 3, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				var fileName3 = $"e-dec_Import_BS_{BGMReference}_21CHEI000042027074_1_CHE293274655_72[3].pdf";
				AssertEquals("eDocs file 3 name", fileName3, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => d.FileName == fileName3).FileName);

				((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => d.FileName == fileName2).IsDeleted = false;
				((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => d.FileName == fileName3).IsDeleted = true;
				Processor.ProcessMessage(ediMessage);
				AssertEquals("eDocs count after delete and re-process", 3, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Count);
				var fileName4 = $"e-dec_Import_BS_{BGMReference}_21CHEI000042027074_1_CHE293274655_72[4].pdf";
				AssertEquals("eDocs file 4 not exists", null, ((IDocManagerSupport)entryHeader).DocManagerInfo.AllEDocs.Cast<IeDoc>().SingleOrDefault(d => d.FileName == fileName4));
			}
		});
	}

	public override void TestDeserializeXML_Failure()
	{
		foreach (var messageType in MessageTypes)
		{
			var ediMessage = MessageProcessorTestHelper.CreateEDIMessage(Factory, messageType: messageType.MessageType, messageSubType: MessageSubType, messageText: "This will cause a deserialize failure!");
			Factory.Save();

			AssertExceptionThrown<InvalidOperationException>(() => Processor.ProcessMessage(ediMessage));
			Factory.Save();
		}
	}
}
