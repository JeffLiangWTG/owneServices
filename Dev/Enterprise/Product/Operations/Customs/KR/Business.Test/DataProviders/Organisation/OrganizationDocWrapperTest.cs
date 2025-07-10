using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(OrganizationDocWrapper))]
	sealed class OrganizationDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganizationMembers()
		{
			var organisation = new Organisation(RoleType.None);
			organisation.CompanyName = "Test Company Name";
			organisation.RepresentativeName = "Test Representative Name";
			organisation.IsIndividual = true;
			organisation.Postcode = "12345";
			organisation.AddressLine1 = "Address 1";
			organisation.AddressLine2 = "Address 2";
			organisation.RoadNameCode = "99999999";
			organisation.BuildingNumber = "9999999999";
			organisation.CountryCode = "KR";
			organisation.PhoneNumber = "010-0000-0000";
			organisation.ExtensionNumber = "0000";
			organisation.Email = "test@wisetechglobal.com";
			organisation.MobileNumber = "0325632745";
			organisation.FaxNumber = "0327143888";
			AssertOrganizationMembers(new OrganizationDocWrapper(organisation));

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test Company Name";
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var orgcontact = orgHeader.Contacts.AddNew();
			orgcontact.OC_ContactName = "Test Representative Name";
			orgcontact.Allocations.AddNew().PC_Type = ContactAllocationType.CEOForKRCustoms;
			orgcontact.OC_PhoneExtension = "0000";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_PostCode = "12345";
			orgAddress.OA_Address1 = "Address 1";
			orgAddress.OA_Address2 = "Address 2";
			orgAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, "99999999");
			orgAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, "9999999999");
			orgAddress.OA_RN_NKCountryCode = "KR";
			orgAddress.OA_Phone = "010-0000-0000";
			orgAddress.OA_Email = "test@wisetechglobal.com";
			orgAddress.OA_Mobile = "0325632745";
			orgAddress.OA_Fax = "0327143888";
			AssertOrganizationMembers(new OrganizationDocWrapper(orgHeader));
			AssertOrganizationMembers(new OrganizationDocWrapper(orgAddress));
		}
		void AssertOrganizationMembers(OrganizationDocWrapper wrapper)
		{
			AssertEquals("Test Company Name", wrapper.CompanyName);
			AssertEquals("Test Representative Name", wrapper.RepresentativeName);
			AssertEquals(true, wrapper.IsIndividual);
			AssertEquals("12345", wrapper.Postcode);
			AssertEquals("123-45", wrapper.FormattedPostcode);
			AssertEquals("Address 1", wrapper.AddressLine1);
			AssertEquals("Address 2", wrapper.AddressLine2);
			AssertEquals("Address 1 Address 2", wrapper.AddressDetails);
			AssertEquals("99999999", wrapper.RoadNameCode);
			AssertEquals("9999999999", wrapper.BuildingNumber);
			AssertEquals("KR", wrapper.CountryCode);
			AssertEquals("010-0000-0000", wrapper.PhoneNumber);
			AssertEquals("0000", wrapper.ExtensionNumber);
			AssertEquals("test@wisetechglobal.com", wrapper.Email);
			AssertEquals("0325632745", wrapper.MobileNumber);
			AssertEquals("0327143888", wrapper.FaxNumber);
		}

		public void TestBusinessRegNoOrIndividualIDForOrgHeaderAndAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			orgHeader.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "BusinessRegNo");
			orgHeader.CustomsCodes.AddNew(IdentificationType.UnipassIDForIndividual, "UnipassIDForIndividual");

			AssertBusinessRegNoOrIndividualIDForOrgHeaderAndAddress(orgHeader, "UnipassIDForIndividual");

			orgHeader.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForForeigner, "KoreanRegNoForForeigner");
			AssertBusinessRegNoOrIndividualIDForOrgHeaderAndAddress(orgHeader, "KoreanRegNoForForeigner");

			orgHeader.CustomsCodes.AddNew(IdentificationType.PassportNo, "PassportNo");
			AssertBusinessRegNoOrIndividualIDForOrgHeaderAndAddress(orgHeader, "PassportNo");

			orgHeader.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "KoreanRegNoForResident");
			AssertBusinessRegNoOrIndividualIDForOrgHeaderAndAddress(orgHeader, "KoreanRegNoForResident");

			orgHeader.OH_Category = OrgConstants.Category.Business;
			AssertBusinessRegNoOrIndividualIDForOrgHeaderAndAddress(orgHeader, "BusinessRegNo");
		}
		void AssertBusinessRegNoOrIndividualIDForOrgHeaderAndAddress(OrgHeader orgHeader, string businessRegNoOrIndividualID)
		{
			var wrapper = new OrganizationDocWrapper(orgHeader);
			AssertEquals(businessRegNoOrIndividualID, wrapper.BusinessRegNoOrIndividualID);

			wrapper = new OrganizationDocWrapper(orgHeader.MainAddress);
			AssertEquals(businessRegNoOrIndividualID, wrapper.BusinessRegNoOrIndividualID);
		}

		public void TestBusinessRegNoOrIndividualIDForOrganisation()
		{
			var organisation = new Organisation(RoleType.None);
			organisation.IsIndividual = true;
			organisation.SetRegistrationIDNumbers(new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "BusinessRegNo", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForIndividual, Number = "UnipassIDForIndividual", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			});

			var wrapper = new OrganizationDocWrapper(organisation);
			AssertEquals("UnipassIDForIndividual", wrapper.BusinessRegNoOrIndividualID);

			organisation.SetRegistrationIDNumbers(new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "BusinessRegNo", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForForeigner, Number = "KoreanRegNoForForeigner", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForIndividual, Number = "UnipassIDForIndividual", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			});
			wrapper = new OrganizationDocWrapper(organisation);
			AssertEquals("KoreanRegNoForForeigner", wrapper.BusinessRegNoOrIndividualID);

			organisation.SetRegistrationIDNumbers(new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "BusinessRegNo", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.PassportNo, Number = "PassportNo", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForForeigner, Number = "KoreanRegNoForForeigner", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForIndividual, Number = "UnipassIDForIndividual", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			});
			wrapper = new OrganizationDocWrapper(organisation);
			AssertEquals("PassportNo", wrapper.BusinessRegNoOrIndividualID);

			organisation.SetRegistrationIDNumbers(new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "BusinessRegNo", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForResident, Number = "KoreanRegNoForResident", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.PassportNo, Number = "PassportNo", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.KoreanRegNoForForeigner, Number = "KoreanRegNoForForeigner", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
				new IDNumberAndType() { Type = IdentificationType.UnipassIDForIndividual, Number = "UnipassIDForIndividual", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			});
			wrapper = new OrganizationDocWrapper(organisation);
			AssertEquals("KoreanRegNoForResident", wrapper.BusinessRegNoOrIndividualID);

			organisation.IsIndividual = false;
			wrapper = new OrganizationDocWrapper(organisation);
			AssertEquals("BusinessRegNo", wrapper.BusinessRegNoOrIndividualID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			IOrganization declarant = new Organisation(RoleType.None);
			return new OrganizationDocWrapper(declarant);
		}
	}
}
