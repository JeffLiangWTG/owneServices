using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class SuspensionDrawbackImportEntryDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExportImportCategoryEntryList()
		{
			var parent = Factory.New<SuspensionDrawbackImportEntryDocument>();
			var listModel = parent.Lookups.ImportDocumentCategoryEntryList;
			AssertEquals(3, listModel.Count);
		}
	}
}
