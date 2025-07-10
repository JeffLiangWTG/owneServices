using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LicencesTest : TransactionedTestCase
	{
		public void TestGetAllModuleCheckpoints()
		{
			var licences = new Licences();
			var moduleCheckpoints = licences.GetAllModuleCheckpoints();

			AssertContainsExactElementsInAnyOrder("Should not have language licence child checkpoints", (checkpoint) => checkpoint.Name, Enumerable.Empty<LanguageLicenceChildCheckpoint>(), moduleCheckpoints.OfType<LanguageLicenceChildCheckpoint>());
			Assert(moduleCheckpoints.Contains(licences.LanguagePackLookup[SharedConstants.Languages.French]));
			Assert(!moduleCheckpoints.Contains(licences.LanguagePackLookup[SharedConstants.Languages.French].DocBuilderLanguageCheckpoint));
			Assert(!moduleCheckpoints.Contains(licences.LanguagePackLookup[SharedConstants.Languages.French].GUILanguageCheckpoint));
			Assert(!moduleCheckpoints.Contains(licences.LanguagePackLookup[SharedConstants.Languages.French].WebTrackerLanguageCheckpoint));
		}

		public void TestLangugeLicenceBeingUsedForNonSystemLanguage()
		{
			var licences = new Licences();
			var factory = new BusinessObjectFactory();
			var systemChineseLanguage = factory.LoadTop1<IRefLocalLanguage>(new ZQuery(RefLocalLanguageSchema.RA_Code, "ZH").AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, "CN"));
			var testLanguage1 = factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "CC";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			testLanguage1.RA_RA_ParentLanguage = systemChineseLanguage.PK;
			factory.Save();

			using (licences.UseLanguageLicense(testLanguage1.FullLanguageCode, LanguageUsageType.GUI))
			{
				Assert("Should use the base system language", licences.LanguagePackLookup[systemChineseLanguage.FullLanguageCode].GUILanguageCheckpoint.IsLoggedIn);
			}
		}

		public void TestLicencesConstructorShouldNotNeedToCreateABusinessObjectFactory()
		{
			var nextInstance = BusinessObjectFactory._NextInstance;
			_ = new Licences();
			AssertEquals(
				"No new BusinessObjectFactory should be created. i.e. Languages for LanguagePacks should not be loaded from database. Why? Because zrs files will only exist for 'known' languages in LanguageHelper.GetDefaultLanguageForOLookUpEditType.",
				0, BusinessObjectFactory._NextInstance - nextInstance);
		}
	}
}
