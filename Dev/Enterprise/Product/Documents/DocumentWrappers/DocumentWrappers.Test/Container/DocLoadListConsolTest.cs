using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocLoadListConsol))]
	sealed class DocLoadListConsolTest : DocumentWrapperTestCase
	{
		public void TestLoadListTotalPackLineWeightAndVolume()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			CommonContainer container2 = Consol.Containers.AddNew();

			CommonShipment shipment = Consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;

			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.SetContainer(Consol, container1);
			line1.JL_ActualWeight = 8;
			line1.JL_ActualWeightUQ = Constants.Weight.Ounces;
			line1.JL_ActualVolume = 864;
			line1.JL_ActualVolumeUQ = Constants.Volume.CubicInches;

			PackLine line2 = shipment.OuterPackLines.AddNew();
			line2.SetContainer(Consol, container2);
			line2.JL_ActualWeight = 3;
			line2.JL_ActualWeightUQ = Constants.Weight.Pounds;
			line2.JL_ActualVolume = 2;
			line2.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;

			DocumentCommonConsol dcc = new DocumentCommonConsol(Consol, Constants.DataContext.Consol);
			dcc.IncludeAllShipments = true;
			LoadListConsolWrapper = DocLoadListConsol.New(dcc, Factory);

			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicFeet;
			Env.Registry.FreightWeightUnit = Constants.Weight.Pounds;

			AssertEquals("Weight", 3.5m, LoadListConsolWrapper.LoadListTotalPackLineWeight);
			AssertEquals("Weight Unit", Constants.Weight.Pounds, LoadListConsolWrapper.LoadListTotalPackLineWeightUnit);
			AssertEquals("Volume", 2.5m, LoadListConsolWrapper.LoadListTotalPackLineVolume);
			AssertEquals("Volume Unit", Constants.Volume.CubicFeet, LoadListConsolWrapper.LoadListTotalPackLineVolumeUnit);

			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;

			AssertEquals("Weight", 1.588m, LoadListConsolWrapper.LoadListTotalPackLineWeight.Round(3));
			AssertEquals("Weight Unit", Constants.Weight.Kilograms, LoadListConsolWrapper.LoadListTotalPackLineWeightUnit);
			AssertEquals("Volume", 0.071m, LoadListConsolWrapper.LoadListTotalPackLineVolume.Round(3));
			AssertEquals("Volume Unit", Constants.Volume.CubicMetres, LoadListConsolWrapper.LoadListTotalPackLineVolumeUnit);
		}

		#region Import Cargo Label

		public void TestStaticNewMethodWithDocumentCommonConsol()
		{
			DocLoadListConsol nullConsol = DocLoadListConsol.New((DocumentCommonConsol)null, Factory);
			AssertNull("Consol wrapper is null", nullConsol);

			var cFSLoadListConsol = Factory.New<CFSLoadListConsol>();
			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(cFSLoadListConsol, Core.Constants.DataContext.LoadListDocument);
			DocLoadListConsol consolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			AssertNotNull("Consol wrapper is not null", consolWrapper);
		}

		public void TestCommonConsolProperties()
		{
			var cFSLoadListConsol = Factory.New<CFSLoadListConsol>();
			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(cFSLoadListConsol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = true;
			DocLoadListConsol consolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			AssertEquals("Include Consignee", true, consolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", true, consolWrapper.IncludeConsignor);
			AssertEquals("Include Customs Broker", true, consolWrapper.IncludeCustomsBroker);
			AssertEquals("Include All Shipments", false, consolWrapper.IncludeAllShipments);
			AssertEquals("Include Packed", false, consolWrapper.IncludePacked);
			AssertEquals("Include UnPacked", true, consolWrapper.IncludeUnPacked);

			documentCommonConsol = new DocumentCommonConsol(cFSLoadListConsol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			consolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			AssertEquals("Include Consignee", true, consolWrapper.IncludeConsignee);
			AssertEquals("Include Consignor", true, consolWrapper.IncludeConsignor);
			AssertEquals("Include Customs Broker", true, consolWrapper.IncludeCustomsBroker);
			AssertEquals("Include All Shipments", true, consolWrapper.IncludeAllShipments);
			AssertEquals("Include Packed", false, consolWrapper.IncludePacked);
			AssertEquals("Include UnPacked", false, consolWrapper.IncludeUnPacked);
		}

		#endregion

		public void TestContainerRegos()
		{
			AssertEquals("No containers", 0, LoadListConsolWrapper.ContainerRegos.Count);

			Consol.Containers.AddNew();
			AssertEquals("One container", 1, LoadListConsolWrapper.ContainerRegos.Count);

			Consol.Containers.AddNew();
			AssertEquals("One container", 2, LoadListConsolWrapper.ContainerRegos.Count);
		}

		public void TestContext()
		{
			AssertEquals("CFSCONSOL", LoadListConsolWrapper.Context);
		}

		public void TestContainerInfoForInvoice()
		{
			AssertEquals("ContainerInfoForInvoice", ZString.Empty, LoadListConsolWrapper.ContainerInfoForInvoice);

			var rego = Consol.Containers.AddNew();
			rego.JC_ContainerJobID = "D1234";
			rego.JC_ContainerNum = "Con1";
			rego.JC_ContainerMode = "MMM";

			var containerCode = Factory.LoadTop1<RefContainer>(new ZQuery());
			rego.JC_RC = containerCode.PK;

			AssertEquals("ContainerInfoForInvoice", "D1234/CON1/MMM/" + containerCode.RC_Code, LoadListConsolWrapper.ContainerInfoForInvoice);
		}

		public void TestContainerInfoForInvoiceWithoutContainerCode()
		{
			AssertEquals("ContainerInfoForInvoice", ZString.Empty, LoadListConsolWrapper.ContainerInfoForInvoice);

			var rego = Consol.Containers.AddNew();
			rego.JC_ContainerJobID = "D1234";
			rego.JC_ContainerNum = "Con1";
			rego.JC_ContainerMode = "MMM";

			AssertEquals("ContainerInfoForInvoice", "D1234/CON1/MMM/NA", LoadListConsolWrapper.ContainerInfoForInvoice);
		}

		public void TestCurrentForwarder()
		{
			AssertNull(LoadListConsolWrapper.CurrentForwarder);
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG";
			Consol.JK_OH_Forwarder = org.PK;
			AssertEquals("TEST ORG", LoadListConsolWrapper.CurrentForwarder.Name);
		}

		public void TestAgentsReference()
		{
			AssertEquals("", LoadListConsolWrapper.AgentsReference);
			Consol.JK_AgentsReference = "AGENT REF 123";
			AssertEquals("AGENT REF 123", LoadListConsolWrapper.AgentsReference);
		}

		public void TestConsolNumber()
		{
			AssertEquals("", LoadListConsolWrapper.ConsolNumber);
			Consol.JK_UniqueConsignRef = "C000009999";
			AssertEquals("C000009999", LoadListConsolWrapper.ConsolNumber);
		}

		public void TestLoadListInstructionNote()
		{
			AssertEquals("", LoadListConsolWrapper.LoadListInstructions);
			var note = Consol.Notes.AddNew();
			note.ST_ParentID = Consol.PK;
			note.ST_Table = Consol.TableName;
			note.ST_Description = PredefinedNoteTypes.Instance.LoadListInstructions.Description;
			note.ST_NoteDataAsText = "Do not break stuff.\nCarry carefuly\n";

			var otherNote = Consol.Notes.AddNew();
			otherNote.ST_ParentID = Consol.PK;
			otherNote.ST_Table = Consol.TableName;
			otherNote.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			otherNote.ST_NoteDataAsText = "Should not be included.";

			AssertEquals("Do not break stuff.\nCarry carefuly", LoadListConsolWrapper.LoadListInstructions);
		}

		public void TestHandlingInstructions()
		{
			AssertEquals("", LoadListConsolWrapper.HandlingInstructions);
			var note = Consol.Notes.AddNew();
			note.ST_ParentID = Consol.PK;
			note.ST_Table = Consol.TableName;
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_NoteDataAsText = "Do not break stuff.\nCarry carefuly\n";
			AssertEquals("Do not break stuff.\nCarry carefuly", LoadListConsolWrapper.HandlingInstructions);
		}

		public void TestCartageInstructions()
		{
			string pickupDesc = PredefinedNoteTypes.Instance.PickupInstructionsNote.Description;
			string deliveryDesc = PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description;

			AssertEquals("", LoadListConsolWrapper.CartageInstructions);

			FreightHelperClass.AddNote(Consol, pickupDesc, "Consol Pickup Instructions");
			FreightHelperClass.AddNote(Consol, deliveryDesc, "Consol Delivery Instructions");

			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Consol Delivery Instructions", LoadListConsolWrapper.CartageInstructions);

			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Consol Pickup Instructions", LoadListConsolWrapper.CartageInstructions);
		}

		public void TestPackLines()
		{
			AssertEquals(0, LoadListConsolWrapper.PackLines.Count);

			SetSailing();
			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = currentUNLOCO;
			Origin.JA_RL_NKPortOfLoading = otherUNLOCO;

			CFSContainer importCont = Consol.Containers.AddNew();

			CFSShipment ship1 = Consol.Shipments.AddNew();
			ship1.JS_InterimReceipt = "S1234";
			ship1.JS_UniqueConsignRef = "S99991000";

			PackLine pack1 = ship1.OuterPackLines.AddNew();
			pack1.SetContainer(Consol, importCont);

			CFSShipment ship2 = Consol.Shipments.AddNew();
			ship2.JS_UniqueConsignRef = "S99991222";
			ship2.JS_InterimReceipt = "A1234";

			PackLine pack2 = ship2.OuterPackLines.AddNew();
			pack2.SetContainer(Consol, importCont);

			LoadListConsolWrapper = DocLoadListConsol.New(Consol, Factory);
			AssertEquals(2, LoadListConsolWrapper.PackLines.Count);

			var ship3 = Consol.Shipments.AddNew();
			PackLine pack3 = ship3.OuterPackLines.AddNew();
			PackLine pack4 = ship3.OuterPackLines.AddNew();
			ship3.JS_InterimReceipt = "B1234";
			ship3.JS_UniqueConsignRef = "S99991333";

			LoadListConsolWrapper = DocLoadListConsol.New(Consol, Factory);
			DocLoadListPackLineCollection coll = LoadListConsolWrapper.PackLines;
			AssertEquals(3, coll.Count);
			AssertEquals("A1234", coll[0].Shipment.InterimReceipt);
			AssertEquals("B1234", coll[1].Shipment.InterimReceipt);
			AssertEquals("S1234", coll[2].Shipment.InterimReceipt);
		}

		public void TestGroupPackLine()
		{
			var ship1 = Consol.Shipments.AddNew();
			var line1 = (PackLine)ship1.OuterPackLines.AddNew();
			var line2 = (PackLine)ship1.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 5M, "M3", 5, "BOX");

			PackLocation packLoc = line2.PackLocations.AddNew();
			packLoc.JQ_NoPackages = 5;
			packLoc.JQ_WarehouseLocation = "LOC 1";

			SetSailing();
			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = currentUNLOCO;
			Origin.JA_RL_NKPortOfLoading = otherUNLOCO;

			var importCont = Consol.Containers.AddNew();
			importCont.PackLines.Add(line1);
			importCont.PackLines.Add(line2);

			DocLoadListPackLineCollection result = LoadListConsolWrapper.PackLines;
			AssertEquals("Number of PackLines in the Collection", 1, result.Count);
			AssertEquals("Total volume of packlines", 15M, result[0].Volume);
			AssertEquals("Total weight of packlines", 305M, result[0].Weight);
			AssertEquals("Group Pack Type", "Packages", result[0].PackType);
			AssertEquals("Cargo locations", "LOC 1,  Packs: 5 BOX", result[0].CargoLocationAndPacks);
			Assert("Package Details", result[0].PackageDetails.Contains("5 Box 5000 G 5 M3"));
			Assert("Package Details", result[0].PackageDetails.Contains("10 Pallet 300 KG 10 M3"));
			Assert("IsGroupPackLine", result[0].IsGroupPackLine);
		}

		public void TestWeightVolumeOnSeparatePackLine()
		{
			SetSailing();
			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = otherUNLOCO;
			Origin.JA_RL_NKPortOfLoading = currentUNLOCO;

			var ship1 = Consol.Shipments.AddNew();
			ship1.JS_HouseBill = "HBL111";
			ship1.JS_InterimReceipt = "RCT1";
			ship1.JS_UniqueConsignRef = "S33452938";
			var line1 = ship1.OuterPackLines.AddNew();

			CFSShipment ship2 = Consol.Shipments.AddNew();
			ship2.JS_HouseBill = "HBL222";
			ship2.JS_InterimReceipt = "RCT2";
			ship2.JS_UniqueConsignRef = "S88882938";
			var line2 = ship2.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 5M, "M3", 5, "BOX");

			var exportCont = Consol.Containers.AddNew();
			exportCont.PackLines.Add(line1);
			exportCont.PackLines.Add(line2);

			DocLoadListPackLineCollection result = LoadListConsolWrapper.PackLines;
			AssertEquals("Number of PackLines in the Collection", 2, result.Count);
			AssertEquals("Volume on packline 1", 10M, result[0].Volume);
			AssertEquals("Weight on packline 1", 300M, result[0].Weight);
			AssertEquals("Package Details", "10 Pallet 300 KG 10 M3", result[0].PackageDetails);
			AssertEquals("Group Pack Type", "Pallet", result[0].PackType);
			AssertEquals("IsGroupPackLine", ZBool.False, result[0].IsGroupPackLine);

			AssertEquals("Volume on packline 2", 5M, result[1].Volume);
			AssertEquals("Weight on packline 2", 5M, result[1].Weight);
			AssertEquals("Package Details", "5 Box 5000 G 5 M3", result[1].PackageDetails);
			AssertEquals("Group Pack Type", "Box", result[1].PackType);
			AssertEquals("IsGroupPackLine", ZBool.False, result[1].IsGroupPackLine);
		}

		public void TestDimensionsOnPackLineCollection()
		{
			SetSailing();
			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = otherUNLOCO;
			Origin.JA_RL_NKPortOfLoading = currentUNLOCO;

			var ship1 = Consol.Shipments.AddNew();
			ship1.JS_HouseBill = "HBL111";
			ship1.JS_InterimReceipt = "RCT1";
			ship1.JS_UniqueConsignRef = "S88882938";
			var line1 = ship1.OuterPackLines.AddNew();

			var ship2 = Consol.Shipments.AddNew();
			ship2.JS_HouseBill = "HBL222";
			ship2.JS_InterimReceipt = "RCT2";
			ship2.JS_UniqueConsignRef = "S88883927";
			var line2 = (PackLine)ship2.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 60M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 80M, "M3", 5, "BOX");
			SetPackLineLengthWidhtHeight(line1, 10M, 2M, 3M, "M");
			SetPackLineLengthWidhtHeight(line2, 1M, 20M, 4M, "M");

			var exportCont = Consol.Containers.AddNew();
			exportCont.PackLines.Add(line1);
			exportCont.PackLines.Add(line2);

			DocLoadListPackLineCollection result = LoadListConsolWrapper.PackLines;
			AssertEquals("Number of PackLines in the Collection", 2, result.Count);
			AssertEquals("Package & Dimension Details for Pack1", "10 Pallet 300 KG 600 M3\n   (L): 10  (W): 2  (H): 3 M", result[0].PackageDetails);
			AssertEquals("Package & Dimension Details for Pack2", "5 Box 5000 G 400 M3\n   (L): 1  (W): 20  (H): 4 M", result[1].PackageDetails);
		}

		public void TestDimensionOneShipmentManyPackLines()
		{
			SetSailing();
			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = otherUNLOCO;
			Origin.JA_RL_NKPortOfLoading = currentUNLOCO;

			var ship1 = Consol.Shipments.AddNew();
			ship1.JS_HouseBill = "HBL111";
			ship1.JS_UniqueConsignRef = "S88823938";
			var line1 = (PackLine)ship1.OuterPackLines.AddNew();
			var line2 = (PackLine)ship1.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineWeightVolume(line2, 5000M, "G", 80M, "M3", 5, "BOX");
			SetPackLineLengthWidhtHeight(line1, 10M, 0M, 0M, "M");
			SetPackLineLengthWidhtHeight(line2, 1M, 20M, 4M, "M");

			DocLoadListPackLineCollection result = LoadListConsolWrapper.PackLines;
			AssertEquals("Number of PackLines in the Collection", 1, result.Count);

			ZString expected = "10 Pallet 300 KG 10 M3\n   (L): 10  (W): 0  (H): 0 M\n5 Box 5000 G 400 M3\n   (L): 1  (W): 20  (H): 4 M";
			AssertEquals("Package & Dimension Details", expected, result[0].PackageDetails);
		}

		public void TestDimensionOneShipmentWithDimensions()
		{
			SetSailing();
			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = otherUNLOCO;
			Origin.JA_RL_NKPortOfLoading = currentUNLOCO;

			var ship1 = Consol.Shipments.AddNew();
			ship1.JS_HouseBill = "HBL111";
			ship1.JS_UniqueConsignRef = "S82482938";
			var line1 = (PackLine)ship1.OuterPackLines.AddNew();

			SetPackLineWeightVolume(line1, 300M, "KG", 10M, "M3", 10, "PLT");
			SetPackLineLengthWidhtHeight(line1, 10M, 0M, 0M, "M");

			DocLoadListPackLineCollection result = LoadListConsolWrapper.PackLines;
			AssertEquals("(L): 10  (W): 0  (H): 0 M", result[0].PackageDetailsAndHandlingInstructionNote);
		}

		public void TestShipmentReceivals()
		{
			AssertEquals(0, LoadListConsolWrapper.ShipmentReceivals.Count);

			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			AssertEquals(3, LoadListConsolWrapper.ShipmentReceivals.Count);
		}

		public void TestContainersPackLine()
		{
			AssertEquals(0, LoadListConsolWrapper.ContainersPackLine.Count);
			SetSailing();

			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = currentUNLOCO;
			Origin.JA_RL_NKPortOfLoading = otherUNLOCO;

			var importCont = Consol.Containers.AddNew();
			CFSShipment shipment = Consol.Shipments.AddNew();
			PackLine pack1 = shipment.OuterPackLines.AddNew();
			pack1.SetContainer(Consol, importCont);

			PackLine pack3 = shipment.OuterPackLines.AddNew();
			pack3.SetContainer(Consol, importCont);

			PackLine pack2 = shipment.OuterPackLines.AddNew();
			pack2.SetContainer(Consol, importCont);

			LoadListConsolWrapper = DocLoadListConsol.New(Consol, Factory);
			AssertEquals(3, LoadListConsolWrapper.ContainersPackLine.Count);
		}

		public void TestPackLinesForLoadList_UseFreightWeightUnit()
		{
			SetSailing();
			CFSShipment shipment = Consol.Shipments.AddNew();
			var container1 = Consol.Containers.AddNew();
			var container2 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "1";
			container2.JC_ContainerNum = "2";

			CFSPackLine packKG = shipment.OuterPackLines.AddNew();
			CFSPackLine packLB = shipment.OuterPackLines.AddNew();
			CFSPackLine unPackLB = shipment.OuterPackLines.AddNew();

			container1.AddPackLine(packKG);
			container2.AddPackLine(packLB);

			SetPackLineWeightVolume(packKG, 50, "KG", 51, "M3", 1, "PKG");
			SetPackLineWeightVolume(packLB, 50, "LB", 51, "CF", 1, "PKG");
			SetPackLineWeightVolume(unPackLB, 50, "LB", 51, "CF", 1, "PKG");

			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			DocLoadListConsol loadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);

			DocLoadListPackLineCollection packLines = loadListConsolWrapper.PackLinesForLoadList;
			AssertEquals(3, packLines.Count);

			foreach (DocLoadListPackLine docPackline in packLines)
			{
				AssertEquals("All packlines should total in Freight Weight Unit", loadListConsolWrapper.WeightUnit, docPackline.ContainerTotalManifestWeightUnit);
				AssertEquals("All packlines should total in Freight Weight Volume", loadListConsolWrapper.VolumeUnit, docPackline.ContainerTotalManifestVolumeUnit);
			}
		}

		public void TestPackLinesForLoadList()
		{
			AssertEquals(0, LoadListConsolWrapper.PackLinesForLoadList.Count);
			SetSailing();

			ZString currentUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString otherUNLOCO = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, currentUNLOCO))).RL_Code;
			Destination.JB_RL_NKPortOfDischarge = currentUNLOCO;
			Origin.JA_RL_NKPortOfLoading = otherUNLOCO;

			var container = Consol.Containers.AddNew();
			container.JC_JK = Consol.PK;
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			CFSPackLine unPack = shipment.OuterPackLines.AddNew();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();

			consignor.OH_FullName = "Consignor";
			consignee.OH_FullName = "Consignee";
			exportBroker.OH_FullName = "ExportBroker";
			importBroker.OH_FullName = "ImportBroker";

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_OH_ExportBroker = exportBroker.PK;
			shipment.JS_OH_ImportBroker = importBroker.PK;

			container.AddPackLine(pack);

			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			LoadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			AssertEquals(0, LoadListConsolWrapper.PackLinesForLoadList.Count);

			documentCommonConsol.IncludePacked = true;
			documentCommonConsol.IncludeUnPacked = false;
			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludeConsignee = false;
			documentCommonConsol.IncludeConsignor = false;
			documentCommonConsol.IncludeCustomsBroker = false;
			LoadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(1, LoadListConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("ConsignorAndConsignee", ZString.Empty, LoadListConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);
			AssertEquals("CustomsBroker", ZString.Empty, LoadListConsolWrapper.PackLinesForLoadList[0].CustomsBrokerForLoadList);

			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			LoadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(2, LoadListConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("ConsignorAndConsignee", "Consignor" + System.Environment.NewLine + "Consignee", LoadListConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);
			AssertEquals("CustomsBroker", "Broker: ExportBroker", LoadListConsolWrapper.PackLinesForLoadList[0].CustomsBrokerForLoadList);

			documentCommonConsol.IncludeAllShipments = false;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = true;
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = false;
			documentCommonConsol.IncludeCustomsBroker = true;
			LoadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals(1, LoadListConsolWrapper.PackLinesForLoadList.Count);
			AssertEquals("ConsignorAndConsignee", "Consignee", LoadListConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);
			AssertEquals("CustomsBroker", "Broker: ImportBroker", LoadListConsolWrapper.PackLinesForLoadList[0].CustomsBrokerForLoadList);

			shipment.ConsigneePK = Guid.Empty;
			shipment.ConsignorPK = Guid.Empty;
			shipment.JS_OH_ExportBroker = Guid.Empty;
			shipment.JS_OH_ImportBroker = Guid.Empty;

			documentCommonConsol.IncludeAllShipments = true;
			documentCommonConsol.IncludePacked = false;
			documentCommonConsol.IncludeUnPacked = false;
			documentCommonConsol.IncludeConsignee = true;
			documentCommonConsol.IncludeConsignor = true;
			documentCommonConsol.IncludeCustomsBroker = true;
			LoadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ConsignorAndConsignee", System.Environment.NewLine, LoadListConsolWrapper.PackLinesForLoadList[0].ConsignorAndConsigneeForLoadList);
		}

		public void TestPackLinesForLoadListMerging()
		{
			AssertEquals(0, LoadListConsolWrapper.PackLinesForLoadList.Count);
			SetSailing();

			var container = Consol.Containers.AddNew();
			container.JC_JK = Consol.PK;
			var shipment = Consol.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Now;
			CFSPackLine pack = shipment.OuterPackLines.AddNew();
			CFSPackLine pack2 = shipment.OuterPackLines.AddNew();
			CFSPackLine unPack = shipment.OuterPackLines.AddNew();

			container.AddPackLine(pack);
			container.AddPackLine(pack2);

			pack2.JL_ContainerPackingOrder = 1;
			pack.JL_ContainerPackingOrder = 3;

			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(Consol, Core.Constants.DataContext.LoadListDocument);
			documentCommonConsol.IncludePacked = true;
			documentCommonConsol.IncludeUnPacked = true;
			documentCommonConsol.IncludeAllShipments = true;
			LoadListConsolWrapper = DocLoadListConsol.New(documentCommonConsol, Factory);
			LoadListConsolWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals(2, LoadListConsolWrapper.PackLinesForLoadList.Count);

			LoadListConsolWrapper.SetReportNameForTesting("Anything DetAiLed");
			DocLoadListPackLineCollection result = LoadListConsolWrapper.PackLinesForLoadList;
			AssertEquals(3, result.Count);
			AssertEquals(0, result[0].ContainerPackingOrder);
			AssertEquals(1, result[1].ContainerPackingOrder);
			AssertEquals(3, result[2].ContainerPackingOrder);
		}

		public void TestWarehouseLocations()
		{
			AssertEquals(0, LoadListConsolWrapper.WarehouseLocations.Count);

			CommonShipment shipment1 = Consol.Shipments.AddNew();
			CommonShipment shipment2 = Consol.Shipments.AddNew();

			AssertEquals(0, LoadListConsolWrapper.WarehouseLocations.Count);

			shipment1.OuterPackLines.AddNew();
			shipment1.OuterPackLines.AddNew();
			shipment1.OuterPackLines.AddNew();
			shipment1.OuterPackLines[0].PackLocations.AddNew();
			shipment1.OuterPackLines[1].PackLocations.AddNew();
			shipment1.OuterPackLines[2].PackLocations.AddNew();

			shipment2.OuterPackLines.AddNew();
			shipment2.OuterPackLines[0].PackLocations.AddNew();
			shipment2.OuterPackLines[0].PackLocations.AddNew();

			AssertEquals(5, LoadListConsolWrapper.WarehouseLocations.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			Consol = Factory.New<CFSLoadListConsol>();
			LoadListConsolWrapper = DocLoadListConsol.New(Consol, Factory);
			base.SetUp();
		}

		void SetSailing()
		{
			Sailing = Factory.New<JobSailing>();
			Voyage = Factory.New<JobVoyage>();
			Origin = Factory.New<VoyageOrigin>();
			Destination = Factory.New<VoyageDestination>();
			Vessel = Factory.New<RefVessel>();

			Vessel.RV_Name = "TEST VESSEL";
			Voyage.JV_RV_NKVessel = Vessel.RV_FK;
			Voyage.JV_VoyageFlight = "TEST VOY";
			Origin.JA_JV = Voyage.PK;
			Destination.JB_JV = Voyage.PK;
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;

			Transport transport = Consol.Transports[0];
			transport.JW_JX = Sailing.PK;
		}

		void SetPackLineWeightVolume(PackLine line, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ, ZInt package, ZString type)
		{
			line.JL_ActualVolume = volume;
			line.JL_ActualVolumeUQ = volumeUQ;
			line.JL_ActualWeight = weight;
			line.JL_ActualWeightUQ = weightUQ;
			line.JL_PackageCount = package;
			line.JL_F3_NKPackType = type;
		}

		void SetPackLineLengthWidhtHeight(PackLine line, ZDecimal length, ZDecimal width, ZDecimal height, ZString unitOfDimension)
		{
			line.JL_Length = length;
			line.JL_Width = width;
			line.JL_Height = height;
			line.JL_UnitOfDimension = unitOfDimension;
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocLoadListConsol.New(Consol, Factory) };
		}

		DocLoadListConsol LoadListConsolWrapper;
		CFSLoadListConsol Consol;
		JobSailing Sailing;
		JobVoyage Voyage;
		VoyageOrigin Origin;
		VoyageDestination Destination;
		RefVessel Vessel;

		#endregion
	}
}
