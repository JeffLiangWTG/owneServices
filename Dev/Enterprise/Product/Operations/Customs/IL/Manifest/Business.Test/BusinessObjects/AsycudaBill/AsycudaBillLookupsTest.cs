using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBillKindList()
		{
			AssertType<AsycudaBillKindList>(lookups.BillKindList);
		}

		public void TestBillConditionList()
		{
			AssertType<ILBillConditionList>(lookups.ConditionList);
			var conditionList = Factory.GetCachedValue<ILBillConditionList>();

			AssertEquals("Should contain 4 items", 4, conditionList.Count);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("27", "Door to door");
			expectedList.AddPair("28", "Door to pier");
			expectedList.AddPair("29", "Pier to door");
			expectedList.AddPair("30", "Pier to pier");
			AssertEquals("Elements should match", expectedList.ElementsAsString, conditionList.ElementsAsString);

			var bill = Factory.New<AsycudaBill>();
			AssertSame("Condition list is cached", conditionList, bill.Lookups.ConditionList);
		}

		public void TestPackageTypeList()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILManifestPackageTypes, "IL Manifest Package Type List", Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "MPKG", "1A", "Drum steel", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "MPKG", "BE", "Bundle", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var codeList = lookups.PackageTypeList;
			AssertEquals(2, codeList.Count);
			AssertEquals("list contains BE, 1A and Sorted", "BE, 1A", codeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			lookups = bill.Lookups;
		}

		AsycudaBillLookups lookups;
	}
}
