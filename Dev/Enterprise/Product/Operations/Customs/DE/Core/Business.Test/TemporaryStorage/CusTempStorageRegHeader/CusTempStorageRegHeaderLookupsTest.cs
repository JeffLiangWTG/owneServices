using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public class CusTempStorageRegHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPreviousReferenceTypeList()
		{
			AssertSame(Factory.GetCachedValue<PreviousReferenceType>(), header.Lookups.PreviousReferenceTypeList);
		}

		public void TestCustomsOfficeList()
		{
			AssertType(typeof(CustomsOfficeCodeCollection), header.Lookups.CustomsOfficeList);
		}

		public void TestStatusList()
		{
			AssertSame(Factory.GetCachedValue<CustomsStatusList>(), header.Lookups.StatusList);
		}

		public void TestReportStatusList()
		{
			var provider = new CusTempStorageRegHeaderLookups(header);
			var actualCodes = provider.ReportStatusList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DEL", "FIN", "LCK", "PAC", "PRE", "TST", "NCM" }, actualCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusTempStorageRegHeader>();
		}
		CusTempStorageRegHeader header;
	}
}

