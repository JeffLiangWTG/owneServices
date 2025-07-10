using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ColorTheme))]
	sealed class ColorThemeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCanBeModified()
		{
			ColorTheme theme1 = new ColorTheme((NoResString)"Hello", false);
			AssertEquals(true, theme1.NameInfo.ReadOnly);

			ColorTheme theme2 = new ColorTheme((NoResString)"Hello", true);
			AssertEquals(false, theme2.NameInfo.ReadOnly);
		}

		public void TestColors()
		{
			ColorTheme theme = new ColorTheme((NoResString)"Hello", false);
			foreach (DescribedColor item in theme.ChosenColors)
			{
				AssertEquals(Color.Empty, item.Color);
				item.Color = Color.Red;
				AssertEquals(Color.Red.ToArgb(), item.Color.ToArgb());
			}
		}

		public void TestTitleBarBackgroundColour_IfNotSet_UsesNavBarButtonColour()
		{
			var theme = new ColorTheme((NoResString)"NewTheme", false);
			AssertEquals("PRE: New colour theme has Title Bar Background unset by default", Color.Empty, theme.TitleBarBackground);

			theme.NavBarButtonColor1 = Color.Red;
			AssertEquals("Title bar background uses NavBarButtonColor1 (aka Navigation Bar Tile Background) if unset", Color.Red, theme.TitleBarBackground);

			theme.TitleBarBackground = SystemColors.Control;
			theme.NavBarButtonColor1 = Color.Green;
			AssertEquals("Title bar background uses NavBarButtonColor1 (aka Navigation Bar Tile Background) if Set to SystemColors.Control", Color.Green, theme.TitleBarBackground);

			theme.TitleBarBackground = Color.Empty;
			theme.NavBarButtonColor1 = Color.Green;
			AssertEquals("Title bar background uses NavBarButtonColor1 (aka Navigation Bar Tile Background) if Set to Color.Empty", Color.Green, theme.TitleBarBackground);
		}

		public void TestTitleBarTextColour_IfNotSet_UsesNavBarTextColour()
		{
			var theme = new ColorTheme((NoResString)"NewTheme", false);
			AssertEquals("PRE: New colour theme has Title Bar Text unset by default", Color.Empty, theme.TitleBarText);

			theme.NavBarTextColor = Color.Red;
			AssertEquals("Title bar text uses NavBarTextColor (aka Navigation Bar Tile Text) if unset", Color.Red, theme.TitleBarText);

			theme.TitleBarText = SystemColors.Control;
			theme.NavBarTextColor = Color.Green;
			AssertEquals("Title bar text uses NavBarTextColor (aka Navigation Bar Tile Text) if Set to SystemColors.Control", Color.Green, theme.TitleBarText);

			theme.TitleBarText = Color.Empty;
			theme.NavBarTextColor = Color.Green;
			AssertEquals("Title bar text uses NavBarTextColor (aka Navigation Bar Tile Text) if Set to Color.Empty", Color.Green, theme.TitleBarText);
		}

		public void TestOpacity()
		{
			ColorTheme theme = new ColorTheme((NoResString)"Hello", false);
			foreach (DescribedColor item in theme.ChosenColors)
			{
				int i = 0;
				AssertEquals(Color.Empty, item.Color);
				item.Color = Color.FromArgb(i, 255, 0, 0);
				AssertEquals(Color.Red.ToArgb(), item.Color.ToArgb());
				++i;
			}
		}

		public void TestDescriptionContainsAllTheColors()
		{
			var themeColors = new ColorTheme()
				.ChosenColors
				.Select(c => c.ColorPropertyName)
				.ToList();

			var colors = typeof(IColorTheme)
				.GetProperties()
				.Where(p => p.PropertyType == typeof(Color))
				.Select(p => p.Name)
				.ToList();

			AssertNotEquals("PRE", 0, colors.Count);
			AssertEquals(colors.Count, themeColors.Count);

			var distinctColors = colors.Except(themeColors);

			if (distinctColors.Any())
			{
				Fail("Unmapped colors " + String.Join(", ", distinctColors));
			}
		}

		public void TestColorThemeOfAnotherColorScheme()
		{
			var originalTheme = DefinedColorThemes.AmethystColorTheme;
			var newTheme = new ColorTheme(originalTheme);

			foreach (var property in typeof(IColorTheme).GetProperties())
			{
				AssertEquals(property.Name, property.GetValue(originalTheme), property.GetValue(newTheme));
			}
		}

		public void TestModifyingTheNewColorThemeDoesNotEffectTheOldOne()
		{
			var originalTheme = DefinedColorThemes.AmethystColorTheme;
			var newTheme = new ColorTheme(originalTheme);

			AssertEquals("PRE", originalTheme.GridReadOnlyColor, newTheme.GridReadOnlyColor);

			newTheme.GridReadOnlyColor = Color.Green;

			AssertEquals(Color.Green, newTheme.GridReadOnlyColor);
			AssertNotEquals(Color.Green, originalTheme.GridReadOnlyColor);
		}
	}
}
