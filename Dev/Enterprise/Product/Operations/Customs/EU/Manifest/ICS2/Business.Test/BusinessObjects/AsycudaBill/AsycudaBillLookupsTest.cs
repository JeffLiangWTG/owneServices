using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestScreeningAuthorizedPersonTypes()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.ScreeningAuthorizedPersonTypes;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, bill.Lookups.ScreeningAuthorizedPersonTypes);
			Assert(list.ContainsCode("1"));
			Assert(list.ContainsCode("2"));
			Assert(list.ContainsCode("3"));
		}

		public void TestPrepaidCollectList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.PrepaidCollectList;

			AssertType<EUICS2PaymentMethodList>(list);
			AssertSame("Cache", list, bill.Lookups.PrepaidCollectList);
		}

		public void TestCustomsPackList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PKG");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.PackageTypes, "TST", "TST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.PackUQList;
			AssertArrayEqualsByElements(new[] { "TST" }, list.GetAllCodes());
		}

		public void TestCodeList733()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL733, "CL733");
			helper.CreateNewOrGetExistingCusCodeList(RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL733, "733TST", "733 Test Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.CodeList733;

			AssertArrayEqualsByElements(new[] { "733TST" }, list.GetAllCodes());
		}
	}
}
