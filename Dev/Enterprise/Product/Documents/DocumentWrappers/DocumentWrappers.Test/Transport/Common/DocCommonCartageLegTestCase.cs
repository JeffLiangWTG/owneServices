using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonCartageLeg))]
	sealed class DocCommonCartageLegTestCase : DocumentWrapperTestCase
	{
		public void TestIDocCartageAdviceDates()
		{
			CommonCartageLeg cartageLeg = Factory.New<CommonCartageLeg>();
			CommonBookedCtgMove bookedCtgMove = Factory.New<CommonBookedCtgMove>();
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartageLeg.JU_EW = bookedCtgMove.PK;
			bookedCtgMove.EW_JJ = cartage.PK;

			JobVoyage voyage = Factory.New<JobVoyage>();
			JobSailing sailing = voyage.Sailings.AddNew();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			origin.JA_CutOff = new ZDateTime(2011, 1, 18);
			origin.JA_ReceivalCommences = new ZDateTime(2011, 1, 22);
			destination.JB_AvailabilityDate = new ZDateTime(2011, 4, 18);
			destination.JB_StorageDate = new ZDateTime(2011, 4, 22);

			sailing.JX_DepotCutOff = new ZDateTime(2011, 1, 25);
			sailing.JX_DepotReceivalCommences = new ZDateTime(2011, 1, 20);
			sailing.JX_DepotAvailabilityDate = new ZDateTime(2011, 4, 20);
			sailing.JX_DepotStorageDate = new ZDateTime(2011, 4, 25);

			DocCommonCartageLeg wrapper = DocCommonCartageLeg.New(cartageLeg, Factory);

			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
			cartage.JJ_JX_Sailing = sailing.PK;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), wrapper.CartageStorageCommenceDate);

			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
			cartage.JJ_JX_Sailing = sailing.PK;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 25), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 20), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), wrapper.CartageStorageCommenceDate);
		}

		public void TestPickupDemurrage()
		{
			var leg = Factory.New<CommonCartageLeg>();
			var legWrapper = DocCommonCartageLeg.New(leg, Factory);
			leg.JU_CartagePickupDemurrage = new ZDateTime(2010, 1, 1);
			AssertEquals("", legWrapper.PickupDemurrage);

			leg.JU_CartagePickupDemurrage = new ZDateTime(2010, 1, 1, 2, 30, 0);
			AssertEquals("2:30", legWrapper.PickupDemurrage);

			leg.JU_CartagePickupDemurrage = new ZDateTime(2010, 1, 1, 14, 30, 0);
			AssertEquals("14:30", legWrapper.PickupDemurrage);
		}

		public void TestDeliveryDemurrage()
		{
			var leg = Factory.New<CommonCartageLeg>();
			var legWrapper = DocCommonCartageLeg.New(leg, Factory);
			leg.JU_CartageDeliveryDemurrage = new ZDateTime(2010, 1, 1);
			AssertEquals("", legWrapper.DeliveryDemurrage);

			leg.JU_CartageDeliveryDemurrage = new ZDateTime(2010, 1, 1, 2, 30, 0);
			AssertEquals("2:30", legWrapper.DeliveryDemurrage);

			leg.JU_CartageDeliveryDemurrage = new ZDateTime(2010, 1, 1, 14, 30, 0);
			AssertEquals("14:30", legWrapper.DeliveryDemurrage);
		}

		public void TestSignature()
		{
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			DocCommonCartageLeg doc = DocCommonCartageLeg.New(leg, Factory);

			StmData stm = Factory.New<StmData>();
			SignatureData data = new SignatureData(leg);
			Factory.Save();
			stm.SD_Owner = leg.PK;
			Factory.Save();
			var stream = new MemoryStream();
			stream.Write(BitConverter.GetBytes(800), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(480), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(1), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(2), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(300), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(300), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(200), 0, sizeof(Int32));
			stream.Write(BitConverter.GetBytes(200), 0, sizeof(Int32));

			stm.SD_BinaryValue = new ZBlob(stream.ToArray());
			Factory.Save();
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var img = doc.Signature;
			AssertNotNull(img);
		}

		public void TestPickupDeliverySlotOpenCloseDates()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T12345678";
			cartage.JJ_GoodsDescription = "the goods description";

			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "CONT123456";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container.JC_ArrivalSlotReference = "ARV SLOT";
			container.JC_DepartureSlotReference = "DEP SLOT";
			container.JC_ReleaseNum = "102030";

			ZDateTime now = ZDateTime.Now;
			ZDateTime containerSlotArrival = now;
			ZDateTime containerSlotDeparture = now.AddMinutes(5);
			ZDateTime movePickupStart = now.AddMinutes(10);
			ZDateTime movePickupEnd = now.AddMinutes(15);
			ZDateTime moveDeliveryStart = now.AddMinutes(20);
			ZDateTime moveDeliveryEnd = now.AddMinutes(25);
			ZDateTime orgPickupStart = now.AddMinutes(30);
			ZDateTime orgPickupEnd = now.AddMinutes(35);
			ZDateTime orgDeliveryStart = now.AddMinutes(40);
			ZDateTime orgDeliveryEnd = now.AddMinutes(45);

			OrgHeader ctoOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			JobDocAddress ctoDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			ctoDocAddress.E2_OA_Address = ctoOrg.MainAddress.PK;

			OrgHeader cneOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D"));
			JobDocAddress cneDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			cneDocAddress.E2_OA_Address = cneOrg.MainAddress.PK;

			OrgHeader cydOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "E"));
			JobDocAddress cydDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			cydDocAddress.E2_OA_Address = cydOrg.MainAddress.PK;

			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			move.CartageLegs.DeleteAll();
			CommonCartageLeg ctoTocne = move.CartageLegs.AddNew();
			CommonCartageLeg cneTocyd = move.CartageLegs.AddNew();

			move.EW_E2PickupAddressID = ctoDocAddress.PK;
			move.EW_E2WaitPointAddressID = cneDocAddress.PK;
			ctoTocne.JU_E2PickupAddressID = ctoDocAddress.PK;
			ctoTocne.JU_E2DeliveryAddressID = cneDocAddress.PK;
			cneTocyd.JU_E2PickupAddressID = cneDocAddress.PK;
			cneTocyd.JU_E2DeliveryAddressID = cydDocAddress.PK;

			container.JC_ArrivalSlotDateTime = containerSlotArrival;
			container.JC_DepartureSlotDateTime = containerSlotDeparture;

			cydOrg.MainAddress.OA_PickupFromTimeOnly = orgPickupStart;
			cydOrg.MainAddress.OA_PickupToTimeOnly = orgPickupEnd;
			cydOrg.MainAddress.OA_DeliverFromTimeOnly = orgDeliveryStart;
			cydOrg.MainAddress.OA_DeliverToTimeOnly = orgDeliveryEnd;

			move.EW_RequestedDeliveryTimeStart = moveDeliveryStart;
			move.EW_RequestedDeliveryTimeEnd = moveDeliveryEnd;
			move.EW_RequestedPickupTimeStart = movePickupStart;
			move.EW_RequestedPickupTimeEnd = movePickupEnd;

			DocCommonCartageLeg ctoTocneWrapper = DocCommonCartageLeg.New(ctoTocne, Factory);
			DocCommonCartageLeg cneTocydWrapper = DocCommonCartageLeg.New(cneTocyd, Factory);

			AssertEquals("SLOT:", ctoTocneWrapper.PickupReadyHeading);
			AssertEquals(containerSlotArrival, ctoTocneWrapper.PickupReady);
			AssertEquals("", ctoTocneWrapper.PickupCloseHeading);
			AssertEquals(ZDateTime.Empty, ctoTocneWrapper.PickupClose);

			AssertEquals("READY FROM:", ctoTocneWrapper.DeliveryReadyHeading);
			AssertEquals(moveDeliveryStart, ctoTocneWrapper.DeliveryReady);
			AssertEquals("READY TO:", ctoTocneWrapper.DeliveryCloseHeading);
			AssertEquals(moveDeliveryEnd, ctoTocneWrapper.DeliveryClose);

			AssertEquals("READY FROM:", cneTocydWrapper.PickupReadyHeading);
			AssertEquals(movePickupStart, cneTocydWrapper.PickupReady);
			AssertEquals("READY TO:", cneTocydWrapper.PickupCloseHeading);
			AssertEquals(movePickupEnd, cneTocydWrapper.PickupClose);

			AssertEquals("OPEN:", cneTocydWrapper.DeliveryReadyHeading);
			AssertEquals(orgDeliveryStart, cneTocydWrapper.DeliveryReady);
			AssertEquals("CLOSE:", cneTocydWrapper.DeliveryCloseHeading);
			AssertEquals(orgDeliveryEnd, cneTocydWrapper.DeliveryClose);

			cneTocyd.JU_E2PickupAddressID = ZGuid.Empty;
			cneTocyd.JU_E2DeliveryAddressID = ZGuid.Empty;
			AssertEquals("", cneTocydWrapper.PickupReadyHeading);
			AssertEquals(ZDateTime.Empty, cneTocydWrapper.PickupReady);
			AssertEquals("", cneTocydWrapper.PickupCloseHeading);
			AssertEquals(ZDateTime.Empty, cneTocydWrapper.PickupClose);

			ctoTocne.JU_E2PickupAddressID = ZGuid.Empty;
			ctoTocne.JU_E2DeliveryAddressID = ZGuid.Empty;
			AssertEquals("", cneTocydWrapper.DeliveryReadyHeading);
			AssertEquals(ZDateTime.Empty, cneTocydWrapper.DeliveryReady);
			AssertEquals("", cneTocydWrapper.DeliveryCloseHeading);
			AssertEquals(ZDateTime.Empty, cneTocydWrapper.DeliveryClose);
		}

		public void TestRemarks()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			cartage.JJ_ConsignmentID = "T12345678";
			cartage.JJ_GoodsDescription = "the goods description";

			CommonCartageLeg cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			cartageLeg.BookedCtgMove.EW_BookedPackCount = 11;
			cartageLeg.BookedCtgMove.EW_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			cartageLeg.BookedCtgMove.EW_BookedLength = 11;
			cartageLeg.BookedCtgMove.EW_BookedWidth = 12;
			cartageLeg.BookedCtgMove.EW_BookedHeight = 13;
			cartageLeg.BookedCtgMove.EW_DimUnit = Core.Constants.Length.Metres;
			cartageLeg.BookedCtgMove.EW_VolumeUQ = Core.Constants.Volume.CubicMetres;
			cartageLeg.BookedCtgMove.EW_BookedVolume = 14;
			cartageLeg.BookedCtgMove.EW_WeightUQ = Core.Constants.Weight.Kilograms;
			cartageLeg.BookedCtgMove.EW_BookedWeight = 15;

			DocCommonCartageLeg legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals(
@"PACKS: 11 PLT @ 11x12x13 M
WGT: 15 KG / VOL: 14 M3
JOB #: T12345678
the goods description", legWrapper.Remarks);

			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "CONT123456";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container.JC_ArrivalSlotReference = "ARV SLOT";
			container.JC_ArrivalSlotDateTime = new ZDateTime(2010, 1, 2, 3, 3, 0);
			container.JC_DepartureSlotReference = "DEP SLOT";
			container.JC_DepartureSlotDateTime = new ZDateTime(2010, 1, 2, 4, 4, 0);
			container.JC_ReleaseNum = "102030";

			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			move.CartageLegs.DeleteAll();
			cartageLeg = move.CartageLegs.AddNew();
			cartageLeg.JU_E2PickupAddressID = ZGuid.Empty;
			cartageLeg.JU_E2WaitPointAddressID = ZGuid.Empty;
			cartageLeg.JU_E2DeliveryAddressID = ZGuid.Empty;

			legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456 / WGT: 3830 KG
JOB #: T12345678
the goods description", legWrapper.Remarks);

			cartageLeg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456 / WGT: 3830 KG
SLOT REF: ARV SLOT @ 02-Jan-10 03:03
JOB #: T12345678
the goods description", legWrapper.Remarks);

			cartageLeg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			cartageLeg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456 / WGT: 3830 KG
SLOT REF: DEP SLOT @ 02-Jan-10 04:04
JOB #: T12345678
the goods description", legWrapper.Remarks);

			cartageLeg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard).PK;
			cartageLeg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456 / WGT(MT): 3830 KG
