using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.Testing;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ILMAN170MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("header is must", () => new ILMAN170MessageBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>("AsycudaManifestMessageSendingObject is must", () => new ILMAN170MessageBuilder(Factory.New<AsycudaManifestHeader>(), null));
		}

		public void TestGetOcean1170Message_AfterSuccess()
		{
			var messageSendingObject = new AsycudaManifestMessageSendingObject(header);
			var messageBuilder = new ILMAN170MessageBuilder(header, messageSendingObject);
			var messageManager = new ManifestMessageManager(header, messageBuilder);
			AssertEquals("AMA_MessageStatus prerequisite", "", header.AMA_MessageStatus);
			messageManager.SendMessage(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("AMA_MessageStatus", "AWA", header.AMA_MessageStatus);
			AssertSame("Header should be linked to the sent message", header, header.Messages.OfType<ILEDIMessage>().First().EM_LinkedObject);
		}

		public void TestGetOcean1170Message_OnMessageStatusAwaiting()
		{
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			var messageSendingObject = new AsycudaManifestMessageSendingObject(header);
			var messageBuilder = new ILMAN170MessageBuilder(header, messageSendingObject);
			var messageBuilderResult = ((IMessageBuilder)messageBuilder).PopulateMessages();
			AssertEquals("Populating should fail", false, messageBuilderResult.IsSuccess);
			var buildResults = messageBuilderResult.GetBuilderResults();
			AssertEquals("One build result expected", 1, buildResults.Count());
			var buildResult = buildResults.First();
			AssertContainsExactElementsInAnyOrder("There should be 1 error", new string[] { "There are messages waiting for a response. You are unable to send a message until a valid response is received. If a response has been received, exit the job, then re-open to refresh the status" }, buildResult.Errors);
			AssertNotNull("Build result still should have message", buildResult.Message);
		}

		protected override IMessageBuilder GetMessageBuilder()
		{
			var messageSendingObject = (AsycudaManifestMessageSendingObject)header.MessageSendingConfiguration.GetNewMessageSendingObjectParent(header).SelectedSendingObjects.Single();
			return new ILMAN170MessageBuilder(header, messageSendingObject);
		}

		protected override string GetExpectedMessageSubType() => "170";

		protected override string GetExpectedMessageText()
		{
			return new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IL.Manifest.Business.Testing.Message.MessageBuilder.TestFiles.Ocean1170Message.xml");
		}
		protected override string GetExpectedMessageType() => "MAN";

		protected override BusinessObject GetLinkedObject() => header;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => header.Messages;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";
			header.AMA_ApplicationCode = "NVC";
			header.AMA_ManifestType = "785";
		}

		AsycudaManifestHeader header;
	}
}
