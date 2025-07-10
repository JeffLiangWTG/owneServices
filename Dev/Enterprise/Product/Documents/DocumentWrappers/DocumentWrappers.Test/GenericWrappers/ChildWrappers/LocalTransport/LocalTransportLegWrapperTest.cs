using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LocalTransportLegWrapper))]
	sealed class LocalTransportLegWrapperTest : GenericWrapperTest
	{
		#region TestProperties

		public void TestProperties()
		{
			var freeWaitingTimeCollection = new FreeWaitingTimeCollection();
			var freeWaitingTime = freeWaitingTimeCollection.AddNew();
			freeWaitingTime.DropMode = Constants.EquipmentNeeded.Any;
			freeWaitingTime.CTO = new ZDateTime(2012, 1, 1, 0, 10, 0);
			freeWaitingTime.CNR = new ZDateTime(2012, 1, 1, 0, 20, 0);
			freeWaitingTime.CNE = new ZDateTime(2012, 1, 1, 0, 20, 0);
			freeWaitingTime.CYD = new ZDateTime(2012, 1, 1, 0, 30, 0);

			TransportRegistry.Instance.AmountOfFreeWaitingTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, freeWaitingTimeCollection);

			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNEWAITtoCYD, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			leg.JU_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			leg.JU_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;

			var legWrapper = new LocalTransportLegWrapper(leg, Factory);

			leg.JU_PlannedPickupTime = new ZDateTime(2010, 1, 1);
			leg.JU_PlannedPickupTimeEnd = new ZDateTime(2010, 1, 2);
			leg.JU_PickupTimeIn = new ZDateTime(2010, 1, 3, 10, 0, 0);
			leg.JU_PickupTimeOut = new ZDateTime(2010, 1, 4, 10, 25, 0);

			leg.JU_WaitPointTimeIn = new ZDateTime(2010, 1, 6, 10, 0, 0);
			leg.JU_WaitPointTimeOut = new ZDateTime(2010, 1, 7, 11, 0, 0);

			leg.JU_EstimatedDeliveryTime = new ZDateTime(2010, 1, 8);
			leg.JU_EstimatedDeliveryTimeEnd = new ZDateTime(2010, 1, 9);
			leg.JU_DeliverTimeIn = new ZDateTime(2010, 1, 10, 10, 0, 0);
			leg.JU_DeliverTimeOut = new ZDateTime(2010, 1, 10, 10, 15, 0);

			leg.JU_DeliverySignedFor = "Bob";
			leg.JU_RunSheetSequence = 4;
			leg.JU_LegNotes = "Leg n0tes";
			leg.JU_DisplayOrder = 3;

			var wrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals("Planned Pickup Time.", new ZDateTime(2010, 1, 1), wrapper.PlannedPickupTime);
			AssertEquals("Planned Pickup Time End.", new ZDateTime(2010, 1, 2), wrapper.PlannedPickupTimeEnd);
			AssertEquals("Pickup Time In.", new ZDateTime(2010, 1, 3, 10, 0, 0), wrapper.PickupTimeIn);
			AssertEquals("Pickup Time Out.", new ZDateTime(2010, 1, 4, 10, 25, 0), wrapper.PickupTimeOut);
			AssertEquals("Pickup Demurrage.", new ZString("24:15"), wrapper.PickupTimeDemurrage);

			AssertEquals("Planned Wait Point Time.", new ZDateTime(2010, 1, 8), wrapper.PlannedWaitPointTime);
			AssertEquals("Planned Wait Point End.", new ZDateTime(2010, 1, 9), wrapper.PlannedWaitPointTimeEnd);
			AssertEquals("Wait Point Time In.", new ZDateTime(2010, 1, 6, 10, 0, 0), wrapper.WaitPointTimeIn);
			AssertEquals("Wait Point Time Out.", new ZDateTime(2010, 1, 7, 11, 0, 0), wrapper.WaitPointTimeOut);
			AssertEquals("Wait Point Demurrage.", new ZString("24:40"), wrapper.WaitPointTimeDemurrage);

			AssertEquals("Planned Delivery Time.", new ZDateTime(2010, 1, 8), wrapper.PlannedDeliveryTime);
			AssertEquals("Planned Delivery Time End.", new ZDateTime(2010, 1, 9), wrapper.PlannedDeliveryTimeEnd);
			AssertEquals("Delivery Time In.", new ZDateTime(2010, 1, 10, 10, 0, 0), wrapper.DeliveryTimeIn);
			AssertEquals("Delivery Time Out.", new ZDateTime(2010, 1, 10, 10, 15, 0), wrapper.DeliveryTimeOut);
			AssertEquals("Delivery Demurrage.", new ZString(""), wrapper.DeliveryTimeDemurrage);

			AssertEquals("Signed For.", "Bob", wrapper.DeliverySignedFor);
			AssertEquals("Sequence.", 4, wrapper.Sequence);
			AssertEquals("Leg Notes", "Leg n0tes", wrapper.LegNotes);
			AssertEquals("Display Order.", 3, wrapper.DisplayOrder);
			AssertEquals("IsLoose", false, wrapper.IsLoose);

			cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirImport, 1);
			move = cartage.LooseBookedMoves[0];
			leg = move.CartageLegs.AddNew();
			wrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals("IsLoose", true, wrapper.IsLoose);
		}

		public void TestIsDeliveryTheRequestedBooking()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			var legWrapper = new LocalTransportLegWrapper(leg, Factory);

			// CNR as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			AssertEquals(false, legWrapper.IsDeliveryTheRequestedBooking);

			// CNR as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(false, legWrapper.IsDeliveryTheRequestedBooking);

			// CFS as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(false, legWrapper.IsDeliveryTheRequestedBooking);

			// CFS as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			AssertEquals(true, legWrapper.IsDeliveryTheRequestedBooking);

			// CNE as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			AssertEquals(true, legWrapper.IsDeliveryTheRequestedBooking);

			// CNE as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			AssertEquals(true, legWrapper.IsDeliveryTheRequestedBooking);
		}

		#endregion

		#region TestIsPickupTheRequestedBooking

		public void TestIsPickupTheRequestedBooking()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			var legWrapper = new LocalTransportLegWrapper(leg, Factory);

			// CNR as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			AssertEquals(true, legWrapper.IsPickupTheRequestedBooking);

			// CNR as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			AssertEquals(true, legWrapper.IsPickupTheRequestedBooking);

			// CFS as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			AssertEquals(false, legWrapper.IsPickupTheRequestedBooking);

			// CFS as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(false, legWrapper.IsPickupTheRequestedBooking);

			// CNE as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(false, legWrapper.IsPickupTheRequestedBooking);

			// CNE as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			AssertEquals(false, legWrapper.IsPickupTheRequestedBooking);
		}

		#endregion

		#region TestIsWaitPointTheRequestedBooking

		public void TestIsWaitPointTheRequestedBooking()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			var legWrapper = new LocalTransportLegWrapper(leg, Factory);

			// CNR as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			leg.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			AssertEquals(false, legWrapper.IsWaitPointTheRequestedBooking);

			// CNR as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageExporter).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			leg.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(false, legWrapper.IsWaitPointTheRequestedBooking);

			// CFS as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			leg.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(false, legWrapper.IsWaitPointTheRequestedBooking);

			// CFS as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			leg.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			AssertEquals(true, legWrapper.IsWaitPointTheRequestedBooking);

			// CNE as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			leg.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			AssertEquals(true, legWrapper.IsWaitPointTheRequestedBooking);

			// CNE as Requested Address Type
			move.EW_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			move.EW_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			leg.JU_E2WaitPointAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter).PK;
			AssertEquals(true, legWrapper.IsWaitPointTheRequestedBooking);
		}

		#endregion

		#region TestRelatedObjects

		#region TestBookedMove

		public void TestBookedMove()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var leg = move.CartageLegs.AddNew();
			var legWrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals(move, legWrapper.BookedMove.WrappedObject);
		}

		#endregion

		#endregion

		#region TestSignature

		public void TestSignature()
		{
			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var leg = Factory.New<CommonCartageLeg>();
			var wrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals(false, wrapper.HasSignature);

			var stm = Factory.New<StmData>();
			var data = new SignatureData(leg);
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
			AssertEquals(false, wrapper.HasSignature);

			TransportRegistry.Instance.ShowLegSignatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, wrapper.HasSignature);
			AssertNotNull(wrapper.Signature);
		}

		#endregion

		#region TestPickupDeliverySlotOpenCloseDates

		public void TestPickupDeliverySlotOpenCloseDates()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			cartage.JJ_ConsignmentID = "T12345678";
			cartage.JJ_GoodsDescription = "the goods description";

			var container = cartage.Containers.First();
			container.JC_ContainerNum = "CONT123456";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container.JC_ArrivalSlotReference = "ARV SLOT";
			container.JC_DepartureSlotReference = "DEP SLOT";
			container.JC_ReleaseNum = "102030";

			var now = ZDateTime.Now;
			var containerSlotArrival = now;
			var containerSlotDeparture = now.AddMinutes(5);
			var movePickupStart = now.AddMinutes(10);
			var movePickupEnd = now.AddMinutes(15);
			var moveDeliveryStart = now.AddMinutes(20);
			var moveDeliveryEnd = now.AddMinutes(25);
			var orgPickupStart = new ZDateTime(2010, 1, 1, 7, 0, 0);
			var orgPickupEnd = new ZDateTime(2010, 1, 1, 10, 0, 0);
			var orgDeliveryStart = new ZDateTime(2010, 1, 1, 12, 0, 0);
			var orgDeliveryEnd = new ZDateTime(2010, 1, 1, 14, 0, 0);

			var ctoOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			var ctoDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			ctoDocAddress.E2_OA_Address = ctoOrg.MainAddress.PK;

			var cneOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "D"));
			var cneDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			cneDocAddress.E2_OA_Address = cneOrg.MainAddress.PK;

			var cydOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "E"));
			var cydDocAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			cydDocAddress.E2_OA_Address = cydOrg.MainAddress.PK;

			var move = cartage.BookedMovesCollection[0];
			move.CartageLegs.DeleteAll();
			var ctoTocne = move.CartageLegs.AddNew();
			var cneTocyd = move.CartageLegs.AddNew();
			var cneTocto = move.CartageLegs.AddNew();
			var cneToctoTocyd = move.CartageLegs.AddNew();
			var cydTocneTocto = move.CartageLegs.AddNew();

			move.EW_E2PickupAddressID = ctoDocAddress.PK;
			move.EW_E2WaitPointAddressID = cneDocAddress.PK;
			ctoTocne.JU_E2PickupAddressID = ctoDocAddress.PK;
			ctoTocne.JU_E2DeliveryAddressID = cneDocAddress.PK;
			cneTocyd.JU_E2PickupAddressID = cneDocAddress.PK;
			cneTocyd.JU_E2DeliveryAddressID = cydDocAddress.PK;
			cneTocto.JU_E2PickupAddressID = cneDocAddress.PK;
			cneTocto.JU_E2DeliveryAddressID = ctoDocAddress.PK;
			cneToctoTocyd.JU_E2PickupAddressID = cneDocAddress.PK;
			cneToctoTocyd.JU_E2WaitPointAddressID = ctoDocAddress.PK;
			cneToctoTocyd.JU_E2DeliveryAddressID = cydDocAddress.PK;
			cydTocneTocto.JU_E2PickupAddressID = cydDocAddress.PK;
			cydTocneTocto.JU_E2WaitPointAddressID = cneDocAddress.PK;
			cydTocneTocto.JU_E2DeliveryAddressID = ctoDocAddress.PK;

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

			var ctoTocneWrapper = new LocalTransportLegWrapper(ctoTocne, Factory);
			var cneTocydWrapper = new LocalTransportLegWrapper(cneTocyd, Factory);
			var cneToctoWrapper = new LocalTransportLegWrapper(cneTocto, Factory);
			var cneToctoTocydWrapper = new LocalTransportLegWrapper(cneToctoTocyd, Factory);
			var cydTocneToctoWrapper = new LocalTransportLegWrapper(cydTocneTocto, Factory);

			AssertEquals("SLOT:", ctoTocneWrapper.PickupReadyHeading);
			AssertEquals(containerSlotArrival.ToLongTimeString(), ctoTocneWrapper.PickupReady);
			AssertEquals("", ctoTocneWrapper.PickupCloseHeading);
			AssertEquals("", ctoTocneWrapper.PickupClose);

			AssertEquals("READY FROM:", ctoTocneWrapper.DeliveryReadyHeading);
			AssertEquals(moveDeliveryStart.ToLongTimeString(), ctoTocneWrapper.DeliveryReady);
			AssertEquals("READY TO:", ctoTocneWrapper.DeliveryCloseHeading);
			AssertEquals(moveDeliveryEnd.ToLongTimeString(), ctoTocneWrapper.DeliveryClose);

			AssertEquals("READY FROM:", cneTocydWrapper.PickupReadyHeading);
			AssertEquals(movePickupStart.ToLongTimeString(), cneTocydWrapper.PickupReady);
			AssertEquals("READY TO:", cneTocydWrapper.PickupCloseHeading);
			AssertEquals(movePickupEnd.ToLongTimeString(), cneTocydWrapper.PickupClose);

			AssertEquals("OPEN:", cneTocydWrapper.DeliveryReadyHeading);
			AssertEquals(orgDeliveryStart.ToShortTimeString(), cneTocydWrapper.DeliveryReady);
			AssertEquals("CLOSE:", cneTocydWrapper.DeliveryCloseHeading);
			AssertEquals(orgDeliveryEnd.ToShortTimeString(), cneTocydWrapper.DeliveryClose);

			AssertEquals("SLOT:", cneToctoWrapper.DeliveryReadyHeading);
			AssertEquals(containerSlotDeparture.ToLongTimeString(), cneToctoWrapper.DeliveryReady);
			AssertEquals("", cneToctoWrapper.DeliveryCloseHeading);
			AssertEquals("", cneToctoWrapper.DeliveryClose);

			AssertEquals("SLOT:", cneToctoTocydWrapper.WaitPointReadyHeading);
			AssertEquals(containerSlotArrival.ToLongTimeString(), cneToctoTocydWrapper.WaitPointReady);
			AssertEquals("", cneToctoTocydWrapper.WaitPointCloseHeading);
			AssertEquals("", cneToctoTocydWrapper.WaitPointClose);

			AssertEquals("READY FROM:", cydTocneToctoWrapper.WaitPointReadyHeading);
			AssertEquals(moveDeliveryStart.ToLongTimeString(), cydTocneToctoWrapper.WaitPointReady);
			AssertEquals("READY TO:", cydTocneToctoWrapper.WaitPointCloseHeading);
			AssertEquals(moveDeliveryEnd.ToLongTimeString(), cydTocneToctoWrapper.WaitPointClose);

			cneTocyd.JU_E2PickupAddressID = ZGuid.Empty;
			cneTocyd.JU_E2DeliveryAddressID = ZGuid.Empty;
			AssertEquals("", cneTocydWrapper.PickupReadyHeading);
			AssertEquals("", cneTocydWrapper.PickupReady);
			AssertEquals("", cneTocydWrapper.PickupCloseHeading);
			AssertEquals("", cneTocydWrapper.PickupClose);

			ctoTocne.JU_E2PickupAddressID = ZGuid.Empty;
			ctoTocne.JU_E2DeliveryAddressID = ZGuid.Empty;
			AssertEquals("", cneTocydWrapper.DeliveryReadyHeading);
			AssertEquals("", cneTocydWrapper.DeliveryReady);
			AssertEquals("", cneTocydWrapper.DeliveryCloseHeading);
			AssertEquals("", cneTocydWrapper.DeliveryClose);
		}

		#endregion

		#region TestRemarks

		public void TestRemarks()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			cartage.JJ_ConsignmentID = "T12345678";
			cartage.JJ_GoodsDescription = "the goods description";

			var cartageLeg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
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

			var legWrapper = new LocalTransportLegWrapper(cartageLeg, Factory);
			AssertEquals(
@"PACKS: 11 PLT @ 11x12x13 M
WGT: 15 KG / VOL: 14 M3
JOB #: T12345678
the goods description", legWrapper.Remarks);

			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_ContainerNum = "CONT123456";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			container.JC_ArrivalSlotReference = "ARV SLOT";
			container.JC_ArrivalSlotDateTime = new ZDateTime(2010, 1, 2, 3, 3, 0);
			container.JC_DepartureSlotReference = "DEP SLOT";
			container.JC_DepartureSlotDateTime = new ZDateTime(2010, 1, 2, 4, 4, 0);
			container.JC_ReleaseNum = "102030";

			var move = cartage.GetBookedMoves(container)[0];
			move.CartageLegs.DeleteAll();
			cartageLeg = move.CartageLegs.AddNew();
			cartageLeg.JU_E2PickupAddressID = ZGuid.Empty;
			cartageLeg.JU_E2WaitPointAddressID = ZGuid.Empty;
			cartageLeg.JU_E2DeliveryAddressID = ZGuid.Empty;

			legWrapper = new LocalTransportLegWrapper(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456   JOB #: T12345678
TARE: 3830.0 KG  NET: 0.0 KG  GROSS: 3830.0 KG
the goods description", legWrapper.Remarks);

			cartageLeg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			legWrapper = new LocalTransportLegWrapper(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456   JOB #: T12345678
TARE: 3830.0 KG  NET: 0.0 KG  GROSS: 3830.0 KG
SLOT REF: ARV SLOT @ 02-Jan-10 03:03
the goods description", legWrapper.Remarks);

			cartageLeg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			cartageLeg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			legWrapper = new LocalTransportLegWrapper(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456   JOB #: T12345678
TARE: 3830.0 KG  NET: 0.0 KG  GROSS: 3830.0 KG
SLOT REF: DEP SLOT @ 02-Jan-10 04:04
the goods description", legWrapper.Remarks);

			cartageLeg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard).PK;
			cartageLeg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS).PK;
			legWrapper = new LocalTransportLegWrapper(cartageLeg, Factory);
			AssertEquals(
@"40GP - CONT123456   JOB #: T12345678
TARE: 3830.0 KG  NET: 0.0 KG  GROSS: 3830.0 KG
RELEASE #: 102030
the goods description", legWrapper.Remarks);
		}

		#endregion

		#region TestIsPickupCTO

		public void TestIsPickupCTO()
		{
			var cartage = Factory.New<CommonCartage>();
			var leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			var wrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals(false, wrapper.IsPickupCTO);

			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(true, wrapper.IsPickupCTO);
		}

		#endregion

		#region TestIsPickupContainerYard

		public void TestIsPickupContainerYard()
		{
			var cartage = Factory.New<CommonCartage>();
			var leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			var wrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals(false, wrapper.IsPickupContainerYard);

			leg.JU_E2PickupAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard).PK;
			AssertEquals(true, wrapper.IsPickupContainerYard);
		}

		#endregion

		#region TestIsDeliveryCTO

		public void TestIsDeliveryCTO()
		{
			var cartage = Factory.New<CommonCartage>();
			var leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			var wrapper = new LocalTransportLegWrapper(leg, Factory);
			AssertEquals(false, wrapper.IsDeliveryCTO);

			leg.JU_E2DeliveryAddressID = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO).PK;
			AssertEquals(true, wrapper.IsDeliveryCTO);
		}

		#endregion

		#region Overrides

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
LocalTransportLeg
======================================================================
Name                                    Type
----------------------------------------------------------------------
DeliverTo                               Address
PickupFrom                              Address
WaitPoint                               Address
Container                               Container
Cartage                                 Freight
BookedMove                              LocalTransportBookedMove
Client                                  Organisation
DeliveryClose                           String
DeliveryCloseHeading                    String
DeliveryReady                           String
DeliveryReadyHeading                    String
DeliverySignedFor                       String
DeliveryTimeDemurrage                   String
DeliveryTimeIn                          DateTime
DeliveryTimeOut                         DateTime
DisplayOrder                            Int
HasSignature                            Bool
HasWaitPoint                            Bool
IsDeliveryCTO                           Bool
IsDeliveryTheRequestedBooking           Bool
IsLoose                                 Bool
IsPickupContainerYard                   Bool
IsPickupCTO                             Bool
IsPickupTheRequestedBooking             Bool
IsWaitPointTheRequestedBooking          Bool
LegNotes                                String
PickupClose                             String
PickupCloseHeading                      String
PickupReady                             String
PickupReadyHeading                      String
PickupTimeDemurrage                     String
PickupTimeIn                            DateTime
PickupTimeOut                           DateTime
PlannedDeliveryTime                     DateTime
PlannedDeliveryTimeEnd                  DateTime
PlannedPickupTime                       DateTime
PlannedPickupTimeEnd                    DateTime
PlannedWaitPointTime                    DateTime
PlannedWaitPointTimeEnd                 DateTime
Remarks                                 String
Sequence                                Int
WaitPointClose                          String
WaitPointCloseHeading                   String
WaitPointReady                          String
WaitPointReadyHeading                   String
WaitPointTimeDemurrage                  String
WaitPointTimeIn                         DateTime
WaitPointTimeOut                        DateTime

