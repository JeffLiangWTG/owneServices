using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.JAS.Registry.Business.Testing
{
	class CognosModeMappingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(CognosModeMappingValidation), ModeMapping.Validation.AutoValidationType);
		}

		public void TestValidateSelectedMode()
		{
			ModeMapping.SelectedMode = "";
			ModeMapping.Validation.ValidateSelectedMode();
			AssertMandatoryValidationError(ModeMapping.SelectedModeInfo, true);
			ModeMapping.SelectedMode = "XXX";
			AssertMandatoryValidationError(ModeMapping.SelectedModeInfo, false);
			AssertListValidationInvalidCodeError(ModeMapping.SelectedModeInfo, true);
			ModeMapping.SelectedMode = "AI";
			AssertNoErrors("Valid Cognos mode, should have no errors", ModeMapping.SelectedModeInfo);
		}

		public void TestValidateAll()
		{
			ModeMapping.Validation.ValidateAll();
			AssertHasErrors("Should call ValidateSelectedMode()", ModeMapping.SelectedModeInfo);
			AssertMandatoryValidationError(ModeMapping.SelectedModeInfo, true);
		}

		CognosModeMapping ModeMapping
		{
			get
			{
				if (fModeMapping == null)
				{
					fModeMapping = new CognosModeMapping();
				}

				return fModeMapping;
			}
		}

		CognosModeMapping fModeMapping;
	}
}
