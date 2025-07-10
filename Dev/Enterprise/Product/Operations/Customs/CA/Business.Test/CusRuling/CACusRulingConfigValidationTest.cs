using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using ConfigCategories = Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACusRulingConfigValidationTest : TestCaseWithFactory
	{
		public void TestCategoryAndTypeUniqueness()
		{
			var expectedMessage = "Duplicate configuration records with the same Category and Type are not permitted.";
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config1 = cusRuling.Configurations.AddNew();
			config1.ZZY_Category = ConfigCategories.DTY;
			config1.ZZY_Type = ConfigTypes.AcceptAmount;
			var config2 = cusRuling.Configurations.AddNew();
			config2.ZZY_Category = ConfigCategories.DTY;
			config2.ZZY_Type = ConfigTypes.AcceptAmount;
			config2.Validation.ValidateZZY_Category();
			AssertHasErrorContaining(config1.ZZY_TypeInfo, expectedMessage);
			AssertHasErrorContaining(config1.ZZY_CategoryInfo, expectedMessage);
			AssertHasErrorContaining(config2.ZZY_TypeInfo, expectedMessage);
			AssertHasErrorContaining(config2.ZZY_CategoryInfo, expectedMessage);
			config2.ZZY_Type = ConfigTypes.AcceptRate;
			config2.Validation.ValidateZZY_Category();
			AssertNoErrorContaining(config1.ZZY_TypeInfo, expectedMessage);
			AssertNoErrorContaining(config1.ZZY_CategoryInfo, expectedMessage);
			AssertNoErrorContaining(config2.ZZY_TypeInfo, expectedMessage);
			AssertNoErrorContaining(config2.ZZY_CategoryInfo, expectedMessage);
			config2.ZZY_Category = ConfigCategories.SIM;
			config2.ZZY_Type = ConfigTypes.AcceptAmount;
			config1.ZZY_Category = ConfigCategories.SIM;
			config1.ZZY_Type = ConfigTypes.AcceptAmount;
			config2.Validation.ValidateZZY_Category();
			AssertHasErrorContaining(config1.ZZY_TypeInfo, expectedMessage);
			AssertHasErrorContaining(config1.ZZY_CategoryInfo, expectedMessage);
			AssertHasErrorContaining(config2.ZZY_TypeInfo, expectedMessage);
			AssertHasErrorContaining(config2.ZZY_CategoryInfo, expectedMessage);

			config1.ZZY_Category = ConfigCategories.EXC;
			config1.ZZY_Type = ConfigTypes.AcceptAmount;
			config1.Validation.ValidateZZY_Category();
			AssertNoErrorContaining(config1.ZZY_TypeInfo, expectedMessage);
			AssertNoErrorContaining(config1.ZZY_CategoryInfo, expectedMessage);
			AssertNoErrorContaining(config2.ZZY_TypeInfo, expectedMessage);
			AssertNoErrorContaining(config2.ZZY_CategoryInfo, expectedMessage);
		}

		public void TestCheckZZY_Type()
		{
			AssertCheckZZY_Type(ConfigCategories.DTY);
			AssertCheckZZY_Type(ConfigCategories.SIM);
			AssertCheckZZY_Type(ConfigCategories.EXC);
			AssertCheckZZY_Type(ConfigCategories.GST);

			var expectedMessage = string.Format("{0} and {1} options cannot both be selected with same Category {2}.", RefCusRulingConfigTypes.Descriptions.DSD, RefCusRulingConfigTypes.Descriptions.REL, new RefCusRulingConfigCategories().GetDescriptionFromCode(ConfigCategories.DAT));
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config1 = cusRuling.Configurations.AddNew();
			config1.ZZY_Category = ConfigCategories.DAT;
			config1.ZZY_Type = ConfigTypes.REL;
			AssertNoErrorContaining(config1.ZZY_TypeInfo, expectedMessage);

			var config2 = cusRuling.Configurations.AddNew();
			config2.ZZY_Category = ConfigCategories.DAT;
			config2.ZZY_Type = ConfigTypes.DSD;
			AssertHasErrorContaining(config2.ZZY_TypeInfo, expectedMessage);

			config2.ZZY_Type = ConfigTypes.AcceptAmount;
			AssertHasError(config2.ZZY_TypeInfo, "Enter a valid selection.");
		}

		void AssertCheckZZY_Type(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var expectedMessage = string.Format("At least a configuration with category {0} and type Ad-Valorem or Specific should be added when a configuration with category {0} and type Accept Rate entered.",
					new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
				var config1 = cusRuling.Configurations.AddNew();
				config1.ZZY_Category = category;
				config1.ZZY_Type = ConfigTypes.AcceptRate;
				AssertHasErrorContaining(config1.ZZY_TypeInfo, expectedMessage);
				var config2 = cusRuling.Configurations.AddNew();
				config2.ZZY_Category = category;
				config2.ZZY_Type = ConfigTypes.AdValorem;
				config1.Validation.ValidateZZY_Type();
				AssertNoErrorContaining(config1.ZZY_TypeInfo, expectedMessage);

				var expectedMessage2 = string.Format("None/Free should be the only one {0} configuration if it entered. Please remove other {0} configurations.", new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				var config3 = cusRuling.Configurations.AddNew();
				config3.ZZY_Category = category;
				config3.ZZY_Type = ConfigTypes.NoneFree;
				AssertHasErrorContaining(config3.ZZY_TypeInfo, expectedMessage2);
				cusRuling.Configurations.Remove(config1);
				cusRuling.Configurations.Remove(config2);
				config3.Validation.ValidateZZY_Type();
				AssertNoErrorContaining(config3.ZZY_TypeInfo, expectedMessage2);

				var expectedMessage3 = string.Format("Accept Amount and Accept Rate options cannot both be selected with same Category {0}.", new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				config3.ZZY_Type = ConfigTypes.AcceptAmount;
				var config4 = cusRuling.Configurations.AddNew();
				config4.ZZY_Category = category;
				config4.ZZY_Type = ConfigTypes.AcceptRate;
				AssertHasErrorContaining(config3.ZZY_TypeInfo, expectedMessage3);
				AssertHasErrorContaining(config4.ZZY_TypeInfo, expectedMessage3);
				config4.ZZY_Type = ConfigTypes.AdValorem;
				AssertNoErrorContaining(config3.ZZY_TypeInfo, expectedMessage3);
				AssertNoErrorContaining(config4.ZZY_TypeInfo, expectedMessage3);

				var expectedMessage4 = string.Format("Ad-Valorem and Specific options cannot both be selected with same Category {0}.", new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				var config5 = cusRuling.Configurations.AddNew();
				config5.ZZY_Category = category;
				config5.ZZY_Type = ConfigTypes.Specific;
				AssertHasErrorContaining(config4.ZZY_TypeInfo, expectedMessage4);
				AssertHasErrorContaining(config5.ZZY_TypeInfo, expectedMessage4);

				config5.ZZY_Type = ConfigTypes.ExemptCode;
				AssertNoErrorContaining(config4.ZZY_TypeInfo, expectedMessage4);
				AssertNoErrorContaining(config5.ZZY_TypeInfo, expectedMessage4);

				var expectedMessage5 = string.Format("Accept Amount and Maximum options cannot both be selected with same Category {0}.", new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				var expectedMessage6 = string.Format("Accept Amount and Minimum options cannot both be selected with same Category {0}.", new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				var config6 = cusRuling.Configurations.AddNew();
				config6.ZZY_Category = category;
				config6.ZZY_Type = ConfigTypes.Maximum;
				AssertHasErrorContaining(config3.ZZY_TypeInfo, expectedMessage5);
				AssertHasErrorContaining(config6.ZZY_TypeInfo, expectedMessage5);

				config6.ZZY_Type = ConfigTypes.Minimum;
				AssertHasErrorContaining(config3.ZZY_TypeInfo, expectedMessage6);
				AssertHasErrorContaining(config6.ZZY_TypeInfo, expectedMessage6);
				AssertNoErrorContaining(config3.ZZY_TypeInfo, expectedMessage5);
				AssertNoErrorContaining(config6.ZZY_TypeInfo, expectedMessage5);

				config6.ZZY_Type = ConfigTypes.ExemptCode;
				AssertNoErrorContaining(config3.ZZY_TypeInfo, expectedMessage6);
				AssertNoErrorContaining(config6.ZZY_TypeInfo, expectedMessage6);
			});
		}

		public void TestCheckZZY_Rate()
		{
			AssertCheckZZY_Rate(ConfigCategories.DTY);
			AssertCheckZZY_Rate(ConfigCategories.SIM);
			AssertCheckZZY_Rate(ConfigCategories.EXC);
			AssertCheckZZY_Rate(ConfigCategories.GST);

			var messageError = "Invalid Rate, rate cannot be greater than 100%.";
			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var config = cusRuling.Configurations.AddNew();
			config.ZZY_Category = ConfigCategories.DTY;
			config.ZZY_Type = ConfigTypes.AdValorem;
			config.ZZY_Rate = 101m;
			AssertHasMessageErrorContaining(config.ZZY_RateInfo, messageError);
			config.ZZY_Rate = 100m;
			AssertNoMessageErrorContaining(config.ZZY_RateInfo, messageError);
			config.ZZY_Category = ConfigCategories.GST;
			config.ZZY_Type = ConfigTypes.AdValorem;
			config.ZZY_Rate = 101m;
			AssertHasMessageErrorContaining(config.ZZY_RateInfo, messageError);
			config.ZZY_Rate = 100m;
			AssertNoMessageErrorContaining(config.ZZY_RateInfo, messageError);
			config.ZZY_Category = ConfigCategories.SIM;
			config.ZZY_Type = ConfigTypes.AdValorem;
			config.ZZY_Rate = 101m;
			AssertNoMessageErrorContaining(config.ZZY_RateInfo, messageError);
			config.ZZY_Rate = 100m;
			AssertNoMessageErrorContaining(config.ZZY_RateInfo, messageError);
		}

		void AssertCheckZZY_Rate(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
				var config1 = cusRuling.Configurations.AddNew();
				config1.ZZY_Category = category;
				config1.ZZY_Type = ConfigTypes.AcceptRate;

				var expectedMessage1 = "Rates are expressed in percentages not decimals. The rate entered is less than 1%, please confirm that this is correct.";
				var config2 = cusRuling.Configurations.AddNew();
				config2.ZZY_Category = category;
				config2.ZZY_Type = ConfigTypes.AdValorem;
				config2.ZZY_Rate = 0.1m;
				AssertHasWarningContaining(config2.ZZY_RateInfo, expectedMessage1);

				config2.ZZY_Rate = 2m;
				AssertNoWarningContaining(config2.ZZY_RateInfo, expectedMessage1);

				var expectedMessage2 = "Either Rate or Value must be entered.";
				var config3 = cusRuling.Configurations.AddNew();
				config3.ZZY_Category = category;
				config3.ZZY_Type = ConfigTypes.Minimum;
				config3.ZZY_Rate = 1m;
				AssertNoErrorContaining(config3.ZZY_RateInfo, expectedMessage2);
				config3.ZZY_Rate = ZDecimal.Zero;
				AssertHasErrorContaining(config3.ZZY_RateInfo, expectedMessage2);

				var expectedMessage3 = string.Format("Ad-Valorem {0} rate is up to have 1 decimal place when Accept Rate {0} entered.", new RefCusRulingConfigCategories().GetDescriptionFromCode(category));
				config2.ZZY_Rate = 0.12m;
				AssertHasErrorContaining(config2.ZZY_RateInfo, expectedMessage3);
				config2.ZZY_Rate = 0.1m;
				AssertNoErrorContaining(config2.ZZY_RateInfo, expectedMessage3);
			});
		}

		public void TestValidateMaxAndMinValueAndRate()
		{
			AssertValidateMaxAndMinValueAndRate(ConfigCategories.DTY);
			AssertValidateMaxAndMinValueAndRate(ConfigCategories.SIM);
			AssertValidateMaxAndMinValueAndRate(ConfigCategories.EXC);
			AssertValidateMaxAndMinValueAndRate(ConfigCategories.GST);
		}

		void AssertValidateMaxAndMinValueAndRate(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
				var config1 = cusRuling.Configurations.AddNew();
				config1.ZZY_Category = category;
				config1.ZZY_Type = ConfigTypes.Maximum;

				var config2 = cusRuling.Configurations.AddNew();
				config2.ZZY_Category = category;
				config2.ZZY_Type = ConfigTypes.Minimum;

				var expectedMessage1 = "Rate(%) entered for Minimum must be less than or equal to Maximum: 1.0.";
				var expectedMessage2 = "Rate(%) entered for Maximum must be greater than or equal to Minimum: 2.0.";
				var expectedMessage3 = "Value entered for Minimum must be less than or equal to Maximum: 2.";
				var expectedMessage4 = "Value entered for Maximum must be greater than or equal to Minimum: 3.";

				config1.ZZY_Rate = 1.0m;
				config2.ZZY_Rate = 2.0m;
				AssertHasErrorContaining(config2.ZZY_RateInfo, expectedMessage1);
				AssertHasErrorContaining(config1.ZZY_RateInfo, expectedMessage2);

				config1.ZZY_Rate = 3.0m;
				AssertNoErrorContaining(config2.ZZY_RateInfo, expectedMessage1);
				AssertNoErrorContaining(config1.ZZY_RateInfo, expectedMessage2);

				config1.ZZY_Rate = 0.0m;
				config2.ZZY_Rate = 0.0m;
				config1.ZZY_Value = "2";
				config2.ZZY_Value = "3";
				AssertHasErrorContaining(config2.ZZY_ValueInfo, expectedMessage3);
				AssertHasErrorContaining(config1.ZZY_ValueInfo, expectedMessage4);

				config2.ZZY_Value = "2";
				AssertNoErrorContaining(config2.ZZY_ValueInfo, expectedMessage3);
				AssertNoErrorContaining(config1.ZZY_ValueInfo, expectedMessage4);
			});
		}

		public void TestCheckZZY_Value()
		{
			AssertCheckZZY_Value(ConfigCategories.DTY);
			AssertCheckZZY_Value(ConfigCategories.SIM);
			AssertCheckZZY_Value(ConfigCategories.EXC);
			AssertCheckZZY_Value(ConfigCategories.GST);

			var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
			var cusRulingConfig = cusRuling.Configurations.AddNew();
			cusRulingConfig.ZZY_Category = ConfigCategories.DTY;
			cusRulingConfig.ZZY_Type = ConfigTypes.TreatmentCode;
			cusRulingConfig.ZZY_Value = "XX";
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);
			cusRulingConfig.ZZY_Value = "";
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			cusRulingConfig.ZZY_Value = TariffTreatmentCodes.Codes.Australia;
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);

			cusRulingConfig.ZZY_Category = ConfigCategories.GST;
			cusRulingConfig.ZZY_Type = ConfigTypes.ExemptCode;
			cusRulingConfig.ZZY_Value = "XX";
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);
			cusRulingConfig.ZZY_Value = "";
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			cusRulingConfig.ZZY_Value = GSTStatusCodes.Codes.C48;
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);

			cusRulingConfig.ZZY_Category = ConfigCategories.SIM;
			cusRulingConfig.ZZY_Type = ConfigTypes.ExemptCode;
			cusRulingConfig.ZZY_Value = "XX";
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);
			cusRulingConfig.ZZY_Value = "";
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			cusRulingConfig.ZZY_Value = SIMACodes.Codes.C10;
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);

			cusRulingConfig.ZZY_Category = ConfigCategories.EXC;
			cusRulingConfig.ZZY_Type = ConfigTypes.ExemptCode;
			cusRulingConfig.ZZY_Value = "XX";
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);
			cusRulingConfig.ZZY_Value = "";
			AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.MustBeEntered);
			cusRulingConfig.ZZY_Value = ExciseTaxExemptionCodes.Codes.C85;
			AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, ListValidation.InvalidCodeError);
		}

		void AssertCheckZZY_Value(ZString category)
		{
			CombineAssertions(category, () =>
			{
				var cusRuling = Factory.NewWithValidTestData<CACusRuling>();
				var cusRulingConfig = cusRuling.Configurations.AddNew();
				cusRulingConfig.ZZY_Category = category;
				cusRulingConfig.ZZY_Type = ConfigTypes.AcceptAmount;
				cusRulingConfig.ZZY_Value = "1";
				AssertNoErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.ValueCannotBeZero);
				cusRulingConfig.ZZY_Value = "";
				AssertHasErrorContaining(cusRulingConfig.ZZY_ValueInfo, MandatoryValidation.ValueCannotBeZero);

				var cusRulingConfig2 = cusRuling.Configurations.AddNew();
				cusRulingConfig2.ZZY_Category = category;
				cusRulingConfig2.ZZY_Type = ConfigTypes.Minimum;

				cusRulingConfig2.ZZY_Value = "1";
				AssertNoErrorContaining(cusRulingConfig2.ZZY_ValueInfo, "Either Rate or Value must be entered.");

				cusRulingConfig2.ZZY_Value = "";
				AssertHasErrorContaining(cusRulingConfig2.ZZY_ValueInfo, "Either Rate or Value must be entered.");
			});
		}
	}
}
