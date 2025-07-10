using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillBuyerCollection))]
	sealed class AsycudaBillBuyerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateOrganizationFromDefaults()
		{
			var collection = GetCollectionToTest();

			Bill.ABL_BuyerName = "TEST NAME";
			Bill.ABL_BuyerStreet1 = "TEST STREET 1";
			Bill.ABL_BuyerStreet2 = "TEST STREET 2";
			Bill.ABL_BuyerCity = "TEST CITY";
			Bill.ABL_RN_NKBuyerCountry = "US";
			Bill.ABL_BuyerState = "VI";
			Bill.ABL_BuyerPostcode = "123456";
			Bill.ABL_BuyerPhone = "1-123-123456";

			var org = Factory.New<OrgHeader>();
			collection.SetupNewElementButDoNotAddIt(org, true);

			AssertEquals("TEST NAME", org.OH_FullName);
			AssertEquals("TEST STREET 1", org.MainAddress.Address1);
			AssertEquals("TEST STREET 2", org.MainAddress.Address2);
			AssertEquals("TEST CITY", org.MainAddress.City);
			AssertEquals("US", org.MainAddress.OA_RN_NKCountryCode);
			AssertEquals("VI", org.MainAddress.OA_State);
			AssertEquals("123456", org.MainAddress.OA_PostCode);
			AssertEquals("1-123-123456", org.MainAddress.OA_Phone);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaBillBuyerCollection(Factory, Bill);

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
