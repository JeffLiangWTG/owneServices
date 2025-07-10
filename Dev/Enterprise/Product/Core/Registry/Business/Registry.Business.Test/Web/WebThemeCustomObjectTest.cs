using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebThemeCustomObject))]
	sealed class WebThemeCustomObjectTest : RegistryBusinessObjectTemplateTestCase<WebThemeCustomObject>
	{
		public void TestFindImage()
		{
			var theme = new WebThemeCustomObject();

			var image1 = theme.ImageCollection.AddNew();
			image1.ImageName = "logo.png";

			var image2 = theme.ImageCollection.AddNew();
			image2.ImageName = "BACKGROUND.gif";

			AssertEquals(image1, theme.FindImage("LOGO.png"));
			AssertEquals(image2, theme.FindImage("background.GIF"));
			AssertNull(theme.FindImage("background"));
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

		protected override WebThemeCustomObject GetBusinessObjectToClone()
		{
			return new WebThemeCustomObject();
		}

		protected override WebThemeCustomObject GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
