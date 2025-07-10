using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(GoodsCatalogDownloadObjectLookups))]
	class GoodsCatalogDownloadObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsigneeOrConsignorList()
		{
			var parent = new GoodsCatalogDownloadObject(Factory);
			AssertType<ConsigneeOrConsignorCollection>(parent.Lookups.ConsigneeOrConsignorList);
		}

		public void TestBrokerList()
		{
			var parent = new GoodsCatalogDownloadObject(Factory);
			AssertType<GlbStaffCollection>(parent.Lookups.BrokerList);
		}
	}
}
