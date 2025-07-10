using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaBillSellerCollection))]
	sealed class AsycudaBillSellerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCreateOrganizationFromDefaults()
		{
			var collection = GetCollectionToTest();

			Bill.ABL_SellerName = "TEST NAME";
			Bill.ABL_SellerStreet1 = "TEST STREET 1";
			Bill.ABL_SellerStreet2 = "TEST STREET 2";
			Bill.ABL_SellerCity = "TEST CITY";
			Bill.ABL_RN_NKSellerCountry = "US";
			Bill.ABL_SellerState = "VI";
			Bill.ABL_SellerPostcode = "123456";
			Bill.ABL_SellerPhone = "1-123-123456";

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

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaBillSellerCollection(Factory, Bill);

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
