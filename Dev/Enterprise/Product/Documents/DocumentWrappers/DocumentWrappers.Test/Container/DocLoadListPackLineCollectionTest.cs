using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocLoadListPackLineCollection))]
	internal class DocLoadListPackLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocLoadListPackLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LoadListPackLine line1 = new LoadListPackLine();
			return DocLoadListPackLine.New(line1, Factory);
		}

		protected override DocLoadListPackLineCollection GetCollectionToTest()
		{
			return new DocLoadListPackLineCollection(Factory);
		}

		public void TestGetTotalPackLineCollection()
		{
			AssertEquals(0, new DocLoadListPackLineCollection(null, null, Factory).Count);

			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIP1";
			var line1 = shipment1.OuterPackLines.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIP2";
			var line2 = shipment2.OuterPackLines.AddNew();

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line1);
			SetContainer(container1, line2);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			AssertEquals(2, new DocLoadListPackLineCollection(docConsol, packLines, Factory).Count);
		}

		public void TestGetPackLinesOnDifferentConsols()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			DocForwardingConsol docConsol1 = DocForwardingConsol.New(consol1, Factory);

			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIP1";
			var line1 = shipment1.OuterPackLines.AddNew();
			var shipment2 = consol1.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIP2";
			var line2 = shipment2.OuterPackLines.AddNew();

			CommonContainer container1 = CreateContainer(consol1);
			SetContainer(container1, line1);
			SetContainer(container1, line2);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			AssertEquals(2, new DocLoadListPackLineCollection(docConsol1, packLines, Factory).Count);

			var consol2 = Factory.New<ForwardingConsol>();
			DocForwardingConsol docConsol2 = DocForwardingConsol.New(consol2, Factory);

			consol2.Shipments.Add(shipment1);
			CommonContainer container2 = CreateContainer(consol2);
			SetContainer(container2, line1);

			packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));

			AssertEquals(1, new DocLoadListPackLineCollection(docConsol2, packLines, Factory).Count);
		}

		public void TestInterimReceiptSortOnPackLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_InterimReceipt = "RECEIPT2";
			shipment1.JS_UniqueConsignRef = "S1";
			var line1 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_InterimReceipt = "RECEIPT1";
			shipment2.JS_UniqueConsignRef = "S2";
			var line2 = shipment2.OuterPackLines.AddNew();

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line1);
			SetContainer(container1, line2);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals(2, result.Count);
			AssertEquals("RECEIPT1", result[0].Shipment.InterimReceipt);
			AssertEquals("S2", result[0].Shipment.ShipmentNumber);
			AssertEquals("RECEIPT2", result[1].Shipment.InterimReceipt);
		}

		public void TestShipmentNumberSortOnPackLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_InterimReceipt = "RECEIPT2";
			shipment1.JS_UniqueConsignRef = "S3";
			var line1 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_InterimReceipt = "RECEIPT2";
			shipment2.JS_UniqueConsignRef = "S2";
			var line2 = shipment2.OuterPackLines.AddNew();

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_InterimReceipt = "RECEIPT2";
			shipment3.JS_UniqueConsignRef = "S1";
			var line3 = shipment3.OuterPackLines.AddNew();

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line1);
			SetContainer(container1, line2);
			SetContainer(container1, line3);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));
			packLines.Add(DocPackLines.New(line3, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals(3, result.Count);
			AssertEquals("S1", result[0].Shipment.ShipmentNumber);
			AssertEquals("S2", result[1].Shipment.ShipmentNumber);
			AssertEquals("S3", result[2].Shipment.ShipmentNumber);
		}

		public void TestWeightVolumeOnGroupPackLine()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			PackLine line1 = shipment1.OuterPackLines.AddNew();
			PackLine line2 = shipment1.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 5M, "M3", 5, "BOX");

			PackLocation packLoc = line2.PackLocations.AddNew();
			packLoc.JQ_NoPackages = 5;
			packLoc.JQ_WarehouseLocation = "LOC 1";

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line1);
			SetContainer(container1, line2);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals("Number of PackLines in the Collection", 1, result.Count);
			AssertEquals("Total volume of packlines", 15M, result[0].Volume);
			AssertEquals("Total weight of packlines", 305M, result[0].Weight);
			AssertEquals("Group Pack Type", "Packages", result[0].PackType);
			AssertEquals("Cargo locations", "LOC 1,  Packs: 5 BOX", result[0].CargoLocationAndPacks);
			AssertContainsExactElementsInAnyOrder("Package Details",
				new string[] { "5 Box 5000 G 5 M3", "10 Pallet 300 KG 10 M3" },
				result[0].PackageDetails.ToString().Split('\n'));
			Assert("IsGroupPackLine", result[0].IsGroupPackLine);
		}

		public void TestWeightVolumeOnSeparatePackLine()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBL111";
			shipment1.JS_UniqueConsignRef = "SHIP1";
			shipment1.JS_InterimReceipt = "RCT1";
			var line1 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBL222";
			shipment2.JS_UniqueConsignRef = "SHIP2";
			shipment2.JS_InterimReceipt = "RCT2";
			var line2 = shipment2.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 5M, "M3", 5, "BOX");

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line1);
			SetContainer(container1, line2);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals("Number of PackLines in the Collection", 2, result.Count);
			AssertEquals("Volume on packline 1", 10M, result[0].Volume);
			AssertEquals("Weight on packline 1", 300M, result[0].Weight);
			AssertEquals("Package Details", "10 Pallet 300 KG 10 M3", result[0].PackageDetails);
			AssertEquals("Group Pack Type", "Pallet", result[0].PackType);
			AssertEquals("IsGroupPackLine", ZBool.False, result[0].IsGroupPackLine);
		}

		public void TestDimensionsOnPackLineCollection()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBL111";
			shipment1.JS_InterimReceipt = "RCT1";
			shipment1.JS_UniqueConsignRef = "SHIP1";
			var line1 = shipment1.OuterPackLines.AddNew();

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "HBL222";
			shipment2.JS_InterimReceipt = "RCT2";
			shipment2.JS_UniqueConsignRef = "SHIP2";
			var line2 = shipment2.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 60M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 80M, "M3", 5, "BOX");
			SetPackLineLengthWidhtHeight(line1, 10M, 2M, 3M, "M");
			SetPackLineLengthWidhtHeight(line2, 1M, 20M, 4M, "M");

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals("Number of PackLines in the Collection", 2, result.Count);
			AssertEquals("Package & Dimension Details for Pack1", "10 Pallet 300 KG 600 M3\n   (L): 10  (W): 2  (H): 3 M", result[0].PackageDetails);
			AssertEquals("Package & Dimension Details for Pack2", "5 Box 5000 G 400 M3\n   (L): 1  (W): 20  (H): 4 M", result[1].PackageDetails);
		}

		public void TestDimensionOneShipmentManyPackLines()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UnitOfWeight = Constants.Weight.Grams;
			shipment1.JS_HouseBill = "HBL111";
			shipment1.JS_UniqueConsignRef = "SHIP1";
			var line1 = shipment1.OuterPackLines.AddNew();
			var line2 = shipment1.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 80M, "M3", 5, "BOX");
			SetPackLineLengthWidhtHeight(line1, 10M, 0M, 0M, "M");
			SetPackLineLengthWidhtHeight(line2, 1M, 20M, 4M, "M");

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals("Number of PackLines in the Collection", 1, result.Count);

			ZString expected = "10 Pallet 300 KG 10 M3\n   (L): 10  (W): 0  (H): 0 M\n5 Box 5000 G 400 M3\n   (L): 1  (W): 20  (H): 4 M";
			AssertEquals("Package & Dimension Details", expected, result[0].PackageDetails);
		}

		public void TestDimensionOneShipmentWithDimensions()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HBL111";
			shipment1.JS_UniqueConsignRef = "SHIP1";
			var line1 = shipment1.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineLengthWidhtHeight(line1, 10M, 0M, 0M, "M");

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, Factory);
			AssertEquals("(L): 10  (W): 0  (H): 0 M", result[0].PackageDetailsAndHandlingInstructionNote);
		}

		public void TestSimpleSort()
		{
			DocLoadListPackLineCollection coll = new DocLoadListPackLineCollection(Factory);

			LoadListPackLine pack1 = new LoadListPackLine(CreateDocPackLine("INT1", "NO1"), null);
			DocLoadListPackLine docLoadListPack1 = DocLoadListPackLine.New(pack1, Factory);
			LoadListPackLine pack2 = new LoadListPackLine(CreateDocPackLine("INT2", "NO2"), null);
			DocLoadListPackLine docLoadListPack2 = DocLoadListPackLine.New(pack2, Factory);
			coll.Add(docLoadListPack1);
			coll.Add(docLoadListPack2);

			coll.SortOnShipmentInterimReceipt();
			AssertEquals("INT1", coll[0].Shipment.InterimReceipt);
			AssertEquals("INT2", coll[1].Shipment.InterimReceipt);

			LoadListPackLine pack3 = new LoadListPackLine(CreateDocPackLine("", "NO0"), null);
			DocLoadListPackLine docLoadListPack3 = DocLoadListPackLine.New(pack3, Factory);
			coll.Add(docLoadListPack3);

			coll.SortOnShipmentInterimReceipt();
			AssertEquals("", coll[0].Shipment.InterimReceipt);
			AssertEquals("INT1", coll[1].Shipment.InterimReceipt);
			AssertEquals("INT2", coll[2].Shipment.InterimReceipt);
		}

		public void TestShipmentNumberSort()
		{
			DocLoadListPackLineCollection coll = new DocLoadListPackLineCollection(Factory);

			LoadListPackLine pack1 = new LoadListPackLine(CreateDocPackLine("INT1", "NO2"), null);
			LoadListPackLine pack2 = new LoadListPackLine(CreateDocPackLine("INT1", "NO1"), null);
			DocLoadListPackLine docLoadListPack1 = DocLoadListPackLine.New(pack1, Factory);
			DocLoadListPackLine docLoadListPack2 = DocLoadListPackLine.New(pack2, Factory);
			coll.Add(docLoadListPack1);
			coll.Add(docLoadListPack2);

			coll.SortOnShipmentInterimReceipt();
			AssertEquals("NO1", coll[0].Shipment.ShipmentNumber);
			AssertEquals("NO2", coll[1].Shipment.ShipmentNumber);
		}

		public void TestPackLinesMerging()
		{
			AssertEquals(0, new DocLoadListPackLineCollection(null, null, Factory).Count);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIP1";
			PackLine line1 = shipment1.OuterPackLines.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIP2";
			PackLine line2 = shipment2.OuterPackLines.AddNew();
			PackLine line3 = shipment2.OuterPackLines.AddNew();

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line1);
			SetContainer(container1, line2);

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line1, Factory));
			packLines.Add(DocPackLines.New(line2, Factory));
			packLines.Add(DocPackLines.New(line3, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			AssertEquals(2, new DocLoadListPackLineCollection(docConsol, packLines, Factory).Count);
			AssertEquals(2, new DocLoadListPackLineCollection(docConsol, packLines, true, Factory).Count);
			AssertEquals(3, new DocLoadListPackLineCollection(docConsol, packLines, false, Factory).Count);
		}

		public void TestPackingOrderSort()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_InterimReceipt = "RECEIPT2";
			shipment1.JS_UniqueConsignRef = "S3";
			PackLine line11 = shipment1.OuterPackLines.AddNew();
			PackLine line12 = shipment1.OuterPackLines.AddNew();

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_InterimReceipt = "RECEIPT1";
			shipment2.JS_UniqueConsignRef = "S2";
			PackLine line21 = shipment2.OuterPackLines.AddNew();
			PackLine line22 = shipment2.OuterPackLines.AddNew();

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_InterimReceipt = "RECEIPT2";
			shipment3.JS_UniqueConsignRef = "S1";
			PackLine line31 = shipment3.OuterPackLines.AddNew();

			CommonContainer container1 = CreateContainer(consol);
			SetContainer(container1, line11);
			SetContainer(container1, line12);
			SetContainer(container1, line21);
			SetContainer(container1, line22);
			SetContainer(container1, line31);

			line11.JL_ContainerPackingOrder = 3;
			line12.JL_ContainerPackingOrder = 1;
			line21.JL_ContainerPackingOrder = 1;
			line22.JL_ContainerPackingOrder = 2;
			line31.JL_ContainerPackingOrder = 2;

			DocPackLinesCollection packLines = new DocPackLinesCollection(Factory);
			packLines.Add(DocPackLines.New(line11, Factory));
			packLines.Add(DocPackLines.New(line12, Factory));
			packLines.Add(DocPackLines.New(line21, Factory));
			packLines.Add(DocPackLines.New(line22, Factory));
			packLines.Add(DocPackLines.New(line31, Factory));

			DocForwardingConsol docConsol = DocForwardingConsol.New(consol, Factory);
			DocLoadListPackLineCollection result = new DocLoadListPackLineCollection(docConsol, packLines, false, Factory);
			result.SortOnPackingOrderShipmentInterimReceipt();
			AssertEquals(5, result.Count);
			AssertEquals("S2", result[0].Shipment.ShipmentNumber);
			AssertEquals(1, result[0].ContainerPackingOrder);
			AssertEquals("S3", result[1].Shipment.ShipmentNumber);
			AssertEquals(1, result[1].ContainerPackingOrder);
			AssertEquals("S2", result[2].Shipment.ShipmentNumber);
			AssertEquals(2, result[2].ContainerPackingOrder);
			AssertEquals("S1", result[3].Shipment.ShipmentNumber);
			AssertEquals(2, result[3].ContainerPackingOrder);
			AssertEquals("S3", result[4].Shipment.ShipmentNumber);
			AssertEquals(3, result[4].ContainerPackingOrder);
		}

		protected DocPackLines CreateDocPackLine(ZString shipmentInterimReceipt, ZString shipmentNumber)
		{
			var ship = Factory.New<ForwardingShipment>();
			ship.JS_InterimReceipt = shipmentInterimReceipt;
			ship.JS_UniqueConsignRef = shipmentNumber;
			var pack = ship.OuterPackLines.AddNew();
			return DocPackLines.New(pack, Factory);
		}

		#region Implementation

		protected CommonContainer CreateContainer(ForwardingConsol consol)
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container1.JC_Purpose = Core.Constants.ContainerPackingMode.Export;
			return container1;
		}

		protected void SetContainer(CommonContainer container, PackLine line)
		{
			var pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = line.PK;
			pivot1.J6_JC = container.PK;
		}

		protected void SetPackLineWeightVolume(PackLine line, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ, ZInt package, ZString type)
		{
			line.JL_ActualVolume = volume;
			line.JL_ActualVolumeUQ = volumeUQ;
			line.JL_ActualWeight = weight;
			line.JL_ActualWeightUQ = weightUQ;
			line.JL_PackageCount = package;
			line.JL_F3_NKPackType = type;
		}

		protected void SetPackLineLengthWidhtHeight(PackLine line, ZDecimal length, ZDecimal width, ZDecimal height, ZString unitOfDimension)
		{
			line.JL_Length = length;
			line.JL_Width = width;
			line.JL_Height = height;
			line.JL_UnitOfDimension = unitOfDimension;
		}

		#endregion
	}
}
