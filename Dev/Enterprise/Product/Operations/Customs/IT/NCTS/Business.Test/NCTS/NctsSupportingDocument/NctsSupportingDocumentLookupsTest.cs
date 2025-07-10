using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStatusList()
	{
		var supportingDocument = Factory.New<NctsSupportingDocument>();
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "DER, PAP", supportingDocument.Lookups.StatusList.CodesAsString);
			AssertSame("Cached", supportingDocument.Lookups.StatusList, supportingDocument.Lookups.StatusList);
		});
	}
}
