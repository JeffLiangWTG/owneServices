using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class WebThemeCustomObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateThemeName()
		{
			var collection = new WebThemeCustomObjectCollection();
			var themeObject = collection.AddNew();

			themeObject.ValidateThemeName();
			AssertMandatoryValidationError(themeObject.ThemeNameInfo, true);

			themeObject.ThemeName = "Theme One";
			themeObject.ValidateThemeName();
			AssertNoErrors(themeObject.ThemeNameInfo);

			var themeObject2 = collection.AddNew();
			themeObject2.ThemeName = "Theme One";
			themeObject2.ValidateThemeName();
			AssertHasError(themeObject2.ThemeNameInfo, "The Theme Name has been duplicated and must be unique.");

			themeObject2.ThemeName = "Theme Two";
			themeObject2.ValidateThemeName();
			AssertNoErrors(themeObject2.ThemeNameInfo);
		}
	}
}
