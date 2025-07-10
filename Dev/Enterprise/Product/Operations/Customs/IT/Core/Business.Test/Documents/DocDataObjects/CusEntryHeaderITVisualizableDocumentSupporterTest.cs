using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusEntryHeaderITVisualizableDocumentSupporterTest : TestCaseWithFactory
{
	public void TestGetDocProviderKey()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var jobDeclarationITVisualizableDocumentSupporter = new CusEntryHeaderITVisualizableDocumentSupporterForTest(entryHeader);

		AssertEquals("GetDocProviderKey()", "IT", jobDeclarationITVisualizableDocumentSupporter.GetDocProviderKeyExposed());
	}

	class CusEntryHeaderITVisualizableDocumentSupporterForTest : CusEntryHeaderITVisualizableDocumentSupporter
	{
		public CusEntryHeaderITVisualizableDocumentSupporterForTest(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public string GetDocProviderKeyExposed() => GetDocProviderKey();
	}
}
