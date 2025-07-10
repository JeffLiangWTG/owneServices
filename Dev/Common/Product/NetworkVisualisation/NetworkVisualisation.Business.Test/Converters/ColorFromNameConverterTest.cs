using System.Drawing;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test.Converters
{
	public class ColorFromNameConverterTest : TestCase
	{
		public void TestColorFromName()
		{
			AssertEquals(Color.Black, ColorFromNameConverter.ColorFromName("Black"));
			AssertEquals(Color.AliceBlue, ColorFromNameConverter.ColorFromName("AliceBlue"));
			AssertEquals(Color.AliceBlue, ColorFromNameConverter.ColorFromName("Alice Blue"));
			AssertEquals(Color.AntiqueWhite, ColorFromNameConverter.ColorFromName("AntiqueWhite"));
			AssertEquals(Color.AntiqueWhite, ColorFromNameConverter.ColorFromName("Antique White"));
			AssertEquals(Color.Empty, ColorFromNameConverter.ColorFromName(string.Empty));
		}
	}
}
