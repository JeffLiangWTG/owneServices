using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class GoodsItemProviderTest : DataProviderTestCase<GoodsItemProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaPack missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(pack));
			});
		}

		public void TestGoodsItemNumber()
		{
			pack.APA_LineNo = 1;
			AssertEquals("GoodsItemNumber", 1, Provider.GoodsItemNumber);
		}

		public void TestGrossMass()
		{
			pack.APA_Weight = 143125m;
			pack.APA_WeightUQ = "G";
			AssertEquals("GrossMass", 143.125m, Provider.GrossMass);
		}

		public void TestNumberOfPackages()
		{
			pack.APA_PackQty = 1;
			AssertEquals("NumberOfPackages", 1, Provider.NumberOfPackages);
		}

		public void TestDescriptionOfGoods()
		{
			pack.PackedItem.API_GoodsDescription = "goods description";
			AssertEquals("DescriptionOfGoods", "goods description", Provider.DescriptionOfGoods);
		}

		public void TestAdditionalInformationCollection()
		{
			pack.AdditionalInfos.AddNew();
			Assert("AdditionalInfos", Provider.AdditionalInformationCollection.Count > 0);
		}

		public void TestAdditionalSupplyChainActors()
		{
			pack.CusSupplyChainActorReferences.AddNew();
			Assert("AdditionalSupplyChainActor", Provider.AdditionalSupplyChainActors.Count > 0);
		}

		public void TestSupportingDocuments()
		{
			pack.SupportingDocuments.AddNew();
			AssertEquals(1, Provider.SupportingDocuments.Count);
		}

		public void TestUNDGs()
		{
			var newUdng = pack.UNDGs.AddNew();
			newUdng.DI_DG_NKSubs = "123bb";

			var udng = Provider.UNDGs.Single();
			AssertEquals("123b", udng);
		}

		public void TestPackaging()
		{
			pack.APA_MarksAndNumbers = "1";
			pack.APA_PackUQ = "KG";
			pack.APA_PackQty = 2;

			var packaging = Provider.Packaging.Single();

			CombineAssertions(() =>
			{
				AssertEquals("1", packaging.ShippingMarks);
				AssertEquals("KG", packaging.TypeOfPackages);
				AssertEquals(2, packaging.NumberOfPackages);
			});

			pack.APA_PackUQ = "VQ";
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, packaging.ShippingMarks);
				AssertEquals("VQ", packaging.TypeOfPackages);
				AssertNull(packaging.NumberOfPackages);
			});
		}

		public void TestTransportEquipmentCollection()
		{
			AssertEquals(1, Provider.TransportEquipmentCollection.Count);
		}

		public void TestTransportEquipmentCollection_NoContainer()
		{
			pack.ContainerPK = ZGuid.Empty;
			var provider = GetProvider();
			AssertEquals(0, provider.TransportEquipmentCollection.Count);
		}

		public void TestCommodityCode()
		{
			pack.PackedItem.API_Tariff = "12345678";

			CombineAssertions(() =>
			{
				AssertEquals("HarmonizedSystemSubHeadingCode", "123456", Provider.CommodityCode.HarmonizedSystemSubHeadingCode);
				AssertEquals("CombinedNomenclatureCode", "78", Provider.CommodityCode.CombinedNomenclatureCode);
			});
		}

		public void TestPostalCharge()
		{
			pack.PackedItem.API_GoodsValue = 10.00m;
			pack.PackedItem.API_RX_NKGoodsValueCurrency = "EUR";

			AssertEquals("PostalCharge Value", 10.00m, Provider.PostalCharge.Value);
			AssertEquals("PostalCharge Currency", "EUR", Provider.PostalCharge.Currency);
		}

		public void TestTypesOfGoods()
		{
			pack.PackedItem.API_TypeOfGoods = "123";

			AssertEquals("Type Of Goods", "123", Provider.TypesOfGoods);
		}

		public void TestCUSCode()
		{
			CombineAssertions(() =>
			{
				AssertNull(Provider.CUSCode);
				pack.PackedItem.API_ChemicalSubstanceCode = "ABC123456";
				AssertEquals("ABC123456", Provider.CUSCode);
			});
		}

		GoodsItemProvider GenerateProvider(AsycudaPack pack) => new(pack);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
		}
		AsycudaPack pack;

		protected override GoodsItemProvider GetProvider()
		{
			return GenerateProvider(pack);
		}
	}
}
