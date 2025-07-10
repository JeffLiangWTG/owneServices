using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackSailingSynchronisationTest : TestCaseWithFactory
	{
		public void TestIsMatched()
		{
			var sailingBill = Factory.New<BillOfLading>();
			sailingBill.JS_HouseBill = "B1";
			var sailingContainer = sailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = "C1";
			var sailingPackLine = sailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_ItemNo = 0;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "B1";
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "C1";
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.APA_LineNo = 0;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingPackLine>)pack;
			AssertEquals("Matched when BillNumber, ContainerNumber and PackNumber are same", true, synchronisationTarget.IsMatched(sailingPackLine));

			sailingContainer.JC_ContainerNum = "SC1";
			AssertEquals("Not matched when BillNumber is different", false, synchronisationTarget.IsMatched(sailingPackLine));

			sailingContainer.JC_ContainerNum = "C1";
			sailingBill.JS_HouseBill = "SB1";
			AssertEquals("Not matched when ContainerNumber is different", false, synchronisationTarget.IsMatched(sailingPackLine));

			sailingBill.JS_HouseBill = "B1";
			sailingPackLine.JL_ItemNo = 1;
			AssertEquals("Not matched when PackNumber is different", false, synchronisationTarget.IsMatched(sailingPackLine));
		}

		public void TestSet()
		{
			var sailingPackLine = Factory.New<BillOfLadingPackLine>();
			var pack = Factory.New<AsycudaPack>();

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingPackLine>)pack;
			AssertNull("NULL when not matched", synchronisationTarget.Source);

			synchronisationTarget.Set(sailingPackLine);
			AssertSame("Finds sailingPackLine when Set", sailingPackLine, synchronisationTarget.Source);
		}

		public void TestSource()
		{
			var sailingBill = Factory.New<BillOfLading>();
			sailingBill.JS_HouseBill = "B1";
			var sailingContainer = sailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = "C1";
			var sailingPackLine = sailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_ItemNo = 0;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "B1";
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "C1";
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.APA_LineNo = 0;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingPackLine>)pack;
			AssertNull("NULL when BillOfLadingForSync is not set.", synchronisationTarget.Source);

			header.BillOfLadingForSync = sailingBill;

			AssertEquals("Matched when BillNumber, ContainerNumber and PackNumber are same", true, synchronisationTarget.IsMatched(sailingPackLine));
			AssertSame("Finds sailingPackLine when matched", sailingPackLine, synchronisationTarget.Source);

			synchronisationTarget.Set(null);
			pack.ContainerPK = ZGuid.Empty;
			AssertNull("NULL when pack is not linked to a container.", synchronisationTarget.Source);

			pack.ContainerPK = container.PK;
			sailingContainer.JC_ContainerNum = "SC1";
			AssertNull("NULL when not matched", synchronisationTarget.Source);

			sailingContainer.JC_ContainerNum = "C1";
			sailingBill.JS_HouseBill = "SB1";
			AssertNull("NULL when not matched", synchronisationTarget.Source);

			sailingBill.JS_HouseBill = "B1";
			sailingPackLine.JL_ItemNo = 1;
			AssertNull("NULL when not matched", synchronisationTarget.Source);
		}

		public void TestSynchroniseFromSet()
		{
			// this mode of synchronise will be employed when initialising a new pack from a packline
			var sailingBill = Factory.New<BillOfLading>();
			sailingBill.JS_HouseBill = "B1";
			var sailingContainer = sailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = "C2";
			var sailingPackLine = sailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_ItemNo = 1;
			sailingPackLine.JL_HarmonisedCode = "01011000";
			sailingPackLine.JL_ActualWeight = 1000m;
			sailingPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			sailingPackLine.JL_F3_NKPackType = "PLT";
			sailingPackLine.JL_ActualVolume = 15m;
			sailingPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			sailingPackLine.JL_PackageCount = 11;
			sailingPackLine.JL_DetailedDescription = "DESCRIPTION";
			sailingPackLine.JL_MarksAndNumbers = "MARKS AND NUMBERS";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "B1";
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "C1";
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "C2";
			var pack1 = bill.Packs.AddNew();
			pack1.ContainerPK = container2.PK;
			pack1.APA_LineNo = 0;
			pack1.APA_PackQty = 2;

			header.BillOfLadingForSync = sailingBill;

			var newPack = bill.Packs.AddNew();
			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingPackLine>)newPack;
			AssertNull("NULL when not matched", synchronisationTarget.Source);

			synchronisationTarget.Set(sailingPackLine);
			synchronisationTarget.Synchronise();

			CombineAssertions(() =>
			{
				AssertEquals("APA_LineNo", (ZShort)1, newPack.APA_LineNo);
				AssertEquals("APA_PackQty", 11, newPack.APA_PackQty);
				AssertEquals("APA_PackUQ", "PLT", newPack.APA_PackUQ);
				AssertEquals("APA_Weight", 1000m, newPack.APA_Weight);
				AssertEquals("APA_WeightUQ", Core.Constants.Weight.Grams, newPack.APA_WeightUQ);
				AssertEquals("APA_Volume", 15m, newPack.APA_Volume);
				AssertEquals("APA_VolumeUQ", Core.Constants.Volume.Litre, newPack.APA_VolumeUQ);
				AssertEquals("APA_GoodsDescription", "DESCRIPTION", newPack.APA_GoodsDescription);
				AssertEquals("APA_MarksAndNumbers", "MARKS AND NUMBERS", newPack.APA_MarksAndNumbers);

				AssertEquals("Locates matching container for pack", container2.PK, newPack.ContainerPK);
				AssertEquals("Existing pack is not affected", 2, pack1.APA_PackQty);
			});
		}

		public void TestSynchroniseFromSource()
		{
			// this mode of synchronise will be employed when updating an existing pack from a packline
			var sailingBill = Factory.New<BillOfLading>();
			sailingBill.JS_HouseBill = "B1";
			var sailingContainer = sailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = "C1";
			var sailingPackLine = sailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_ItemNo = 1;
			sailingPackLine.JL_HarmonisedCode = "01011000";
			sailingPackLine.JL_ActualWeight = 1000m;
			sailingPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			sailingPackLine.JL_F3_NKPackType = "PLT";
			sailingPackLine.JL_ActualVolume = 15m;
			sailingPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			sailingPackLine.JL_PackageCount = 11;
			sailingPackLine.JL_DetailedDescription = "DESCRIPTION";
			sailingPackLine.JL_MarksAndNumbers = "MARKS AND NUMBERS";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "B1";
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "C1";
			var pack1 = bill.Packs.AddNew();
			pack1.ContainerPK = container.PK;
			pack1.APA_LineNo = 0;
			pack1.APA_PackQty = 2;
			var pack2 = bill.Packs.AddNew();
			pack2.ContainerPK = container.PK;
			pack2.APA_LineNo = 1;

			header.BillOfLadingForSync = sailingBill;

			var synchronisationTarget = (ISailingSynchronisationTarget<BillOfLadingPackLine>)pack2;
			AssertSame("Finds sailingPackLine when matched", sailingPackLine, synchronisationTarget.Source);
			synchronisationTarget.Synchronise();

			CombineAssertions(() =>
			{
				AssertEquals("APA_LineNo", (ZShort)1, pack2.APA_LineNo);
				AssertEquals("APA_PackQty", 11, pack2.APA_PackQty);
				AssertEquals("APA_PackUQ", "PLT", pack2.APA_PackUQ);
				AssertEquals("APA_Weight", 1000m, pack2.APA_Weight);
				AssertEquals("APA_WeightUQ", Core.Constants.Weight.Grams, pack2.APA_WeightUQ);
				AssertEquals("APA_Volume", 15m, pack2.APA_Volume);
				AssertEquals("APA_VolumeUQ", Core.Constants.Volume.Litre, pack2.APA_VolumeUQ);
				AssertEquals("APA_GoodsDescription", "DESCRIPTION", pack2.APA_GoodsDescription);
				AssertEquals("APA_MarksAndNumbers", "MARKS AND NUMBERS", pack2.APA_MarksAndNumbers);

				AssertEquals("Locates matching container for pack", container.PK, pack2.ContainerPK);
				AssertEquals("Existing pack is not affected", 2, pack1.APA_PackQty);
			});
		}
	}
}
