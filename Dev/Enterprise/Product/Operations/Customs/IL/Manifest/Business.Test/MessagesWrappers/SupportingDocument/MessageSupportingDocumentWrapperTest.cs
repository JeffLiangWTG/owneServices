using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class MessageSupportingDocumentWrapperTest : DataProviderTestCase<IMessageSupportingDocument>
	{
		public void TestNewOrNull()
		{
			AssertNull("When SupportingDocument is null", MessageSupportingDocumentWrapper.NewOrNull(null));
			AssertNotNull("When SupportingDocument is not null", Provider);
		}

		public void TestDocumentId()
		{
			AssertEquals("DocumentId is CSI_ReferenceNumber2", 2232323, Provider.DocumentId);
		}

		public void TestAttachment()
		{
			AssertNotNull("Attachment is not null", Provider.Attachment);
			AssertType<AttachmentWrapper>("Attachment is AttachmentWrapper", Provider.Attachment);
		}

		public void TestRelatedEntity()
		{
			AssertNotNull("RelatedEntity is not null", Provider.RelatedEntity);
			AssertType<ConnectedEntityWrapper>("RelatedEntity is ConnectedEntityWrapper", Provider.RelatedEntity);
		}

		public void TestRequestContentHeader()
		{
			AssertNotNull("RequestContentHeader is always not null", Provider.RequestContentHeader);
			AssertType<RequestContentHeaderWrapper>("RelatedEntity is RequestContentHeaderWrapper", Provider.RequestContentHeader);
		}

		protected override IMessageSupportingDocument GetProvider()
			=> MessageSupportingDocumentWrapper.NewOrNull(supportingDocument);

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestNumber = "MAN123";
			var bill = header.Bills.AddNew();
			var asycudaTransportDocumentInfo = bill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo.CSI_Code = "IL1";
			asycudaTransportDocumentInfo.CSI_ReferenceNumber = "REFIL1";
			supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber2 = "2232323";
		}

		SupportingDocument supportingDocument;
	}
}
