using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class CommodityType03ProviderTest : Customs.Business.Testing.DataProviderTestCase<CommodityType03Provider>
	{
		public void TestDescriptionOfGoods()
		{
			item.BY_Description = "desc";
			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			provider = new CommodityType03Provider(unloadedItem);
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("No changes are made to BY_Description", provider.DescriptionOfGoods);

				unloadedItem.BY_Description = "desc changed";
				AssertEquals("Changes are made to BY_Description", "desc changed", provider.DescriptionOfGoods);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new CommodityType03Provider(item);
				AssertEquals("Description is always sent on NEW goodsItem", "desc", provider.DescriptionOfGoods);
			});
		}

		public void TestCusCode()
		{
			item.BY_CusC4Number = "cuscode";
			AssertEquals("cuscode", Provider.CusCode);
		}

		public void TestHarmonizedSystemSubHeadingCode()
		{
			item.BY_HarmonisedTariff = "87654321";
			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			provider = new CommodityType03Provider(unloadedItem);
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("No changes are made to BY_HarmonisedTariff", provider.HarmonizedSystemSubHeadingCode);

				unloadedItem.BY_HarmonisedTariff = "12345678";
				AssertEquals("Changes are made to BY_HarmonisedTariff", "123456", provider.HarmonizedSystemSubHeadingCode);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new CommodityType03Provider(item);
				AssertEquals("HarmonizedSystemSubHeadingCode is always sent on NEW GoodsItem", "876543", provider.HarmonizedSystemSubHeadingCode);
			});
		}

		public void TestCombinedNomenclatureCode()
		{
			item.BY_HarmonisedTariff = "87654321";
			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			provider = new CommodityType03Provider(unloadedItem);
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("No changes are made to HarmonizedSystemSubHeadingCode", provider.HarmonizedSystemSubHeadingCode);

				unloadedItem.BY_HarmonisedTariff = "12345678";
				AssertEquals("Changes are made to HarmonizedSystemSubHeadingCode", "78", provider.CombinedNomenclatureCode);

				unloadedItem.BY_HarmonisedTariff = "123456";
				AssertNullOrEmpty("Changes are made to HarmonizedSystemSubHeadingCode, length less than 8", provider.CombinedNomenclatureCode);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new CommodityType03Provider(item);
				AssertEquals("CombinedNomenclatureCode is always sent on NEW GoodsItem, when length of HarmonizedSystemSubHeadingCode = 8", "21", provider.CombinedNomenclatureCode);

				item.BY_HarmonisedTariff = "876543";
				AssertNullOrEmpty("CombinedNomenclatureCode is not sent on NEW GoodsItem, when length of HarmonizedSystemSubHeadingCode < 8", provider.CombinedNomenclatureCode);
			});
		}

		public void TestGrossMass()
		{
			item.BY_GrossWeight = 200.0000m;
			item.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			provider = new CommodityType03Provider(unloadedItem);
			CombineAssertions(() =>
			{
				AssertNull("GrossMass is null if no changes are made to BY_GrossWeight", provider.GrossMass);

				unloadedItem.BY_GrossWeight = 100.0000m;
				AssertEquals("GrossMass in Kilograms", 0.1m, provider.GrossMass);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new CommodityType03Provider(item);
				AssertEquals("GrossMass is always sent on NEW GoodsItem", 0.2m, provider.GrossMass);
			});
		}

		public void TestNetMass()
		{
			item.BY_NetWeight = 200.0000m;
			item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			provider = new CommodityType03Provider(unloadedItem);
			CombineAssertions(() =>
			{
				AssertNull("NetMass is null if no changes are made to BY_NetWeight", provider.NetMass);

				unloadedItem.BY_NetWeight = 100.0000m;
				AssertEquals("NetMass in Kilograms", 0.1m, provider.NetMass);

				unloadedItem.BY_NetWeight = 0;
				AssertNull("NetMass is null if BY_NetWeight = 0", provider.NetMass);

				unloadedItem.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
				AssertNull("NetMass is null in MIS state", provider.NetMass);

				item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				provider = new CommodityType03Provider(item);
				AssertEquals("NetMass is always sent on NEW GoodsItems", 0.2m, provider.NetMass);
			});
		}

		public void TestNetMass_Zero()
		{
			item.BY_NetWeight = 200.0000m;
			item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var unloadedItem = item.UnloadedGoodsItem;
			provider = new CommodityType03Provider(unloadedItem);
			AssertNetMass_Zero(item.Header.PreviousDocuments, provider, unloadedItem);
			AssertNetMass_Zero(item.Bill.PreviousDocuments, provider, unloadedItem);
		}

		static void AssertNetMass_Zero(ICusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> previousDocuments, CommodityType03Provider provider, NctsCommonCargoDesc item)
		{
			var previousDoc = previousDocuments.AddNew();
			previousDoc.CSI_Code = "N830";
			item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			CombineAssertions($"{previousDocuments.Master.GetType()} {previousDocuments.GetType()}", () =>
			{
				item.BY_NetWeight = 0;
				AssertEquals("Value 0", 0m, provider.NetMass);
				item.BY_NetWeight = 100.0000m;
				AssertEquals("Not Empty", 0.1m, provider.NetMass);
			});
		}

		protected override CommodityType03Provider GetProvider() => provider;

		protected override void SetUp()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			item = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
			provider = new CommodityType03Provider(item);
		}
		CommodityType03Provider provider;
		NctsArrivalCargoDesc item;
	}
}
