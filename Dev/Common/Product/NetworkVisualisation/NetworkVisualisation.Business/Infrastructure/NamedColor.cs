using System.Drawing;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NamedColor
	{
		public NamedColor(string name, Color color)
		{
			Name = name;
			Color = color;
		}

		public string Name { get; }
		public Color Color { get; }

		public ResourceString TranslatedName => (ResourceString)GetResourceStringCaption();

		public const ushort NamedColorAssemblyId = 10;

		public string GetResourceStringKey()
		{
			return CustomizableDataResourceStrings.GetCustomizableDataKey(nameof(NamedColor), Name.TrimEnd());
		}

		public IResString GetResourceStringCaption()
		{
			return ResString._GetMultilingualString(NamedColorAssemblyId, GetResourceStringKey(), Name.TrimEnd());
		}
	}
}
