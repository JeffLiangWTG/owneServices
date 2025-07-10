using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DeliveryDocAddressSynchroniserTest : SynchroniserTestCase
	{
		public void TestDeliveryDocAddressSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var source = shipment.ConsigneeDeliveryAddress;
			source.E2_AddressOverride = true;
			source.E2_CompanyName = "NAME";
			source.E2_Address1 = "ADDRESS1";
			source.E2_Address2 = "ADDRESS2";
			source.E2_City = "CITY";
			source.E2_State = "STATE";
			source.E2_Postcode = "2017";
			source.E2_RN_NKCountryCode = "AU";
			source.E2_Phone = "1234 5678";
			source.E2_Contact = "FRANK";
			var destination = Factory.New<CusSCAHouse>();
			var synchroniser = new DeliveryDocAddressSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("Name", "NAME", destination.CA_DeliveryName);
			AssertEquals("ADDRESS LINE 1", "ADDRESS1", destination.CA_DeliveryAddress1);
			AssertEquals("ADDRESS LINE 2", "ADDRESS2", destination.CA_DeliveryAddress2);
			AssertEquals("CITY", "CITY", destination.CA_DeliverySuburb);
			AssertEquals("State", "STATE", destination.CA_DeliveryState);
			AssertEquals("PostCode", "2017", destination.CA_DeliveryPostcode);
			AssertEquals("Country", "AU", destination.CA_RN_NKDeliveryCountryCode);
			AssertEquals("Contact", "FRANK", destination.CA_DeliveryContactName);
			AssertEquals("Phone", "1234 5678", destination.CA_DeliveryPhone);

			source.E2_CompanyName = "OR NAME";
			source.E2_Address1 = "OR ADDRESS1";
			source.E2_Address2 = "OR ADDRESS2";
			source.E2_City = "OR CITY";
			source.E2_State = "OR STATE";
			source.E2_Postcode = "2020";
			source.E2_RN_NKCountryCode = "NZ";
			source.E2_Phone = "2345 6789";
			source.E2_Contact = "OR CONTACT";

			AssertEquals("Name", "OR NAME", destination.CA_DeliveryName);
			AssertEquals("ADDRESS LINE 1", "OR ADDRESS1", destination.CA_DeliveryAddress1);
			AssertEquals("ADDRESS LINE 2", "OR ADDRESS2", destination.CA_DeliveryAddress2);
			AssertEquals("CITY", "OR CITY", destination.CA_DeliverySuburb);
			AssertEquals("State", "OR STATE", destination.CA_DeliveryState);
			AssertEquals("PostCode", "2020", destination.CA_DeliveryPostcode);
			AssertEquals("Country", "NZ", destination.CA_RN_NKDeliveryCountryCode);
			AssertEquals("Contact", "OR CONTACT", destination.CA_DeliveryContactName);
			AssertEquals("Phone", "2345 6789", destination.CA_DeliveryPhone);
		}
	}
}
