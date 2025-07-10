using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class PivotSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniser()
		{
			var consol = CreateFCLConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = TestContainerNumber;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 3;
			var packline1 = shipment.OuterPackLines[0];
			packline1.SetContainer(consol, container);
			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.SetContainer(consol, container);

			var seaSynchroniser = GetSeaCargoSynchroniser(consol);
			var oceanBill = seaSynchroniser.OceanBill;
			var houseBill = oceanBill.HouseBills[0];
			var obContainer = oceanBill.Containers.Find(c => c.CN_ContainerNumber == TestContainerNumber).First();
			var pivot = houseBill.Pivot.Find(p => p.CV_CN == obContainer.PK).First();

			shipment.JS_GoodsDescription = TestGoodsDescription;
			AssertEquals("Pivot Goods Description", TestGoodsDescription, pivot.CV_GoodsDescription);

			packline1.JL_F3_NKPackType = Test3CharPackingType;
			AssertEquals("Multiple Packing Types", CMRPackageTypes.Codes.UnpackedOrPacked, pivot.CV_PackageType);
			packline2.JL_F3_NKPackType = Test3CharPackingType;
			AssertEquals("Pivot PackingType", Test2CharPackingType, pivot.CV_PackageType);
			packline1.JL_PackageCount = TestPackageCount;
			AssertEquals("Pivot PackageCount", TestPackageCount, pivot.CV_PackageCount);

			packline1.JL_ActualWeight = 123m;
			AssertEquals("Pivot Weight (default KG when UQ not set)", 123m, pivot.CV_Weight);
			AssertEquals("Pivot Weight UQ (default KG when not set on Packlines)", Enterprise.Core.Constants.Weight.Kilograms, pivot.CV_WeightUQ);
			packline1.JL_ActualWeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			AssertEquals("Pivot Weight UQ", Enterprise.Core.Constants.Weight.ShortTons, pivot.CV_WeightUQ);
			AssertEquals("Pivot Weight (Ton)", 123m, pivot.CV_Weight);

			packline2.JL_ActualWeight = 456m;
			AssertEquals("Pivot Weight (default KG when UQ not set)", 123.456m, pivot.CV_Weight);
			AssertEquals("Pivot Weight UQ (from first Packline)", Enterprise.Core.Constants.Weight.ShortTons, pivot.CV_WeightUQ);
			packline2.JL_ActualWeightUQ = Enterprise.Core.Constants.Weight.Tonnes;
			AssertEquals("Pivot Weight (Ton)", 579m, pivot.CV_Weight);

			packline1.JL_ActualVolume = TestVolume;
			AssertEquals("Pivot Volume - Assumed UQ M3 when not set", TestVolume, pivot.CV_Volume);
			packline1.JL_ActualVolumeUQ = TestVolumeUQNotM3;
			ZDecimal volumeInM3 = Enterprise.Core.Constants.Volume.Convert(packline1.JL_ActualVolume, TestVolumeUQNotM3, Enterprise.Core.Constants.Volume.CubicMetres);
			volumeInM3 = Math.Round(volumeInM3, 3);
			AssertEquals("Pivot Volume - Converted To M3", volumeInM3, pivot.CV_Volume);
		}

		public void TestUQ_T_ConvertedToUQ_TN()
		{
			var consol = CreateFCLConsol();
			var addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 3;
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer);

			var scaSynchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = scaSynchroniser.GetHouseBill(shipment);
			var pivot = Factory.New<CusSCAPivot>();

			var testSynchroniser = GetPivotSynchroniser(pivot, shipment.OuterPackLines[0]);
			testSynchroniser.HouseBill = houseBill;
			testSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.JS_GoodsDescription = TestGoodsDescription;
			shipment.OuterPackLines[0].JL_F3_NKPackType = Test3CharPackingType;
			shipment.OuterPackLines[0].JL_PackageCount = TestPackageCount;
			shipment.OuterPackLines[0].JL_ActualWeight = 3.5m;
			shipment.OuterPackLines[0].JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Pivot Weight UQ should be converted to 'TN'", Core.Constants.Weight.ShortTons, pivot.CV_WeightUQ);
			AssertEquals("Pivot Weight should not be converted - the issue is just a UQ code mismatch - it is still tonnes", 3.5m, pivot.CV_Weight);
		}

		public void TestConvertedUQWhenOverflow()
		{
			var consol = CreateFCLConsol();
			var addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 3;
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer);

			var scaSynchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = scaSynchroniser.GetHouseBill(shipment);
			var pivot = Factory.New<CusSCAPivot>();

			var testSynchroniser = GetPivotSynchroniser(pivot, shipment.OuterPackLines[0]);
			testSynchroniser.HouseBill = houseBill;
			testSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			shipment.JS_GoodsDescription = TestGoodsDescription;
			shipment.OuterPackLines[0].JL_F3_NKPackType = Test3CharPackingType;
			shipment.OuterPackLines[0].JL_PackageCount = TestPackageCount;
			shipment.OuterPackLines[0].JL_ActualWeight = 1300000m;
			shipment.OuterPackLines[0].JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Pivot Weight UQ should be converted to 'TN'", Core.Constants.Weight.ShortTons, pivot.CV_WeightUQ);
			AssertEquals("Pivot Weight should be converted", 1300m, pivot.CV_Weight);

			shipment.OuterPackLines[0].JL_ActualWeight = 2600000m;
			shipment.OuterPackLines[0].JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Pivot Weight UQ should be converted to 'KT'", Core.Constants.Weight.Kilotonnes, pivot.CV_WeightUQ);
			AssertEquals("Pivot Weight should be converted", 2600m, pivot.CV_Weight);

			shipment.OuterPackLines[0].JL_ActualWeight = 3900000000m;
			shipment.OuterPackLines[0].JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("Pivot Weight UQ should be converted to 'KT'", Core.Constants.Weight.Kilotonnes, pivot.CV_WeightUQ);
			AssertEquals("Pivot Weight should be converted", 3900m, pivot.CV_Weight);
		}

		public void TestSynchroniseLongGoodDescription()
		{
			ForwardingConsol consol = CreateFCLConsol();

			CommonContainer addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 3;
			shipment.JS_GoodsDescription = "Short Description";
			shipment.DetailedGoodsDescriptionNoteText = LongDescription;

			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse house = synchroniser.GetHouseBill(shipment);
			CusSCAPivot pivot = house.Pivot[0];

			AssertEquals("Pivot Goods Description", LongDescription, pivot.CV_GoodsDescription);

			shipment.DetailedGoodsDescriptionNoteText = ChangedDescription;
			AssertEquals("Pivot Goods Description", ChangedDescription, pivot.CV_GoodsDescription);
		}

		public void TestSynchroniseMarksAndNumbers()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 3;
			shipment.JS_MarksAndNumbersShort = LongDescription.Substring(0, 35);
			StmNote[] marksAndNumbersNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
			StmNote marksAndNumbersNote = null;
			if (marksAndNumbersNotes.Length > 0)
			{
				marksAndNumbersNote = marksAndNumbersNotes[0];
			}

			marksAndNumbersNote.ST_NoteText = LongDescription;
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse house = synchroniser.GetHouseBill(shipment);
			CusSCAPivot pivot = house.Pivot[0];
			AssertEquals("Pivot Marks and Numbers", LongDescription, pivot.CV_MarksAndNumbers);
			shipment.JS_MarksAndNumbersShort = ChangedDescription.Substring(0, 35);
			Assert("Pivot Marks and Numbers", pivot.CV_MarksAndNumbers.Contains(ChangedDescription.Substring(0, 35)));
			marksAndNumbersNote.ST_NoteText = LongDescription;
			AssertEquals("Pivot Marks and Numbers", LongDescription, pivot.CV_MarksAndNumbers);
		}

		public void TestSynchroniserWhenDeletingSCAPivot()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 3;
			shipment.OuterPackLines[0].JL_F3_NKPackType = "PKG";
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer);

			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = sCASynchroniser.OceanBill;

			CusSCAHouse houseBill = sCASynchroniser.GetHouseBill(shipment);
			shipment.OuterPackLines[0].JL_PackageCount = 30;
			AssertEquals("Pivot Package Count", 30, houseBill.Pivot[0].CV_PackageCount);
			houseBill.Pivot.RemoveAndDeleteAll();

			shipment.OuterPackLines[0].JL_PackageCount = 40;
			AssertEquals("Pivot should not exist", 0, houseBill.Pivot.Count);
		}

		public void TestMarksAndNumberSynchronisation()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TotalPackageCount = 10;
			PackLine newPackLine = shipment.OuterPackLines.AddNew();
			newPackLine.SetContainer(addedContainer.PK);

			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse houseBill = sCASynchroniser.GetHouseBill(shipment);
			CusSCAPivot pivot = houseBill.Pivot[0];

			StmNote note = shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = TestMarksAndNumbers;
			AssertEquals("Pivot Marks And Numbers", TestMarksAndNumbers.ToUpper(), pivot.CV_MarksAndNumbers);

			shipment.JS_MarksAndNumbers = TestMarksAndNumbers2;
			AssertEquals("Pivot Marks And Numbers", TestMarksAndNumbers2.ToUpper(), pivot.CV_MarksAndNumbers);
		}

		public void TestMarksAndNumberSynchronisationForFCLContainers()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine newPackLine = shipment.OuterPackLines.AddNew();
			newPackLine.SetContainer(addedContainer.PK);

			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse houseBill = sCASynchroniser.GetHouseBill(shipment);
			CusSCAPivot pivot = houseBill.Pivot[0];
			AssertEquals("Marks and Numbers should default to the container number when FCL", TestContainerNumber, pivot.CV_MarksAndNumbers);
		}

		public void TestDestinationDeletedEvent()
		{
			PackLine packLine = Factory.New<PackLine>();
			CusSCAPivot pivot = Factory.New<CusSCAPivot>();
			PivotSynchroniser testSynchroniser = GetPivotSynchroniser(pivot, packLine);
			testSynchroniser.DestinationDeleted += new EventHandler(TestSynchroniser_DestinationDeleted);
			destinationDeletedCalled = false;
			pivot.Delete();
			AssertEquals("Failed to call destination Deleted Event", true, destinationDeletedCalled);
		}

		public void TestSynchroniserReadOnlyState()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer addedContainer = consol.Containers.AddNew();
			addedContainer.JC_ContainerNum = TestContainerNumber;
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine newPackLine = shipment.OuterPackLines.AddNew();
			newPackLine.SetContainer(addedContainer.PK);

			SeaCargoSynchroniser sCASynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAHouse houseBill = sCASynchroniser.GetHouseBill(shipment);
			CusSCAPivot pivot = houseBill.Pivot[0];

			var oceanBill = houseBill.OceanBill;
			Assert(!oceanBill.OverrideFreightDefaults);
			AssertEquals("ReadOnly CV_GoodsDescriptionInfo", true, pivot.CV_GoodsDescriptionInfo.ReadOnly);
			AssertEquals("ReadOnly CV_MarksAndNumbersInfo", true, pivot.CV_MarksAndNumbersInfo.ReadOnly);
			AssertEquals("ReadOnly CV_PackageCountInfo", true, pivot.CV_PackageCountInfo.ReadOnly);
			AssertEquals("ReadOnly CV_WeightInfo", true, pivot.CV_WeightInfo.ReadOnly);
			AssertEquals("ReadOnly CV_VolumeInfo", true, pivot.CV_VolumeInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals("ReadOnly CV_GoodsDescriptionInfo", false, pivot.CV_GoodsDescriptionInfo.ReadOnly);
			AssertEquals("ReadOnly CV_MarksAndNumbersInfo", false, pivot.CV_MarksAndNumbersInfo.ReadOnly);
			AssertEquals("ReadOnly CV_PackageCountInfo", false, pivot.CV_PackageCountInfo.ReadOnly);
			AssertEquals("ReadOnly CV_WeightInfo", false, pivot.CV_WeightInfo.ReadOnly);
			AssertEquals("ReadOnly CV_VolumeInfo", false, pivot.CV_VolumeInfo.ReadOnly);
		}

		bool destinationDeletedCalled;
		void TestSynchroniser_DestinationDeleted(object sender, EventArgs e)
		{
			destinationDeletedCalled = true;
		}

		public void TestWatchingAdditionalPackLines()
		{
			var consol2 = Factory.New<ForwardingConsol>();
			var con2Container = consol2.Containers.AddNew();
			con2Container.JC_ContainerNum = "XXXU1231230";

			ForwardingConsol consol = CreateGroupageConsol();
			Transport transport = consol.Transports[0];
			transport.JW_JX = ImportSailing.PK;

			CommonContainer groupageContainer = consol.Containers.AddNew();
			groupageContainer.JC_ContainerNum = "XXXU9999990";
			CommonShipment shipment = consol.Shipments.AddNew();
			consol2.Shipments.Add(shipment);
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualVolume = .4m;

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;
			var pivot = houseBill.Pivot.AddNew();
			PivotSynchroniser testSynchroniser = GetPivotSynchroniser(pivot, packLine1);
			testSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			AssertEquals("Pivot Package Count", 10, pivot.CV_PackageCount);

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			testSynchroniser.AddPackLineWatch(packLine2);
			packLine2.JL_F3_NKPackType = "PKG";
			packLine2.JL_ActualWeight = 200m;
			packLine2.JL_ActualVolume = .5m;
			AssertEquals("Pivot Package Count", 10, pivot.CV_PackageCount);
			AssertEquals("Pivot Package Type", "PK", pivot.CV_PackageType);
			AssertEquals("Pivot Gross Weight", 300m, pivot.CV_Weight);
			AssertEquals("Pivot Net Weight", 300m, pivot.CV_NetWeight);
			AssertEquals("Pivot VOlume", .9m, pivot.CV_Volume);
			packLine2.JL_PackageCount = 20;
			packLine2.JL_F3_NKPackType = "BOX";
			AssertEquals("Synchroniser Watching PackLine", true, testSynchroniser.IsWatchingPackLine(packLine2));
			AssertEquals("Pivot Package Count", 30, pivot.CV_PackageCount);
			AssertEquals("Pivot Package Type", expectedUnpackedOrPackedCode, pivot.CV_PackageType);
			packLine2.JL_F3_NKPackType = "PKG";
			AssertEquals("Pivot Package Type", "PK", pivot.CV_PackageType);
			packLine3.JL_PackageCount = 30;
			AssertEquals("Pivot Package Count", 30, pivot.CV_PackageCount);
			packLine3.JL_F3_NKPackType = "BOX";
			testSynchroniser.AddPackLineWatch(packLine3);
			AssertEquals("Pivot Package Count", 60, pivot.CV_PackageCount);
			AssertEquals("Pivot Package Type", expectedUnpackedOrPackedCode, pivot.CV_PackageType);
			packLine3.JL_F3_NKPackType = "PKG";
			AssertEquals("Pivot Package Type", "PK", pivot.CV_PackageType);
			packLine2.Delete();
			testSynchroniser.RemovePackLineWatch(packLine2);
			AssertEquals("Pivot Package Count", 40, pivot.CV_PackageCount);
			AssertEquals("Pivot Package Type", "PK", pivot.CV_PackageType);
			packLine1.JL_F3_NKPackType = "BOX";
			AssertEquals("Pivot Package Type", expectedUnpackedOrPackedCode, pivot.CV_PackageType);
			AssertEquals("Synchroniser Watching PackLine", false, testSynchroniser.IsWatchingPackLine(packLine2));
			testSynchroniser.RemovePackLineWatch(packLine3);
			AssertEquals("Pivot Package Count", 10, pivot.CV_PackageCount);
			AssertEquals("Pivot Package Type", "BX", pivot.CV_PackageType);
			AssertEquals("Synchroniser Watching PackLine", false, testSynchroniser.IsWatchingPackLine(packLine3));
		}

		public void TestMarksAndNumberSynchronisationForLCLContainers()
		{
			var consol = CreateFCLConsol();
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT0000022";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_HouseBill = "IANTESTHB1";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);

			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.GetHouseBill(shipment);
			var pivot = houseBill.Pivot[0];
			AssertEquals(ZString.Empty, pivot.CV_MarksAndNumbers);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			synchroniser.Synchronise(SynchroniseAction.Force);
			AssertEquals("CONT0000022/IANTESTHB1", pivot.CV_MarksAndNumbers);
		}

		public void TestPacklineMoveContainer()
		{
			var consol = CreateFCLConsol();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "TEST0000011";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "TEST0000022";

			var shipment = consol.Shipments.AddNew();
			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "BOX";
			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 20;
			line2.JL_F3_NKPackType = "PKG";

			line1.SetContainer(consol, container1);
			line2.SetContainer(consol, container2);
			AssertEquals("Packline 1 Container", container1, line1.GetContainer(consol));
			AssertEquals("Packline 2 Container", container2, line2.GetContainer(consol));
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var house = seaCargoSynchroniser.GetHouseBill(shipment);
			AssertEquals("There should be 2 containers and 2 pivots", 2, house.Pivot.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var pivot = newFactory.Load<CusSCAPivot>(house.Pivot[0].PK);
			var packLine = newFactory.Load<PackLine>(line1.PK);
			var houseBill = pivot.HouseBill;
			var houseBillSynchroniser = new CMRHouseBillSynchroniser(houseBill, houseBill.Shipment);
			var pivotSynchroniser = new CMRPivotSynchroniser(houseBillSynchroniser, pivot, packLine, shipment);
			pivotSynchroniser.SetEnabled(true, false);
			packLine.SetContainer(consol, container2);
			Assert(pivot.IsDeleted);
			AssertEquals(1, houseBill.Pivot.Count);

			var cmrPivot = houseBill.Pivot[0];
			AssertEquals("TEST0000022", cmrPivot.CV_AssociatedContainer);
		}

		public void TestDeletedPacklineIsSynchronisedByRefreshBus()
		{
			var consol = CreateFCLConsol();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "TEST0000011";
			var shipment = consol.Shipments.AddNew();
			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "BOX";
			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 20;
			line2.JL_F3_NKPackType = "PKG";
			line1.SetContainer(consol, container1);
			line2.SetContainer(consol, container1);
			AssertEquals("Packline 1 Container", container1, line1.GetContainer(consol));
			AssertEquals("Packline 2 Container", container1, line2.GetContainer(consol));
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var house = seaCargoSynchroniser.GetHouseBill(shipment);
			AssertEquals("There should be just one pivot", 1, house.Pivot.Count);

			var houseSynch = seaCargoSynchroniser.BusinessObjectSynchronisers.OfType<HouseBillSynchroniser>().FirstOrDefault();
			var pivotSynch = houseSynch.PivotSynchronisers.FirstOrDefault();
			Assert("Line2 is a watched line", pivotSynch.IsWatchingPackLine(line2));
			Factory.Save();

			bool hasChangesChanged = false;
			house.HasChangesChanged += (object sender, HasChangesChangedEventArgs e) =>
			{
				if (e.ObjectThatWasChanged is CusSCAPivot)
				{
					hasChangesChanged = true;
				}
			};

			var newFactory = new BusinessObjectFactory();
			var pivot = newFactory.Load<CusSCAPivot>(house.Pivot[0].PK);
			var pivotPackLine = newFactory.Load<PackLine>(line1.PK);
			var watchedPackLine = newFactory.Load<PackLine>(line2.PK);
			var houseBill = pivot.HouseBill;
			var houseBillSynchroniser = new CMRHouseBillSynchroniser(houseBill, houseBill.Shipment);
			houseBillSynchroniser.SetEnabled(true, false);
			Assert("Line2 is a watched line", houseBillSynchroniser.PivotSynchronisers.FirstOrDefault().IsWatchingPackLine(watchedPackLine));

			watchedPackLine.Delete();
			Assert(!pivot.IsDeleted);
			AssertEquals(1, houseBill.Pivot.Count);
			Assert(!consol.HasChanges);
			newFactory.Save();

			AssertEquals(1, house.Pivot.Count);
			Assert(!consol.HasChanges);
			Assert("Does not trigger Has Changes Changed / Save remains disabled on the form", !hasChangesChanged);

			pivotPackLine.Delete();
			Assert(pivot.IsDeleted);
			AssertEquals(0, houseBill.Pivot.Count);
			Assert(!consol.HasChanges);
			newFactory.Save();

			AssertEquals(0, house.Pivot.Count);
			Assert("Does not trigger Has Changes Changed / Save remains disabled on the form", !hasChangesChanged);
			Assert(!consol.HasChanges);
		}

		public void TestDeletedContainerIsSynchronisedByRefreshBus()
		{
			var consol = CreateFCLConsol();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "TEST0000011";
			var shipment = consol.Shipments.AddNew();
			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "BOX";
			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 20;
			line2.JL_F3_NKPackType = "PKG";
			line1.SetContainer(consol, container1);
			line2.SetContainer(consol, container1);
			AssertEquals("Packline 1 Container", container1, line1.GetContainer(consol));
			AssertEquals("Packline 2 Container", container1, line2.GetContainer(consol));
			var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
			var house = seaCargoSynchroniser.GetHouseBill(shipment);
			AssertEquals("There should be just one pivot", 1, house.Pivot.Count);

			var houseSynch = seaCargoSynchroniser.BusinessObjectSynchronisers.OfType<HouseBillSynchroniser>().FirstOrDefault();
			var pivotSynch = houseSynch.PivotSynchronisers.FirstOrDefault();
			Assert("Line2 is a watched line", pivotSynch.IsWatchingPackLine(line2));
			Factory.Save();

			bool hasChangesChanged = false;
			pivotSynch.Destination.HasChangesChanged += (object sender, HasChangesChangedEventArgs e) =>
			{
				if (e.ObjectThatWasChanged is CusSCAPivot p && !p.IsDeleted)
				{
					hasChangesChanged = true;
				}
			};

			var newFactory = new BusinessObjectFactory();
			var pivot = newFactory.Load<CusSCAPivot>(house.Pivot[0].PK);
			pivot.CV_CN = ZGuid.Empty;
			newFactory.Save();

			Assert(!consol.HasChanges);
			Assert("Does not trigger Has Changes Changed / Save remains disabled on the form", !hasChangesChanged);
		}

		public void TestDeleteDuplicatedPivot()
		{
			ForwardingConsol consol = CreateFCLConsol();
			var forwardingContainer = consol.Containers.AddNew();
			forwardingContainer.JC_ContainerNum = "XXXU1231230";

			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, forwardingContainer);
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "PKG";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualVolume = .4m;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, forwardingContainer);
			packLine1.JL_PackageCount = 2;

			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;
			var pivot1 = houseBill.Pivot.AddNew();
			var cusContainer = oceanBill.Containers.AddNew();
			cusContainer.CN_ContainerNumber = "XXXU1231230";
			pivot1.CV_CN = cusContainer.PK;

			var pivot2 = houseBill.Pivot.AddNew();
			pivot2.CV_CN = cusContainer.PK;

			PivotSynchroniser testSynchroniser = GetPivotSynchroniser(pivot1, packLine1);
			testSynchroniser.AddPackLineWatch(packLine2);
			testSynchroniser.SetEnabled(true, false);
			testSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));

			AssertNullOrEmpty("No Last Message Reported", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Fields Not Synchronised
		//			CV_FumigationCert
		//			CV_PerishableGoods
		//			CV_Flammable
		//			CV_PersonalEffects
		//			CV_HazardousGoods
		//			CV_Timber
		//			CV_CN
		//			CV_CA

		#endregion

		#region Implementation

		protected ZString expectedUnpackedOrPackedCode = "NE";
		protected abstract SeaCargoSynchroniser GetSeaCargoSynchroniser(ForwardingConsol consol);
		protected abstract PivotSynchroniser GetPivotSynchroniser(CusSCAPivot pivot, PackLine packLine);

		protected const string LongDescription = @"DESCRIPTION OF THE GOODS MAKING A VERY VERY LONG TEST
IT IS NOT ENTIRELY POSSIBLE FOR ALL OF THIS INFORMATION TO MAKE IT ACROSS 
TO THE SEA CARGO JOB, BUT WE SHOULD BE SENDING AS MUCH AS POSSIBLE";

		protected const string ChangedDescription = @"THIS IS A SMALL CHANGE OF THE GOODS MAKING A VERY VERY LONG TEST
IT IS NOT ENTIRELY POSSIBLE FOR ALL OF THIS INFORMATION TO MAKE IT ACROSS 
TO THE SEA CARGO JOB, BUT WE SHOULD BE SENDING AS MUCH AS POSSIBLE";

		#endregion
	}
}
