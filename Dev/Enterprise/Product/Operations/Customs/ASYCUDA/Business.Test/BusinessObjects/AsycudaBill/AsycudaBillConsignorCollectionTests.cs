using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillConsignorCollection))]
	sealed class AsycudaBillConsignorCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestCreateOrganizationFromDefaults()
		{
			var collection = GetCollectionToTest();

			Bill.ABL_ShipperName = "TEST NAME";
			Bill.ABL_ShipperStreet1 = "TEST STREET 1";
			Bill.ABL_ShipperStreet2 = "TEST STREET 2";
			Bill.ABL_ShipperCity = "TEST CITY";
			Bill.ABL_RN_NKShipperCountry = "US";
			Bill.ABL_ShipperState = "VI";
			Bill.ABL_ShipperPostcode = "123456";
			Bill.ABL_ShipperPhone = "+00123456888";

			OrgHeader org = Factory.New<OrgHeader>();
			collection.SetupNewElementButDoNotAddIt(org, true);

			AssertEquals("TEST NAME", org.OH_FullName);
			AssertEquals("TEST STREET 1", org.MainAddress.Address1);
			AssertEquals("TEST STREET 2", org.MainAddress.Address2);
			AssertEquals("TEST CITY", org.MainAddress.City);
			AssertEquals("US", org.MainAddress.OA_RN_NKCountryCode);
			AssertEquals("VI", org.MainAddress.OA_State);
			AssertEquals("123456", org.MainAddress.OA_PostCode);
			AssertEquals("+00123456888", org.MainAddress.OA_Phone);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaBillConsignorCollection(Factory, Bill);

		AsycudaBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					bill = header.Bills.AddNew();
				}
				return bill;
			}
		}
		AsycudaBill bill;
	}
}
