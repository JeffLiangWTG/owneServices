using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LanguageLicenceCheckpointTest : TransactionedTestCase
	{
		public void TestLicenceConsumption()
		{
			var licences = new Licences();
			var languageLicence = new LanguageLicenceCheckpoint("XX1", "DocBuilder Only Langauge", licences, null);

			try
			{
				AssertEquals(LicenceLoginResponse.Granted, languageLicence.DocBuilderLanguageCheckpoint.Login(LanguageLicencedComponent.Instance));
				AssertEquals("IsLoggedIn", true, languageLicence.DocBuilderLanguageCheckpoint.IsLoggedIn);
				AssertEquals(true, languageLicence.DocBuilderLanguageCheckpoint.ConsumptionLogCreated);

				AssertEquals(LicenceLoginResponse.Granted, languageLicence.GUILanguageCheckpoint.Login(LanguageLicencedComponent.Instance));
				AssertEquals("IsLoggedIn", true, languageLicence.GUILanguageCheckpoint.IsLoggedIn);
				AssertEquals(true, languageLicence.GUILanguageCheckpoint.ConsumptionLogCreated);

				AssertEquals(LicenceLoginResponse.Granted, languageLicence.WebTrackerLanguageCheckpoint.Login(LanguageLicencedComponent.Instance));
				AssertEquals("IsLoggedIn", true, languageLicence.WebTrackerLanguageCheckpoint.IsLoggedIn);
				AssertEquals(true, languageLicence.WebTrackerLanguageCheckpoint.ConsumptionLogCreated);
			}
			finally
			{
				languageLicence.Logout(LanguageLicencedComponent.Instance);
			}
		}
	}
}
