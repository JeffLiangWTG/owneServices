using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TariffValidatorTest : TestCaseWithFactory
	{
		public void TestValidateFourDigitTariff()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "9955000000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			RunOneFourCharTariffValidationTest("9955", "");
			RunOneFourCharTariffValidationTest("9956", "Tariff code 9956 not found in the customs tariff code list.");
		}

		void RunOneFourCharTariffValidationTest(ZString tariffCode, ZString expectNotification)
		{
			CusClassification classification = Factory.New<CusClassification>();
			using (classification.Details.SuspendValidationTesting())
			using (classification.Details.GetValidationSuspender())
			{
				classification.CCA_99TariffCode = tariffCode;
				Validator.ValidateFourDigitTariff(classification.CCA_99TariffCodeInfo);
				if (expectNotification.IsEmpty)
				{
					AssertNoMessageErrors("No notification expected", classification.CCA_99TariffCodeInfo);
				}
				else
				{
					AssertHasMessageError("Notification expected", classification.CCA_99TariffCodeInfo, expectNotification);
				}
			}
		}

		TariffValidator Validator
		{
			get { return validator ?? (validator = new TariffValidator(Factory)); }
		}
		TariffValidator validator;
	}
}
