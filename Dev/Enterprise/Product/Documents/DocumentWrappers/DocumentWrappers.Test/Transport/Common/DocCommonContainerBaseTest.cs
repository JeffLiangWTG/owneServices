using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCommonContainer))]
	sealed class DocCommonContainerBaseTest : DocumentWrapperTestCase
	{
		#region Property Tests

		public void TestIDocCartageAdviceDates()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Containers.Add(container);

			Transport transport1 = consol.Transports[0];
			transport1.JW_ETD = new ZDateTime(2011, 2, 1);
			transport1.JW_ETA = new ZDateTime(2011, 2, 15);

			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_ETD = new ZDateTime(2011, 1, 1);
			transport2.JW_ETA = new ZDateTime(2011, 1, 15);
			transport2.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			transport2.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			transport2.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			transport2.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_ETD = new ZDateTime(2011, 4, 1);
			transport3.JW_ETA = new ZDateTime(2011, 4, 15);
			transport3.JW_TerminalAvailabilityDate = new ZDateTime(2011, 4, 18);
			transport3.JW_DepotAvailabilityDate = new ZDateTime(2011, 4, 20);
			transport3.JW_TerminalStorageDate = new ZDateTime(2011, 4, 22);
			transport3.JW_DepotStorageDate = new ZDateTime(2011, 4, 25);

			Transport transport4 = consol.Transports.AddNew();
			transport4.JW_ETD = new ZDateTime(2011, 3, 1);
			transport4.JW_ETA = new ZDateTime(2011, 3, 15);

			DocCommonContainer wrapper = DocCommonContainer.New(container, cartage, Factory);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 18), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 22), wrapper.CartageStorageCommenceDate);

			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), wrapper.CartageCutOffDate);
			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 4, 20), wrapper.CartageAvailableDate);
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), wrapper.CartageReceivalDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 4, 25), wrapper.CartageStorageCommenceDate);
		}

		public void TestEstimatedPickup()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();

			leg1.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			leg1.JU_E2DeliveryAddressID = cartage.SecondDocAddress.PK;
			leg2.JU_E2PickupAddressID = cartage.SecondDocAddress.PK;
			leg2.JU_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;

			DocCommonContainer docCommonContainer = DocCommonContainer.New(container, cartage, Factory);
			AssertEquals(ZDateTime.Empty, docCommonContainer.EstimatedPickup);

			ZDateTime now = ZDateTime.Now;
			container.JC_DepartureEstimatedPickup = now.AddDays(1);
			AssertEquals(now.AddDays(1), docCommonContainer.EstimatedPickup);

			leg1.JU_PlannedPickupTime = now.AddDays(2);
			AssertEquals(now.AddDays(1), docCommonContainer.EstimatedPickup);

			leg2.JU_PlannedPickupTime = now.AddDays(3);
			AssertEquals(now.AddDays(3), docCommonContainer.EstimatedPickup);
		}

		public void TestEstimatedDelivery()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();

			leg1.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			leg1.JU_E2DeliveryAddressID = cartage.SecondDocAddress.PK;
			leg2.JU_E2PickupAddressID = cartage.SecondDocAddress.PK;
			leg2.JU_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;

			DocCommonContainer docCommonContainer = DocCommonContainer.New(container, cartage, Factory);
			AssertEquals(ZDateTime.Empty, docCommonContainer.EstimatedDelivery);

			ZDateTime now = ZDateTime.Now;
			container.JC_ArrivalEstimatedDelivery = now.AddDays(1);
			AssertEquals(now.AddDays(1), docCommonContainer.EstimatedDelivery);

			leg2.JU_EstimatedDeliveryTime = now.AddDays(2);
			AssertEquals(now.AddDays(1), docCommonContainer.EstimatedDelivery);

			leg1.JU_EstimatedDeliveryTime = now.AddDays(3);
			AssertEquals(now.AddDays(3), docCommonContainer.EstimatedDelivery);
		}

		#region TestPrintTwoJourneys

		public void TestPrintTwoHourneys()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			DocCommonContainer doc = DocCommonContainer.New(container, cartage, Factory);
			AssertEquals(false, doc.PrintTwoJourneys);
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			AssertEquals(false, doc.PrintTwoJourneys);
			move.CartageLegs.AddNew();
			AssertEquals(false, doc.PrintTwoJourneys);
			move.CartageLegs.AddNew();
			AssertEquals(true, doc.PrintTwoJourneys);
		}

		#endregion

		#region Hourney Heading

		public void TestJourneyOnePickUpHeading()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			DocCommonContainer doc = DocCommonContainer.New(container, cartage, Factory);
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();

			AssertEquals("PICKUP FULL", doc.JourneyOnePickUpHeading);

			JobDocAddress pickUpAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			leg1.JU_E2PickupAddressID = pickUpAddress.PK;
			container.JC_ReleaseNum = "BR";

			AssertEquals("PICKUP EMPTY REF. BR", doc.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverHeading()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			DocCommonContainer doc = DocCommonContainer.New(container, cartage, Factory);
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();

			AssertEquals("DELIVER TO FULL", doc.JourneyOneDeliverToHeading);

			JobDocAddress pickUpAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			leg1.JU_E2PickupAddressID = pickUpAddress.PK;
			container.JC_ReleaseNum = "BR";

			AssertEquals("DELIVER TO EMPTY", doc.JourneyOneDeliverToHeading);

			ZDateTime now = ZDateTime.Now;
			container.JC_EmptyRequired = now;
			AssertEquals("DELIVER TO EMPTY DATE " + now.ToLongTimeString(), doc.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			DocCommonContainer doc = DocCommonContainer.New(container, cartage, Factory);
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();

			AssertEquals("PICKUP FULL", doc.JourneyTwoPickUpHeading);

			JobDocAddress pickUpAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			leg1.JU_E2PickupAddressID = pickUpAddress.PK;
			container.JC_ReleaseNum = "BR";

			AssertEquals("PICKUP EMPTY", doc.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverHeading()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			DocCommonContainer doc = DocCommonContainer.New(container, cartage, Factory);
			CommonBookedCtgMove move = cartage.GetBookedMoves(container)[0];
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();

			AssertEquals("DELIVER TO FULL", doc.JourneyTwoDeliverToHeading);

			JobDocAddress pickUpAddress = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageYard);
			leg1.JU_E2PickupAddressID = pickUpAddress.PK;
			container.JC_ReleaseNum = "BR";

			AssertEquals("DELIVER TO EMPTY", doc.JourneyTwoDeliverToHeading);
		}

		#endregion

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			return new DocumentWrapper[] { DocCommonContainer.New(cartage.ContainerBookedMoves.AddNew().Container, cartage, Factory) };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			return DocCommonContainer.New(cartage.ContainerBookedMoves.AddNew().Container, cartage, Factory);
		}
	}
}
