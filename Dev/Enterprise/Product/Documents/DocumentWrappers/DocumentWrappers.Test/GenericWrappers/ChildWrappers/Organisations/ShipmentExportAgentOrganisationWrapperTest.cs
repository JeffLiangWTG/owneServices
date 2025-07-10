using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ShipmentExportAgentOrganisationWrapper))]
	sealed class ShipmentExportAgentOrganisationWrapperTest : GenericWrapperTest
	{
		public void TestCompanyNameAndAddress_SCACCodeIsSpecified_ReturnNameAndAddressWithSCACCode()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "MCLAREN";
			header.OH_Code = "MP4-27";
			header.MainAddress.OA_Address1 = "UK";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, "FMCN");
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, "CHBLN");
			header.CustomsCodes.AddNew("CCC", "SCACN", "US");

			var wrapperToTest = new ShipmentExportAgentOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.All, Factory);
			var actualValue = wrapperToTest.CompanyNameAndAddress;

			AssertContains("The NameAndAddress should include SCACN code", "MCLAREN\r\nSCAC NO. SCACN\r\nUK", actualValue);
		}

		public void TestCompanyNameAndAddress_SCACCodeIsNotSpecified_ReturnNameAndAddressWithNoCodes()
		{
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "MCLAREN";
			header.OH_Code = "MP4-27";
			header.MainAddress.OA_Address1 = "UK";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, "FMCN");
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, "CHBLN");

			var wrapperToTest = new ShipmentExportAgentOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.All, Factory);
			var actualValue = wrapperToTest.CompanyNameAndAddress;

			AssertContains("The NameAndAddress should not include any code", "MCLAREN\r\nUK", actualValue);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ShipmentExportAgentOrganisationWrapper(OrganisationUsageType.Test, (OrgHeader)null, ContactType.All, Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);

			var wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);
			AssertEquals("Test", wrapper.TypeDescription);
			AssertEquals("JOHNEATNUT", wrapper.CompanyCode);
			AssertEquals("JOHNNY EATS NUTS", wrapper.CompanyName);
			AssertEquals("wrapper.CompanyNameAndAddress", "JOHNNY EATS NUTS\n'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyNameAndAddress);
			AssertEquals("9025 1100", wrapper.MainPhone);
			AssertEquals("9025 1199", wrapper.MainFax);
			AssertEquals("sales@edi.com.au", wrapper.MainEmail);
			AssertEquals(ContactType.All.DefaultName, wrapper.ContactName);
			AssertEquals("9025 1100", wrapper.ContactPhone);
			AssertEquals("9025 1199", wrapper.ContactFax);
			AssertEquals("sales@edi.com.au", wrapper.ContactEmail);
			AssertEquals(ContactType.All.DefaultName, wrapper.Contacts[ContactTypeList.Codes.All].FullName);
			AssertEquals("'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.Addresses[AddressTypeList.Codes.Main].Address);
			AssertEquals("wrapper.CustomsCodes.Count", 0, wrapper.CustomsCodes.Count);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AviationSecurity : (No Default Field Value Available on AviationSecurity)
ClosestPort : AUBNE - Brisbane
Country : AU - Australia
DGContact : Scott 'The Best' Wright
MainAddress : QLD\nAUSTRALIA
PartAttribute1 : Part Attrib. 1
PartAttribute2 : Part Attrib. 2
PartAttribute3 : Part Attrib. 3
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUBNE";
			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Scott 'The Best' Wright";
			organisation.MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			organisation.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.WRK;

			return new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ShipmentExportAgentOrganisation     (Default Field: CompanyNameAndAddress)
======================================================================
Name                                    Type
----------------------------------------------------------------------
MainAddress                             Address
AviationSecurity                        AviationSecurity
Country                                 Country
DGContact                               DGContact
ClosestPort                             Location
PartAttribute1                          PartAttribute
PartAttribute2                          PartAttribute
PartAttribute3                          PartAttribute
CompanyAddress                          String
CompanyCode                             String
CompanyName                             String
CompanyNameAndAddress                   String
ContactEmail                            String
ContactFax                              String
ContactName                             String
ContactPhone                            String
FirstLineOfCompanyNameAndAddress        String
LocalBusinessRegNo                      String
LocalVATCode                            String
MainEmail                               String
MainFax                                 String
MainPhone                               String
RANumber                                String
RoadCarrierRegistration                 String
RoadCarrierRegistrationHeading          String
SCAC                                    String
TaxCode                                 String
TypeDescription                         String

Addresses                               Address Collection
AssignedStaff                           AssignedStaffMember Collection
Contacts                                Contact Collection
EDocs                                   eDoc Collection
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
CustomsCodes                            RegistrationNumberCode Collection
";
			}
		}
	}
}
