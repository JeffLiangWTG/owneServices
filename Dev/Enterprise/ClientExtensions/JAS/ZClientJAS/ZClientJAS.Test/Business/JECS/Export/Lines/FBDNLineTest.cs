using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Registry.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class FBDNLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			SetupConsolAWBHeaderForTest();
			Line = new FBDNLine(ConsolAWBHeader, ConsolAWBHeader.AWBRateLines[0]);
			AssertEquals("FBDN3100;20;PLT;20.4;L;M;AAABBBB;;;20.5;117;201.99;HAHA;Consolidation as per attached list;;;", Line.LineAsString);
			SetupShipmentAWBHeaderForTest();
			Line = new FBDNLine(ShipmentAWBHeader, ShipmentAWBHeader.AWBRateLines[0]);
			AssertEquals("FBDN3100;11;PAL;100;K;Q;HAHAHA;;;111;90.39;888;yadayadayada;DESCRIPTOIN;;;", Line.LineAsString);
		}

		public void TestNatureAndQtyOfGoodsMaxLength()
		{
			Assert("NatureAndQtyOfGoods field needs to be trimmed", ExportAWBRateLine.Schema.ER_NatureAndQtyOfGoodsMaxLength < JXCConstants.FBDNFieldBoundaries.NatureAndQtyOfGoodsMaxLength);
		}

		public void TestLineAsString_FieldsExceedMaxLength()
		{
			SetupShipmentAWBHeaderForTest();
			ShipmentAWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "0123456789012345678901234567890123456789";
			Line = new FBDNLine(ShipmentAWBHeader, ShipmentAWBHeader.AWBRateLines[0]);
			AssertEquals("FBDN3100;11;PAL;100;K;Q;HAHAHA;;;111;90.39;888;yadayadayada;01234567890123456789012345678901234;;;", Line.LineAsString);
		}

		public void TestNatureAndQtyOfGoods()
		{
			for (int i = 1; i < ConsolAWBHeader.AWBRateLines.Count; i++)
			{
				FBDNLine line = new FBDNLine(ConsolAWBHeader, ConsolAWBHeader.AWBRateLines[i]);
				ConsolAWBHeader.AWBRateLines[i].NatureAndQtyOfGoods.Text = i.ToString();
				AssertEquals(i.ToString(), line.NatureAndQtyOfGoods);
			}
		}

		public void TestTypeOfPieces_Shipment()
		{
			AssertTypeOfPieces(Constants.PkgUnit.Basket, "BSK");
			AssertTypeOfPieces(Constants.PkgUnit.Box, "BOX");
			AssertTypeOfPieces(Constants.PkgUnit.Case, "CAS");
			AssertTypeOfPieces(Constants.PkgUnit.Carton, "CTN");
			AssertTypeOfPieces(Constants.PkgUnit.Container, "CBC");
			AssertTypeOfPieces(Constants.PkgUnit.Crate, "CRT");
			AssertTypeOfPieces(Constants.PkgUnit.Cylinder, "CYL");
			AssertTypeOfPieces(Constants.PkgUnit.Drum, "DRM");
			AssertTypeOfPieces(Constants.PkgUnit.Keg, "KEG");
			AssertTypeOfPieces(Constants.PkgUnit.Package, "PKG");
			AssertTypeOfPieces(Constants.PkgUnit.Pail, "PAL");
			AssertTypeOfPieces(Constants.PkgUnit.Pallet, "PLT");
			AssertTypeOfPieces(Constants.PkgUnit.Piece, "PCS");
			AssertTypeOfPieces(Constants.PkgUnit.Reel, "REL");
			AssertTypeOfPieces(Constants.PkgUnit.Roll, "ROL");
			AssertTypeOfPieces(Constants.PkgUnit.Sheet, "SHT");
			AssertTypeOfPieces(Constants.PkgUnit.Skid, "SKD");
			AssertTypeOfPieces(Constants.PkgUnit.Unit, "UNT");
			AssertTypeOfPieces(Constants.PkgUnit.BaleCompressed, "BLE");
			AssertTypeOfPieces(Constants.PkgUnit.Bag, "BAG");
		}

		public void TestTypeOfPieces_Consol()
		{
			FBDNLine line = new FBDNLine(ConsolAWBHeader, ConsolAWBHeader.AWBRateLines[0]);
			AssertEquals(FreightPacksDataRegistry.Instance.OuterPackUnit.Value, line.TypeOfPieces);
		}

		#region Implementation
		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.FBDNFieldCount;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.FBDN;
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new FBDNLine(ConsolAWBHeader, ConsolAWBHeader.AWBRateLines[0]);
		}

		void AssertTypeOfPieces(ZString packageUnit, ZString expectedJASPackageUnit)
		{
			ShipmentAWBHeader.Shipment.JS_F3_NKPackType = packageUnit;
			FBDNLine line = new FBDNLine(ShipmentAWBHeader, ShipmentAWBHeader.AWBRateLines[0]);
			AssertEquals(expectedJASPackageUnit, line.TypeOfPieces);
		}

		ConsolExportAWBHeader ConsolAWBHeader
		{
			get
			{
				if (fConsolAWBHeader == null)
				{
					JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
					consol.JK_TransportMode = Constants.TransportModes.Air;
					fConsolAWBHeader = (ConsolExportAWBHeader)consol.AWBHeader;
				}

				return fConsolAWBHeader;
			}
		}

		ShipmentExportAWBHeader ShipmentAWBHeader
		{
			get
			{
				if (fShipmentAWBHeader == null)
				{
					JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
					shipment.JS_TransportMode = Constants.TransportModes.Air;
					fShipmentAWBHeader = (ShipmentExportAWBHeader)shipment.AWBHeader;
				}

				return fShipmentAWBHeader;
			}
		}

		void SetupConsolAWBHeaderForTest()
		{
			var aWBRateLineMock = Factory.NewMoq<ExportAWBRateLine>();
			var rateLine = aWBRateLineMock.Object;
			ConsolAWBHeader.AWBRateLines.RemoveAll();
			ConsolAWBHeader.AWBRateLines.Add(rateLine);
			rateLine.ER_NoOfPiecesOrRCP = "20";
			rateLine.ER_GrossWeight = 20.4m;
			rateLine.ER_WeightInLBsOrKGs = "L";
			rateLine.ER_RateClass = "M";
			rateLine.ER_CommodityItemNumber = "AAABBBB";
			rateLine.ER_ChargeableWeight = 20.5m;
			aWBRateLineMock.Setup(m => m.ER_RateChargeOrDiscount).Returns((ZDecimal)117);
			aWBRateLineMock.Setup(m => m.ER_Total).Returns((ZDecimal)201.99m);
			rateLine.ER_LineCount = 1;
			ConsolAWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "HAHA";
			ConsolAWBHeader.Consol.JK_AgentType = Core.Constants.AgentType.Agent;
		}

		void SetupShipmentAWBHeaderForTest()
		{
			var aWBRateLineMock = Factory.NewMoq<ExportAWBRateLine>();
			var rateLine = aWBRateLineMock.Object;
			ShipmentAWBHeader.AWBRateLines.RemoveAll();
			ShipmentAWBHeader.AWBRateLines.Add(rateLine);
			rateLine.ER_NoOfPiecesOrRCP = "11";
			rateLine.ER_GrossWeight = 100;
			rateLine.ER_WeightInLBsOrKGs = "K";
			rateLine.ER_RateClass = "Q";
			rateLine.ER_CommodityItemNumber = "HAHAHA";
			rateLine.ER_ChargeableWeight = 111;
			aWBRateLineMock.Setup(m => m.ER_RateChargeOrDiscount).Returns((ZDecimal)90.39m);
			aWBRateLineMock.Setup(m => m.ER_Total).Returns((ZDecimal)888);
			rateLine.ER_LineCount = 2;
			ShipmentAWBHeader.AWBRateLine1.NatureAndQtyOfGoods.Text = "yadayadayada";
			ShipmentAWBHeader.Shipment.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			ShipmentAWBHeader.Shipment.DetailedGoodsDescriptionNoteText = "DESCRIPTOIN";
		}

		ConsolExportAWBHeader fConsolAWBHeader;
		ShipmentExportAWBHeader fShipmentAWBHeader;
		#endregion
	}
}
