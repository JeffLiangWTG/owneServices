using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecDeclarantDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new EdecDeclarantDataProvider(null));
	}

	public void TestNewDeclarantDataProvider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = Factory.New<OrgAddress>();
		orgHeader.Addresses.Add(orgAddress);

		var declarantDataProvider = EdecDeclarantDataProvider.New(null);
		AssertNull(declarantDataProvider);

		declaration.JE_OA_DeclarantAddress = orgAddress.PK;
		declarantDataProvider = EdecDeclarantDataProvider.New(declaration);
		AssertType<EdecDeclarantDataProvider>(declarantDataProvider);
	}

	public void TestDeclarantDataProvider()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarantOrgHeader = Factory.New<OrgHeader>();
		var declarantOrgAddress = Factory.New<OrgAddress>();
		var forwarderOrgHeader = Factory.New<OrgHeader>();
		var forwarderOrgAddress = Factory.New<OrgAddress>();

		declarantOrgHeader.OH_FullName = "Declarant Company";
		declarantOrgAddress.OA_Address1 = "Declarant Street";
		declarantOrgAddress.OA_PostCode = "DeclarantP";
		declarantOrgAddress.OA_City = "Declarant City";
		declarantOrgAddress.OA_RN_NKCountryCode = "US";
		declarantOrgAddress.OA_State = "NY";
		declarantOrgHeader.Addresses.Add(declarantOrgAddress);

		forwarderOrgHeader.OH_FullName = "Forwarder Company";
		forwarderOrgAddress = forwarderOrgHeader.MainAddress;
		forwarderOrgAddress.OA_Address1 = "Forwarder Street";
		forwarderOrgAddress.OA_PostCode = "ForwarderP";
		forwarderOrgAddress.OA_City = "Forwarder City";
		forwarderOrgAddress.OA_RN_NKCountryCode = "CA";
		forwarderOrgAddress.OA_State = "BC";

		CombineAssertions(() =>
		{
			var mainAddress = declaration.Branch?.OrgProxy?.MainAddress;
			var declarantDataProvider = EdecDeclarantDataProvider.New(declaration);
			assertDeclarantDataProvider("No declarant", mainAddress.Header?.OH_FullName, mainAddress.OA_Address1, mainAddress.Postcode, mainAddress.OA_City, mainAddress.OA_RN_NKCountryCode);

			declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;
			declarantDataProvider = EdecDeclarantDataProvider.New(declaration);
			assertDeclarantDataProvider("With declarant", declarantOrgHeader.OH_FullName, declarantOrgAddress.OA_Address1, declarantOrgAddress.Postcode, declarantOrgAddress.OA_City, declarantOrgAddress.OA_RN_NKCountryCode);

			declaration.JE_OH_Forwarder = forwarderOrgHeader.PK;
			declarantDataProvider = EdecDeclarantDataProvider.New(declaration);
			assertDeclarantDataProvider("With forwarder", forwarderOrgHeader.OH_FullName, forwarderOrgAddress.OA_Address1, forwarderOrgAddress.Postcode, forwarderOrgAddress.OA_City, forwarderOrgAddress.OA_RN_NKCountryCode);

			void assertDeclarantDataProvider(string assertionMessage, string expectedName, string expectedStreet, string expectedPostcode, string expectedCity, string expectedCountry)
			{
				AssertEquals($"{assertionMessage}: Name", expectedName, declarantDataProvider.Name);
				AssertEquals($"{assertionMessage}: Street", expectedStreet, declarantDataProvider.Street);
				AssertEquals($"{assertionMessage}: Postcode", expectedPostcode, declarantDataProvider.PostalCode);
				AssertEquals($"{assertionMessage}: City", expectedCity, declarantDataProvider.City);
				AssertEquals($"{assertionMessage}: Country", expectedCountry, declarantDataProvider.Country);
			}
		});
	}

	public void TestTraderIdentificationNumber()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
			var declarantDataProvider = EdecDeclarantDataProvider.New(declaration);
			AssertEquals("TraderIdentificationNumber Default Value", "-", declarantDataProvider.TraderIdentificationNumber);

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456789";
			AssertEquals("TraderIdentificationNumber", "123456789", declarantDataProvider.TraderIdentificationNumber);
		});
	}

	public void TestDeclarantNumber()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			var declarantDataProvider = EdecDeclarantDataProvider.New(declaration);
			AssertEquals("DeclarantNumber Default Value", "0", declarantDataProvider.DeclarantNumber);

			var agent = Factory.NewWithValidTestData<GlbStaff>();
			agent.GS_Code = "AG1";
			declaration.JE_GS_NKCusAgent = agent.GS_Code;
			AssertEquals("DeclarantNumber Default Value", "0", declarantDataProvider.DeclarantNumber);

			declaration.CHDPassword.GP_UserID = ZString.Empty;
			AssertEquals("DeclarantNumber Default Value", "0", declarantDataProvider.DeclarantNumber);

			declaration.CHDPassword.GP_UserID = "123456";
			AssertEquals("DeclarantNumber", "123456", declarantDataProvider.DeclarantNumber);
		});
	}
}
