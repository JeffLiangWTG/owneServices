using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class JobEUDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckEUD_AgreedPlaceCode_IsRequired()
		{
			const string messageError = "Incoterm Place Code or Country Code is required";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					declaration.AddInfoChildValidation.ValidateEUD_AgreedPlaceCode();
					AssertHasMessageError("Empty EUD_AgreedPlaceCode", declaration.EUD_AgreedPlaceCodeInfo, messageError);

					declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.Other;
					declaration.AddInfoChildValidation.ValidateEUD_AgreedPlaceCode();
					AssertNoNotifications("Type is XXX (Other)", declaration.EUD_AgreedPlaceCodeInfo);
				}
			});
		}

		public void TestCheckEUD_AgreedPlaceCode_IsRequired_EUD_AgreedPlaceCodeValidationSupportIsFalse()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Protected().Setup<bool>("EUD_AgreedPlaceCodeValidationSupportCore").Returns(false);

			var declaration = declarationMock.Object;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				declaration.AddInfoChildValidation.ValidateEUD_AgreedPlaceCode();
				AssertNoNotifications("EUD_AgreedPlaceCodeValidationSupportCore is false", declaration.EUD_AgreedPlaceCodeInfo);
			}
		}

		public void TestCheckEUD_AgreedPlaceCode_Country()
		{
			const string message = "Incoterm Place Code must be a valid country";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.EUD_AgreedPlaceCode = "XX";
					AssertHasMessageError("EUD_AgreedPlaceCode 'XX' should have an error", declaration.EUD_AgreedPlaceCodeInfo, message);

					declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertNoMessageError("EUD_AgreedPlaceCode 'BE' should not have an error", declaration.EUD_AgreedPlaceCodeInfo, message);
				}
			});
		}

		public void TestCheckEUD_AgreedPlaceCode_Country_Validation_JE_ShipmentIncoTermPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.EUD_AgreedPlaceCode = "BEBRU";
					AssertNoMessageErrors(declaration.JE_ShipmentIncoTermPlaceInfo);
					declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertHasMessageErrors(declaration.JE_ShipmentIncoTermPlaceInfo);
				}
			});
		}

		public void TestCheckEUD_AgreedPlaceCode_UNLOCODE()
		{
			const string massage = "Incoterm Place Code must be a valid UNLOCODE";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.EUD_AgreedPlaceCode = "BEAAA";
					AssertHasMessageError("EUD_AgreedPlaceCode 'BEAAA' should have an error", declaration.EUD_AgreedPlaceCodeInfo, massage);

					declaration.EUD_AgreedPlaceCode = "BEBRU";
					AssertNoMessageError("EUD_AgreedPlaceCode 'BEBRU' should not have an error", declaration.EUD_AgreedPlaceCodeInfo, massage);
				}
			});
		}

		public void TestCheckEUD_AgreedPlaceCode_Invalid()
		{
			const string message = "Incoterm Place Code must either be a valid country or a valid UNLOCODE";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.EUD_AgreedPlaceCode = "TRY";
					AssertHasMessageError("EUD_AgreedPlaceCode 'TRY' should have an error", declaration.EUD_AgreedPlaceCodeInfo, message);

					declaration.EUD_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertNoMessageError("EUD_AgreedPlaceCode 'BE' should not have an error", declaration.EUD_AgreedPlaceCodeInfo, message);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_IsRequired()
		{
			const string messageError = "Incoterm Place Code or Country Code is required";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertHasMessageError("Empty ZG_AgreedPlaceCode", declaration.ZG_AgreedPlaceCodeInfo, messageError);

					declaration.ZG_AgreedPlaceCode = "BEBRU";
					declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("Valid ZG_AgreedPlaceCode", declaration.ZG_AgreedPlaceCodeInfo);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_IsRequired_ZG_AgreedPlaceCodeValidationSupportIsFalse()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Protected().Setup<bool>("ZG_AgreedPlaceCodeValidationSupportCore").Returns(false);

			var declaration = declarationMock.Object;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("ZG_AgreedPlaceCodeValidationSupportCore is false", declaration.ZG_AgreedPlaceCodeInfo);
			}
		}

		public void TestCheckZG_AgreedPlaceCode_Country()
		{
			const string message = "Agreed Place Code must be a valid country";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.ZG_AgreedPlaceCode = "XX";
					AssertHasMessageError("ZG_AgreedPlaceCode 'XX' should have an error", declaration.ZG_AgreedPlaceCodeInfo, message);

					declaration.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertNoMessageError("ZG_AgreedPlaceCode 'BE' should not have an error", declaration.ZG_AgreedPlaceCodeInfo, message);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_Country_Validation_JE_ShipmentIncoTermPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.ZG_AgreedPlaceCode = "BEBRU";
					declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
					AssertNoMessageErrors(declaration.JE_ShipmentIncoTermPlaceInfo);
					declaration.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
					AssertHasMessageErrors(declaration.JE_ShipmentIncoTermPlaceInfo);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_UNLOCODE()
		{
			const string massage = "Agreed Place Code must be a valid UNLOCODE";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.ZG_AgreedPlaceCode = "BEAAA";
					AssertHasMessageError("ZG_AgreedPlaceCode 'BEAAA' should have an error", declaration.ZG_AgreedPlaceCodeInfo, massage);

					declaration.ZG_AgreedPlaceCode = "BEBRU";
					AssertNoMessageError("ZG_AgreedPlaceCode 'BEBRU' should not have an error", declaration.ZG_AgreedPlaceCodeInfo, massage);
				}
			});
		}

		public void TestCheckZG_AgreedPlaceCode_Invalid()
		{
			const string message = "Agreed Place Code must either be a valid country or a valid UNLOCODE";
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					declaration.ZG_AgreedPlaceCode = "TRY";
					AssertHasMessageError("ZG_AgreedPlaceCode 'TRY' should have an error", declaration.ZG_AgreedPlaceCodeInfo, message);

					declaration.ZG_AgreedPlaceCode = Core.Constants.CountryCodes.Belgium;
					AssertNoMessageError("ZG_AgreedPlaceCode 'BE' should not have an error", declaration.ZG_AgreedPlaceCodeInfo, message);
				}
			});
		}
	}
}
