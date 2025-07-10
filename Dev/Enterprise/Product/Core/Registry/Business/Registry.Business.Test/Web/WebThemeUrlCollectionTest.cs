using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebThemeUrlCollection))]
	sealed class WebThemeUrlCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<WebThemeUrlCollection>
	{
		public void TestFind()
		{
			var collection = new WebThemeUrlCollection();

			var themeUrl1 = collection.AddNew();
			themeUrl1.ThemeName = "Theme 1";
			themeUrl1.Url = "http://test.org/campaign/";
			themeUrl1.CompanyCode = "AAA";

			var themeUrl2 = collection.AddNew();
			themeUrl2.ThemeName = "Theme 2";
			themeUrl2.Url = "http://test.org/campaign/";
			themeUrl2.CompanyCode = "";

			var themeUrl3 = collection.AddNew();
			themeUrl3.ThemeName = "Theme 3";
			themeUrl3.Url = "http://test.org/campaign/online/";
			themeUrl3.CompanyCode = "";

			var themeUrl4 = collection.AddNew();
			themeUrl4.ThemeName = "Theme 4";
			themeUrl4.Url = "test.org/campaigns/";
			themeUrl4.CompanyCode = "AAA";

			AssertEquals(themeUrl1, collection.Find("http://test.org/campaign/login.aspx?data=123", "AAA"));
			AssertEquals(themeUrl2, collection.Find("http://test.org/campaign/login.aspx?data=123", "BBB"));
			AssertNull(collection.Find("http://test.org/default.aspx", "AAA"));
			AssertEquals(themeUrl3, collection.Find("http://test.org/campaign/online/exam.aspx", "CCC"));
			AssertEquals("http url matches theme url without protocol", themeUrl4, collection.Find("http://test.org/campaigns/exam.aspx", "AAA"));
			AssertEquals("https url matches theme url without protocol", themeUrl4, collection.Find("https://test.org/campaigns/exam.aspx", "AAA"));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override WebThemeUrlCollection GetCollectionToTest()
		{
			return new WebThemeUrlCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WebThemeUrl();
		}

		#endregion
	}
}
