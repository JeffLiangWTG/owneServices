using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsCusStorageDocPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAvailableEDocs()
		{
			var header = Factory.New<NctsHeader>();
			var eDoc1 = header.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			var pivot = header.EDocPivotCollection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Available EDocs count is 1", 1, pivot.Lookups.AvailableEDocs.Count);
				AssertEquals("Edoc", eDoc1.UniqueKey, pivot.Lookups.AvailableEDocs[0].PK);
			});
		}
	}
}
