using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Tsting
{
	[TestedType(typeof(DocIMOShipment))]
	sealed class DocIMOShipmentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocIMOShipment.New(Shipment, Consol, Factory) };
		}

		public void TestIMOSender()
		{
			AssertNull(IMOShipmentWrapper.IMOSender);
			var cnor = Factory.New<OrgHeader>();
			cnor.OH_FullName = "CONSIGNOR";
			var cnee = Factory.New<OrgHeader>();
			cnee.OH_FullName = "CONSIGNEE";
			Shipment.ConsignorPK = cnor.PK;
			Shipment.ConsigneePK = cnee.PK;
			AssertEquals("CONSIGNOR", IMOShipmentWrapper.IMOSender.Name);
		}

		public void TestIMOConsignee()
		{
			AssertNull(IMOShipmentWrapper.IMOConsignee);
			var cnor = Factory.New<OrgHeader>();
			cnor.OH_FullName = "CONSIGNOR";
			var cnee = Factory.New<OrgHeader>();
			cnee.OH_FullName = "CONSIGNEE";
			Shipment.ConsignorPK = cnor.PK;
			Shipment.ConsigneePK = cnee.PK;
			AssertEquals("CONSIGNEE", IMOShipmentWrapper.IMOConsignee.Name);
		}

		public void TestIMOCarrier()
		{
			AssertNull(IMOShipmentWrapper.IMOCarrier);
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "CARRIER";
			Consol.SetDefaultShippingLineAddress(carrier);
			AssertEquals("CARRIER", IMOShipmentWrapper.IMOCarrier.Name);
		}

		public void TestIMOConsolNumber()
		{
			Consol.JK_UniqueConsignRef = "C00093832";
			AssertEquals("Consol: C00093832", IMOShipmentWrapper.IMOConsolNumber);

			Consol.JK_TransportMode = "SEA";
			AssertEquals("Consol: C00093832", IMOShipmentWrapper.IMOConsolNumber);

			Consol.JK_MasterBillNum = "MASTER90383";
			AssertEquals("Consol: C00093832\nOCEAN BILL OF LADING: MASTER90383", IMOShipmentWrapper.IMOConsolNumber);

			Consol.JK_TransportMode = "AIR";
			Consol.JK_MasterBillNum = "MASTER90383";
			AssertEquals("Consol: C00093832\nMAWB: MASTER90383", IMOShipmentWrapper.IMOConsolNumber);

			Consol.JK_TransportMode = "RAI";
			Consol.JK_MasterBillNum = "MASTER90383";
			AssertEquals("Consol: C00093832\nMASTER: MASTER90383", IMOShipmentWrapper.IMOConsolNumber);
		}

		public void TestIMOShippersRef()
		{
			IMOShipmentWrapper.SetReportNameForTesting("IMO");
			AssertEquals("", IMOShipmentWrapper.IMOShippersRef);
			Consol.JK_AgentsReference = "AGENTREF123";
			Consol.JK_BookingReference = "123";
			AssertEquals("123", IMOShipmentWrapper.IMOShippersRef);

			IMOShipmentWrapper.SetReportNameForTesting("CFS IMO");
			AssertEquals("AGENTREF123", IMOShipmentWrapper.IMOShippersRef);
		}

		public void TestIMOForwardersRef()
		{
			IMOShipmentWrapper.SetReportNameForTesting("IMO");
			AssertEquals("", IMOShipmentWrapper.IMOForwardersRef);
			Consol.JK_AgentsReference = "AGENTREF123";
			AssertEquals("AGENTREF123", IMOShipmentWrapper.IMOForwardersRef);

			IMOShipmentWrapper.SetReportNameForTesting("CFS IMO");
			AssertEquals("Empty for CFS", "", IMOShipmentWrapper.IMOForwardersRef);
		}

		public void TestIMOVesselVoyage()
		{
			Consol.JK_TransportMode = "SEA";

			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = "VESSEL 123";
			transport.JW_VoyageFlight = "VOYAGE 123";
			AssertEquals("VESSEL 123 / VOYAGE 123", IMOShipmentWrapper.IMOVesselVoyage);

			Consol.JK_TransportMode = "AIR";
			transport.JW_VoyageFlight = "QF123";
			AssertEquals("QF123", IMOShipmentWrapper.IMOVesselVoyage);

			Consol.JK_TransportMode = "RAI";
			transport.JW_Vessel = "";
			transport.JW_VoyageFlight = "TRAIN123";
			AssertEquals("TRAIN123", IMOShipmentWrapper.IMOVesselVoyage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestIMOETD()
		{
			AssertEquals("", IMOShipmentWrapper.IMOETD);

			Consol.JK_TransportMode = "SEA";
			Transport transport = Consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			AssertEquals(ZDateTime.Today.ToString("dd-MMM-yy"), IMOShipmentWrapper.IMOETD);
		}

		public void TestIMOPortOfLoading()
		{
			AssertEquals("", IMOShipmentWrapper.IMOPortOfLoading);
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			AssertEquals("AUBNE, Brisbane", IMOShipmentWrapper.IMOPortOfLoading);
		}

		public void TestIMOPortOfDischarge()
		{
			AssertEquals("", IMOShipmentWrapper.IMOPortOfDischarge);
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "SGSIN";
			AssertEquals("SGSIN, Singapore", IMOShipmentWrapper.IMOPortOfDischarge);
		}

		public void TestIMODestination()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMODestination);
			Shipment.JS_RL_NKDestination = "ITROM";
			AssertEquals("ITROM, Rome (Roma)", IMOShipmentWrapper.IMODestination);
		}

		public void TestIMOHandlingInstructions()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMOHandlingInstructions);

			var note = Consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			note.ST_NoteDataAsText = "Handling instructions 123 consol";
			AssertEquals("Handling instructions from consol", "Handling instructions 123 consol", IMOShipmentWrapper.IMOHandlingInstructions);

			var note2 = Shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			note2.ST_NoteDataAsText = "Handling instructions 123 shipment";
			AssertEquals("Handling instructions from shipment", "Handling instructions 123 shipment", IMOShipmentWrapper.IMOHandlingInstructions);
		}

		public void TestIMOContainerNum()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMOContainerNum);
		}

		public void TestIMOSealNum()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMOSealNum);
		}

		public void TestIMOContainerType()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMOContainerType);
		}

		public void TestIMOContainerTare()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMOContainerTare);
		}

		public void TestIMOTotalGrossMassAndTare()
		{
			AssertEquals("Should be empty", ZString.Empty, IMOShipmentWrapper.IMOTotalGrossMassAndTare);
		}

		public void TestIMOShipmentsForShipment()
		{
			IMOShipmentWrapper.SetReportNameForTesting("SHIPMENT IMO");

			Shipment.JS_UniqueConsignRef = "S03980003";
			Shipment.JS_OuterPacks = 20;
			Shipment.JS_F3_NKPackType = "PLT";
			Shipment.JS_GoodsDescription = "Goods";

			PackLine line1 = Shipment.OuterPackLines.AddNew();
			PackLine line2 = Shipment.OuterPackLines.AddNew();

			DocIMOBodyCollection coll = IMOShipmentWrapper.IMOShipments;
			AssertEquals("Should be 1 item in the collection", 1, coll.Count);
			AssertEquals("Marks and numbers", "No Uncontainerized Hazardous packlines available for Shipment S03980003.", coll[0].MarksAndNumbers);
			AssertEquals("Description should be empty", "", coll[0].GoodsDescription);
			AssertEquals("Gross mass should be empty", "", coll[0].GrossMass);
			AssertEquals("Net mass line should be empty", "", coll[0].NetMassLine);
			AssertEquals("Volume should be empty", "", coll[0].Volume);

			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("Count", 1, IMOShipmentWrapper.IMOShipments.Count);

			line2.JL_PackageCount = 12;
			line2.JL_ActualWeight = 30M;
			line2.JL_ActualVolume = 10M;
			line2.JL_F3_NKPackType = "BOX";
			line2.JL_ActualWeightUQ = "KG";
			line2.JL_ActualVolumeUQ = "M3";
			line2.UNDGs.AddNew().DI_DG = UNDGCode.PK;

			ZString expectedDesc = "Job Reference: S03980003" + "\n";
			expectedDesc += "12 Box 30 KG 10 M3" + "\n";
			expectedDesc += "UN9900, SHIPPING NAME, class 1, PG III, MARINE POLLUTANT\n";
			expectedDesc += "Description: Goods";
			AssertEquals("Desc", expectedDesc, IMOShipmentWrapper.IMOShipments[0].GoodsDescription);

			var cont1 = (CommonContainer)Consol.Containers.AddNew();
			cont1.JC_ContainerNum = "CONT123";
			line2.SetContainer(Consol, cont1);

			coll = IMOShipmentWrapper.IMOShipments;
			AssertEquals("Should be 1 item in the collection", 1, coll.Count);
			AssertEquals("Marks and numbers", "No Uncontainerized Hazardous packlines available for Shipment S03980003.", coll[0].MarksAndNumbers);
			AssertEquals("Description should be empty", "", coll[0].GoodsDescription);
			AssertEquals("Gross mass should be empty", "", coll[0].GrossMass);
			AssertEquals("Net mass line should be empty", "", coll[0].NetMassLine);
			AssertEquals("Volume should be empty", "", coll[0].Volume);
		}

		public void TestIMOShipmentsForConsolAndCFS()
		{
			Shipment.JS_UniqueConsignRef = "S03980003";
			Shipment.JS_OuterPacks = 20;
			Shipment.JS_F3_NKPackType = "PLT";
			Shipment.JS_GoodsDescription = "Goods";
			var marks = Shipment.Notes.AddNew();
			marks.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			marks.ST_NoteDataAsText = "Marks and also numbers";

			DocIMOBodyCollection coll = IMOShipmentWrapper.IMOShipments;
			AssertEquals("Should be 1 item in the collection", 1, coll.Count);
			AssertEquals("Marks and numbers", "No Uncontainerized Hazardous packlines available for Shipment S03980003.", coll[0].MarksAndNumbers);
			AssertEquals("Description should be empty", "", coll[0].GoodsDescription);
			AssertEquals("Gross mass should be empty", "", coll[0].GrossMass);
			AssertEquals("Net mass line should be empty", "", coll[0].NetMassLine);
			AssertEquals("Volume should be empty", "", coll[0].Volume);

			PackLine line = Shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line.UNDGs.AddNew().DI_DG = UNDGCode.PK;
			line.JL_PackageCount = 15;
			line.JL_F3_NKPackType = "PLT";
			line.JL_ActualWeight = 200M;
			line.JL_ActualWeightUQ = "G";
			line.JL_ActualVolume = 300M;
			line.JL_ActualVolumeUQ = "L";

			ZString expectedDesc = "Job Reference: S03980003" + "\n";
			expectedDesc += "15 Pallet 200 G 300 L" + "\n";
			expectedDesc += "UN9900, SHIPPING NAME, class 1, PG III, MARINE POLLUTANT\n";
			expectedDesc += "Description: Goods";

			AssertEquals("Count", 1, IMOShipmentWrapper.IMOShipments.Count);
			AssertEquals("Marks and also numbers", IMOShipmentWrapper.IMOShipments[0].MarksAndNumbers);
			AssertEquals(expectedDesc, IMOShipmentWrapper.IMOShipments[0].GoodsDescription);
			AssertEquals("0.2", IMOShipmentWrapper.IMOShipments[0].GrossMass);
			AssertEquals("0.3", IMOShipmentWrapper.IMOShipments[0].Volume);

			PackLine line2 = Shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 5;
			line2.JL_F3_NKPackType = "PLT";
			line2.JL_ActualWeight = 200M;
			line2.JL_ActualWeightUQ = "KG";
			line2.JL_ActualVolume = 300M;
			line2.JL_ActualVolumeUQ = "M3";

			IMOShipmentWrapper = DocIMOShipment.New(Shipment, Factory);
			AssertEquals("Count", 1, IMOShipmentWrapper.IMOShipments.Count);
			AssertEquals(expectedDesc, IMOShipmentWrapper.IMOShipments[0].GoodsDescription);

			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line2.UNDGs.AddNew().DI_DG = UNDGCode.PK;

			IMOShipmentWrapper = DocIMOShipment.New(Shipment, Factory);
			AssertEquals("Count", 1, IMOShipmentWrapper.IMOShipments.Count);

			expectedDesc = "Job Reference: S03980003" + "\n";
			expectedDesc += "15 Pallet 200 G 300 L" + "\n";
			expectedDesc += "UN9900, SHIPPING NAME, class 1, PG III, MARINE POLLUTANT\n";
			expectedDesc += "5 Pallet 200 KG 300 M3" + "\n";
			expectedDesc += "UN9900, SHIPPING NAME, class 1, PG III, MARINE POLLUTANT\n";
			expectedDesc += "Description: Goods";
			AssertEquals(expectedDesc, IMOShipmentWrapper.IMOShipments[0].GoodsDescription);
			AssertEquals("200.2", IMOShipmentWrapper.IMOShipments[0].GrossMass);
			AssertEquals("300.3", IMOShipmentWrapper.IMOShipments[0].Volume);
		}

		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		DocIMOShipment IMOShipmentWrapper;
		UNDGSubstance fUNDGCode;
		UNDGSubstance UNDGCode
		{
			get
			{
				if (fUNDGCode == null)
				{
					fUNDGCode = Factory.New<UNDGSubstance>();
					fUNDGCode.DG_UNNO = "9900";
					fUNDGCode.DG_Class = "1";
					fUNDGCode.DG_PSN = "SHIPPING NAME";
					fUNDGCode.DG_PG = "III";
					fUNDGCode.DG_MP = "X";
				}
				return fUNDGCode;
			}
		}

		protected override void SetUp()
		{
			Consol = Factory.New<ForwardingConsol>();
			Shipment = Consol.Shipments.AddNew();
			IMOShipmentWrapper = DocIMOShipment.New(Shipment, Consol, Factory);

			base.SetUp();
		}
	}
}
