using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	class DocumentCommonWrapperTest : WrapperHelperTest<DocumentCommonWrapper>
	{
		public void TestName()
		{
			AssertEquals("Expected filled Name", SupportingDocumentData.Code, wrapper.Name);
		}

		public void TestNumber()
		{
			AssertEquals("Expected filled Number", SupportingDocumentData.Reference, wrapper.Number);
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new DocumentCommonWrapper(SupportingDocumentData.Code, SupportingDocumentData.Reference);
		}
		DocumentCommonWrapper wrapper;

		protected override DocumentCommonWrapper GetProvider() => wrapper;
	}
}
