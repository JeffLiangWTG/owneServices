using System.Drawing;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class ColorFromNameConverter
	{
		public static Color ColorFromName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return Color.Empty;
			}

			return Enterprise.ZArchitecture.Core.ColorList.ColorFromName(name);
		}
	}
}
