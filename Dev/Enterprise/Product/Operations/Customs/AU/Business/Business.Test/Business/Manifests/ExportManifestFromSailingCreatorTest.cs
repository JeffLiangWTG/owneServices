using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal sealed class ExportManifestFromSailingCreatorTest : TestCaseWithFactory
	{
		public void TestTypeOfCANListsEqual()
		{
			var bill = Factory.New<BillOfLading>();
			var manifest = Factory.New<ExportCustomsManifestHeader>();
			var line = manifest.Lines.AddNew();

			AssertEquals(bill.ShipmentCustomsEntryNumber.EntryType_List.GetType(), line.Lookups.TypeOfCANs.GetType());
		}

		public void TestCreateOnlyFromExportSailings()
		{
			SetupVoyage(Core.Constants.VoyageType.MainVoyage, "ADMIRALENGRACHT", "45S",
				new OriginPorts() { { "AUSYD", new ZDateTime(2006, 11, 01) }, { "NZAKL", new ZDateTime(2006, 11, 10) } },
				new ZString[] { "USLAX", "AUSYD" });

			AssertEquals(4, voyage.Sailings.Count);
			{
				var sailing = FetchSailingForPorts(voyage.Sailings, "NZAKL", "AUSYD");

				var bill = Factory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;
				bill.CustomsEntryNumberType = "CAN";
				bill.CustomsEntryNumber = "foo";
			}

			{
				var sailing = FetchSailingForPorts(voyage.Sailings, "AUSYD", "AUSYD");
				var bill = Factory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;
				bill.CustomsEntryNumberType = "CAN";
				bill.CustomsEntryNumber = "bar";
			}

			{
				var sailing = FetchSailingForPorts(voyage.Sailings, "AUSYD", "USLAX");
				var bill = Factory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;
				bill.CustomsEntryNumberType = "CAN";
				bill.CustomsEntryNumber = "baz";
			}

			Factory.Save();

			var holder = manager.CreateExportManifests();

			AssertEquals(1, holder.Manifests.Count);
			AssertNull(holder.Manifests[0].ExportManifest);
			AssertEquals(1, holder.Manifests[0].CalcExportManifest.Lines.Count);
			AssertEquals("baz", holder.Manifests[0].CalcExportManifest.Lines[0].EntryNumber);
		}

		public void TestCreateExportManifests_Main()
		{
			ZString manifestType = ManifestTypeList.Codes.ExportMainManifest;

			CreateVoyageAndBillsOfLading();

			Factory.Save();

			var manifests = AssertManifests(6);

			var manifest = AssertManifestHeader(manifests, manifestType, "AUSYD", "US", new ZDateTime(2006, 11, 1), 6, 3, 36);
			AssertEquals("Should have 11 lines", 11, manifest.Lines.Count);
			{
				AssertContainsManifestLine(manifest, "", "", "book1", 0, 0);
				AssertContainsManifestLine(manifest, "", "", "book2", 0, 7);
				AssertContainsManifestLine(manifest, "", "", "book3", 3, 6);
				AssertContainsManifestLine(manifest, "CAN", "bNo. 1", "book3", 1, 0);
				AssertContainsManifestLine(manifest, "CAN", "bNo. 2", "book3", 1, 0);
				AssertContainsManifestLine(manifest, "", "", "bol1", 0, 0);
				AssertContainsManifestLine(manifest, "", "", "bol2", 0, 4);
				AssertContainsManifestLine(manifest, "EXDC", "", "bol3", 0, 5);
				AssertContainsManifestLine(manifest, "EXDC", "", "EX", 0, 7);
				AssertContainsManifestLine(manifest, "", "", "EX", 0, 0);
				AssertContainsManifestLine(manifest, "CCN", "No 1", "bol7", 1, 7);
			}

			manifest = AssertManifestHeader(manifests, manifestType, "AUBNE", "NZ", new ZDateTime(2006, 11, 10), 30, 0, 10);
			AssertEquals("Should have 1 line", 1, manifest.Lines.Count);
			{
				AssertContainsManifestLine(manifest, "CAN", "blak", "haar", 30, 10);
			}

			manifest = AssertManifestHeader(manifests, manifestType, "AUSYD", "CA", new ZDateTime(2006, 11, 01), 0, 0, 0);
			AssertEquals("Should have NO lines", 0, manifest.Lines.Count);

			var bol8 = CreateShipment(false, false, "FCL", "AUSYD", "USLAX", "", "", "EX", 0);
			CreateContainer(bol8, "", "", 1, 0, false);
			CreateContainer(bol8, "EXDC", "", 1, 8, false);
			var container83 = CreateContainer(bol8, "CCN", "No 1", 1, 0, false);
			var container84 = CreateContainer(bol8, "", "", 1, 4, false);

			var bol9 = CreateShipment(false, false, "FCL", "AUSYD", "USLAX", "CCN", "No 2", "bol9", 0);
			CreateContainer(bol9, "", "", 1, 0, true);
			CreateContainer(bol9, "", "", 1, 9, false);

			Factory.Save();

			manifests = AssertManifests(6);

			manifest = AssertManifestHeader(manifests, manifestType, "AUSYD", "US", new ZDateTime(2006, 11, 1), 11, 4, 57);
			AssertEquals("Should have 12 lines", 12, manifest.Lines.Count);
			{
				AssertContainsManifestLine(manifest, "", "", "book1", 0, 0);
				AssertContainsManifestLine(manifest, "", "", "book2", 0, 7);
				AssertContainsManifestLine(manifest, "", "", "book3", 3, 6);
				AssertContainsManifestLine(manifest, "CAN", "bNo. 1", "book3", 1, 0);
				AssertContainsManifestLine(manifest, "CAN", "bNo. 2", "book3", 1, 0);
				AssertContainsManifestLine(manifest, "", "", "bol1", 0, 0);
				AssertContainsManifestLine(manifest, "", "", "bol2", 0, 4);
				AssertContainsManifestLine(manifest, "EXDC", "", "bol3", 0, 5);
				AssertContainsManifestLine(manifest, "EXDC", "", "EX", 1, 15);
				AssertContainsManifestLine(manifest, "", "", "EX", 2, 4);
				AssertContainsManifestLine(manifest, "CCN", "No 1", "", 2, 7);
				AssertContainsManifestLine(manifest, "CCN", "No 2", "bol9", 1, 9);
			}

			CreateContainer(bol9, "", "", 11, 50, false);
			container83.Delete();
			container84.CustomsEntryNumberType = "CCN";
			container84.CustomsEntryNumber = "No 2";

			Factory.Save();

			manifests = AssertManifests(6);

			manifest = AssertManifestHeader(manifests, manifestType, "AUSYD", "US", new ZDateTime(2006, 11, 1), 21, 4, 107);
			AssertEquals("Should have 12 lines", 12, manifest.Lines.Count);
			{
				AssertContainsManifestLine(manifest, "", "", "book1", 0, 0);
				AssertContainsManifestLine(manifest, "", "", "book2", 0, 7);
				AssertContainsManifestLine(manifest, "", "", "book3", 3, 6);
				AssertContainsManifestLine(manifest, "CAN", "bNo. 1", "book3", 1, 0);
				AssertContainsManifestLine(manifest, "CAN", "bNo. 2", "book3", 1, 0);
				AssertContainsManifestLine(manifest, "", "", "bol1", 0, 0);
				AssertContainsManifestLine(manifest, "", "", "bol2", 0, 4);
				AssertContainsManifestLine(manifest, "EXDC", "", "bol3", 0, 5);
				AssertContainsManifestLine(manifest, "EXDC", "", "EX", 1, 15);
				AssertContainsManifestLine(manifest, "", "", "EX", 1, 0);
				AssertContainsManifestLine(manifest, "CCN", "No 1", "bol7", 1, 7);
				AssertContainsManifestLine(manifest, "CCN", "No 2", "", 13, 63);
			}
		}

		public void TestPerformLoadsUsingThePassedInFactoryNotTheVoyageFactory()
		{
			ZGuid voyagePK;
			{
				var createFactory = new BusinessObjectFactory();

				var voyage = createFactory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				var sailing = voyage.Sailings[0];

				var bill = createFactory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;

				var container1 = bill.RealContainers.AddNew();
				container1.JC_ContainerNum = "TEST4100013";
				container1.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				var booking = createFactory.New<AgencyBooking>();
				booking.JS_JX = sailing.PK;

				var container2 = booking.BookedContainers.AddNew();
				container2.JC_ContainerCount = 2;
				container2.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				createFactory.Save();

				voyagePK = voyage.PK;
			}

			{
				var sailingFactory = new BusinessObjectFactory();
				var generateFactory = new BusinessObjectFactory();

				var voyage = sailingFactory.Load<JobVoyage>(voyagePK);
				voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
				voyage.MarkAsNeedingValidationIncludingChildren();
				voyage.RunPreSaveValidation();
				sailingFactory.ResetDatabaseLoadCount();

				var wrapper = new CustomsJobVoyageWrapper(voyage);
				var creator = new ExportManifestFromSailingCreator(generateFactory, wrapper);

				var holder = creator.CreateExportManifests();

				AssertMaxDbHits("Should not have loaded anything new in the sailing factory", 0, sailingFactory);
				AssertEquals("manifest holder should be using the generate factory.", generateFactory, holder.Factory);
			}
		}

		public void TestVoyageType()
		{
			var voyage = Factory.New<JobVoyage>();

			voyage.JV_VoyageType = ZString.Empty;
			var voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			AssertExceptionThrown("Should be Exception when Empty", typeof(ArgumentException), "The Voyage Type should be set up before creating Export Manifest", delegate
			{ new ExportManifestFromSailingCreator(new BusinessObjectFactory(), voyageWrapper); });

			voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			AssertNoExceptionThrown("Should be NO Exceptions", delegate
			{ new ExportManifestFromSailingCreator(new BusinessObjectFactory(), voyageWrapper); });

			voyage.JV_VoyageType = "AAA";
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			AssertExceptionThrown("Should be Exception when incorrect type", typeof(ArgumentException), "Invalid Voyage Type: AAA", delegate
			{ new ExportManifestFromSailingCreator(new BusinessObjectFactory(), voyageWrapper); });

			voyage.JV_VoyageType = Core.Constants.VoyageType.SlotVoyage;
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			AssertNoExceptionThrown("Should be NO Exceptions", delegate
			{ new ExportManifestFromSailingCreator(new BusinessObjectFactory(), voyageWrapper); });
		}

		public void TestCreateExportManifests_Slot()
		{
			ZString manifestType = ManifestTypeList.Codes.SlotExportSubManifest;

			SetupVoyage(manifestType, "ADMIRALENGRACHT", "45S",
				new OriginPorts() { { "AUSYD", new ZDateTime(2006, 11, 01) }, { "AUBNE", new ZDateTime(2006, 11, 10) } },
				new ZString[] { "NZAKL", "USLAX", "CAVAN" });

			AssertEquals(6, voyage.Sailings.Count);

			var consignorPK = GetOrgHeader("consignor").PK;

			var bookingPartyPK = GetOrgHeader("bookingParty").PK;

			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "desc1", 1, consignorPK, "");
			CreateShipment(true, false, "FCL", "AUSYD", "NZAKL", "", "", "desc2", 2, consignorPK, "", bookingPartyPK, "");
			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "desc3", 3, consignorPK, "", ZGuid.Empty, "bookingParty_Name");
			CreateShipment(true, false, "BLK", "AUSYD", "NZAKL", "", "", "desc4", 4, ZGuid.Empty, "consignor_Name");
			CreateShipment(true, false, "FCL", "AUSYD", "USLAX", "EXDC", "", "desc5", 5, ZGuid.Empty, "consignor_Name", bookingPartyPK, "");
			CreateShipment(true, false, "BLK", "AUSYD", "NZAKL", "", "", "desc6", 6, ZGuid.Empty, "consignor_Name", ZGuid.Empty, "bookingParty_Name");
			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "desc7", 7, ZGuid.Empty, "", bookingPartyPK, "");
			CreateShipment(true, false, "FCL", "AUSYD", "NZAKL", "", "", "desc8", 8, ZGuid.Empty, "", ZGuid.Empty, "bookingParty_Name");
			CreateShipment(false, false, "FCL", "AUSYD", "CAVAN", "CAN", "123", "desc9", 9, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "", "", "desc10", 10, ZGuid.Empty, "consignor_Name");

			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "CAN", "CAN 1", "book1", 10, consignorPK, "");
			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "CAN", "CAN 1", "book2", 20, ZGuid.Empty, "consignor_Name");
			var book3 = CreateShipment(true, false, "FCL", "AUSYD", "NZAKL", "", "", "book3", 0, consignorPK, "");
			{
				CreateContainer(book3, "", "", 2, 0, true);
				CreateContainer(book3, "", "", 3, 6, false);
				CreateContainer(book3, "CAN", "bNo. 1", 1, 0, false, false).JC_RC = book3.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
				CreateContainer(book3, "CAN", "bNo. 2", 1, 0, false, false).JC_RC = book3.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			}
			CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "CAN", "CAN 2", "bol1", 5, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "NZAKL", "CAN", "CAN 2", "bol2", 4, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "CCN", "CCN 1", "bol3", 5, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "NZAKL", "CCN", "CCN 1", "EX", 3, consignorPK, "");
			var bol5 = CreateShipment(false, false, "FCL", "AUSYD", "NZAKL", "", "", "bol5", 0, consignorPK, "");
			{
				var container1 = CreateContainer(bol5, "CAN", "CAN 2", 1, 11, false);
				var container2 = CreateContainer(bol5, "CCN", "CCN 1", 1, 0, true);
			}

			CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "exdc1", 10, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "NZAKL", "EXDC", "", "exdc1", 10, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "exdc2", 10, consignorPK, "");
			CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "exdc2", 10, consignorPK, "");

			var bill = CreateShipment(false, false, "FCL", "AUBNE", "NZAKL", "CAN", "blak", "haar", 0, consignorPK, "");
			{
				CreateContainer(bill, "", "", 30, 10, false);
			}

			Factory.Save();

			var manifests = AssertManifests(2);

			var manifest = AssertManifestHeader(manifests, manifestType, "AUBNE", "", new ZDateTime(2006, 11, 10), 30, 0, 10);
			AssertEquals("Should have 1 line", 1, manifest.Lines.Count);
			{
				AssertContainsManifestLine(manifest, "CAN", "blak", "haar", 30, 10, consignorPK, "consignor", "NZ");
			}

			manifest = AssertManifestHeader(manifests, manifestType, "AUSYD", "", new ZDateTime(2006, 11, 1), 6, 3, 135);
			AssertEquals("Should have 20 lines", 20, manifest.Lines.Count);
			{
				AssertContainsManifestLine(manifest, "EXDC", "", "desc1", 0, 1, consignorPK, "consignor", "US");
				AssertContainsManifestLine(manifest, "", "", "desc2", 0, 0, consignorPK, "consignor", "NZ");
				AssertContainsManifestLine(manifest, "EXDC", "", "desc3", 0, 3, consignorPK, "consignor", "US");
				AssertContainsManifestLine(manifest, "", "", "desc4", 0, 4, ZGuid.Empty, "consignor_Name", "NZ");
				AssertContainsManifestLine(manifest, "EXDC", "", "desc5", 0, 0, ZGuid.Empty, "consignor_Name", "US");
				AssertContainsManifestLine(manifest, "", "", "desc6", 0, 6, ZGuid.Empty, "consignor_Name", "NZ");
				AssertContainsManifestLine(manifest, "EXDC", "", "desc7", 0, 7, bookingPartyPK, "bookingParty", "US");
				AssertContainsManifestLine(manifest, "", "", "desc8", 0, 0, ZGuid.Empty, "bookingParty_Name", "NZ");
				AssertContainsManifestLine(manifest, "CAN", "123", "desc9", 0, 0, consignorPK, "consignor", "CA");
				AssertContainsManifestLine(manifest, "", "", "desc10", 0, 10, ZGuid.Empty, "consignor_Name", "US");

				AssertContainsManifestLine(manifest, "CAN", "CAN 1", "book1", 0, 10, consignorPK, "consignor", "US");
				AssertContainsManifestLine(manifest, "CAN", "CAN 1", "book2", 0, 20, ZGuid.Empty, "consignor_Name", "US");
				AssertContainsManifestLine(manifest, "", "", "book3", 3, 6, consignorPK, "consignor", "NZ");
				AssertContainsManifestLine(manifest, "CAN", "bNo. 1", "book3", 1, 0, consignorPK, "consignor", "NZ");
				AssertContainsManifestLine(manifest, "CAN", "bNo. 2", "book3", 1, 0, consignorPK, "consignor", "NZ");
				AssertContainsManifestLine(manifest, "CAN", "CAN 2", "", 1, 20, consignorPK, "consignor", "");
				AssertContainsManifestLine(manifest, "CCN", "CCN 1", "", 0, 8, consignorPK, "consignor", "");

				AssertContainsManifestLine(manifest, "EXDC", "", "exdc1", 0, 10, consignorPK, "consignor", "US");
				AssertContainsManifestLine(manifest, "EXDC", "", "exdc1", 0, 10, consignorPK, "consignor", "NZ");
				AssertContainsManifestLine(manifest, "EXDC", "", "exdc2", 0, 20, consignorPK, "consignor", "US");
			}
		}

		public void TestDuplicatedManifests()
		{
			ZString manifestType = ManifestTypeList.Codes.SlotExportSubManifest;

			SetupVoyage(manifestType, "ADMIRALENGRACHT", "45S",
				new OriginPorts() { { "AUSYD", new ZDateTime(2006, 11, 01) }, { "AUBNE", new ZDateTime(2006, 11, 10) } },
				new ZString[] { "NZAKL", "USLAX", "CAVAN" });

			AssertEquals(6, voyage.Sailings.Count);

			var consignorPK = GetOrgHeader("consignor").PK;

			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "desc1", 1, consignorPK, "");

			var header1 = Factory.New<ExportCustomsManifestHeader>();
			header1.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header1.ED_ManifestType = manifestType;
			header1.ED_VesselName = voyage.Vessel.RV_Code;
			header1.ED_VoyageNumber = voyage.JV_VoyageFlight;
			header1.ED_RL_NKPortOfDeparture = "AUBNE";
			header1.ED_FolioReference = "FL_01";

			var header2 = Factory.New<ExportCustomsManifestHeader>();
			header2.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header2.ED_ManifestType = manifestType;
			header2.ED_VesselName = voyage.Vessel.RV_Code;
			header2.ED_VoyageNumber = voyage.JV_VoyageFlight;
			header2.ED_RL_NKPortOfDeparture = "AUBNE";
			header2.ED_FolioReference = "FL_02";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			voyage = newFactory.Load<JobVoyage>(voyage.PK);
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			manager = new ExportManifestFromSailingCreator(newFactory, voyageWrapper);

			var holder = manager.CreateExportManifests();

			AssertEquals(3, holder.Manifests.Count);

			AssertContainsCalcManifest(holder.Manifests, "ADMIRALENGRACHT", "45S", "AUSYD", false, true);
			AssertContainsManifest(holder.Manifests, "ADMIRALENGRACHT", "45S", "AUBNE", "FL_01", true, false);
			AssertContainsManifest(holder.Manifests, "ADMIRALENGRACHT", "45S", "AUBNE", "FL_02", true, false);
		}

		[ExpectNoExceptions]
		public void TestLongNameDoesNotCauseException()
		{
			ZString manifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			SetupVoyage(manifestType, "ADMIRALENGRACHT", "45S",
				new OriginPorts() { { "AUSYD", new ZDateTime(2006, 11, 01) }, { "AUBNE", new ZDateTime(2006, 11, 10) } },
				new ZString[] { "NZAKL", "USLAX", "CAVAN" });
			var consignorPK = GetOrgHeader("consignor01234567890123456789012345xxx").PK;
			CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "desc1", 1, consignorPK, "");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			voyage = newFactory.Load<JobVoyage>(voyage.PK);
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			new ExportManifestFromSailingCreator(newFactory, voyageWrapper).CreateExportManifests();
		}

		#region Implementation

		void AssertContainsCalcManifest(TemporaryManifestsCollection manifests, ZString vessel, ZString voyageNo, ZString portOfDeparture, bool isDuplicated, bool shouldSave)
		{
			ZString messageFormat = "{0}-{1}-{2} IsDuplicated:{3}, ShouldSave:{4}\r\n";
			ZString expected = string.Format(messageFormat, vessel, voyageNo, portOfDeparture, isDuplicated, shouldSave);
			var found = ZString.Empty;
			foreach (TemporaryManifest manifest in manifests)
			{
				if (manifest.CalcExportManifest.VesselName == vessel &&
					manifest.CalcExportManifest.VoyageNumber == voyageNo &&
					manifest.CalcExportManifest.Departure == portOfDeparture &&
					manifest.IsDuplicated == isDuplicated &&
					manifest.ShouldSave == shouldSave)
				{
					return;
				}
				else
				{
					found += string.Format(messageFormat, manifest.CalcExportManifest.VesselName, manifest.CalcExportManifest.VoyageNumber, manifest.CalcExportManifest.Departure, manifest.IsDuplicated, manifest.ShouldSave);
				}
			}

			AssertEquals(expected, found);
		}

		void AssertContainsManifest(TemporaryManifestsCollection manifests, ZString vessel, ZString voyageNo, ZString portOfDeparture, ZString folio, bool isDuplicated, bool shouldSave)
		{
			ZString messageFormat = "{0}-{1}-{2} (Folio:{3}) IsDuplicated:{4}, ShouldSave:{5}\r\n";
			ZString expected = string.Format(messageFormat, vessel, voyageNo, portOfDeparture, folio, isDuplicated, shouldSave);
			var found = ZString.Empty;
			foreach (TemporaryManifest manifest in manifests)
			{
				if (manifest.ExportManifest != null)
				{
					if (manifest.ExportManifest.ED_VesselName == vessel &&
						manifest.ExportManifest.ED_VoyageNumber == voyageNo &&
						manifest.ExportManifest.ED_RL_NKPortOfDeparture == portOfDeparture &&
						manifest.ExportManifest.ED_FolioReference == folio &&
						manifest.IsDuplicated == isDuplicated &&
						manifest.ShouldSave == shouldSave)
					{
						return;
					}
					else
					{
						found += string.Format(messageFormat, manifest.ExportManifest.ED_VesselName, manifest.ExportManifest.ED_VoyageNumber, manifest.ExportManifest.ED_RL_NKPortOfDeparture, manifest.ExportManifest.ED_FolioReference, manifest.IsDuplicated, manifest.ShouldSave);
					}
				}
			}

			AssertEquals(expected, found);
		}

		void SetupVoyage(ZString voyageType, ZString vessel, ZString voyageNumber, OriginPorts origins, ZString[] destinations)
		{
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageType = voyageType;
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageNumber;

			foreach (var originPort in origins.Keys)
			{
				var origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = originPort;
				origin.JA_E_DEP = origins[originPort];
			}

			foreach (var destinationPort in destinations)
			{
				var destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = destinationPort;
			}
		}

		void AssertContainsManifestLine(ExportCustomsManifestHeader manifest, ZString entryType, ZString entryNumber, ZString goodsDescription, ZInt containerCount, ZInt packageCount)
		{
			AssertContainsManifestLine(manifest, entryType, entryNumber, goodsDescription, containerCount, packageCount, ZGuid.Empty, ZString.Empty, manifest.ED_RN_NKCountryOfDestination);
		}

		void AssertContainsManifestLine(ExportCustomsManifestHeader manifest, ZString entryType, ZString entryNumber, ZString goodsDescription, ZInt containerCount, ZInt packageCount, ZGuid owner, ZString goodsOwner, ZString dischargeCountry)
		{
			foreach (ExportCustomsManifestLines line in manifest.Lines)
			{
				if (line.EL_TypeOfCAN == entryType &&
					line.EL_CAN == entryNumber &&
					line.EL_GoodsDescription == goodsDescription &&
					line.EL_NumberOfContainers == (ZShort)containerCount &&
					line.EL_NumberOfPackages == packageCount &&
					line.EL_OH_Owner == owner &&
					line.EL_GoodsOwner == goodsOwner &&
					line.EL_RN_NKCountryOfDestination == dischargeCountry)
				{
					return;
				}
			}

			var builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("*** Expected line was not found ***");
			FormatLine(builder, entryType, entryNumber, goodsDescription, containerCount, packageCount, owner, goodsOwner, dischargeCountry);
			builder.AppendLine();
			builder.AppendLine("*** Lines Found ***");

			foreach (ExportCustomsManifestLines line in manifest.Lines)
			{
				builder.AppendLine();
				FormatLine(builder, line.EL_TypeOfCAN, line.EL_CAN, line.EL_GoodsDescription, line.EL_NumberOfContainers, line.EL_NumberOfPackages, line.EL_OH_Owner, line.EL_GoodsOwner, line.EL_RN_NKCountryOfDestination);
			}

			Fail(builder.ToString());
		}

		void FormatLine(StringBuilder builder, ZString entryType, ZString entryNumber, ZString goodsDescription, ZInt containerCount, ZInt packageCount, ZGuid owner, ZString goodsOwner, ZString dischargeCountry)
		{
			const string format =
				"EntryType: '{0}'\r\n" +
				"EntryNo: '{1}'\r\n" +
				"Description: '{2}'\r\n" +
				"Containers: {3}\r\n" +
				"Packs: {4}\r\n" +
				"Owner: {5}\r\n" +
				"GoodsOwner: {6}\r\n" +
				"Country/Region of Destination: {7}\r\n" +
				"";

			builder.AppendFormat(format, entryType, entryNumber, goodsDescription, containerCount, packageCount, owner, goodsOwner, dischargeCountry);
		}

		ExportCustomsManifestHeader AssertManifestHeader(ExportCustomsManifestHeaderCollection manifests, ZString manifestType, ZString portOfDeparture, ZString countryOfDestination, ZDateTime departureDate, ZInt containerCount, ZInt emptyContainerCount, ZInt packageCount)
		{
			var manifest = FetchManifestForDepPortAndCountry(manifests, manifestType, portOfDeparture, countryOfDestination);

			CombineAssertions(delegate
			{
				AssertEquals("manifest.ED_TransportMode", Core.Constants.TransportModes.Sea, manifest.ED_TransportMode);
				AssertEquals("manifest.ED_RL_NKPortOfDeparture", portOfDeparture, manifest.ED_RL_NKPortOfDeparture);
				AssertEquals("manifest.ED_RN_NKCountryOfDestination", countryOfDestination, manifest.ED_RN_NKCountryOfDestination);
				AssertEquals("manifest.ED_DepartureDate", departureDate, manifest.ED_DepartureDate);
				AssertEquals("manifest.ED_VesselName", voyage.Vessel.RV_Code, manifest.ED_VesselName);
				AssertEquals("manifest.ED_VoyageNumber", voyage.JV_VoyageFlight, manifest.ED_VoyageNumber);
				AssertEquals("manifest.ED_ManifestType", manifestType, manifest.ED_ManifestType);

				AssertEquals("manifest.ED_NoOfContainer", (ZShort)containerCount, manifest.ED_NoOfContainer);
				AssertEquals("manifest.ED_NoOfEmptyContainers", (ZShort)emptyContainerCount, manifest.ED_NoOfEmptyContainers);
				AssertEquals("manifest.ED_NoOfPacks", packageCount, manifest.ED_NoOfPacks);
			});

			return manifest;
		}

		ExportCustomsManifestHeaderCollection AssertManifests(ZInt manifestCount)
		{
			var newFactory = new BusinessObjectFactory();
			voyage = newFactory.Load<JobVoyage>(voyage.PK);
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			manager = new ExportManifestFromSailingCreator(newFactory, voyageWrapper);

			var holder = manager.CreateExportManifests();
			foreach (TemporaryManifest manifest in holder.Manifests)
			{
				if (manifest.ExportManifest == null)
				{
					manifest.CalcExportManifest.ApplyTo(newFactory);
				}
				else
				{
					manifest.CalcExportManifest.ApplyTo(manifest.ExportManifest);
				}
			}
			newFactory.Save();

			var manifests = new ExportCustomsManifestHeaderCollection(Factory);
			manifests.Load();
			AssertEquals(manifestCount, manifests.Count);

			foreach (ExportCustomsManifestHeader header in manifests)
			{
				AssertEquals(true, header.IsInDatabase);
			}

			return manifests;
		}

		void CreateVoyageAndBillsOfLading()
		{
			SetupVoyage(Core.Constants.VoyageType.MainVoyage, "ADMIRALENGRACHT", "45S",
				new OriginPorts() { { "AUSYD", new ZDateTime(2006, 11, 01) }, { "AUBNE", new ZDateTime(2006, 11, 10) } },
				new ZString[] { "NZAKL", "USLAX", "CAVAN" });

			AssertEquals(6, voyage.Sailings.Count);

			var book1 = CreateShipment(true, false, "BLK", "AUSYD", "USLAX", "", "", "book1", 0);

			var book2 = CreateShipment(true, true, "BLK", "AUSYD", "USLAX", "", "", "book2", 0);
			{
				book2.TopLevelPacks.AddNew().JC_ContainerCount = 5;
				book2.TopLevelPacks.AddNew().JC_ContainerCount = 2;
			}

			var book3 = CreateShipment(true, false, "FCL", "AUSYD", "USLAX", "", "", "book3", 0);
			{
				CreateContainer(book3, "", "", 2, 0, true);
				CreateContainer(book3, "", "", 3, 6, false);
				CreateContainer(book3, "CAN", "bNo. 1", 1, 0, false, false).JC_RC = book3.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
				CreateContainer(book3, "CAN", "bNo. 2", 1, 0, false, false).JC_RC = book3.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			}

			var bol1 = CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "", "", "bol1", 0);

			var bol2 = CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "", "", "bol2", 4);

			var bol3 = CreateShipment(false, true, "BLK", "AUSYD", "USLAX", "EXDC", "", "bol3", 5);

			var bol4 = CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "EX", 3);

			var bol5 = CreateShipment(false, false, "BLK", "AUSYD", "USLAX", "EXDC", "", "EX", 4);

			var bol6 = CreateShipment(false, false, "FCL", "AUSYD", "USLAX", "", "", "EX", 0);

			var bol7 = CreateShipment(false, false, "FCL", "AUSYD", "USLAX", "CCN", "No 1", "bol7", 0);
			{
				var container1 = CreateContainer(bol7, "", "", 1, 0, false);
				{
					CreatePackLine(container1, 3);
					CreatePackLine(container1, 4);
				}
				var container2 = CreateContainer(bol7, "", "", 1, 0, true);
			}

			var bol8 = CreateShipment(false, false, "FCL", "AUBNE", "NZAKL", "CAN", "blak", "haar", 0);
			{
				CreateContainer(bol8, "", "", 30, 10, false);
			}
		}

		AgencyShipment CreateShipment(ZBool isBooking, ZBool useTransport, ZString packingMode, ZString loadPort, ZString dischargePort, ZString entryType, ZString entryNumber, ZString goodsDescription, ZInt packageCount)
		{
			AgencyShipment result;
			if (isBooking)
			{
				result = Factory.NewWithValidTestData<AgencyBooking>();
			}
			else
			{
				result = Factory.NewWithValidTestData<BillOfLading>();
			}

			result.JS_PackingMode = packingMode;

			if (useTransport)
			{
				var transport = result.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_JX = FetchSailingPKForPorts(voyage.Sailings, loadPort, dischargePort);
			}
			else
			{
				result.JS_JX = FetchSailingPKForPorts(voyage.Sailings, loadPort, dischargePort);
			}

			if (!entryType.IsEmpty)
			{
				result.CustomsEntryNumberType = entryType;
				result.CustomsEntryNumber = entryNumber;
			}

			result.JS_GoodsDescription = goodsDescription;

			result.OuterPackLines.RemoveAndDeleteAll();
			result.TopLevelPacks.RemoveAndDeleteAll();
			if (packageCount > 0)
			{
				if (AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes.Contains(packingMode))
				{
					var topLevelPack = result.TopLevelPacks.AddNew();
					topLevelPack.JC_ContainerCount = (ZShort)packageCount;
				}
				else
				{
					PackLine line = result.OuterPackLines.AddNew();
					line.JL_PackageCount = packageCount;
				}
			}

			return result;
		}

		AgencyShipment CreateShipment(ZBool isBooking, ZBool useTransport, ZString packingMode, ZString loadPort, ZString dischargePort, ZString entryType, ZString entryNumber, ZString goodsDescription, ZInt packageCount, ZGuid consignor, ZString consignorName)
		{
			var shipment = this.CreateShipment(isBooking, useTransport, packingMode, loadPort, dischargePort, entryType, entryNumber, goodsDescription, packageCount);

			if (consignorName.IsEmpty)
			{
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor;
			}
			else
			{
				shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
				shipment.ConsignorDocumentaryAddress.E2_CompanyName = consignorName;
			}

			return shipment;
		}

		AgencyShipment CreateShipment(ZBool isBooking, ZBool useTransport, ZString packingMode, ZString loadPort, ZString dischargePort, ZString entryType, ZString entryNumber, ZString goodsDescription, ZInt packageCount, ZGuid consignor, ZString consignorName, ZGuid bookingParty, ZString bookingPartyName)
		{
			var shipment = this.CreateShipment(isBooking, useTransport, packingMode, loadPort, dischargePort, entryType, entryNumber, goodsDescription, packageCount, consignor, consignorName);

			if (bookingPartyName.IsEmpty)
			{
				shipment.BookingPartyDocumentaryAddress.OrganisationPK = bookingParty;
			}
			else
			{
				shipment.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
				shipment.BookingPartyDocumentaryAddress.E2_CompanyName = bookingPartyName;
			}

			return shipment;
		}

		AgencyShipmentContainer CreateContainer(AgencyShipment shipment, ZString entryType, ZString entryNumber, ZInt containerCount, ZInt packageCount, ZBool isEmpty)
		{
			return CreateContainer(shipment, entryType, entryNumber, containerCount, packageCount, isEmpty, (shipment is AgencyBooking));
		}

		AgencyShipmentContainer CreateContainer(AgencyShipment shipment, ZString entryType, ZString entryNumber, ZInt containerCount, ZInt packageCount, ZBool isEmpty, bool isBooking)
		{
			var collection = isBooking ? shipment.BookedContainers : shipment.RealContainers;
			var result = collection.AddNew();
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			result.JC_RC = refContainer;

			if (!entryType.IsEmpty)
			{
				result.CustomsEntryNumberType = entryType;
				result.CustomsEntryNumber = entryNumber;
			}

			if (packageCount > 0)
			{
				PackLine line = shipment.OuterPackLines.AddNew();
				line.JL_PackageCount = packageCount;
				line.JL_JC = result.PK;
			}

			result.JC_IsEmptyContainer = isEmpty;

			if (isBooking)
			{
				result.JC_ContainerCount = (ZShort)containerCount;
			}
			else
			{
				result.JC_ContainerNum = "Con" + (++containerIndex);

				for (var i = 1; i < containerCount; i++)
				{
					result = collection.AddNew();
					result.JC_RC = refContainer;

					result.JC_ContainerNum = "Con" + (++containerIndex);

					if (!entryType.IsEmpty)
					{
						result.CustomsEntryNumberType = entryType;
						result.CustomsEntryNumber = entryNumber;
					}

					result.JC_IsEmptyContainer = isEmpty;
				}
			}

			return result;
		}

		int containerIndex;

		PackLine CreatePackLine(AgencyShipmentContainer container, ZInt packageCount)
		{
			var result = Factory.NewWithValidTestData<PackLine>();
			result.JL_FreightMode = FreightConstants.OuterPackType;
			result.JL_PackageCount = packageCount;

			container.PackLines.Add(result);
			container.JC_IsEmptyContainer = false;

			return result;
		}

		ExportCustomsManifestHeader FetchManifestForDepPortAndCountry(ExportCustomsManifestHeaderCollection manifests, ZString manifestType, ZString departurePort, ZString countryCode)
		{
			foreach (ExportCustomsManifestHeader manifest in manifests)
			{
				if (manifest.ED_RN_NKCountryOfDestination == countryCode && manifest.ED_RL_NKPortOfDeparture == departurePort && manifest.ED_ManifestType == manifestType)
				{
					return manifest;
				}
			}

			Fail("Manifest not found for country code " + countryCode);
			return null;
		}

		ZGuid FetchSailingPKForPorts(JobSailingCollection sailings, ZString loadPort, ZString dischargePort)
		{
			var sailing = FetchSailingForPorts(sailings, loadPort, dischargePort);
			if (sailing != null)
			{
				return sailing.PK;
			}

			return ZGuid.Empty;
		}

		JobSailing FetchSailingForPorts(JobSailingCollection sailings, ZString loadPort, ZString dischargePort)
		{
			foreach (JobSailing sailing in sailings)
			{
				if (sailing.JX_JA_RL_NKPortOfLoading == loadPort && sailing.JX_JB_RL_NKPortOfDischarge == dischargePort)
				{
					return sailing;
				}
			}

			Fail(string.Format("Sailing not found for ports {0} and {1}.", loadPort, dischargePort));
			return null;
		}

		OrgHeader GetOrgHeader(ZString fullName)
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = fullName;

			return header;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestCaseHelper.ClearTable(ExportCustomsManifestHeaderSchema.Constants.TableName);

			voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			manager = new ExportManifestFromSailingCreator(new BusinessObjectFactory(), voyageWrapper);
		}

		ExportManifestFromSailingCreator manager;
		JobVoyage voyage;
		CustomsJobVoyageWrapper voyageWrapper;

		class OriginPorts : Dictionary<ZString, ZDateTime>
		{
		}

		#endregion
	}
}
