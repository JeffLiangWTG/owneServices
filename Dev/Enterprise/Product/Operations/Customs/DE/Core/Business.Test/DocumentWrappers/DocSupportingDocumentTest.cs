using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	sealed class DocSupportingDocumentTest : DocBaseWrapperTest
	{
		public void TestNew()
		{
			AssertNull(DocSupportingDocument.New(null, Factory));
		}

		public void TestRegistrationNumber()
		{
			supportingDocument.CSI_Code = "ABC";
			AssertEquals("ABC", Wrapper.Code);
		}

		public void TestDate()
		{
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2022, 9, 15);
			AssertEquals("15.09.2022", Wrapper.Date);
		}

		public void TestQuantity()
		{
			supportingDocument.CSI_Quantity = 1.2345m;
			AssertEquals("1,235", Wrapper.Quantity);
		}

		public void TestUnitOfQuantity()
		{
			supportingDocument.CSI_UnitOfQuantity = "KG";
			AssertEquals("KG", Wrapper.UnitOfQuantity);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => DocSupportingDocument.New(supportingDocument, Factory);

		new DocSupportingDocument Wrapper => (DocSupportingDocument)base.Wrapper;

		protected override void SetUp()
		{
			base.SetUp();
			supportingDocument = Factory.New<SupportingDocument>();
		}
		SupportingDocument supportingDocument;
	}
}
