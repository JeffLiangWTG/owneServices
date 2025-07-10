using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ImportAction = Enterprise.Customs.Business.ImportAction;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderSailingSynchronisationTest : SailingSynchronisationTest
	{
		public void TestImportBillsOfLadingLinkedToTheSameSailing()
		{
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));

			var bills = header.Bills.Cast<AsycudaBill>().ToArray();

			var bill = bills.First(x => x.ABL_BillNumber == "AAA");
			AssertEquals("Should sync from the bill target.", 11, bill.ABL_ManifestQty);

			bill.ABL_ManifestQty = 12;

			bill = bills.First(x => x.ABL_BillNumber == "BBB");
			AssertEquals("Should sync from the bill target.", 3, bill.ABL_ManifestQty);

			bill.ABL_ManifestQty = 4;

			bill = bills.First(x => x.ABL_BillNumber == "CCC");
			AssertEquals("Should sync from the bill target.", 3, bill.ABL_ManifestQty);

			bill.ABL_ManifestQty = 5;

			bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "DDD";

			var billImportActions = new BillImportActionCollection(header.Bills);
			var billImportActionArray = billImportActions.Cast<BillImportAction>().ToArray();

			billImportActionArray.First(x => x.BillNumber == "AAA").IsSelected = true;
			billImportActionArray.First(x => x.BillNumber == "BBB").IsSelected = false;
			billImportActionArray.First(x => x.BillNumber == "CCC").IsSelected = true;
			billImportActionArray.First(x => x.BillNumber == "DDD").IsSelected = true;

			Factory.Save();
			header.ImportBillsOfLadingLinkedToTheSameSailing(billImportActions);

			bills = header.Bills.Cast<AsycudaBill>().ToArray();

			bill = bills.First(x => x.ABL_BillNumber == "AAA");
			AssertEquals("Should sync from the bill target.", 11, bill.ABL_ManifestQty);

			bill = bills.First(x => x.ABL_BillNumber == "BBB");
			AssertEquals("Should not sync from the bill target as it's not selected.", 4, bill.ABL_ManifestQty);

			bill = bills.First(x => x.ABL_BillNumber == "CCC");
			AssertEquals("Should sync from the bill target.", 3, bill.ABL_ManifestQty);

			bill = bills.FirstOrDefault(x => x.ABL_BillNumber == "DDD");
			AssertNull("There is no import target with DDD.", bill);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestImportBillsOfLadingLinkedToTheSameSailing_ExistingBillsAndContainers()
		{
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "bill1";
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "CN0001";
			var pack1 = bill1.Packs.AddNew();
			pack1.ContainerPK = container1.PK;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "bill2";
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "CN0002";
			var pack2 = bill2.Packs.AddNew();
			pack2.ContainerPK = container2.PK;

			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "bill3";
			var pack3 = bill3.Packs.AddNew();
			pack3.ContainerPK = container2.PK;

			var looseBill = header.Bills.AddNew();
			looseBill.ABL_BillNumber = "bill4";
			var looseContainer = header.Containers.AddNew();
			looseContainer.ACN_ContainerNumber = "CN0004";

			var billImportActions = new BillImportActionCollection(header.Bills);
			var billImportActionArray = billImportActions.Cast<BillImportAction>().ToArray();

			billImportActionArray.First(x => x.BillNumber == "bill1").IsSelected = true;
			billImportActionArray.First(x => x.BillNumber == "bill2").IsSelected = false;
			billImportActionArray.First(x => x.BillNumber == "bill3").IsSelected = true;
			billImportActionArray.First(x => x.BillNumber == "bill4").IsSelected = true;

			Factory.Save();
			header.ImportBillsOfLadingLinkedToTheSameSailing(billImportActions);

			// Bills
			var bills = header.Bills.Cast<AsycudaBill>().ToArray();
			AssertNull("bill1 is deleted", bills.FirstOrDefault(x => x.ABL_BillNumber == "bill1"));
			AssertNotNull("bill2 remains", bills.FirstOrDefault(x => x.ABL_BillNumber == "bill2"));
			AssertNull("bill3 is deleted", bills.FirstOrDefault(x => x.ABL_BillNumber == "bill3"));
			AssertNull("bill4 is deleted", bills.FirstOrDefault(x => x.ABL_BillNumber == "bill4"));
			AssertNotNull("Should create a new bill from FCLSailingBill", bills.FirstOrDefault(x => x.ABL_BillNumber == "AAA"));
			AssertNotNull("ROROSailingBill is synched", bills.FirstOrDefault(x => x.ABL_BillNumber == "BBB"));
			AssertNotNull("BulkSailingBill is synched", bills.FirstOrDefault(x => x.ABL_BillNumber == "CCC"));

			AssertEquals("Deletes selected bills", 4, bills.Length);

			// Containers
			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			AssertNull("Container on Bill1 is deleted", containers.FirstOrDefault(x => x.ACN_ContainerNumber == "CN0001"));
			AssertNotNull("Container on Bill2 remains", containers.FirstOrDefault(x => x.ACN_ContainerNumber == "CN0002"));
			AssertNull("Loose Container is deleted", containers.FirstOrDefault(x => x.ACN_ContainerNumber == "CN0004"));
			AssertNotNull("Should sync container from FCLSailingBill", containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111"));
			AssertEquals("Deletes containers on selected bills", 2, containers.Length);

			AssertEquals("Correctly reports number of Bills", 4, header.AMA_NoOfBills);
			AssertEquals("Correctly reports number of Containers", 2, header.AMA_NoOfContainers);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestImportBillsOfLadingLinkedToTheSameSailing_ReplaceExistingBillsAndContainers()
		{
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "AAA";
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "CN0001";
			var pack1 = bill1.Packs.AddNew();
			pack1.ContainerPK = container1.PK;

			var billImportActions = new BillImportActionCollection(header.Bills);
			var billImportActionArray = billImportActions.Cast<BillImportAction>().ToArray();

			var billAAAImportAction = billImportActionArray.First(x => x.BillNumber == "AAA");
			AssertEquals(ImportAction.Replace, billAAAImportAction.Action);
			billAAAImportAction.IsSelected = true;

			Factory.Save();
			header.ImportBillsOfLadingLinkedToTheSameSailing(billImportActions);

			var bills = header.Bills.Cast<AsycudaBill>().ToArray();
			var billAAA = bills.FirstOrDefault(x => x.ABL_BillNumber == "AAA");
			AssertNotNull("Should create a new bill from FCLSailingBill", billAAA);
			AssertNotNull("ROROSailingBill is synched", bills.FirstOrDefault(x => x.ABL_BillNumber == "BBB"));
			AssertNotNull("BulkSailingBill is synched", bills.FirstOrDefault(x => x.ABL_BillNumber == "CCC"));
			AssertEquals("Creates new bills", 3, bills.Length);

			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var containerABCD = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111");
			AssertNotNull("Should sync container from FCLSailingBill", containerABCD);
			AssertNull("Original Container on Bill is deleted", containers.FirstOrDefault(x => x.ACN_ContainerNumber == "CN0001"));
			AssertEquals("Has no other containers", 1, containers.Length);

			AssertEquals("Correctly reports number of Bills", 3, header.AMA_NoOfBills);
			AssertEquals("Correctly reports number of Containers", 1, header.AMA_NoOfContainers);

			Assert("Container is on Bill", billAAA.ContainersOnThisBill.Contains(containerABCD));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestImportBillsOfLadingLinkedToTheSameSailing_MultipleContainers()
		{
			sailingContainer.JC_ContainerNum = "AAAC0001";

			rOROSailingBill.JS_PackingMode = Core.Constants.ContainerModes.Containerised;
			var c2 = CreateSailingContainer(rOROSailingBill, "BBBC0001");
			CreatePackLine(fCLSailingBill, c2, 8);

			bulkSailingBill.JS_PackingMode = Core.Constants.ContainerModes.Combination;
			var c3 = CreateSailingContainer(bulkSailingBill, "CCCC0001");
			CreatePackLine(bulkSailingBill, c3, 6);

			Factory.Save();
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));

			// Bills
			var bills = header.Bills.Cast<AsycudaBill>().ToArray();
			var billA = bills.FirstOrDefault(x => x.ABL_BillNumber == "AAA");
			AssertEquals("Should sync qty from FCLSailingBill  PackLines", 11, billA?.ABL_ManifestQty);
			var billB = bills.FirstOrDefault(x => x.ABL_BillNumber == "BBB");
			AssertEquals("Should sync qty from ROROSailingBill PackLines", 8, billB?.ABL_ManifestQty);
			var billC = bills.FirstOrDefault(x => x.ABL_BillNumber == "CCC");
			AssertEquals("Should sync qty from BulkSailingBill PackLines", 6, billC?.ABL_ManifestQty);
			AssertEquals(3, bills.Length);

			// Containers
			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var containerA = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "AAAC0001");
			AssertNotNull("Should sync container from FCLSailingBill", containerA);
			var containerB = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "BBBC0001");
			AssertNotNull("Should sync container from ROROSailingBill", containerB);
			var containerC = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "CCCC0001");
			AssertNotNull("Should sync container1 from BulkSailingBill", containerC);

			AssertEquals("Correctly reports number of Bills", 3, header.AMA_NoOfBills);
			AssertEquals("Correctly reports number of Containers", 3, header.AMA_NoOfContainers);

			// Pack Lines
			var packA = billA.Packs.Cast<AsycudaPack>().FirstOrDefault();
			AssertEquals("Should sync qty from FCLSailingBill PackLines", 11, packA?.APA_PackQty);
			var packB = billB.Packs.Cast<AsycudaPack>().FirstOrDefault();
			AssertEquals("Should sync qty from ROROSailingBill PackLines", 8, packB?.APA_PackQty);
			var packC = billC.Packs.Cast<AsycudaPack>().FirstOrDefault();
			AssertEquals("Should sync qty from ROROSailingBill PackLines", 6, packC?.APA_PackQty);

			AssertEquals("BillA has one packline to its container", 1, billA.Packs.Count);
			AssertEquals("BillB has one packline to its container", 1, billB.Packs.Count);
			AssertEquals("BillC has one packline to its container", 1, billC.Packs.Count);

			Assert("ContainerA is on BillA", billA.ContainersOnThisBill.Contains(containerA));
			Assert("ContainerB is on BillB", billB.ContainersOnThisBill.Contains(containerB));
			Assert("ContainerC is on BillC", billC.ContainersOnThisBill.Contains(containerC));
			AssertEquals("BillA has one container", 1, billA.ContainersOnThisBill.Count());
			AssertEquals("BillB has one container", 1, billB.ContainersOnThisBill.Count());
			AssertEquals("BillC has one container", 1, billC.ContainersOnThisBill.Count());
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestImportBillsOfLadingLinkedToTheSameSailing_ContainersWithoutPackLines()
		{
			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "2222";
			var sailingContainer2 = fCLSailingBill.RealContainers.AddNew();
			sailingContainer2.JC_ContainerNum = "ABCD2222";
			sailingContainer2.JC_RC = refContainer2.PK;
			sailingContainer2.JC_IsEmptyContainer = true;

			var refContainer3 = Factory.New<RefContainer>();
			refContainer3.RC_Code = "3333";
			var sailingContainer3 = fCLSailingBill.RealContainers.AddNew();
			sailingContainer3.JC_ContainerNum = "ABCD3333";
			sailingContainer3.JC_RC = refContainer3.PK;
			sailingContainer3.JC_IsEmptyContainer = false;

			Factory.Save();
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));

			var bills = header.Bills.Cast<AsycudaBill>().ToArray();
			var bill = bills.FirstOrDefault(x => x.ABL_BillNumber == "AAA");

			AssertEquals("Creates containers to match all the sailing containers", 3, header.Containers.Count);
			AssertEquals("Creates packs to match the sailing packlines and create empty packlines for contianers without packlines", 3, bill.Packs.Count);

			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var container1 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111");
			var container2 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD2222");
			var container3 = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD3333");

			var billPacks = bill.Packs.Cast<AsycudaPack>().OrderBy(p => p.APA_LineNo).ToArray();
			AssertEquals("Pack1 is mapped to container1", container1.PK, billPacks[0].ContainerPK);
			AssertEquals("Pack2 is mapped to container2", container2.PK, billPacks[1].ContainerPK);
			AssertEquals("Pack3 is mapped to container3", container3.PK, billPacks[2].ContainerPK);
		}

		public void TestImportBillsOfLadingLinkedToTheSameSailing_MultiplePackLines()
		{
			var sailingPackLine = CreatePackLine(fCLSailingBill, sailingContainer, 4);
			sailingPackLine.JL_ItemNo = 1;

			Factory.Save();
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));

			var bills = header.Bills.Cast<AsycudaBill>().ToArray();
			var billA = bills.FirstOrDefault(x => x.ABL_BillNumber == "AAA");
			AssertEquals("Should sum qty from FCLSailingBill PackLines", 15, billA?.ABL_ManifestQty);
			AssertNotNull("ROROSailingBill is synched", bills.FirstOrDefault(x => x.ABL_BillNumber == "BBB"));
			AssertNotNull("BulkSailingBill is synched", bills.FirstOrDefault(x => x.ABL_BillNumber == "CCC"));
			AssertEquals(3, bills.Length);

			var containers = header.Containers.Cast<AsycudaContainer>().ToArray();
			var container = containers.FirstOrDefault(x => x.ACN_ContainerNumber == "ABCD1111");
			AssertNotNull("Should sync container from FCLSailingBill", container);
			AssertEquals("Has 1 container", 1, containers.Length);

			var billPacks = billA.Packs.Cast<AsycudaPack>().OrderBy(p => p.APA_LineNo).ToArray();
			AssertEquals("Bill has two packlines", 2, billPacks.Length);

			AssertEquals("Should sync qty from PackLine 1", 11, billPacks[0].APA_PackQty);
			AssertEquals("Should sync qty from PackLine 2", 4, billPacks[1].APA_PackQty);
			AssertEquals("PackLine1 is linked to container", container.PK, billPacks[0].ContainerPK);
			AssertEquals("PackLine2 is linked to container", container.PK, billPacks[1].ContainerPK);
			Assert("Container is on Bill", billA.ContainersOnThisBill.Contains(container));
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestCanChangeSailing()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			Assert("PreCondition", !header.Messages.Any());
			Assert("Should be true as there is no message.", header.CanChangeSailing);

			header.Messages.AddNew();
			Assert("Should be false as there is at least one message.", !header.CanChangeSailing);
		}

		public void TestAMA_MasterBill_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Default to false.", false, header.AMA_MasterBillInfo.ReadOnly);
		}

		BillOfLadingContainer CreateSailingContainer(BillOfLading sailingBill, ZString containerNum)
		{
			var sailingContainer = sailingBill.RealContainers.AddNew();
			sailingContainer.JC_ContainerNum = containerNum;
			sailingContainer.JC_RC = rContainer.PK;
			sailingContainer.JC_SealNum = "123";
			sailingContainer.JC_AdditionalSealNum = "456";
			sailingContainer.JC_Additional2SealNum = "789";
			sailingContainer.JC_StowagePosition = "Darnassus";
			sailingContainer.JC_RH_NKContainerCommodityCode = "ABC";
			sailingContainer.JC_GrossWeight = 982m;
			sailingContainer.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			return sailingContainer;
		}

		BillOfLadingPackLine CreatePackLine(BillOfLading sailingBill, BillOfLadingContainer sailingContainer, ZInt packageCount)
		{
			var sailingPackLine = sailingBill.OuterPackLines.AddNew();
			sailingContainer.PackLines.Add(sailingPackLine);
			sailingPackLine.JL_HarmonisedCode = "01011000";
			sailingPackLine.JL_ActualWeight = 1000m;
			sailingPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			sailingPackLine.JL_F3_NKPackType = "PLT";
			sailingPackLine.JL_ActualVolume = 15m;
			sailingPackLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			sailingPackLine.JL_PackageCount = packageCount;
			sailingPackLine.JL_DetailedDescription = "DESCRIPTION";
			sailingPackLine.JL_MarksAndNumbers = "MARKS AND NUMBERS";
			sailingPackLine.JL_ItemNo = 0;
			return sailingPackLine;
		}
	}
}
