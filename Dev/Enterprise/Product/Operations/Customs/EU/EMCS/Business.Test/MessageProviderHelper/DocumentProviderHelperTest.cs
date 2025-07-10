using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class DocumentProviderHelperTest : TestCaseWithFactory
	{
		public void TestDocumentType()
		{
			document.CSI_SubType = "SUBTY";
			AssertEquals("SUBTY", helper.DocumentType);
		}

		public void TestDocumentReference()
		{
			document.CSI_ReferenceNumber = "REFERENCE";
			AssertEquals("REFERENCE", helper.DocumentReference);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			document = declaration.Documents.AddNew();
			helper = new DocumentProviderHelper(document);
		}
		EMCSDocument document;
		DocumentProviderHelper helper;
	}
}
