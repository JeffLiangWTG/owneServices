using CargoWise.Definitions;
using CargoWise.IO;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrganisationWrapper))]
	sealed class OrganisationWrapperTest : Base.Testing.GenericWrapperWithNotesTest
	{
		public void TestGetCustomField()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.SetUserDefinedValue("WHS DOC PACK", new ZString("AAA"));
			var address = org.MainAddress;

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;

			var wrapper = new OrganisationWrapper(OrganisationUsageType.Test, docAddress, Factory);
			AssertEquals("AAA", wrapper.GetCustomField("WHS DOC PACK"));
		}

		public void TestCountryCodeOverride()
		{
			var supplier = Factory.New<OrgHeader>();
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			supplierAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			DocSADH wrapper = DocSADH.New(entryHeader, Factory);
			AssertContains("AU", wrapper.Box2Supplier.Country.Code);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "US";

			wrapper = DocSADH.New(entryHeader, Factory);

			AssertContains("US", wrapper.Box2Supplier.Country.Code);
		}

		public void TestTaxCode()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_IsDebtor = true;
			var wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);
			AssertEquals(wrapper.TaxCode, Country.GetConsumptionTaxDescription(Constants.CountryCodes.Australia));

			organisation.OH_RL_NKClosestPort = "MYPKG";
			AssertEquals(wrapper.TaxCode, Country.GetConsumptionTaxDescription(Constants.CountryCodes.Malaysia));

			organisation.OH_IsDebtor = false;
			AssertEquals(string.Empty, wrapper.TaxCode);

			organisation.OH_IsDebtor = true;
			AssertEquals(wrapper.TaxCode, Country.GetConsumptionTaxDescription(Constants.CountryCodes.Malaysia));

			organisation.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			AssertEquals(string.Empty, wrapper.TaxCode);
		}

		public void TestRANumber()
		{
			var org = Factory.New<OrgHeader>();
			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals(ZString.Empty, wrapper.RANumber);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RegulatedAgentID, "1234", Constants.CountryCodes.Australia);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "6666", Constants.CountryCodes.Australia);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RegulatedAgentID, "5678", Constants.CountryCodes.Australia);
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "9999", Constants.CountryCodes.Jamaica);
			wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals("Only RA numbers from log in country should be returned", "1234, 5678", wrapper.RANumber);

			org.CustomsCodes.RemoveAll();
			GenRegCertAccredMaintList cert = GlbStaff.CurrentUser.Certificates.AddNew();
			cert.XZ_Type = CertificateTypePairList.Codes.DT1;
			cert.XZ_RefNumber = "111-222-333";
			wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals("Australian login should fall back to DTA number", "111-222-333", wrapper.RANumber);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedKingdom);
			org.CustomsCodes.AddNew(OrgCusCode.UnitedKingdomCodeTypes.AirCargoAgentsListedNumber, "4343");
			wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals("UK login should fall back to ACL number", "4343", wrapper.RANumber);
		}

		#region TestRoadCarrierRegistration

		#region TestRoadCarrierRegistrationHeading

		public void TestRoadCarrierRegistrationHeading()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals("Road Carrier Registration", wrapper.RoadCarrierRegistrationHeading);
		}

		#endregion

		#region TestRoadCarrierRegistration

		public void TestRoadCarrierRegistration()
		{
			var org = Factory.New<OrgHeader>();
			var wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals(ZString.Empty, wrapper.RoadCarrierRegistration);

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RoadCarrierRegistration, "RCR1234");
			wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals("Road Carrier Registration Number", "RCR1234", wrapper.RoadCarrierRegistration);
		}

		#endregion

		#endregion

		public void TestSCAC()
		{
			var org = Factory.New<OrgHeader>();
			var usObject = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = usObject.Code;
			cusCode.OK_CustomsRegNo = "BLAH";
			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, org, ContactType.All, Factory);
			AssertEquals("BLAH", wrapper.SCAC);
		}

		public void TestFirstLineOfCompanyNameAndAddress()
		{
			var organization = Factory.New<OrgHeader>();
			organization.MainAddress.OA_Address1 = "ADDRESS 1";
			organization.MainAddress.OA_Address2 = "ADDRESS 2";
			AssertEquals("ADDRESS 1", new OrganisationWrapper(OrganisationUsageType.Test, organization, ContactType.All, Factory).FirstLineOfCompanyNameAndAddress);

			organization.MainAddress.OA_CompanyNameOverride = "APPLE";
			AssertEquals("APPLE", new OrganisationWrapper(OrganisationUsageType.Test, organization, ContactType.All, Factory).FirstLineOfCompanyNameAndAddress);
		}

		public void TestCreateWithOrgAddress()
		{
			OrganisationWrapper testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, (OrgAddress)null, ContactType.ShippingLine, Factory);
			AssertNull(testWrapper.Organisation);
			Assert(testWrapper.CompanyNameAndAddress.IsEmpty);

			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.MainAddress.OA_Address1 = "MAIN ADDRESS";
			OrgAddress secondAddres = testHeader.Addresses.AddNew();
			secondAddres.OA_Address1 = "SECOND ADDRESS";
			secondAddres.OA_RN_NKCountryCode = "AU";

			testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, secondAddres, ContactType.ShippingLine, Factory);
			AssertEquals(testWrapper.Organisation, testHeader);
			AssertEquals("SECOND ADDRESS\nAUSTRALIA", testWrapper.CompanyNameAndAddress);
		}

		public void TestCreateWithOrgAddressAndContact()
		{
			var testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, (OrgAddress)null, (OrgContact)null, Factory);
			AssertNull(testWrapper.Organisation);
			Assert(testWrapper.CompanyNameAndAddress.IsEmpty);
			Assert(testWrapper.ContactName.IsEmpty);

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.MainAddress.OA_Address1 = "MAIN ADDRESS";
			testHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			var contact = testHeader.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";

			testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, testHeader.MainAddress, contact, Factory);
			AssertEquals(testWrapper.Organisation, testHeader);
			AssertEquals("MAIN ADDRESS\nAUSTRALIA", testWrapper.CompanyNameAndAddress);
			AssertEquals("Contact Name", testWrapper.ContactName);
		}

		public void TestWhenContactNameTextIsNotEmpty()
		{
			OrganisationWrapper testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, Factory.GetNull<JobDocAddress>(), Factory) { ContactNameText = "test contact name text" };
			AssertEquals("test contact name text", testWrapper.ContactName);
		}

		public void TestFactoryIsSingletonForDifferentContactWrappers()
		{
			var testWrapper1 = new OrganisationWrapper(OrganisationUsageType.Test, Factory.GetNull<OrgAddress>(), ContactType.CTO, ZString.Empty, Factory) { ContactNameText = "test contact name text" };
			var testWrapper2 = new OrganisationWrapper(OrganisationUsageType.Test, Factory.GetNull<OrgAddress>(), ContactType.CTO, ZString.Empty, Factory) { ContactNameText = "test contact name text" };
			Assert("Different organisation wrappers should use the factory if provided so.", testWrapper1.WrappedMainContact.ContactBO.Factory == testWrapper2.WrappedMainContact.ContactBO.Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			OrganisationWrapper wrapperEmpty = new OrganisationWrapper(OrganisationUsageType.Test, (OrgHeader)null, ContactType.CTO, Factory);
			AssertEquals("wrapperEmpty.TypeDescription", "Test", wrapperEmpty.TypeDescription);
			AssertEquals("wrapperEmpty.CompanyCode", "", wrapperEmpty.CompanyCode);
			AssertEquals("wrapperEmpty.CompanyName", "", wrapperEmpty.CompanyName);
			AssertEquals("wrapperEmpty.CompanyNameAndAddress", "", wrapperEmpty.CompanyNameAndAddress);
			AssertEquals("wrapperEmpty.CompanyAddress", "", wrapperEmpty.CompanyAddress);
			AssertEquals("wrapperEmpty.MainPhone", "", wrapperEmpty.MainPhone);
			AssertEquals("wrapperEmpty.MainFax", "", wrapperEmpty.MainFax);
			AssertEquals("wrapperEmpty.MainEmail", "", wrapperEmpty.MainEmail);
			AssertEquals("wrapperEmpty.ContactName", ContactType.CTO.DefaultName, wrapperEmpty.ContactName);
			AssertEquals("wrapperEmpty.ContactPhone", "", wrapperEmpty.ContactPhone);
			AssertEquals("wrapperEmpty.ContactFax", "", wrapperEmpty.ContactFax);
			AssertEquals("wrapperEmpty.ContactEmail", "", wrapperEmpty.ContactEmail);
			AssertEquals("wrapperEmpty.Contacts.Count", 0, wrapperEmpty.Contacts.Count);
			AssertEquals("wrapperEmpty.Addresses.Count", 0, wrapperEmpty.Addresses.Count);
			AssertEquals("wrapperEmpty.CustomsCodes.Count", 0, wrapperEmpty.CustomsCodes.Count);

			AssertEquals("wrapper.Organisation.OH_Code", "", wrapperEmpty.Organisation.OH_Code);
			AssertEquals("wrapper.Organisation.OH_FullName", "", wrapperEmpty.Organisation.OH_FullName);
			AssertNotEquals("wrapper.Organisation.PK", ZGuid.Empty, wrapperEmpty.Organisation.PK);

			AssertEquals("wrapperEmpty.PartAttribute1", "", wrapperEmpty.PartAttribute1.Name);
			AssertEquals("wrapperEmpty.PartAttribute2", "", wrapperEmpty.PartAttribute2.Name);
			AssertEquals("wrapperEmpty.PartAttribute3", "", wrapperEmpty.PartAttribute3.Name);
			AssertNull("wrapperEmpty.ClientDocumentLogo", wrapperEmpty.ClientDocumentLogo);
		}

		public void TestWrapperMappingFromOrganisation()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);
			AssertEquals("wrapper.TypeDescription", "Test", wrapper.TypeDescription);
			AssertEquals("wrapper.TypeDescription", "JOHNEATNUT", wrapper.CompanyCode);
			AssertEquals("wrapper.TypeDescription", "JOHNNY EATS NUTS", wrapper.CompanyName);
			AssertEquals("wrapper.CompanyNameAndAddress", "JOHNNY EATS NUTS\n'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyNameAndAddress);
			AssertEquals("wrapper.CompanyAddress", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyAddress);
			AssertEquals("wrapper.MainPhone", "9025 1100", wrapper.MainPhone);
			AssertEquals("wrapper.MainFax", "9025 1199", wrapper.MainFax);
			AssertEquals("wrapper.MainEmail", "sales@edi.com.au", wrapper.MainEmail);
			AssertEquals("wrapper.ContactName", ContactType.All.DefaultName, wrapper.ContactName);
			AssertEquals("wrapper.ContactPhone", "9025 1100", wrapper.ContactPhone);
			AssertEquals("wrapper.ContactFax", "9025 1199", wrapper.ContactFax);
			AssertEquals("wrapper.ContactEmail", "sales@edi.com.au", wrapper.ContactEmail);
			AssertEquals("wrapper.Contacts[All].FullName", ContactType.All.DefaultName, wrapper.Contacts[ContactTypeList.Codes.All].FullName);
			AssertEquals("wrapper.Addresses[Main].Address", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.Addresses[AddressTypeList.Codes.Main].Address);
			AssertEquals("wrapper.CustomsCodes.Count", 0, wrapper.CustomsCodes.Count);

			AssertEquals("wrapper.Organisation.OH_Code", "JOHNEATNUT", wrapper.Organisation.OH_Code);
			AssertEquals("wrapper.Organisation.OH_FullName", "JOHNNY EATS NUTS", wrapper.Organisation.OH_FullName);
			AssertEquals("wrapper.Organisation.PK", organisation.PK, wrapper.Organisation.PK);

			AssertNotNull("wrapper.PartAttribute1", wrapper.PartAttribute1);
			AssertNotNull("wrapper.PartAttribute2", wrapper.PartAttribute2);
			AssertNotNull("wrapper.PartAttribute3", wrapper.PartAttribute3);
		}

		public void TestDGContactNameAndPhone()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgContact contact1 = OrgTestHelper.AddNewContact(organisation, "Ben Govett", "9025 1121", "9025 1199", "ben@edi.com.au", "0421 944 321", "8814 6253", "Hey you!", "Kickarse Specialist", "221", "LABOUR", "9481 1111", "PAGEMENOW");
			organisation.MiscServ.OM_OC_EXDefaultDGContact = contact1.PK;
			organisation.MiscServ.OM_EXDefaultDGContactPhoneUsed = PhoneTypeList.Codes.WRK;

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);

			AssertEquals("Ben Govett", wrapper.DGContact.Name);
			AssertEquals("9025 1121", wrapper.DGContact.Phone);
		}

		public void TestWrapperMappingWithRealContact()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);
			OrgContact contact1 = OrgTestHelper.AddNewContact(organisation, "Ben Govett", "9025 1121", "9025 1199", "ben@edi.com.au", "0421 944 321", "8814 6253", "Hey you!", "Kickarse Specialist", "221", "LABOUR", "9481 1111", "PAGEMENOW");
			OrgDocument contact1documentType1 = OrgTestHelper.AddNewDocumentType(contact1, ContactType.All, true);

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);
			AssertEquals("wrapper.TypeDescription", "Test", wrapper.TypeDescription);
			AssertEquals("wrapper.CompanyCode", "JOHNEATNUT", wrapper.CompanyCode);
			AssertEquals("wrapper.CompanyName", "JOHNNY EATS NUTS", wrapper.CompanyName);
			AssertEquals("wrapper.CompanyNameAndAddress", "JOHNNY EATS NUTS\n'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyNameAndAddress);
			AssertEquals("wrapper.CompanyAddress", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyAddress);
			AssertEquals("wrapper.MainPhone", "9025 1100", wrapper.MainPhone);
			AssertEquals("wrapper.MainFax", "9025 1199", wrapper.MainFax);
			AssertEquals("wrapper.MainEmail", "sales@edi.com.au", wrapper.MainEmail);
			AssertEquals("wrapper.ContactName", "Ben Govett", wrapper.ContactName);
			AssertEquals("wrapper.ContactPhone", "9025 1121", wrapper.ContactPhone);
			AssertEquals("wrapper.ContactFax", "9025 1199", wrapper.ContactFax);
			AssertEquals("wrapper.ContactEmail", "ben@edi.com.au", wrapper.ContactEmail);
			AssertEquals("wrapper.Contacts[All].FullName", "Ben Govett", wrapper.Contacts[ContactTypeList.Codes.All].FullName);
			AssertEquals("wrapper.Addresses[Main].Address", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.Addresses[AddressTypeList.Codes.Main].Address);
		}

		public void TestWrapperMappingGetsRightContact()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);
			OrgContact contact1 = OrgTestHelper.AddNewContact(organisation, "Ben Govett", "9025 1121", "9025 1199", "ben@edi.com.au", "0421 944 321", "8814 6253", "Hey you!", "Kickarse Specialist", "221", "LABOUR", "9481 1111", "PAGEMENOW");
			OrgDocument contact1documentType1 = OrgTestHelper.AddNewDocumentType(contact1, ContactType.All, true);
			OrgContact contact2 = OrgTestHelper.AddNewContact(organisation, "Richard White", "9025 1101", "9025 1199", "richard.white@edi.com.au", "0421 944 301", "9999 9999", "Oi!", "Him Big Chief", "201", "LABOUR", "9481 1111", "PAGEMENOW");
			OrgDocument contact2documentType1 = OrgTestHelper.AddNewDocumentType(contact2, ContactType.Consignee, true);

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.Consignee, Factory);
			AssertEquals("Test", wrapper.TypeDescription);
			AssertEquals("JOHNEATNUT", wrapper.CompanyCode);
			AssertEquals("JOHNNY EATS NUTS", wrapper.CompanyName);
			AssertEquals("wrapper.CompanyNameAndAddress", "JOHNNY EATS NUTS\n'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyNameAndAddress);
			AssertEquals("wrapper.CompanyAddress", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyAddress);
			AssertEquals("9025 1100", wrapper.MainPhone);
			AssertEquals("9025 1199", wrapper.MainFax);
			AssertEquals("sales@edi.com.au", wrapper.MainEmail);
			AssertEquals("Richard White", wrapper.ContactName);
			AssertEquals("9025 1101", wrapper.ContactPhone);
			AssertEquals("9025 1199", wrapper.ContactFax);
			AssertEquals("richard.white@edi.com.au", wrapper.ContactEmail);
			AssertEquals("Richard White", wrapper.Contacts[ContactTypeList.Codes.Consignee].FullName);
			AssertEquals("'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.Addresses[AddressTypeList.Codes.Main].Address);
		}

		public void TestWrapperMappingGetsRightContactForFreightAgentContactType()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);
			var contact1 = OrgTestHelper.AddNewContact(organisation, "Dora Doll", "9025 1121", "9025 1199", "ben@edi.com.au", "0421 944 321", "8814 6253", "Hey you!", "Kickarse Specialist", "221", "LABOUR", "9481 1111", "PAGEMENOW");
			var contact1documentType1 = OrgTestHelper.AddNewDocumentType(contact1, ContactType.ImportSeaFreightAgent, true);
			var contact2 = OrgTestHelper.AddNewContact(organisation, "Barbie Doll", "9025 1101", "9025 1199", "barbie@edi.com.au", "0421 944 301", "9999 9999", "Oi!", "Him Big Chief", "201", "LABOUR", "9481 1111", "PAGEMENOW");
			var contact2documentType1 = OrgTestHelper.AddNewDocumentType(contact2, ContactType.ImportAirFreightAgent, true);

			var wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.ImportFreightAgent, Core.Constants.TransportModes.Air, Factory);
			AssertEquals("Test", wrapper.TypeDescription);
			AssertEquals("JOHNEATNUT", wrapper.CompanyCode);
			AssertEquals("JOHNNY EATS NUTS", wrapper.CompanyName);
			AssertEquals("wrapper.CompanyNameAndAddress", "JOHNNY EATS NUTS\n'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyNameAndAddress);
			AssertEquals("wrapper.CompanyAddress", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyAddress);
			AssertEquals("9025 1100", wrapper.MainPhone);
			AssertEquals("9025 1199", wrapper.MainFax);
			AssertEquals("sales@edi.com.au", wrapper.MainEmail);
			AssertEquals("Barbie Doll", wrapper.ContactName);
			AssertEquals("9025 1101", wrapper.ContactPhone);
			AssertEquals("9025 1199", wrapper.ContactFax);
			AssertEquals("barbie@edi.com.au", wrapper.ContactEmail);
			AssertEquals("'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.Addresses[AddressTypeList.Codes.Main].Address);
		}

		public void TestWrapperMappingContactFallsBackIfRightContactNotAvailable()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);
			OrgContact contact1 = OrgTestHelper.AddNewContact(organisation, "Ben Govett", "9025 1121", "9025 1199", "ben@edi.com.au", "0421 944 321", "8814 6253", "Hey you!", "Kickarse Specialist", "221", "LABOUR", "9481 1111", "PAGEMENOW");
			OrgDocument contact1documentType1 = OrgTestHelper.AddNewDocumentType(contact1, ContactType.All, true);
			OrgContact contact2 = OrgTestHelper.AddNewContact(organisation, "Richard White", "9025 1101", "9025 1199", "richard.white@edi.com.au", "0421 944 301", "9999 9999", "Oi!", "Him Big Chief", "201", "LABOUR", "9481 1111", "PAGEMENOW");
			OrgDocument contact2documentType1 = OrgTestHelper.AddNewDocumentType(contact2, ContactType.Consignee, true);

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.Consignor, Factory);
			AssertEquals("JOHNEATNUT", wrapper.CompanyCode);
			AssertEquals("JOHNNY EATS NUTS", wrapper.CompanyName);
			AssertEquals("wrapper.CompanyNameAndAddress", "JOHNNY EATS NUTS\n'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyNameAndAddress);
			AssertEquals("wrapper.CompanyAddresss", "'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.CompanyAddress);
			AssertEquals("9025 1100", wrapper.MainPhone);
			AssertEquals("9025 1199", wrapper.MainFax);
			AssertEquals("sales@edi.com.au", wrapper.MainEmail);
			AssertEquals("Ben Govett", wrapper.ContactName);
			AssertEquals("9025 1121", wrapper.ContactPhone);
			AssertEquals("9025 1199", wrapper.ContactFax);
			AssertEquals("ben@edi.com.au", wrapper.ContactEmail);
			AssertEquals("Ben Govett", wrapper.Contacts[ContactTypeList.Codes.Consignor].FullName);
			AssertEquals("'BIRKENHEAD TOWER' BUILDING\n344 OCEAN ROAD\nMANLYVALE NSW 2454\nAUSTRALIA", wrapper.Addresses[AddressTypeList.Codes.Main].Address);
		}

		public void TestWrapperMappingFromJobDocAddress()
		{
			OrgHeader organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			OrgAddress address = organisation.MainAddress;
			OrgContact contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "BEN");

			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = address.PK;

			OrganisationWrapper wrapperFull = new OrganisationWrapper(OrganisationUsageType.Test, docAddress, Factory);
			AssertEquals("wrapperFull.TypeDescription", "Test", wrapperFull.TypeDescription);
			AssertEquals("wrapperFull.Location.UNLOCO", "AUMEL", wrapperFull.ClosestPort.UNLOCO);
			AssertEquals("wrapperFull.CompanyName", "CLINTY EATS DOGS", wrapperFull.CompanyName);
			AssertEquals("wrapperFull.CompanyNameAndAddress", "CLINTY EATS DOGS\n'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.CompanyNameAndAddress);
			AssertEquals("wrapperFull.CompanyAddress", "'IMPRESSIVE TOWER' BUILDING\n344 RED CRAYON ROAD\nMANLYVALE VIC 2354\nAUSTRALIA", wrapperFull.CompanyAddress);
			AssertEquals("wrapperFull.ContactName", "BEN_NAME", wrapperFull.ContactName);
			AssertEquals("wrapperFull.ContactPhone", "BEN_P", wrapperFull.ContactPhone);
			AssertEquals("wrapperFull.ContactFax", "BEN_F", wrapperFull.ContactFax);
			AssertEquals("wrapperFull.ContactEmail", "BEN_E", wrapperFull.ContactEmail);
			AssertEquals("wrapperFull.Phone", "BEN_P", wrapperFull.MainPhone);
			AssertEquals("wrapperFull.Fax", "BEN_F", wrapperFull.MainFax);
			AssertEquals("wrapperFull.Email", "BEN_E", wrapperFull.MainEmail);

			AssertEquals("wrapperFull.Organisation.OH_Code", "CLNTEATDOG", wrapperFull.Organisation.OH_Code);
			AssertEquals("wrapperFull.Organisation.OH_FullName", "CLINTY EATS DOGS", wrapperFull.Organisation.OH_FullName);
			AssertEquals("wrapperFull.Organisation.PK", organisation.PK, wrapperFull.Organisation.PK);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "ROTTEN DOGS AND HATSTANDS";
			docAddress.E2_Address1 = "UNMAIN ADDRESS 1";
			docAddress.E2_Address2 = "UNMAIN ADDRESS 2";
			docAddress.E2_City = "ALEXANDRIA";
			docAddress.E2_State = "NSW";
			docAddress.E2_RN_NKCountryCode = "ZA";
			docAddress.E2_Postcode = "2015";
			docAddress.E2_Contact = "OV_CONTACT";
			docAddress.E2_Phone = "OV_PHONE";
			docAddress.E2_Fax = "OV_FAX";
			docAddress.E2_Email = "OV_EMAIL";

			wrapperFull = new OrganisationWrapper(OrganisationUsageType.Test, docAddress, Factory);
			AssertEquals("wrapperFull.TypeDescription", "Test", wrapperFull.TypeDescription);
			AssertEquals("wrapperFull.Location.UNLOCO", "AUSYD", wrapperFull.ClosestPort.UNLOCO);
			AssertEquals("wrapperFull.CompanyName", "ROTTEN DOGS AND HATSTANDS", wrapperFull.CompanyName);
			AssertMultilineASCIIEquals("wrapperFull.CompanyNameAndAddress", "ROTTEN DOGS AND HATSTANDS\nUNMAIN ADDRESS 1\nUNMAIN ADDRESS 2\nALEXANDRIA\n2015\nSOUTH AFRICA", wrapperFull.CompanyNameAndAddress);
			AssertMultilineASCIIEquals("wrapperFull.CompanyAddress", "UNMAIN ADDRESS 1\nUNMAIN ADDRESS 2\nALEXANDRIA\n2015\nSOUTH AFRICA", wrapperFull.CompanyAddress);
			AssertEquals("wrapperFull.Phone", "OV_PHONE", wrapperFull.MainPhone);
			AssertEquals("wrapperFull.Fax", "OV_FAX", wrapperFull.MainFax);
			AssertEquals("wrapperFull.Email", "OV_EMAIL", wrapperFull.MainEmail);
			AssertEquals("wrapperFull.ContactName", "OV_CONTACT", wrapperFull.ContactName);
			AssertEquals("wrapperFull.ContactPhone", "OV_PHONE", wrapperFull.ContactPhone);
			AssertEquals("wrapperFull.ContactFax", "OV_FAX", wrapperFull.ContactFax);
			AssertEquals("wrapperFull.ContactEmail", "OV_EMAIL", wrapperFull.ContactEmail);

			AssertEquals("wrapperFull.Organisation.OH_Code", "MISC", wrapperFull.Organisation.OH_Code);
			AssertEquals("wrapperFull.Organisation.OH_FullName", "MISCELLANEOUS ORGANISATION (SYSTEM DEFINED)", wrapperFull.Organisation.OH_FullName);
			AssertNotEquals("wrapperFull.Organisation.PK", organisation.PK, wrapperFull.Organisation.PK);
			AssertNotEquals("wrapperFull.Organisation.PK", ZGuid.Empty, wrapperFull.Organisation.PK);
		}

		public void TestSetCountryCode()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("CLNTEATDOG", "CLINTY EATS DOGS",
				"'IMPRESSIVE TOWER' BUILDING", "344 RED CRAYON ROAD", "AU", "MANLYVALE", "2354", "VIC", "AUMEL",
				"PHONE", "FAX", "email@domain.com", "MOBILE", "", Factory);
			var address = organisation.MainAddress;
			var contact = OrgTestHelper.AddNewContact(organisation, ContactType.All, "BEN");

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_RN_NKCountryCode = "US";
			docAddress.E2_OA_Address = address.PK;

			var list = new CodeDescriptionPairList();
			list.AddPair("FR", "France");

			var wrapper = new OrganisationWrapper(OrganisationUsageType.Consignee, docAddress, "FR", list.GetMultilingualDescriptionFromCode("FR"), "FR",  Factory);

			AssertContains("Country Code should be equal to the one set in the call of OrganisationWrapper.", "FRANCE", wrapper.CompanyNameAndAddress);
		}

		public void TestCustomsCodes()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			OrgCusCode cusCode1 = header.CustomsCodes.AddNew("GST", "123456");
			cusCode1.OK_RN_NKCodeCountry = "AU";
			OrgCusCode cusCode2 = header.CustomsCodes.AddNew("GST", "987654");
			cusCode2.OK_RN_NKCodeCountry = "NZ";
			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, header, ContactType.All, Factory);
			AssertEquals("wrapper.CustomsCodes.Count", 2, wrapper.CustomsCodes.Count);
			AssertEquals("wrapper.CustomsCodes[0].RegistrationNumberOrCode", "123456", wrapper.CustomsCodes[0].RegistrationNumberOrCode);
			AssertEquals("wrapper.CustomsCodes[0].Type.Code", "GST", wrapper.CustomsCodes[0].Type.Code);
			AssertEquals("wrapper.CustomsCodes[0].CountryOfIssue.Code", "AU", wrapper.CustomsCodes[0].CountryOfIssue.Code);
			AssertEquals("wrapper.CustomsCodes[1].RegistrationNumberOrCode", "987654", wrapper.CustomsCodes[1].RegistrationNumberOrCode);
			AssertEquals("wrapper.CustomsCodes[1].Type.Code", "GST", wrapper.CustomsCodes[1].Type.Code);
			AssertEquals("wrapper.CustomsCodes[1].CountryOfIssue.Code", "NZ", wrapper.CustomsCodes[1].CountryOfIssue.Code);
		}

		public void TestAttributes()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.MiscServ.OM_IMPartAttrib1Name = "PartAttrib1";
			organisation.MiscServ.OM_IMPartAttrib2Name = "PartAttrib2";
			organisation.MiscServ.OM_IMPartAttrib3Name = "PartAttrib3";

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);

			AssertEquals("PartAttrib1", wrapper.PartAttribute1.Name);
			AssertEquals("PartAttrib2", wrapper.PartAttribute2.Name);
			AssertEquals("PartAttrib3", wrapper.PartAttribute3.Name);
		}

		public override void TestWrapperNotes()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "These notes exist against the organisation and should be algamated");
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is the second line that gets added to the first line using the indexer");
			header.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "Some agent notes that should not be joined together as there are no others");

			OrganisationWrapper orgWrapper = new OrganisationWrapper(OrganisationUsageType.Test, header, ContactType.All, Factory);
			AssertEquals("Notes Count", 3, orgWrapper.Notes.Count);

			NoteWrapper noteWrapper = orgWrapper.Notes[PredefinedNoteTypes.Instance.HandlingInstructions.Description];
			AssertNotNull("The handling instructions is found using the indexer", noteWrapper);
			AssertEquals("The Text is algamated", "These notes exist against the organisation and should be algamated\r\nThis is the second line that gets added to the first line using the indexer", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Goods Handling Instructions", noteWrapper.Description);

			noteWrapper = orgWrapper.Notes[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Int indexer gets the agent Notes", noteWrapper);
			AssertEquals("noteWrapper.Text", "Some agent notes that should not be joined together as there are no others", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Agent Notes", noteWrapper.Description);
		}

		public void TestNewConstructorWithZString()
		{
			bool flag = Env.Registry.OrgAllowMixedCase;
			try
			{
				Env.Registry.SetOrgAllowMixedCase(true);
				OrganisationWrapper testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, "name and address", Factory);
				AssertEquals("name and address", testWrapper.CompanyNameAndAddress);
				testWrapper = new OrganisationWrapper(OrganisationUsageType.Test, ZString.Empty, Factory);
			}
			finally
			{
				Env.Registry.SetOrgAllowMixedCase(flag);
			}
		}

		public void TestCountryIsLocalized()
		{
			var organisation = OrgTestHelper.GetNewOrganisation("JOHNEATNUT", "JOHNNY EATS NUTS",
				"'BIRKENHEAD TOWER' BUILDING", "344 OCEAN ROAD", "AU", "MANLYVALE", "2454", "NSW", "AUSYD",
				"9025 1100", "9025 1199", "sales@edi.com.au", "", "", Factory);
			var wrapper1 = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);

			var wrapper2 = new OrganisationWrapper(OrganisationUsageType.Test, organisation.MainAddress, ContactType.All, Factory);

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultContactType = ContactType.All;
			docAddress.E2_OA_Address = organisation.MainAddress.PK;
			var wrapper3 = new OrganisationWrapper(OrganisationUsageType.Test, docAddress, Factory);

			var wrapper4 = new OrganisationWrapper(OrganisationUsageType.Test, wrapper3);

			AssertContains("AUSTRALIA", wrapper1.CompanyAddress);
			AssertContains("AUSTRALIA", wrapper2.CompanyAddress);
			AssertContains("AUSTRALIA", wrapper3.CompanyAddress);
			AssertContains("AUSTRALIA", wrapper4.CompanyAddress);

			using (var mock = Res.UseMockData())
			{
				var key = CustomizableDataResourceStrings.GetCustomizableDataKey(RefCountry.Schema.RN_Desc, "Australia");
				mock.Put(key, new ResourceStringData(key, "Ailartsua"));

				AssertNotContains("AUSTRALIA", wrapper1.CompanyAddress);
				AssertContains("AILARTSUA", wrapper1.CompanyAddress);

				AssertNotContains("AUSTRALIA", wrapper2.CompanyAddress);
				AssertContains("AILARTSUA", wrapper2.CompanyAddress);

				AssertNotContains("AUSTRALIA", wrapper3.CompanyAddress);
				AssertContains("AILARTSUA", wrapper3.CompanyAddress);

				AssertNotContains("AUSTRALIA", wrapper4.CompanyAddress);
				AssertContains("AILARTSUA", wrapper4.CompanyAddress);
			}
		}

		public void TestRefreshMainAddressWrapperWhenLocalizationChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.OA_Language = SharedConstants.Languages.English;
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Address1 = "Bourke Street";

			var translatedAddress = address.AddNewTranslatedAddress();
			translatedAddress.OTA_Language = SharedConstants.Languages.French;
			translatedAddress.OTA_Address1 = "Bourke Rue";

			using (DocumentGenerationHelper.SetIsGeneratingDocument())
			{
				var wrapper1 = default(OrganisationWrapper);
				var wrapper2 = default(OrganisationWrapper);
				var wrapper3 = default(OrganisationWrapper);

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.French))
				{
					wrapper1 = new OrganisationWrapper(OrganisationUsageType.Debtor, org, ContactType.All, Constants.TransportModes.All, Factory);
					wrapper2 = new OrganisationWrapper(OrganisationUsageType.Debtor, org, ContactType.All, Factory);
					wrapper3 = new OrganisationWrapper(OrganisationUsageType.Debtor, org, default(OrgContact), Factory);

					AssertEquals("BOURKE RUE\nNSW\nAUSTRALIE", wrapper1.CompanyNameAndAddress);
					AssertEquals("BOURKE RUE\nNSW\nAUSTRALIE", wrapper2.CompanyNameAndAddress);
					AssertEquals("BOURKE RUE\nNSW\nAUSTRALIE", wrapper3.CompanyNameAndAddress);
				}

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.English))
				{
					wrapper1 = new OrganisationWrapper(OrganisationUsageType.Debtor, org, ContactType.All, Constants.TransportModes.All, Factory);
					wrapper2 = new OrganisationWrapper(OrganisationUsageType.Debtor, org, ContactType.All, Factory);
					wrapper3 = new OrganisationWrapper(OrganisationUsageType.Debtor, org, default(OrgContact), Factory);

					AssertEquals("BOURKE STREET\nNSW\nAUSTRALIA", wrapper1.CompanyNameAndAddress);
					AssertEquals("BOURKE STREET\nNSW\nAUSTRALIA", wrapper2.CompanyNameAndAddress);
					AssertEquals("BOURKE STREET\nNSW\nAUSTRALIA", wrapper3.CompanyNameAndAddress);
				}
			}
		}

		public void TestNoExceptionThrownWhenOrganisationIsNull()
		{
			AssertNoExceptionThrown(() =>
			{
				new OrganisationWrapper(OrganisationUsageType.Debtor, organisation: null, ContactType.All, Constants.TransportModes.All, Factory);
				new OrganisationWrapper(OrganisationUsageType.Debtor, organisation: null, ContactType.All, Factory);
			});
		}

		public void TestClientDocumentLogo()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Test Org";
			organisation.MainAddress.OA_Address1 = "Test Address";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			OrgMiscServ miscServ = organisation.MiscServ;

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var memStream = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AWBMasterTitle.png");

			var note = Factory.New<StmNote>();
			note.ST_NoteData = memStream;
			note.ST_ParentID = miscServ.PK;
			note.ST_Table = miscServ.TableName;
			note.ST_NoteType = nameof(StmNoteVisibility.DOC);
			note.ST_Description = "ClientDocumentLogo";
			Factory.Save();

			OrganisationWrapper wrapper = new OrganisationWrapper(OrganisationUsageType.Test, organisation, ContactType.All, Factory);
			AssertNotNull(wrapper.ClientDocumentLogo);
		}

		#region Implementation

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Organisation                    (Default Field: CompanyNameAndAddress)
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

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new OrganisationWrapper(OrganisationUsageType.Test, (OrgHeader)null, ContactType.All, Factory);
		}

		#endregion
	}
}
