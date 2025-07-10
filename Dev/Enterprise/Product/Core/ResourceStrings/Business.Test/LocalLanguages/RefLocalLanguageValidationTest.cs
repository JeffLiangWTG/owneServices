using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class RefLocalLanguageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRACode()
		{
			LocalLanguage.RA_Code = "";
			Assert("Expecting RA_Code to be empty and have errors.", LocalLanguage.RA_CodeInfo.HasErrors());
		}

		public void TestCheckRACountryCode()
		{
			LocalLanguage.RA_RN_NKCountryCode = "FD";
			AssertHasError("Should have error when invalid country code is entered", LocalLanguage.RA_RN_NKCountryCodeInfo, "Enter a valid Country/Region.");
		}

		public void TestCheckFullLanguageCode()
		{
			var testLanguage1 = Factory.New<RefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			Factory.Save();

			var testLanguage2 = Factory.New<RefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_RA_ParentLanguage = testLanguage1.PK;
			Factory.Save();

			testLanguage1.RA_Code = "BB";
			AssertHasError("Full language code should have error when duplicated", testLanguage1.FullLanguageCodeInfo, "Full language code must be unique, please change the ISO language code or change the country/region.");
		}

		public void TestCheckRADesc()
		{
			LocalLanguage.RA_Description = "";
			Assert("Expecting RA_Description to be empty and have errors.", LocalLanguage.RA_DescriptionInfo.HasErrors());
		}

		public void TestCheckParentLanguageCode()
		{
			var testLanguage1 = Factory.New<RefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			Factory.Save();

			var testLanguage2 = Factory.New<RefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_RA_ParentLanguage = testLanguage1.PK;
			Factory.Save();

			var testLanguage3 = Factory.New<RefLocalLanguage>();
			testLanguage3.RA_Code = "CC";
			testLanguage3.RA_RN_NKCountryCode = "CN";
			testLanguage3.RA_Description = "Test Language3";
			testLanguage3.RA_RA_ParentLanguage = testLanguage2.PK;
			Factory.Save();

			testLanguage1.ParentLanguageCode = string.Empty;
			AssertHasWarning("A notification will tell the user default parent language is English", testLanguage1.ParentLanguageCodeInfo, "Parent language will be defaulted to English.");
			testLanguage1.ParentLanguageCode = testLanguage1.FullLanguageCode;
			AssertHasError("ParentLanguageCode should have error if set its value to current record", testLanguage1.ParentLanguageCodeInfo, "Parent language cannot be the language itself.");
			testLanguage1.Factory.ClearCachedValue<CodeDescriptionPairList>("RefLocalLanguageLookups.LocalLanguagesList");
			testLanguage1.ParentLanguageCode = testLanguage3.FullLanguageCode;
			AssertHasError("ParentLanguageCode should have error if set its value to a language whose parent is current record", testLanguage1.ParentLanguageCodeInfo, "Current language is the parent of this language. Please select another language.");
		}

		#region Implementation

		RefLocalLanguage LocalLanguage;

		protected override void SetUp()
		{
			base.SetUp();
			LocalLanguage = Factory.New<RefLocalLanguage>();
			LocalLanguage.RA_Code = "CN";
		}

		#endregion
	}
}
