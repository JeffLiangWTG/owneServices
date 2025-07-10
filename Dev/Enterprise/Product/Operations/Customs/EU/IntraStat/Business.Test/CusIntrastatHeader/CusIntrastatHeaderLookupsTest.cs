using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.EU.Intrastat.Business.Testing
{
	sealed class CusIntrastatHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			var refCountryCollection = lookups.Countries;
			AssertType<RefCountryCollection>(refCountryCollection);
		}

		public void TestSuppliers()
		{
			var suppliers = lookups.Suppliers;
			AssertType<ConsignorCollection>(suppliers);
		}

		public void TestConsignees()
		{
			var consignees = lookups.Consignees;
			AssertType<ConsigneeCollection>(consignees);
		}

		public void TestNatureOfTransactionList()
		{
			var lookupsNatureOfTransactionList = lookups.NatureOfTransactionList;
			AssertType<NatureOfTransactionList>(lookupsNatureOfTransactionList);
		}

		public void TestModeOfTransportListList()
		{
			var modeOfTransportList = lookups.ModeOfTransportList;
			AssertType<TransportTypeList>(modeOfTransportList);
		}

		public void TestIncoTermList()
		{
			var incoTermList = lookups.IncoTermList;
			AssertType<IncoTermsCodeDescriptionPairList>(incoTermList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new CusIntrastatHeaderLookups(IntrastatTestDataHelper.New(Factory).NewCusIntrastatHeaderWithValidData());
		}
		CusIntrastatHeaderLookups lookups;
	}
}
