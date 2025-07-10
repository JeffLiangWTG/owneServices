using System.Text;
using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AttachmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<IAttachment>
	{
		public void TestNewOrNull()
		{
			AssertNull("When SupportingDocument is null", AttachmentWrapper.NewOrNull(null));
			AssertNotNull("When SupportingDocument is not null", Provider);
		}

		public void TestContent()
		{
			AssertNotNull(Provider.Content);
			AssertEquals("Content is Hello world-AsycudaManifestHeader", "Hello world-AsycudaManifestHeader", Encoding.UTF8.GetString(Provider.Content));
		}

		public void TestFileName()
		{
			AssertNotNull(Provider.FileName);
			AssertEquals("FileName is sample.txt", "sample.txt", Provider.FileName);
		}

		public void TestDocumentType()
		{
			AssertNotNull(Provider.DocumentType);
			AssertEquals("DocumentType is 380", "380", Provider.DocumentType);
		}

		public void TestKeywords()
		{
			AssertNull("Keywords is null", Provider.Keywords);
		}

		public void TestAttachmentId()
		{
			AssertNull("AttachmentId is null", Provider.AttachmentId);
		}

		public void TestExternalAttachmentId()
		{
			AssertNotNull(Provider.ExternalAttachmentId);
			AssertEquals("ExternalAttachmentId is SupportingDocument.PK.ToString()", supportingDocument.PK.ToString(), Provider.ExternalAttachmentId);
		}

		public void TestRemark()
		{
			AssertNull("Remark is null", Provider.Remark);
		}

		public void TestIsAttachment()
		{
			AssertEquals("IsAttachment", "True", Provider.IsAttachment);
		}

		public void TestAdditionalData()
		{
			AssertNotNull(Provider.AdditionalData);
			AssertEquals("AdditionalData count is 1", 1, Provider.AdditionalData.Count);
		}

		protected override IAttachment GetProvider()
			=> AttachmentWrapper.NewOrNull(supportingDocument);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestNumber = "MAN123";
			var bill = header.Bills.AddNew();

			supportingDocument = bill.SupportingDocuments.AddNew();

			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world-AsycudaManifestHeader"), "sample.txt", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;
			supportingDocument.CSI_Code = "380";

			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.AddNew();
			supportingDocumentMetaData.CY_Code = "14";
			supportingDocumentMetaData.CY_Data = "DataType";
		}

		SupportingDocument supportingDocument;
	}
}
