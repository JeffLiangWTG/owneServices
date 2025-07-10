using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing
{
	class EuEoriProviderAndValidatorTest : TestCaseWithFactory
	{
		public void TestGetRegoCodeOfThisOrg_OldestFirst()
		{
			var org = Factory.New<OrgHeader>();
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "001", "ES");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "002", "DE");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Spanish Business Organization has NIF Registration Number", "DE002", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, OrgCusCode.SpainCodeTypes.NIF));
		}

		public void TestOrgHeader_GetEuIdentificationNumber_IgnoreCountryOfIssuanceIfNotMatched()
		{
			var org = Factory.New<OrgHeader>();
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "ES");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "DE");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Should not match", ZString.Empty, org.GetEuIdentificationNumber("KD"));
			AssertEquals("Should match to oldest", "DE002", org.GetEuIdentificationNumber("KD", true));
			AssertEquals("Should match to ES", "ES001", org.GetEuIdentificationNumber("ES", true));
			AssertEquals("Should match to DE", "DE002", org.GetEuIdentificationNumber("DE", true));
		}

		public void TestJobDocAddress_GetEuIdentificationNumber_IgnoreCountryOfIssuanceIfNotMatched()
		{
			var org = Factory.New<OrgHeader>();
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "ES");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "DE");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_GovRegNum = "JK004";
			AssertEquals("Should match docAddress.E2_GovRegNum", "JK004", docAddress.GetEuIdentificationNumber());
			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("Should match to oldest", "DE002", docAddress.GetEuIdentificationNumber());
			org.OH_RL_NKClosestPort = "ES";
			AssertEquals("Should match to ES", "ES001", docAddress.GetEuIdentificationNumber());
			org.OH_RL_NKClosestPort = "DE";
			AssertEquals("Should match to DE", "DE002", docAddress.GetEuIdentificationNumber());
			org.OH_RL_NKClosestPort = "IE";
			AssertEquals("Should match to oldest", "DE002", docAddress.GetEuIdentificationNumber());
		}

		public void TestJobDocAddress_GetEuIdentificationNumber_EORIWithCountryOfIssuance()
		{
			var org = Factory.New<OrgHeader>();
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "GB");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "XI");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
			var code3 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "003", "ES");
			code3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);
			var code4 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "004", "DE");
			code4.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 07);
			var code5 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "005", "AU");
			code5.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 06);
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = ZBool.True;
			docAddress.E2_GovRegNum = "AA123";
			AssertEquals("Should match docAddress.E2_GovRegNum even with GB EORI", "AA123", docAddress.GetEuIdentificationNumber("GB"));
			org.MainAddress.OA_RN_NKCountryCode = "NL";
			docAddress.E2_OA_Address = org.MainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("Should match to oldest", "AU005", docAddress.GetEuIdentificationNumber("XX", true));
				AssertEquals("Should match to GB", "GB001", docAddress.GetEuIdentificationNumber("GB"));
				AssertEquals("Should match to XI", "XI002", docAddress.GetEuIdentificationNumber("XI"));
				AssertEquals("Should match to ES", "ES003", docAddress.GetEuIdentificationNumber("ES"));
				AssertEquals("Should match to DE", "DE004", docAddress.GetEuIdentificationNumber("EUN"));
			});
		}

		public void TestOrgHeader_GetEuIdentificationNumber_EORIWithEUNCountryOfIssuance()
		{
			var org = Factory.New<OrgHeader>();
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "ES");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "GB");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
			var code3 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "003", "AU");
			code3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);

			CombineAssertions(() =>
			{
				AssertEquals("Should not match", ZString.Empty, org.GetEuIdentificationNumber("KD"));
				AssertEquals("Should not match to oldest", "AU003", org.GetEuIdentificationNumber("KD", true));
				AssertEquals("Should match to the one for the EU country (excluding GB)", "ES001", org.GetEuIdentificationNumber("EUN"));
				AssertEquals("Should match to ES", "ES001", org.GetEuIdentificationNumber("ES"));
				AssertEquals("Should match to GB", "GB002", org.GetEuIdentificationNumber("GB"));

				code1.Delete();
				AssertEquals("Should not match when country of issuance is EUN and there are no EORIs for EU country or GB that starts with XI", ZString.Empty, org.GetEuIdentificationNumber("EUN"));

				var code4 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI004", "GB");
				code4.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("Should match to GB stating with XI when country of issuance is EUN and there are no EORIs for EU country but there is one GB that starts with XI", "XI004", org.GetEuIdentificationNumber("EUN"));
			});
		}

		public void TestOrgAddress_GetEuIdentificationNumber_EORIWithEUNCountryOfIssuance()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			var code1 = address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "ES");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "GB");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);
			var code3 = address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "003", "AU");
			code3.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 08);

			CombineAssertions(() =>
			{
				AssertEquals("Should not match", ZString.Empty, address.GetEuIdentificationNumber("KD"));
				AssertEquals("Should not match to oldest", "AU003", address.GetEuIdentificationNumber("KD", true));
				AssertEquals("Should match to the one for the EU country (excluding GB)", "ES001", address.GetEuIdentificationNumber("EUN"));
				AssertEquals("Should match to ES", "ES001", address.GetEuIdentificationNumber("ES"));
				AssertEquals("Should match to GB", "GB002", address.GetEuIdentificationNumber("GB"));

				code1.Delete();
				AssertEquals("Should not match when country of issuance is EUN and there are no EORIs for EU country or GB that starts with XI", ZString.Empty, address.GetEuIdentificationNumber("EUN"));

				var code4 = address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI004", "GB");
				code4.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
				AssertEquals("Should match to GB stating with XI when country of issuance is EUN and there are no EORIs for EU country but there is one GB that starts with XI", "XI004", address.GetEuIdentificationNumber("EUN"));
			});
		}

		public void TestGetRegoCodeOfThisOrg_IgnoreCountryOfIssuanceIfNotMatched()
		{
			var org = Factory.New<OrgHeader>();
			var code1 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "ES");
			code1.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 10);
			var code2 = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "002", "DE");
			code2.OK_SystemCreateTimeUtc = new ZDateTime(2022, 11, 09);

			AssertEquals("Should not match", ZString.Empty, org.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "KD"));
			AssertEquals("Should match to oldest", "DE002", org.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "KD", true));
			AssertEquals("Should match to ES", "ES001", org.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES", true));
			AssertEquals("Should match to DE", "DE002", org.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DE", true));
		}

		public void TestGetRegoCodeOfThisOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "001", "ES");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "002", "DE");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("RC3", "003", "ES");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("RC4", "FR004", "FR");
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("RC5", "XI005", "GB");

			CombineAssertions(() =>
			{
				org.OH_Category = OrgConstants.Category.Business;
				AssertEquals("Spanish Business Organization has NIF Registration Number", "ES001", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "NIF", "ES"));
				AssertEquals("Non Spanish Business Organization has NIF Registration Number", "DE002", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "NIF", "DE"));
				AssertEquals("Spanish Business Organization has Non-NIF Registration Number", "ES003", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "RC3"));
				AssertEquals("Business Organization already has matching Country Code in Registration Number", "FR004", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "RC4"));
				AssertEquals("Business Organization with GB Registration Number starting with Nothern Ireland prefix", "XI005", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "RC5"));

				org.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Spanish Natural Person has NIF Registration Number", "001", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "NIF", "ES"));
				AssertEquals("Non Spanish Natural Person has NIF Registration Number", "DE002", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "NIF", "DE"));
				AssertEquals("Spanish Natural Person has Non-NIF Registration Number", "ES003", EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(org, "RC3"));
			});
		}

		public void TestGetRegoCodeOfThisAddress()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "001", "ES");
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "002", "DE");
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("RC3", "003", "ES");
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("RC4", "FR004", "FR");
			address.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("RC5", "XI005", "GB");

			CombineAssertions(() =>
			{
				org.OH_Category = OrgConstants.Category.Business;
				AssertEquals("Spanish Business Organization has NIF Registration Number", "ES001", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "NIF", "ES"));
				AssertEquals("Non Spanish Business Organization has NIF Registration Number", "DE002", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "NIF", "DE"));
				AssertEquals("Spanish Business Organization has Non-NIF Registration Number", "ES003", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "RC3"));
				AssertEquals("Business Organization already has matching Country Code in Registration Number", "FR004", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "RC4"));
				AssertEquals("Business Organization with GB Registration Number starting with Nothern Ireland prefix", "XI005", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "RC5"));

				org.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Spanish Natural Person has NIF Registration Number", "001", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "NIF", "ES"));
				AssertEquals("Non Spanish Natural Person has NIF Registration Number", "DE002", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "NIF", "DE"));
				AssertEquals("Spanish Natural Person has Non-NIF Registration Number", "ES003", EuEoriProviderAndValidator.GetRegoCodeOfThisAddress(address, "RC3"));
			});
		}

		public void TestGetEUVATCodeOfThisOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "001", "ES");

			CombineAssertions(() =>
			{
				org.OH_Category = OrgConstants.Category.Business;
				AssertEquals("Spanish Business Organization has NIF Registration Number", "ES001", EuEoriProviderAndValidator.GetEUVATCodeOfThisOrg(org));
				org.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Spanish Natural Person has NIF Registration Number", "001", EuEoriProviderAndValidator.GetEUVATCodeOfThisOrg(org));
			});
		}

		public void TestGetEUVATCodesOfThisOrg()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Category = OrgConstants.Category.Business;
			AssertEquals("No codes Returned", 0, EuEoriProviderAndValidator.GetEUVATCodesOfThisOrg(org).Count());

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(TaxTypeGenericList.Codes.Vat, "001", "GB");
			AssertEquals("GB code Returned", "GB001", EuEoriProviderAndValidator.GetEUVATCodesOfThisOrg(org).ElementAt(0));

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(GermanyOrgCusCodeInfo.OrgCusCodes.UST, "002", "DE");
			var vatCodes = EuEoriProviderAndValidator.GetEUVATCodesOfThisOrg(org);
			AssertEquals("Two codes Returned", 2, vatCodes.Count());
			AssertEquals("DE code Returned", "DE002", vatCodes.ElementAt(0));
			AssertEquals("GB code Returned", "GB001", vatCodes.ElementAt(1));

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SpainCodeTypes.NIF, "003", "DE");
			AssertNotEquals("NIF Not DE Code", "DE003", EuEoriProviderAndValidator.GetEUVATCodesOfThisOrg(org).ElementAt(0));
		}

		public void TestValidEORIorTCUIFormat()
		{
			var factory = new BusinessObjectFactory();
			var eoriOrTcui = string.Empty;
			AssertEquals("Invalid - empty string", false, EuEoriProviderAndValidator.ValidEORIorTCUIFormat(eoriOrTcui, factory));
			eoriOrTcui = "XXInvalid";
			AssertEquals("Invalid - prefix not a country", false, EuEoriProviderAndValidator.ValidEORIorTCUIFormat(eoriOrTcui, factory));
			eoriOrTcui = "IE1234567891234567";
			AssertEquals("Invalid - reference too long", false, EuEoriProviderAndValidator.ValidEORIorTCUIFormat(eoriOrTcui, factory));
			eoriOrTcui = "IE123456789123456";
			AssertEquals("Valid - EORI", true, EuEoriProviderAndValidator.ValidEORIorTCUIFormat(eoriOrTcui, factory));
			eoriOrTcui = "ZA123456789123456";
			AssertEquals("Valid - TCUI", true, EuEoriProviderAndValidator.ValidEORIorTCUIFormat(eoriOrTcui, factory));
		}
	}
}
