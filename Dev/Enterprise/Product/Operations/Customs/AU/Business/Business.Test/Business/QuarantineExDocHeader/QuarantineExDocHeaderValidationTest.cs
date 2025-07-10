using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQH_QuotaType()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_HOR, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				eXDOCHeader.QH_QuotaType = "AAAAA";
				AssertNoMessageErrors(eXDOCHeader.QH_QuotaTypeInfo);

				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				eXDOCHeader.QH_QuotaType = "BBBBB";
				AssertHasMessageError(eXDOCHeader.QH_QuotaTypeInfo, "Quota Type may only be present when the Produce Type is Meat");
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_HOR, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				eXDOCHeader.QH_QuotaType = "AAA";
				AssertNoMessageErrors(eXDOCHeader.QH_QuotaTypeInfo);

				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				eXDOCHeader.QH_QuotaType = "BBB";
				AssertNoMessageErrors(eXDOCHeader.QH_QuotaTypeInfo);
			}
		}

		public void TestCheckQH_ProduceType()
		{
			const string expectedMessageError_RequiredCDD03 = "Declaration code CDD03 is required when produce type is Dairy, Fish or Eggs.";
			AssertNoMessageErrors("Pre-Condition no errors", eXDOCHeader.QH_ProduceTypeInfo);
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			AssertNoMessageErrors("A valid code", eXDOCHeader.QH_ProduceTypeInfo);
			eXDOCHeader.QH_ProduceType = "SCO";
			AssertHasMessageErrors("Invalid code should have message errors", eXDOCHeader.QH_ProduceTypeInfo);
			eXDOCHeader.QH_ProduceType = ZString.Empty;
			AssertHasMessageErrors("Empty code should have message errors", eXDOCHeader.QH_ProduceTypeInfo);

			var supportingInfo = eXDOCHeader.SupportingInfos.AddNew();
			CombineAssertions("Message errors relevant to CDD03 when NEXDOC_Fish enable", () =>
			{
				var validator = eXDOCHeader.Validation;
				foreach (var code in new string[] { EXDOCCommodityCodes.Codes.Dairy, EXDOCCommodityCodes.Codes.Fish, EXDOCCommodityCodes.Codes.Eggs })
				{
					supportingInfo.CSI_Description = "UNK";
					eXDOCHeader.QH_ProduceType = code;
					AssertHasMessageError($"[{code}] Not has a CDD03", eXDOCHeader.QH_ProduceTypeInfo, expectedMessageError_RequiredCDD03);
					supportingInfo.CSI_Description = "CDD03";
					validator.ValidateQH_ProduceType();
					AssertNoMessageError($"[{code}] Has a CDD03", eXDOCHeader.QH_ProduceTypeInfo, expectedMessageError_RequiredCDD03);
				}
			});
		}

		public void TestCheckQH_RL_NKBorderInspectionPort()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_RL_NKBorderInspectionPortInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCHeader.QH_RL_NKBorderInspectionPort = "JPABO";
			Assert("Aboshi, Japan is a valid code", !eXDOCHeader.QH_RL_NKBorderInspectionPortInfo.HasMessageErrors());
			eXDOCHeader.QH_RL_NKBorderInspectionPort = "AUSYD";
			Assert("Sydney, Australia is a valid code just not valid for EXDOCS", eXDOCHeader.QH_RL_NKBorderInspectionPortInfo.HasMessageErrors());
			eXDOCHeader.QH_RL_NKBorderInspectionPort = "ZZZZZ";
			Assert("Invalid code", eXDOCHeader.QH_RL_NKBorderInspectionPortInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				eXDOCHeader.QH_RL_NKBorderInspectionPort = "JPABO";
				AssertHasMessageErrorContaining(eXDOCHeader.QH_RL_NKBorderInspectionPortInfo, "Border Inspection Port must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQH_AbsoluteTemperature()
		{
			eXDOCHeader.Messages.AddNew().EM_ApplicationCode = EDIInterchange.ApplicationCodes.EXDOC;
			Assert("Pre-Condition", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_AbsoluteTemperature = 12.1m;
			Assert("Horticulture cannot have temperature values", eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_AbsoluteTemperature = 12.2m;
			Assert("GrainsAndPlants cannot have temperature values", eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_AbsoluteTemperature = 12.3m;
			Assert("SkinsAndHides cannot have temperature values", eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_AbsoluteTemperature = 12.4m;
			Assert("Wool cannot have temperature values", eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_AbsoluteTemperature = 12.5m;
			Assert("Fish can have temperature values", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_AbsoluteTemperature = 12.6m;
			Assert("Meat can have temperature values", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AbsoluteTemperature = 12.7m;
			Assert("Dairy can have temperature values", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCHeader.QH_AbsoluteTemperature = 12.8m;
			Assert("Eggs can have temperature values", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_AbsoluteTemperature = 12.9m;
			Assert("InedibleMeat can have temperature values", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_MinimumTemperature = 23.5m;
			eXDOCHeader.QH_MaximumTemperature = ZDecimal.Zero;
			Assert("Minimum temperature cannot be set as well", eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_MinimumTemperature = ZDecimal.Zero;
			eXDOCHeader.QH_MaximumTemperature = 26.8m;
			Assert("Maximum temperature cannot be set as well", eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_MinimumTemperature = ZDecimal.Zero;
			eXDOCHeader.QH_MaximumTemperature = ZDecimal.Zero;
			Assert("Valid produce code, no temperature range", !eXDOCHeader.QH_AbsoluteTemperatureInfo.HasMessageErrors());
		}

		public void TestCheckQH_AbsoluteTemperature_NEXDOCFish()
		{
			const string message = "Absolute temperature is not allowed for this produce type.";
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;

			eXDOCHeader.QH_AbsoluteTemperature = 12.1m;
			AssertHasMessageError("NEXDOC enabled, QH_AbsoluteTemperature not 0", eXDOCHeader.QH_AbsoluteTemperatureInfo, message);

			eXDOCHeader.QH_AbsoluteTemperature = 0;
			AssertNoMessageError("NEXDOC enabled, QH_AbsoluteTemperature 0", eXDOCHeader.QH_AbsoluteTemperatureInfo, message);

			eXDOCHeader.Messages.AddNew().EM_ApplicationCode = EDIInterchange.ApplicationCodes.EXDOC;
			eXDOCHeader.QH_AbsoluteTemperature = 12.1m;
			AssertNoMessageError("NEXDOC disabled, QH_AbsoluteTemperature not 0", eXDOCHeader.QH_AbsoluteTemperatureInfo, message);
		}

		public void TestCheckQH_MinimumTemperature()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_MinimumTemperature = 12.1m;
			eXDOCHeader.QH_MaximumTemperature = 26.8m;
			Assert("Horticulture cannot have range temperature values", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_MinimumTemperature = 12.2m;
			Assert("Dairy cannot have range temperature values", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_MinimumTemperature = 12.3m;
			Assert("GrainsAndPlants cannot have range temperature values", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_MinimumTemperature = 12.4m;
			Assert("Meat cannot have range temperature values", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_MinimumTemperature = 12.5m;
			Assert("SkinsAndHides cannot have range temperature values", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_MinimumTemperature = 12.6m;
			Assert("Wool cannot have range temperature values", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_MinimumTemperature = 12.7m;
			Assert("Fish can have range temperature values", !eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCHeader.QH_MinimumTemperature = 12.8m;
			Assert("Eggs can have range temperature values", !eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_MinimumTemperature = 12.9m;
			Assert("InedibleMeat can have range temperature values", !eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_AbsoluteTemperature = 34.6m;
			eXDOCHeader.QH_MaximumTemperature = 0;
			Assert("Absolute temperature cannot be set as well", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_AbsoluteTemperature = ZDecimal.Zero;
			eXDOCHeader.QH_MinimumTemperature = ZDecimal.Zero;
			eXDOCHeader.QH_MaximumTemperature = 26.8m;
			Assert("Minimum temperature is et a 0 maximum temperature is higher", !eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_MinimumTemperature = 34.5m;
			eXDOCHeader.QH_MaximumTemperature = 26.8m;
			Assert("Minimum temperature needs to be less then maximum temperature", eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_MinimumTemperature = 26.8m;
			eXDOCHeader.QH_MaximumTemperature = 34.5m;
			Assert("Minimum less then maximum, produce fish and no absolute", !eXDOCHeader.QH_MinimumTemperatureInfo.HasMessageErrors());
		}

		public void TestCheckQH_MaximumTemperature()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_MaximumTemperature = 12.1m;
			Assert("Horticulture cannot have range temperature values", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_MaximumTemperature = 12.2m;
			Assert("Dairy cannot have range temperature values", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_MaximumTemperature = 12.3m;
			Assert("GrainsAndPlants cannot have range temperature values", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_MaximumTemperature = 12.4m;
			Assert("Meat cannot have range temperature values", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_MaximumTemperature = 12.5m;
			Assert("SkinsAndHides cannot have range temperature values", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_MaximumTemperature = 12.6m;
			Assert("Wool cannot have range temperature values", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_MaximumTemperature = 12.7m;
			Assert("Fish can have range temperature values", !eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCHeader.QH_MaximumTemperature = 12.8m;
			Assert("Eggs can have range temperature values", !eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_MaximumTemperature = 12.9m;
			Assert("InedibleMeat can have range temperature values", !eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_AbsoluteTemperature = 34.6m;
			Assert("Absolute temperature cannot be set as well", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_AbsoluteTemperature = ZDecimal.Zero;
			eXDOCHeader.QH_MaximumTemperature = ZDecimal.Zero;
			eXDOCHeader.QH_MinimumTemperature = 12.5m;
			Assert("Maximum temperature needs to be set if minimum temperature is", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_MinimumTemperature = 34.5m;
			eXDOCHeader.QH_MaximumTemperature = 26.8m;
			Assert("Maximum temperature needs to be greater than minimum temperature", eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
			eXDOCHeader.QH_MinimumTemperature = 26.8m;
			eXDOCHeader.QH_MaximumTemperature = 34.5m;
			Assert("Minimum less then maximum, produce fish and no absolute", !eXDOCHeader.QH_MaximumTemperatureInfo.HasMessageErrors());
		}

		public void TestCheckQH_TemperatureUM()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Assert("Temperature units not valid for horticultre", eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			Assert("Temperature units not valid for GrainsAndPlants", eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Assert("Temperature units not valid for SkinsAndHides", eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			Assert("Temperature units not valid for Wool", eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Assert("Temperature units are valid for InedibleMeat", !eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			Assert("Temperature units are valid for Meat", !eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			Assert("Temperature units are valid for Dairy", !eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			Assert("Temperature units are valid for Eggs", !eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Celsius;
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_TemperatureUM = EXDOCTemperatureUnitCodes.Codes.Fahrenheit;
			Assert("Valid produce type and list value", !eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
			eXDOCHeader.QH_TemperatureUM = "BUL";
			Assert("Invalid temperature unit value", eXDOCHeader.QH_TemperatureUMInfo.HasMessageErrors());
		}

		public void TestCheckQH_AuthorisationEstablishment()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			Assert("Authorisation establishment is mandatory for produce type dairy", eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			Assert("Authorisation establishment is not mandatory for produce type skins and hides", !eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			Assert("Authorisation establishment is not required for produce type wool", !eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_AuthorisationEstablishment = "123";
			Assert("Authorisation establishment is valid", !eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());
		}

		public void TestAuthorisationEstablishmentValidation()
		{
			// Test why above test case which seems to confirm mandatory validation is working... (i.e. without a value & then with a value) is apparently not working functionally.... error message still showing on Grid.
			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
			var expectedErrorMessage = "Authorization Establishment must be entered.";
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			AssertHasMessageError("Authorisation establishment is mandatory for produce type dairy", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);

			eXDOCHeader.QH_AuthorisationEstablishment = "88";
			AssertNoMessageError("Authorisation establishment is now entered - validation should pass", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);

			eXDOCHeader.Validation.ValidateAll();
			AssertNoMessageError("Authorisation establishment is valid - validate all should not be generating a validation message", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedErrorMessage);
		}

		public void TestValidationTakesBothEstablishmentOptionsIntoAccount()
		{
			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
			var expectedErrorMessage = "Authorization Establishment must be entered.";
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			AssertHasMessageError("Authorisation establishment is mandatory for produce type dairy", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);

			eXDOCHeader.QH_AuthorisationEstablishment = "88";
			AssertNoMessageError("Authorisation establishment is now entered - validation should now pass", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);

			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Organisation;
			eXDOCHeader.Validation.ValidateAll();
			AssertNoMessageError("Authorisation establishment option has changed to Organisation - validate all should not be generating a validation message for Code", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedErrorMessage);

			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			eXDOCHeader.QH_OA_AuthorisationEstablishment = address.PK;
			var expectedMessageError = ZString.Format(QuarantineExDocHeaderValidation.ESNIsRequired, "Authorisation");
			AssertHasMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedMessageError);

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			cusCode.OK_CustomsRegNo = "23456";
			cusCode.OK_OA_PremisesAddress = address.PK;

			eXDOCHeader.QH_OA_AuthorisationEstablishment = address.PK;
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedMessageError);
			AssertNoMessageErrors(eXDOCHeader.QH_AuthorisationEstablishmentInfo);
			AssertNoMessageErrors(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo);
		}

		public void TestAuthorisationForAqisPlace()
		{
			eXDOCHeader.Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			eXDOCHeader.QH_AuthorisationLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			var expectedErrorMessage = "Authorization Establishment must be entered.";
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisationEstablishmentInfo.HasMessageErrors());

			eXDOCHeader.Declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			AssertHasMessageError("Authorisation establishment is mandatory for produce type dairy", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);

			eXDOCHeader.QH_AuthorisationEstablishment = "XXX";
			AssertNoMessageError("Authorisation establishment is now entered - mandatory entry validation should pass", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);
			AssertHasMessageError("List validation error should be shown", eXDOCHeader.QH_AuthorisationEstablishmentInfo, ListValidation.InvalidCodeMessageError);

			eXDOCHeader.QH_AuthorisationEstablishment = "SYD";
			AssertNoMessageError("Authorisation establishment is now entered - validation should pass", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);

			eXDOCHeader.Validation.ValidateAll();
			AssertNoMessageError("Authorisation establishment is valid - validate all should not be generating a validation message", eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedErrorMessage);
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedErrorMessage);
		}

		public void TestCheckQH_AuthorisingOfficerID()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisingOfficerIDInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_AuthorisingOfficerID = "1234";
			Assert("Authorising Officer ID cannot be set for produce type dairy", eXDOCHeader.QH_AuthorisingOfficerIDInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AuthorisingOfficerID = "1234A";
			Assert("Correct produce type and officer id", !eXDOCHeader.QH_AuthorisingOfficerIDInfo.HasMessageErrors());
		}

		public void TestCheckQH_PackDate()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_PackDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_PackDate = ZDateTime.Now;
			Assert("Pack date cannot be set for produce type meat", eXDOCHeader.QH_PackDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_PackDate = ZDateTime.Now;
			Assert("Correct produce type and pack date", !eXDOCHeader.QH_PackDateInfo.HasMessageErrors());
		}

		public void TestCheckQH_OriginCatchZone()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_OriginCatchZoneInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_OriginCatchZone = "Elizabeth River";
			Assert("Origin catch zone cannot be set for produce type meat", eXDOCHeader.QH_OriginCatchZoneInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_OriginCatchZone = "Darwin Harbour";
			Assert("Correct produce type and origin catch zone", !eXDOCHeader.QH_OriginCatchZoneInfo.HasMessageErrors());
		}

		public void TestCheckQH_LotNumber()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_LotNumberInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_LotNumber = "123";
			Assert("Lot number cannot be set for produce type skins and hides", eXDOCHeader.QH_LotNumberInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_LotNumber = "123,34";
			Assert("Correct produce type and lot number", !eXDOCHeader.QH_LotNumberInfo.HasMessageErrors());
		}

		public void TestCheckQH_CertificatePrintIndicator()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Custom certificate cannot be set for produce type fish", eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Custom certificate cannot be set for produce type Eggs", eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Custom certificate cannot be set for produce type Dairy", eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Custom certificate cannot be set for produce type InedibleMeat", eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Custom certificate cannot be set for produce type Meat", eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Correct produce type and custom certificate", !eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Correct produce type and custom certificate", !eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.CustomCertificate;
			Assert("Correct produce type and custom certificate", !eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Manual;
			Assert("Incorrect produce type, correct custom certificate", !eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
			eXDOCHeader.QH_CertificatePrintIndicator = "Z";
			Assert("Incorrect print indicator", eXDOCHeader.QH_CertificatePrintIndicatorInfo.HasMessageErrors());
		}

		public void TestCheckQH_ShipsStores()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_ShipsStoresInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Fish;
			eXDOCHeader.QH_ShipsStores = ZBool.True;
			Assert("Ships stores cannot be set for produce type fish", eXDOCHeader.QH_ShipsStoresInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_ShipsStores = ZBool.True;
			Assert("Correct produce type and ships stores", !eXDOCHeader.QH_ShipsStoresInfo.HasMessageErrors());
		}

		public void TestCheckQH_AMLCQuota()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AMLCQuotaInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_AMLCQuota = ZBool.True;
			AssertHasMessageError("Expected error when Produce Type is Horticulture", eXDOCHeader.QH_AMLCQuotaInfo, "AMLC Quota can only be set when produce type is Meat or Dairy.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			AssertNoMessageError("No error when Produce Type is Meat", eXDOCHeader.QH_AMLCQuotaInfo, "AMLC Quota can only be set when produce type is Meat or Dairy.");

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			AssertNoMessageError("No error when Produce Type is Meat", eXDOCHeader.QH_AMLCQuotaInfo, "AMLC Quota can only be set when produce type is Meat or Dairy.");

			eXDOCHeader.QH_AMLCQuotaYear = ZDateTime.Today.Year.ToString();
			eXDOCHeader.QH_AMLCQuota = ZBool.False;
			AssertHasMessageError("Expected error when Quota Year is set", eXDOCHeader.QH_AMLCQuotaInfo, "AMLC Quota indicator needs to be Set when AMCL Quota Year is entered.");

			eXDOCHeader.QH_AMLCQuota = ZBool.True;
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaInfo, "AMLC Quota indicator needs to be Set when AMCL Quota Year is entered.");
		}

		public void TestCheckQH_AMLCQuotaYear()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AMLCQuotaYearInfo.HasMessageErrors());

			eXDOCHeader.QH_AMLCQuota = ZBool.False;
			eXDOCHeader.QH_AMLCQuotaYear = "";
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year needs to be entered when the AMLC Quota indicator is YES.");

			eXDOCHeader.QH_AMLCQuota = ZBool.True;
			eXDOCHeader.QH_AMLCQuotaYear = "";
			AssertHasMessageError("Expected error when Quota Year is not set", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year needs to be entered when the AMLC Quota indicator is YES.");

			var thisYear = ZDateTime.Today.Year;
			var lastYear = thisYear - 1;
			var nextYear = thisYear + 1;

			eXDOCHeader.QH_AMLCQuotaYear = thisYear.ToString();
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year needs to be entered when the AMLC Quota indicator is YES.");
			Assert(!eXDOCHeader.QH_AMLCQuotaYearInfo.HasMessageErrors());

			eXDOCHeader.QH_AMLCQuotaYear = lastYear.ToString();
			AssertHasMessageError("Expected error when Quota Year is too low", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year must be this year or next year.");

			eXDOCHeader.QH_AMLCQuotaYear = nextYear.ToString();
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year must be this year or next year.");

			eXDOCHeader.QH_AMLCQuotaYear = (nextYear + 1).ToString();
			AssertHasMessageError("Expected error when Quota Year is too high", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year must be this year or next year.");

			eXDOCHeader.QH_AMLCQuotaYear = (thisYear + "-" + (nextYear - 2000));
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year must be this year or next year.");
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year range must be consecutive years.");
			AssertNoMessageError(eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year range must begin last year, this year or next year.");

			eXDOCHeader.QH_AMLCQuotaYear = (lastYear + "-" + (nextYear - 2000));
			AssertHasMessageError("Expected error when Quota Year range is too wide", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year range must be consecutive years.");

			eXDOCHeader.QH_AMLCQuotaYear = ((lastYear - 1) + "-" + (lastYear - 2000));
			AssertHasMessageError("Expected error when Quota Year is too low", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year range must begin last year, this year or next year.");

			eXDOCHeader.QH_AMLCQuotaYear = ((nextYear) + "-" + (nextYear + 1 - 2000));
			Assert(!eXDOCHeader.QH_AMLCQuotaYearInfo.HasMessageErrors());

			eXDOCHeader.QH_AMLCQuotaYear = ((nextYear + 1) + "-" + (nextYear + 2 - 2000));
			AssertHasMessageError("Expected error when Quota Year is too high", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMCL Quota Year range must begin last year, this year or next year.");

			eXDOCHeader.QH_AMLCQuotaYear = "ABC123";
			AssertHasMessageError("Expected error when Quota Year is invalid", eXDOCHeader.QH_AMLCQuotaYearInfo, "AMLC Quota Year must be in the format CCYY, or CCYY-YY (e.g. 2017-18)");
		}

		public void TestCheckQH_InspectionRequestedDate()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_InspectionRequestedDateInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_InspectionRequestedDate = ZDateTime.Now;
			Assert("Inspection requested date cannot be set for produce type wool", eXDOCHeader.QH_InspectionRequestedDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			eXDOCHeader.QH_InspectionRequestedDate = ZDateTime.Now;
			Assert("Inspection requested date cannot be greater than departure date", eXDOCHeader.QH_InspectionRequestedDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now;
			eXDOCHeader.QH_InspectionRequestedDate = ZDateTime.Now.AddDays(-1);
			Assert("Correct produce, departure date and inspection requested date", !eXDOCHeader.QH_InspectionRequestedDateInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				eXDOCHeader.QH_InspectionRequestedDate = ZDateTime.Now;
				AssertHasMessageErrorContaining(eXDOCHeader.QH_InspectionRequestedDateInfo, "Inspection Requested Date must not be present when Produce Type is Horticulture, Grains and Seeds, Skins and Hides or Wool");
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
				eXDOCHeader.QH_InspectionRequestedDate = ZDateTime.Now;
				AssertHasMessageErrorContaining(eXDOCHeader.QH_InspectionRequestedDateInfo, "Inspection Requested Date must not be present when Produce Type is Horticulture, Grains and Seeds, Skins and Hides or Wool");
			}
		}

		public void TestCheckQH_AuthorisedStartDate()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisedStartDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Now;
			AssertHasMessageError("Authorised start date cannot be set for produce type meat", eXDOCHeader.QH_AuthorisedStartDateInfo, "Authorised start date can only be set when produce type is horticulture or grains and plants.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Now;
			AssertHasMessageError("Authorised start date cannot be greater than export date", eXDOCHeader.QH_AuthorisedStartDateInfo, "Authorised start date must be less than or equal to departure date.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Now.AddDays(-1);
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now.AddDays(-2);
			AssertHasMessageError("Authorised start date cannot be greater than authorised end date", eXDOCHeader.QH_AuthorisedStartDateInfo, "Authorised start date must be less than or equal to authorised end date.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Empty;
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now;
			AssertHasMessageError("Authorised start date cannot be empty when authorised end date is not for valid commodity", eXDOCHeader.QH_AuthorisedStartDateInfo, "Authorised start date must be present when authorised end date is entered.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Empty;
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now;
			AssertHasMessageError("Authorised start date cannot be empty when authorised end date is not", eXDOCHeader.QH_AuthorisedStartDateInfo, "Authorised start date must be present when authorised end date is entered.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Now.AddDays(-1);
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Empty;
			Assert("Correct produce type, export date and authorised start date", !eXDOCHeader.QH_AuthorisedStartDateInfo.HasMessageErrors());
		}

		public void TestCheckQH_AuthorisedEndDate()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_AuthorisedEndDateInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now;
			AssertHasMessageError("Authorised end date cannot be set for produce type Dairy", eXDOCHeader.QH_AuthorisedEndDateInfo, "Authorised end date is not allowed when produce type is dairy.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now.AddDays(-1);
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now;
			AssertHasMessageError("Authorised end date cannot be greater than export date", eXDOCHeader.QH_AuthorisedEndDateInfo, "Authorised end date must be less than or equal to departure date.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Wool;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Empty;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Now.AddDays(-1);
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now.AddDays(-2);
			AssertHasMessageError("Authorised end date cannot be less then authorised start date", eXDOCHeader.QH_AuthorisedEndDateInfo, "Authorised end date must be greater than or equal to authorised start date.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.Declaration.JE_ExportDate = ZDateTime.Now;
			eXDOCHeader.QH_AuthorisedStartDate = ZDateTime.Now.AddDays(-2);
			eXDOCHeader.QH_AuthorisedEndDate = ZDateTime.Now.AddDays(-1);
			Assert("Correct produce type, export date, authorised start date and authorised end date", !eXDOCHeader.QH_AuthorisedEndDateInfo.HasMessageErrors());
		}

		public void TestCheckQH_AQISRegion()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("AQISP", "AQISP");

			var canberra = refHelper.CreateNewOrGetExistingCusCodeList("AU", "AQISP", "CBR", "CANBERRA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.QuarantineRegion, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.Dairy, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.InedibleMeat, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.Meat, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.SkinsAndHides, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(canberra.PK, EXDOCCommodityCodeAttributes.Codes.Wool, "");

			var redcliffs = refHelper.CreateNewOrGetExistingCusCodeList("AU", "AQISP", "RED", "RED CLIFFS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(redcliffs.PK, EXDOCCommodityCodeAttributes.Codes.QuarantineOffice, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(redcliffs.PK, EXDOCCommodityCodeAttributes.Codes.State, "VIC");
			Factory.Save();

			Assert("Pre-Condition", !eXDOCHeader.QH_AQISRegionInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			eXDOCHeader.QH_AQISRegion = ZString.Empty;
			AssertNoMessageErrors("Not mandatory for Meat", eXDOCHeader.QH_AQISRegionInfo);
			eXDOCHeader.QH_AQISRegion = "B";
			AssertHasMessageError("Production region is invalid for any commodity if entered", eXDOCHeader.QH_AQISRegionInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_AQISRegion = "CBR";
			AssertNoMessageError(eXDOCHeader.QH_AQISRegionInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_AQISRegion = "RED";
			AssertHasMessageError("Production region is invalid for meat", eXDOCHeader.QH_AQISRegionInfo, "The code you have selected is not in the list.");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_AQISRegion = ZString.Empty;
			AssertHasMessageError("Production region cannot be empty", eXDOCHeader.QH_AQISRegionInfo, "You have not entered a " + eXDOCHeader.QH_AQISRegionInfo.Description + ".");
			eXDOCHeader.QH_AQISRegion = "CBR";
			AssertNoMessageError(eXDOCHeader.QH_AQISRegionInfo, "You have not entered a " + eXDOCHeader.QH_AQISRegionInfo.Description + ".");
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.InedibleMeat;
			eXDOCHeader.QH_AQISRegion = ZString.Empty;
			AssertNoMessageErrors("Not mandatory for Inedible Meat", eXDOCHeader.QH_AQISRegionInfo);

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_AQISRegion = ZString.Empty;
			AssertEquals(true, eXDOCHeader.IsNEXDOCSActive);
			eXDOCHeader.QH_AQISRegion = "B";
			AssertNoMessageError("Production region is invalid for any commodity if entered", eXDOCHeader.QH_AQISRegionInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckQH_CertificateRequiredLocation()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("AQISP", "AQISP");

			var darwin = refHelper.CreateNewOrGetExistingCusCodeList("AU", "AQISP", "DR2", "DARWIN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.QuarantineRegion, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Dairy, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Eggs, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Fish, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.GrainsAndPlants, "");
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(darwin.PK, EXDOCCommodityCodeAttributes.Codes.Horticulture, "");

			Factory.Save();

			Assert("Pre-Condition", !eXDOCHeader.QH_CertificateRequiredLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_PrintLocation = QuarantineExDocHeaderLookups.AqisPlaceCode;
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.QH_CertificateRequiredLocation = "B";
			Assert("Certificate Required Location is invalid for any commodity", eXDOCHeader.QH_CertificateRequiredLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_CertificateRequiredLocation = "DR2";
			Assert("Certificate required location is valid", !eXDOCHeader.QH_CertificateRequiredLocationInfo.HasMessageErrors());
			eXDOCHeader.QH_CertificatePrintIndicator = EXDOCCertificatePrintCodes.Codes.Automatic;
			eXDOCHeader.QH_CertificateRequiredLocation = ZString.Empty;
			Assert("Certificate required location is invalid as print indicator is A", eXDOCHeader.QH_CertificateRequiredLocationInfo.HasMessageErrors());
		}

		public void TestCheckQH_InspectorComments()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_InspectorCommentsInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisingOfficerID = ZString.Empty;
			eXDOCHeader.QH_InspectorComments = "Some comments about nothing";
			Assert("Comments are invalid when there is no Officer ID", eXDOCHeader.QH_InspectorCommentsInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisingOfficerID = "ABCD";
			eXDOCHeader.QH_InspectorComments = "Some comments about nothing 2";
			Assert("Comments are valid as there is an Officer ID", !eXDOCHeader.QH_InspectorCommentsInfo.HasMessageErrors());
		}

		public void TestCheckQH_StartHoldSeal()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_StartHoldSealInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCHeader.QH_AuthorisingOfficerID = "AQAMEAT";
			eXDOCHeader.QH_StartHoldSeal = "123456";
			Assert("Seal number is too cold", eXDOCHeader.QH_StartHoldSealInfo.HasMessageErrors());
			eXDOCHeader.QH_StartHoldSeal = "1234567";
			Assert("Seal number is too hot", !eXDOCHeader.QH_StartHoldSealInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisingOfficerID = ZString.Empty;
			eXDOCHeader.QH_StartHoldSeal = "1234567";
			Assert("Seal number is entered without officer ID", eXDOCHeader.QH_StartHoldSealInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisingOfficerID = "AQAFISH";
			eXDOCHeader.QH_StartHoldSeal = "7654321";
			Assert("Seal number is entered with officer ID", !eXDOCHeader.QH_StartHoldSealInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				eXDOCHeader.QH_StartHoldSeal = "7654321";
				AssertHasMessageErrorContaining(eXDOCHeader.QH_StartHoldSealInfo, "Start Hold Seal must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQH_EndHoldSeal()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_EndHoldSealInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCHeader.QH_AuthorisingOfficerID = "AQAMEAT";
			eXDOCHeader.QH_EndHoldSeal = "123456";
			Assert("Seal number is too cold", eXDOCHeader.QH_EndHoldSealInfo.HasMessageErrors());
			eXDOCHeader.QH_EndHoldSeal = "1234567";
			Assert("Seal number is too hot", !eXDOCHeader.QH_EndHoldSealInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisingOfficerID = ZString.Empty;
			eXDOCHeader.QH_EndHoldSeal = "1234567";
			Assert("Seal number is entered without officer ID", eXDOCHeader.QH_EndHoldSealInfo.HasMessageErrors());
			eXDOCHeader.QH_AuthorisingOfficerID = "AQAFISH";
			eXDOCHeader.QH_EndHoldSeal = "7654321";
			Assert("Seal number is entered with officer ID", !eXDOCHeader.QH_EndHoldSealInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
				eXDOCHeader.QH_EndHoldSeal = "7654321";
				AssertHasMessageErrorContaining(eXDOCHeader.QH_EndHoldSealInfo, "End Hold Seal must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQH_ConsigneeAgentName()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_ConsigneeAgentNameInfo.HasMessageErrors());
			AssertEquals("Pre-Condition", false, UniversalReferenceHelper.Errata53Enabled());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.QH_ConsigneeAgentName = "Name";
			Assert("No message error", !eXDOCHeader.QH_EndHoldSealInfo.HasMessageErrors());

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(true, UniversalReferenceHelper.Errata53Enabled());

				eXDOCHeader.QH_ConsigneeAgentName = "TestName";
				AssertHasMessageErrorContaining(eXDOCHeader.QH_ConsigneeAgentNameInfo, "Consignee Representative must not be present when Produce Type is Horticulture or Grains and Seeds");
			}
		}

		public void TestCheckQH_SplitHealthCertByContainer()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_SplitHealthCertByContainerInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_SplitHealthCertByContainer = ZBool.True;
			Assert("Split by container is valid for dairy", !eXDOCHeader.QH_SplitHealthCertByContainerInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.Validation.ValidateQH_SplitHealthCertByContainer();
			Assert("Split by container is invalid for Horticulture", eXDOCHeader.QH_SplitHealthCertByContainerInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Validation.ValidateQH_SplitHealthCertByContainer();
			Assert("Split by container is invalid for Grains and plants", eXDOCHeader.QH_SplitHealthCertByContainerInfo.HasMessageErrors());
		}

		public void TestCheckQH_SplitHealthCertByMarks()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_SplitHealthCertByMarksInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_SplitHealthCertByMarks = ZBool.True;
			Assert("Split by marks is valid for dairy", !eXDOCHeader.QH_SplitHealthCertByMarksInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.Validation.ValidateQH_SplitHealthCertByMarks();
			Assert("Split by marks is invalid for horticulture", eXDOCHeader.QH_SplitHealthCertByMarksInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Validation.ValidateQH_SplitHealthCertByMarks();
			Assert("Split by marks is invalid for grains and horticulture", eXDOCHeader.QH_SplitHealthCertByMarksInfo.HasMessageErrors());
		}

		public void TestCheckQH_SplitHealthCertByPacker()
		{
			Assert("Pre-Condition", !eXDOCHeader.QH_SplitHealthCertByPackerInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_SplitHealthCertByPacker = ZBool.True;
			Assert("Split by packer is valid for dairy", !eXDOCHeader.QH_SplitHealthCertByPackerInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Horticulture;
			eXDOCHeader.Validation.ValidateQH_SplitHealthCertByPacker();
			Assert("Split by packer is invalid for horticulture", eXDOCHeader.QH_SplitHealthCertByPackerInfo.HasMessageErrors());
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
			eXDOCHeader.Validation.ValidateQH_SplitHealthCertByPacker();
			Assert("Split by packer is invalid for grains and horticulture", eXDOCHeader.QH_SplitHealthCertByPackerInfo.HasMessageErrors());
		}

		public void TestCheckQH_ForwardStatus_EXDOC()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				Assert("Pre-Condition", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageErrors());

				eXDOCHeader.QH_ForwardeeEDIUserIdentifier = "123TEST";
				eXDOCHeader.QH_ForwardStatus = ZString.Empty;
				Assert("Forward status cannot be empty when EDI user identifier is entered", eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status may not be empty when forward edi user identifier is not empty."));

				eXDOCHeader.QH_ForwardStatus = "1234";
				Assert("Forward status has been entered", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status may not be empty when forward edi user identifier is not empty."));
				Assert("Forward status is entered with an edi user identifier", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status is not allowed when forward edi user identifier is not entered."));

				eXDOCHeader.QH_ForwardeeEDIUserIdentifier = ZString.Empty;
				eXDOCHeader.Validation.ValidateQH_ForwardStatus();
				Assert("Forward status is entered without an edi user identifier", eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status is not allowed when forward edi user identifier is not entered."));
			}
		}

		public void TestCheckQH_ForwardStatus_NEXDOCS()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				Assert("Pre-Condition", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageErrors());

				eXDOCHeader.QH_ForwardeeEDIUserIdentifier = "123TEST";
				eXDOCHeader.QH_ForwardStatus = ZString.Empty;
				Assert("Forward status may be empty when EDI user identifier is entered (For NEXDOCS)", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status may not be empty when forward edi user identifier is not empty."));

				eXDOCHeader.QH_ForwardStatus = "1234";
				Assert("Forward status has been entered", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status may not be empty when forward edi user identifier is not empty."));
				Assert("Forward status is entered with an edi user identifier", !eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status is not allowed when forward edi user identifier is not entered."));

				eXDOCHeader.QH_ForwardeeEDIUserIdentifier = ZString.Empty;
				eXDOCHeader.Validation.ValidateQH_ForwardStatus();
				Assert("Forward status is entered without an edi user identifier", eXDOCHeader.QH_ForwardStatusInfo.HasMessageError("Forward status is not allowed when forward edi user identifier is not entered."));
			}
		}

		public void TestCheckQH_ImportedProductFlag()
		{
			AssertNoMessageErrors("Pre-Condition", eXDOCHeader.QH_ImportedProductFlagInfo);

			eXDOCHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.No;
			AssertNoMessageErrors("No Message error when importedProductFlag has a value", eXDOCHeader.QH_ImportedProductFlagInfo);

			eXDOCHeader.QH_ImportedProductFlag = ZString.Empty;
			AssertNoMessageErrors("No Message Error when importedProductFlag is empty", eXDOCHeader.QH_ImportedProductFlagInfo);

			eXDOCHeader.QH_ImportedProductFlag = EXDOCYesNoEmpty.Codes.Yes;
			AssertNoMessageErrors("No Message error when importedProductFlag has a value", eXDOCHeader.QH_ImportedProductFlagInfo);

			eXDOCHeader.QH_ImportedProductFlag = "@@";
			AssertHasMessageErrors("Message Error when importedProductFlag is invalid", eXDOCHeader.QH_ImportedProductFlagInfo);
		}

		public void TestCheckQH_OH_TransferEDIUserLocationOrganisation()
		{
			AssertHeaderWithRegCodes(eXDOCHeader.QH_OH_TransferEDIUserLocationOrganisationInfo, OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, QuarantineExDocHeaderValidation.NEXDOCRequired);
		}

		public void TestCheckQH_OH_TransferExporterLocationOrganisation()
		{
			AssertHeaderWithRegCodes(eXDOCHeader.QH_OH_TransferExporterLocationOrganisationInfo, OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExportNumber, QuarantineExDocHeaderValidation.EXDOCESNRequired);
		}

		public void TestCheckQH_OH_ForwardLocationOrganisation()
		{
			AssertHeaderWithRegCodes(eXDOCHeader.QH_OH_ForwardLocationOrganisationInfo, OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, QuarantineExDocHeaderValidation.NEXDOCRequired);
		}

		public void TestCheckQH_OH_PrintLocationOrganisation()
		{
			AssertHeaderWithRegCodes(eXDOCHeader.QH_OH_PrintLocationOrganisationInfo, OrgCusCode.AUQuarantineCodeTypes.EXDOCEDIUser, OrgCusCode.AUQuarantineCodeTypes.NEXDOCSExternalID, QuarantineExDocHeaderValidation.NEXDOCRequired);
		}

		public void TestCheckQH_DecOfCompliance()
		{
			AssertNoMessageErrors("Pre-Condition", eXDOCHeader.QH_DecOfComplianceInfo);

			eXDOCHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.No;
			AssertNoMessageErrors("No Message error when compliance has a value", eXDOCHeader.QH_DecOfComplianceInfo);

			eXDOCHeader.QH_DecOfCompliance = ZString.Empty;
			AssertNoMessageErrors("No Message Error when compliance is empty", eXDOCHeader.QH_DecOfComplianceInfo);

			eXDOCHeader.QH_DecOfCompliance = EXDOCYesNoEmpty.Codes.Yes;
			AssertNoMessageErrors("No Message error when compliance has a value", eXDOCHeader.QH_DecOfComplianceInfo);

			eXDOCHeader.QH_DecOfCompliance = "@@";
			AssertHasMessageErrors("Message Error when compliance is invalid", eXDOCHeader.QH_DecOfComplianceInfo);
		}

		public void TestCheckQH_OA_AuthorisationEstablishment()
		{
			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Organisation;
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			eXDOCHeader.QH_OA_AuthorisationEstablishment = address.PK;
			var expectedMessageError = ZString.Format(QuarantineExDocHeaderValidation.ESNIsRequired, "Authorisation");
			AssertHasMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedMessageError);

			eXDOCHeader.QH_OA_AuthorisationEstablishment = ZGuid.Empty;
			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Code;
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedMessageError);

			eXDOCHeader.QH_AuthorisationLocation = EXDOCCodeOrganisation.Codes.Organisation;
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			cusCode.OK_CustomsRegNo = "23456";
			cusCode.OK_OA_PremisesAddress = address.PK;

			eXDOCHeader.QH_OA_AuthorisationEstablishment = address.PK;
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedMessageError);

			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			eXDOCHeader.QH_OA_AuthorisationEstablishment = Guid.Empty;
			Assert("Authorisation establishment is mandatory for produce type dairy", eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo.HasMessageErrors());

			eXDOCHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.SkinsAndHides;
			eXDOCHeader.QH_AuthorisationEstablishment = ZString.Empty;
			eXDOCHeader.QH_OA_AuthorisationEstablishment = ZGuid.Empty;
			AssertNoMessageError(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo, expectedMessageError);
			AssertNoMessageError(eXDOCHeader.QH_AuthorisationEstablishmentInfo, expectedMessageError);
			AssertNoMessageErrors(eXDOCHeader.QH_OA_AuthorisationEstablishmentInfo);
			AssertNoMessageErrors(eXDOCHeader.QH_AuthorisationEstablishmentInfo);
		}

		public void TestCheckQH_OA_StorageEstablishment()
		{
			var org = Factory.New<OrgHeader>();
			var address = org.Addresses.AddNew();
			eXDOCHeader.QH_OA_StorageEstablishment = address.PK;
			var expectedMessageError = ZString.Format(QuarantineExDocHeaderValidation.ESNIsRequired, "Storage");
			AssertHasMessageError(eXDOCHeader.QH_OA_StorageEstablishmentInfo, expectedMessageError);

			eXDOCHeader.QH_OA_StorageEstablishment = ZGuid.Empty;
			AssertNoMessageError(eXDOCHeader.QH_OA_StorageEstablishmentInfo, expectedMessageError);

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CodeType = OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber;
			cusCode.OK_CustomsRegNo = "23456";
			cusCode.OK_OA_PremisesAddress = address.PK;

			eXDOCHeader.QH_OA_StorageEstablishment = address.PK;
			AssertNoMessageError(eXDOCHeader.QH_OA_StorageEstablishmentInfo, expectedMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			eXDOCHeader = invoiceHeader.QuarantineExDocHeader;
		}

		QuarantineExDocHeader eXDOCHeader;

		void AssertHeaderWithRegCodes(ZPropertyInfo info, string exdocCode, string nexdocCode, string noexdocError)
		{
			CombineAssertions(() =>
			{
				var invalidError = "Enter a valid selection.";

				((QuarantineExDocHeader)info.BizObj).QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
				Assert("Pre-Condition", !info.HasNotifications());

				var testOrg = Factory.New<OrgHeader>();
				testOrg.OH_Code = "ABCDEF";
				testOrg.OH_FullName = "THIS IS A TEST ORG";

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
				{
					info.Value = ZGuid.Invalid;
					AssertHasError("Invalid Guid", info, invalidError);

					info.Value = ZGuid.NewZGuid();
					AssertHasError("Wrong Organisation", info, noexdocError);

					info.Value = testOrg.PK;
					AssertHasError("Should have an error as there is no matched exdoc codes.", info, noexdocError);

					testOrg.CustomsCodes.AddNew(exdocCode, "1234").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
					Factory.Save();

					info.Value = ZGuid.Empty;
					info.Value = testOrg.PK;
					AssertNoNotifications("Should not have the error as there is a matched exdoc code.", info);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_MEA, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
				{
					info.Value = ZGuid.Empty;
					info.Value = testOrg.PK;
					AssertHasError("Should has error as there is no matched nexdoc codes.", info, noexdocError);

					testOrg.CustomsCodes.AddNew(nexdocCode, "5678").OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
					Factory.Save();

					info.Value = ZGuid.Empty;
					info.Value = testOrg.PK;
					AssertNoNotifications("Should not have the error as there is a matched nexdoc code.", info);
				}
			});
		}
	}
}
