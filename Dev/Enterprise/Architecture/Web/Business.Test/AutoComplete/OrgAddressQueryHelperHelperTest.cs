using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class OrgAddressQueryHelperHelperTest : TestCaseWithFactory
	{
		void SetupTestData(ZGuid parentPK, OrgAddressType addressType)
		{
			if (addressType != null)
			{
				addressType1 = Factory.NewWithValidTestData<OrgAddress>();
				addressType1.OA_Code = addressType.Code;
				addressType1.OA_OH = parentPK;
				addressType1.OA_IsActive = true;
				var addressTypeCapability1 = Factory.NewWithValidTestData<OrgAddressCapability>();
				addressTypeCapability1.PZ_OA = addressType1.PK;
				addressTypeCapability1.PZ_AddressType = addressType.Code;

				addressType2 = Factory.NewWithValidTestData<OrgAddress>();
				addressType2.OA_Code = addressType.Code + "2";
				addressType2.OA_OH = parentPK;
				addressType2.OA_IsActive = true;
				var addressTypeCapability2 = Factory.NewWithValidTestData<OrgAddressCapability>();
				addressTypeCapability2.PZ_OA = addressType2.PK;
				addressTypeCapability2.PZ_AddressType = addressType.Code;
				addressTypeCapability2.PZ_IsMainAddress = true;
			}

			var someOtherAddress = Factory.NewWithValidTestData<OrgAddress>();
			someOtherAddress.OA_Code = "ZZZ";
			someOtherAddress.OA_OH = parentPK;
			someOtherAddress.OA_IsActive = true;
			var someOtherCapability = Factory.NewWithValidTestData<OrgAddressCapability>();
			someOtherCapability.PZ_OA = someOtherAddress.PK;
			someOtherCapability.PZ_AddressType = "ZZZ";
			someOtherCapability.PZ_IsMainAddress = true;

			Factory.Save();
		}
		OrgAddress addressType1;
		OrgAddress addressType2;

		public void TestAddressTypeQuery_IncludeMainAddress()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			SetupTestData(parentOrg.PK, OrgAddressType.Receivables);

			var query = new ZDBOnlyQuery(typeof(OrgAddress));
			query.AddToFilter(OrgAddressSchema.OA_OH, parentOrg.PK);
			var subQuery = OrgAddressQueryHelper.GetAddressTypeSubQuery(OrgAddressType.Receivables);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var result = Factory.Load<OrgAddress>(query);

			AssertEquals("Should be 2 receivable addresses and 1 main address", 3, result.Length);
			AssertContainsExactElementsInAnyOrder(result, new[] { parentOrg.MainAddress, addressType1, addressType2 });
		}

		public void TestAddressTypeQuery_NoMainAddress()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			SetupTestData(parentOrg.PK, OrgAddressType.Receivables);

			var query = new ZDBOnlyQuery(typeof(OrgAddress));
			query.AddToFilter(OrgAddressSchema.OA_OH, parentOrg.PK);
			var subQuery = OrgAddressQueryHelper.GetAddressTypeSubQuery(OrgAddressType.Receivables, false);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var result = Factory.Load<OrgAddress>(query);

			AssertEquals("Should be 2 receivable main addresses", 2, result.Length);
			AssertContainsExactElementsInAnyOrder(result, new[] { addressType1, addressType2 });
		}
	}
}
