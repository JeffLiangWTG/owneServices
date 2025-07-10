using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			NctsPreviousDocumentTestHelper.SetupCusCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N);
			Factory.Save();

			var list = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "DC40N_1", "DC40N_2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CodeList);
			});
		}

		public void TestCodeList_IncludeParentDataGrouping()
		{
			NctsPreviousDocumentTestHelper.SetupCusCodes_IncludeParentDataGrouping(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N);
			Factory.Save();

			var list = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "DC40N_1" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CodeList);
			});
		}

		public void TestCodeList_WithoutParentDataGrouping()
		{
			NctsPreviousDocumentTestHelper.SetupCusCodes_IncludeParentDataGrouping(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N);
			Factory.Save();

			nctsHeader.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var list = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("List", new[] { "DC40N_1" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CodeList);
			});
		}

		public void TestUnitOfQuantity2List()
		{
			var unpackPackType = Factory.SetupUnpackCusCode();
			var bulkPackType = Factory.SetupBulkCusCode();
			AssertEquals($"{unpackPackType}, {bulkPackType}", lookups.UnitOfQuantity2List.CodesAsString);
		}

		public void TestCustomsUnitOfQuantityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Argentina, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = lookups.UnitOfQuantityList;
				AssertEquals("ABC, DEF, JKL", list.CodesAsString);
				AssertSame(list, lookups.UnitOfQuantityList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			lookups = new NctsPreviousDocumentPhase5Lookups(previousDocument);
		}
		NctsHeader nctsHeader;
		NctsPreviousDocumentPhase5Lookups lookups;
	}
}
