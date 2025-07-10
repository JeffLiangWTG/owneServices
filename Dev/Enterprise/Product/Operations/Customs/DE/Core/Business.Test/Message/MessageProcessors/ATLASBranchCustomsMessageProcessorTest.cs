using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ATLASBranchCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessUnsupportedMessageType()
		{
			var unknowMessage = CreateMessage("T!T12", "02342323", "T!T", "SVE");
			processor.ExecuteBatch();
			AssertMultilineASCIIEquals("Logg", @"	Failed to locate a processor for message (Application Code: DEA, Application Reference: T!T12, Message Type: T!T, Message Sub Type: SVE)",
			string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>()));
			var newFactory = new BusinessObjectFactory();
			unknowMessage = newFactory.Load<AtlasInboundEDIMessage<IUnderCustomsControl>>(unknowMessage.PK);
			AssertEquals("unknowMessage.EM_Status", EDIMessage.Status.Failed, unknowMessage.EM_Status);
		}

		public void TestResolveATLASMessageProcessors()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageSubType = ImportMessageSubTypeList.Codes.BondedWarehouseCompletionInformation;
			var messages = ATLASResponseMessageDetails.Instance.ResponseMessages.Select(d => d.Key).ToList();
			CombineAssertions(() =>
			{
				foreach (var technicalMessageName in messages)
				{
					message.EM_ApplicationReference = technicalMessageName;
					var prc = processor.GetApplicationTypeProcessorCore(message);
					AssertEquals(technicalMessageName, true, prc is ApplicationTypeMessageProcessor);
				}
			});
		}

		public void TestProcessSupportedMessageType()
		{
			var message = CreateMessage(nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NSTAXK), "02342323", EDIMessageTypeList.Codes.Import, "SVE");
			processor.ExecuteBatch();
			AssertMultilineASCIIEquals("Logg", @"	Pre-Process Message #02342323
	Saving...
	1 message pre-processed
	Processing Message #02342323
	Successfully Added eDoc: file1.pdf.
	Saving...
	1 message processed",
			string.Join("\r\n", processor.Logger.UserLogStrings.Cast<string>()));
			var newFactory = new BusinessObjectFactory();
			message = newFactory.Load<AtlasInboundEDIMessage<IUnderCustomsControl>>(message.PK);
			AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestMultipleMessagesWithDocumentsProcessedAsABatchAndSavedDoesNotHaveAnException()
		{
			var message1 = CreateMessage(nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NSTAXK), "02342323", EDIMessageTypeList.Codes.Import, "SVE");
			var message2 = CreateMessage(nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.NSTAXK), "02342324", EDIMessageTypeList.Codes.Import, "SVE");

			var numberOfDocsBefore1 = ((IDocManagerSupport)message1).DocManagerInfo.AllEDocs.Count;
			var numberOfDocsBefore2 = ((IDocManagerSupport)message2).DocManagerInfo.AllEDocs.Count;

			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var messageReloaded1 = newFactory.Load<AtlasInboundEDIMessage<IUnderCustomsControl>>(message1.PK);
			var messageReloaded2 = newFactory.Load<AtlasInboundEDIMessage<IUnderCustomsControl>>(message2.PK);

			var numberOfDocsAfter1 = ((IDocManagerSupport)messageReloaded1).DocManagerInfo.AllEDocs.Count;
			var numberOfDocsAfter2 = ((IDocManagerSupport)messageReloaded2).DocManagerInfo.AllEDocs.Count;

			CombineAssertions(() =>
			{
				AssertEquals("message1 has 1 document", 1, numberOfDocsAfter1 - numberOfDocsBefore1);
				AssertEquals("message2 has 1 document", 1, numberOfDocsAfter2 - numberOfDocsBefore2);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new ATLASBranchCustomsMessageProcessor
			{
				Logger = new LoggingInformation()
			};
		}
		ATLASBranchCustomsMessageProcessor processor;

		AtlasInboundEDIMessage<IUnderCustomsControl> CreateMessage(ZString applicationReference, ZString messageNum, ZString messageType, ZString messageSubType)
		{
			var message = Factory.New<AtlasInboundEDIMessage<IUnderCustomsControl>>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_ApplicationReference = applicationReference;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_MessageNum = messageNum;
			message.EM_MessageSubType = messageSubType;
			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DECustomsData>
	<LogbookTime>2019-02-25T16:29:00.2347048+02:00</LogbookTime>
	<CustomsData>
		<{applicationReference}>
			<MetaData>
				<Preparation>
					<Date>2019-02-25</Date>
					<Time>16:00:00</Time>
				</Preparation>
				<InterchangeControlReference>0000000375302</InterchangeControlReference>
				<MessageReferenceNumber>1</MessageReferenceNumber>
				<MessageIdentifier>CUSTST58750000000375302250219160050</MessageIdentifier>
				<MessageGroup>SVM</MessageGroup>
				<MessageType>{applicationReference}</MessageType>
				<InterchangeSender>
					<Identification>
						<ReferenceNumber>DE005875</ReferenceNumber>
					</Identification>
				</InterchangeSender>
			</MetaData>
			<Header>
				<ReferenceNumber>ATB150002930220195875</ReferenceNumber>
			</Header>
			<Body />
		</{applicationReference}>
	</CustomsData>
	<AttachedDocumentCollection>
		<AttachedDocument>
			<FileName>file1.pdf</FileName>
			<Type>
				<Code>CAU</Code>
				<Description>Report EXQQUE</Description>
			</Type>
			<ImageData>MkE5UlZFQUMtQVpQMlU0RjItSkhKRVNDUkMtWllSSFpOUEYtTFREODQ2TFYtMzlRRDZGTk0tM1Y3VzZNRDgtUTdVMjk5VTI=</ImageData>
		</AttachedDocument>
	</AttachedDocumentCollection>
</DECustomsData>";
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			return message;
		}
	}
}
