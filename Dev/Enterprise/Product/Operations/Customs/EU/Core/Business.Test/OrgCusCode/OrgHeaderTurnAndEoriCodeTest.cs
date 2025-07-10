using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class OrgHeaderTurnAndEoriCodeTest : TestCaseWithFactory
	{
		public void TestGetEuIdentificationNumberForFrance()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "AB123456";
			AssertEquals("FRAB123456", EuEoriProviderAndValidator.GetEuIdentificationNumber(orgHeader));

			cusCode.OK_CustomsRegNo = "OCCASIONNEL";
			AssertEquals("OCCASIONNEL", EuEoriProviderAndValidator.GetEuIdentificationNumber(orgHeader));
		}

		public void TestGetEuIdentificationNumberOfThisAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "XI12345678";
			OrgAddress address = null;
			AssertEquals("NULL Address", ZString.Empty, address.GetEuIdentificationNumber());
			AssertEquals("XI12345678", orgHeader.MainAddress.GetEuIdentificationNumber());
		}

		public void TestGetRegoCodeOfThisOrg()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.AgentCode;
			cusCode.OK_CustomsRegNo = "GB12345678";
			AssertEquals("GB12345678", orgHeader.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.AgentCode));

			cusCode.OK_CustomsRegNo = "AU12345678";
			AssertEquals("GBAU12345678", orgHeader.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.AgentCode));

			cusCode.OK_CustomsRegNo = "XI12345678";
			AssertEquals("XI12345678", orgHeader.GetRegoCodeOfThisOrg(OrgCusCode.CodeTypes.AgentCode));

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "XI12345678";
			AssertEquals("XI12345678", orgHeader.GetEuIdentificationNumber());
		}

		public void TestGetEUVATCodeOfThisOrg()
		{
			var orgHeader = Factory.New<OrgHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("No cusCodes", ZString.Empty, orgHeader.GetEUVATCodeOfThisOrg());

				orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "tva1", Core.Constants.CountryCodes.Italy);
				AssertEquals("Mismatch codeType and country", ZString.Empty, orgHeader.GetEUVATCodeOfThisOrg());

				var vatCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "tva2", Core.Constants.CountryCodes.France);
				AssertEquals("Correct codeType and country", "FRTVA2", orgHeader.GetEUVATCodeOfThisOrg());

				vatCode.OK_CustomsRegNo = "frtva3";
				AssertEquals("RegNo already contains country prefix", "FRTVA3", orgHeader.GetEUVATCodeOfThisOrg());

				vatCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vatCode.OK_CustomsRegNo = "xitva4";
				AssertEquals("NorthernIreland_ForUseOnlyByEuInCertainScopes", "XITVA4", orgHeader.GetEUVATCodeOfThisOrg());
			});
		}

		public void TestTraderIdRetrievals()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("", org.GetEuIdentificationNumber());
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			AssertEquals("", org.GetEuIdentificationNumber());
			cusCode.OK_CustomsRegNo = "PR";
			AssertEquals("GBPR", org.GetEuIdentificationNumber());
			cusCode.OK_CustomsRegNo = "XX";
			AssertNoExceptionThrown(delegate
			{ org.GetEuIdentificationNumber(); });
		}

		public void TestGetEuIdentificationNumberIsPaddedTo12CharsWhenRealTurnCodeIsNullAndWeHaveToMakeItFromTheCountryCodeAndTheVatCode()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			RefCountry countryGood = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);

			organisation = Factory.New<OrgHeader>();
			RefCountry countryMadeUp = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "ER");  // Eritrea = fake country
			OrgCusCode cusCodeBad = organisation.CustomsCodes.AddNew();
			cusCodeBad.OK_RN_NKCodeCountry = countryMadeUp.Code;
			cusCodeBad.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCodeBad.OK_CustomsRegNo = "123456789";
			string paddedTraderId = EuEoriProviderAndValidator.GetEuIdentificationNumber(organisation);
			AssertEquals("VAT on a non EU country should not make up EU Identification", "", paddedTraderId);

			OrgCusCode cusCodeGood = organisation.CustomsCodes.AddNew();
			cusCodeGood.OK_RN_NKCodeCountry = countryGood.Code;
			cusCodeGood.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCodeGood.OK_CustomsRegNo = "123456789";
			paddedTraderId = EuEoriProviderAndValidator.GetEuIdentificationNumber(organisation);
			AssertEquals("GB123456789000", paddedTraderId);
		}

		public void TestTraderIdCodeFallsBackToTraderIdCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("", org.GetEuIdentificationNumber());
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "PR";
			AssertEquals("Precondition: org.GetEuIdentificationNumber(country)", "GBPR", org.GetEuIdentificationNumber());

			OrgCusCode cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = country.Code;
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode2.OK_CustomsRegNo = "OVERRIDE";
			AssertEquals("org.GetEuIdentificationNumber(country)", "GBOVERRIDE", org.GetEuIdentificationNumber());
		}

		public void TestTraderIdVersusTurnCode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("", org.GetEuIdentificationNumber());
			AssertEquals("", org.GetEuIdentificationNumber());

			// Check TraderId and Turn are not thw same:
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "987654321000";
			AssertEquals("GB987654321000", org.GetEuIdentificationNumber());

			// Check no padding for 9-char TRN
			org.CustomsCodes.Remove(cusCode);
			cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "987654321";
			AssertEquals("GB987654321", org.GetEuIdentificationNumber());

			// Check we do not have GBGB as the prefix:
			org.CustomsCodes.Remove(cusCode);
			cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "123456789000";
			AssertEquals("GB123456789000", org.GetEuIdentificationNumber());

			// Check verbatim turn codes returned
			org.CustomsCodes.Remove(cusCode);
			cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "FOO";
			AssertEquals("GBFOO", org.GetEuIdentificationNumber());

			// Check VAT is used when we have no TRN
			org.CustomsCodes.Remove(cusCode);
			cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "888888888";
			AssertEquals("GB888888888000", org.GetEuIdentificationNumber());

			// Check TRN takes priority over VAT when we have both:
			OrgCusCode cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = country.Code;
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			cusCode2.OK_CustomsRegNo = "111111111000";
			AssertEquals("GB111111111000", org.GetEuIdentificationNumber());
		}

		public void TestEORINumberForOtherThanGBIncludingNonEu()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;
			cusCode.OK_CodeType = "VAT";
			cusCode.OK_CustomsRegNo = "77753359";

			CombineAssertions(() =>
			{
				AssertEquals("Non EU Countries with VAT should not be mistaken for an EORI Number", "", org.GetEuIdentificationNumber());

				(string countryCode, string vatCode)[] countryVatCodes =
				{
					(Core.Constants.CountryCodes.France, OrgCusCode.FranceCodeTypes.TVA),
					(Core.Constants.CountryCodes.Cyprus, OrgCusCode.CodeTypes.VATCode),
					(Core.Constants.CountryCodes.Spain, OrgCusCode.SpainCodeTypes.NIF)
				};
				foreach (var (countryCode, vatCode) in countryVatCodes)
				{
					org.CustomsCodes.RemoveAndDeleteAll();
					org.CustomsCodes.AddNew(vatCode, "74913359", countryCode);
					AssertEquals($"{countryCode} {vatCode}, Should now find the EU Country VAT code, but not manipulate it", countryCode + "74913359", org.GetEuIdentificationNumber());
				}
			});
		}

		public void TestTcuin()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			cusCode.OK_CustomsRegNo = "123456789012345";
			AssertEquals("US123456789012345", org.GetEuIdentificationNumber());
			var cusCode2 = org.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			cusCode2.OK_CustomsRegNo = "ABCDEFGHIJKLMNO";
			org.OH_RL_NKClosestPort = "JPTYO";
			AssertEquals("JPABCDEFGHIJKLMNO", org.GetEuIdentificationNumber());
			org.OH_RL_NKClosestPort = "USATL";
			AssertEquals("US123456789012345", org.GetEuIdentificationNumber());
		}

		public void TestEORITrumpsVatAndTurn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "74913359", Core.Constants.CountryCodes.France);
			CombineAssertions(() =>
			{
				AssertEquals("Has VAT, no EORI", "FR74913359", org.GetEuIdentificationNumber());
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.France);
				AssertEquals("Has EORI", "FR123456", org.GetEuIdentificationNumber());
			});
		}
		public void TestEORIComponents()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.France);
			AssertEquals("EORI CountryCode", "FR", org.GetEuIdentificationNumberComponents().CountryCode);
			AssertEquals("EORI Registration Number", "123456", org.GetEuIdentificationNumberComponents().RegistrationNumber);
		}

		public void TestEoriVerusOldTurn()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			RefCountry country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("", org.GetEuIdentificationNumber());

			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "987654321000";
			AssertEquals("GB987654321000", org.GetEuIdentificationNumber());

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "987654321001";
			AssertEquals("CusCode of type EORI means we pull out the eori code verbatim", "GB987654321001", org.GetEuIdentificationNumber());

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			cusCode.OK_CustomsRegNo = "987654321001";
			AssertEquals("Org with non-blank turn suffix should return the blank suffix", "GB987654321000", org.GetEuIdentificationNumber());
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Turn;
			cusCode.OK_CustomsRegNo = "987654321000";
			AssertEquals("Org with blank turn suffix should return the blank suffix", "GB987654321000", org.GetEuIdentificationNumber());
		}

		public void TestGetUS0500ForTemporaryAdmissionEtc()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var ok = org.CustomsCodes.AddNew();
			ok.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			ok.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
			ok.OK_CustomsRegNo = "US0500";
			AssertEquals("US0500", org.GetEuIdentificationNumber());
			ok.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("", org.GetEuIdentificationNumber());
		}

		public void TestGetEuIdentificationNumber_OnPremisAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = orgHeader.Addresses.AddNew();
			var address2 = orgHeader.Addresses.AddNew();
			var cusCode1 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI12345678", Core.Constants.CountryCodes.UnitedKingdom);
			cusCode1.OK_OA_PremisesAddress = address1.PK;
			var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.UnitedKingdom);
			CombineAssertions(() =>
			{
				AssertEquals("OrgHeader: EORI without Premise Address", "GB12345678", orgHeader.GetEuIdentificationNumber());
				AssertEquals("OrgAddress: EORI linked to Premise Address", "XI12345678", address1.GetEuIdentificationNumber());
				AssertEquals("OrgAddress: EORI without Premise Address fallback", "GB12345678", address2.GetEuIdentificationNumber());
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
	}
}
