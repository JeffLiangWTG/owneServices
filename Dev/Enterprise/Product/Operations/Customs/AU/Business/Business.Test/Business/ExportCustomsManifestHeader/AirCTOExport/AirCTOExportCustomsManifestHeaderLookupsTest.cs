using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCTOExportCustomsManifestHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestManifestTypeList()
		{
			var header = Factory.New<AirCTOExportCustomsManifestHeader>();
			AssertNotNull(header.Lookups.ManifestTypeList);
			AssertEquals(typeof(AirManifestTypeList), header.Lookups.ManifestTypeList.GetType());
		}
	}
}
