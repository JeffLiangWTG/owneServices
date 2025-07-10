using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.IL.Business.Testing;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ILMAN820MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("header is must", () => new ILMAN820MessageBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>("AsycudaManifestQueryMessageSendingObject is must", () => new ILMAN820MessageBuilder(Factory.New<AsycudaManifestHeader>(), null));
		}

		protected override IMessageBuilder GetMessageBuilder()
		{
			var messageSendingObject = (AsycudaManifestQueryMessageSendingObject)header.MessageSendingConfiguration.GetNewQueryMessageSendingObjectParent(header).SelectedSendingObjects.Single();
			return new ILMAN820MessageBuilder(header, messageSendingObject);
		}

		protected override string GetExpectedMessageSubType()
		{
			return "820";
		}

		protected override string GetExpectedMessageText()
		{
			return new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IL.Manifest.Business.Testing.Message.MessageBuilder.TestFiles.ILManQueryMessageRequest_820.xml");
		}
		protected override string GetExpectedMessageType()
		{
			return "MAN";
		}

		protected override BusinessObject GetLinkedObject() => header;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => header.Messages;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			header.AMA_ApplicationCode = "NVC";
			header.AMA_ManifestType = "785";
			header.AMA_ManifestNumber = "123";
		}

		AsycudaManifestHeader header;
	}
}
