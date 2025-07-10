using CargoWise.Customs.IL.MessageDefinitions.DOC.REQ_271;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class AttachmentAdditionalDataWrapperTest : Customs.Business.Testing.DataProviderTestCase<IAttachmentAdditionalData>
	{
		public void TestNewOrNull()
		{
			AssertNull("When metaData is null", AttachmentAdditionalDataWrapper.NewOrNull(null));
			AssertNotNull("When metaData is not null", Provider);
		}

		public void TestFieldId()
		{
			AssertNotNull(Provider.FieldId);
			AssertEquals("FieldId is 14", 14, Provider.FieldId);
		}

		public void TestFieldData()
		{
			AssertNotNull(Provider.FieldData);
			AssertEquals("FieldData is REF14", "REF14", Provider.FieldData);
		}

		protected override IAttachmentAdditionalData GetProvider()
			=> AttachmentAdditionalDataWrapper.NewOrNull(supportingDocumentMetaData);

		protected override void SetUp()
		{
			base.SetUp();
			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.AddNew();
			supportingDocumentMetaData.CY_Code = "14";
			supportingDocumentMetaData.CY_Data = "REF14";
		}

		SupportingDocumentMetaData supportingDocumentMetaData;
	}
}
