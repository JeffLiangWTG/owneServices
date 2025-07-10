using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ColorThemeRegistryItem))]
	sealed class ColorThemeRegistryItemTest : StronglyTypedRegistryItemTestCase<ColorThemeSelector>
	{
		protected override StronglyTypedRegistryItem<ColorThemeSelector, ColorThemeSelector> GetNewRegistryItem()
		{
			ColorThemeSelector @default = new ColorThemeSelector();
			@default.ChosenThemeName = "Classic";
			return new ColorThemeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", @default);
		}
	}
}
