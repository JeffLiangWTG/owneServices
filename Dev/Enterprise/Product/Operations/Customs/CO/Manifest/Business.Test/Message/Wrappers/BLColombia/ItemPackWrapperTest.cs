using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class ItemPackWrapperTest : TestCaseWithFactory
	{
		public void TestItemPackWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Containers.AddNew();

			var bill = header.Bills.AddNew();
			CreateAndPopulatePack(header);

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var item = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(0).Item;

			CombineAssertions(() =>
			{
				AssertEquals("PO", item.PackagingCode);
				AssertEquals("GOODS", item.GeneralID);
				AssertEquals(ZBool.True, item.DangerousGood);
				AssertEquals(12345, item.ONUSerialNumber);
				AssertEquals("1", item.Ipel);
			});
		}

		public void TestONUSerialNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Containers.AddNew();

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var undg = pack.UNDGs.AddNew();
			undg.DI_DG_NKSubs = "x1";

			var pack2 = bill.Packs.AddNew();
			var undg2 = pack2.UNDGs.AddNew();
			undg2.DI_DG_NKSubs = "xx";

			var pack3 = bill.Packs.AddNew();
			var undg3 = pack3.UNDGs.AddNew();
			undg3.DI_DG_NKSubs = ZString.Empty;

			var pack4 = bill.Packs.AddNew();
			var undg4 = pack4.UNDGs.AddNew();
			undg4.DI_DG_NKSubs = "12x";

			var pack5 = bill.Packs.AddNew();
			var undg5 = pack5.UNDGs.AddNew();
			undg5.DI_DG_NKSubs = "13215";

			Factory.Save();

			var validDocumentIDs = header.GetValidDocumentIDs(header.Bills.Count + 1);
			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, validDocumentIDs);

			var item = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(0).Item;
			var item2 = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(1).Item;
			var item3 = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(2).Item;
			var item4 = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(3).Item;
			var item5 = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(4).Item;

			CombineAssertions(() =>
			{
				AssertEquals(1, item.ONUSerialNumber);
				AssertEquals(0, item2.ONUSerialNumber);
				AssertEquals(0, item3.ONUSerialNumber);
				AssertEquals(12, item4.ONUSerialNumber);
				AssertEquals(13215, item5.ONUSerialNumber);
			});
		}

		void CreateAndPopulatePack(AsycudaManifestHeader header)
		{
			var pack = header.Bills[0].Packs.AddNew();

			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.APA_GoodsDescription = "GOODS";
			pack.IsHazardous = true;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG_NKSubs = "12345";
			undg.DI_IMOClass = "1";

			pack.ContainerPK = header.Containers[0].PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932049";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "PO";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "CO";
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = "BAG";

			Factory.Save();
		}
	}
}
