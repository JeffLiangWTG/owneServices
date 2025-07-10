using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillConsigneeCollection))]
	sealed class AsycudaBillConsigneeCollectionTests : BusinessObjectCollectionTestCase
	{
		public void TestCreateOrganizationFromDefaults()
		{
			var collection = GetCollectionToTest();

			Bill.ABL_ConsigneeName = "TEST NAME";
			Bill.ABL_ConsigneeStreet1 = "TEST STREET 1";
			Bill.ABL_ConsigneeStreet2 = "TEST STREET 2";
			Bill.ABL_ConsigneeCity = "TEST CITY";
			Bill.ABL_RN_NKConsigneeCountry = "US";
			Bill.ABL_ConsigneeState = "VI";
			Bill.ABL_ConsigneePostcode = "123456";
			Bill.ABL_ConsigneePhone = "1-123-123456";

			OrgHeader org = Factory.New<OrgHeader>();
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

		protected override BusinessObjectCollection GetCollectionToTest() => new AsycudaBillConsigneeCollection(Factory, Bill);

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
