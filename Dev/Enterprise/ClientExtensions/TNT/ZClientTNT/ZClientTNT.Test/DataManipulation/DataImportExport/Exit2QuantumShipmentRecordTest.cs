using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	class Exit2QuantumShipmentRecordTest : TestCaseWithFactory
	{
		public void TestMapConsignorConsignee()
		{
			Exit2QuantumShipmentRecord record = new Exit2QuantumShipmentRecord("SYD", "", TestWithConsigneeContact);
			ForwardingShipment shipment = record.CreateShipment(Factory, false, new NotificationBuffer());
			AssertNull("Shipment should not have an Org Consignee", shipment.Consignee);
			AssertNull("Shipment should not have an Org Consignee", shipment.Consignor);
			AssertJobDocAddress(shipment.ConsignorDocumentaryAddress, "SURPLUS BEARINGS P/L", "U 2 49 RANDALL ST", "ADDR2", "SLACKS CREEK", "4127", "QUEENSLAND", "AU", "0738087494", "", "MOORE");
			AssertJobDocAddress(shipment.ConsigneeDocumentaryAddress, "IQ ENGINEERING INC", "8208 NW 30TH TERRACE", "", "MIAMI", "33122", "FL", "US", "5924404", "", "GEITSY GONZALEZ");
			AssertJobDocAddress(shipment.ConsignorPickupAddress, "PICKUP NAME", "PICKUP ADDR1", "PICKUP ADDR2", "PICKUP CITY", "PICKUP", "PICKUP STATE", "NZ", "1234567890", "", "P_Contact");
			AssertJobDocAddress(shipment.ConsigneeDeliveryAddress, "DELIVERY NAME", "DELIVERY ADDR1", "DELIVERY ADDR2", "DELIVERY CITY", "DELPCODE", "DELIVERY STATE", "AU", "123456789", "", "DEL CONTACT");
			AssertEquals(true, shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride);
		}

		public void TestMapConsignorConsigneeWithIncompleteDataForNotifyPartyAddressOverride()
		{
			Exit2QuantumShipmentRecord record = new Exit2QuantumShipmentRecord("SYD", "", TestWithIncompleteDataForNotifyPartyAddressOverride);
			ForwardingShipment shipment = record.CreateShipment(Factory, false, new NotificationBuffer());
			AssertNull("Shipment should not have an Org Consignee", shipment.Consignee);
			AssertNull("Shipment should not have an Org Consignee", shipment.Consignor);
			AssertJobDocAddress(shipment.ConsignorDocumentaryAddress, "SURPLUS BEARINGS P/L", "U 2 49 RANDALL ST", "ADDR2", "SLACKS CREEK", "4127", "QUEENSLAND", "AU", "0738087494", "", "MOORE");
			AssertJobDocAddress(shipment.ConsigneeDocumentaryAddress, "IQ ENGINEERING INC", "8208 NW 30TH TERRACE", "", "", "33122", "FL", "US", "5924404", "", "GEITSY GONZALEZ");
			AssertJobDocAddress(shipment.ConsignorPickupAddress, "PICKUP NAME", "PICKUP ADDR1", "PICKUP ADDR2", "PICKUP CITY", "PICKUP", "PICKUP STATE", "NZ", "1234567890", "", "P_Contact");
			AssertJobDocAddress(shipment.ConsigneeDeliveryAddress, "DELIVERY NAME", "DELIVERY ADDR1", "DELIVERY ADDR2", "DELIVERY CITY", "DELPCODE", "DELIVERY STATE", "AU", "123456789", "", "DEL CONTACT");
			AssertEquals(false, shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride);
		}

		void AssertJobDocAddress(JobDocAddress jobDocAddress, string name, string address1, string address2, string city, string postCode, string state, string country, string phone, string fax, string contactName)
		{
			AssertEquals("Address should be overriden", true, jobDocAddress.E2_AddressOverride);
			AssertEquals("E2_CompanyName", name, jobDocAddress.E2_CompanyName);
			AssertEquals("E2_Address1", address1, jobDocAddress.E2_Address1);
			AssertEquals("E2_Address2", address2, jobDocAddress.E2_Address2);
			AssertEquals("E2_City", city, jobDocAddress.E2_City);
			AssertEquals("E2_Postcode", postCode, jobDocAddress.E2_Postcode);
			AssertEquals("E2_State", state, jobDocAddress.E2_State);
			AssertEquals("E2_RN_NKCountryCode", country, jobDocAddress.E2_RN_NKCountryCode);
			AssertEquals("E2_Phone", phone, jobDocAddress.E2_Phone);
			AssertEquals("E2_Fax", fax, jobDocAddress.E2_Fax);
			AssertEquals("E2_Contact", contactName, jobDocAddress.E2_Contact);
		}

		const string TestWithConsigneeContact = "03922639355 BNEMIA21017331SURPLUS BEARINGS P/L           U 2 49 RANDALL ST              ADDR2                          SLACKS CREEK                   QUEENSLAND                     AU 4127     0738087494  0738087494  MOORE                 PICKUP NAME                    PICKUP ADDR1                   PICKUP ADDR2                   PICKUP CITY                    PICKUP STATE                   NZ PICKUP   1234567890  9999999999  P_Contact             IQ ENGINEERING INC             8208 NW 30TH TERRACE                                          MIAMI                          FL                             US 33122    5924404     5924404     GEITSY GONZALEZ       DELIVERY NAME                  DELIVERY ADDR1                 DELIVERY ADDR2                 DELIVERY CITY                  DELIVERY STATE                 AU DELPCODE 123456789               DEL CONTACT           NS       450.00USD     1    13.100EX2                                                                                    .";
		const string TestWithIncompleteDataForNotifyPartyAddressOverride = "03922639355 BNEMIA21017331SURPLUS BEARINGS P/L           U 2 49 RANDALL ST              ADDR2                          SLACKS CREEK                   QUEENSLAND                     AU 4127     0738087494  0738087494  MOORE                 PICKUP NAME                    PICKUP ADDR1                   PICKUP ADDR2                   PICKUP CITY                    PICKUP STATE                   NZ PICKUP   1234567890  9999999999  P_Contact             IQ ENGINEERING INC             8208 NW 30TH TERRACE                                                                         FL                             US 33122    5924404     5924404     GEITSY GONZALEZ       DELIVERY NAME                  DELIVERY ADDR1                 DELIVERY ADDR2                 DELIVERY CITY                  DELIVERY STATE                 AU DELPCODE 123456789               DEL CONTACT           NS       450.00USD     1    13.100EX2                                                                                    .";
	}
}