UNDGSubstances                          UNDGSubstance Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return
@"BookedMove : (No Default Field Value Available on LocalTransportBookedMove)
Cartage : 
Client : 
Container : 
DeliverTo : 
PickupFrom : 
Registry : (No Default Field Value Available on Registry)
WaitPoint :";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new LocalTransportLegWrapper(Factory.New<CommonCartageLeg>(), Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new LocalTransportLegWrapper(Factory.New<CommonCartageLeg>(), Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			LocalTransportLegWrapper wrapperEmpty = new LocalTransportLegWrapper(null, Factory);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.PlannedPickupTime);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.PlannedPickupTimeEnd);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.PickupTimeIn);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.PickupTimeOut);
			AssertEquals("", wrapperEmpty.PickupReady);
			AssertEquals("", wrapperEmpty.PickupReadyHeading);
			AssertEquals("", wrapperEmpty.PickupClose);
			AssertEquals("", wrapperEmpty.PickupCloseHeading);

			AssertEquals(ZDateTime.Empty, wrapperEmpty.PlannedWaitPointTime);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.PlannedWaitPointTimeEnd);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.WaitPointTimeIn);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.WaitPointTimeOut);
			AssertEquals("", wrapperEmpty.WaitPointReady);
			AssertEquals("", wrapperEmpty.WaitPointReadyHeading);
			AssertEquals("", wrapperEmpty.WaitPointClose);
			AssertEquals("", wrapperEmpty.WaitPointCloseHeading);

			AssertEquals(ZDateTime.Empty, wrapperEmpty.PlannedDeliveryTime);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.PlannedDeliveryTimeEnd);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.DeliveryTimeIn);
			AssertEquals(ZDateTime.Empty, wrapperEmpty.DeliveryTimeOut);
			AssertEquals("", wrapperEmpty.DeliveryReady);
			AssertEquals("", wrapperEmpty.DeliveryReadyHeading);
			AssertEquals("", wrapperEmpty.DeliveryClose);
			AssertEquals("", wrapperEmpty.DeliveryCloseHeading);

			AssertEquals("", wrapperEmpty.DeliverySignedFor);
			AssertEquals(0, wrapperEmpty.Sequence);
			AssertEquals("", wrapperEmpty.LegNotes);
			AssertEquals("", wrapperEmpty.Remarks);
			AssertEquals(false, wrapperEmpty.HasSignature);
			AssertEquals(false, wrapperEmpty.HasWaitPoint);

			AssertEquals("", wrapperEmpty.PickupFrom.CompanyName);
			AssertEquals("", wrapperEmpty.DeliverTo.CompanyName);
			AssertEquals("", wrapperEmpty.Container.ContainerNo);
			AssertEquals("", wrapperEmpty.Cartage.JobNumber);
		}

		#endregion

		#region Helper

		LocalCartageTestHelper Helper
		{
			get { return new LocalCartageTestHelper(Factory); }
		}

		#endregion

	}
}
