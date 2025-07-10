using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class H7MessageBuilderManagerTest : TestCaseWithFactory
	{
		public void TestNewMessageBuilder_WhenMessageTypeIsH7Query_ShouldReturnQueryH7MessageBuilder()
		{
			AssertMessageBuilderType<QueryH7MessageBuilder>(DeclarationMessageTypeList.Codes.H7Query);
		}

		public void TestNewMessageBuilder_WhenMessageTypeIsH7Cancellation_ShouldReturnCancelH7MessageBuilder()
		{
			var builder = AssertMessageBuilderType<CancelH7MessageBuilder>(DeclarationMessageTypeList.Codes.H7Cancellation);
			AssertEquals("H7C", builder.MessageType);
		}

		public void TestNewMessageBuilder_WhenMessageTypeIsH7ReExport_ShouldReturnReExportH7MessageBuilder()
		{
			AssertMessageBuilderType<ReexportH7MessageBuilder>(DeclarationMessageTypeList.Codes.H7ReExport);
		}

		T AssertMessageBuilderType<T>(ZString messageType)
			where T : IMessageBuilderBase
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var bill = header.Bills.AddNew();
			var certificate = new CertificateObject(header.CustomsAgent, header.AMA_CustomsProfile, ZString.Empty);

			var sendingObject = new H7MessageSendingObject(bill);
			sendingObject.Action = messageType;

			var messageBuilderManager = new H7MessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();
			AssertType<T>("NewMessageBuilder is " + messageType + " type", messageBuilder);
			return (T)messageBuilder;
		}
	}
}
