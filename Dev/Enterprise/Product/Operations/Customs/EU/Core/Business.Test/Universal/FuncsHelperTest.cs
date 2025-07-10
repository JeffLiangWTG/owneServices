using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.Business.FuncsHelper;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class FuncsHelperTest : TestCaseWithFactory
	{
		public void TestIsFunctionalityValid()
		{
			var today = ZDateTime.Today;

			var aestp = Constants.FunctionalityTypes.AESTransitionPeriod;
			var ie = Core.Constants.CountryCodes.Ireland;

			CombineAssertions("IsFunctionalityValid", () =>
			{
				AssertEquals("When Code is null, ", false, FuncsHelper.IsFunctionalityValid(null));
				AssertEquals("When Code is Empty, ", false, FuncsHelper.IsFunctionalityValid(ZString.Empty));
			});

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(aestp, ie, today, true))
			{
				CombineAssertions("IsFunctionalityValid", () =>
				{
					AssertEquals("When dataGrouping is IE", true, FuncsHelper.IsFunctionalityValid(aestp, ie));
					AssertEquals("When dataGrouping is IE", false, FuncsHelper.IsFunctionalityValid(aestp, ZDateTime.Today.AddDays(-2), ie));
					AssertEquals("When dataGrouping is null (default EUN)", false, FuncsHelper.IsFunctionalityValid(aestp, today));
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(aestp, "EUN", today, true))
			{
				AssertEquals("When dataGrouping is IE", false, FuncsHelper.IsFunctionalityValid(aestp, ie));
				AssertEquals("When dataGrouping is null (default EUN)", true, FuncsHelper.IsFunctionalityValid(aestp));
			}
		}

		enum SettingState { Enabled, Disabled, Overdue }

		public void TestIsFunctionalityValidForCountryOrEun()
		{
			var testCases = new (string Description, SettingState CountrySettings, SettingState EUNSettings, bool ExpectedResult)[]
			{
				("Enabled at both Current Country and EU levels", SettingState.Enabled, SettingState.Enabled, true),
				("Enabled in Current Country and disabled at EU level", SettingState.Enabled, SettingState.Disabled, true),
				("Enabled in Current Country and overdue at EU level", SettingState.Enabled, SettingState.Overdue, true),

				("Disabled in Current Country and enabled at EU level", SettingState.Disabled, SettingState.Enabled, true),
				("Disabled at both Current Country and EU levels", SettingState.Disabled, SettingState.Disabled, false),
				("Disabled in Current Country and overdue at EU level", SettingState.Disabled, SettingState.Overdue, false),

				("Overdue in Current Country and enabled at EU level", SettingState.Overdue, SettingState.Enabled, false),
				("Overdue in Current Country and disabled at EU level", SettingState.Overdue, SettingState.Disabled, false),
				("Overdue at both Current Country and EU levels", SettingState.Overdue, SettingState.Overdue, false),
			};

			const string testCountry = Core.Constants.CountryCodes.Latvia;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(testCountry))
			{
				CombineAssertions(() =>
				{
					var today = ZDateTime.Today;
					var yesterday = ZDateTime.Today.AddDays(-1);
					const string nctstpCode = Constants.FunctionalityTypes.NCTSTransitionPeriod;
					foreach (var (description, countrySettings, eunSettings, expectedResult) in testCases)
					{
						var countryEffectiveDate = countrySettings != SettingState.Overdue ? today : yesterday;
						var countryIsEnabled = countrySettings != SettingState.Disabled;

						var eunEffectiveDate = eunSettings != SettingState.Overdue ? today : yesterday;
						var eunIsEnabled = eunSettings != SettingState.Disabled;

						using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(nctstpCode, testCountry, countryEffectiveDate, countryIsEnabled))
						using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(nctstpCode, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, eunEffectiveDate, eunIsEnabled))
						{
							AssertEquals(description, expectedResult, FuncsHelper.IsFunctionalityValid(nctstpCode, today, options: ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN));
						}
					}
				});
			}
		}
	}
}
