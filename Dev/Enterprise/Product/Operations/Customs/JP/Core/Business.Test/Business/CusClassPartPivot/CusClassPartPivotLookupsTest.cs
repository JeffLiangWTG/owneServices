using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotLookups))]
	sealed class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFEFTAArticle48List()
		{
			var fEFTAArticle48List = cusClassPartPivotLookups.FEFTAArticle48List;
			AssertType<FEFTAArticle48List>(fEFTAArticle48List);
		}

		public void TestTradeControlOrderAppendixList()
		{
			var newFactory = new BusinessObjectFactory();
			TestDataCoreHelper.PrepareTradeControlOrderAppendixList(newFactory);

			cusClassPartPivot.CI_ChildType = ClassificationTypeList.Codes.HTB;

			var tradeControlOrderAppendixList = cusClassPartPivotLookups.TradeControlOrderAppendixList;
			AssertEquals(0, tradeControlOrderAppendixList.Count);

			cusClassPartPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			tradeControlOrderAppendixList = cusClassPartPivotLookups.TradeControlOrderAppendixList;
			AssertEquals(2, tradeControlOrderAppendixList.Count);
			Assert(tradeControlOrderAppendixList.ContainsCode("12345"));
			Assert(tradeControlOrderAppendixList.ContainsCode("22345"));

			cusClassPartPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			tradeControlOrderAppendixList = cusClassPartPivotLookups.TradeControlOrderAppendixList;
			AssertEquals(2, tradeControlOrderAppendixList.Count);
			Assert(tradeControlOrderAppendixList.ContainsCode("1234"));
			Assert(tradeControlOrderAppendixList.ContainsCode("2345"));
		}

		public void TestConsumptionTaxExemptionIdList()
		{
			new BusinessObjectFactory().CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportConsumptionTaxExemptionCode, "B", "E", "G");
			AssertContainsExactElementsInExactOrder(["B", "E", "G"], cusClassPartPivotLookups.ConsumptionTaxExemptionIDList.GetAllCodes());
		}

		public void TestJP_StorageTypeList()
		{
			var storageTypeList = cusClassPartPivotLookups.StorageTypeList;

			var storageTypeListWhenDeclarationTypeIsA = Factory.GetCachedValue<StorageTypeListWhenDeclarationTypeIsA>();
			var storageTypeListWhenDeclarationTypeIsG = Factory.GetCachedValue<StorageTypeListWhenDeclarationTypeIsG>();
			var storageTypeListWhenTransportModeIsSea = Factory.GetCachedValue<StorageTypeListWhenTransportModeIsSea>();

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddRange(storageTypeListWhenDeclarationTypeIsA);
			expectedList.AddRange(storageTypeListWhenDeclarationTypeIsG);
			expectedList.AddRange(storageTypeListWhenTransportModeIsSea);

			AssertContainsExactElementsInAnyOrder(expectedList, storageTypeList);
		}

		protected override void SetUp()
		{
			cusClassPartPivot = Factory.New<CusClassPartPivot>();
			cusClassPartPivotLookups = cusClassPartPivot.Lookups;
		}

		CusClassPartPivot cusClassPartPivot;
		CusClassPartPivotLookups cusClassPartPivotLookups;
	}
}
