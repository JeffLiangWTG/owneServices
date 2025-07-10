using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class DocAddressValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		#region TestDocAddressWithOrgAddress_WithOrganisationTypePassedIn

		public void TestDocAddressWithOrgAddress_WithOrganisationTypePassedIn()
		{
			var unmatchedOrg = new UnmatchedOrganisation(Factory);
			unmatchedOrg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg);

			var docAddress = new Xsd.DocAddress();
			docAddress.AddressType = Xsd.DocAddressAddressType.CRD;
			docAddress.AddressTypeSpecified = true;

			var reference = new Xsd.AddressReference();
			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";

			var addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			var address1 = addressCollection.AddNew();
			var capability = address1.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
			address1.AddressLine1 = "Test Address1";
			address1.Sequence = 4;

			var address2 = addressCollection.AddNew();
			var capability2 = address2.AddressCapabilities.AddNew();
			capability2.AddressType = Xsd.AddressCapabilityAddressType.DLV;
			address2.AddressLine1 = "Test Address2";
			address2.Sequence = 2;
			reference.AddressSequenceRef = 2;
			docAddress.AddressReference = reference;
			docAddress.AddressReference.IsSpecified = true;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var parent = new JobDocAddressParentForTestingWithNotes(Factory);
			var docAddresses = new JobDocAddressDependentCollection(parent);
			var matchedDocAddress = DocAddressHelper.CreateOrUpdateFromValueObject(docAddresses, docAddress, context, OrganisationTypes.WarehouseClient);

			AssertEquals("Should match the correct address", "NO ADDRESS SPECIFIED", matchedDocAddress.E2_Address1);
			AssertMultilineASCIIEquals("Should be Unmatched Org Details Note", @"Organisation Type: WarehouseClient
Owner Code: 
EDI Code: 
Organisation Name: test org
Address Line 1: Test Address1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 ", parent.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description)[0].ST_NoteText);

			docAddresses.RemoveAndDeleteAll();

			var otherMatchedDocAddress = DocAddressHelper.CreateOrUpdateFromValueObject(docAddresses, docAddress, context, OrganisationTypes.WarehouseClient, OrganisationsSubTypeList.Codes.ReceivingForwarder);
			AssertEquals("Should match the correct address", "NO ADDRESS SPECIFIED", otherMatchedDocAddress.E2_Address1);
			AssertMultilineASCIIEquals("Should be Unmatched Org Details Note", @"Organisation Type: WarehouseClient
Owner Code: 
EDI Code: 
Organisation Name: test org
Address Line 1: Test Address1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 
Organisation Type: RFWD
Owner Code: 
EDI Code: 
Organisation Name: test org
Address Line 1: Test Address1
Address Line 2: 
City: 
Post Code: 
State or Province: 
Country: 
Doc Address Type: 
 ", parent.GetNotes().FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description)[0].ST_NoteText);
		}

		#endregion

		public void TestDocAddressWithOrgAddress()
		{
			Xsd.DocAddress docAddressVO = new Xsd.DocAddress();
			docAddressVO.AddressType = Xsd.DocAddressAddressType.CED;

			Xsd.AddressReference reference = new Xsd.AddressReference();

			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.OrganisationDetails.Name = "test org";
			reference.Organisation.OrganisationDetails.RegistrationNumbers.FindOrCreateForCurrentCountry(Xsd.RegistrationNumberTypes.GST).Number = "test org";
			Xsd.OrgAddressCollection addressCollection = new Xsd.OrgAddressCollection();
			reference.Organisation.OrganisationDetails.Addresses = addressCollection;

			Xsd.OrgAddress address1 = addressCollection.AddNew();
			Xsd.AddressCapability capability = address1.AddressCapabilities.AddNew();
			capability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
			address1.AddressLine1 = "Test Address1";
			address1.Sequence = 4;
			Xsd.OrgAddress address2 = addressCollection.AddNew();
			Xsd.AddressCapability capability2 = address2.AddressCapabilities.AddNew();
			capability2.AddressType = Xsd.AddressCapabilityAddressType.DLV;
			address2.AddressLine1 = "Test Address2";
			address2.Sequence = 2;

			reference.AddressSequenceRef = 2;

			docAddressVO.AddressReference = reference;
			docAddressVO.AddressReference.IsSpecified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));

			var matchedDocAddress = DocAddressHelper.CreateOrUpdateFromValueObject(docAddresses, docAddressVO, context);

			AssertEquals("Should match the correct address", "Test Address2", matchedDocAddress.E2_Address1);
			AssertEquals("Should have updated the PK in the DOcAddress", AddressHelper.FromAddressReferenceGetAddressPK(reference, context), matchedDocAddress.E2_OA_Address);
		}

		public void TestDocAddressWithOrgAddressAndOrgWithEDICode()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TBPAKL";
			org.OH_FullName = "The Butchers Place";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "MHOSYD";
			org2.OH_FullName = "My Home";

			Factory.Save();

			Xsd.DocAddress docAddressVO = new Xsd.DocAddress();
			docAddressVO.AddressType = Xsd.DocAddressAddressType.CED;
			Xsd.AddressReference reference = new Xsd.AddressReference();

			reference.Organisation = new Xsd.Organisation();
			reference.Organisation.EDICode = org2.OH_Code;

			reference.AddressSequenceRef = 1;

			docAddressVO.AddressReference = reference;
			docAddressVO.AddressReference.IsSpecified = true;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			var matchedDocAddress = DocAddressHelper.CreateOrUpdateFromValueObject(docAddresses, docAddressVO, context);

			AssertEquals("Should match the correct org", org2.PK, matchedDocAddress.OrganisationPK);
		}

		public void TestDocAddressWithOverride()
		{
			Xsd.DocAddress docAddressVO = new Xsd.DocAddress();
			docAddressVO.AddressType = Xsd.DocAddressAddressType.CED;

			docAddressVO.CompanyName = "Address1";
			docAddressVO.AddressLine1 = "Address1";
			docAddressVO.AddressLine2 = "Address2";
			docAddressVO.CityOrSuburb = "CITY";
			docAddressVO.StateOrProvince = "STATE";
			docAddressVO.PostCode = "PC1234";
			docAddressVO.CountryCode = "AU";

			docAddressVO.ContactName = "ME me & Me";

			Xsd.TelephoneNumber business = new Enterprise.DataTransfer.Xml.XsdVersion1.TelephoneNumber();
			business.NumberType = Xsd.TelephoneNumberNumberType.Business;
			business.Value = "12345";
			Xsd.TelephoneNumber fax = new Enterprise.DataTransfer.Xml.XsdVersion1.TelephoneNumber();
			fax.NumberType = Xsd.TelephoneNumberNumberType.Fax;
			fax.Value = "54321";
			docAddressVO.TelephoneNumbers.Add(business);
			docAddressVO.TelephoneNumbers.Add(fax);
			docAddressVO.Email = "EmailAddress";
			docAddressVO.IsResidential = true;
			docAddressVO.RegistrationNumber.Number = "ABN1231233";
			docAddressVO.RegistrationNumber.NumberType = Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumberTypes.MID;

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));

			var matchedDocAddress = DocAddressHelper.CreateOrUpdateFromValueObject(docAddresses, docAddressVO, context);

			AssertEquals("Should match - address must be overridden.", true, matchedDocAddress.E2_AddressOverride);

			AssertEquals("Should match - AddressType", docAddressVO.AddressType.ToString(), matchedDocAddress.E2_AddressType);

			AssertEquals("Should match - CompanyName", docAddressVO.CompanyName, matchedDocAddress.E2_CompanyName);
			AssertEquals("Should match - AddressLine1", docAddressVO.AddressLine1, matchedDocAddress.E2_Address1);
			AssertEquals("Should match - AddressLine2", docAddressVO.AddressLine2, matchedDocAddress.E2_Address2);
			AssertEquals("Should match - CityOrSuburb", docAddressVO.CityOrSuburb, matchedDocAddress.E2_City);
			AssertEquals("Should match - StateOrProvince", docAddressVO.StateOrProvince, matchedDocAddress.E2_State);
			AssertEquals("Should match - PostCode", docAddressVO.PostCode, matchedDocAddress.E2_Postcode);
			AssertEquals("Should match - CountryCode", docAddressVO.CountryCode, matchedDocAddress.E2_RN_NKCountryCode);

			AssertEquals("Should match - ContactName", docAddressVO.ContactName, matchedDocAddress.E2_Contact);
			AssertEquals("Should match - BusinessPhone", business.Value, matchedDocAddress.E2_Phone);
			AssertEquals("Should match - Fax", fax.Value, matchedDocAddress.E2_Fax);
			AssertEquals("Should match - Email", docAddressVO.Email, matchedDocAddress.E2_Email);
			AssertEquals(true, matchedDocAddress.E2_IsResidential);
			AssertEquals("Should match - RegistrationNumber.Number", "ABN1231233", matchedDocAddress.E2_GovRegNum);
			AssertEquals("Should match - RegistrationNumber.NumberType", OrgCusCode.USACodeTypes.ManufacturerID, matchedDocAddress.E2_GovRegNumType);
		}

		#endregion

		#region Export

		public void TestExportSubClassDocAddress()
		{
			GetOrgWithExtraAddress();

			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			JobDocAddress docAddressBO = docAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
			docAddressBO.E2_OA_Address = OrgA.PK;
			docAddressBO.E2_Contact = "Wally";

			var buffer = new NotificationBuffer();
			var exportContext = new ValueObjectExportContext(buffer);
			Xsd.ISFManufacturer docAddressVO = DocAddressHelper.ExportToValueObject<Xsd.ISFManufacturer>(docAddressBO, exportContext);
			docAddressVO.ManufacturerID = "MANID123";

			AssertEquals("Should match - CompanyName.", docAddressVO.CompanyName, "");
			AssertEquals("Should match - AddressType.", docAddressVO.AddressType, Xsd.DocAddressAddressType.SUD);
			AssertEquals("Should match - AddressLine1.", docAddressVO.AddressLine1, "");
			AssertEquals("Should match - AddressLine2.", docAddressVO.AddressLine2, "");
			AssertEquals("Should match - CityOrSuburb.", docAddressVO.CityOrSuburb, "");
			AssertEquals("Should match - StateOrProvince.", docAddressVO.StateOrProvince, "");
			AssertEquals("Should match - PostCode.", docAddressVO.PostCode, "");
			AssertEquals("Should match - CountryCode.", docAddressVO.CountryCode, "");
			AssertEquals("Should match - ContactName.", docAddressVO.ContactName, docAddressBO.E2_Contact);
			AssertEquals("Should match - TelephoneNumber Count.", docAddressVO.TelephoneNumbers.Count, 2);
			AssertEquals("Should match - Phone.", docAddressVO.TelephoneNumbers[0].Value, docAddressBO.E2_Phone);
			AssertEquals("Should match - Fax.", docAddressVO.TelephoneNumbers[1].Value, docAddressBO.E2_Fax);
			AssertEquals("Should match - Email.", docAddressVO.Email, docAddressBO.E2_Email);
			AssertEquals(false, docAddressVO.IsResidential);
			AssertEquals("MANID123", docAddressVO.ManufacturerID);
		}

		public void TestExportDocAddressWithOrgAddress()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			GetOrgWithExtraAddress();

			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			JobDocAddress docAddressBO = docAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
			docAddressBO.E2_OA_Address = OrgA.PK;
			docAddressBO.E2_Contact = "Wally";

			var buffer = new NotificationBuffer();
			var context = new ValueObjectExportContext(buffer);
			Xsd.DocAddress docAddressVO = DocAddressHelper.ExportToValueObject(docAddressBO, context);

			AssertEquals("Should match - CompanyName.", docAddressVO.CompanyName, "");
			AssertEquals("Should match - AddressType.", docAddressVO.AddressType, Xsd.DocAddressAddressType.SUD);
			AssertEquals("Should match - AddressLine1.", docAddressVO.AddressLine1, "");
			AssertEquals("Should match - AddressLine2.", docAddressVO.AddressLine2, "");
			AssertEquals("Should match - CityOrSuburb.", docAddressVO.CityOrSuburb, "");
			AssertEquals("Should match - StateOrProvince.", docAddressVO.StateOrProvince, "");
			AssertEquals("Should match - PostCode.", docAddressVO.PostCode, "");
			AssertEquals("Should match - CountryCode.", docAddressVO.CountryCode, "");
			AssertEquals("Should match - ContactName.", docAddressVO.ContactName, docAddressBO.E2_Contact);
			AssertEquals("Should match - TelephoneNumber Count.", docAddressVO.TelephoneNumbers.Count, 2);
			AssertEquals("Should match - Phone.", docAddressVO.TelephoneNumbers[0].Value, docAddressBO.E2_Phone);
			AssertEquals("Should match - Fax.", docAddressVO.TelephoneNumbers[1].Value, docAddressBO.E2_Fax);
			AssertEquals("Should match - Email.", docAddressVO.Email, docAddressBO.E2_Email);
			AssertEquals(false, docAddressVO.IsResidential);

			Xsd.AddressReference reference = docAddressVO.AddressReference;
			AssertEquals("Should match - Code.", reference.Organisation.EDICode, docAddressBO.Organisation.OH_Code);
			AssertEquals("Should match - Name.", reference.Organisation.OrganisationDetails.Name, docAddressBO.E2_CompanyName);
			AssertEquals("Should match - Address Count.", reference.Organisation.OrganisationDetails.Addresses.Count, 2);

			Xsd.OrgAddress orgAVO = reference.Organisation.OrganisationDetails.Addresses[1];
			AssertEquals("Should match - AddressLine1.", orgAVO.AddressLine1, OrgA.OA_Address1);
			AssertEquals("Should match - AddressLine2.", orgAVO.AddressLine2, OrgA.OA_Address2);
			AssertEquals("Should match - CityOrSuburb.", orgAVO.CityOrSuburb, OrgA.OA_City);
			AssertEquals("Should match - StateOrProvince.", orgAVO.StateOrProvince, OrgA.OA_State);
			AssertEquals("Should match - PostCode.", orgAVO.PostCode, OrgA.OA_PostCode);
			AssertEquals("Should match - Location.", orgAVO.Location.Value, OrgA.OA_RL_NKRelatedPortCode);
			AssertEquals("Should match - TelephoneNumber Count.", orgAVO.TelephoneNumbers.Count, 2);
			AssertEquals("Should match - Phone.", orgAVO.TelephoneNumbers[0].Value, docAddressBO.E2_Phone);
			AssertEquals("Should match - Fax.", orgAVO.TelephoneNumbers[1].Value, docAddressBO.E2_Fax);
			AssertEquals("Should match - Email.", orgAVO.Email, docAddressBO.E2_Email);
		}

		public void TestExportDocAddressWithOverride()
		{
			GetOrgWithExtraAddress();

			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			JobDocAddress docAddressBO = docAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
			docAddressBO.E2_OA_Address = OrgA.PK;
			docAddressBO.E2_Contact = "Wally";
			docAddressBO.E2_AddressOverride = true;
			docAddressBO.E2_IsResidential = true;
			docAddressBO.E2_GovRegNum = "MID2343233";
			docAddressBO.E2_GovRegNumType = OrgCusCode.USACodeTypes.ManufacturerID;

			var buffer = new NotificationBuffer();
			var exportContext = new ValueObjectExportContext(buffer);
			Xsd.DocAddress docAddressVO = DocAddressHelper.ExportToValueObject(docAddressBO, exportContext);

			AssertEquals("Should match - CompanyName.", docAddressVO.CompanyName, docAddressBO.E2_CompanyName);
			AssertEquals("Should match - AddressType.", docAddressVO.AddressType, Xsd.DocAddressAddressType.SUD);
			AssertEquals("Should match - AddressLine1.", docAddressVO.AddressLine1, OrgA.OA_Address1);
			AssertEquals("Should match - AddressLine2.", docAddressVO.AddressLine2, OrgA.OA_Address2);
			AssertEquals("Should match - CityOrSuburb.", docAddressVO.CityOrSuburb, OrgA.OA_City);
			AssertEquals("Should match - StateOrProvince.", docAddressVO.StateOrProvince, OrgA.OA_State);
			AssertEquals("Should match - PostCode.", docAddressVO.PostCode, OrgA.OA_PostCode);
			AssertEquals("Should match - CountryCode.", docAddressVO.CountryCode, docAddressBO.E2_RN_NKCountryCode);
			AssertEquals("Should match - ContactName.", docAddressVO.ContactName, docAddressBO.E2_Contact);
			AssertEquals("Should match - TelephoneNumber Count.", docAddressVO.TelephoneNumbers.Count, 2);
			AssertEquals("Should match - Phone.", docAddressVO.TelephoneNumbers[0].Value, docAddressBO.E2_Phone);
			AssertEquals("Should match - Fax.", docAddressVO.TelephoneNumbers[1].Value, docAddressBO.E2_Fax);
			AssertEquals("Should match - Email.", docAddressVO.Email, docAddressBO.E2_Email);
			AssertEquals(true, docAddressVO.IsResidential);

			Xsd.AddressReference reference = docAddressVO.AddressReference;
			AssertEquals("The AddressReference should not be specified.", false, reference.IsSpecified);
			Xsd.RegistrationNumber registrationNumber = docAddressVO.RegistrationNumber;
			AssertEquals("The RegistrationNumber should be specified.", true, registrationNumber.IsSpecified);
			AssertEquals("Should match - RegistrationNumber.Number", "MID2343233", registrationNumber.Number);
			AssertEquals("Should match - RegistrationNumber.Number", Enterprise.DataTransfer.Xml.XsdVersion1.RegistrationNumberTypes.MID, registrationNumber.NumberType);
		}

		public void TestExportDocAddressWithOrgAddressAndContact()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			GetOrgWithExtraAddress();

			OrgC = OrgH.Contacts.AddNew();
			OrgC.OC_ContactName = "Arthur (Bulldog) Black";
			OrgC.OC_Phone = "456789";
			OrgC.OC_Fax = "987654";
			OrgC.OC_Email = "Bulldog@yahoo.com";

			JobDocAddressDependentCollection docAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory));
			JobDocAddress docAddressBO = docAddresses.FindOrCreateWithDocAddressType(DocAddressType.SupplierDocumentaryAddress);
			docAddressBO.E2_OA_Address = OrgA.PK;
			docAddressBO.ContactPK = OrgC.PK;

			AssertEquals("Should match - ContactName.", docAddressBO.E2_Contact, OrgC.OC_ContactName);
			AssertEquals("Should match - Phone.", docAddressBO.E2_Phone, OrgC.OC_Phone);
			AssertEquals("Should match - Fax.", docAddressBO.E2_Fax, OrgC.OC_Fax);
			AssertEquals("Should match - Email.", docAddressBO.E2_Email, OrgC.OC_Email);

			var buffer = new NotificationBuffer();
			var exportContext = new ValueObjectExportContext(buffer);
			Xsd.DocAddress docAddressVO = DocAddressHelper.ExportToValueObject(docAddressBO, exportContext);

			AssertEquals("Should match - CompanyName.", docAddressVO.CompanyName, "");
			AssertEquals("Should match - AddressType.", docAddressVO.AddressType, Xsd.DocAddressAddressType.SUD);
			AssertEquals("Should match - AddressLine1.", docAddressVO.AddressLine1, "");
			AssertEquals("Should match - AddressLine2.", docAddressVO.AddressLine2, "");
			AssertEquals("Should match - CityOrSuburb.", docAddressVO.CityOrSuburb, "");
			AssertEquals("Should match - StateOrProvince.", docAddressVO.StateOrProvince, "");
			AssertEquals("Should match - PostCode.", docAddressVO.PostCode, "");
			AssertEquals("Should match - CountryCode.", docAddressVO.CountryCode, "");
			AssertEquals("Should match - ContactName.", docAddressVO.ContactName, docAddressBO.E2_Contact);
			AssertEquals("Should match - TelephoneNumber Count.", docAddressVO.TelephoneNumbers.Count, 2);
			AssertEquals("Should match - Phone.", docAddressVO.TelephoneNumbers[0].Value, docAddressBO.E2_Phone);
			AssertEquals("Should match - Fax.", docAddressVO.TelephoneNumbers[1].Value, docAddressBO.E2_Fax);
			AssertEquals("Should match - Email.", docAddressVO.Email, docAddressBO.E2_Email);

			Xsd.AddressReference reference = docAddressVO.AddressReference;
			AssertEquals("Should match - Code.", reference.Organisation.EDICode, docAddressBO.Organisation.OH_Code);
			AssertEquals("Should match - Name.", reference.Organisation.OrganisationDetails.Name, docAddressBO.E2_CompanyName);
			AssertEquals("Should match - Address Count.", reference.Organisation.OrganisationDetails.Addresses.Count, 2);

			Xsd.OrgAddress orgAVO = reference.Organisation.OrganisationDetails.Addresses[1];
			AssertEquals("Should match - AddressLine1.", orgAVO.AddressLine1, OrgA.OA_Address1);
			AssertEquals("Should match - AddressLine2.", orgAVO.AddressLine2, OrgA.OA_Address2);
			AssertEquals("Should match - CityOrSuburb.", orgAVO.CityOrSuburb, OrgA.OA_City);
			AssertEquals("Should match - StateOrProvince.", orgAVO.StateOrProvince, OrgA.OA_State);
			AssertEquals("Should match - PostCode.", orgAVO.PostCode, OrgA.OA_PostCode);
			AssertEquals("Should match - Location.", orgAVO.Location.Value, OrgA.OA_RL_NKRelatedPortCode);
		}

		#endregion

		#region ExportToOrganisationValueObject

		public void TestExportToOrganisationValueObject()
		{
			Xsd.Organisation organisation = DocAddressHelper.ExportToOrganisationValueObject(null, exportContext);
			AssertNull(organisation);

			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Hello Inc.";

			JobDocAddress jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = header.MainAddress.PK;

			organisation = DocAddressHelper.ExportToOrganisationValueObject(jobDocAddress, exportContext);
			AssertEquals("Fallback to organisation's exporter when address is not overriden", "Hello Inc.", organisation.OrganisationDetails.Name);

			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			jobDocAddress.E2_AddressOverride = true;

			jobDocAddress.E2_CompanyName = "Evil Inc.";
			jobDocAddress.E2_Address1 = "Address line 1";
			jobDocAddress.E2_Address2 = "Address line 2";
			jobDocAddress.E2_City = "Sydney";
			jobDocAddress.E2_State = "NSW";
			jobDocAddress.E2_Postcode = "2015";
			jobDocAddress.E2_Email = "join@thedarkside.com";
			jobDocAddress.E2_Phone = "100500";
			jobDocAddress.E2_Fax = "1800100";
			jobDocAddress.E2_RN_NKCountryCode = "AU";
			jobDocAddress.E2_Contact = "Darth V.";

			organisation = DocAddressHelper.ExportToOrganisationValueObject(jobDocAddress, exportContext);

			AssertEquals("UNMATCHED", organisation.EDICode);
			AssertEquals("UNMATCHED", organisation.OwnerCode);
			AssertEquals("Evil Inc.", organisation.OrganisationDetails.Name);

			Xsd.OrgAddress xsdAddress = organisation.OrganisationDetails.Addresses.GetMainOrFirstAddress();
			AssertEquals(Xsd.OrgAddressAddressType.MAIN, xsdAddress.AddressType);

			var addressCapabilities = xsdAddress.AddressCapabilities.Cast<Xsd.AddressCapability>();
			AssertNotNull(addressCapabilities.FirstOrDefault(c => c.AddressType == Xsd.AddressCapabilityAddressType.MAIN));
			AssertNotNull(addressCapabilities.FirstOrDefault(c => c.AddressType == Xsd.AddressCapabilityAddressType.OFC && c.IsMainAddress == Xsd.TrueFalse.@true));

			AssertEquals(Constants.Languages.English, xsdAddress.Language);

			AssertEquals("Address line 1", xsdAddress.AddressLine1);
			AssertEquals("Address line 2", xsdAddress.AddressLine2);
			AssertEquals("Address line 1", xsdAddress.AddressCode);

			AssertEquals("Sydney", xsdAddress.CityOrSuburb);
			AssertEquals("NSW", xsdAddress.StateOrProvince);
			AssertEquals("2015", xsdAddress.PostCode);
			AssertEquals("join@thedarkside.com", xsdAddress.Email);

			AssertEquals("100500", xsdAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Business));
			AssertEquals("1800100", xsdAddress.GetPhoneNumber(Xsd.TelephoneNumberNumberType.Fax));

			AssertEquals("AUZZZ", xsdAddress.Location.Value);
			AssertEquals("AUZZZ", organisation.OrganisationDetails.Location.Value);

			var contact = organisation.OrganisationDetails.Contacts[0];
			AssertEquals("Darth V.", contact.Name);
			AssertEquals("join@thedarkside.com", contact.EmailAddress);
			AssertEquals("100500", contact.Phone);
			AssertEquals("1800100", contact.Fax);
			AssertEquals(Constants.Languages.English, contact.Language);
			AssertEquals(Constants.ContactNotifyModes.Email, contact.NotifyMode);
			AssertEquals(OrgConstants.AttachmentType.PDF, contact.AttachmentType);

			organisation = DocAddressHelper.ExportToOrganisationValueObject(jobDocAddress, exportContext, false);
			AssertEquals("Contact not exported", 0, organisation.OrganisationDetails.Contacts.Count);
		}

		#endregion

		#region Implementation

		class JobDocAddressParentForTestingWithNotes : JobDocAddressParentForTesting, IStmNoteParent
		{
			public JobDocAddressParentForTestingWithNotes(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public BusinessObject[] BusinessObjectsWithRelatedNotes
			{
				get { throw new NotImplementedException(); }
			}

			public GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public StmNoteContexts NoteContextsForRelatedNotes
			{
				get { throw new NotImplementedException(); }
			}

			public NoteTypeCollection NoteTypes
			{
				get
				{
					var collection = new NoteTypeCollection();
					collection.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
					return collection;
				}
			}

			public Notes Notes
			{
				get { return notes ?? (notes = new Notes(this)); }
			}

			Notes notes;

			public BusinessObjectFactory NotesFactory
			{
				get { return Factory; }
			}

			public ZGuid NotesParentPK
			{
				get { return PK; }
			}

			public string NotesParentTableName
			{
				get { return TableName; }
			}

			public bool SupportsNotes
			{
				get { return true; }
			}
		}

		NotificationBuffer buffer;
		ValueObjectExportContext exportContext;

		readonly DocAddressValueObjectHelper DocAddressHelper = new DocAddressValueObjectHelper("");
		readonly AddressValueObjectHelper AddressHelper = new AddressValueObjectHelper("");

		void GetOrgWithExtraAddress()
		{
			OrgH = Factory.New<OrgHeader>();
			OrgA = OrgH.Addresses.AddNew();
			OrgA.OA_Address1 = "Address1";
			OrgA.OA_Address2 = "Address2";
			OrgA.OA_City = "City";
			OrgA.OA_State = "State";
			OrgA.OA_PostCode = "PC123";
			OrgA.OA_RL_NKRelatedPortCode = "AUSYD";
			OrgA.OA_Phone = "12345";
			OrgA.OA_Fax = "54321";
			OrgA.OA_Email = "anybody@nowhere.com";

			OrgH.OH_FullName = "My Company";
			OrgH.OH_Code = "MMMMM";

			buffer = new NotificationBuffer();
			exportContext = new ValueObjectExportContext(buffer);
		}

		OrgHeader OrgH;
		OrgAddress OrgA;
		OrgContact OrgC;

		#endregion
	}
}
