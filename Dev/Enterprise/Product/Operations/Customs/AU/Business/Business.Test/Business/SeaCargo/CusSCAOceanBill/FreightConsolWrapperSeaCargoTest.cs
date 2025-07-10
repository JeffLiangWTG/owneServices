using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FreightConsolWrapperSeaCargoTest : SeaCargoTestCase
	{
		public void TestSynchroniseErrorMessageNoErrors()
		{
			var consol = CreateLCLConsol();
			var container = AddContainerToConsol(consol, ContainerNumber1);
			var shipment1 = AddShipmentToConsol(consol, HouseBillNumber1);
			var shipment2 = AddShipmentToConsol(consol, HouseBillNumber2);
			var wrapper = new SeaCargo.FreightConsolWrapper(consol);
			AssertEquals("Consol is ok to Synchronise", true, wrapper.CanSynchronise);
		}

		public void TestSynchroniseErrorMessagesInProgress()
		{
			var consol = CreateLCLConsol();
			var container = AddContainerToConsol(consol, ContainerNumber1);
			var shipment1 = AddShipmentToConsol(consol, HouseBillNumber1);
			var shipment2 = AddShipmentToConsol(consol, HouseBillNumber2);
			var wrapper = new SeaCargo.FreightConsolWrapper(consol);
			var oceanBill = wrapper.SeaCargoSynchroniser.OceanBill;
			var houseInProgress = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment2);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			houseInProgress.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToAmendment;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
			houseInProgress.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;
			AssertEquals("Original Rejected consol is ok to Synchronise", true, wrapper.CanSynchronise);
		}

		public void TestSynchroniseErrorMessageAnotherBillAcknowledged()
		{
			var consol = CreateLCLConsol();
			var container = AddContainerToConsol(consol, ContainerNumber1);
			var shipment1 = AddShipmentToConsol(consol, HouseBillNumber1);
			var shipment2 = AddShipmentToConsol(consol, HouseBillNumber2);
			var wrapper = new SeaCargo.FreightConsolWrapper(consol);
			var oceanBill = wrapper.SeaCargoSynchroniser.OceanBill;
			var houseAcknowledged = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment2);
			houseAcknowledged.Messages.Add(Factory.New(typeof(CMRSEACRMessage)));
			houseAcknowledged.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			AssertEquals("We should not be able to synchronise now", false, wrapper.CanSynchronise);
		}

		public void TestNoSynchronisingWillOccur()
		{
			var consol = CreateLCLConsol();
			var container = AddContainerToConsol(consol, ContainerNumber1);
			var shipment1 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber1);
			var shipment2 = (ForwardingShipment)AddShipmentToConsol(consol, HouseBillNumber2);
			var wrapper = new SeaCargo.FreightConsolWrapper(consol);
			var oceanBill = wrapper.SeaCargoSynchroniser.OceanBill;
			var house1 = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment1);
			var house2 = wrapper.SeaCargoSynchroniser.GetHouseBill(shipment2);
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
