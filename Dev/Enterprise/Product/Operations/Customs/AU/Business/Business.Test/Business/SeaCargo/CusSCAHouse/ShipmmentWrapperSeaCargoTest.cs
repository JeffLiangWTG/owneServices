using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ShipmmentWrapperSeaCargoTest : SeaCargoTestCase
	{
		public void TestSynchroniseErrorMessageNoErrors()
		{
			CommonConsol consol = CreateLCLConsol();
			CommonContainer container = AddContainerToConsol(consol, ContainerNumber1);
			ForwardingShipment shipment1 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber1);
			ForwardingShipment shipment2 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber2);
			ShipmentWrapper wrapper = new ShipmentWrapper(shipment1);
			AssertEquals("Consol is ok to Synchronise", true, wrapper.CanSynchronise);
		}

		public void TestSynchroniseErrorMessagesInProgress()
		{
			CommonConsol consol = CreateLCLConsol();
			CommonContainer container = AddContainerToConsol(consol, ContainerNumber1);
			ForwardingShipment shipment1 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber1);
			ForwardingShipment shipment2 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber2);
			ShipmentWrapper wrapper = new ShipmentWrapper(shipment2);
			CusSCAOceanBill oceanBill = wrapper.SeaCargoSynchroniser.OceanBill;
			CusSCAHouse houseInProgress = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment2);
			houseInProgress.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
			AssertContains("Synchronise Failure Message", CMRBaseStatuses.Descriptions.AwaitingResponseToOriginal, wrapper.SynchroniseFailureMessage);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
			AssertContains("Synchronise Failure Message", CMRBaseStatuses.Descriptions.AwaitingResponseToAmendment, wrapper.SynchroniseFailureMessage);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
			AssertContains("Synchronise Failure Message", CMRBaseStatuses.Descriptions.AwaitingResponseToWithdrawal, wrapper.SynchroniseFailureMessage);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;
			AssertEquals("Original Rejected consol is ok to Synchronise", true, wrapper.CanSynchronise);
		}

		public void TestSynchroniseErrorMessageAnotherBillAcknowledged()
		{
			CommonConsol consol = CreateLCLConsol();
			CommonContainer container = AddContainerToConsol(consol, ContainerNumber1);
			ForwardingShipment shipment1 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber1);
			ForwardingShipment shipment2 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber2);
			ShipmentWrapper wrapper = new ShipmentWrapper(shipment1);
			CusSCAOceanBill oceanBill = wrapper.SeaCargoSynchroniser.OceanBill;
			CusSCAHouse houseAcknowledged = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment2);
			houseAcknowledged.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
			houseAcknowledged.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
		}

		public void TestNoSynchronisingWillOccur()
		{
			CommonConsol consol = CreateLCLConsol();
			CommonContainer container = AddContainerToConsol(consol, ContainerNumber1);
			ForwardingShipment shipment1 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber1);
			ForwardingShipment shipment2 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber2);
			ShipmentWrapper wrapper = new ShipmentWrapper(shipment2);
			CusSCAOceanBill oceanBill = wrapper.SeaCargoSynchroniser.OceanBill;
			CusSCAHouse house1 = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment1);
			CusSCAHouse house2 = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment2);
			AssertEquals("We should be able to synchronise all", true, wrapper.CanSynchronise);
			AssertEquals("Some synchronising will occure", false, wrapper.NoSynchronisingWillOccur);
			house1.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
			house1.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("We should not be able to synchronise all", false, wrapper.CanSynchronise);
			AssertEquals("Some synchronising will occure", false, wrapper.NoSynchronisingWillOccur);
			house2.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
			house2.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("We should not be able to synchronise all", false, wrapper.CanSynchronise);
			AssertEquals("No synchronising will occure", true, wrapper.NoSynchronisingWillOccur);
		}
	}
}
