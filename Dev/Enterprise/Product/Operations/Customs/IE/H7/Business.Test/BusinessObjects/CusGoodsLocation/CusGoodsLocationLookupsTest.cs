using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	public class CusGoodsLocationLookupsTest : TestCaseWithFactory
	{
		public void TestCusGoodsLocationQualifierList()
		{
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			var list = cusGoodsLocation.Lookups.QualifierList;

			CombineAssertions(() =>
			{
				AssertEquals(1, list.Count);

				Assert(list.ContainsCode("U"));
				AssertEquals("UN/LOCODE", list.GetDescriptionFromCode("U"));
			});
		}

		public void TestCusGoodsLocationTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, "location type");
			var mockTypeCodes = new List<string> { "A", "B", "C", "D" };
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType, li, "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime));
			Factory.Save();

			var goodsLocation = Factory.New<CusGoodsLocation>();
			var collection = goodsLocation.Lookups.TypeList;

			AssertEquals("location type list contains 4 items", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder("items exist in packtype list", new[] { "A", "B", "C", "D" }, collection.GetAllCodesZString());
		}

		public void TestUnlocodeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "unlocode type");
			var mockTypeCodes = new List<string> { "A", "B", "C", "D" };
			mockTypeCodes.ForEach(li => helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, IE.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, li, "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime));
			Factory.Save();

			var goodsLocation = Factory.New<CusGoodsLocation>();
			var collection = goodsLocation.Lookups.UnlocodeList as ZZRefCusCodeListCombinedCollection;
			collection.Load();

			AssertEquals("unlocode type list contains 4 items", 4, collection.Count);
			AssertContainsExactElementsInAnyOrder("items exist in unlocode list", new[] { "A", "B", "C", "D" }, collection.Select(c => c.ZZD_Code));
		}
	}
}
