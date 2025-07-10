using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRSeaCargoSynchroniserMarksAndNumbersTest : TestCaseWithFactory
	{
		const string VesselLloydsNumber = "1278239";
		const string MasterBillNumber = "OBL373892029";
		const string ContainerNumber1 = "FSCU6400231";
		const string ContainerNumber2 = "FDSU6400232";
		const string ContainerNumber3 = "FDSU6400233";
		const string TestHouseBill = "HBO200429188";
		const string PackLine1Description = "PACK LINE 1 DESCRIPTION";
		const string TestMarksAndNumbers = "SCRATCH HERE - Scratch there";
		const string TestMarksAndNumbers2 = "More marks and numbers";

		public void TestSynchroniseMarksAndNumbers_ChangePackLineContainer()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;

			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TotalPackageCount = 10;

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill;
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			AssertEquals(0, houseBill.Pivot.Count);

			var newPackLine = shipment.OuterPackLines.AddNew();
			var pivot = houseBill.Pivot[0];
			AssertEquals("Auto-allocated onto first container", "FSCU6400231", pivot.CV_MarksAndNumbers);

			newPackLine.SetContainer(container2.PK);
			AssertEquals(1, houseBill.Pivot.Count);
			pivot = houseBill.Pivot[0];
			AssertEquals("Updated to container 2", "FDSU6400232", pivot.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_ChangePackLineContainer_MultiplePacks()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;

			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TotalPackageCount = 10;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_MarksAndNumbers = "P1";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_MarksAndNumbers = "P2";
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill;
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			AssertEquals(1, houseBill.Pivot.Count);
			var pivot = houseBill.Pivot[0];
			AssertEquals("All on Container 1", "P1,P2", pivot.CV_MarksAndNumbers);

			var houseSynchroniser = seaCargoSynchroniser.BusinessObjectSynchronisers.OfType<HouseBillSynchroniser>().FirstOrDefault(x => x.Source.PK == shipment.PK);
			var pivotSynchroniser = houseSynchroniser.PivotSynchronisers.FirstOrDefault(x => x.Destination.PK == pivot.PK);
			AssertEquals("packLine1 is pivot source", packLine1.PK, pivotSynchroniser.Source.PK);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_MarksAndNumbers = "P3";

			AssertEquals(1, houseBill.Pivot.Count);
			AssertEquals("All on Container 1", "P1,P2,P3", pivot.CV_MarksAndNumbers);

			packLine2.SetContainer(container2.PK);
			AssertEquals("Changing container creates an additional pivot", 2, houseBill.Pivot.Count);
			var pivot2 = houseBill.Pivot[1];
			AssertEquals("P1,P3", pivot.CV_MarksAndNumbers);
			AssertEquals("Watched Packline marks moved to container 2", "P2", pivot2.CV_MarksAndNumbers);

			packLine1.SetContainer(container2.PK);
			AssertEquals(2, houseBill.Pivot.Count);
			AssertEquals("'Source' pivot is deleted", true, pivot.IsDeleted);

			var oceanBillContainer1 = oceanBill.Containers.Find(ContainerNumber1);
			var oceanBillContainer2 = oceanBill.Containers.Find(ContainerNumber2);
			pivot = houseBill.Pivot.Cast<CusSCAPivot>().FirstOrDefault(x => x.CV_CN == oceanBillContainer1.PK);
			AssertEquals("Source Packline marks removed from container 1 pivot", "P3", pivot.CV_MarksAndNumbers);
			pivot2 = houseBill.Pivot.Cast<CusSCAPivot>().FirstOrDefault(x => x.CV_CN == oceanBillContainer2.PK);
			AssertEquals("Source Packline marks moved to container 2 pivot", "P1,P2", pivot2.CV_MarksAndNumbers);

			pivotSynchroniser = houseSynchroniser.PivotSynchronisers.FirstOrDefault(x => x.Destination.PK == pivot.PK);
			AssertEquals("packLine3 is promoted to source on new pivot to container 1", packLine3.PK, pivotSynchroniser.Source.PK);
		}

		public void TestSynchroniseMarksAndNumbers_ChangePackLineContainer_InDatabase()
		{
			var t_consol = CreateConsol();
			t_consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			var t_container1 = (CommonContainer)t_consol.Containers.AddNew();
			t_container1.JC_ContainerNum = ContainerNumber1;
			t_container1.JC_RC = Get40FootGPContainer().PK;
			var t_container2 = (CommonContainer)t_consol.Containers.AddNew();
			t_container2.JC_ContainerNum = ContainerNumber2;
			t_container2.JC_RC = Get40FootGPContainer().PK;

			CommonShipment t_shipment = t_consol.Shipments.AddNew();
			t_shipment.JS_TotalPackageCount = 10;
			var t_packLine1 = t_shipment.OuterPackLines.AddNew();
			t_packLine1.SetContainer(t_container1.PK);
			t_packLine1.JL_MarksAndNumbers = "P1";
			var t_packLine2 = t_shipment.OuterPackLines.AddNew();
			t_packLine2.SetContainer(t_container1.PK);
			t_packLine2.JL_MarksAndNumbers = "P2";

			var t_oceanBill = new CMRSeaCargoSynchroniser(t_consol).OceanBill;
			Factory.Save();

			var secondFactory = new BusinessObjectFactory();
			var consol = secondFactory.Load<ForwardingConsol>(t_consol.PK);
			var container1 = (CommonContainer)consol.Containers.FindByPK(t_container1.PK);
			var container2 = (CommonContainer)consol.Containers.FindByPK(t_container2.PK);
			var shipment = consol.Shipments[0];
			var packLine1 = (PackLine)shipment.OuterPackLines.FindByPK(t_packLine1.PK);
			var packLine2 = (PackLine)shipment.OuterPackLines.FindByPK(t_packLine2.PK);

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol, synchroniseConsol: false);
			var houseBill = seaCargoSynchroniser.GetHouseBill(shipment); // mimic StartSynchronising() in ShipmentWrapper
			AssertEquals(1, houseBill.Pivot.Count);
			var pivot = houseBill.Pivot[0];
			AssertEquals("Both on Container 1", "P1,P2", pivot.CV_MarksAndNumbers);

			packLine2.SetContainer(container2.PK);
			AssertEquals("Changing container creates an additional pivot", 2, houseBill.Pivot.Count);
			var pivot2 = houseBill.Pivot[1];
			AssertEquals("P1", pivot.CV_MarksAndNumbers);
			AssertEquals("Watched Packline marks moved to container 2", "P2", pivot2.CV_MarksAndNumbers);

			var oceanBill = seaCargoSynchroniser.OceanBill;
			var cusContainer2 = oceanBill.Containers.Find(cn => cn.CN_ContainerNumber == ContainerNumber2).First();
			AssertEquals("container2 assigned to pivot2", cusContainer2.PK, pivot2.CV_CN);

			packLine1.SetContainer(container2.PK);
			AssertEquals("Changing container removes a pivot", 1, houseBill.Pivot.Count);
			AssertEquals("'Source' pivot is deleted", true, pivot.IsDeleted);
			AssertEquals("Both on Container 2", "P1,P2", pivot2.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_NewPackLineWithoutContainer()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TotalPackageCount = 10;

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill;
			Factory.Save();

			consol.AutomaticallyUpdatePackLineContainers = false;

			var houseBill = oceanBill.HouseBills[0];
			AssertEquals(0, houseBill.Pivot.Count);

			var newPackLine = shipment.OuterPackLines.AddNew();
			var pivot = houseBill.Pivot[0];
			AssertNull("Packline without container is not given a pivot", pivot);

			newPackLine.SetContainer(container1.PK);
			AssertEquals(1, houseBill.Pivot.Count);
			pivot = houseBill.Pivot[0];
			AssertEquals("Pivot created on container 1", "FSCU6400231", pivot.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_FCL_DefaultingFromContainerNumber()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);

			var packLine1 = (PackLine)shipment.OuterPackLines[0]; // should already have 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = ZString.Empty;

			var packLine2 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine2.JL_Description = "PackLine Description";
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 50m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = ZString.Empty;
			Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipment.JS_MarksAndNumbers);

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();

			var containerPivots = houseBill.Pivot;
			var pivotContainer1 = containerPivots.Find(p => p.CV_CN == obContainer1.PK).First();
			var pivotContainer2 = containerPivots.Find(p => p.CV_CN == obContainer2.PK).First();
			AssertEquals("Marks And Numbers on House Container1 defaults to Container Number", ContainerNumber1, pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 defaults to Container Number", ContainerNumber2, pivotContainer2.CV_MarksAndNumbers);

			shipment.JS_MarksAndNumbers = "Ship Marks";
			AssertEquals("Marks And Numbers on House Container1 overwritten by Shipment MarksAndNumbers", "SHIP MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 overwritten by Shipment MarksAndNumbers", "SHIP MARKS", pivotContainer2.CV_MarksAndNumbers);

			pivotContainer1.CV_MarksAndNumbers = ZString.Empty;
			pivotContainer2.CV_MarksAndNumbers = ZString.Empty;

			packLine1.JL_MarksAndNumbers = "PL1 Marks";
			packLine2.JL_MarksAndNumbers = "PL2 Marks";
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2", "PL2 MARKS", pivotContainer2.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_FCL_DefaultingFromPackLineMarks()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);

			var packLine1 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = ZString.Empty;
			packLine1.JL_MarksAndNumbers = "PL1 Marks";

			var packLine2 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine2.JL_Description = "PackLine Description";
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 50m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = ZString.Empty;
			packLine2.JL_MarksAndNumbers = "PL2 Marks";
			Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipment.JS_MarksAndNumbers);

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();

			var containerPivots = houseBill.Pivot;
			var pivotContainer1 = containerPivots.Find(p => p.CV_CN == obContainer1.PK).First();
			var pivotContainer2 = containerPivots.Find(p => p.CV_CN == obContainer2.PK).First();

			AssertEquals("Marks And Numbers on House Container1 updates from PackLine MarksAndNumbers", "PL1 MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 updates from PackLine MarksAndNumbers", "PL2 MARKS", pivotContainer2.CV_MarksAndNumbers);

			shipment.JS_MarksAndNumbers = "Ship Marks";
			AssertEquals("Marks And Numbers on House Container1 updates from PackLine MarksAndNumbers", "PL1 MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 updates from PackLine MarksAndNumbers", "PL2 MARKS", pivotContainer2.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_FCL_DefaultingFromShipmentMarks()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_MarksAndNumbers = "Ship Marks";

			var packLine1 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = "";

			var packLine2 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine2.JL_Description = "PackLine Description";
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 50m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = "";
			Factory.Save();

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();

			var containerPivots = houseBill.Pivot;
			var container1Pivot = containerPivots.Find(p => p.CV_CN == obContainer1.PK).First();
			var container2Pivot = containerPivots.Find(p => p.CV_CN == obContainer2.PK).First();
			AssertEquals("Marks And Numbers on House Container1 defaulted from Shipment MarksAndNumbers", "SHIP MARKS", container1Pivot.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 defaulted from Shipment MarksAndNumbers", "SHIP MARKS", container2Pivot.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_FCL_DefaultingFromShipmentMarks_Reloaded()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_MarksAndNumbers = "";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			Factory.Save();

			var shipmentF2 = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);

			var packLine1 = shipmentF2.OuterPackLines[0]; // should already be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = "";

			var packLine2 = shipmentF2.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine2.JL_Description = "PackLine Description";
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 50m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = "";

			shipmentF2.Factory.Save();

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var consolF3 = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			var seaCargoSynchroniserF3 = new CMRSeaCargoSynchroniser(consolF3);
			var oceanBillF3 = seaCargoSynchroniserF3.OceanBill;

			var houseBill = oceanBillF3.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();

			var containerPivots = houseBill.Pivot;
			var container1Pivot = containerPivots.Find(p => p.CV_CN == obContainer1.PK).First();
			var container2Pivot = containerPivots.Find(p => p.CV_CN == obContainer2.PK).First();
			AssertEquals("Marks And Numbers on House Container1 defaulted from Container Number", ContainerNumber1, container1Pivot.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 defaulted from Container Number", ContainerNumber2, container2Pivot.CV_MarksAndNumbers);

			shipmentF2.JS_MarksAndNumbers = "Ship Marks";
			shipmentF2.Factory.Save();

			AssertEquals("Marks And Numbers on House Container1 defaulted from Shipment MarksAndNumbers", "SHIP MARKS", container1Pivot.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2 defaulted from Shipment MarksAndNumbers", "SHIP MARKS", container2Pivot.CV_MarksAndNumbers);
			consolF3.Factory.Save();

			var consolF4 = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			var seaCargoSynchroniserF4 = new CMRSeaCargoSynchroniser(consolF4);
			var oceanBillF4 = seaCargoSynchroniserF4.OceanBill;
			var changes = consolF4.Factory.GetChanges().GetChangedObjects();
			AssertEquals(0, changes.Length);
		}

		public void TestSynchroniseMarksAndNumbers_FCL_DefaultingFromShipmentNoteMarks()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TotalPackageCount = 10;

			var newPackLine = shipment.OuterPackLines.AddNew();
			newPackLine.SetContainer(container1.PK);

			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var pivot = houseBill.Pivot[0];

			var note = shipment.Notes.AddNew();
			note.ST_Description = ZArchitecture.Business.PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = TestMarksAndNumbers;
			AssertEquals("Pivot Marks And Numbers", TestMarksAndNumbers.ToUpper(), pivot.CV_MarksAndNumbers);

			shipment.JS_MarksAndNumbers = TestMarksAndNumbers2;
			AssertEquals("Pivot Marks And Numbers", TestMarksAndNumbers2.ToUpper(), pivot.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_LCL()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			var packLine11 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine11.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine11.JL_Description = PackLine1Description;
			packLine11.JL_PackageCount = 11;
			packLine11.JL_F3_NKPackType = "BOX";
			packLine11.JL_ActualWeight = 100m;
			packLine11.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine11.JL_MarksAndNumbers = "C1PL1 Marks";

			var packLine12 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine12.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine12.JL_Description = PackLine1Description;
			packLine12.JL_PackageCount = 12;
			packLine12.JL_F3_NKPackType = "BOX";
			packLine12.JL_ActualWeight = 100m;
			packLine12.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine12.JL_MarksAndNumbers = "C1PL2 Marks";

			var packLine21 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine21.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine21.JL_Description = "PackLine Description";
			packLine21.JL_PackageCount = 21;
			packLine21.JL_F3_NKPackType = "PKG";
			packLine21.JL_ActualWeight = 50m;
			packLine21.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine21.JL_MarksAndNumbers = "C2PL1 Marks";

			var packLine22 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine22.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine22.JL_Description = "PackLine Description";
			packLine22.JL_PackageCount = 22;
			packLine22.JL_F3_NKPackType = "PKG";
			packLine22.JL_ActualWeight = 50m;
			packLine22.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine22.JL_MarksAndNumbers = "C2PL2 Marks";

			Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipment.JS_MarksAndNumbers);

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();

			var containerPivots = houseBill.Pivot;
			var pivotContainer1 = containerPivots.Find(p => p.CV_CN == obContainer1.PK).First();
			var pivotContainer2 = containerPivots.Find(p => p.CV_CN == obContainer2.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "C1PL1 MARKS,C1PL2 MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2", "C2PL1 MARKS,C2PL2 MARKS", pivotContainer2.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_LCL_PacklineDeletedOnShipment()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			var packLine11 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine11.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine11.JL_Description = PackLine1Description;
			packLine11.JL_PackageCount = 11;
			packLine11.JL_F3_NKPackType = "BOX";
			packLine11.JL_ActualWeight = 100m;
			packLine11.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine11.JL_MarksAndNumbers = "C1PL1 Marks";

			var packLine12 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine12.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine12.JL_Description = PackLine1Description;
			packLine12.JL_PackageCount = 12;
			packLine12.JL_F3_NKPackType = "BOX";
			packLine12.JL_ActualWeight = 100m;
			packLine12.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine12.JL_MarksAndNumbers = "C1PL2 Marks";

			Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipment.JS_MarksAndNumbers);

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();

			var containerPivots = houseBill.Pivot;
			var pivotContainer1 = containerPivots.Find(p => p.CV_CN == obContainer1.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "C1PL1 MARKS,C1PL2 MARKS", pivotContainer1.CV_MarksAndNumbers);

			var factory2 = new BusinessObjectFactory();
			var pl12 = factory2.Load<PackLine>(packLine12.PK);
			pl12.Delete();
			factory2.Save();
			AssertEquals("Deleted in main factory", 1, shipment.OuterPackLines.Count);

			shipment.JS_MarksAndNumbers = "TRIGGER A RECALC";
			AssertEquals("No Error reported", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Marks And Numbers on House Container1", "C1PL1 MARKS", pivotContainer1.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_FCL_RefreshBus()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			var container3 = (CommonContainer)consol.Containers.AddNew();
			container3.JC_ContainerNum = ContainerNumber3;
			container3.JC_RC = Get40FootGPContainer().PK;
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;

			var packLine1 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 11;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = "PL1 Marks";

			var packLine2 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine2.JL_Description = PackLine1Description;
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "BOX";
			packLine2.JL_ActualWeight = 100m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = "PL2 Marks";

			var packLine3 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine3.JL_Calc_ContainerNumber = ContainerNumber3;
			packLine3.JL_Description = PackLine1Description;
			packLine3.JL_PackageCount = 13;
			packLine3.JL_F3_NKPackType = "BOX";
			packLine3.JL_ActualWeight = 100m;
			packLine3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine3.JL_MarksAndNumbers = "PL3 Marks";

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			var houseBill = oceanBill.HouseBills[0];
			Factory.Save();

			var shipmentF2 = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			var seaCargoSynchroniserOnShipment = new CMRSeaCargoSynchroniser(shipmentF2.Consols[0]);
			var oceanBillF2 = seaCargoSynchroniserOnShipment.OceanBill;
			var houseBillF2 = oceanBill.HouseBills[0];

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipmentF2.JS_MarksAndNumbers);

			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();
			var obContainer3 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber3).First();

			var pivotContainer1 = houseBill.Pivot.Find(p => p.CV_CN == obContainer1.PK).FirstOrDefault();
			var pivotContainer2 = houseBill.Pivot.Find(p => p.CV_CN == obContainer2.PK).FirstOrDefault();
			var pivotContainer3 = houseBill.Pivot.Find(p => p.CV_CN == obContainer3.PK).FirstOrDefault();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS,PL2 MARKS", pivotContainer1?.CV_MarksAndNumbers);
			AssertEquals("No pivot on Container2", null, pivotContainer2?.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container3", "PL3 MARKS", pivotContainer3?.CV_MarksAndNumbers);
			AssertEquals(2, houseBill.Pivot.Count);

			var packLine1F2 = shipmentF2.OuterPackLines[0];
			var packLine2F2 = shipmentF2.OuterPackLines[1];

			packLine1F2.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine2F2.JL_Calc_ContainerNumber = ContainerNumber2;
			shipmentF2.Factory.Save();

			pivotContainer1 = houseBill.Pivot.Find(p => p.CV_CN == obContainer1.PK).FirstOrDefault();
			pivotContainer2 = houseBill.Pivot.Find(p => p.CV_CN == obContainer2.PK).FirstOrDefault();
			pivotContainer3 = houseBill.Pivot.Find(p => p.CV_CN == obContainer3.PK).FirstOrDefault();
			AssertEquals("Pivot on Container1 is removed by data refresh bus", null, pivotContainer1?.CV_MarksAndNumbers);
			AssertEquals("Pivot is now on Container2", "PL1 MARKS,PL2 MARKS", pivotContainer2?.CV_MarksAndNumbers);
			AssertEquals("Container3 unchanged", "PL3 MARKS", pivotContainer3?.CV_MarksAndNumbers);
			AssertEquals(2, houseBill.Pivot.Count);

			packLine1.JL_MarksAndNumbers = "PL1 Marks V2";
			packLine2.JL_MarksAndNumbers = "PL2 Marks V2";
			packLine3.JL_MarksAndNumbers = "PL3 Marks V2";
			AssertEquals("Marks And Numbers for both lines are synchronising", "PL1 MARKS V2,PL2 MARKS V2", pivotContainer2?.CV_MarksAndNumbers);
			AssertEquals("Container3 synchronising", "PL3 MARKS V2", pivotContainer3?.CV_MarksAndNumbers);
		}

		public void TestSynchroniseMarksAndNumbers_LCL_RefreshBus()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Get40FootGPContainer().PK;
			container1.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			var container2 = (CommonContainer)consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_RC = Get40FootGPContainer().PK;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			var houseBill = oceanBill.HouseBills[0];
			Factory.Save();

			var shipmentF2 = new BusinessObjectFactory().Load<CommonShipment>(shipment.PK);
			var seaCargoSynchroniserOnShipment = new CMRSeaCargoSynchroniser(shipmentF2.Consols[0]);
			var oceanBillF2 = seaCargoSynchroniserOnShipment.OceanBill;
			var houseBillF2 = oceanBill.HouseBills[0];

			var packLine1 = shipmentF2.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 11;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = "PL1 Marks";

			var packLine2 = shipmentF2.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine2.JL_Description = PackLine1Description;
			packLine2.JL_PackageCount = 12;
			packLine2.JL_F3_NKPackType = "BOX";
			packLine2.JL_ActualWeight = 100m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = "PL2 Marks";
			shipmentF2.Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipmentF2.JS_MarksAndNumbers);

			var oceanBillContainers = houseBill.OceanBill.Containers;
			var obContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber1).First();
			var obContainer2 = oceanBillContainers.Find(c => c.CN_ContainerNumber == ContainerNumber2).First();

			var pivotContainer1 = houseBill.Pivot.Find(p => p.CV_CN == obContainer1.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS,PL2 MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals(1, houseBill.Pivot.Count);

			packLine2.JL_Calc_ContainerNumber = ContainerNumber2;
			shipmentF2.Factory.Save();

			pivotContainer1 = houseBill.Pivot.Find(p => p.CV_CN == obContainer1.PK).First();
			var pivotContainer2 = houseBill.Pivot.Find(p => p.CV_CN == obContainer2.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS", pivotContainer1.CV_MarksAndNumbers);
			AssertEquals("Marks And Numbers on House Container2", "PL2 MARKS", pivotContainer2.CV_MarksAndNumbers);
			AssertEquals(2, houseBill.Pivot.Count);
		}

		public void TestSynchroniseMarksAndNumbers_BreakBulk()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;

			var packLine1 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ZString.Empty;  // Break Bulk doesn't have containers
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = "PL1 Marks";

			var packLine2 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ZString.Empty;
			packLine2.JL_Description = "PackLine Description";
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 50m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = "PL2 Marks";
			Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipment.JS_MarksAndNumbers);

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var bbContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == CusSCAPivot.BreakBulk).First();

			var containerPivots = houseBill.Pivot;
			var pivotContainer1 = containerPivots.Find(p => p.CV_CN == bbContainer1.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS,PL2 MARKS", pivotContainer1.CV_MarksAndNumbers);
		}

		public void TestAdditionalPackLineSynchronises_BreakBulk()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			Factory.Save();

			var consignor = CreateConsignor();
			var consignee = CreateConsignee();
			var shipment = consol.Shipments.AddNew();
			SetupShipment(shipment, TestHouseBill, consignor, consignee);
			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;

			var packLine1 = (PackLine)shipment.OuterPackLines[0]; // should be 1 defaulted from Consol.
			packLine1.JL_Calc_ContainerNumber = ZString.Empty;  // Break Bulk doesn't have containers
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_MarksAndNumbers = "PL1 Marks";
			Factory.Save();

			AssertEquals("Marks And Numbers on Shipment", ZString.Empty, shipment.JS_MarksAndNumbers);

			// Generate Sea Cargo.
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var oceanBill = seaCargoSynchroniser.OceanBill; // this is the function called by the PlugIn.
			Factory.Save();

			var houseBill = oceanBill.HouseBills[0];
			var oceanBillContainers = houseBill.OceanBill.Containers;
			var bbContainer1 = oceanBillContainers.Find(c => c.CN_ContainerNumber == CusSCAPivot.BreakBulk).First();

			var containerPivots = houseBill.Pivot;
			var pivotContainer1 = containerPivots.Find(p => p.CV_CN == bbContainer1.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS", pivotContainer1.CV_MarksAndNumbers);

			var packLine2 = (PackLine)shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ZString.Empty;
			packLine2.JL_Description = "PackLine Description";
			packLine2.JL_PackageCount = 8;
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 50m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_MarksAndNumbers = "PL2 Marks";
			Factory.Save();

			containerPivots = houseBill.Pivot;
			pivotContainer1 = containerPivots.Find(p => p.CV_CN == bbContainer1.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS,PL2 MARKS", pivotContainer1.CV_MarksAndNumbers);

			var packLine3 = Factory.New<PackLine>();
			packLine3.JL_Calc_ContainerNumber = ZString.Empty;
			packLine3.JL_Description = "PackLine Description";
			packLine3.JL_PackageCount = 8;
			packLine3.JL_F3_NKPackType = "PKG";
			packLine3.JL_ActualWeight = 50m;
			packLine3.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine3.JL_MarksAndNumbers = "PL3 Marks";

			packLine3.JL_FreightMode = FreightConstants.OuterPackType;
			packLine3.JL_JS = shipment.PK;
			shipment.OuterPackLines.Add(packLine3);
			Factory.Save();

			containerPivots = houseBill.Pivot;
			pivotContainer1 = containerPivots.Find(p => p.CV_CN == bbContainer1.PK).First();
			AssertEquals("Marks And Numbers on House Container1", "PL1 MARKS,PL2 MARKS,PL3 MARKS", pivotContainer1.CV_MarksAndNumbers);
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = MasterBillNumber;
			consol.JK_BookingReference = "BOOKINGREF";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "23";

			var testVessel = RefVessel.New(Factory);
			testVessel.RV_Code = "TESTVESSEL";
			transport.JW_Vessel = testVessel.RV_Code;
			consol.Vessel.RV_LloydsNumber = VesselLloydsNumber;

			var forwarder = CreateValidOrgHeader("Forwarder");
			consol.SetDefaultReceivingForwarderAddress(forwarder);

			var shippingLine = CreateValidOrgHeader("SHIPLINE");
			shippingLine.OH_FullName = "Test Shipping Line";
			shippingLine.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "C011920192");
			shippingLine.LocalBusinessRegNo = "85008945846";
			consol.SetDefaultShippingLineAddress(shippingLine);

			transport.JW_ETD = ZDateTime.Today.AddDays(1);
			transport.JW_ETA = ZDateTime.Today.AddDays(7);

			return consol;
		}

		void SetupShipment(ForwardingShipment shipment, ZString houseBillNumber, OrgHeader consignor, OrgHeader consignee)
		{
			shipment.JS_HouseBill = houseBillNumber;
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //Prepaid
			shipment.JS_GoodsDescription = "Shipment Goods Description";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BOX";

			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
		}

		OrgHeader CreateConsignor()
		{
			var consignor = CreateValidOrgHeader("CONSIGNOR");
			consignor.OH_FullName = "TEST CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "addr1";
			consignor.MainAddress.OA_Address2 = "addr2";
			consignor.MainAddress.OA_City = "foocity";
			consignor.MainAddress.OA_Phone = "123";
			consignor.MainAddress.OA_PostCode = "3234";
			consignor.MainAddress.OA_State = "BOO";
			consignor.OH_IsConsignor = true;

			var consignorPickupAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, false);
			consignorPickupAddress.OA_Address1 = "Pickup addr1";
			consignorPickupAddress.OA_Address2 = "Pickup addr2";
			consignorPickupAddress.OA_City = "Pickup foocity";
			consignorPickupAddress.OA_Phone = "321";
			consignorPickupAddress.OA_PostCode = "4323";
			consignorPickupAddress.OA_State = "NSW";

			return consignor;
		}

		OrgHeader CreateConsignee()
		{
			var consignee = CreateValidOrgHeader("CONSIGNEE");
			consignee.OH_FullName = "TEST CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "1addr";
			consignee.MainAddress.OA_Address2 = "2addr";
			consignee.MainAddress.OA_City = "barcity";
			consignee.MainAddress.OA_Phone = "333";
			consignee.MainAddress.OA_Fax = "555";
			consignee.MainAddress.OA_PostCode = "6768";
			consignee.MainAddress.OA_State = "HEH";
			consignee.OH_IsConsignee = true;

			var consigneeDeliveryAddress = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			consigneeDeliveryAddress.OA_Address1 = "Delivery 1addr";
			consigneeDeliveryAddress.OA_Address2 = "Delivery 2addr";
			consigneeDeliveryAddress.OA_City = "Delivery barcity";
			consigneeDeliveryAddress.OA_Phone = "444";
			consigneeDeliveryAddress.OA_Fax = "666";
			consigneeDeliveryAddress.OA_PostCode = "8687";
			consigneeDeliveryAddress.OA_State = "ACT";

			return consignee;
		}

		OrgHeader CreateValidOrgHeader(ZString code)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = "45 Somewhere Over";
			result.MainAddress.OA_Address2 = "The Rainbow";
			result.MainAddress.OA_City = "Emerald City";
			result.MainAddress.OA_State = "OZ";
			result.MainAddress.OA_PostCode = "21290";

			return result;
		}

		RefContainer Get40FootGPContainer()
		{
			return Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
		}
	}
}
