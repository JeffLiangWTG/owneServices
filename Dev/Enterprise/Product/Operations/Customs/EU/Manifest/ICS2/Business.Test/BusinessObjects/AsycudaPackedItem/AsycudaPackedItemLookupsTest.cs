using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaPackedItemLookups))]
	sealed class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate]
		public void TestTypeOfGoodsList()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "IC2TG");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var lvDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunDataGrouping);
			var matchingCodeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "A", "EuropeanUnion 1", yesterday, tomorrow);
			var matchingCodeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "B", "EuropeanUnion 2", yesterday, tomorrow);
			var nonMatchingCodeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "C", "EuropeanUnion 3", yesterday.AddDays(-1), yesterday);
			var nonMatchingCodeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "D", "Latvia 1", yesterday, tomorrow);
			var nonMatchingCodeLists3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "E", "Latvia 2", yesterday, tomorrow);
			var nonMatchingCodeLists4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2TG, "F", "Germany 1", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packItem = bill.PackedItems.AddNew();
			var codeList = packItem.Lookups.TypeOfGoodsList;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Matched CodeType: IC2TG, dataGrouping: EUN, date: today", ["A", "B"], codeList.GetAllCodesZString());
				AssertSame("Cached", codeList, packItem.Lookups.TypeOfGoodsList);
			});
		}

		[TestDate]
		public void TestChemicalSubstanceCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ECICS");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");

			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var lvDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunDataGrouping);
			Factory.Save();

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "A", "Latvian 1", yesterday, tomorrow);
			var codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "B", "Latvian 1", yesterday, tomorrow);
			var codeLists3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "C", "Bad Type", yesterday, tomorrow);
			var codeLists4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "D", "European", yesterday, tomorrow);
			var codeLists5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "E", "Outdated", yesterday, yesterday);
			var codeLists6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "F", "European 1", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packItem = bill.PackedItems.AddNew();
			var codeList = packItem.Lookups.ChemicalSubstanceCodeList;
			codeList.Load();

			AssertContainsExactElementsInAnyOrder("Matched CodeType: ECICS, dataGrouping: EUN, date: today",
				new ZGuid[] { codeLists6.PK }, codeList.Select(x => x.PK));
		}
	}
}
