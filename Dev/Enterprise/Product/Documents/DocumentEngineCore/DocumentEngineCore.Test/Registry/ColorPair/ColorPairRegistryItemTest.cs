using System.Drawing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(ColorPairRegistryItem))]
	sealed class ColorPairRegistryItemTest : StronglyTypedRegistryItemTestCase<ColorPairSelector>
	{
		protected override StronglyTypedRegistryItem<ColorPairSelector, ColorPairSelector> GetNewRegistryItem()
		{
			var pairSelector = new ColorPairSelector {
					PrimaryColor = Color.CornflowerBlue,
					SecondaryColor = Color.Firebrick };
			return new ColorPairRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, pairSelector);
		}
	}
}
