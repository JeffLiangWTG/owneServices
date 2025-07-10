using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExportAWBRateLine = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class FBDNRecordTest : JXCRecordTestCase
	{
		public void TestUpdateShipment_GoodsDescription()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_GoodsDescription = "HAHAH";
			FBDNRecord record = (FBDNRecord)RecordFactory.NewRecord("FBDN3100;0010;;0116.00;K;Q;;;;0000116;00022.00;000002552.00;CONSOL SHIPMENT;This is the description;;;");
			record.UpdateShipment(shipment);
			AssertEquals("Should be unchanged", "HAHAH", shipment.JS_GoodsDescription);
			shipment.JS_GoodsDescription = "";
			record.UpdateShipment(shipment);
			AssertEquals("Should be changed", "This is the description", shipment.JS_GoodsDescription);
			shipment.JS_GoodsDescription = "";
			record = (FBDNRecord)RecordFactory.NewRecord("FBDN3100;0010;;0116.00;K;Q;;;;0000116;00022.00;000002552.00;CONSOL SHIPMENT;" + new ZString('+', 2000) + ";;;");
			record.UpdateShipment(shipment);
			AssertEquals("Should be trimmed", new ZString('+', JobShipmentSchema.JS_GoodsDescription.MaxLength), shipment.JS_GoodsDescription);
		}

		public void TestUpdateShipment_OuterPackType()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Mix;
			FBDNRecord record = (FBDNRecord)RecordFactory.NewRecord("FBDN3100;0010;PAL;0116.00;K;Q;;;;0000116;00022.00;000002552.00;CONSOL SHIPMENT;This is the description;;;");
			record.UpdateShipment(shipment);
			AssertEquals("Should be unchanged", Core.Constants.PkgUnit.Mix, shipment.JS_F3_NKPackType);
			shipment.JS_F3_NKPackType = "";
			record.UpdateShipment(shipment);
			AssertEquals("Should be changed", Core.Constants.PkgUnit.Pail, shipment.JS_F3_NKPackType);
			shipment.JS_F3_NKPackType = FreightPacksDataRegistry.Instance.OuterPackUnit.Value;
			record.UpdateShipment(shipment);
			AssertEquals("Should be changed", Core.Constants.PkgUnit.Pail, shipment.JS_F3_NKPackType);
		}

		[ExpectNoExceptions]
		public void TestUpdateAWBHeader_NullParam()
		{
			FBDNRecord record = (FBDNRecord)RecordFactory.NewRecord("FBDN3100;0010;;0116.00;K;Q;;;;0000116;00022.00;000002552.00;CONSOL SHIPMENT;CONSOL SHIPMENT;;;");
			record.UpdateAWBHeader(null);
		}

		public void TestUpdateAWBHeader()
		{
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			FBDNRecord record = (FBDNRecord)RecordFactory.NewRecord("FBDN3100;0010;;0116.00;K;Q;1234;;;0000116;00022.00;000002552.00;CONSOL SHIPMENT;CONSOL SHIPMENT;;;");
			aWBHeader.AWBRateLines[0].Clear();
			aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "";
			record.UpdateAWBHeader(aWBHeader);
			AssertEquals("10", aWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
			AssertEquals(116m, aWBHeader.AWBRateLines[0].ER_GrossWeight);
			AssertEquals("K", aWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
			AssertEquals("Q", aWBHeader.AWBRateLines[0].ER_RateClass);
			AssertEquals("1234", aWBHeader.AWBRateLines[0].ER_CommodityItemNumber);
			AssertEquals(116m, aWBHeader.AWBRateLines[0].ER_ChargeableWeight);
			AssertEquals(22m, aWBHeader.AWBRateLines[0].ER_RateChargeOrDiscount);
			AssertEquals(2552m, aWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals("CONSOL SHIPMENT", aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestUpdateShipment_ExcessivelyLongStringIsTrimmed()
		{
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			FBDNRecord record = (FBDNRecord)RecordFactory.NewRecord("FBDN3100;0010;;0116.00;Kilograms;Qsomethingorrather;123456789019283;;;0000116;00022.00;000002552.00;" + new ZString('9', 500) + ";CONSOL SHIPMENT;;;");
			aWBHeader.AWBRateLines[0].Clear();
			aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "";
			record.UpdateAWBHeader(aWBHeader);
			AssertEquals("10", aWBHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP);
			AssertEquals(116m, aWBHeader.AWBRateLines[0].ER_GrossWeight);
			AssertEquals("K", aWBHeader.AWBRateLines[0].ER_WeightInLBsOrKGs);
			AssertEquals("Q", aWBHeader.AWBRateLines[0].ER_RateClass);
			AssertEquals("1234567", aWBHeader.AWBRateLines[0].ER_CommodityItemNumber);
			AssertEquals(116m, aWBHeader.AWBRateLines[0].ER_ChargeableWeight);
			AssertEquals(22m, aWBHeader.AWBRateLines[0].ER_RateChargeOrDiscount);
			AssertEquals(2552m, aWBHeader.AWBRateLines[0].ER_Total);
			AssertEquals(new ZString('9', ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength), aWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription);
		}

		public void TestGetFirstEmptyRateLine()
		{
			FBDNRecord record = new FBDNRecord("", "");
			ConsolExportAWBHeader aWBHeader = Factory.New<ConsolExportAWBHeader>();
			foreach (ExportAWBRateLine rateLine in aWBHeader.AWBRateLines)
			{
				rateLine.Clear();
				rateLine.NatureAndQtyOfGoods.Text = "nature and qty of goods!";
			}

			for (int i = 0; i < aWBHeader.AWBRateLines.Count; i++)
			{
				ExportAWBRateLine expectedRateLine = aWBHeader.AWBRateLines[i];
				AssertEquals(expectedRateLine.PK, record.GetFirstEmptyRateLine(aWBHeader).PK);
				expectedRateLine.ER_NoOfPiecesOrRCP = "1";
			}
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new FBDNRecord(lineType, lineContent);
		}
	}
}
