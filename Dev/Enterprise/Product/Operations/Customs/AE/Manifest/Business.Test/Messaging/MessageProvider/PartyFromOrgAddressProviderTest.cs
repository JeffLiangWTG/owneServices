using System;
using CargoWise.Types;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class PartyFromOrgAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyFromOrgAddressProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new PartyFromOrgAddressProvider(null, null), "Null OrgAddress");
			AssertNoExceptionThrown("Valid OrgAddress", () => new PartyFromOrgAddressProvider(orgAddress, null));
		});
	}

	[ExpectNoExceptions]
	public void TestPartyName()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().PartyName, Is.Null.Or.Empty, "Org name not set - should be [null] or [empty]");

			org.OH_FullName = "ABC";
			NUnit.Framework.Assert.That(GetProvider().PartyName, Is.EqualTo("ABC"), "Org name set");
		});
	}

	[ExpectNoExceptions]
	public void TestStreetAddress()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().StreetAddress, Is.Null.Or.Empty, "Street Address not set - should be [null] or [empty]");

			orgAddress.OA_Address1 = "ABC";
			NUnit.Framework.Assert.That(GetProvider().StreetAddress, Is.EqualTo("ABC"), "Only Address1 set");

			orgAddress.OA_Address1 = ZString.Empty;
			orgAddress.OA_Address2 = "LMN";
			NUnit.Framework.Assert.That(GetProvider().StreetAddress, Is.EqualTo("LMN"), "Only Address2 set");

			orgAddress.OA_Address1 = "ABC";
			NUnit.Framework.Assert.That(GetProvider().StreetAddress, Is.EqualTo("ABC LMN"), "Street Address set");
		});
	}

	[ExpectNoExceptions]
	public void TestCity()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(GetProvider().City, Is.Null.Or.Empty, "City not set - should be [null] or [empty]");

			orgAddress.OA_City = "ABC";
			NUnit.Framework.Assert.That(GetProvider().City, Is.EqualTo("ABC"), "City is set");
		});
	}

	[ExpectNoExceptions]
	public void TestCountry() => NUnit.Framework.Assert.That(GetProvider().Country, Is.EqualTo("AE"), "Country is set");

	[ExpectNoExceptions]
	public void TestPartyFunctionCode() => NUnit.Framework.Assert.That(GetProvider().PartyFunctionCode, Is.EqualTo("XYZ"));

	[ExpectNoExceptions]
	public void TestPartyIdentifier() => CombineAssertions(() =>
	{
		orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
		orgCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedArabEmirates;
		orgCusCode.OK_CustomsRegNo = "1234";
		NUnit.Framework.Assert.That(GetProvider().PartyIdentifier, Is.Null, "Not a consignee party. Expect PartyIdentifier to be null");

		AssertPartyIdentifier(OrgCusCode.CodeTypes.TaxFileCode, "ABC1234", "ABC1234");
		AssertPartyIdentifier(OrgCusCode.CodeTypes.PassportID, "EFG1234", "EFG1234");
		AssertPartyIdentifier(OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber, "XYZ1234", "XYZ1234");
		AssertPartyIdentifier(OrgCusCode.CodeTypes.CorporationCode, "UVW1234", "UVW1234");
		AssertPartyIdentifier(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, "XYZ1234", null);
		return;

		void AssertPartyIdentifier(string codeType, string regNo, string expectedPartyIdentifier)
		{
			orgCusCode.OK_CodeType = codeType;
			orgCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedArabEmirates;
			orgCusCode.OK_CustomsRegNo = regNo;
			var provider = new PartyFromOrgAddressProvider(orgAddress, PartyFunctionCodeQualifierList.Consignee);
			NUnit.Framework.Assert.That(provider.PartyIdentifier, Is.EqualTo(expectedPartyIdentifier),
				$"Consignee party has OrgCusCode of type {codeType}, registration No. {regNo}. Expect PartyIdentifier to be {expectedPartyIdentifier}.");
		}
	});

	[ExpectNoExceptions]
	public void TestCodeListIdentificationCode() => CombineAssertions(() =>
	{
		orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxFileCode;
		orgCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedArabEmirates;
		orgCusCode.OK_CustomsRegNo = "1234";
		NUnit.Framework.Assert.That(GetProvider().CodeListIdentificationCode, Is.Null, "Not a consignee party. Expect CodeListIdentificationCode to be null");

		AssertCodeListIdentificationCode(OrgCusCode.CodeTypes.TaxFileCode, "1");
		AssertCodeListIdentificationCode(OrgCusCode.CodeTypes.PassportID, "2");
		AssertCodeListIdentificationCode(OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber, "2");
		AssertCodeListIdentificationCode(OrgCusCode.CodeTypes.CorporationCode, "1");
		AssertCodeListIdentificationCode(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, null);
		AssertCodeListIdentificationCode(OrgCusCode.CodeTypes.TaxFileCode, null, "", "PartyIdentifier is empty. Expect CodeListIdentificationCode to be null.");
		AssertCodeListIdentificationCode(OrgCusCode.CodeTypes.TaxFileCode, null, null, "PartyIdentifier is null. Expect CodeListIdentificationCode to be null.");
		return;

		void AssertCodeListIdentificationCode(string codeType, string expectedCodeListIdentificationCode, string regNo = "1234", string message = null)
		{
			orgCusCode.OK_CodeType = codeType;
			orgCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedArabEmirates;
			orgCusCode.OK_CustomsRegNo = regNo;
			var provider = new PartyFromOrgAddressProvider(orgAddress, PartyFunctionCodeQualifierList.Consignee);
			NUnit.Framework.Assert.That(provider.CodeListIdentificationCode, Is.EqualTo(expectedCodeListIdentificationCode),
				message ?? $"Consignee party has OrgCusCode of type {codeType}. Expect CodeListIdentificationCode to be {expectedCodeListIdentificationCode}.");
		}
	});

	[ExpectNoExceptions]
	public void TestContactCommunication() => CombineAssertions(() =>
	{
		const string phoneNumber = "+1 23 45 67 890";
		const string email = "abc@xyz.com";
		const string webUrl = "www.abc.com";

		NUnit.Framework.Assert.That(GetProvider().ContactCommunication, Is.Null);

		orgAddress.OA_Phone = phoneNumber;
		orgAddress.OA_Email = email;
		org.MainWebURL.PU_URL = webUrl;

		var contactCommunication = GetProvider().ContactCommunication;
		NUnit.Framework.Assert.That(contactCommunication.CommunicationIdentifier, Is.EqualTo(phoneNumber), "Phone Number used");
		NUnit.Framework.Assert.That(contactCommunication.CommunicationCode, Is.EqualTo(CommunicationMeansTypeCodeList.Telephone.ToString()), "Phone Number code");

		orgAddress.OA_Phone = ZString.Empty;
		contactCommunication = GetProvider().ContactCommunication;
		NUnit.Framework.Assert.That(contactCommunication.CommunicationIdentifier, Is.EqualTo(email), "Phone Number unavailable, email used");
		NUnit.Framework.Assert.That(contactCommunication.CommunicationCode, Is.EqualTo(CommunicationMeansTypeCodeList.ElectronicMail.ToString()), "Email code");

		orgAddress.OA_Email = ZString.Empty;
		contactCommunication = GetProvider().ContactCommunication;
		NUnit.Framework.Assert.That(contactCommunication.CommunicationIdentifier, Is.EqualTo(webUrl), "Email unavailable, Org url used");
		NUnit.Framework.Assert.That(contactCommunication.CommunicationCode, Is.EqualTo(CommunicationMeansTypeCodeList.UniformResourceLocationUrl.ToString()), "Web url code");
	});

	protected override PartyFromOrgAddressProvider GetProvider() => new PartyFromOrgAddressProvider(orgAddress, "XYZ");

	protected override void SetUp()
	{
		base.SetUp();
		org = Factory.New<OrgHeader>();
		orgAddress = org.Addresses.AddNew();
		orgCusCode = org.CustomsCodes.AddNew();
	}
	OrgHeader org;
	OrgAddress orgAddress;
	OrgCusCode orgCusCode;
}
