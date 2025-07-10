using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Registry.Business.Testing
{
	sealed class WebThemeUrlValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationThemeName()
		{
			var collection = new WebThemeUrlCollection();
			var rule1 = collection.AddNew();

			rule1.ValidateThemeName();
			AssertMandatoryValidationError(rule1.ThemeNameInfo, true);

			WebCustomThemeImageBusinessObjectCollection imageCollection = new WebCustomThemeImageBusinessObjectCollection();
			imageCollection.Add(new WebCustomThemeImageBusinessObject()
			{
				Data = new byte[] { 4, 4, 3, 5 },
				ImageName = "logo.png"
			});
			var themeCollection = new WebThemeCustomObjectCollection();
			WebThemeCustomObject themeObject = new WebThemeCustomObject();
			themeObject.CSS = "";
			themeObject.ThemeName = "Test Theme";
			themeObject.ImageCollection.Add(imageCollection.First<WebCustomThemeImageBusinessObject>());
			themeCollection.Add(themeObject);

			WebDataRegistry.Instance.WebCampaignCustomTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, themeCollection);

			rule1.ThemeName = "Wrong Theme";
			rule1.ValidateThemeName();
			AssertHasError(rule1.ThemeNameInfo, "Enter a valid selection.");

			rule1.ThemeName = "Test Theme";
			AssertNoErrors(rule1.ThemeNameInfo);
		}

		public void TestValidateCompanyPk()
		{
			var rule = new WebThemeUrl();
			rule.ValidateCompanyCode();
			AssertNoErrors(rule.CompanyCodeInfo);

			rule.CompanyCode = "AAA";
			rule.ValidateCompanyCode();
			AssertHasErrors(rule.CompanyCodeInfo);

			rule.CompanyCode = Env.CurrentCompany.Code;
			rule.ValidateCompanyCode();
			AssertNoErrors(rule.CompanyCodeInfo);
		}
	}
}
