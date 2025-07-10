using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	public class EntityToOrgAddressMatchingConverterTest : TransactionedTestCase
	{
		public void TestConvert()
		{
			var converter = new EntityToOrgAddressMatchingConverter();

			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress");
			var portDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress.RelatedPortCode");
			var sessionServices = new AncillaryImportServices();
			var address = new Entity(definition, sessionServices);
			var port = new Entity(portDefinition, sessionServices);

			address["Code"] = "CODE";
			address["Address1"] = "Address1";
			address["Address2"] = "Address2";
			address["City"] = "City";
			address["Language"] = "Language";
			address["PostCode"] = "PostCode";
			address["State"] = "State";
			address["Fax"] = "Fax";
			address["Phone"] = "Phone";
			address["Email"] = "Email";
			address["Mobile"] = "Mobile";
			address["CompanyNameOverride"] = "CompanyName";
			address["IsActive"] = false;
			address.ParentCollection.Add(port);

			port["Code"] = "Port";

			var result = new OrgAddressForMatching();
			converter.Convert(result, address);

			AssertEquals("CODE", result.OA_Code);
			AssertEquals("Address1", result.OA_Address1);
			AssertEquals("Address2", result.OA_Address2);
			AssertEquals("City", result.OA_City);
			AssertEquals("Language", result.OA_Language);
			AssertEquals("PostCode", result.OA_PostCode);
			AssertEquals("State", result.OA_State);
			AssertEquals("Fax", result.OA_Fax);
			AssertEquals("Phone", result.OA_Phone);
			AssertEquals("Email", result.OA_Email);
			AssertEquals("Mobile", result.OA_Mobile);
			AssertEquals("CompanyName", result.OA_CompanyNameOverride);
			AssertEquals(false, result.OA_IsActive);
		}

		public void TestConvert_MainAddress()
		{
			var converter = new EntityToOrgAddressMatchingConverter();

			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress");
			var portDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress.RelatedPortCode");
			var capabilityDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress.OrgAddressCapability");
			var sessionServices = new AncillaryImportServices();
			var address = new Entity(definition, sessionServices);
			var capability = new Entity(capabilityDefinition, sessionServices);
			var port = new Entity(portDefinition, sessionServices);

			address["Code"] = "CODE";
			address["Address1"] = "Address1";
			address["Address2"] = "Address2";
			address["City"] = "City";
			address["Language"] = "Language";
			address["PostCode"] = "PostCode";
			address["State"] = "State";
			address["Fax"] = "Fax";
			address["Phone"] = "Phone";
			address["Email"] = "Email";
			address["Mobile"] = "Mobile";
			address["CompanyNameOverride"] = "CompanyName";
			address["IsActive"] = false;

			address.ParentCollection.Add(port);
			port["Code"] = "Port";

			address.ChildrenCollection.Add(capability);
			capability["AddressType"] = "OFC";
			capability["IsMainAddress"] = "true";

			var result = new OrgAddressForMatching();
			converter.Convert(result, address);

			AssertEquals("CODE", result.OA_Code);
			AssertEquals("Address1", result.OA_Address1);
			AssertEquals("Address2", result.OA_Address2);
			AssertEquals("City", result.OA_City);
			AssertEquals("Language", result.OA_Language);
			AssertEquals("PostCode", result.OA_PostCode);
			AssertEquals("State", result.OA_State);
			AssertEquals("Fax", result.OA_Fax);
			AssertEquals("Phone", result.OA_Phone);
			AssertEquals("Email", result.OA_Email);
			AssertEquals("Mobile", result.OA_Mobile);
			AssertEquals("CompanyName", result.OA_CompanyNameOverride);
			AssertEquals(false, result.OA_IsActive);
			AssertEquals(true, result.IsMainAddress);
		}
	}
}
