using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Testing.MessageProcessors;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class Message8241ProcessorTest : BaseILBranchCustomsApplicationTypeMessageProcessorTest<Message8241Processor, ILMAN821ResponseMessage>
	{
		public void TestProcessMessage_Discarded_WhenNoManifestHeaderFound()
		{
			SetupHeader(Factory, "I123456789123457");
			var message = GetResponseMessage();

			AssertMessageDiscarded("When No Manifest Header Found - consider Manifest Number", message);
		}

		public void TestProcessMessage_ProcessedOK_WhenManifestHeaderFound()
		{
			var factory = Factory;
			var message = GetResponseMessage();

			SetupHeader(factory, "MAN12345");

			Processor.ProcessMessage(message);
			factory.Save();

			message.Reload();
			AssertEquals("When Manifest Header Found the message status is ProcessedOK", "PRS", message.EM_Status);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			header.AMA_ManifestNumber = "MAN12345";
			bill = header.Bills.AddNew();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");

			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		protected override Message8241Processor CreateProcessor(LoggingInformation loggingInformation) => new Message8241Processor(new LoggingInformation());

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.ManifestQueryResponse_8241.xml");

		protected override string ExpectedMessageFriendlyName => "IL Manifest Query Response Message";

		protected override string ExpectedMessageTypesToInclude => "MAN";

		protected override string ExpectedMessageSubTypesToInclude => "821";

		protected override BusinessObject ExpectedLinkedObject => header;

		protected override ZGuid ExpectedBranchPk => header.Branch.PK;

		void SetupHeader(BusinessObjectFactory factory, ZString manifestNumber)
		{
			header.AMA_ManifestNumber = manifestNumber;
			bill.ABL_SequenceNumber = 6556;

			factory.Save();
		}

		void AssertMessageDiscarded(string scenarioName, ILMAN821ResponseMessage message)
		{
			var metaData = Processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());

			CombineAssertions(scenarioName, () =>
			{
				Assert("Discard reason was provided", !metaData.DiscardReason.IsEmpty);
				AssertContains("Discard reason is as expected", "Couldn't locate Job using provided Manifest #", metaData.DiscardReason);
			});
		}

		ILMAN821ResponseMessage GetResponseMessage()
		{
			var message = Factory.New<ILMAN821ResponseMessage>();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "MAN";
			message.EM_MessageSubType = "821";
			message.EM_Status = "QUE";
			message.EM_ReceiveTransmit = "RCV";

			message.EM_MessageText = new EmbeddedResourceRetriever().GetString($"Enterprise.Customs.IL.Manifest.Business.Testing.MessageProcessors.TestFiles.ManifestQueryResponse_8241.xml");
			return message;
		}

		IDisposable disposableAction;
		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
