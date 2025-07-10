using System;
using System.Drawing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ColorThemeSelector))]
	sealed class ColorThemeSelectorTest : RegistryBusinessObjectTemplateTestCase<ColorThemeSelector>
	{
		#region Implementation

		protected override ColorThemeSelector GetBusinessObjectToClone()
		{
			ColorThemeSelector result = new ColorThemeSelector();
			result.ChosenThemeName = "Classic";

			return result;
		}

		protected override ColorThemeSelector GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public void TestCloneColorThemeSelector()
		{
			var selector = new ColorThemeSelector();
			selector.ChosenThemeName = "Custom 1";
			selector.ColorThemeList.GetThemeFromName("Custom 1").ChosenColors[0].Color = Color.Red;
			selector.ColorThemeList.GetThemeFromName("Custom 2").ChosenColors[0].Color = Color.Green;

			var clone = (ColorThemeSelector)selector.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

			AssertEquals(Color.FromArgb(255, Color.Red), clone.ColorThemeList.GetThemeFromName("Custom 1").ChosenColors[0].Color);
			AssertEquals(Color.FromArgb(255, Color.Green), clone.ColorThemeList.GetThemeFromName("Custom 2").ChosenColors[0].Color);
		}

		[ExpectNoExceptions]
		public void TestMultiLingualColorThemeSelector()
		{
			SetColorThemeSelectorForSpecificLanguage(SharedConstants.Languages.Spanish);
			SetColorThemeSelectorForSpecificLanguage(SharedConstants.Languages.English);
			SetColorThemeSelectorForSpecificLanguage(SharedConstants.Languages.French);
			SetColorThemeSelectorForSpecificLanguage(SharedConstants.Languages.ChineseTraditional);
		}

		void SetColorThemeSelectorForSpecificLanguage(string languageCode)
		{
			using (Res.TemporarilySwitchLanguage(languageCode))
			{
				var registryItem = new ColorThemeRegistryItem("", null, null, null, new ColorThemeSelector());
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ColorThemeSelector());
			}
		}

		#endregion
	}
}
