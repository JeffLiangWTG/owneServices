using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.TNT.Testing
{
	public class TNTOrganisationTest : TestCaseWithFactory
	{
		public void TestConsignorFromConsignmentRecord()
		{
			ConsignmentRecord record = new ConsignmentRecord(ConsignmentRecordString);
			TNTOrganisation consignor = TNTOrganisation.Consignor(record);
			AssertNotNull("Consignor should not be null", consignor);
			AssertEquals("Consignor LegacyCode", record.ConsignorLegacyCode, consignor.LegacyCode);
			AssertEquals("Consignor CompanyName", record.ConsignorName, consignor.CompanyName);
			AssertEquals("Consignor Address1", record.ConsignorAddress1, consignor.Address1);
			AssertEquals("Consignor Address2", record.ConsignorAddress2, consignor.Address2);
			AssertEquals("Consignor City", record.ConsignorCity, consignor.City);
			AssertEquals("Consignor PostCode", record.ConsignorPostCode, consignor.PostCode);
			AssertEquals("Consignor State", record.ConsignorState, consignor.State);
			AssertEquals("Consignor Country", record.ConsignorCountry, consignor.Country);
			AssertEquals("Consignor Phone", record.ConsignorPhone, consignor.Phone);
			AssertEquals("Consignor Fax", "", consignor.Fax);
			AssertEquals("Consignor Email", "", consignor.Email);
			AssertEquals("Consignor Language", "", consignor.Language);
			AssertEquals("Consignor ContactName", record.ConsignorContactName, consignor.ContactName);
			AssertEquals("Consignor ContactPhone", record.ConsignorContactPhone, consignor.ContactPhone);
		}

		public void TestConsignorFromCusHAWB()
		{
			CusHAWB houseBill = Factory.New<CusHAWB>();
			houseBill.CS_ConsignorName = "TEST COMPANY NAME";
			houseBill.CS_ConsignorStreet = "TEST STREET";
			houseBill.CS_ConsignorStreet2 = "TEST STREET2";
			houseBill.CS_ConsignorCity = "SYDNEY";
			houseBill.CS_ConsignorState = "NSW";
			houseBill.CS_ConsignorPostcode = "2000";
			houseBill.CS_RN_NKConsignorCountry = "AU";
			houseBill.CS_ConsignorPhone = "02 9023 1231";
			houseBill.CS_ConsignorContactName = "BOB SMITH";
			TNTOrganisation consignor = TNTOrganisation.Consignor(houseBill);
			AssertNotNull("Consignor should not be null", consignor);
			AssertEquals("Consignor LegacyCode", "", consignor.LegacyCode);
			AssertEquals("Consignor CompanyName", houseBill.CS_ConsignorName, consignor.CompanyName);
			AssertEquals("Consignor Address1", houseBill.CS_ConsignorStreet, consignor.Address1);
			AssertEquals("Consignor Address2", houseBill.CS_ConsignorStreet2, consignor.Address2);
			AssertEquals("Consignor City", houseBill.CS_ConsignorCity, consignor.City);
			AssertEquals("Consignor State", houseBill.CS_ConsignorState, consignor.State);
			AssertEquals("Consignor PostCode", houseBill.CS_ConsignorPostcode, consignor.PostCode);
			AssertEquals("Consignor Country", houseBill.CS_RN_NKConsignorCountry, consignor.Country);
			AssertEquals("Consignor Phone", houseBill.CS_ConsignorPhone, consignor.Phone);
			AssertEquals("Consignor Fax", "", consignor.Fax);
			AssertEquals("Consignor Language", "", consignor.Language);
			AssertEquals("Consignor ContactName", houseBill.CS_ConsignorContactName, consignor.ContactName);
			AssertEquals("Consignor ContactPhone", "", consignor.ContactPhone);
		}

		public void TestConsigneeFromCusHAWB()
		{
			CusHAWB houseBill = Factory.New<CusHAWB>();
			houseBill.CS_ConsigneeName = "TEST COMPANY NAME";
			houseBill.CS_ConsigneeStreet = "TEST STREET";
			houseBill.CS_ConsigneeStreet2 = "TEST STREET2";
			houseBill.CS_ConsigneeCity = "SYDNEY";
			houseBill.CS_ConsigneeState = "NSW";
			houseBill.CS_ConsigneePostcode = "2000";
			houseBill.CS_RN_NKConsigneeCountry = "AU";
			houseBill.CS_ConsigneePhone = "02 9000 2333";
			houseBill.CS_ConsigneeContactName = "BOB SMITH";
			TNTOrganisation consignee = TNTOrganisation.Consignee(houseBill);
			AssertNotNull("Consignee should not be null", consignee);
			AssertEquals("Consignee LegacyCode", "", consignee.LegacyCode);
			AssertEquals("Consignee CompanyName", houseBill.CS_ConsigneeName, consignee.CompanyName);
			AssertEquals("Consignee Address1", houseBill.CS_ConsigneeStreet, consignee.Address1);
			AssertEquals("Consignee Address2", houseBill.CS_ConsigneeStreet2, consignee.Address2);
			AssertEquals("Consignee City", houseBill.CS_ConsigneeCity, consignee.City);
			AssertEquals("Consignee State", houseBill.CS_ConsigneeState, consignee.State);
			AssertEquals("Consignee PostCode", houseBill.CS_ConsigneePostcode, consignee.PostCode);
			AssertEquals("Consignee Country", houseBill.CS_RN_NKConsigneeCountry, consignee.Country);
			AssertEquals("Consignee Phone", houseBill.CS_ConsigneePhone, consignee.Phone);
			AssertEquals("Consignee Fax", "", consignee.Fax);
			AssertEquals("Consignee Language", "", consignee.Language);
			AssertEquals("Consignee ContactName", houseBill.CS_ConsigneeContactName, consignee.ContactName);
			AssertEquals("Consignee ContactPhone", "", consignee.ContactPhone);
		}

		public void TestConsigneeFromConsignmentRecord()
		{
			ConsignmentRecord record = new ConsignmentRecord(ConsignmentRecordString);
			TNTOrganisation consignee = TNTOrganisation.Consignee(record);
			AssertNotNull("Consignee should not be null", consignee);
			AssertEquals("Consignee LegacyCode", "", consignee.LegacyCode);
			AssertEquals("Consignee CompanyName", record.ConsigneeName, consignee.CompanyName);
			AssertEquals("Consignee Address1", record.ConsigneeAddress1, consignee.Address1);
			AssertEquals("Consignee Address2", record.ConsigneeAddress2, consignee.Address2);
			AssertEquals("Consignee City", record.ConsigneeCity, consignee.City);
			AssertEquals("Consignee PostCode", record.ConsigneePostCode, consignee.PostCode);
			AssertEquals("Consignee State", record.ConsigneeState, consignee.State);
			AssertEquals("Consignee Country", record.ConsigneeCountry, consignee.Country);
			AssertEquals("Consignee Phone", record.ConsigneePhone, consignee.Phone);
			AssertEquals("Consignee Fax", record.ConsigneeFax, consignee.Fax);
			AssertEquals("Consignee Email", "", consignee.Email);
			AssertEquals("Consignee Language", "", consignee.Language);
			AssertEquals("Consignee ContactName", record.ConsigneeContactName, consignee.ContactName);
			AssertEquals("Consignee ContactPhone", record.ConsigneeContactPhone, consignee.ContactPhone);
		}

		public void TestPickupFromConsignmentRecord()
		{
			ConsignmentRecord record = new ConsignmentRecord(ConsignmentRecordString);
			TNTOrganisation pickup = TNTOrganisation.Pickup(record);
			AssertNotNull("Pickup should not be null", pickup);
			AssertEquals("Pickup LegacyCode", "", pickup.LegacyCode);
			AssertEquals("Pickup CompanyName", record.PickupName, pickup.CompanyName);
			AssertEquals("Pickup Address1", record.PickupAddress1, pickup.Address1);
			AssertEquals("Pickup Address2", record.PickupAddress2, pickup.Address2);
			AssertEquals("Pickup City", record.PickupCity, pickup.City);
			AssertEquals("Pickup PostCode", record.PickupPostCode, pickup.PostCode);
			AssertEquals("Pickup State", record.PickupState, pickup.State);
			AssertEquals("Pickup Country", record.PickupCountry, pickup.Country);
			AssertEquals("Pickup Phone", record.PickupPhone, pickup.Phone);
			AssertEquals("Pickup Fax", "", pickup.Fax);
			AssertEquals("Pickup Email", "", pickup.Email);
			AssertEquals("Pickup Language", "", pickup.Language);
			AssertEquals("Pickup ContactName", record.PickupContactName, pickup.ContactName);
			AssertEquals("Pickup ContactPhone", record.PickupContactPhone, pickup.ContactPhone);
		}

		public void TestDeliveryFromConsignmentRecord()
		{
			ConsignmentRecord record = new ConsignmentRecord(ConsignmentRecordString);
			TNTOrganisation delivery = TNTOrganisation.Delivery(record);
			AssertNotNull("Delivery should not be null", delivery);
			AssertEquals("Delivery LegacyCode", "", delivery.LegacyCode);
			AssertEquals("Delivery CompanyName", record.DeliveryName, delivery.CompanyName);
			AssertEquals("Delivery Address1", record.DeliveryAddress1, delivery.Address1);
			AssertEquals("Delivery Address2", record.DeliveryAddress2, delivery.Address2);
			AssertEquals("Delivery City", record.DeliveryCity, delivery.City);
			AssertEquals("Delivery PostCode", record.DeliveryPostCode, delivery.PostCode);
			AssertEquals("Delivery State", record.DeliveryState, delivery.State);
			AssertEquals("Delivery Country", record.DeliveryCountry, delivery.Country);
			AssertEquals("Delivery Phone", record.DeliveryPhone, delivery.Phone);
			AssertEquals("Delivery Fax", "", delivery.Fax);
			AssertEquals("Delivery Email", "", delivery.Email);
			AssertEquals("Delivery Language", "", delivery.Language);
			AssertEquals("Delivery ContactName", record.DeliveryContactName, delivery.ContactName);
			AssertEquals("Delivery ContactPhone", record.DeliveryContactPhone, delivery.ContactPhone);
		}

		public void TestJobDocAddressFromOrg()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_CompanyName = "TEST COMPANY NAME";
			docAddress.E2_Address1 = "TEST ADDRESS 1";
			docAddress.E2_Address2 = "TEST ADDRESS 2";
			docAddress.E2_City = "SYDNEY";
			docAddress.E2_Postcode = "2000";
			docAddress.E2_State = "NSW";
			docAddress.E2_Phone = "02 9032 2342";
			docAddress.E2_Fax = "02 9032 2343";
			docAddress.E2_Email = "email@email.com";
			docAddress.E2_Contact = "BOB SMITH";
			TNTOrganisation orgDocAddress = TNTOrganisation.JobDocAddress(docAddress);
			AssertNotNull("PatternAddress should not be null", orgDocAddress);
			AssertEquals("PatternAddress LegacyCode", "", orgDocAddress.LegacyCode);
			AssertEquals("PatternAddress CompanyName", docAddress.E2_CompanyName, orgDocAddress.CompanyName);
			AssertEquals("PatternAddress Address1", docAddress.E2_Address1, orgDocAddress.Address1);
			AssertEquals("PatternAddress Address2", docAddress.E2_Address2, orgDocAddress.Address2);
			AssertEquals("PatternAddress City", docAddress.E2_City, orgDocAddress.City);
			AssertEquals("PatternAddress PostCode", docAddress.E2_Postcode, orgDocAddress.PostCode);
			AssertEquals("PatternAddress State", docAddress.E2_State, orgDocAddress.State);
			AssertEquals("PatternAddress Country", "", orgDocAddress.Country);
			AssertEquals("PatternAddress Phone", docAddress.E2_Phone, orgDocAddress.Phone);
			AssertEquals("PatternAddress Fax", docAddress.E2_Fax, orgDocAddress.Fax);
			AssertEquals("PatternAddress Email", docAddress.E2_Email, orgDocAddress.Email);
			AssertEquals("PatternAddress ContactName", docAddress.E2_Contact, orgDocAddress.ContactName);
			AssertEquals("PatternAddress ContactPhone", "02 9032 2342", orgDocAddress.ContactPhone);
		}

		public void TestIsValid()
		{
			TNTOrganisation tNTOrg = new TNTOrganisation();
			AssertEquals("Not valid", false, tNTOrg.IsValid);
			tNTOrg.CompanyName = "COMPANY NAME";
			AssertEquals("Not valid", true, tNTOrg.IsValid);
		}

		const string ConsignmentRecordString = "03940432180 ADLUSO20908767COVANCE P/L                    FL 3 4 RESEARCH PARK DR        MACQUARIE UNI                  NORTH RYDE                     NEW SOUTH WALES                AU 2113     0288792000  0288792000  BOB SMITH             " + "SLEEP LAB LEVEL 6 MCEWIN BLDG  ROYAL ADELAIDE HOSPITAL        NORTH TERRACE                  ADELAIDE                       SOUTH AUSTRALIA                AU 5000     08485648144 08468451155 GAIL KOSHOREK         " + "HENRY FORD HOSPITAL            SLEEP DISORDERS RESEARCH CENT  2799 W GRAND BLVD CFP 3NIK     DETROIT                        MI                             US 48202    68454578982 12345678901 GAIL KOSHOREK         " + "DELIVERY COMPANY NAME          DELIVERY ADDRESS 1             DELIVERY ADDRESS 2             DELIVERY CITY                  DELIVERY STATE                 CAN123456789012345678901234567890123DELIVERY CONTACT NAME " + "NS1233233234.34AUD12345 123456.620T-DOC1234Delivery Instructions for the remark    12345678901212345678901               .";
	}
}
