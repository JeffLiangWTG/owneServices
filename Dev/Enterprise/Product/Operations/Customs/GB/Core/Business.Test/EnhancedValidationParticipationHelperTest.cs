using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.EnhancedValidationParticipationHelper;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(EnhancedValidationParticipationHelper))]
	sealed class EnhancedValidationParticipationHelperTest : TestCaseWithFactory
	{
		public void TestCheckEORITreatment()
		{
			var testData = EnhancedValidationParticipationHelperTestData.GetEORIData();
			foreach (var (eori, treatment) in testData)
			{
				AssertCheckEORITreatment(eori, treatment);
			}
		}

		public void TestCheckLRNTreatment_WhenWarningFactorIsTwo()
		{
			var warningFactor = WarningFactor.Two;
			foreach (var (lrn, treatment) in EnhancedValidationParticipationHelperTestData.GetLRNData(warningFactor))
			{
				AssertCheckLRNTreatment(lrn, warningFactor, treatment);
			}
		}

		public void TestCheckLRNTreatment_WhenWarningFactorIsSixteen()
		{
			var warningFactor = WarningFactor.Sixteen;
			foreach (var (lrn, treatment) in EnhancedValidationParticipationHelperTestData.GetLRNData(warningFactor))
			{
				AssertCheckLRNTreatment(lrn, warningFactor, treatment);
			}
		}

		public void TestGetWarningFactor()
		{
			AssertNull("When FUNCS isn't set", EnhancedValidationParticipationHelper.GetWarningFactor());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, false))
			{
				AssertNull("When FUNCS is off", EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			{
				AssertNull("When FUNCS is on, Attribute isn't set", EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today.AddDays(3), Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "2.0"))
			{
				AssertNull("When FUNCS is on, Attribute is off", EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "1.0"))
			{
				AssertNull("When FUNCS is on, Attribute is 1.0", EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "2.0"))
			using (GBCustomsDataRegistry.Instance.DigitalPromptsTrialParticipateInNudgeTrial.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertNull("When FUNCS is on, Attribute is 2.0, Registry is disabled", EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "2.0"))
			{
				AssertEquals("When FUNCS is on, Attribute is 2.0", WarningFactor.Two, EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "4.0"))
			{
				AssertEquals("When FUNCS is on, Attribute is 4.0", WarningFactor.Four, EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "8.0"))
			{
				AssertEquals("When FUNCS is on, Attribute is 8.0", WarningFactor.Eight, EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "16.0"))
			{
				AssertEquals("When FUNCS is on, Attribute is 16.0", WarningFactor.Sixteen, EnhancedValidationParticipationHelper.GetWarningFactor());
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, "16"))
			{
				AssertEquals("When FUNCS is on, Attribute is 16", WarningFactor.Sixteen, EnhancedValidationParticipationHelper.GetWarningFactor());
			}
		}

		public void TestEnhancedValidationParticipation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedKingdom);
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = country.Code;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			declaration.Branch.GB_OH_OrgProxy = org.PK;

			foreach (var eoriGroup in EnhancedValidationParticipationHelperTestData.GetData().GroupBy(x => x.EORI))
			{
				var eori = eoriGroup.Key;
				cusCode.OK_CustomsRegNo = eori.Substring(2);

				declaration.CustomsEntryHeaders.Clear();
				eoriGroup.ForEach(x =>
				{
					var entry = declaration.CustomsEntryHeaders.AddNew();
					entry.LRN = x.LRN;
				});

				AssertEquals($"Pre-Condition EORI: {eori}", eori, declaration.GetEori());

				var expectedFactorTwoNudges = eoriGroup.Where(x => x.ShowNudgeFactorTwoTreatment).Select(x => x.LRN).ToArray();
				AssertEnhancedValidationParticipation(declaration, WarningFactor.Two, expectedFactorTwoNudges);

				var expectedFactorSixteenNudgesExpected = eoriGroup.Where(x => x.ShowNudgeFactorSixteenTreatment).Select(x => x.LRN).ToArray();
				AssertEnhancedValidationParticipation(declaration, WarningFactor.Sixteen, expectedFactorSixteenNudgesExpected);
			}
		}

		void AssertCheckEORITreatment(string eori, bool expectedTreatment)
		{
			var treatment = EnhancedValidationParticipationHelper.CheckEORITreatment(Factory, eori);
			AssertEquals($"EORI: {eori}", expectedTreatment, treatment);
		}

		void AssertCheckLRNTreatment(string lrn, WarningFactor warningFactor, bool expectedTreatment)
		{
			var treatment = EnhancedValidationParticipationHelper.CheckLRNTreatment(Factory, lrn, warningFactor);
			AssertEquals($"LRN: {lrn}, Warning Factor: {warningFactor}", expectedTreatment, treatment);
		}

		void AssertEnhancedValidationParticipation(JobDeclaration declaration, WarningFactor warningFactor, string[] expectedNudges)
		{
			var attributeValue = warningFactor == WarningFactor.Two ? "2.0" : "16.0";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionalityAttribute(Constants.FunctionalityTypes.HmrcDigitalPrompts, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.WarningFactor, attributeValue))
			{
				var entries = EnhancedValidationParticipationHelper.EnhancedValidationParticipation(declaration, declaration.CustomsEntryHeaders).Select(x => x.LRN).ToArray();
				AssertContainsExactElementsInExactOrder($"EORI: {declaration.GetEori()}, WarningFactor: {attributeValue}", expectedNudges, entries);
			}
		}
	}
}
