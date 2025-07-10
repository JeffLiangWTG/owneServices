using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	internal class EDIOrgAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMandatoryFieldsForOfficeTypeWithEmptyCountryCode()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			var port = Factory.NewWithValidTestData<RefUNLOCO>();
			org.OH_RL_NKClosestPort = port.RL_Code;
			EDIOrgAddress newOrgAddress = (EDIOrgAddress)org.Addresses.AddNew();
			newOrgAddress.OA_RN_NKCountryCode = ZString.Empty;

			newOrgAddress.RunPreSaveValidation();
			AssertEquals("OA_CompanyNameOverride HasErrors", false, newOrgAddress.OA_CompanyNameOverrideInfo.HasErrors());
			AssertEquals("OA_Address1 HasErrors", true, newOrgAddress.OA_Address1Info.HasErrors());
			AssertEquals("OA_City HasErrors", false, newOrgAddress.OA_CityInfo.HasErrors());
			AssertEquals("OA_PostCode HasErrors", false, newOrgAddress.OA_PostCodeInfo.HasErrors());
			AssertEquals("OA_State HasErrors", false, newOrgAddress.OA_StateInfo.HasErrors());
			AssertEquals("OA_Phone HasErrors", false, newOrgAddress.OA_PhoneInfo.HasErrors());
			AssertEquals("OA_RL_NKRelatedPortCode HasErrors", false, newOrgAddress.OA_RL_NKRelatedPortCodeInfo.HasErrors());

			newOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			newOrgAddress.RunPreSaveValidation();

			AssertEquals("OA_CompanyNameOverride HasErrors", false, newOrgAddress.OA_CompanyNameOverrideInfo.HasErrors());
			AssertEquals("OA_Address1 HasErrors", true, newOrgAddress.OA_Address1Info.HasErrors());
			AssertEquals("OA_City HasErrors", false, newOrgAddress.OA_CityInfo.HasErrors());
			AssertEquals("OA_PostCode HasErrors", false, newOrgAddress.OA_PostCodeInfo.HasErrors());
			AssertEquals("OA_State HasErrors", false, newOrgAddress.OA_StateInfo.HasErrors());
			AssertEquals("OA_Phone HasErrors", false, newOrgAddress.OA_PhoneInfo.HasErrors());
			AssertEquals("OA_RL_NKRelatedPortCode HasErrors", true, newOrgAddress.OA_RL_NKRelatedPortCodeInfo.HasErrors());
		}

		public void TestValidateMainAddress()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var address = Factory.NewWithValidTestData<EDIOrgAddress>();
			address.OA_Code = "AAA";
			address.OA_OH = org.PK;
			Factory.Save();

			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			address.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Receivables);
			address.Validation.ValidateAll();
			AssertEquals("Address is set to main but not linked to client branch", false, address.HasRowErrors);

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var clientBranch = Factory.New<ClientBranch>();
			clientBranch.LCB_Code = "AAA";
			clientBranch.LCB_OA = address.PK;
			clientBranch.LCB_LD = database.PK;
			Factory.Save();

			var loadedAddress = new BusinessObjectFactory().Load<EDIOrgAddress>(address.PK);
			loadedAddress.Validation.ValidateAll();
			AssertEquals("Address is set to main and linked to client branch", true, loadedAddress.HasRowErrors);

			loadedAddress.AddressCapability.SetIsNotMainAddress(OrgConstants.AddressType.Receivables);
			loadedAddress.Validation.ValidateAll();
			AssertEquals("Address is set to non-main and linked to client branch", false, loadedAddress.HasRowErrors);
		}
	}
}
