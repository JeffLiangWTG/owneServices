using System.Drawing;

namespace Enterprise.Registry.Business
{
	public class CustomColorTheme : ColorTheme
	{
		public CustomColorTheme(int customNumber)
			: base(ResString.GetMultilingualString("8f082dc7-5c4b-4e2d-bbfd-abcebbb1c650", "Custom {0}", customNumber), true)
		{
		}

		public void UpdateColor(string colorPropertyName, Color value)
		{
			foreach (DescribedColor colorUsage in ChosenColors)
			{
				if (colorUsage.ColorPropertyName == colorPropertyName)
				{
					colorUsage.Color = value;
					break;
				}
			}
		}
	}
}
