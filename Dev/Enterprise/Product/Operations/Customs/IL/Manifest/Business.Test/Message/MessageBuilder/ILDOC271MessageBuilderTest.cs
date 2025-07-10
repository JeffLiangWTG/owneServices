using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.IL.Business.Testing;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class ILDOC271MessageBuilderTest : ILMessageBuilderBaseTest
	{
		protected override IMessageBuilder GetMessageBuilder() => new ILDOC271MessageBuilder(asycudaManifestHeader, supportingDocument);

		protected override string GetExpectedMessageText() => new EmbeddedResourceRetriever().GetString("Enterprise.Customs.IL.Manifest.Business.Testing.Message.MessageBuilder.TestFiles.SendDocumentToCustomsMessage_2715.xml").Replace("testID", supportingDocument.PK.ToString());

		protected override BusinessObject GetLinkedObject() => asycudaManifestHeader;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => asycudaManifestHeader.Messages;

		protected override void SetUp()
		{
			base.SetUp();

			asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = asycudaManifestHeader.Bills.AddNew();
			supportingDocument = bill.SupportingDocuments.AddNew();
		}

		protected override string GetExpectedMessageSubType() => "271";

		protected override string GetExpectedMessageType() => "DOC";

		AsycudaManifestHeader asycudaManifestHeader;
		AsycudaBill bill;
		SupportingDocument supportingDocument;
	}
}
