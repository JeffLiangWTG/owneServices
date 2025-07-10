using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend50GoodsItemProviderTest : DataProviderTestCase<SendAndAmend50GoodsItemProvider>
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

		public void TestSupportingDocuments()
		{
			pack.SupportingDocuments.AddNew();
			AssertEquals(1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalInformationCollection()
		{
			pack.AdditionalInfos.AddNew();
			AssertEquals(1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestAdditionalSupplyChainActors()
		{
			pack.CusSupplyChainActorReferences.AddNew();
			AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestDescriptionOfGoods()
		{
			pack.PackedItem.API_GoodsDescription = "goods description";
			AssertEquals("DescriptionOfGoods", "goods description", Provider.DescriptionOfGoods);
		}

		public void TestCUSCode()
		{
			AssertNull(Provider.CUSCode);
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

		public void TestUNDGs()
		{
			var newUdng = pack.UNDGs.AddNew();
			newUdng.DI_DG_NKSubs = "123bb";

			var udng = Provider.UNDGs.Single();
			AssertEquals("123b", udng);
		}

		public void TestGrossMass()
		{
			pack.APA_Weight = 1312.123m;
			pack.APA_WeightUQ = "G";
			AssertEquals("GrossMass", 1.312123m, Provider.GrossMass);
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

		public void TestPassiveBorderTransportMeans()
		{
			_ = pack.AsycudaTransportMeans.AddNew();

			AssertEquals(1, Provider.PassiveBorderTransportMeans.Count);
			AssertType<PassiveBorderTransportMeansProvider>(Provider.PassiveBorderTransportMeans.Single());
		}

		public void TestTransportEquipmentCollection()
		{
			AssertEquals(1, Provider.TransportEquipmentCollection.Count);
		}

		SendAndAmend50GoodsItemProvider GenerateProvider(AsycudaPack pack) => new(pack);

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

		protected override SendAndAmend50GoodsItemProvider GetProvider()
		{
			return GenerateProvider(pack);
		}
	}
}
