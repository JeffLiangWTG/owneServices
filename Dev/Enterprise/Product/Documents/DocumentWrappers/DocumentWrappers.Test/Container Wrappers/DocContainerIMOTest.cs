using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	sealed class DocContainerIMOTest : TestCaseWithFactory
	{
		public void TestIMOOrgs()
		{
			var sender = Factory.New<OrgHeader>();
			sender.OH_Code = "SEND";
			var receiver = Factory.New<OrgHeader>();
			receiver.OH_Code = "RECV";
			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SHIP";

			Consol.SetDefaultSendingForwarderAddress(sender);
			Consol.SetDefaultReceivingForwarderAddress(receiver);
			Consol.SetDefaultShippingLineAddress(shipper);
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("IMO Sender", "SEND", ContainerWrapper.IMOSender.Code);
			AssertEquals("IMO Consignee", "RECV", ContainerWrapper.IMOConsignee.Code);
			AssertEquals("IMO Carrier", "SHIP", ContainerWrapper.IMOCarrier.Code);
		}

		public void TestIMOOrgsForShipmentIMO()
		{
			var shipment = Consol.Shipments.AddNew();
			ContainerWrapper = DocContainer.New(FreightContainer, shipment, Factory);
			ContainerWrapper.SetReportNameForTesting("SHIPMENT IMO");
			AssertNull("IMO Sender is Shipment's Consignor", ContainerWrapper.IMOSender);
			AssertNull("IMO Consignee is Shipment's Consignee", ContainerWrapper.IMOConsignee);

			var cnor = Factory.New<OrgHeader>();
			cnor.OH_Code = "CNOR";
			var cnee = Factory.New<OrgHeader>();
			cnee.OH_Code = "CNEE";

			shipment.ConsignorPK = cnor.PK;
			shipment.ConsigneePK = cnee.PK;
			AssertEquals("IMO Sender is Shipment's Consignor", "CNOR", ContainerWrapper.IMOSender.Code);
			AssertEquals("IMO Consignee is Shipment's Consignee", "CNEE", ContainerWrapper.IMOConsignee.Code);
		}

		public void TestIMOConsolNumber()
		{
			Consol.JK_UniqueConsignRef = "C00093832";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Consol: C00093832", ContainerWrapper.IMOConsolNumber);

			Consol.JK_TransportMode = "SEA";
			AssertEquals("Consol: C00093832", ContainerWrapper.IMOConsolNumber);

			Consol.JK_MasterBillNum = "MASTER90383";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Consol: C00093832\r\nOCEAN BILL OF LADING: MASTER90383", ContainerWrapper.IMOConsolNumber);

			Consol.JK_TransportMode = "AIR";
			Consol.JK_MasterBillNum = "MASTER90383";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Consol: C00093832\r\nMAWB: MASTER90383", ContainerWrapper.IMOConsolNumber);

			Consol.JK_TransportMode = "RAI";
			Consol.JK_MasterBillNum = "MASTER90383";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Consol: C00093832\r\nMASTER: MASTER90383", ContainerWrapper.IMOConsolNumber);
		}

		public void TestIMOShippersRef()
		{
			AssertEquals("", ContainerWrapper.IMOShippersRef);
			Consol.JK_BookingReference = "BOOK384848";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("BOOK384848", ContainerWrapper.IMOShippersRef);
		}

		public void TestIMOForwardersRef()
		{
			AssertEquals("", ContainerWrapper.IMOForwardersRef);
			Consol.JK_AgentsReference = "AGENTSREF84848";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("AGENTSREF84848", ContainerWrapper.IMOForwardersRef);
		}

		public void TestIMOVesselVoyage()
		{
			Consol.JK_TransportMode = "SEA";
			Transport transport = Consol.Transports[0];
			transport.JW_Vessel = "RIO";
			transport.JW_VoyageFlight = "999";
			AssertEquals("RIO / 999", ContainerWrapper.IMOVesselVoyage);

			Consol.JK_TransportMode = "AIR";
			transport.JW_VoyageFlight = "SQ999";
			AssertEquals("SQ999", ContainerWrapper.IMOVesselVoyage);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestIMOETD()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			Consol.JK_TransportMode = "SEA";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("IMOETD from consol ETD", ZDateTime.Today.ToString("dd-MMM-yy"), ContainerWrapper.IMOETD);

			var shipment = Consol.Shipments.AddNew();
			ContainerWrapper = DocContainer.New(FreightContainer, shipment, Factory);
			ContainerWrapper.SetReportNameForTesting("SHIPMENT IMO");
			AssertEquals("IMOETD fallback to consol ETD", ZDateTime.Today.ToString("dd-MMM-yy"), ContainerWrapper.IMOETD);

			shipment.JS_E_DEP = ZDateTime.Today.AddDays(1);
			AssertEquals("IMOETD from shipment ETD", ZDateTime.Today.AddDays(1).ToString("dd-MMM-yy"), ContainerWrapper.IMOETD);
		}

		public void TestIMOPortOfLoading()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("AUBNE, Brisbane", ContainerWrapper.IMOPortOfLoading);
		}

		public void TestIMOPortOfDischarge()
		{
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "SGSIN";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("SGSIN, Singapore", ContainerWrapper.IMOPortOfDischarge);
		}

		public void TestIMODestination()
		{
			AssertEquals("", ContainerWrapper.IMODestination);

			var ship1 = Consol.Shipments.AddNew();
			ship1.JS_UniqueConsignRef = "SHIP1";
			ship1.JS_RL_NKDestination = "SGSIN";

			var line1 = ship1.OuterPackLines.AddNew();
			var line2 = ship1.OuterPackLines.AddNew();
			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line1.SetContainer(Consol, FreightContainer);
			line2.SetContainer(Consol, FreightContainer);

			AssertEquals("", ContainerWrapper.IMODestination);

			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("SGSIN, Singapore", ContainerWrapper.IMODestination);

			var ship2 = Consol.Shipments.AddNew();
			ship2.JS_UniqueConsignRef = "SHIP2";
			var line3 = ship2.OuterPackLines.AddNew();
			line3.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line3.SetContainer(Consol, FreightContainer);

			AssertEquals("SGSIN, Singapore", ContainerWrapper.IMODestination);

			line3.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("", ContainerWrapper.IMODestination);
		}

		public void TestIMOHandlingInstructions()
		{
			AssertEquals("", ContainerWrapper.IMOHandlingInstructions);
			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ReportName, DocBaseWrapper.ConsolIMO);
			ContainerWrapper.SetTemplateConstants(constants);

			var note = Consol.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			note.ST_NoteDataAsText = "DG Handling instructions 123";
			AssertEquals("DG Handling instructions 123", ContainerWrapper.IMOHandlingInstructions);

			constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ReportName, DocBaseWrapper.ShipmentIMO);
			ContainerWrapper = DocContainer.New(FreightContainer, Shipment, Factory);
			ContainerWrapper.SetTemplateConstants(constants);
			AssertEquals("DG Handling instructions 123", ContainerWrapper.IMOHandlingInstructions);

			var note2 = Shipment.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description;
			note2.ST_NoteDataAsText = "DG Handling instructions from shipment";
			AssertEquals("DG Handling instructions from shipment", ContainerWrapper.IMOHandlingInstructions);
		}

		public void TestIMOContainerNum()
		{
			AssertEquals(FreightContainer.JC_ContainerNum, ContainerWrapper.IMOContainerNum);
		}

		public void TestIMOSealNum()
		{
			AssertEquals("", ContainerWrapper.IMOSealNum);
			FreightContainer.JC_SealNum = "09384784";
			AssertEquals("09384784", ContainerWrapper.IMOSealNum);
		}

		public void TestIMOContainerType()
		{
			AssertEquals("", ContainerWrapper.IMOContainerType);
			FreightContainer.JC_RC = (Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"))).PK;
			FreightContainer.JC_ContainerMode = "LCL";
			AssertEquals("20FR LCL", ContainerWrapper.IMOContainerType);
		}

		public void TestIMOContainerTare()
		{
			AssertEquals("", ContainerWrapper.IMOContainerTare);
			var refCont = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
			FreightContainer.JC_RC = refCont.PK;
			refCont.RC_TareWeight = 1000M;
			AssertEquals("1000", ContainerWrapper.IMOContainerTare);
		}

		public void TestHazardousPackLines()
		{
			var ship1 = Consol.Shipments.AddNew();
			ship1.JS_UniqueConsignRef = "SHIP1";

			var line1 = ship1.OuterPackLines.AddNew();
			var line2 = ship1.OuterPackLines.AddNew();
			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line1.SetContainer(Consol, FreightContainer);
			line2.SetContainer(Consol, FreightContainer);

			var ship2 = Consol.Shipments.AddNew();
			ship2.JS_UniqueConsignRef = "SHIP2";
			var line3 = ship2.OuterPackLines.AddNew();
			line3.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line3.SetContainer(Consol, FreightContainer);

			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Haz PackLines", 0, ContainerWrapper.HazardousPackLines.Count);

			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("HAZ PackLines", 1, ContainerWrapper.HazardousPackLines.Count);

			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("HAZ PackLines", 2, ContainerWrapper.HazardousPackLines.Count);

			var line4 = ship2.OuterPackLines.AddNew();
			line4.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line4.SetContainer(Consol, FreightContainer);
			AssertEquals("HAZ PackLines", 3, ContainerWrapper.HazardousPackLines.Count);
		}

		public void TestIMOTotalGrossMassAndTare()
		{
			Consol.AutomaticallyUpdatePackLineContainers = true;
			FreightContainer.JC_ContainerNum = "CONT1";
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Weight should be zero", "0", ContainerWrapper.IMOTotalGrossMassAndTare);

			FreightContainer.JC_TareWeight = 1000;
			FreightContainer.JC_DunnageWeight = 2000;
			AssertEquals("Should contain combined weight", "3000", ContainerWrapper.IMOTotalGrossMassAndTare);

			FreightContainer.JC_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("Should contain converted combined weight", "3", ContainerWrapper.IMOTotalGrossMassAndTare);

			FreightContainer.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			ShipmentPackLine.JL_ActualWeight = 10000;
			ShipmentPackLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("Should contain converted combined weight", "13000", ContainerWrapper.IMOTotalGrossMassAndTare);
		}

		public void TestIMOShipmentDetails()
		{
			UNDGSubstance uNDG1 = Factory.New<UNDGSubstance>();
			uNDG1.DG_UNNO = "9911";
			uNDG1.DG_Class = "1";
			uNDG1.DG_PSN = "SHIP NAME 1";
			uNDG1.DG_PG = "I";
			uNDG1.DG_MP = "X";

			UNDGSubstance uNDG2 = Factory.New<UNDGSubstance>();
			uNDG2.DG_UNNO = "9922";
			uNDG2.DG_Class = "2";
			uNDG2.DG_PSN = "SHIP NAME 2";
			uNDG2.DG_PG = "II";

			var ship1 = Consol.Shipments.AddNew();
			var ship2 = Consol.Shipments.AddNew();

			DocIMOBodyCollection coll = ContainerWrapper.IMOShipments;

			AssertEquals("Should be 1 item in the collection", 1, coll.Count);
			AssertEquals("Marks and numbers", "No Hazardous packlines available for this container", coll[0].MarksAndNumbers);
			AssertEquals("Description should be empty", "", coll[0].GoodsDescription);
			AssertEquals("Gross mass should be empty", "", coll[0].GrossMass);
			AssertEquals("Net mass line should be empty", "", coll[0].NetMassLine);
			AssertEquals("Volume should be empty", "", coll[0].Volume);

			var line1 = ship1.OuterPackLines.AddNew();
			var line2 = ship1.OuterPackLines.AddNew();
			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line1.UNDGs.AddNew().DI_DG = uNDG1.PK;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			line1.SetContainer(Consol, FreightContainer);
			line2.SetContainer(Consol, FreightContainer);

			var line3 = ship2.OuterPackLines.AddNew();
			line3.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line3.UNDGs.AddNew().DI_DG = uNDG2.PK;
			line3.SetContainer(Consol, FreightContainer);

			var ship1Marks = ship1.Notes.AddNew();
			ship1Marks.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			ship1Marks.ST_NoteDataAsText = "Marks and numbers shipment 1";
			ship1.JS_UniqueConsignRef = "S00009393";
			ship1.JS_GoodsDescription = "Ship1 goods";
			SetPackLineDetails(line1, 30, "DOZ", 100M, "KG", 3000M, "M3");
			SetPackLineDetails(line2, 2, "BOX", 2000M, "G", 1000M, "L");

			var ship2Marks = ship2.Notes.AddNew();
			ship2Marks.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			ship2Marks.ST_NoteDataAsText = "Marks and numbers shipment 2";
			ship2.JS_UniqueConsignRef = "S00009423";
			ship2.JS_GoodsDescription = "Ship2 goods";
			SetPackLineDetails(line3, 15, "BOX", 2000M, "KG", 1000M, "L");

			coll = ContainerWrapper.IMOShipments;

			ZString expectedDesc = "Job Reference: S00009393\n30 Dozen 100 KG 3000 M3\n";
			expectedDesc += "UN9911, SHIP NAME 1, class 1, PG I, MARINE POLLUTANT\n";
			expectedDesc += "Description: Ship1 goods";

			AssertEquals("Marks and numbers shipment 1", coll[0].MarksAndNumbers);
			AssertEquals(expectedDesc, coll[0].GoodsDescription);
			AssertEquals("100", coll[0].GrossMass);
			AssertEquals("", coll[0].NetMassLine);
			AssertEquals("3000", coll[0].Volume);

			expectedDesc = "Job Reference: S00009423\n15 Box 2000 KG 1000 L\n";
			expectedDesc += "UN9922, SHIP NAME 2, class 2, PG II\n";
			expectedDesc += "Description: Ship2 goods";

			AssertEquals("Marks and numbers shipment 2", coll[1].MarksAndNumbers);
			AssertEquals(expectedDesc, coll[1].GoodsDescription);
			AssertEquals("2000", coll[1].GrossMass);
			AssertEquals("", coll[1].NetMassLine);
			AssertEquals("1", coll[1].Volume);

			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line2.UNDGs.AddNew().DI_DG = uNDG2.PK;
			expectedDesc = "Job Reference: S00009393\n30 Dozen 100 KG 3000 M3\n";
			expectedDesc += "UN9911, SHIP NAME 1, class 1, PG I, MARINE POLLUTANT\n";
			expectedDesc += "2 Box 2000 G 1000 L\n";
			expectedDesc += "UN9922, SHIP NAME 2, class 2, PG II\n";
			expectedDesc += "Description: Ship1 goods";

			coll = ContainerWrapper.IMOShipments;
			AssertEquals(expectedDesc, coll[0].GoodsDescription);
			AssertEquals("102", coll[0].GrossMass);
			AssertEquals("", coll[0].NetMassLine);
			AssertEquals("3001", coll[0].Volume);
		}

		public void TestIMOShipmetsForShipmentIMO()
		{
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S9990009";
			shipment.JS_GoodsDescription = "Ship1 goods";
			ContainerWrapper = DocContainer.New(FreightContainer, shipment, Factory);
			ContainerWrapper.SetReportNameForTesting("SHIPMENT IMO");
			var line1 = shipment.OuterPackLines.AddNew();
			var line2 = shipment.OuterPackLines.AddNew();

			DocIMOBodyCollection coll = ContainerWrapper.IMOShipments;
			AssertEquals("Should be 1 item in the collection", 1, coll.Count);
			AssertEquals("Marks and numbers", "No Hazardous packlines available for this container", coll[0].MarksAndNumbers);
			AssertEquals("Description should be empty", "", coll[0].GoodsDescription);
			AssertEquals("Gross mass should be empty", "", coll[0].GrossMass);
			AssertEquals("Net mass line should be empty", "", coll[0].NetMassLine);
			AssertEquals("Volume should be empty", "", coll[0].Volume);

			UNDGSubstance uNDGCode = Factory.New<UNDGSubstance>();
			uNDGCode.DG_UNNO = "9911";
			uNDGCode.DG_PSN = "SHIP NAME";
			uNDGCode.DG_PG = "II";
			uNDGCode.DG_Class = "2";

			line1.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line1.UNDGs.AddNew().DI_DG = uNDGCode.PK;
			line2.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			line2.UNDGs.AddNew().DI_DG = uNDGCode.PK;

			SetPackLineDetails(line1, 3, "DOZ", 1M, "KG", 3M, "M3");
			SetPackLineDetails(line2, 2, "BOX", 2M, "KG", 1M, "M3");

			ZString expectedDesc = "Job Reference: S9990009\n3 Dozen 1 KG 3 M3\n";
			expectedDesc += "UN9911, SHIP NAME, class 2, PG II\n";
			expectedDesc += "2 Box 2 KG 1 M3\n";
			expectedDesc += "UN9911, SHIP NAME, class 2, PG II\n";
			expectedDesc += "Description: Ship1 goods";

			coll = ContainerWrapper.IMOShipments;
			AssertEquals("Count", 1, coll.Count);
			AssertEquals("Description", expectedDesc, coll[0].GoodsDescription);
		}

		public void TestDocContainerNewMethod()
		{
			DocContainer containerWrapper = DocContainer.New(null, Factory);
			AssertNull("New method should return null when no container is specified", containerWrapper);
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "120893";
			containerWrapper = DocContainer.New(container, Factory);
			AssertNotNull("New method should return a container wrapper", containerWrapper);
			AssertEquals("ContainerWrapper should be a DocContainer", typeof(DocContainer), containerWrapper.GetType());
			AssertEquals("ContainerWrapper should wrap Container", container.JC_ContainerNum, containerWrapper.ContainerNumber);
		}

		#region Implementation

		ForwardingContainer FreightContainer;
		DocContainer ContainerWrapper;
		ForwardingConsol Consol;
		ForwardingShipment Shipment;
		PackLine ShipmentPackLine;

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			FreightContainer = Factory.NewWithValidTestData<ForwardingContainer>();
			Consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Shipment = Consol.Shipments.AddNew();
			ShipmentPackLine = Shipment.OuterPackLines.Count > 0 ? Shipment.OuterPackLines[0] : Shipment.OuterPackLines.AddNew();
			Consol.Containers.Add(FreightContainer);
			FreightContainer.AddPackLine(ShipmentPackLine);
			//			Factory.Save();
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			base.SetUp();
		}

		void SetPackLineDetails(PackLine line, ZInt packs, ZString packType, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			line.JL_PackageCount = packs;
			line.JL_F3_NKPackType = packType;
			line.JL_ActualWeight = weight;
			line.JL_ActualWeightUQ = weightUQ;
			line.JL_ActualVolume = volume;
			line.JL_ActualVolumeUQ = volumeUQ;
		}
		#endregion
	}
}
