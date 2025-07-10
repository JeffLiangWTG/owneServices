using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PHACEndUseCodesTest : TestCase
	{
		public void TestIsValidCategoryCode()
		{
			CombineAssertions(() =>
			{
				Assert(!PHACEndUseCodes.IsValidCategoryCode("AAAA", PHACCategories.Codes.PH01));

				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH02));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH03));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH05));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, "BBBB"));

				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH02));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH03));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH05));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, "BBBB"));

				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH02));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH03));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH05));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, "BBBB"));

				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH02));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH03));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH05));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, "BBBB"));

				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH02));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH03));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH05));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, "BBBB"));

				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH02));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH03));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH05));
				Assert(PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, "BBBB"));
			});
		}

		public void TestIsPathogenToxinLicenceMandatory()
		{
			CombineAssertions(() =>
			{
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory("AAAA", PHACCategories.Codes.PH01));

				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH02));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH03));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH05));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH01, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH01, "BBBB"));

				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH02));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH03));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH05));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH02, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH02, "BBBB"));

				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH02));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH03));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH05));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH03, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH03, "BBBB"));

				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH02));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH03));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH05));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH04, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH04, "BBBB"));

				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH02));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH03));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH05));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH05, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH05, "BBBB"));

				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH01));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH02));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH03));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH04));
				Assert(PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH05));
				Assert(!PHACEndUseCodes.IsPathogenToxinLicenceMandatory(PHACEndUseCodes.Codes.PH06, PHACCategories.Codes.PH06));
				Assert(!PHACEndUseCodes.IsValidCategoryCode(PHACEndUseCodes.Codes.PH06, "BBBB"));
			});
		}
	}
}
