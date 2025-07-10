using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ConsigneeDocAddressSynchroniserTest : SynchroniserTestCase
	{
		public void TestConsigneeDocAddressSynchroniser()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORGCODE";
			orgHeader.OH_FullName = "NAME";
			orgHeader.MainAddress.OA_Address1 = "ADDRESS LINE 1";
			orgHeader.MainAddress.OA_Address2 = "ADDRESS LINE 2";
			orgHeader.MainAddress.OA_City = "CITY";
			orgHeader.MainAddress.OA_State = "NSW";
			orgHeader.MainAddress.OA_PostCode = "2017";
			orgHeader.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			orgHeader.MainAddress.OA_Phone = "1234 5678";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "FRANK";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = orgHeader.PK;
			shipment.ConsigneeDocumentaryAddress.ContactPK = contact.PK;
			var destination = Factory.New<CusSCAHouse>();
			var source = shipment.ConsigneeDocumentaryAddress;
			var synchroniser = new ConsigneeDocAddressSynchroniser(destination, source);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals("Org PK", orgHeader.PK, destination.CA_OH_Consignee);
			AssertEquals("Name", "NAME", destination.CA_ConsigneeName);
			AssertEquals("ADDRESS LINE 1", "ADDRESS LINE 1", destination.CA_ConsigneeAddress1);
			AssertEquals("ADDRESS LINE 2", "ADDRESS LINE 2", destination.CA_ConsigneeAddress2);
			AssertEquals("CITY", "CITY", destination.CA_ConsigneeSuburb);
			AssertEquals("State", "NSW", destination.CA_ConsigneeState);
			AssertEquals("PostCode", "2017", destination.CA_ConsigneePostcode);
			AssertEquals("Country", "AU", destination.CA_RN_NKConsigneeCountryCode);
			AssertEquals("Contact", "FRANK", destination.CA_ConsigneeContactName);
			AssertEquals("Phone", "1234 5678", destination.CA_ConsigneePhone);

			source.E2_AddressOverride = true;
			source.E2_CompanyName = "OR NAME";
			source.E2_Address1 = "OR ADDRESS1";
			source.E2_Address2 = "OR ADDRESS2";
			source.E2_City = "OR CITY";
			source.E2_State = "OR STATE";
			source.E2_Postcode = "2020";
			source.E2_RN_NKCountryCode = "NZ";
			source.E2_Phone = "2345 6789";
			source.E2_Contact = "OR CONTACT";

			AssertEquals("Org PK", ZGuid.Empty, destination.CA_OH_Consignee);
			AssertEquals("Name", "OR NAME", destination.CA_ConsigneeName);
			AssertEquals("ADDRESS LINE 1", "OR ADDRESS1", destination.CA_ConsigneeAddress1);
			AssertEquals("ADDRESS LINE 2", "OR ADDRESS2", destination.CA_ConsigneeAddress2);
			AssertEquals("CITY", "OR CITY", destination.CA_ConsigneeSuburb);
			AssertEquals("State", "OR STATE", destination.CA_ConsigneeState);
			AssertEquals("PostCode", "2020", destination.CA_ConsigneePostcode);
			AssertEquals("Country", "NZ", destination.CA_RN_NKConsigneeCountryCode);
			AssertEquals("Contact", "OR CONTACT", destination.CA_ConsigneeContactName);
			AssertEquals("Phone", "2345 6789", destination.CA_ConsigneePhone);
		}
	}
}