RELEASE #: 102030
JOB #: T12345678
the goods description", legWrapper.Remarks);
		}

		public void TestContainerLegReference()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T12345678";
			CommonCartageLeg cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			AssertNotNull(cartage.BookedMovesCollection);
			Assert(cartage.BookedMovesCollection.Count > 0);
			DocCommonCartageLeg legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals("Should be empty", ZString.Empty, legWrapper.ContainerLegReference);

			Factory.Save();
			AssertEquals("Should NOT be empty", "T12345678/A", legWrapper.ContainerLegReference);

			cartageLeg = cartage.BookedMovesCollection[0].CartageLegs.AddNew();
			legWrapper = DocCommonCartageLeg.New(cartageLeg, Factory);
			AssertEquals("Should be empty", ZString.Empty, legWrapper.ContainerLegReference);

			Factory.Save();
			AssertEquals("Should NOT be empty", "T12345678/B", legWrapper.ContainerLegReference);

			cartage.JJ_ConsignmentID = "S12345678/I";
			AssertEquals("Should NOT be empty", "S12345678/B", legWrapper.ContainerLegReference);
		}

		public void TestContainers()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			leg.JU_EW = move.PK;

			DocCommonCartageLeg legWrapper = DocCommonCartageLeg.New(leg, Factory);
			AssertEquals("No Containers", 0, legWrapper.Containers.Count);

			leg = Factory.New<CommonCartageLeg>();
			move = Factory.New<CommonBookedCtgMove>();
			CommonContainer container = Factory.New<CommonContainer>();
			leg.JU_EW = move.PK;
			move.EW_JC_Container = container.PK;

			legWrapper = DocCommonCartageLeg.New(leg, Factory);
			AssertEquals("1 Container", 1, legWrapper.Containers.Count);
			AssertEquals("Container", container, legWrapper.Containers[0].WrappedObject);
		}

		public void TestEquipmentType()
		{
			var cartage = Factory.New<CommonCartage>();
			var leg = cartage.CartageLegs.AddNew();
			CommonBookedCtgMove move = Factory.New<CommonBookedCtgMove>();
			leg.JU_EW = move.PK;
			move.EW_JJ = cartage.PK;
			cartage.JJ_DropMode = "HSL";
			var legWrapper = DocCommonCartageLeg.New(leg, Factory);
			AssertEquals(cartage.JJ_DropMode, legWrapper.EquipmentType);

			move.EW_DropMode = "ASK";
			legWrapper = DocCommonCartageLeg.New(leg, Factory);
			AssertEquals(move.EW_DropMode, legWrapper.EquipmentType);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			DocCommonCartageLeg legWrapper = DocCommonCartageLeg.New(leg, Factory);

			ZDateTime now = ZDateTime.Now;

			leg.JU_IsEmptyContainer = true;
			leg.JU_EstimatedDeliveryTime = now;

			AssertEquals("JourneyOneDeliverToHeading", "DELIVER EMPTY - EST. " + now.ToLongTimeString(), legWrapper.JourneyOneDeliverToHeading);

			leg.JU_IsEmptyContainer = false;
			AssertEquals("JourneyOneDeliverToHeading", "DELIVER FULL - EST. " + now.ToLongTimeString(), legWrapper.JourneyOneDeliverToHeading);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CommonCartageLeg leg = Factory.New<CommonCartageLeg>();
			DocCommonCartageLeg legWrapper = DocCommonCartageLeg.New(leg, Factory);
			return new DocumentWrapper[] { legWrapper };
		}
	}
}
