using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	class FrOrgCusCodeHelperTest : TestCaseWithFactory
	{
		public void TestGetFrIdentificationNumberOnOrganization()
		{
			var organisationHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("FR Identification Number should be empty when no TCU, TRN, VAT or EOR is available.", ZString.Empty, organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "00000000000001", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("FR Identification Number (even if strictly country parametered) should equal foreign TCU number when no EORI is available.", "US00000000000001", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, false));
			AssertEquals("FR Identification Number (not strictly country parametered) should equal foreign TCU number when no EORI is available.", "US00000000000001", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000002", Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("FR Identification Number (strictly country parametered) should fallback to TCU number because existing EOR doesn't strictly match specified country.", "US00000000000001", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, false));
			AssertEquals("FR Identification Number (not strictly country parametered) should fallback to foreign EORI number when no FR EORI is available.", "GB00000000000002", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			var frEori = organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000003", Core.Constants.CountryCodes.France);
			AssertEquals("FR Identification Number should equal GB EORI number when GB is specified as preferable country.", "GB00000000000002", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom, true));
			AssertEquals("FR Identification Number should equal FR EORI number when FR is specified as preferable country.", "FR00000000000003", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = "000000000";
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.France);
			AssertEquals("FR Identification Number should equal FR EORI number + EoriBranchSuffix", "FR00000000012345", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = FRConstants.VATRegistrationNumberValue.OCCASIONNEL;
			AssertEquals("FR Identification Number should equal 'OCCASIONNEL' without suffix.", "OCCASIONNEL", organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));
		}

		public void TestGetFrIdentificationNumberOnAddress()
		{
			var organisationHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisationHeader.Addresses.AddNew();

			AssertEquals("FR Identification Number should be empty when no TCU, TRN, VAT or EOR is available.", ZString.Empty, organisationHeader.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "00000000000001", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("FR Identification Number (even if strictly country parametered) should equal organisation foreign TCU number when no organisation EORI is available.", "US00000000000001", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, false));
			AssertEquals("FR Identification Number (not strictly country parametered) should equal organisation foreign TCU number when no organisation EORI is available.", "US00000000000001", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000002", Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("FR Identification Number (strictly country parametered) should fallback to organisation TCU number because existing EOR doesn't strictly match specified country.", "US00000000000001", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, false));
			AssertEquals("FR Identification Number (not strictly country parametered) should fallback to organisation foreign  EORI number when no organisation FR EORI is available.", "GB00000000000002", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			var frEori = organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000003", Core.Constants.CountryCodes.France);
			AssertEquals("FR Identification Number should equal organisation GB EORI number when GB is specified as preferable country.", "GB00000000000002", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.UnitedKingdom, true));
			AssertEquals("FR Identification Number should equal organisation FR EORI number when FR is specified as preferable country.", "FR00000000000003", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = "000000000";
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.France);
			AssertEquals("FR Identification Number should equal organisation FR EORI number + organisation EoriBranchSuffix when no EoriBranchSuffix has been set for address.", "FR00000000012345", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			var eoriSuffixOnAddress = organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00000", Core.Constants.CountryCodes.France);
			eoriSuffixOnAddress.OK_OA_PremisesAddress = address.PK;
			AssertEquals("FR Identification Number should equal organisation FR EORI number + address EoriBranchSuffix when available.", "FR00000000000000", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = FRConstants.VATRegistrationNumberValue.OCCASIONNEL;
			AssertEquals("FR Identification Number should equal 'OCCASIONNEL' without suffix.", "OCCASIONNEL", address.GetEuIdentificationNumber(Core.Constants.CountryCodes.France, true));
		}

		public void TestGetFrEoriOnlyIdentificationNumberOnOrganisation()
		{
			var organisationHeader = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("FR EORI only Identification Number should be empty when no EOR is available.", ZString.Empty, organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "00000000000001", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("FR EORI only Identification Number should be empty when no EOR is available.", ZString.Empty, organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000002", Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("EORI Number (strictly country parametered) should be empty when no EORI matching country is available.", ZString.Empty, organisationHeader.GetEORI(Core.Constants.CountryCodes.France, false));
			AssertEquals("EORI Number should equal foreign EORI number when no FR EORI is available.", "GB00000000000002", organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			var frEori = organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000003", Core.Constants.CountryCodes.France);
			AssertEquals("EORI Number should equal GB EORI number when GB is specified as preferable country.", "GB00000000000002", organisationHeader.GetEORI(Core.Constants.CountryCodes.UnitedKingdom, true));
			AssertEquals("EORI Number should equal FR EORI number when GB is specified as preferable country.", "FR00000000000003", organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = "000000000";
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.France);
			AssertEquals("EORI Number should equal FR EORI number + EoriBranchSuffix.", "FR00000000012345", organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = FRConstants.VATRegistrationNumberValue.OCCASIONNEL;
			AssertEquals("FR Identification Number should equal 'OCCASIONNEL' without suffix.", "OCCASIONNEL", organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));
		}

		public void TestGetFrEoriOnlyIdentificationNumberOnAddress()
		{
			var organisationHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisationHeader.Addresses.AddNew();

			AssertEquals("FR EORI only Identification Number should be empty when no EOR is available.", ZString.Empty, organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "00000000000001", Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("FR EORI only Identification Number should be empty when no EOR is available.", ZString.Empty, organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));

			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000002", Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("FR Identification Number (strictly country parametered) should be empty when no EORI matching country is available.", ZString.Empty, address.GetEORI(Core.Constants.CountryCodes.France, false));
			AssertEquals("FR Identification Number (not strictly country parametered) should fallback to organisation foreign  EORI number when no organisation FR EORI is available.", "GB00000000000002", address.GetEORI(Core.Constants.CountryCodes.France, true));

			var frEori = organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "00000000000003", Core.Constants.CountryCodes.France);
			AssertEquals("FR Identification Number should equal organisation GB EORI number when GB is specified as preferable country.", "GB00000000000002", address.GetEORI(Core.Constants.CountryCodes.UnitedKingdom, true));
			AssertEquals("FR Identification Number should equal organisation FR EORI number when FR is specified as preferable country.", "FR00000000000003", address.GetEORI(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = "000000000";
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.France);
			AssertEquals("FR Identification Number should equal organisation FR EORI number + organisation EoriBranchSuffix when no EoriBranchSuffix has been set for address.", "FR00000000012345", address.GetEORI(Core.Constants.CountryCodes.France, true));

			var eoriSuffixOnAddress = organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00000", Core.Constants.CountryCodes.France);
			eoriSuffixOnAddress.OK_OA_PremisesAddress = address.PK;
			AssertEquals("FR Identification Number should equal organisation FR EORI number + address EoriBranchSuffix when available.", "FR00000000000000", address.GetEORI(Core.Constants.CountryCodes.France, true));

			frEori.OK_CustomsRegNo = FRConstants.VATRegistrationNumberValue.OCCASIONNEL;
			AssertEquals("FR Identification Number should equal 'OCCASIONNEL' without suffix.", "OCCASIONNEL", address.GetEORI(Core.Constants.CountryCodes.France, true));
		}

		public void TestGetUnprefixedEORIOnOrganisation()
		{
			var organisationHeader = Factory.NewWithValidTestData<OrgHeader>();
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "000000000", Core.Constants.CountryCodes.France);
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.France);
			AssertEquals("Prerequisite: FR Identification Number should equal organisation FR EORI number + address EoriBranchSuffix when available.", "FR00000000012345", organisationHeader.GetEORI(Core.Constants.CountryCodes.France, true));
			AssertEquals("UnprefixedEori should not feature the country information", "00000000012345", organisationHeader.GetUnprefixedEORI());
		}

		public void TestGetUnprefixedEORIOnAddress()
		{
			var organisationHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisationHeader.Addresses.AddNew();
			organisationHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "000000000", Core.Constants.CountryCodes.France);
			var eoriSuffixOnAddress = organisationHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "12345", Core.Constants.CountryCodes.France);
			eoriSuffixOnAddress.OK_OA_PremisesAddress = address.PK;
			AssertEquals("Prerequisite: FR Identification Number should equal organisation FR EORI number + address EoriBranchSuffix when available.", "FR00000000012345", address.GetEORI(Core.Constants.CountryCodes.France, true));
			AssertEquals("UnprefixedEori should not feature the country information", "00000000012345", address.GetUnprefixedEORI());
		}
	}
}
