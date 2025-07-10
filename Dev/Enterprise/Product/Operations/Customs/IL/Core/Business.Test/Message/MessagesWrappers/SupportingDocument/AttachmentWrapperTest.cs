using System.Text;
using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;

namespace Enterprise.Customs.IL.Business.Testing
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
			AssertEquals("Content is Hello world", "Hello world", Encoding.UTF8.GetString(Provider.Content));
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
			AssertEquals("When no value in CY_Data, AdditionalData count is 0", 0, Provider.AdditionalData.Count);
			supportingDocument.SupportingDocumentMetadataItems.FirstOrDefault().CY_Data = "DataType";
			AssertEquals("When CY_Data has value, AdditionalData count is 1", 1, Provider.AdditionalData.Count);
		}

		protected override IAttachment GetProvider()
			=> AttachmentWrapper.NewOrNull(supportingDocument);

		protected override void SetUp()
		{
			base.SetUp();

			var factory = Factory;
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			supportingDocument = jobDeclaration.SupportingDocuments.AddNew();
			var docManagerInfo = jobDeclaration.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "sample.txt", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;
			supportingDocument.CSI_Code = "380";

			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.AddNew();
			supportingDocumentMetaData.CY_Code = "14";
		}

		SupportingDocument supportingDocument;
	}
}
