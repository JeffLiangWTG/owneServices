using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebThemeCustomObjectCollection))]
	sealed class WebThemeCustomObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WebThemeCustomObjectCollection>
	{
		public void TestFind()
		{
			var collection = new WebThemeCustomObjectCollection();

			var theme1 = collection.AddNew();
			theme1.ThemeName = "Theme 1";

			var theme2 = collection.AddNew();
			theme2.ThemeName = "Theme 2";

			AssertEquals(theme1, collection.Find("theme 1"));
			AssertEquals(theme2, collection.Find("THEME 2"));
			AssertNull(collection.Find("Theme 3"));

			var defaultTheme = collection.AddNew();
			defaultTheme.ThemeName = WebThemeCustomObject.Schema.DefaultThemeName;

			AssertEquals(defaultTheme, collection.Find("Theme 3"));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override WebThemeCustomObjectCollection GetCollectionToTest()
		{
			return new WebThemeCustomObjectCollection(NewFallbackLevel(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WebThemeCustomObject(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
