using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ConsumptionTaxRegistrationCodesList))]
	sealed class ConsumptionTaxRegistrationCodesListTest : ValueProviderWithLoadControlFactoryTest<ConsumptionTaxRegistrationCodesList>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("Should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("Should not match <ConsumptionTaxRegistrationCodesList>", !ValueProviderToTest.IsResponsibleForReplacing("<ConsumptionTaxRegistrationCodesList>", Passes.SecondPass));
			Assert("Should not match <ConsumptionTaxRegistrationCodesList()>", !ValueProviderToTest.IsResponsibleForReplacing("<ConsumptionTaxRegistrationCodesList()>", Passes.SecondPass));
			Assert("Should not match < ConsumptionTaxRegistrationCodesList ( ) >", !ValueProviderToTest.IsResponsibleForReplacing("<ConsumptionTaxRegistrationCodesList()>", Passes.SecondPass));
			Assert("Should match <ConsumptionTaxRegistrationCodesList(AALSHI)>", ValueProviderToTest.IsResponsibleForReplacing("<ConsumptionTaxRegistrationCodesList(AALSHI)>", Passes.SecondPass));
			Assert("Should match < ConsumptionTaxRegistrationCodesList ( AALSHI ) >", ValueProviderToTest.IsResponsibleForReplacing("<ConsumptionTaxRegistrationCodesList(AALSHI)>", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			var factory = ((ValueProviderWithLoadControlFactory)ValueProviderToTest).FactoryForTesting;
			var header = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "SGGST", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Singapore));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "SGVAT", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Singapore));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "AUGST", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Australia));
			header.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "AUABN", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Australia));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CompanyNumber, "UKCNO", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.UnitedKingdom));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "DEVAT", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Germany));
			header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister, "DEHRB", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Germany));
			header.CustomsCodes.AddNew("UST", "DEUST", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Germany));

			AssertEquals("SG SGGST, AU AUABN, DE DEUST", ValueProviderToTest.GetReplacement("<ConsumptionTaxRegistrationCodesList(AALSHI)>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OrgCode", "AALSHI"));
			var factory = ((ValueProviderWithLoadControlFactory)ValueProviderToTest).FactoryForTesting;
			var header = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "SGGST", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Singapore));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "SGVAT", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Singapore));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GSTCode, "AUGST", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Australia));
			header.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "AUABN", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Australia));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CompanyNumber, "UKCNO", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.UnitedKingdom));
			header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "DEVAT", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Germany));
			header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.Handelsregister, "DEHRB", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Germany));
			header.CustomsCodes.AddNew("UST", "DEUST", RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Germany));
			factory.Save();
		}
	}
}
