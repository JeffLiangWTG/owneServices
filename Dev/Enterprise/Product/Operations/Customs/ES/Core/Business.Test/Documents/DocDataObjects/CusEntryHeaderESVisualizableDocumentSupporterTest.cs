using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Environment;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing
{
	sealed class CusEntryHeaderESVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var supporter = new CusEntryHeaderESVisualizableDocumentSupporter(entryHeader);

			AssertEquals("CustomizeFormCheckpoint", Env.Security.MaintainJobDeclarationCustomiseForms, supporter.CustomizeFormCheckpoint);
		}
	}
}
