using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	sealed class DocSADHPageTest_DoNotInheritTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		public void TestBISCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			DocSADHPage page = DocSADHPage.New(Factory, entryLine);

			AssertEquals("BIS caption empty for EU", ZString.Empty, page.BISCaption);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocSADHPage.New(Factory, null);
		}
	}
}
