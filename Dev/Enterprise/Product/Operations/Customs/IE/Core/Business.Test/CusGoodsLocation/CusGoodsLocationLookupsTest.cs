using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusGoodsLocationLookups))]
	sealed class CusGoodsLocationLookupsTest : TestCaseWithFactory
	{
		public void TestUnlocodeList_UCC5()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "GoodsLocation");
			var g1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G1", "G1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var g2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G2", "G2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5, "Ireland_UCC5");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "GoodsLocation");
			var g3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G3", "G3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var g4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G4", "G4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;

			var list = (ZZRefCusCodeListCombinedCollection)ts.GoodsLocation.Lookups.UnlocodeList;
			CombineAssertions(() =>
			{
				AssertSame("CustomsOfficeList should be same with UnlocodeList", list, ts.GoodsLocation.Lookups.CustomsOfficeList);
				var filter = list.CompleteFilter;
				Assert(Factory.Load<ZZRefCusCodeListCombined>(g3.PK).MatchesFilter(filter));
				Assert(Factory.Load<ZZRefCusCodeListCombined>(g4.PK).MatchesFilter(filter));
			});
		}

		public void TestUnlocodeList_UCC6()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "GoodsLocation");
			var g1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G1", "G1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var g2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G2", "G2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5, "Ireland_UCC5");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "GoodsLocation");
			var g3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G3", "G3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var g4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.IEUcc5, UniversalReferenceConstants.RefCusCodeListTypes.Codes.GoodsLocation, "G4", "G4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var ts = Factory.New<TemporaryStorageHeader>();
			ts.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;

			var list = (ZZRefCusCodeListCombinedCollection)ts.GoodsLocation.Lookups.UnlocodeList;
			CombineAssertions(() =>
			{
				AssertSame("CustomsOfficeList should be same with UnlocodeList", list, ts.GoodsLocation.Lookups.CustomsOfficeList);
				var filter = list.CompleteFilter;
				Assert(Factory.Load<ZZRefCusCodeListCombined>(g1.PK).MatchesFilter(filter));
				Assert(Factory.Load<ZZRefCusCodeListCombined>(g2.PK).MatchesFilter(filter));
			});
		}
	}
}
