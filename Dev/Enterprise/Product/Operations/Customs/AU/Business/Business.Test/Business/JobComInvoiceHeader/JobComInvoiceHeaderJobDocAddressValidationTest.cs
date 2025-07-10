using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal class JobComInvoiceHeaderJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_OA_Address()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_FullName = "TestOrg1";
			testOrg1.OH_Code = "TS1";
			var testOrg1Address = testOrg1.MainAddress;
			testOrg1Address.OA_Phone = string.Empty;
			testOrg1Address.OA_Mobile = string.Empty;

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_FullName = "TestOrg2";
			testOrg2.OH_Code = "TS2";
			var testOrg2Address = testOrg2.MainAddress;
			testOrg2Address.OA_Phone = "02 123";
			testOrg2Address.OA_Mobile = string.Empty;

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_FullName = "TestOrg3";
			testOrg3.OH_Code = "TS3";
			var testOrg3Address = testOrg3.MainAddress;
			testOrg3Address.OA_Phone = string.Empty;
			testOrg3Address.OA_Mobile = "0402 123";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var docAddress = invHeader.AQISResponsiblePerson;
			AssertEquals(DocAddressType.AQISResponsiblePerson, docAddress.DocAddressType);
			docAddress.E2_AddressOverride = false;
			docAddress.OrganisationPK = testOrg1.PK;

			string expectedNotification = "Either Phone or Mobile is required";
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			docAddress.OrganisationPK = testOrg2.PK;
			AssertNoMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			docAddress.OrganisationPK = testOrg3.PK;
			AssertNoMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			var loadingEstablishmentDocAddress = invHeader.AQISLoadingEstablishmentLocation;
			docAddress.OrganisationPK = testOrg1.PK;
			AssertNoMessageErrorContaining(loadingEstablishmentDocAddress.E2_ContactInfo, expectedNotification);
		}

		public void TestCheckE2_Contact()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_FullName = "TestOrg1";
			testOrg1.OH_Code = "TS1";
			var testOrg1Address = testOrg1.MainAddress;
			testOrg1Address.OA_Phone = string.Empty;
			testOrg1Address.OA_Mobile = string.Empty;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var docAddress = invHeader.AQISResponsiblePerson;
			AssertEquals(DocAddressType.AQISResponsiblePerson, docAddress.DocAddressType);
			docAddress.E2_AddressOverride = false;
			docAddress.OrganisationPK = testOrg1.PK;

			string expectedNotification = "Either Phone or Mobile is required";
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			var contact1 = testOrg1.Contacts.AddNew();
			contact1.OC_Phone = string.Empty;
			contact1.OC_Mobile = string.Empty;
			docAddress.ContactPK = contact1.PK;
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			var contact2 = testOrg1.Contacts.AddNew();
			contact2.OC_Phone = "02 123";
			contact2.OC_Mobile = string.Empty;
			docAddress.ContactPK = contact2.PK;
			AssertNoMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			var contact3 = testOrg1.Contacts.AddNew();
			contact3.OC_Phone = string.Empty;
			contact3.OC_Mobile = "0402 123";
			docAddress.ContactPK = contact3.PK;
			AssertNoMessageErrorContaining(docAddress.E2_ContactInfo, expectedNotification);

			var loadingEstablishmentDocAddress = invHeader.AQISLoadingEstablishmentLocation;
			docAddress.ContactPK = contact1.PK;
			AssertNoMessageErrorContaining(loadingEstablishmentDocAddress.E2_ContactInfo, expectedNotification);
		}

		public void TestCheckE2_Phone_Formatted()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.AdditionalValidation = new JobComInvoiceHeaderJobDocAddressValidation(docAddress, Factory.New<JobComInvoiceHeader>());
			docAddress.DocAddressType = DocAddressType.AQISTransitDestination;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "TestOrg1";
			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = string.Empty;

			string expectedNotification = "Either Phone or Mobile is required";

			docAddress.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(docAddress.E2_Phone_FormattedInfo, expectedNotification);

			docAddress.E2_Phone = "02 123";
			docAddress.E2_Mobile = string.Empty;
			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining(docAddress.E2_Phone_FormattedInfo, expectedNotification);

			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = "0402 123";
			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining(docAddress.E2_Phone_FormattedInfo, expectedNotification);

			docAddress.DocAddressType = DocAddressType.AQISLoadingEstablishment;
			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = string.Empty;
			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining(docAddress.E2_Phone_FormattedInfo, expectedNotification);
		}

		public void TestCheckE2_Mobile_Formatted()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.AdditionalValidation = new JobComInvoiceHeaderJobDocAddressValidation(docAddress, Factory.New<JobComInvoiceHeader>());
			docAddress.DocAddressType = DocAddressType.AQISEUContactPerson;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "TestOrg1";
			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = string.Empty;

			string expectedNotification = "Either Phone or Mobile is required";

			docAddress.AdditionalValidation.ValidateAll();
			AssertHasMessageErrorContaining(docAddress.E2_Mobile_FormattedInfo, expectedNotification);

			docAddress.E2_Phone = "02 123";
			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining(docAddress.E2_Mobile_FormattedInfo, expectedNotification);

			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = "0402 123";
			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining(docAddress.E2_Mobile_FormattedInfo, expectedNotification);

			docAddress.DocAddressType = DocAddressType.AQISLoadingEstablishment;
			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = string.Empty;
			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrorContaining(docAddress.E2_Mobile_FormattedInfo, expectedNotification);
		}

		public void TestCheckNonAQISDocAddress()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.AdditionalValidation = new JobComInvoiceHeaderJobDocAddressValidation(docAddress, Factory.New<JobComInvoiceHeader>());
			docAddress.DocAddressType = DocAddressType.Carrier;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "TestOrg1";
			docAddress.E2_Phone = string.Empty;
			docAddress.E2_Mobile = string.Empty;

			docAddress.AdditionalValidation.ValidateAll();
			AssertNoMessageErrors(docAddress.E2_Mobile_FormattedInfo);
			AssertNoMessageErrors(docAddress.E2_Phone_FormattedInfo);
		}
	}
}
