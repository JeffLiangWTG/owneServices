using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class OrganisationWrapperTest : TestCaseWithFactory
	{
		public void TestOrganisationNumber()
		{
			var orgHeader = cusEntryHeader.DeclarantOrganisation;
			AssertEquals("", organisationWrapper.OrganisationNumber);

			var eori = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			AssertEquals("FR12345678900001", organisationWrapper.OrganisationNumber);

			eori.OK_CustomsRegNo = "OCCASIONNEL";
			AssertEquals("OCCASIONNEL", organisationWrapper.OrganisationNumber);
		}

		public void TestOrganisationNumberEoriOnly()
		{
			var orgHeader = cusEntryHeader.DeclarantOrganisation;
			AssertEquals("", organisationWrapper.OrganisationNumberEoriOnly);

			var eori = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			AssertEquals("FR12345678900001", organisationWrapper.OrganisationNumberEoriOnly);

			eori.OK_CustomsRegNo = "OCCASIONNEL";
			AssertEquals("OCCASIONNEL", organisationWrapper.OrganisationNumberEoriOnly);
		}

		public void TestOrganisationWrapperConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Entryheader", () => new OrganisationWrapper(null, Factory.New<OrgAddress>(), Factory.New<OrgHeader>()));
			});
		}

		public void TestFullName()
		{
			AssertEquals("Company", organisationWrapper.FullName);
		}

		public void TestPartnerConsigneeID()
		{
			AssertEquals(WrapperTestHelper.CustomsClientID, organisationWrapper.PartnerConsigneeID);
		}

		public void TestPartnerDestIDInfo()
		{
			AssertEquals("", organisationWrapper.PartnerDestIDInfo);
		}

		public void TestAddressData()
		{
			AssertEquals("Address1", organisationWrapper.Address);
			AssertEquals("FR", organisationWrapper.CountryCode);
			AssertEquals("24750", organisationWrapper.PostCode);
			AssertEquals("City", organisationWrapper.City);

			var address = cusEntryHeader.DeclarantOrganisation.Addresses.AddNew();
			address.Address1 = "Address1";
			address.OA_RN_NKCountryCode = "FR";
			address.OA_PostCode = "24750";
			address.OA_City = "City";
			address.CompanyName = "Company";
			var wrapper = new OrganisationWrapper(cusEntryHeader, address, cusEntryHeader.DeclarantOrganisation, "QR");
			AssertEquals("QR", wrapper.CountryCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var wrapperHelper = new WrapperTestHelper();
			cusEntryHeader = wrapperHelper.CreateTestCusEntryHeader();
			var address = cusEntryHeader.DeclarantOrganisation.Addresses.AddNew();
			address.Address1 = "Address1";
			address.OA_RN_NKCountryCode = "FR";
			address.OA_PostCode = "24750";
			address.OA_City = "City";
			address.CompanyName = "Company";
			organisationWrapper = new OrganisationWrapper(cusEntryHeader, address, cusEntryHeader.DeclarantOrganisation);
		}

		Declaration.CusEntryHeader cusEntryHeader;
		IOrganisation organisationWrapper;
	}
}
