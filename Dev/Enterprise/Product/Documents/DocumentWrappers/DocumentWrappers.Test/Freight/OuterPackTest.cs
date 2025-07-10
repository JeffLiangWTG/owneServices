using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(OuterPack))]
	sealed class OuterPackTest : NonPersistentBusinessObjectTestCase
	{
		#region Properties

		public void TestConsolOrigin()
		{
			AssertEquals("ConsolOrigin", "", OuterPack.ConsolOrigin);

			ExportAWBHeader.EH_AWBOriginCode = "ZZZ";
			AssertEquals("ConsolOrigin", "ZZZ", OuterPack.ConsolOrigin);
		}

		public void TestConsolDestination()
		{
			AssertEquals("ConsolDestination", "", OuterPack.ConsolOrigin);

			ExportAWBHeader.EH_AirportOfDestinationCode = "ZZZ";
			AssertEquals("ConsolDestination", "ZZZ", OuterPack.ConsolDestination);
		}

		public void TestConsolPieceNumber()
		{
			AssertEquals("ConsolPieceNumber", "0", OuterPack.ConsolPieceNumber);

			OuterPack.NumberInConsol = 32;
			AssertEquals("ConsolPieceNumber", "32", OuterPack.ConsolPieceNumber);
		}

		public void TestConsolPieceCount()
		{
			DocAWB docAWB = DocAWB.New(Shipment.AWBHeader, Factory);
			OuterPack = new OuterPack();
			OuterPack.SetupOptionalInformation(docAWB);

			AssertEquals("ConsolPieceCount", "0", OuterPack.ConsolPieceCount);

			OuterPack.ConsolTotalPieceCount = 32;
			AssertEquals("ConsolPieceCount", "32", OuterPack.ConsolPieceCount);
		}

		public void TestConsolWeight()
		{
			AssertEquals("ConsolWeight", "0", OuterPack.ConsolWeight);

			Shipment.JS_ActualWeight = 100;
			Consol.Shipments.AddNew().JS_ActualWeight = 200;
			Consol.Shipments.AddNew().JS_ActualWeight = 50;

			AssertEquals("ConsolWeight", "350", OuterPack.ConsolWeight);
		}

		public void TestShipmentNumber()
		{
			AssertEquals("ShipmentNumber", "", OuterPack.HousebillNumber);

			Shipment.JS_HouseBill = "12345678";
			AssertEquals("ShipmentNumber", "12345678", OuterPack.HousebillNumber);
		}

		public void TestShipmentPieceNumber()
		{
			AssertEquals("ShipmentPieceNumber", "0", OuterPack.ShipmentPieceNumber);

			OuterPack.DocShipment.SequenceNumber = 32;
			AssertEquals("ShipmentPieceNumber", "32", OuterPack.ShipmentPieceNumber);
		}

		public void TestShipmentPieceCount()
		{
			AssertEquals("ShipmentPieceCount", "0", OuterPack.ShipmentPieceCount);

			Shipment.JS_OuterPacks = 32;
			AssertEquals("ShipmentTotalPieceCount", 0, OuterPack.ShipmentTotalPieceCount);
			AssertEquals("ShipmentPieceCount", "32", OuterPack.ShipmentPieceCount);
		}

		public void TestShipmentPieceCountOverride()
		{
			AssertEquals("ShipmentPieceCount", "0", OuterPack.ShipmentPieceCount);

			Shipment.JS_OuterPacks = 32;
			OuterPack.ShipmentTotalPieceCount = 69;
			AssertEquals("ShipmentPieceCount", "69", OuterPack.ShipmentPieceCount);
		}

		public void TestShipmentWeight()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			AssertEquals("ShipmentWeight", "0", OuterPack.ShipmentWeight);

			Shipment.JS_ActualWeight = 32;
			AssertEquals("ShipmentWeight", "32", OuterPack.ShipmentWeight);
		}

		public void TestShipmentHandlingInformation()
		{
			AssertEquals("ShipmentHandlingInformation", "", OuterPack.ShipmentHandlingInformation);

			var note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteDataAsText = "this is text";
			note.ST_ParentID = Shipment.PK;
			note.ST_Table = Shipment.TableName;
			Factory.Save();

			AssertEquals("ShipmentHandlingInformation", "this is text", OuterPack.ShipmentHandlingInformation);
		}

		public void TestIssuingCompanyName()
		{
			AssertEquals("IssuingCompanyName", GlbBranch.CurrentBranch.OrgProxy.OH_FullName, OuterPack.IssuingCompanyName);

			GlbBranch.CurrentBranch.OrgProxy.OH_FullName = "";
			AssertEquals("IssuingCompanyName", GlbCompany.CurrentCompany.OrgProxy.OH_FullName, OuterPack.IssuingCompanyName);
		}

		#endregion

		#region Barcode

		public void TestAppendTextToEncode()
		{
			ZString textToEncode = "";
			ZString appendix = "appendix";
			OuterPack.AppendTextToEncode(textToEncode, appendix);
			AssertEquals("New text with no delimiter", appendix, OuterPack.AppendTextToEncode(textToEncode, appendix));

			textToEncode = "source";
			appendix = "appendix";
			OuterPack.AppendTextToEncode(textToEncode, appendix);
			AssertEquals("New text inserted", textToEncode + "+" + appendix, OuterPack.AppendTextToEncode(textToEncode, appendix));

			textToEncode = "source";
			appendix = "";
			OuterPack.AppendTextToEncode(textToEncode, appendix);
			AssertEquals("No delimiter if appendix is empty", textToEncode, OuterPack.AppendTextToEncode(textToEncode, appendix));
		}

		public void TestEncodePrimaryBarcode()
		{
			ZString textToEncode = "SAMPLE";
			TextBarcode barcode = new TextBarcode(textToEncode);
			OuterPack.EncodePrimaryBarcode(textToEncode);

			AssertEquals("Encoded barcoded as128 font", barcode.TextAs128sFontString, OuterPack.PrimaryBarcode);
			AssertEquals("Encoded barcoded text", barcode.TextToEncode, OuterPack.PrimaryBarcodeDisplayText);
		}

		public void TestEncodeSecondaryBarcode()
		{
			ZString textToEncode = "SAMPLE";
			TextBarcode barcode = new TextBarcode(textToEncode);
			OuterPack.EncodeSecondaryBarcode(textToEncode);

			AssertEquals("Encoded barcoded as128 font", barcode.TextAs128sFontString, OuterPack.SecondaryBarcode);
			AssertEquals("Encoded barcoded text", barcode.TextToEncode, OuterPack.SecondaryBarcodeDisplayText);
		}

		#endregion

		#region Optional Information Properties Matching

		public void TestOpitonalInformationMatching()
		{
			OuterPackForTest outerPackForTest = new OuterPackForTest();

			for (int index = 1; index <= 6; index++)
			{
				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ConsolOrigin);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "ORI", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ConsolDestination);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "DES", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ConsolPieceCount);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "10", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ConsolPieceNumber);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "5", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ConsolPieceNumberOfCount);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("ConsolTotalPieceCount", 0, outerPackForTest.ConsolTotalPieceCount);
				AssertEquals("Optional Information", "5 of ", OptionalInformationProperty(outerPackForTest, index));

				outerPackForTest.ConsolTotalPieceCount = 10;
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "5 of 10", OptionalInformationProperty(outerPackForTest, index));
				outerPackForTest.ConsolTotalPieceCount = 0;

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ConsolWeight);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "240", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.IssuingCompanyName);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "IssuingCompanyName", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ShipmentHandlingInformation);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "ShipmentHandlingInformation", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.HousebillNumber);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "HBL123456", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ShipmentPieceCount);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "7", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ShipmentPieceNumber);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "2", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ShipmentPieceNumberOfCount);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "2 of 7", OptionalInformationProperty(outerPackForTest, index));

				SetupRegistry(index, AWBLabelOptionalInformationList.Codes.ShipmentWeight);
				outerPackForTest.SetupOptionalInformation(DocAWB.New(ExportAWBHeader, Factory));
				AssertEquals("Optional Information", "100", OptionalInformationProperty(outerPackForTest, index));
			}
		}

		public void TestSecondaryBarcodeMatching()
		{
			AWBLabelCustomisation awbLabel = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design1;
			awbLabel.BarcodeConsolDestination = true;
			awbLabel.BarcodeConsolOrigin = true;
			awbLabel.BarcodeConsolPieceCount = true;
			awbLabel.BarcodeConsolWeight = true;
			awbLabel.BarcodeHousebillNumber = true;
			awbLabel.BarcodeShipmentPieceCount = true;
			awbLabel.BarcodeShipmentPieceNumber = true;
			awbLabel.BarcodeShipmentWeight = true;
			awbLabel.BarcodeShipmentHandlingInformation = true;
			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Pounds;

			OuterPackForTest outerPackForTest1 = new OuterPackForTest("go handle yourself you handling handler");
			outerPackForTest1.DocShipment = DocShipment.New(shipment, Factory);
			outerPackForTest1.SetupOptionalInformation(DocAWB.New(consol.AWBHeader, Factory));

			var fullyPopulatedBarcodeText = "HHBL123456+S0007+Y0002+A0000100L+OORI+DDES+P0010+T0000240L+Bgo handle yourself you handling handle";
			AssertEquals("Secondary Barcode", fullyPopulatedBarcodeText, outerPackForTest1.SecondaryBarcodeDisplayText);

			OuterPackForTest outerPackForTest2 = new OuterPackForTest("go handle yourself");
			outerPackForTest2.DocShipment = DocShipment.New(shipment, Factory);
			outerPackForTest2.SetupOptionalInformation(DocAWB.New(consol.AWBHeader, Factory));

			var fullyPopulatedBarcodeText2 = "HHBL123456+S0007+Y0002+A0000100L+OORI+DDES+P0010+T0000240L+Bgo handle yourself";
			AssertEquals("Secondary Barcode", fullyPopulatedBarcodeText2, outerPackForTest2.SecondaryBarcodeDisplayText);
		}

		void SetupRegistry(int optionalInfoIndex, ZString selectedCode)
		{
			AWBLabelCustomisation awbLabel = FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			awbLabel.CustomDesign = AWBLabelCustomDesignList.Codes.Design1;
			switch (optionalInfoIndex)
			{
				case 1:
					awbLabel.OptionalInformation1 = selectedCode;
					break;
				case 2:
					awbLabel.OptionalInformation2 = selectedCode;
					break;
				case 3:
					awbLabel.OptionalInformation3 = selectedCode;
					break;
				case 4:
					awbLabel.OptionalInformation4 = selectedCode;
					break;
				case 5:
					awbLabel.OptionalInformation5 = selectedCode;
					break;
				case 6:
					awbLabel.OptionalInformation6 = selectedCode;
					break;
				default:
					break;
			}

			FreightDataRegistry.Instance.AWBBarcodeLabelCustomisation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, awbLabel);
		}

		ZString OptionalInformationProperty(OuterPackForTest outerPack, int index)
		{
			switch (index)
			{
				case 1:
					return outerPack.OptionalInformation1;
				case 2:
					return outerPack.OptionalInformation2;
				case 3:
					return outerPack.OptionalInformation3;
				case 4:
					return outerPack.OptionalInformation4;
				case 5:
					return outerPack.OptionalInformation5;
				case 6:
					return outerPack.OptionalInformation6;
				default:
					return "";
			}
		}

		#region OuterPack overriden class for tests

		public class OuterPackForTest : OuterPack
		{
			public OuterPackForTest(string shipmentHandlingInformation = "ShipmentHandlingInformation")
			{
				this.shipmentHandlingInformation = shipmentHandlingInformation;
			}

			public override ZString ConsolOrigin
			{
				get { return "ORI"; }
			}

			public override ZString ConsolDestination
			{
				get { return "DES"; }
			}

			public override ZString ConsolPieceNumber
			{
				get { return "5"; }
			}

			public override ZString ConsolPieceCount
			{
				get { return "10"; }
			}

			public override ZString ConsolWeight
			{
				get { return "240"; }
			}

			public override ZString HousebillNumber
			{
				get { return "HBL123456"; }
			}

			public override ZString ShipmentPieceNumber
			{
				get { return "2"; }
			}

			public override ZString ShipmentPieceCount
			{
				get { return "7"; }
			}

			public override ZString ShipmentWeight
			{
				get { return "100"; }
			}

			public override ZString ShipmentHandlingInformation
			{
				get { return shipmentHandlingInformation; }
			}

			readonly ZString shipmentHandlingInformation;

			public override ZString IssuingCompanyName
			{
				get { return "IssuingCompanyName"; }
			}
		}

		#endregion

		#endregion

		#region Implementation

		OuterPack OuterPack;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		ExportAWBHeader ExportAWBHeader;

		protected override void SetUp()
		{
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ExportAWBHeader = Consol.AWBHeader;

			OuterPack = new OuterPack();
			Shipment = Consol.Shipments.AddNew();
			OuterPack.DocShipment = DocShipment.New(Shipment, Factory);
			OuterPack.SetupOptionalInformation(DocAWB.New(Consol.AWBHeader, Factory));

			base.SetUp();
		}

		#endregion
	}
}
