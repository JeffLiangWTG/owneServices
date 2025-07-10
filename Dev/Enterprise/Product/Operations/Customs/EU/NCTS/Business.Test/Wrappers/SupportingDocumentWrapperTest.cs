using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class SupportingDocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentWrapper>
	{
		public void TestDocumentType()
		{
			supportDocument.CSI_Code = "T1";
			AssertEquals("T1", wrapper.DocumentType);
		}

		public void TestDocumentReference()
		{
			supportDocument.CSI_ReferenceNumber = "PREV01";
			AssertEquals("PREV01", wrapper.DocumentReference);
		}

		public void TestDocumentReference_Length()
		{
			supportDocument.CSI_ReferenceNumber = new string('A', 40);
			AssertEquals(35, wrapper.DocumentReference.Length);
		}

		public void TestDocumentReferenceLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.DocumentReferenceLanguage);
		}

		public void TestComplementOfInformation()
		{
			supportDocument.CSI_Description = "SUPPORTING INFO";
			AssertEquals("SUPPORTING INFO", wrapper.ComplementOfInformation);
		}

		public void TestComplementOfInformation_Length()
		{
			supportDocument.CSI_Description = new string('B', 30);
			AssertEquals(26, wrapper.ComplementOfInformation.Length);
		}

		public void TestComplementOfInformationLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.ComplementOfInformationLanguage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			supportDocument = goodsItem.SupportingDocuments.AddNew();
			wrapper = new SupportingDocumentWrapper(supportDocument);
		}
		SupportingDocumentWrapper wrapper;
		NctsSupportingDocument supportDocument;

		protected override SupportingDocumentWrapper GetProvider() => wrapper;
	}
}
