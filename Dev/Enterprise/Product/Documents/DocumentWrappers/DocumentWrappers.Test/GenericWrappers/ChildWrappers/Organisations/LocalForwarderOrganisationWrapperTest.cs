using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LocalForwarderOrganisationWrapper))]
	sealed class LocalForwarderOrganisationWrapperTest : Base.Testing.GenericWrapperTest
	{
		public void TestCompanyNameAndAddressBothWithAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_Code = "co";
			header.MainAddress.OA_Address1 = "ADR1";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, "FMCN");
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, "CHBLN");

			LocalForwarderOrganisationWrapper wrapper = new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.FreightAgent, Factory);
			AssertContains("should include codes", "NAME\r\nFMC NO. FMCN / CHB NO. CHBLN\r\nADR1", wrapper.CompanyNameAndAddress);
		}

		public void TestCompanyNameAndAddressBothWithoutAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "co";
			header.OH_FullName = "NAME";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, "FMCN");
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, "CHBLN");

			LocalForwarderOrganisationWrapper wrapper = new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.FreightAgent, Factory);
			AssertContains("should include codes", "NAME\r\nFMC NO. FMCN / CHB NO. CHBLN", wrapper.CompanyNameAndAddress);
		}

		public void TestCompanyNameAndAddressFMCOnlyWithAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.MainAddress.OA_Address1 = "ADRS";
			header.OH_Code = "co";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, "FMCN");

			LocalForwarderOrganisationWrapper wrapper = new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.FreightAgent, Factory);
			AssertContains("should include codes", "NAME\r\nFMC NO. FMCN\r\nADRS", wrapper.CompanyNameAndAddress);
		}

		public void TestCompanyNameAndAddressFMCOnlyWithoutAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_Code = "co";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, "FMCN");

			LocalForwarderOrganisationWrapper wrapper = new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.FreightAgent, Factory);
			AssertContains("should include codes", "NAME\r\nFMC NO. FMCN", wrapper.CompanyNameAndAddress);
		}

		public void TestCompanyNameAndAddressCHBOnlyWithAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.MainAddress.OA_Address1 = "ADRS";
			header.OH_Code = "co";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, "CHBLN");

			LocalForwarderOrganisationWrapper wrapper = new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.FreightAgent, Factory);
			AssertContains("should include codes", "NAME\r\nCHB NO. CHBLN\r\nADRS", wrapper.CompanyNameAndAddress);
		}

		public void TestCompanyNameAndAddressCHBOnlyWithoutAddress()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "NAME";
			header.OH_Code = "co";
			header.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, "CHBLN");

			LocalForwarderOrganisationWrapper wrapper = new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, header, ContactType.FreightAgent, Factory);
			AssertContains("should include codes", "NAME\r\nCHB NO. CHBLN", wrapper.CompanyNameAndAddress);
		}

		public override void TestWrapperMappingsEmpty()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Consignee, organisation, ContactType.All, Factory);
			AssertEquals("Consignee", wrapper.TypeDescription);
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

		#region Implementation

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

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
LocalForwarderOrganisation      (Default Field: CompanyNameAndAddress)
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

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new LocalForwarderOrganisationWrapper(OrganisationUsageType.Test, (OrgHeader)null, ContactType.FreightAgent, Factory);
		}

		#endregion
	}
}
