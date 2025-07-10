using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_OA_Address()
		{
			var house = Master.HouseBills.AddNew();
			var consignee = house.DocAddresses.CreateWithAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			consignee.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var shipper = house.DocAddresses.CreateWithAddressType(DocAddressType.ConsignorDocumentaryAddress);
			shipper.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var deliveryAddress = house.DocAddresses.CreateWithAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			deliveryAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var notifyParty = house.DocAddresses.CreateWithAddressType(DocAddressType.NotifyParty);
			notifyParty.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var broker = house.DocAddresses.CreateWithAddressType(DocAddressType.ImportBroker);
			broker.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var forwarder = house.DocAddresses.CreateWithAddressType(DocAddressType.ReceivingForwarderAddress);
			forwarder.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var carrier = house.DocAddresses.CreateWithAddressType(DocAddressType.Carrier);
			carrier.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var warhhouse = house.DocAddresses.CreateWithAddressType(DocAddressType.Warehouse);
			warhhouse.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var consolidator = house.DocAddresses.CreateWithAddressType(DocAddressType.Consolidator);
			consolidator.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var placeOfConsolidation = house.DocAddresses.CreateWithAddressType(DocAddressType.PlaceOfConsolidation);
			placeOfConsolidation.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);

			AssertEquals("House bill doesn't have Message Errors not to show duplicate message.", false, house.HasMessageErrors);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(consignee);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(shipper);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(deliveryAddress);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(notifyParty);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(broker);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(forwarder);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(carrier);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(warhhouse);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(consolidator);
			CAAddressValidatorTest.AssertAddressCityNameValidationIfNotEntered(placeOfConsolidation);

			house.ClearAllNotifications();
			consignee.E2_Contact = "AAAAAAAAAASSSSSSSSSSDDDDDDDDDDFFFFFFFFFFGGGGGGGGGGHHHHHHHHHHJJJJJJJJJJ";
			house.Validation.ValidateAll();
			AssertEquals("House bill doesn't have Message Errors not to show duplicate message.", true, house.HasMessageErrors);
			AssertEquals("When HouseBill has no message errors we will validate addresses with warnings.", false, consignee.E2_OA_AddressInfo.HasWarnings());
		}

		public void TestCheckE2_City()
		{
			var house = Master.HouseBills.AddNew();
			var consignee = house.DocAddresses.CreateWithAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			consignee.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var shipper = house.DocAddresses.CreateWithAddressType(DocAddressType.ConsignorDocumentaryAddress);
			shipper.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var deliveryAddress = house.DocAddresses.CreateWithAddressType(DocAddressType.ConsigneePickupDeliveryAddress);
			deliveryAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var notifyParty = house.DocAddresses.CreateWithAddressType(DocAddressType.NotifyParty);
			notifyParty.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var broker = house.DocAddresses.CreateWithAddressType(DocAddressType.ImportBroker);
			broker.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var forwarder = house.DocAddresses.CreateWithAddressType(DocAddressType.ReceivingForwarderAddress);
			forwarder.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var carrier = house.DocAddresses.CreateWithAddressType(DocAddressType.Carrier);
			carrier.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var warhhouse = house.DocAddresses.CreateWithAddressType(DocAddressType.Warehouse);
			warhhouse.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var consolidator = house.DocAddresses.CreateWithAddressType(DocAddressType.Consolidator);
			consolidator.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			var placeOfConsolidation = house.DocAddresses.CreateWithAddressType(DocAddressType.PlaceOfConsolidation);
			placeOfConsolidation.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);

			CombineAssertions("Check E2_City", () =>
			{
				consignee.E2_AddressOverride = true;
				consignee.E2_City = ZString.Empty;
				consignee.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(consignee.E2_CityInfo, "You have not entered a Consignee: City.");
				consignee.E2_City = "City";
				consignee.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(consignee.E2_CityInfo, "You have not entered a Consignee: City.");

				shipper.E2_AddressOverride = true;
				shipper.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(shipper.E2_CityInfo, "You have not entered a Shipper: City.");
				shipper.E2_City = "City";
				shipper.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(shipper.E2_CityInfo, "You have not entered a Shipper: City.");

				notifyParty.E2_AddressOverride = true;
				notifyParty.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(notifyParty.E2_CityInfo, "You have not entered a Notify Party: City.");
				notifyParty.E2_City = "City";
				notifyParty.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(notifyParty.E2_CityInfo, "You have not entered a Notify Party: City.");

				deliveryAddress.E2_AddressOverride = true;
				deliveryAddress.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(deliveryAddress.E2_CityInfo, "You have not entered a Delivery Address: City.");
				deliveryAddress.E2_City = "City";
				deliveryAddress.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(deliveryAddress.E2_CityInfo, "You have not entered a Delivery Address: City.");

				broker.E2_AddressOverride = true;
				broker.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(broker.E2_CityInfo, "You have not entered a Broker: City.");
				broker.E2_City = "City";
				broker.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(broker.E2_CityInfo, "You have not entered a Broker: City.");

				forwarder.E2_AddressOverride = true;
				forwarder.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(forwarder.E2_CityInfo, "You have not entered a Forwarder: City.");
				forwarder.E2_City = "City";
				forwarder.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(forwarder.E2_CityInfo, "You have not entered a Forwarder: City.");

				carrier.E2_AddressOverride = true;
				carrier.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(carrier.E2_CityInfo, "You have not entered a Carrier: City.");
				carrier.E2_City = "City";
				carrier.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(carrier.E2_CityInfo, "You have not entered a Carrier: City.");

				warhhouse.E2_AddressOverride = true;
				warhhouse.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(warhhouse.E2_CityInfo, "You have not entered a Warehouse: City.");
				warhhouse.E2_City = "City";
				warhhouse.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(warhhouse.E2_CityInfo, "You have not entered a Warehouse: City.");

				consolidator.E2_AddressOverride = true;
				consolidator.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(consolidator.E2_CityInfo, "You have not entered a Consolidator Address: City.");
				consolidator.E2_City = "City";
				consolidator.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(consolidator.E2_CityInfo, "You have not entered a Consolidator Address: City.");

				placeOfConsolidation.E2_AddressOverride = true;
				placeOfConsolidation.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(placeOfConsolidation.E2_CityInfo, "You have not entered a Place of Consolidation: City.");
				placeOfConsolidation.E2_City = "City";
				placeOfConsolidation.Validation.ValidateE2_City();
				AssertNoMessageErrorContaining(placeOfConsolidation.E2_CityInfo, "You have not entered a Place of Consolidation: City.");
			});
		}

		CusCAeMHMaster Master
		{
			get { return master ?? (master = Factory.NewWithValidTestData<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster master;
	}
}
