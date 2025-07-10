using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public sealed class ColorThemeList : CodeDescriptionPairList
	{
		public ColorThemeList()
		{
			var properties = typeof(DefinedColorThemes).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			foreach (var property in properties)
			{
				AddTheme((ColorTheme)property.GetValue(null));
			}

			AddTheme(new CustomColorTheme(1));
			AddTheme(new CustomColorTheme(2));
			AddTheme(new CustomColorTheme(3));
			AddTheme(new CustomColorTheme(4));
		}

		void AddTheme(ColorTheme theme)
		{
			Add(new ColorThemeItem(theme));
		}

		public ColorTheme GetThemeFromName(string name)
		{
			ColorThemeItem item = (ColorThemeItem)this[name];
			return item != null ? item.Theme : null;
		}

		public class ColorThemeItem : CodeDescriptionPair
		{
			public ColorThemeItem(ColorTheme theme)
				: base(theme.Name, theme.Name)
			{
				this.Theme = theme;
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Exceptional case")]
			public readonly ColorTheme Theme;
		}
	}
}
