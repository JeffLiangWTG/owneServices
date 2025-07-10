using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaBillSailingSynchronisationTest : SailingSynchronisationTest
	{
		public void TestIsMatched()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
			Assert("Should be true as the bill number is same with the house bill.", synchronisationTarget.IsMatched(fCLSailingBill));
			Assert("Should be false as the bill number is not same with the house bill.", !synchronisationTarget.IsMatched(rOROSailingBill));
		}

		public void TestSetBillOfLading()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "XXX";

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
			synchronisationTarget.Set(fCLSailingBill);

			AssertEquals("Should update the value from the bill of lading.", "AAA", bill.ABL_BillNumber);
		}

		public void TestSynchronise()
		{
			header.Bills.RemoveAndDeleteAll();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA";

			bill.ABL_RL_NKOrigin = "USCHI";
			bill.ABL_RL_NKFinalDestination = "AUBNE";

			bill.ABL_OA_Shipper = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_Consignee = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			bill.ABL_OA_NotifyParty = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			bill.ABL_PrepaidCollect = "PPD";
			bill.ABL_FreightValue = 998m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Australia;

			bill.ABL_MarksAndNumbers = "TEST MARK ON ASYCUDA BILL";
			bill.ABL_GoodsDescription = "TEST GOODS DESC ON ASYCUDA BILL";

			bill.ABL_GrossWeight = 1m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_Volume = 2m;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.ABL_ManifestQty = 13;
			bill.ABL_ManifestUQ = "PKG";

			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "C1";

			var pack1 = bill.Packs.AddNew();
			pack1.APA_LineNo = 0;
			pack1.APA_PackQty = 2;

			var pack2 = bill.Packs.AddNew();
			pack2.APA_LineNo = 1;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
			synchronisationTarget.Synchronise();

			CombineAssertions(() =>
			{
				AssertNull("Should reset to null after synchronisation.", header.BillOfLadingForSync);

				AssertEquals("JS_RL_NKOrigin", "AUSYD", bill.ABL_RL_NKOrigin);
				AssertEquals("JS_RL_NKDestination", "SGSIN", bill.ABL_RL_NKFinalDestination);

				AssertEquals("Consignor.MainAddress.PK", consignor.MainAddress.PK, bill.ABL_OA_Shipper);
				AssertEquals("Consignee.MainAddress.PK", consignee.MainAddress.PK, bill.ABL_OA_Consignee);
				AssertEquals("NotifyParty.MainAddress.PK", notifyParty.MainAddress.PK, bill.ABL_OA_NotifyParty);

				AssertEquals("JS_INCO", "CLT", bill.ABL_PrepaidCollect);
				AssertEquals("JS_GoodsValue", 2018m, bill.ABL_FreightValue);
				AssertEquals("JS_RX_NKGoodsValueCurr", Core.Constants.CurrencyCodes.Singapore, bill.ABL_RX_NKFreightValueCurrency);

				AssertEquals("JS_MarksAndNumbers", "TEST MARKS ON FCLSailingBill", bill.ABL_MarksAndNumbers);
				AssertEquals("JS_GoodsDescription", "TEST GOODS DESC ON FCLSailingBill", bill.ABL_GoodsDescription);

				AssertEquals("JS_ActualWeight", 1000m, bill.ABL_GrossWeight);
				AssertEquals("JS_UnitOfWeight", Core.Constants.Weight.Grams, bill.ABL_GrossWeightUQ);

				AssertEquals("JS_ActualVolume", 15m, bill.ABL_Volume);
				AssertEquals("JS_UnitOfVolume", Core.Constants.Volume.Litre, bill.ABL_VolumeUQ);

				AssertEquals("JS_OuterPacks", 11, bill.ABL_ManifestQty);
				AssertEquals("JS_F3_NKPackType", "PLT", bill.ABL_ManifestUQ);
			});

			AssertEquals("Creates a new container to match the sailing container. Keeps the existing.", 2, header.Containers.Count);
			var container2 = header.Containers.Cast<AsycudaContainer>().FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111");
			AssertEquals("ACN_ContainerNumber", "ABCD1111", container2.ACN_ContainerNumber);

			AssertEquals("Creates a new pack to match the sailing packline, deletes the existing.", 1, bill.Packs.Count);
			var pack = bill.Packs.Cast<AsycudaPack>().FirstOrDefault(x => x.APA_LineNo == 0);

			CombineAssertions(() =>
			{
				AssertEquals("APA_LineNo", (ZShort)0, pack.APA_LineNo);
				AssertEquals("APA_PackQty", 11, pack.APA_PackQty);
				AssertEquals("APA_PackUQ", "PLT", pack.APA_PackUQ);
				AssertEquals("APA_Weight", 1000m, pack.APA_Weight);
				AssertEquals("APA_WeightUQ", Core.Constants.Weight.Grams, pack.APA_WeightUQ);
				AssertEquals("APA_Volume", 15m, pack.APA_Volume);
				AssertEquals("APA_VolumeUQ", Core.Constants.Volume.Litre, pack.APA_VolumeUQ);
				AssertEquals("APA_GoodsDescription", "DESCRIPTION", pack.APA_GoodsDescription);
				AssertEquals("APA_MarksAndNumbers", "MARKS AND NUMBERS", pack.APA_MarksAndNumbers);

				AssertEquals("Locates matching container for pack", container2.PK, pack.ContainerPK);
			});
		}

		public void TestSynchroniseMultipleContainers()
		{
			var sailingContainer2 = fCLSailingBill.RealContainers.AddNew();
			sailingContainer2.JC_ContainerNum = "ABCD2222";
			sailingContainer2.JC_RC = Factory.New<RefContainer>().PK;
			var sailingPackLine2 = fCLSailingBill.OuterPackLines.AddNew();
			sailingPackLine2.JL_ItemNo = 1;
			sailingContainer2.PackLines.Add(sailingPackLine2);
			var sailingPackLine3 = fCLSailingBill.OuterPackLines.AddNew();
			sailingPackLine3.JL_ItemNo = 2;
			sailingContainer2.PackLines.Add(sailingPackLine3);

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA"; // for matching

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
			synchronisationTarget.Synchronise();

			AssertEquals("Creates containers to match the sailing containers", 2, header.Containers.Count);
			AssertEquals("Creates packs to match the sailing packlines", 3, bill.Packs.Count);

			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var container1 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111");
			var container2 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD2222");

			var billPacks = bill.Packs.Cast<AsycudaPack>().OrderBy(p => p.APA_LineNo).ToArray();
			AssertEquals("Pack1 is mapped to container1", container1.PK, billPacks[0].ContainerPK);
			AssertEquals("Pack2 is mapped to container2", container2.PK, billPacks[1].ContainerPK);
			AssertEquals("Pack3 is mapped to container2", container2.PK, billPacks[2].ContainerPK);
		}

		public void TestSynchroniseEmptyContainers()
		{
			var sailingContainer2 = fCLSailingBill.RealContainers.AddNew();
			sailingContainer2.JC_ContainerNum = "ABCD2222";
			sailingContainer2.JC_RC = Factory.New<RefContainer>().PK;
			sailingContainer2.JC_IsEmptyContainer = true;

			var sailingContainer3 = fCLSailingBill.RealContainers.AddNew();
			sailingContainer3.JC_ContainerNum = "ABCD3333";
			sailingContainer3.JC_RC = Factory.New<RefContainer>().PK;
			sailingContainer3.JC_IsEmptyContainer = false;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "AAA"; // for matching

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLading>)bill;
			synchronisationTarget.Synchronise();

			AssertEquals("Creates containers to match all the sailing containers including container without packlines", 3, header.Containers.Count);
			AssertEquals("Creates packs to match the sailing packlines and create empty packline for container without packlines", 3, bill.Packs.Count);

			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var container1 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111");
			var container2 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD2222");
			var container3 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD3333");

			var billPacks = bill.Packs.Cast<AsycudaPack>().OrderBy(p => p.APA_LineNo).ToArray();
			AssertEquals("Pack1 is mapped to container1", container1.PK, billPacks[0].ContainerPK);
			AssertEquals("Pack2 is mapped to container2", container2.PK, billPacks[1].ContainerPK);
			AssertEquals("Pack2 is mapped to container3", container3.PK, billPacks[2].ContainerPK);
		}
	}
}
